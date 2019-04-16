﻿using System;
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
using System.Threading;

namespace EXIFGeotaggerv0._1
{
    public partial class EXIFGeoTagger : Form
    {
        public string mDBPath;
        public string mLayer;
        public Color mlayerColour;
        public String mlayerColourHex;


        GMapOverlay overlay;

        //index of currently layer in checkbox
        private int mSelectedOverlay;
        private Dictionary<string, Record> mRecordDict;
        public string[] mFiles; //array containing absolute paths of photos.
        public string outFolder; //folder path to save geotag photos
        string inFolder; //folder path to read geotag photos

        //FORMS
        private ImportDataForm importForm;
        private ProgressForm progress;

        private int geoTagCount;
        private int errorCount;
        private int stationaryCount;
        private int layerCount;
        private ImageList imageList;
        private List<string> layerList;

        private Assembly myAssembly;
        private Stream myStream;

        private static Bitmap bmpPhoto;
        private GMapOverlay markers;
        private GMapOverlay photoMarkers;
        

        private List<GMapMarker> photoMarkerArray;
        private List<GMapMarker> markerArray;
        private List<EXIFMarker> exifPhotoMarkerArray;
        private List<GMapOverlay> gpsOverlayArray;

        private Dictionary<string, GMapMarker[]> overlayDict;

        private Boolean data = false;

        private Boolean mouseDown = false;
        private PointLatLng topLeft;
        private PointLatLng bottomRight;
        private List<PointLatLng> zoomRect;
        private GMapOverlay zoomOverlay;
        private GMapPolygon rect;

        private ListViewItem layerItem;

        public EXIFGeoTagger()
        {
            InitializeComponent();

            this.menuRunGeoTag.Enabled = true;

        }

        private void gMap_Load(object sender, EventArgs e)
        {
            gMap.MapProvider = GMapProviders.GoogleMap;
            gMap.Position = new PointLatLng(-36.939318, 174.892701);
            gMap.MouseWheelZoomEnabled = true;
            gMap.ShowCenter = false;
            gMap.MaxZoom = 23;
            gMap.MinZoom = 5;
            gMap.Zoom = 10;
            gMap.DragButton = MouseButtons.Left;
            gMap.OnMapZoomChanged += gMap_OnMapZoomChanged;
            gMap.MouseMove += gMap_OnMouseMoved;
            gMap.DragButton = MouseButtons.Right;
            gMap.OnMapZoomChanged += gMap_OnMapZoomChanged;
            gMap.MouseDown += gMap_MouseDown;
            gMap.MouseUp += gMap_MouseUp;
            gMap.MouseMove += gMap_MouseMove;
            //gMap.MouseClick += gMap_MouseClick;
            layerCount = 0;

            overlayDict = new Dictionary<string, GMapMarker[]>();
            //layerItem = new ListViewItem();
            imageList = new ImageList();
        }

        private void fileMenuOpen_Click(object sender, ToolStripItemClickedEventArgs e)
        {
        }

        #region DatabaseConnect


        private void connectAccess_Click(object sender, EventArgs e)
        {
            importForm = new ImportDataForm();
            mRecordDict = new Dictionary<string, Record>();
            importForm.mParent = this;

            importForm.Show();

        }
        public void importAccessData(object sender, EventArgs e)
        {
            importForm.Close();

            string connectionString = string.Format("Provider={0}; Data Source={1}; Jet OLEDB:Engine Type={2}",
                                "Microsoft.Jet.OLEDB.4.0", mDBPath, 5);

            OleDbConnection connection = new OleDbConnection(connectionString);
            string connectionStr = connection.ConnectionString;

            string strSQL = "SELECT * FROM PhotoList"; // WHERE PhotoList.GeoMark = true;";

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
            plotLayer();

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
                r.GeoMark = Convert.ToBoolean(row[12]);
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
        }

        private void ckBoxLayers_SelectedIndexChanged(object sender, EventArgs e)
        {
            //txtConsole.AppendText(ckBoxLayers.Items.IndexOf(ckBoxLayers.SelectedItem).ToString());
            //txtConsole.AppendText(ckBoxLayers.SelectedItem.ToString());
            mSelectedOverlay = ckBoxLayers.Items.IndexOf(ckBoxLayers.SelectedItem);
        }

