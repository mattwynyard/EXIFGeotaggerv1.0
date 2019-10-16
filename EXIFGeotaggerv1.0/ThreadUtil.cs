using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using GMap.NET;

using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace EXIFGeotagger
{
    class ThreadUtil
    {
        //delegates
        public event SetMinMaxDelegate setMinMax;
        public delegate void SetMinMaxDelegate(double lat, double lng);
        public event AddRecordDelegate addRecord;
        public delegate void AddRecordDelegate(string photo, Record record);
        public event GeoTagCompleteDelegate geoTagComplete;
        public delegate void GeoTagCompleteDelegate(GeotagReport report);
        //properties
        private OleDbConnection connection;
        private static readonly Object obj = new Object();
        private ProgressForm progressForm;
        private int geoTagCount;
        private int errorCount;
        private int stationaryCount;
        private int id;
        private int startCount;
        private int sizeRecord;
        private int sizeBitmap;
        private CancellationTokenSource cts;
        private ConcurrentDictionary<string, string> photoDict;
        private ConcurrentDictionary<string, object[]> photoZipDict;
        private BlockingCollection<object[]> bitmapQueue;
        private BlockingCollection<Record> queue;
        private ConcurrentDictionary<string, Exception> errorDict;
        public ConcurrentDictionary<string, Record> noPhotoDict;
        public ConcurrentDictionary<string, Record> dict;
        private BlockingCollection<ThreadInfo> geoTagQueue;
        private Boolean geotagging = false;
        private Boolean mZip;
        private int tagRate = -1;

        private static ManualResetEvent mre = new ManualResetEvent(false);
        private static ManualResetEvent producerMRE = new ManualResetEvent(false);

        private Stopwatch stopwatch;
        public TimeSpan ts;
        public GeotagReport report;
       


        /// <summary>
        /// Default constructor
        /// </summary>
        public ThreadUtil()
        {
            intialise();
        }

        private void intialise(int sizeRecord = 50000, int sizeBitmap = 100)
        {
            startCount = 0;
            geoTagCount = 0;
            this.sizeRecord = sizeRecord;
            this.sizeBitmap = sizeBitmap;
            dict = new ConcurrentDictionary<string, Record>();
            queue = new BlockingCollection<Record>(sizeRecord); //100,000 upper bound
            geoTagQueue = new BlockingCollection<ThreadInfo>();
            noPhotoDict = new ConcurrentDictionary<string, Record>();
            bitmapQueue = new BlockingCollection<object[]>(sizeBitmap);
            errorDict = new ConcurrentDictionary<string, Exception>();
            stopwatch = Stopwatch.StartNew();
        }

        public BlockingCollection<string> buildQueue(string path)
        {
            BlockingCollection<string> fileQueue = new BlockingCollection<string>();
            string[] files = Directory.GetFiles(path);
            Task producer = Task.Factory.StartNew(() =>
            {
                foreach (string file in files)
                {
                    fileQueue.Add(file);
                }
                fileQueue.CompleteAdding();
            });
            Task.WaitAll(producer);
            return fileQueue;
        }

        public void photoReader(string path, Boolean zip)
        {
            //String files = 
            //photoDict = new ConcurrentDictionary<string, string>();
            mZip = zip;
            Task build = Task.Factory.StartNew(() =>
            {
                if (zip)
                {
                    photoDict = new ConcurrentDictionary<string, string>();
                    string[] files = Directory.GetFiles(path, "*.zip", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        string f = file;
                        using (FileStream zipToOpen = new FileStream(file, FileMode.Open))
                        {
                            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                            {
                                foreach (ZipArchiveEntry entry in archive.Entries)
                                {
                                    string s = entry.FullName;
                                    string[] tokens = s.Split('/');
                                    s = tokens[tokens.Length - 1];
                                    if (s.Substring(s.Length - 3) == ("jpg"))
                                        
                                    {
                                        string key = s.Substring(0, s.Length - 4);
                                        photoDict.TryAdd(key, file);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    photoDict = new ConcurrentDictionary<string, string>();
                    string[] files = Directory.GetFiles(path, "*.jpg", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        string key = Path.GetFileNameWithoutExtension(file);
                        
                        photoDict.TryAdd(key, file);
                    }
                }
            });
            Task.WaitAll(build);
        }

        public async Task<ConcurrentDictionary<string, Record>> buildDictionary(string photoPath, string dbPath, string outPath, Boolean allRecords)
        {
            progressForm = new ProgressForm("Processing...");
            progressForm.Show();
            progressForm.BringToFront();
            progressForm.cancel += cancelImport;
            progressForm.Finish += Finish;
            cts = new CancellationTokenSource();
            var token = cts.Token;
            var progressHandler1 = new Progress<object>(a =>
            {
                ts = stopwatch.Elapsed;
                double rate = 0;
                var elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
                int[] values = (int[])a;
                if (ts.TotalSeconds > 0)
                {
                    rate = values[1] / ts.TotalSeconds;
                    tagRate = (int)Math.Round(rate);
                }

                progressForm.ProgressValue = values[0];
                progressForm.Message = "Database read, please wait... " + values[0].ToString() + "% completed\n" +
                values[1] + " of " + values[2] + " records processed\n" +
                "Queue size: " + values[3] + "\n" +
                "Record Dictionary size: " + values[4] + "\n" +
                "Photo Dictionary size: " + values[5] + "\n" +
                "Bitmap Queue size: " + values[6] + "\n" +
                "Geotag count: " + values[7] + "......Processing " + tagRate.ToString() + " items/sec" + "\n" +
                "No photo: " + values[9] + "\n" +
                "Elapsed Time: " + elapsedTime;

            });
            var progressValue = progressHandler1 as IProgress<object>;
            int length = 0;
            int count = 0;
            int noGeomark = 0;
           
            int dictCount = 0;
            int queueCount = 0;
            int noRecord = 0;

            string connectionString = string.Format("Provider={0}; Data Source={1}; Jet OLEDB:Engine Type={2}",
                "Microsoft.Jet.OLEDB.4.0", dbPath, 5);
            connection = new OleDbConnection(connectionString);
            string strSQL;
            string strRecord = null;
            string lengthSQL; //sql count string

            if (allRecords)
            {
                strSQL = "SELECT * FROM PhotoList";
                lengthSQL = "SELECT Count(PhotoID) FROM PhotoList;";
            }
            else
            {
                strSQL = "SELECT * FROM PhotoList WHERE PhotoList.GeoMark = true AND PhotoList.Inspector = 'IN';";
                lengthSQL = "SELECT Count(PhotoID) FROM PhotoList WHERE PhotoList.GeoMark = true  AND PhotoList.Inspector = 'IN';";
                strRecord = "SELECT * FROM PhotoList WHERE PhotoID = @photo AND PhotoList.GeoMark = true;";
            }
            OleDbCommand commandLength = new OleDbCommand(lengthSQL, connection);
            //OleDbCommand command = new OleDbCommand(strSQL, connection);
            connection.Open();
            length = Convert.ToInt32(commandLength.ExecuteScalar());
            commandLength.Dispose();
            

            string[] files = Directory.GetFiles(photoPath);
 
            Task producer = Task.Factory.StartNew(async () =>
            {
                OleDbCommand command = new OleDbCommand(strSQL, connection);
                using (OleDbDataReader reader = command.ExecuteReader())
                {
                    Object[] row;
                    while (reader.Read())
                    {
                        row = new Object[reader.FieldCount];
                        reader.GetValues(row);
                        Record r = await buildRecord(row);
                        queue.Add(r);                       
                    }
                    reader.Close();
                    queue.CompleteAdding();
                    command.Dispose();
                    connection.Close();
                }            
            });

            Task consumer = Task.Factory.StartNew(() =>
            {
                foreach (var item in queue.GetConsumingEnumerable())
                {
                    
                    if (token.IsCancellationRequested)
                   {
                       token.ThrowIfCancellationRequested();
                   }
                   try
                   {
                       ThreadInfo threadInfo = new ThreadInfo();
                       Boolean found;
                       string folder = null;
                       found = photoDict.TryRemove(item.PhotoName, out folder);
                       //dictCount = photoDict.Count;
                       if (found)
                       {
                           threadInfo.Folder = folder;
                           threadInfo.Zip = mZip;
                           threadInfo.OutPath = outPath;
                           threadInfo.Record = item;
                           threadInfo.Photo = item.PhotoName;
                           if (threadInfo.Record.GeoMark)
                           {
                               if (!geotagging)
                               {
                                    lock (obj)
                                    {
                                        geotagging = true;
                                    }
                               }                               
                                Record newRecord = null;
                                newRecord = ProcessFile(threadInfo);
                                dict.TryAdd(item.PhotoName, item);
                           }
                           else
                           {
                               lock (obj)
                               {
                                   noGeomark++;
                               }
                           }
                       }
                       else
                       {
                           lock (obj)
                           {
                               geotagging = false;                             
                               noPhotoDict.TryAdd(item.PhotoName, item);                            
                            }
                            Task progress = Task.Factory.StartNew(() => updateUI(count, length, progressValue));
                            Thread.Sleep(1);
                       }
                   }
                   catch (Exception ex)
                   {
                       String s = ex.Message;
                   }
                   lock (obj)
                   {
                       count++;
                   }
                }
                bitmapQueue.CompleteAdding();
                mre.Set();
                Task progress2 = Task.Factory.StartNew(() => updateUI(count, length, progressValue));

            }, cts.Token);

            Task consumeBitmaps = Task.Factory.StartNew(() =>
            {               
                foreach (var item in bitmapQueue.GetConsumingEnumerable())
                {
                    if (!bitmapQueue.IsCompleted)
                    {
                        mre.WaitOne();
                        if(bitmapQueue.IsAddingCompleted)
                        {
                            if (item != null)
                            {
                                processImage(item);
                            }
                            if (bitmapQueue.Count == 0) 
                            {
                                Task progressEnd = Task.Factory.StartNew(() => updateUI(count, length, progressValue));
                                break;
                            }
                            
                        } else
                        {
                            processImage(item);
                            if (bitmapQueue.Count == 0)
                            {
                                mre.Reset();
                            }
                        }                     
                    }
                    Task progress = Task.Factory.StartNew(() => updateUI(count, length, progressValue));
                }
            });

            await Task.WhenAll(producer, consumer);
            //Task.WhenAll(progress);
            await Task.WhenAll(consumeBitmaps);
            //Thread.Sleep(500);
            stopwatch.Stop();
            progressForm.enableOK();
            progressForm.disableCancel();
            report = new GeotagReport();
            report.RecordDictionary = dict;
            report.ErrorDictionary = errorDict;
            report.NoPhotoDictionary = noPhotoDict;
            report.NoRecordDictionary = photoDict;
            report.GeotagCount = geoTagCount;
            report.ProcessedRecords = count;
            report.TotalRecords = length;
            report.Path = Path.GetDirectoryName(dbPath);
            report.Time = ts;
            progressForm.setReport(report);
            return dict;
        }

        private void updateUI(int count, int length, IProgress<object> progressValue)
        {
                TimeSpan ts = stopwatch.Elapsed;
                double percent = ((double)count / length) * 100;
                int percentInt = (int)percent;
                int[] values = { percentInt, count, length, queue.Count, dict.Count, photoDict.Count, bitmapQueue.Count, geoTagCount, tagRate, noPhotoDict.Count };
                object a = (object)values;
            try
            {
                progressForm.Invoke(new MethodInvoker(() =>
                {
                    if (progressValue != null)
                    {
                        progressValue.Report(a);

                    }
                }));
            } catch (Exception ex)
            {
                String err = ex.StackTrace;
            }
        }

        public void Finish()
        {
            geoTagComplete(report);
        }

            /// <summary>
            /// Intialises a new Record and adds data extracted from access to each relevant field.
            /// The record is then added to the Record Dictionary.
            /// </summary>
            /// <param name="i: the number of records read"></param>
            /// <param name="row: the access record"></param>
            private async Task<Record> buildRecord(Object[] row)
        {
            Record r = new Record((string)row[1]);
            await Task.Run(() =>
            {              
                try
                {                  
                    int id = (int)row[0];
                    r.Id = id.ToString();
                    r.PhotoRename = Convert.ToString(row[2]);
                    r.Latitude = (double)row[3];
                    r.Longitude = (double)row[4];
                    r.Altitude = (double)row[5];
                    r.Bearing = Convert.ToDouble(row[6]);
                    r.Velocity = Convert.ToDouble(row[7]);
                    r.Satellites = Convert.ToInt32(row[8]);
                    r.PDop = Convert.ToDouble(row[9]);
                    r.Inspector = Convert.ToString(row[10]);
                    r.TimeStamp = Convert.ToDateTime(row[12]);
                    r.GeoMark = Convert.ToBoolean(row[13]);
                    r.Side = Convert.ToString(row[19]);
                    r.Road = Convert.ToInt32(row[20]);
                    r.Carriageway = Convert.ToInt32(row[21]);
                    r.ERP = Convert.ToInt32(row[22]);
                    r.FaultID = Convert.ToInt32(row[23]);
                    //r.TACode = Convert.ToInt32(row[24]);
                    //DataRow dRow = new DataRow()
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                   
                }
                
            });
            return r;
        }

        public async Task<GMapOverlay> readGeoTag(BlockingCollection<string> fileQueue, string folderPath, string layer, string color)
        {
            int mQueueSize = fileQueue.Count;
            string[] files = Directory.GetFiles(folderPath);
            GMapOverlay overlay = new GMapOverlay(layer);
            progressForm = new ProgressForm("Importing Photos...");
            progressForm.Show();
            progressForm.BringToFront();
            progressForm.cancel += cancelImport;
            cts = new CancellationTokenSource();
            var token = cts.Token;
            geoTagCount = 0;
            id = 0;
            
            int length = files.Length;
            Bitmap bitmap = ColorTable.getBitmap(color, 4);
            Dictionary<string, Record> recordDict = new Dictionary<string, Record>();
            var progressHandler1 = new Progress<int>(value =>
            {
                progressForm.ProgressValue = value;
                progressForm.Message = "Import in progress, please wait... " + value.ToString() + "% completed\n" +
                geoTagCount + " of " + mQueueSize + " photos geotagged";

            });
            var progressValue = progressHandler1 as IProgress<int>;
            await Task.Factory.StartNew(() =>
            {            
                try
                {                 
                    while (fileQueue.Count != 0)
                    {
                        if (token.IsCancellationRequested)
                        {
                            token.ThrowIfCancellationRequested();
                        }
                        Parallel.Invoke(
                            () =>
                            {
                                ThreadInfo threadInfo = new ThreadInfo();
                                threadInfo.Length = mQueueSize;
                                threadInfo.ProgressHandler = progressHandler1;
                                threadInfo.File = fileQueue.Take();
                                Record r = null;
                                r = readData(threadInfo);                              
                                MarkerTag tag = new MarkerTag(color, id);
                                GMapMarker marker = new GMarkerGoogle(new PointLatLng(r.Latitude, r.Longitude), bitmap);
                                marker.Tag = tag;
                                tag.Size = 4;
                                tag.PhotoName = r.PhotoName;
                                tag.Record = r;
                                tag.Path = Path.GetFullPath(r.Path);
                                overlay.Markers.Add(marker);
                            });
                    }
                }
                catch (OperationCanceledException)
                {

                    cts.Dispose();
                }
            }, cts.Token);
            progressForm.Close();
            return overlay;
        }

        private Record readData(ThreadInfo threadInfo)
        {
            //string path = threadInfo.OutPath + "\\" + item.PhotoName + ".jpg";
            string outPath = threadInfo.OutPath;
            int length = threadInfo.Length;
            string file = Path.GetFullPath(threadInfo.File);
            string photo = Path.GetFileNameWithoutExtension(file);
            Image image = new Bitmap(file);
            Record r = new Record(photo);
                var progressValue = threadInfo.ProgressHandler as IProgress<int>;

                lock (obj)
                {
                    id++;
                }
                PropertyItem[] propItems = image.PropertyItems;
                PropertyItem propItemLatRef = image.GetPropertyItem(0x0001);
                PropertyItem propItemLat = image.GetPropertyItem(0x0002);
                PropertyItem propItemLonRef = image.GetPropertyItem(0x0003);
                PropertyItem propItemLon = image.GetPropertyItem(0x0004);
                PropertyItem propItemAltRef = image.GetPropertyItem(0x0005);
                PropertyItem propItemAlt = image.GetPropertyItem(0x0006);
                PropertyItem propItemDateTime = image.GetPropertyItem(0x0132);

                image.Dispose();
                byte[] latBytes = propItemLat.Value;
                byte[] latRefBytes = propItemLatRef.Value;
                byte[] lonBytes = propItemLon.Value;
                byte[] lonRefBytes = propItemLonRef.Value;
                byte[] altRefBytes = propItemAltRef.Value;
                byte[] altBytes = propItemAlt.Value;
                byte[] dateTimeBytes = propItemDateTime.Value;
                string latitudeRef = ASCIIEncoding.UTF8.GetString(latRefBytes);
                string longitudeRef = ASCIIEncoding.UTF8.GetString(lonRefBytes);
                string altRef = ASCIIEncoding.UTF8.GetString(altRefBytes);
                double latitude = byteToDegrees(latBytes);
                double longitude = byteToDegrees(lonBytes);
                double altitude = byteToDecimal(altBytes);
                DateTime dateTime = byteToDate(dateTimeBytes);

                if (latitudeRef.Equals("S\0"))
                {
                    latitude = -latitude;
                }
                if (longitudeRef.Equals("W\0"))
                {
                    longitude = -longitude;
                }
                if (!altRef.Equals("\0"))
                {
                    altitude = -altitude;
                }
                r.Latitude = latitude;
                r.Longitude = longitude;
                r.Altitude = altitude;
                r.TimeStamp = dateTime;
                r.Path = Path.GetFullPath(file);
                r.Id = id.ToString();   
                addRecord(photo, r);
                lock (obj)
                {
                    geoTagCount++;
                }
                setMinMax(latitude, longitude);
                double percent = ((double)geoTagCount / length) * 100;
                int percentInt = (int)(Math.Round(percent));
                if (progressValue != null)
                {
                    progressValue = threadInfo.ProgressHandler;
                    progressForm.Invoke(
                        new MethodInvoker(() => progressValue.Report(percentInt)
                    ));
                }
            return r;
        }

    

        private void MsgBox(string title, string message, MessageBoxButtons buttons)
        {
            DialogResult result = MessageBox.Show(message, title, buttons);
            if (result == DialogResult.OK)
            {
               
            }
        }

        private void cancelImport(object sender, EventArgs e)
        {
            if (cts != null)
                cts.Cancel();
        }

        private Record ProcessFile(object a)
        {
            ThreadInfo threadInfo = a as ThreadInfo;
            Record r = threadInfo.Record;
            //string file = threadInfo.Folder + "\\" + threadInfo.Photo + ".jpg";
            string outPath = threadInfo.OutPath;
            int length = threadInfo.Length;
            string path;
            Bitmap bmp = null;
            PropertyItem[] propItems = null;
            r.GeoTag = true;

            string photoName = r.PhotoName;
            string photoRename = r.PhotoRename;
            //string photoSQL = "SELECT Photo_Geotag FROM PhotoList WHERE Photo_Camera = '" + photoName + "';";
            //OleDbCommand commandGetPhoto = new OleDbCommand(photoSQL, connection);

            r.PhotoName = photoRename; //new photo name 
            string geotagSQL = "UPDATE PhotoList SET PhotoList.GeoTag = True WHERE Photo_Camera = '" + photoName + "';";
            //OleDbCommand commandGeoTag = new OleDbCommand(geotagSQL, connection);
            
            path = outPath + "\\" + photoRename + ".jpg";
            string uncPath = GetUNCPath(path);
            string pathSQL = "UPDATE PhotoList SET Path = '" + path + "' WHERE Photo_Camera = '" + photoName + "';";
            //OleDbCommand commandPath = new OleDbCommand(pathSQL, connection);
            //try
            //{
            //    commandGeoTag.ExecuteNonQuery();
            //    commandPath.ExecuteNonQuery();
            //}
            //catch (Exception ex)
            //{

            //}
            r.Path = uncPath;
            threadInfo.OutPath = uncPath;
            setMinMax(r.Latitude, r.Longitude);
            try
            {
                if (!threadInfo.Zip)
                {
                    bmp = new Bitmap(threadInfo.Folder);
                    propItems = bmp.PropertyItems;
                    threadInfo.propItemLatRef = bmp.GetPropertyItem(0x0001);
                    threadInfo.propItemLat = bmp.GetPropertyItem(0x0002);
                    threadInfo.propItemLonRef = bmp.GetPropertyItem(0x0003);
                    threadInfo.propItemLon = bmp.GetPropertyItem(0x0004);
                    threadInfo.propItemAltRef = bmp.GetPropertyItem(0x0005);
                    threadInfo.propItemAlt = bmp.GetPropertyItem(0x0006);
                    threadInfo.propItemSat = bmp.GetPropertyItem(0x0008);
                    threadInfo.propItemDir = bmp.GetPropertyItem(0x0011);
                    threadInfo.propItemVel = bmp.GetPropertyItem(0x000D);
                    threadInfo.propItemPDop = bmp.GetPropertyItem(0x000B);
                    threadInfo.propItemDateTime = bmp.GetPropertyItem(0x0132);
                }
                else
                {
                    using (FileStream zipToOpen = new FileStream(threadInfo.Folder, FileMode.Open))
                    {
                        String[] tokens = zipToOpen.Name.Split('\\');
                        string s = tokens[tokens.Length - 1];
                        string key = s.Substring(0, s.Length - 4);
                        using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                        {
                            string entry = threadInfo.Photo + ".jpg";
                            ZipArchiveEntry zip = archive.GetEntry(entry);
                            //ZipArchiveEntry zip = threadInfo.Entry;
                            Stream stream = zip.Open();
                            Image img = Image.FromStream(stream);
                            propItems = img.PropertyItems;
                            threadInfo.propItemLatRef = img.GetPropertyItem(0x0001);
                            threadInfo.propItemLat = img.GetPropertyItem(0x0002);
                            threadInfo.propItemLonRef = img.GetPropertyItem(0x0003);
                            threadInfo.propItemLon = img.GetPropertyItem(0x0004);
                            threadInfo.propItemAltRef = img.GetPropertyItem(0x0005);
                            threadInfo.propItemAlt = img.GetPropertyItem(0x0006);
                            threadInfo.propItemSat = img.GetPropertyItem(0x0008);
                            threadInfo.propItemDir = img.GetPropertyItem(0x0011);
                            threadInfo.propItemVel = img.GetPropertyItem(0x000D);
                            threadInfo.propItemPDop = img.GetPropertyItem(0x000B);
                            threadInfo.propItemDateTime = img.GetPropertyItem(0x0132);
                            bmp = new Bitmap(img);
                            img.Dispose();
                            stream.Close();
                        }
                    }
                }
            }
                catch (Exception ex)
            {
                String s = ex.StackTrace;
            }
            object[] o = { threadInfo, bmp };
            bitmapQueue.Add(o);
            mre.Set();
            return r;
        }

        //private async void updateDatabase(OleDbCommand command)
        //{
        //    await int rows = command.ExecuteNonQuery();
        //}

        private async void processImage(object[] item)
        {
            try {
                
                ThreadInfo threadInfo = item[0] as ThreadInfo;
                Bitmap bmp = item[1] as Bitmap;
                //Bitmap bmp = null;
                string file = threadInfo.Folder + "\\" + threadInfo.Photo + ".jpg";
                RecordUtil RecordUtil = new RecordUtil(threadInfo.Record);
                PropertyItem propItemLat = RecordUtil.getEXIFCoordinate("latitude", threadInfo.propItemLat);
                PropertyItem propItemLon = RecordUtil.getEXIFCoordinate("longitude", threadInfo.propItemLon);
                PropertyItem propItemAlt = RecordUtil.getEXIFNumber(threadInfo.propItemAlt, "altitude", 10);
                PropertyItem propItemLatRef = RecordUtil.getEXIFCoordinateRef("latitude", threadInfo.propItemLatRef);
                PropertyItem propItemLonRef = RecordUtil.getEXIFCoordinateRef("longitude", threadInfo.propItemLonRef);
                PropertyItem propItemAltRef = RecordUtil.getEXIFAltitudeRef(threadInfo.propItemAltRef);
                PropertyItem propItemDir = RecordUtil.getEXIFNumber(threadInfo.propItemDir, "bearing", 10);
                PropertyItem propItemVel = RecordUtil.getEXIFNumber(threadInfo.propItemVel, "velocity", 100);
                PropertyItem propItemPDop = RecordUtil.getEXIFNumber(threadInfo.propItemPDop, "pdop", 10);
                PropertyItem propItemSat = RecordUtil.getEXIFInt(threadInfo.propItemSat, threadInfo.Record.Satellites);
                PropertyItem propItemDateTime = RecordUtil.getEXIFDateTime(threadInfo.propItemDateTime);
                RecordUtil = null;
                Image image = null;
                //do image correction
                //CLAHE correction
                try
                {
                    Image<Bgr, Byte> img = new Image<Bgr, Byte>(bmp);
                    Mat src = img.Mat;
                    Image<Bgr, Byte> emguImage = CorrectionUtil.ClaheCorrection(src, 0.5);
                    bmp.Dispose();
                    emguImage = CorrectionUtil.GammaCorrection(emguImage);
                    image = CorrectionUtil.ImageFromEMGUImage(emguImage);
                    emguImage.Dispose();
                }
                catch (Exception ex)
                {
                    String s = ex.StackTrace;
                }
                image.SetPropertyItem(propItemLat);
                image.SetPropertyItem(propItemLon);
                image.SetPropertyItem(propItemLatRef);
                image.SetPropertyItem(propItemLonRef);
                image.SetPropertyItem(propItemAlt);
                image.SetPropertyItem(propItemAltRef);
                image.SetPropertyItem(propItemDir);
                image.SetPropertyItem(propItemVel);
                image.SetPropertyItem(propItemPDop);
                image.SetPropertyItem(propItemSat);
                image.SetPropertyItem(propItemDateTime);
                await saveFile(image, threadInfo.OutPath);               
                lock (obj)
                {
                    geoTagCount++;
                }
                image.Dispose();
                image = null;               
            }
            catch (Exception ex)
            {
                String s = ex.StackTrace;
        }
    }

    [DllImport("mpr.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int WNetGetConnection([MarshalAs(UnmanagedType.LPTStr)] string localName,
        [MarshalAs(UnmanagedType.LPTStr)] StringBuilder remoteName,
    ref int length);
        public static string GetUNCPath(string originalPath)
        {
            StringBuilder sb = new StringBuilder(512);
            int size = sb.Capacity;

            if (originalPath.Length > 2 && originalPath[1] == ':')
            {
                char c = originalPath[0];
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
                {
                    int error = WNetGetConnection(originalPath.Substring(0, 2), sb, ref size);
                    if (error == 0)
                    {
                        DirectoryInfo dir = new DirectoryInfo(originalPath);
                        string path = Path.GetFullPath(originalPath).Substring(Path.GetPathRoot(originalPath).Length);
                        return Path.Combine(sb.ToString().TrimEnd(), path);
                    }
                }
            }

            return originalPath;
        }

        private async void incrementGeoTagError(object a)
        {
            string s = a as string;
            await Task.Run(() =>
            {
                if (s.Equals("nokey"))
                {
                    lock (obj)
                    {
                        errorCount++;            
                    }
                }
                else
                {
                    lock (obj)
                    {
                        stationaryCount++;
                    }
                }
            });
        }

        private async Task saveFile(Image image, string path)
        {
            await Task.Run(() =>
            {
                image.Save(path);
            });
        }

        private DateTime byteToDate(byte[] b)
        {
            try
            {
                int year = byteToDateInt(b, 0, 4);
                string dateTime = Encoding.UTF8.GetString(b);
                int month = byteToDateInt(b, 5, 2);
                int day = byteToDateInt(b, 8, 2);
                int hour = byteToDateInt(b, 11, 2);
                int min = byteToDateInt(b, 14, 2);
                int sec = byteToDateInt(b, 17, 2);
                return new DateTime(year, month, day, hour, min, sec);
            } catch (ArgumentOutOfRangeException ex)
            {
                return new DateTime();
            }
        }
        private int byteToDateInt(byte[] b, int offset, int len)
        {
            byte[] a = new byte[len];
            Array.Copy(b, offset, a, 0, len);
            string s = ASCIIEncoding.UTF8.GetString(a);
            try
            {
                int i = Int32.Parse(s);
                return i;
            }
            catch (FormatException e)
            {
                return -1;
            }
        }

        private double byteToDecimal(byte[] b) //type 5
        {
            double numerator = BitConverter.ToInt32(b, 0);
            double denominator = BitConverter.ToInt32(b, 4);

            return Math.Round(numerator / denominator, 2);
        }
        private double byteToDegrees(byte[] source)
        {
            double coordinate = 0;
            int dms = 1; //degrees minute second divisor
            for (int offset = 0; offset < source.Length; offset += 8)
            {
                byte[] b = new byte[4];
                Array.Copy(source, offset, b, 0, 4);
                int temp = BitConverter.ToInt32(b, 0);
                Array.Copy(source, offset + 4, b, 0, 4);
                int multiplier = BitConverter.ToInt32(b, 0) * dms;
                dms *= 60;
                coordinate += Convert.ToDouble(temp) / Convert.ToDouble(multiplier);
            }
            return Math.Round(coordinate, 6);
        }
    }
}
