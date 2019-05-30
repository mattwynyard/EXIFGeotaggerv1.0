using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GMap.NET;

using GMap.NET.MapProviders;
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
        public delegate void GeoTagCompleteDelegate(int geotagCount, int stationaryCount, int errorCount);
        //properties
        private OleDbConnection connection;
        private static readonly Object obj = new Object();
        private ProgressForm progressForm;
        int geoTagCount;
        int errorCount;
        int stationaryCount;
        int id;
        CancellationTokenSource cts;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ThreadUtil()
        {
        }

        /// <summary>
        /// Builds a queue containing all the file path to process
        /// </summary>
        /// <param name="path"> the full path of the folder containing the files</param>
        /// <returns>the queue</returns>
        public async Task<BlockingCollection<string>> buildQueue(string path)
        {
            BlockingCollection<string> mFileQueue = new BlockingCollection<string>();
            await Task.Run(() =>
            {
            
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                mFileQueue.Add(file);
            }
                
            });
            return mFileQueue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="allRecords"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, Record>> readFromDatabase(string path, Boolean allRecords)
        {
            Dictionary<string, Record> recordDict = new Dictionary<string, Record>();
            ProgressForm progressForm = new ProgressForm("Reading from database...");
            progressForm.Show();
            progressForm.BringToFront();
            progressForm.cancel += cancelImport;
            cts = new CancellationTokenSource();
            var token = cts.Token;
            var progressHandler1 = new Progress<int>(value =>
            {
                progressForm.ProgressValue = value;
                progressForm.Message = "Database read, please wait... " + value.ToString() + "% completed";

            });
            var progressValue = progressHandler1 as IProgress<int>;
            try
            {
                await Task.Run(() =>
                {
                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }
                    string connectionString = string.Format("Provider={0}; Data Source={1}; Jet OLEDB:Engine Type={2}",
                                        "Microsoft.Jet.OLEDB.4.0", path, 5);
                    connection = new OleDbConnection(connectionString);
                    //string connectionStr = connection.ConnectionString;
                    string strSQL;
                    string lengthSQL; //sql count string
                    int length; //number of records to process
                    //double percent;
                    if (allRecords)
                    {
                        strSQL = "SELECT * FROM PhotoList";
                        lengthSQL = "SELECT Count(Photo) FROM PhotoList;";
                    }
                    else
                    {
                        strSQL = "SELECT * FROM PhotoList WHERE PhotoList.GeoMark = true;";
                        lengthSQL = "SELECT Count(PhotoID) FROM PhotoList WHERE PhotoList.GeoMark = true;";
                    }
                    OleDbCommand commandLength = new OleDbCommand(lengthSQL, connection);
                    OleDbCommand command = new OleDbCommand(strSQL, connection);

                    connection.Open();
                    length = (Int32)commandLength.ExecuteScalar();
                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        int i = 0;
                        while (reader.Read())
                        {
                            Object[] row = new Object[reader.FieldCount];
                            reader.GetValues(row);
                            String photo = (string)row[1];
                            Record r = buildDictionary(i, row).Result;
                            recordDict.Add(r.PhotoName, r);
                            i++;
                            double percent = ((double)i / length) * 100;
                            int percentInt = (int)Math.Ceiling(percent);
                            if (progressValue != null)
                            {
                                progressValue.Report(percentInt);

                            }
                            //token.ThrowIfCancellationRequested();
                        }
                    }

                }, cts.Token);

            }
            catch (ArgumentException ex)
            {

            }
            catch (NullReferenceException ex)
            {
                //txtConsole.AppendText(ex.StackTrace);
            }
            catch (OperationCanceledException)
            {
                cts.Dispose();
                //string titleCancel = "Cancelled";
                //string messageCancel = "Read cancelled";
                //MessageBoxButtons buttonsCancel = MessageBoxButtons.OK;
                //MsgBox(messageCancel, titleCancel, buttonsCancel);
                //progressForm.Close();
            }
            progressForm.Close();
            return recordDict;
        }

        /// <summary>
        /// Intialises a new Record and adds data extracted from access to each relevant field.
        /// The record is then added to the Record Dictionary.
        /// </summary>
        /// <param name="i: the number of records read"></param>
        /// <param name="row: the access record"></param>
        private async Task<Record> buildDictionary(int i, Object[] row)
        {
            Record r = new Record((string)row[1]);
            await Task.Run(() =>
            {             
                try
                {
                    int id = (int)row[0];
                    r.Id = id.ToString();
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
                                tag.Path = Path.GetFullPath(r.Path);
                                overlay.Markers.Add(marker);
                            });
                    }
                }
                catch (OperationCanceledException)
                {
                    //overlay = new GMapOverlay(layer);
                    cts.Dispose();
                    //string titleCancel = "Cancelled";
                    //string messageCancel = "Import cancelled";
                    //MessageBoxButtons buttonsCancel = MessageBoxButtons.OK;
                    //MsgBox(messageCancel, titleCancel, buttonsCancel);
                    //progressForm.Close();
                }
            }, cts.Token);
            progressForm.Close();
            return overlay;
        }

        private Record readData(ThreadInfo threadInfo)
        {
            Record r;
            string outPath = threadInfo.OutPath;
            int length = threadInfo.Length;
            string file = Path.GetFullPath(threadInfo.File);
            string photo = file;
            Image image = new Bitmap(file);
            r = new Record(photo);
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

                lock (obj)
                {
                    addRecord(photo, r);
                    geoTagCount++;
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
                }
            return r;
        }

        public  async Task<Dictionary<string, Record>> writeGeoTag(Dictionary<string, Record> recordDict, BlockingCollection<string> fileQueue, string inPath, string outPath)
        {
            int mQueueSize = fileQueue.Count;
      
            progressForm = new ProgressForm("Writing geotags to photos...");
            string[] _files = Directory.GetFiles(inPath);
            progressForm.Show();
            progressForm.BringToFront();
            progressForm.cancel += cancelImport;
            cts = new CancellationTokenSource();
            var token = cts.Token;
            var progressHandler1 = new Progress<int>(value =>
            {
                progressForm.ProgressValue = value;
                progressForm.Message = "Geotagging, please wait... " + value.ToString() + "% completed\n" +
                geoTagCount + " of " + mQueueSize + " photos geotagged\n" +
               "Photos with no geomark: " + stationaryCount + "\n" + "Photos with no gps point: " + errorCount + "\n";
            });
            var progressValue = progressHandler1 as IProgress<int>;
            geoTagCount = 0;
            errorCount = 0;
            stationaryCount = 0;
            Dictionary<string, Record> newRecordDict = new Dictionary<string, Record>();

            int processors = Environment.ProcessorCount;
            int minWorkerThreads = processors;
            int minIOThreads = processors;
            int maxWorkerThreads = processors;
            int maxIOThreads = processors;

            //ThreadPool.SetMinThreads(minWorkerThreads, minIOThreads);
            //ThreadPool.SetMaxThreads(maxWorkerThreads, maxIOThreads);
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
                        Parallel.Invoke (
                            () =>
                            {
                                ThreadInfo threadInfo = new ThreadInfo();
                                threadInfo.OutPath = outPath;
                                threadInfo.Length = mQueueSize;
                                threadInfo.ProgressHandler = progressHandler1;
                                threadInfo.File = fileQueue.Take();
                                try
                                {
                                      Record r = recordDict[Path.GetFileNameWithoutExtension(threadInfo.File)];
                                      threadInfo.Record = r;

                                    if (r.GeoMark)
                                    {
                                        Record newRecord = null;
                                        newRecord = ProcessFile(threadInfo).Result;
                                        newRecordDict.Add(r.PhotoName, r);
                                    }
                                    else
                                    {
                                        object a = "nogps";
                                        incrementGeoTagError(a);
                                    }
                                }
                                catch (KeyNotFoundException ex)
                                {
                                    object a = "nokey";
                                    incrementGeoTagError(a);
                                }
                            });
                    }
                }
                catch (OperationCanceledException)
                {
                    cts.Dispose();
                    //cts.Cancel();
                    //progressForm.Close();
                    //string titleCancel = "Geotagging cancelled";
                    //string messageCancel = "Cancelled";
                    //MessageBoxButtons buttonsCancel = MessageBoxButtons.OK;
                    //MsgBox(messageCancel, titleCancel, buttonsCancel);
                    //progressForm.Close();
                    //return newRecordDict;
                }
                
            }, cts.Token);
            progressForm.Invoke(
                        new MethodInvoker(() => progressValue.Report(100)
                    ));
            progressForm.enableOK();
            progressForm.disableCancel();
            return newRecordDict;
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

        private async Task<Record> ProcessFile(object a)
        {
            ThreadInfo threadInfo = a as ThreadInfo;
            Record r = threadInfo.Record;
            await Task.Factory.StartNew(async () =>
            {
                
            var progressValue = threadInfo.ProgressHandler as IProgress<int>;
            string outPath = threadInfo.OutPath;
            int length = threadInfo.Length;
            
            string photo;
            string path;
            Bitmap image = new Bitmap(threadInfo.File);
            PropertyItem[] propItems = image.PropertyItems;
            PropertyItem propItemLatRef = image.GetPropertyItem(0x0001);
            PropertyItem propItemLat = image.GetPropertyItem(0x0002);
            PropertyItem propItemLonRef = image.GetPropertyItem(0x0003);
            PropertyItem propItemLon = image.GetPropertyItem(0x0004);
            PropertyItem propItemAltRef = image.GetPropertyItem(0x0005);
            PropertyItem propItemAlt = image.GetPropertyItem(0x0006);
            PropertyItem propItemSat = image.GetPropertyItem(0x0008);
            PropertyItem propItemDir = image.GetPropertyItem(0x0011);
            PropertyItem propItemVel = image.GetPropertyItem(0x000D);
            PropertyItem propItemPDop = image.GetPropertyItem(0x000B);
            PropertyItem propItemDateTime = image.GetPropertyItem(0x0132);
            RecordUtil RecordUtil = new RecordUtil(r);
            propItemLat = RecordUtil.getEXIFCoordinate("latitude", propItemLat);
            propItemLon = RecordUtil.getEXIFCoordinate("longitude", propItemLon);
            propItemAlt = RecordUtil.getEXIFNumber(propItemAlt, "altitude", 10);
            propItemLatRef = RecordUtil.getEXIFCoordinateRef("latitude", propItemLatRef);
            propItemLonRef = RecordUtil.getEXIFCoordinateRef("longitude", propItemLonRef);
            propItemLonRef = RecordUtil.getEXIFCoordinateRef("longitude", propItemLonRef);
            propItemAltRef = RecordUtil.getEXIFAltitudeRef(propItemAltRef);
            propItemDir = RecordUtil.getEXIFNumber(propItemDir, "bearing", 10);
            propItemVel = RecordUtil.getEXIFNumber(propItemVel, "velocity", 100);
            propItemPDop = RecordUtil.getEXIFNumber(propItemPDop, "pdop", 10);
            propItemSat = RecordUtil.getEXIFInt(propItemSat, r.Satellites);
            propItemDateTime = RecordUtil.getEXIFDateTime(propItemDateTime);
            RecordUtil = null;
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
            r.GeoTag = true;
            
                string photoName = Path.GetFileNameWithoutExtension(threadInfo.File);
                string photoSQL = "SELECT Photo_Geotag FROM PhotoList WHERE Photo_Camera = '" + photoName + "';";
                OleDbCommand commandGetPhoto = new OleDbCommand(photoSQL, connection);

                photo = (string)commandGetPhoto.ExecuteScalar();

                r.PhotoName = photo; //new photo name 
                string geotagSQL = "UPDATE PhotoList SET PhotoList.GeoTag = True WHERE Photo_Camera = '" + photoName + "';";
                OleDbCommand commandGeoTag = new OleDbCommand(geotagSQL, connection);
                
                commandGeoTag.ExecuteNonQuery();
                path = outPath + "\\" + photo + ".jpg";
                string pathSQL = "UPDATE PhotoList SET Path = '" + path + "' WHERE Photo_Camera = '" + photoName + "';";
                OleDbCommand commandPath = new OleDbCommand(pathSQL, connection);
                commandPath.ExecuteNonQuery();
                r.Path = path;
                lock (obj)
                {
                    geoTagCount++;

                    setMinMax(r.Latitude, r.Longitude);
                    int totalCount = geoTagCount + errorCount;
                    double percent = ((double)totalCount / length) * 100;
                    int percentInt = (int)Math.Floor(percent);
                    progressValue = threadInfo.ProgressHandler;
                    progressForm.Invoke(
                        new MethodInvoker(() => progressValue.Report(percentInt)
                    ));
                }               
                await saveFile(image, path);
                image.Dispose();
                image = null;
            });
            return r;
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
