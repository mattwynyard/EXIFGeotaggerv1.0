﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
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
        private CancellationTokenSource cts;
        private ConcurrentDictionary<string, string> photoDict;
        private Boolean geotagging = false;


        /// <summary>
        /// Default constructor
        /// </summary>
        public ThreadUtil()
        {
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
            photoDict = new ConcurrentDictionary<string, string>();
            Task build = Task.Factory.StartNew(() =>
            {
                if (zip)
                {
                    string[] files = Directory.GetFiles(path);
                    foreach (var file in files)
                    {
                        String f = file;
                        using (FileStream zipToOpen = new FileStream(file, FileMode.Open))
                        {
                            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                            {
                                foreach (ZipArchiveEntry entry in archive.Entries)
                                {
                                    String s = entry.FullName;
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
                    string[] files = Directory.GetFiles(path);
                    foreach (var file in files)
                    {
                        string key = Path.GetFileNameWithoutExtension(file);
                        
                        photoDict.TryAdd(key, path);
                    }
                }
            });
            Task.WaitAll(build);
            
      
        }

        public async Task<ConcurrentDictionary<string, Record>> buildDictionary(string photoPath, string dbPath, string outPath, Boolean allRecords)
        {
            ProgressForm progressForm = new ProgressForm("Reading from database...");
            ConcurrentDictionary<string, Record> dict = new ConcurrentDictionary<string, Record>();
            BlockingCollection<Record> queue = new BlockingCollection<Record>();
            BlockingCollection<ThreadInfo> geoTagQueue = new BlockingCollection<ThreadInfo>();
            //BlockingCollection<String> fileQueue = new BlockingCollection<String>();
            progressForm.Show();
            progressForm.BringToFront();
            progressForm.cancel += cancelImport;
            cts = new CancellationTokenSource();
            var token = cts.Token;
            var progressHandler1 = new Progress<object>(a =>
            {
                int[] values = (int[])a;
                progressForm.ProgressValue = values[0];
                progressForm.Message = "Database read, please wait... " + values[0].ToString() + "% completed\n" +
                values[1] + " of " + values[2] + " records processed\n" +
                "Queue size: " + values[3] + "\n" +
                "Record Dictionary size: " + values[4] + "\n" +
                "Photo Dictionary size: " + values[5] + "\n" +
                "Geotag count: " + values[6] + "\n" +
                "No photo: " + values[7];

            });
            var progressValue = progressHandler1 as IProgress<object>;
            int length = 0;
            int count = 0;
            int noGeomark = 0;
            int noPhoto = 0;
            int geoTagCount = 0;

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
                strSQL = "SELECT * FROM PhotoList WHERE PhotoList.GeoMark = true;";
                lengthSQL = "SELECT Count(PhotoID) FROM PhotoList WHERE PhotoList.GeoMark = true;";
                strRecord = "SELECT * FROM PhotoList WHERE PhotoID = @photo AND PhotoList.GeoMark = true;";
            }
            OleDbCommand commandLength = new OleDbCommand(lengthSQL, connection);
            OleDbCommand command = new OleDbCommand(strSQL, connection);
            connection.Open();
            length = Convert.ToInt32(commandLength.ExecuteScalar());
            commandLength.Dispose();
            int queueCount = 0;
            int noRecord = 0;
            string[] files = Directory.GetFiles(photoPath);
 
            Task producer = Task.Factory.StartNew(async () =>
            {
                using (OleDbDataReader reader = command.ExecuteReader())
                {
                    Object[] row;
                    while (reader.Read())
                    {
                        row = new Object[reader.FieldCount];
                        reader.GetValues(row);
                        Record r = await buildRecord(row);
                        queue.Add(r);
                        if (queue.Count > 25000)
                        {
                            if (geotagging)
                            {
                                Thread.Sleep(400000);
                            } else
                            {
                                Thread.Sleep(25000);
                            }                          
                        }                       
                    }
                    reader.Close();
                    queue.CompleteAdding();
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
                        string path = photoPath + "\\" + item.PhotoName + ".jpg";
                        string folder;
                        //zipDict.TryRemove(item.PhotoName, out folder);

                        if (File.Exists(path))
                        {
                           
                            ThreadInfo threadInfo = new ThreadInfo();
                            threadInfo.OutPath = outPath;
                            threadInfo.Record = item;
                            if (threadInfo.Record.GeoMark)
                            {
                                geotagging = true;
                                threadInfo.File = path;
                                Record newRecord = null;
                                Task geotagQueue = Task.Factory.StartNew(() =>
                                {
                                    newRecord = ProcessFile(threadInfo).Result;
                                    dict.TryAdd(item.PhotoName, item);
                                    geoTagCount++;
                                });
                                Task.WaitAll(geotagQueue);
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
                            geotagging = false;
                            lock (obj)
                            {
                                noPhoto++;
                            }
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
                    Task progress = Task.Factory.StartNew(() =>
                    {
                        double percent = ((double)count / length) * 100;
                        int percentInt = (int)Math.Ceiling(percent);
                        int[] values = { percentInt, count, length, queue.Count, dict.Count, photoDict.Count, geoTagCount, noPhoto };
                        object a = (object)values;
                        progressForm.Invoke(new MethodInvoker(() =>
                        {
                            if (progressValue != null)
                            {
                                progressValue.Report(a);

                            }
                        }));

                    });
                    Task.WaitAll(progress);
                }
            }, cts.Token);

        await Task.WhenAll(producer, consumer);
        progressForm.Close();
        return dict;
    }

        public async Task<ConcurrentDictionary<string, Record>> WriteGeotag(BlockingCollection<string> queue, ConcurrentDictionary<string, Record> dict, string inPath, string outPath)
        {
            int mQueueSize = queue.Count;

            progressForm = new ProgressForm("Writing geotags to photos...");
            //string[] _files = Directory.GetFiles(inPath);
            progressForm.Show();
            progressForm.BringToFront();
            progressForm.cancel += cancelImport;
            cts = new CancellationTokenSource();
            var token = cts.Token;
            var progressHandler1 = new Progress<object>(a =>
            {
                int[] values = (int[])a;
                progressForm.ProgressValue = values[0];
                progressForm.Message = "Geotagging, please wait... " + values[0].ToString() + "% completed\n" +
                geoTagCount + " of " + mQueueSize + " photos geotagged\n" +
               "Photos with no geomark: " + stationaryCount + "\n" + "Photos with no gps point: " + errorCount + "\n";
            });
            var progressValue = progressHandler1 as IProgress<object>;
            geoTagCount = 0;
            errorCount = 0;
            stationaryCount = 0;
            ConcurrentDictionary<string, Record> newRecordDict = new ConcurrentDictionary<string, Record>();

            await Task.Factory.StartNew(() =>
            {
                foreach (var item in queue.GetConsumingEnumerable())
                {
                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }
                    Parallel.Invoke(() =>
                   {
                       try
                       {
                           ThreadInfo threadInfo = new ThreadInfo();
                           threadInfo.OutPath = outPath;
                           threadInfo.File = item;
                           string key = Path.GetFileNameWithoutExtension(item);
                           Record value;
                           if (dict.TryGetValue(key, out value))
                           {
                               threadInfo.Record = value;
                               //threadInfo.File = outPath + "\\" + item + ".jpg";
                               //threadInfo.File = item;
                               //Record r = dict[item];
                               //threadInfo.Record = r;

                               if (threadInfo.Record.GeoMark)
                               {
                                   Record newRecord = null;
                                   //newRecord = ProcessFile(threadInfo).Result;
                                   newRecordDict.TryAdd(threadInfo.Record.PhotoName, threadInfo.Record);
                                   Task.Run(() =>
                                   {
                                       double percent = ((double)(mQueueSize - queue.Count) / mQueueSize) * 100;
                                       int percentInt = (int)Math.Ceiling(percent);
                                       int[] values = { percentInt };
                                       object a = (object)values;
                                       progressForm.Invoke(new MethodInvoker(() =>
                                       {
                                           if (progressValue != null)
                                           {
                                               progressValue.Report(a);

                                           }
                                       }));
                                       Thread.Sleep(10);
                                   });
                               }
                               else
                               {
                                   object a = "nogeomark";
                                   incrementGeoTagError(a);
                               }
                           }
                           else
                           {

                           }
                       }
                       catch (Exception ex2)
                       {
                           String s = ex2.StackTrace;
                       }
                   });
                }
            });

            return newRecordDict;
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

        private async Task<Record> ProcessFile(object a)
        {
            ThreadInfo threadInfo = a as ThreadInfo;
            Record r = threadInfo.Record;
           
            string outPath = threadInfo.OutPath;
            int length = threadInfo.Length;
            string path;
            try
            {
                Bitmap bmp = new Bitmap(threadInfo.File);

                PropertyItem[] propItems = bmp.PropertyItems;
                PropertyItem propItemLatRef = bmp.GetPropertyItem(0x0001);
                PropertyItem propItemLat = bmp.GetPropertyItem(0x0002);
                PropertyItem propItemLonRef = bmp.GetPropertyItem(0x0003);
                PropertyItem propItemLon = bmp.GetPropertyItem(0x0004);
                PropertyItem propItemAltRef = bmp.GetPropertyItem(0x0005);
                PropertyItem propItemAlt = bmp.GetPropertyItem(0x0006);
                PropertyItem propItemSat = bmp.GetPropertyItem(0x0008);
                PropertyItem propItemDir = bmp.GetPropertyItem(0x0011);
                PropertyItem propItemVel = bmp.GetPropertyItem(0x000D);
                PropertyItem propItemPDop = bmp.GetPropertyItem(0x000B);
                PropertyItem propItemDateTime = bmp.GetPropertyItem(0x0132);
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
                } catch (Exception ex)
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
                r.GeoTag = true;

                string photoName = r.PhotoName;
                string photoRename = r.PhotoRename;
                //string photoSQL = "SELECT Photo_Geotag FROM PhotoList WHERE Photo_Camera = '" + photoName + "';";
                //OleDbCommand commandGetPhoto = new OleDbCommand(photoSQL, connection);

                r.PhotoName = photoRename; //new photo name 
                string geotagSQL = "UPDATE PhotoList SET PhotoList.GeoTag = True WHERE Photo_Camera = '" + photoName + "';";
                //OleDbCommand commandGeoTag = new OleDbCommand(geotagSQL, connection);

                //commandGeoTag.ExecuteNonQuery();
                path = outPath + "\\" + photoRename + ".jpg";
                string pathSQL = "UPDATE PhotoList SET Path = '" + path + "' WHERE Photo_Camera = '" + photoName + "';";
                //OleDbCommand commandPath = new OleDbCommand(pathSQL, connection);
                //commandPath.ExecuteNonQuery();
                path = outPath + "\\" + photoRename + ".jpg";
                r.Path = path;
                int totalCount;
                lock (obj)
                {
                    geoTagCount++;

                }
                totalCount = geoTagCount + errorCount + stationaryCount;
                setMinMax(r.Latitude, r.Longitude);
                await saveFile(image, path);
                image.Dispose();
                image = null;
            }
            catch (Exception ex)
            {
                String s = ex.StackTrace;
            }
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