        private void listLayers_SelectedIndexChanged(object sender, EventArgs e)
        {
            //mSelectedOverlay = listLayers.Items.IndexOf(listLayers.SelectedItems);
        }

        private void ckBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            overlay = gMap.Overlays.ElementAt(mSelectedOverlay);
            if (overlay.IsVisibile == false)
            {
                overlay.IsVisibile = true;
            }
            else
            {
                overlay.IsVisibile = false;
            }

        }

        private void plotLayer()
        {
            txtConsole.Clear();
            txtConsole.AppendText("Colour: " + mlayerColourHex.ToString());
            txtConsole.AppendText("ColourName: " + mlayerColour.ToString());
            GMapOverlay newOverlay = new GMapOverlay(mLayer);

            //string icon = ColorTable.ColorTableDict[mlayerColourHex];

            MarkerTag tag = new MarkerTag(mlayerColourHex, 4);
            tag.setBitmap();
            newOverlay = buildMarker(newOverlay, tag, mLayer);

            gMap.Overlays.Add(newOverlay);
            GMapMarker[] markers = newOverlay.Markers.ToArray<GMapMarker>();

            overlayDict.Add(newOverlay.Id, markers);
            overlay = newOverlay;

            ckBoxLayers.Items.Add(overlay.Id, true);

            //Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ColorTable.ColorTableDict[mlayerColourHex.ToString()] + "_24px.png");
            //Bitmap bitmap = (Bitmap)Image.FromStream(stream);
            //imageList.Images.Add(bitmap);

            //ListViewItem layerItem = new ListViewItem(overlay.Id, layerCount);
            //layerCount++;
            
            ////layerItem.Text = overlay.Id;
            //layerItem.Checked = true;
            
            //listLayers.SmallImageList = imageList;
            //listLayers.Items.Add(layerItem);
            
            newOverlay.IsVisibile = true;
            overlay.IsVisibile = true;
        }


        private GMapOverlay buildMarker(GMapOverlay overlay, MarkerTag tag, String name)
        {
            Bitmap bitmap = tag.getBitmap();
            if (mRecordDict != null)
            {
                foreach (KeyValuePair<string, Record> record in mRecordDict)
                {
                    Double lat = record.Value.Latitude;
                    Double lon = record.Value.Longitude;

                    GMapMarker marker = new GMarkerGoogle(new PointLatLng(lat, lon), bitmap);
                    marker.Tag = tag;
                    overlay.Markers.Add(marker);
                    //markerArray.Add(marker);
                }
            }
            //bitmap.Dispose();
            return overlay;
        }

        private void rebuildMarkers(GMapOverlay overlay, int size)
        {
            overlay.Markers.Clear();
            GMapMarker[] markers = overlayDict[overlay.Id];


            int count = markers.Length;
            MarkerTag tag = (MarkerTag)markers[0].Tag;
            tag.Size = size;

            int step = getStep(size);
            //tag.getBitmap().Dispose();
            tag.setBitmap();
            Bitmap bitmap = tag.getBitmap();

            for (int i = 0; i < count - 1; i += step)
            {
                GMapMarker marker = markers[i];
                GMapMarker newMarker = new GMarkerGoogle(marker.Position, bitmap);
                if (marker.Tag != null)
                {
                    newMarker.Tag = marker.Tag;
                }
                overlay.Markers.Add(newMarker);
            }
        }

        private void photosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            browseFolder();
            GMapOverlay photoOverlay = new GMapOverlay("photos");
            //Assembly assembly = Assembly.GetExecutingAssembly();
            //Stream stream = assembly.GetManifestResourceStream(icon);
            //bmpPhoto = (Bitmap)Image.FromStream(stream);
            MarkerTag tag = new MarkerTag("ffff80ff", 4);
            tag.setBitmap();
            photoOverlay = buildPhotoMarker(photoOverlay, tag, "photos");
            gMap.Overlays.Add(photoOverlay);
            GMapMarker[] markers = photoOverlay.Markers.ToArray<GMapMarker>();

            overlayDict.Add(photoOverlay.Id, markers);
            overlay = photoOverlay;

