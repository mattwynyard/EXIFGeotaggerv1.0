using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.ComponentModel;
using System.Data;
using GMap.NET;
using System.Windows.Forms;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace EXIFGeotaggerv0._1
{
    public partial class EXIFGeoTagger : Form
    {
        String mDBPath;
        Dictionary<string, Record> mRecordDict;
        string[] mFiles; //array containing absolute paths of photos.

        private int geoTagCount;
        private int errorCount;

        private Assembly myAssembly;
        private Stream myStream;
        private Bitmap bmpMarker;
        private Bitmap bmpPhoto;
        private GMapOverlay markers;
        private GMapOverlay photoMarkers;
        private ProgressForm progress;

        private List<GMapMarker> photoMarkerArray;
        private List<GMapMarker> gpsMarkerArray;

        private Boolean data = false;

        public EXIFGeoTagger()
        {
            InitializeComponent();
            mRecordDict = new Dictionary<string, Record>();
            this.menuRunGeoTag.Enabled = false;

        }

        private void gMap_Load(object sender, EventArgs e)
        {
            gMap.MapProvider = GMapProviders.OpenStreetMap;
            gMap.Position = new PointLatLng(-36.939318, 174.892701);
            gMap.MouseWheelZoomEnabled = true;
            gMap.ShowCenter = false;
            gMap.MaxZoom = 23;
            gMap.MinZoom = 5;
            gMap.Zoom = 10;
            gMap.DragButton = MouseButtons.Left;
            gMap.OnMapZoomChanged += gMap_OnMapZoomChanged;
            //gMap.MouseMove += gMap_OnMouseMoved;

        }

        private void fileMenuOpen_Click(object sender, ToolStripItemClickedEventArgs e)
        {
            //connectAccess(sender, e);
        }

        #region DatabaseConnect
        private void connectAccess(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "mdb files|*.mdb";
            openFileDialog.FilterIndex = 2;
            openFileDialog.Title = "Browse Files";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.DefaultExt = "mdb";
            openFileDialog.ShowDialog();

            mDBPath = openFileDialog.FileName;

            string connectionString = string.Format("Provider={0}; Data Source={1}; Jet OLEDB:Engine Type={2}",
                                "Microsoft.Jet.OLEDB.4.0", mDBPath, 5);

            OleDbConnection connection = new OleDbConnection(connectionString);
            string connectionStr = connection.ConnectionString;

            string strSQL = "SELECT * FROM PhotoList WHERE PhotoList.GeoMark = true;";

            OleDbCommand command = new OleDbCommand(strSQL, connection);
            // Open the connection and execute the select command.  
            int recordCount = 0;
            try
            {
                // Open connecton  
                connection.Open();

                using (OleDbDataReader reader = command.ExecuteReader())
                {
                    int i = 0;
                    while (reader.Read())
                    {
                        Object[] row = new Object[reader.FieldCount];
                        reader.GetValues(row);
                        String photo = (string)row[1];
                        txtConsole.AppendText("Reading... " + photo + " from database" + Environment.NewLine);
                        txtConsole.Clear();
                        buildDictionary(i, row);
                        i++;
                    }
                    recordCount = i;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                this.menuRunGeoTag.Enabled = false;
            }
            this.menuRunGeoTag.Enabled = true;

        }

        private void buildDictionary(int i, Object[] row)
        {
            try
            {
                //Record r = mRecordDict[photo];
                Record r = new Record((string)row[1]);
                r.Latitude = (double)row[2];
                r.Longitude = (double)row[3];
                r.Altitude = (double)row[4];
                r.Bearing = Convert.ToDouble(row[5]);
                r.Velocity = Convert.ToDouble(row[6]);
                r.Satellites = Convert.ToInt32(row[7]);
                r.PDop = Convert.ToDouble(row[8]);
                r.Inspector = Convert.ToString(row[9]);
                r.TimeStamp = Convert.ToDateTime(row[11]);
                mRecordDict.Add(r.PhotoName, r);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }
        #endregion


        #region Markers
        private void markersMenuItem_Click(object sender, EventArgs e)
        {
            markers = new GMapOverlay("markers");
            gpsMarkerArray = new List<GMapMarker>();
            markers = buildMarker("EXIFGeotaggerv0._1.BitMap.OpenCamera8px.png", "markers");
            gMap.Overlays.Add(markers);
            txtConsole.Clear();
            txtConsole.AppendText("Built markers...");

        }

        private void photosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            photoMarkers = new GMapOverlay("photos");
            photoMarkerArray = new List<GMapMarker>();
            photoMarkers = buildPhotoMarker("EXIFGeotaggerv0._1.BitMap.OpenCameraOrange_8px.png", "photos");
            gMap.Overlays.Add(photoMarkers);
        }

        private Bitmap getIcon(String icon)
        {
            myAssembly = Assembly.GetExecutingAssembly();
            myStream = myAssembly.GetManifestResourceStream(icon);
            return (Bitmap)Image.FromStream(myStream);
        }

        private GMapOverlay buildPhotoMarker(String icon, String name)
        {
            
            bmpPhoto = getIcon(icon);
            browseFolder();
            foreach (string filePath in mFiles)
            {
                try
                {
                    Image image = new Bitmap(filePath);
                    PropertyItem[] propItems = image.PropertyItems;
                    PropertyItem propItemLatRef = image.GetPropertyItem(0x0001);
                    PropertyItem propItemLat = image.GetPropertyItem(0x0002);
                    PropertyItem propItemLonRef = image.GetPropertyItem(0x0003);
                    PropertyItem propItemLon = image.GetPropertyItem(0x0004);
                    image.Dispose();
                    byte[] latBytes = propItemLat.Value;
                    byte[] latRefBytes = propItemLatRef.Value;
                    byte[] lonBytes = propItemLon.Value;
                    byte[] lonRefBytes = propItemLonRef.Value;

                    string latitudeRef = ASCIIEncoding.UTF8.GetString(latRefBytes);
                    string longitudeRef = ASCIIEncoding.UTF8.GetString(lonRefBytes);

                    double latitude = byteToDegrees(latBytes);
                    double longitude = byteToDegrees(lonBytes);
                    if (latitudeRef.Equals("S\0"))
                    {
                        latitude = -latitude;
                    }
                    if (longitudeRef.Equals("W\0"))
                    {
                        longitude = -longitude;
                    }
                    GMapMarker marker = new GMarkerGoogle(new PointLatLng(latitude, longitude), bmpPhoto);
                    //marker.Tag = record.Value.PhotoName + "\n" + record.Value.TimeStamp;
                    photoMarkers.Markers.Add(marker);
                    photoMarkerArray.Add(marker);

                    //txtConsole.AppendText("Latitude: " + latitude.ToString() + " Longitude: " + longitude.ToString() + Environment.NewLine);
                } catch (ArgumentException ex) {

                }
                catch (NullReferenceException ex)
                {
                    txtConsole.AppendText(ex.StackTrace);
                }
            }
            txtConsole.AppendText("Photos ready.." + Environment.NewLine);
            return photoMarkers;
        }



        private GMapOverlay buildMarker(String icon, String name)
        {
            bmpMarker = getIcon(icon);

            if (mRecordDict != null)
            {
                foreach (KeyValuePair<string, Record> record in mRecordDict)
                {
                    Double lat = record.Value.Latitude;
                    Double lon = record.Value.Longitude;
                    GMapMarker marker = new GMarkerGoogle(new PointLatLng(lat, lon), bmpMarker);

                    marker.Tag = record.Value.PhotoName;
                    markers.Markers.Add(marker);
                    gpsMarkerArray.Add(marker);
                }
            }
            //bmpMarker.Dispose();
            return markers;
        }
        #endregion

        private GMapOverlay rebuildMarkers(GMapOverlay overlay, List<GMapMarker> array, String icon)
        {
            //bmpPhoto.Dispose();
            overlay.Markers.Clear();
            bmpPhoto = getIcon(icon);
            foreach (GMapMarker m in array)
            {
               
                GMapMarker marker = new GMarkerGoogle(m.Position, bmpPhoto);
                if (m.Tag != null)
                {
                    marker.Tag = m.Tag;
                }
                
                overlay.Markers.Add(marker);
            }
            return overlay;
        }

        private void gMap_OnMapZoomChanged()
        {
            txtConsole.Clear();
            txtConsole.AppendText(gMap.Zoom.ToString());
            if ((int)gMap.Zoom < 12)
            {
                if (markers != null)
                {
                    markers = rebuildMarkers(markers, gpsMarkerArray, "EXIFGeotaggerv0._1.BitMap.OpenCamera4px.png");
                }
                if (photoMarkers != null)
                {
                    
                    photoMarkers = rebuildMarkers(photoMarkers, photoMarkerArray, "EXIFGeotaggerv0._1.BitMap.OpenCameraOrange_4px.png");
                }

                }
            else if ((int)gMap.Zoom < 16 && (int)gMap.Zoom >= 12)
            {
                if (markers != null)
                {
                    markers = rebuildMarkers(markers, gpsMarkerArray, "EXIFGeotaggerv0._1.BitMap.OpenCamera8px.png");
                }
                if (photoMarkers != null)
                {
                    photoMarkers = rebuildMarkers(photoMarkers, photoMarkerArray, "EXIFGeotaggerv0._1.BitMap.OpenCameraOrange_8px.png");
                }

                }
            else if ((int)gMap.Zoom < 18 && (int)gMap.Zoom >= 16)
            {
                if (markers != null)
                {
                    markers = rebuildMarkers(markers, gpsMarkerArray, "EXIFGeotaggerv0._1.BitMap.OpenCamera12px.png");
                }
                if (photoMarkers != null)
                {
                    photoMarkers = rebuildMarkers(photoMarkers, photoMarkerArray, "EXIFGeotaggerv0._1.BitMap.OpenCameraOrange_12px.png");
                }

                }
            else if ((int)gMap.Zoom < 20 && (int)gMap.Zoom >= 18)
            {
                if (markers != null)
                {
                    markers = rebuildMarkers(markers, gpsMarkerArray, "EXIFGeotaggerv0._1.BitMap.OpenCamera16px.png");
                }
                if (photoMarkers != null)
                {
                    photoMarkers = rebuildMarkers(photoMarkers, photoMarkerArray, "EXIFGeotaggerv0._1.BitMap.OpenCameraOrange_16px.png");
                    txtConsole.AppendText("Map zoomed in");
                }
                }
            else
            {
                if (markers != null)
                {
                    markers = rebuildMarkers(markers, gpsMarkerArray, "EXIFGeotaggerv0._1.BitMap.OpenCamera24px.png");
                }
                if (photoMarkers != null)
                {
                    photoMarkers = rebuildMarkers(photoMarkers, photoMarkerArray, "EXIFGeotaggerv0._1.BitMap.OpenCameraOrange_24px.png");
                }
                }
            if (markers != null) {
                gMap.Overlays.Add(markers);
            }
            if (photoMarkers != null)
            {
                gMap.Overlays.Add(photoMarkers);
            }
        }

        #region Events
        private void accessmdbToolStripMenuItem_Click(object sender, EventArgs e)
        {
            connectAccess(sender, e);
        }

        private void gMap_OnMouseMoved(object sender, MouseEventArgs e)
        {
            var point = gMap.FromLocalToLatLng(e.X, e.Y);

            lbPosition.Text = "latitude: " + Math.Round(point.Lat, 6) + " longitude: " + Math.Round(point.Lng, 6);
            //gMap.MouseHover += gMap_OnMouseHoverChanged;
        }

        private void gMap_OnMarkerClick(GMapMarker marker, MouseEventArgs e)
        {
            String id = marker.Tag.ToString();
            myAssembly = Assembly.GetExecutingAssembly();
            //myStream = myAssembly.GetManifestResourceStream(icon);
            //bmpMarker = (Bitmap)Image.FromStream(myStream);
      
            MessageBox.Show(id);
        }

       

        private void menuQuit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Really Quit?", "Exit", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                Application.Exit();
            }
        }

        private void menuRunGeoTag_Click(object sender, EventArgs e)
        {
            browseFolder();
            if (bgWorker1.IsBusy != true)
            {
                // create a new instance of the alert form
                progress = new ProgressForm();
                // event handler for the Cancel button in AlertForm
               progress.Canceled += new EventHandler<EventArgs>(buttonCancel_Click);
                progress.Show();
                // Start the asynchronous operation.
                bgWorker1.RunWorkerAsync();
            }
        }

        private void bgWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            Record r;
            int length = mFiles.Length;
            geoTagCount = 0;
           
            double percent;
            foreach (string filePath in mFiles)
            {
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    Image image = new Bitmap(filePath);
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

                    try
                    {
                        r = mRecordDict[Path.GetFileNameWithoutExtension(filePath)];
                        propItemLat = r.getEXIFCoordinate("latitude", propItemLat);
                        propItemLon = r.getEXIFCoordinate("longitude", propItemLon);
                        propItemAlt = r.getEXIFNumber(propItemAlt, "altitude", 10);
                        propItemLatRef = r.getEXIFCoordinateRef("latitude", propItemLatRef);
                        propItemLonRef = r.getEXIFCoordinateRef("longitude", propItemLonRef);
                        propItemAltRef = r.getEXIFAltitudeRef(propItemAltRef);

                        propItemDir = r.getEXIFNumber(propItemDir, "bearing", 10);
                        propItemVel = r.getEXIFNumber(propItemVel, "velocity", 100);
                        propItemPDop = r.getEXIFNumber(propItemPDop, "pdop", 10);
                        propItemSat = r.getEXIFInt(propItemSat, r.Satellites);

                        propItemDateTime = r.getEXIFDateTime(propItemDateTime);

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

                        image.Save("C:\\androidapp\\GeoRef" + "\\" + Path.GetFileName(filePath));
                        image.Dispose();
                    } catch (KeyNotFoundException ex)
                    {
                        errorCount++;
                    }      
                }
                geoTagCount++;
                percent = ((double)geoTagCount / length) * 100;
                worker.ReportProgress((int)percent);
            }
        }

        // This event handler cancels the backgroundworker, fired from Cancel button in AlertForm.
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (bgWorker1.WorkerSupportsCancellation == true)
            {
                // Cancel the asynchronous operation.
                bgWorker1.CancelAsync();
                // Close the AlertForm
                progress.Close();
            }
        }

        private void bgWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            progress.Message = "Geotagging in n progress, please wait... " + e.ProgressPercentage.ToString() + "% completed";
            progress.ProgressValue = e.ProgressPercentage;
        }

        private void bgWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                string title = "Cancelled";
                string message = "Geotagging cancelled";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result = MessageBox.Show(message, title, buttons);
                if (result == DialogResult.Yes)
                {
                    this.Close();
                }
            }
            else
            {
                if (e.Error != null)
                {
                    string title = "Error";
                    string message = e.Error.ToString();
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    DialogResult result = MessageBox.Show(message, title, buttons);
                    if (result == DialogResult.Yes)
                    {
                        this.Close();
                    }
                }
                else
                {
                    string title = "Finished";
                    string message = "Geotagging complete\n" + (geoTagCount - errorCount) + " of " + mFiles.Length + " photos geotagged";
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    DialogResult result = MessageBox.Show(message, title, buttons);
                    if (result == DialogResult.Yes)
                    {
                        this.Close();
                    }
                }
            }
            progress.Close();
           
        }

        #endregion

        #region HELPERFUNCTIONS
        private void browseFolder()
        {
            using (var browseDialog = new FolderBrowserDialog())
            {
                DialogResult result = browseDialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(browseDialog.SelectedPath))
                {
                    mFiles = Directory.GetFiles(browseDialog.SelectedPath);
                    MessageBox.Show("Files found: " + mFiles.Length.ToString(), "Message");
                }
            }
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
            return coordinate;
        }
        #endregion

    } //end class   
} //end namespace