            ckBoxLayers.Items.Add(overlay.Id, true);
            photoOverlay.IsVisibile = true;
            overlay.IsVisibile = true;

        }

        private GMapOverlay buildPhotoMarker(GMapOverlay overlay, MarkerTag tag, String name)
        {

            Bitmap bitmap = tag.getBitmap();
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
                    GMapMarker marker = new GMarkerGoogle(new PointLatLng(latitude, longitude), bitmap);
                    marker.Tag = tag;
                    overlay.Markers.Add(marker);
 
                } catch (ArgumentException ex) {

                }
                catch (NullReferenceException ex)
                {
                    txtConsole.AppendText(ex.StackTrace);
                }
            }
            txtConsole.AppendText("Photos ready.." + Environment.NewLine);
            return overlay;
        }

        #endregion

        private void gMap_OnMapZoomChanged()
        {
            txtConsole.Clear();
            txtConsole.AppendText(gMap.Zoom.ToString());
            GMapOverlay[] overlays = gMap.Overlays.ToArray<GMapOverlay>();
            if ((int)gMap.Zoom < 11)
            {
                foreach (GMapOverlay overlay in overlays)
                {
                    rebuildMarkers(overlay, 4);

                }
            }
            else if ((int)gMap.Zoom < 15 && (int)gMap.Zoom >= 11)
            {
                foreach (GMapOverlay overlay in overlays)
                {
                    rebuildMarkers(overlay, 8);
                }
            }
            else if ((int)gMap.Zoom < 18 && (int)gMap.Zoom >= 15)
            {
                foreach (GMapOverlay overlay in overlays)
                {
                    rebuildMarkers(overlay, 12);
                }
            }
            else if ((int)gMap.Zoom < 20 && (int)gMap.Zoom >= 18)
            {
                foreach (GMapOverlay overlay in overlays)
                {

                    rebuildMarkers(overlay, 16);
                }
            }
            else
            {
                foreach (GMapOverlay overlay in overlays)
                {
                    rebuildMarkers(overlay, 24);
                }

            }
        }

        #region Events
        //private void accessmdbToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    connectAccess_Click(sender, e);
        //    GMapOverlay overlay = new GMapOverlay("markers");

        //    //gpsMarkerArray = new List<GMapMarker>();
        //    overlay = buildMarker(overlay, "EXIFGeotaggerv0._1.BitMap.OpenCamera8px.png", "markers");
        //    gMap.Overlays.Add(overlay);

        //    ckBoxLayers.Items.Add(markers.Id, false);
        //    overlay.IsVisibile = true;
        //    this.menuRunGeoTag.Enabled = true;

        //}

        private void gMap_MouseDown(object sender, MouseEventArgs e)
        {
            zoomOverlay = new GMapOverlay("zoom");
            mouseDown = true;
            topLeft = gMap.FromLocalToLatLng(e.X, e.Y);
            zoomRect = new List<PointLatLng>();
            var point = gMap.FromLocalToLatLng(e.X, e.Y);
            txtConsole.Clear();
            txtConsole.AppendText("latitude: " + Math.Round(point.Lat, 6) + " longitude: " + Math.Round(point.Lng, 6));
        }

        private void gMap_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown == true)
            {
                zoomRect.Clear();

                bottomRight = gMap.FromLocalToLatLng(e.X, e.Y);
                zoomRect.Add(topLeft);

                PointLatLng topRight = new PointLatLng(topLeft.Lat, bottomRight.Lng);
                PointLatLng bottomLeft = new PointLatLng(bottomRight.Lat, topLeft.Lng);
                zoomRect.Add(topRight);
                zoomRect.Add(bottomRight);
                zoomRect.Add(bottomLeft);

                if (rect != null)
                {
                    gMap.Overlays.Remove(zoomOverlay);
                    zoomOverlay.Polygons.Remove(rect);
                }
                rect = new GMapPolygon(zoomRect, "zoom");
                rect.Fill = new SolidBrush(Color.FromArgb(20, Color.Black));
                rect.Stroke = new Pen(Color.DarkGray, 1);
                zoomOverlay.Polygons.Add(rect);
                gMap.Overlays.Add(zoomOverlay);
            }
            var point = gMap.FromLocalToLatLng(e.X, e.Y);
            //txtConsole.AppendText("latitude: " + Math.Round(point.Lat, 6) + " longitude: " + Math.Round(point.Lng, 6));
        }

        private void gMap_MouseUp(object sender, MouseEventArgs e)
        {
            zoomOverlay.Polygons.Remove(rect);

            gMap.Overlays.Remove(zoomOverlay);
            mouseDown = false;
            bottomRight = gMap.FromLocalToLatLng(e.X, e.Y);
            var point = gMap.FromLocalToLatLng(e.X, e.Y);
            txtConsole.Clear();
            txtConsole.AppendText("latitude: " + Math.Round(point.Lat, 6) + " longitude: " + Math.Round(point.Lng, 6));
            gMap.SetZoomToFitRect(polygonToRect(zoomRect));
            zoomRect.Clear();
        }

        //private void gMap_MouseClick(object sender, MouseEventArgs e)
        //{
        //    if (mouseDown == true)
        //    {
        //        if (rect != null)
        //        {
        //            zoomRect.Clear();
        //            gMap.Overlays.Remove(zoomOverlay);
        //            zoomOverlay.Polygons.Remove(rect);
        //        }
                           
        //    }
        //}

        private RectLatLng polygonToRect(List<PointLatLng> points)
        {
            double lat = (points[0].Lat + points[3].Lat) / 2;
            double lon = (points[0].Lng + points[3].Lng) / 2;

            double width = Math.Abs(points[1].Lng) - Math.Abs(points[0].Lng);
            double height = Math.Abs(points[3].Lat) - Math.Abs(points[0].Lat);
            RectLatLng rect = new RectLatLng(lat, lon, width, height);
            return rect;
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
            GeotagForm geotagForm = new GeotagForm();
            geotagForm.mParent = this;
            geotagForm.Show();
        }

        public void startWorker(object sender, EventArgs e)
        {
            if (bgWorker1.IsBusy != true)
            {
                // create a new instance of the alert form
                progress = new ProgressForm();
                // event handler for the Cancel button in AlertForm
                progress.Canceled += new EventHandler<EventArgs>(buttonCancel_Click);
                progress.Show();
                progress.BringToFront();
                // Start the asynchronous operation.
                bgWorker1.RunWorkerAsync();
            }
        }

        private void bgWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            Record r;
            //GMapOverlay photoOverlay = new GMapOverlay("photos");
            int length = mFiles.Length;
            geoTagCount = 0;
            errorCount = 0;
            stationaryCount = 0;
   
            //Bitmap photoIcon = new Bitmap("EXIFGeotaggerv0._1.BitMap.OpenCamera8px.png");
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
                    Bitmap image = new Bitmap(filePath);
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

                        if (r.GeoMark)
                        {

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

                            r = null;
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
                            image.Save(outFolder + "\\" + Path.GetFileName(filePath));
                            image.Dispose();
                            image = null;
                        } else
                        {
                            stationaryCount++;
                        }
                    } catch (KeyNotFoundException ex)
                    {
                        errorCount++;
                    }
                }
                //photoIcon.Dispose();
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
                this.BringToFront();
                progress.Close();
            }
        }

        private void bgWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progress.Message = "Geotagging in progress, please wait... " + e.ProgressPercentage.ToString() + "% completed";
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
                    string message = "Geotagging complete\n" + (geoTagCount - errorCount) + " of " + mFiles.Length + " photos geotagged\n" 
                        + "Photos with no geomark: " + stationaryCount + "\n" + "Photos with no gps point: " + errorCount + "\n";
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

        private int getStep(int size)
        {
            if (size == 4)
            {
                return 20;
            }
            else if (size == 8)
            {
                return 10;
            }
            else if (size == 12)
            {
                return 2;
            }
            else
            {
                return 1;
            }
        }
        #endregion

        private void menuSave_Click(object sender, EventArgs e)
        {

        }



        private void EXIFGeoTagger_Load(object sender, EventArgs e)
        {

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 AboutBox = new AboutBox1();
            AboutBox.Show();
        }

        
    } //end class   
} //end namespace
