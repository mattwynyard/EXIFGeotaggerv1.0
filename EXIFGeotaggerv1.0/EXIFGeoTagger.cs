using System;
using System.IO;
using System.Collections;
using System.Windows.Forms;
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

using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System.Threading;
using Amazon;
using System.Net;
using System.Collections.Concurrent;

public delegate Image getAWSImage();

namespace EXIFGeotagger //v0._1
{
    public partial class EXIFGeoTagger : Form
    {

        public delegate Task<GMapOverlay> ReadGeoTagDelegate(string folderPath, string layer, string color);

        private string connectionString;

        private Image image; //photo in photo viewer
        public string mDBPath;
        public string mLayer; //imported layer
        public Color mlayerColour;
        public String mlayerColourHex;
        private int id;

        private OleDbConnection connection;
        private Dictionary<string, GMapMarker[]> mOverlayDict;
        private GMapOverlay mOverlay; //the currently active overlay
        private GMapOverlay selectedMarkersOverlay; //overlay containing selected markers
        private GMapMarker currentMarker; //the current marker selected by user
        private GMapOverlay mSelectedOverlay;
        //index of currently layer in checkbox
        private int mSelectedOverlayIndex;
        private Dictionary<string, Record> mRecordDict;
        private static readonly Object obj = new Object();
        private  Dictionary<string, Record> mNewRecordDict;
        private LayerAttributes mLayerAttributes;
        public string[] mFiles; //array containing absolute paths of photos.
        public string outFolder; //folder path to save geotag photos

        public Boolean allRecords;
        private int importLength;
        //Tools
        private Boolean mZoom = false;
        private Boolean mArrow = true;
        //FORMS
        private ProgressForm progress;

        private int geoTagCount;
        private int errorCount;
        private int stationaryCount;
        private int layerCount;
        private ImageList imageList;

        private Boolean mouseDown = false;
        private PointLatLng topLeft;
        private PointLatLng bottomRight;
        private List<PointLatLng> zoomRect;
        private GMapOverlay zoomOverlay; //overlay containing zoom rectangle
        private GMapPolygon rect;

        private static double min_lat;
        private static double min_lng;
        private static double max_lat;
        private static double max_lng;

        private Boolean mouseInBounds;
        private CancellationTokenSource _cts;
        private BlockingCollection<string> fileQueue;
        private int mQueueSize;
        private string mOutPath;
        private ProgressForm progressForm;
        private CountdownEvent countdownEvent;


        /// <summary>
        /// Class constructor to intialize form
        /// </summary>
        public EXIFGeoTagger()
        {
            InitializeComponent();
            //awsClient = new AWSConnection();
            this.menuRunGeoTag.Enabled = true;
            
        }

        /// <summary>
        /// Initial method called to setup map paramenters and register events.
        /// Also intialises overaly dictionary.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gMap_Load(object sender, EventArgs e)
        {
            gMap.MapProvider = GMapProviders.GoogleMap;
            gMap.Position = new PointLatLng(-36.939318, 174.892701);
            gMap.MouseWheelZoomEnabled = true;
            gMap.ShowCenter = false;
            gMap.MaxZoom = 23;
            gMap.MinZoom = 5;
            gMap.Zoom = 10;
            //gMap.DragButton = MouseButtons.Left;
            gMap.OnMapZoomChanged += gMap_OnMapZoomChanged;
            gMap.MouseMove += gMap_OnMouseMoved;
            gMap.DragButton = MouseButtons.Right;
            gMap.OnMapZoomChanged += gMap_OnMapZoomChanged;
            gMap.MouseDown += gMap_MouseDown;
            gMap.MouseUp += gMap_MouseUp;
            gMap.MouseMove += gMap_MouseMove;
            gMap.MouseClick += gMap_MouseClick;
            gMap.MapScaleInfoEnabled = true;
            gMap.PreviewKeyDown += gMap_KeyDown;
            gMap.Enter += gMap_onEnter;
            gMap.OnMarkerDoubleClick += gMap_onMarkerDoubleClick;
            gMap.Leave += gMap_onLeave;
            mOverlayDict = new Dictionary<string, GMapMarker[]>();
            //layerItem = new ListViewItem();
            imageList = new ImageList();
            layerCount = 0;
            selectedMarkersOverlay = new GMapOverlay("selected");
            //gMap.Overlays.Add(selectedMarkersOverlay);
            //mOverlayDict.Add("selected", null);
        }

        

        /// <summary>
        /// Creates an import data form to get the path of the access database selected by the user
        /// Intialiases the record dictionary which will eventually recieve the data from access.
        /// </summary>
        /// <param name="sender - the connect access option clicked on the menu tab"></param>
        /// <param name="e"></param>
        private void connectAccess_Click(object sender, EventArgs e)
        {
            ImportDataForm importForm = new ImportDataForm("access");
            mRecordDict = new Dictionary<string, Record>();
            importForm.mParent = this;
            importForm.Show();
        }

        ///// <summary>
        ///// Intialises a new Record and adds data extracted from access to each relevant field.
        ///// The record is then added to the Record Dictionary.
        ///// </summary>
        ///// <param name="i: the number of records read"></param>
        ///// <param name="row: the access record"></param>
        //private void buildDictionary(int i, Object[] row)
        //{
        //   try
        //    {
        //        Record r = new Record((string)row[1]);
        //        int id = (int)row[0];
        //        r.Id = id.ToString();
        //        r.Latitude = (double)row[3];
        //        r.Longitude = (double)row[4];
        //        r.Altitude = (double)row[5];
        //        r.Bearing = Convert.ToDouble(row[6]);
        //        r.Velocity = Convert.ToDouble(row[7]);
        //        r.Satellites = Convert.ToInt32(row[8]);
        //        r.PDop = Convert.ToDouble(row[9]);
        //        r.Inspector = Convert.ToString(row[10]);
        //        r.TimeStamp = Convert.ToDateTime(row[12]);
        //        r.GeoMark = Convert.ToBoolean(row[13]);
        //        r.Side = Convert.ToString(row[19]);
        //        r.Road = Convert.ToInt32(row[20]);
        //        r.Carriageway = Convert.ToInt32(row[21]);
        //        r.ERP = Convert.ToInt32(row[22]);
        //        r.FaultID = Convert.ToInt32(row[23]);

        //        mRecordDict.Add(r.PhotoName, r);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.StackTrace);
        //    }
        //}

        #region Plotting
        /// <summary>
        /// Called from <see cref="importAccessData(object sender, EventArgs e)"/> creates new overaly and adds it to overlays in map control
        /// Intialises new MarkerTag which sets colour and inital size of the icon and sets the bitmap for the marker
        /// </summary>
        private void plotLayer(string layer, string color)
        {
            GMapOverlay newOverlay = new GMapOverlay(layer);
            newOverlay = buildMarkers(newOverlay, color);
            gMap.Overlays.Add(newOverlay);
            GMapMarker[] markers = newOverlay.Markers.ToArray<GMapMarker>();
            mOverlayDict.Add(newOverlay.Id, markers);
            mOverlay = newOverlay;

            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ColorTable.ColorTableDict[color.ToString()] + "_24px.png");
            Bitmap bitmap = (Bitmap)Image.FromStream(stream);
            imageList.Images.Add(bitmap);

            ListViewItem layerItem = new ListViewItem(newOverlay.Id, layerCount);
            layerCount++;

            layerItem.Text = newOverlay.Id;
            layerItem.Checked = true;

            listLayers.SmallImageList = imageList;
            listLayers.Items.Add(layerItem);

            newOverlay.IsVisibile = true;
            mOverlay.IsVisibile = true;
            zoomToMarkers();
        }

        /// <summary>
        /// Called when layer is intially bulit when importing raw data (database/files etc)
        /// Iterates through the record dictionary and assigns a marker tag to each record which contains information 
        /// about icon size, color etc. Then creates a new google marker adds bitmap, coordinate information and marker tag.
        /// </summary>
        /// <param name="overlay - the intial overlay that markers will be added to"></param>
        /// <returns>the GPOverlay containing the markers</returns>
        private GMapOverlay buildMarkers(GMapOverlay overlay, string color)
        {
            if (mRecordDict != null)
            {
                int id = 0;
                Bitmap bitmap = ColorTable.getBitmap(color, 4);
                foreach (KeyValuePair<string, Record> record in mRecordDict)
                {
                    MarkerTag tag = new MarkerTag(color, id);
                    tag.PhotoName = record.Value.PhotoName;
                    tag.Path = record.Value.Path;
                    tag.Size = 4;
                    tag.PhotoName = record.Key;
                    tag.Record = record.Value;
                    Double lat = record.Value.Latitude;
                    Double lon = record.Value.Longitude;
                    GMapMarker marker = new GMarkerGoogle(new PointLatLng(lat, lon), bitmap);
                    marker.Tag = tag;
                    marker = setToolTip(marker);
                    overlay.Markers.Add(marker);
                    id++;
                }
            }
            overlay.IsVisibile = true;
            return overlay;
        }

        private GMapMarker setToolTip(GMapMarker marker)
        {
            MarkerTag tag = (MarkerTag)marker.Tag;
            
            marker.ToolTipText = '\n' + tag.ToString();
            marker.ToolTip.Fill = Brushes.White;
            marker.ToolTip.Foreground = Brushes.Black;
            marker.ToolTip.Stroke = Pens.Black;
            //marker.ToolTip.TextPadding = new Size(20, 20);
            marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
            return marker;
        }

        private void rebuildMarkers(GMapOverlay overlay, int size)
        {
            overlay.Markers.Clear();
            try
            {
                GMapMarker[] markers = mOverlayDict[overlay.Id];
                MarkerTag tag = (MarkerTag)markers[0].Tag;
                if (tag == null)
                {
                    Bitmap redBitmap = ColorTable.getBitmap("Red", size);
                    GMapMarker[] redMarkers = mOverlayDict["selected"];
                    int redCount = redMarkers.Length;
                    for (int j = 0; j < redCount; j += 1)
                    {
                        GMapMarker newMarker = new GMarkerGoogle(redMarkers[j].Position, redBitmap);
                        overlay.Markers.Add(newMarker);
                    }
                }
                else
                {
                    markers = mOverlayDict[overlay.Id];
                    int count = markers.Length;
                    int step = getStep(size);
                    Bitmap bitmap = ColorTable.getBitmap(tag.Color, size);
                    for (int i = 0; i < count; i += step)
                    {
                        tag = (MarkerTag)markers[i].Tag;
                        tag.Size = size;
                        GMapMarker newMarker = new GMarkerGoogle(markers[i].Position, bitmap);

                        if (markers[i].Tag != null)
                        {
                            newMarker.Tag = markers[i].Tag;
                        }
                        //setToolTip(newMarker);
                        overlay.Markers.Add(newMarker);
                    }
                }
            } catch (KeyNotFoundException ex)
            {

            }
        }

        private void refreshUI(GMapOverlay overlay, string color)
        {
            gMap.Overlays.Add(overlay);
            GMapMarker[] markers = overlay.Markers.ToArray<GMapMarker>();

            mOverlayDict.Add(overlay.Id, markers);
            mOverlay = overlay;

            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ColorTable.ColorTableDict[color] + "_24px.png");
            Bitmap bitmap = (Bitmap)Image.FromStream(stream);
            imageList.Images.Add(bitmap);

            ListViewItem layerItem = new ListViewItem(overlay.Id, layerCount);
            layerCount++;

            layerItem.Text = overlay.Id;
            layerItem.Checked = true;

            listLayers.SmallImageList = imageList;
            listLayers.Items.Add(layerItem);

            overlay.IsVisibile = true;
            mOverlay.IsVisibile = true;
            zoomToMarkers();


        }

        private void zoomToMarkers()
        {
            zoomRect = new List<PointLatLng>();

            PointLatLng topLeft = new PointLatLng(max_lat, min_lng);
            PointLatLng topRight = new PointLatLng(max_lat, max_lng);
            PointLatLng bottomRight = new PointLatLng(min_lat, max_lng);
            PointLatLng bottomLeft = new PointLatLng(min_lat, min_lng);
            zoomRect.Add(topLeft);
            zoomRect.Add(topRight);
            zoomRect.Add(bottomRight);
            zoomRect.Add(bottomLeft);
            gMap.SetZoomToFitRect(polygonToRect(zoomRect));
            zoomRect.Clear();
        }

        #endregion

        #region GMap Events

        private void gMap_OnMapZoomChanged()
        {
            txtConsole.Clear();
            txtConsole.AppendText(gMap.Zoom.ToString());
            double metersPerPx = 156543.03392 * Math.Cos(gMap.ViewArea.LocationMiddle.Lat * Math.PI / 180) / Math.Pow(2, gMap.Zoom);
            //txtConsole.AppendText("Scale: " + metersPerPx + Environment.NewLine);
            double scale = metersPerPx * (96 / 0.0254);
            txtConsole.AppendText("Scale 1:" + scale + "m" + Environment.NewLine);

            lbScale.Text = "Scale 1: " + Math.Round(scale);

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
                    rebuildMarkers(overlay, 20);
                }

            }
        }

        private void gMap_KeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                    gMap.Offset(0, -10);
                    break;
                case Keys.Up:
                    gMap.Offset(0, 10);
                    break;
                case Keys.Right:
                    gMap.Offset(-10, 0);
                    break;
                case Keys.Left:
                    gMap.Offset(10, 0);
                    break;
                default:
                    break;
            }
        }

        private void gMap_MouseDown(object sender, MouseEventArgs e)
        {
            if (mZoom)
            {
                mouseDown = true;
                zoomOverlay = new GMapOverlay("zoom");

                topLeft = gMap.FromLocalToLatLng(e.X, e.Y);
                zoomRect = new List<PointLatLng>();
                var point = gMap.FromLocalToLatLng(e.X, e.Y);
                txtConsole.Clear();
                txtConsole.AppendText("latitude: " + Math.Round(point.Lat, 6) + " longitude: " + Math.Round(point.Lng, 6));
            }
            
        }

        private void gMap_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown && mZoom)
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
                    zoomOverlay.Polygons.Remove(rect);
                    gMap.Overlays.Remove(zoomOverlay);

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
            if (mouseDown && mZoom)
            {
                zoomOverlay.Polygons.Remove(rect);
                gMap.Overlays.Remove(zoomOverlay);
                bottomRight = gMap.FromLocalToLatLng(e.X, e.Y);
                var point = gMap.FromLocalToLatLng(e.X, e.Y);
                txtConsole.Clear();
                txtConsole.AppendText("latitude: " + Math.Round(point.Lat, 6) + " longitude: " + Math.Round(point.Lng, 6));
                if (zoomRect.Count == 4)
                {
                    gMap.SetZoomToFitRect(polygonToRect(zoomRect));
                }
                zoomRect.Clear();
            }
            mouseDown = false;

        }

        /// <summary>
        /// Clears marker selection from map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gMap_MouseClick(object sender, MouseEventArgs e)
        {

            if (mouseInBounds && e.Button == MouseButtons.Left)
            {
                if (currentMarker != null)
                {
                    selectedMarkersOverlay.Clear();
                    MarkerTag currentTag = (MarkerTag)currentMarker.Tag;
                    currentTag.IsSelected = false;
                    mOverlayDict.Remove("selected");
                    currentMarker = null;
                }
            }
        }

        private void gMap_OnMouseMoved(object sender, MouseEventArgs e)
        {
            var point = gMap.FromLocalToLatLng(e.X, e.Y);

            lbPosition.Text = "latitude: " + Math.Round(point.Lat, 6) + " longitude: " + Math.Round(point.Lng, 6);
            //gMap.MouseHover += gMap_OnMouseHoverChanged;
        }

        private void gMap_onMarkerDoubleClick(GMapMarker marker, MouseEventArgs e)
        {
            //marker.
        }

        private void gMap_OnMarkerClick(GMapMarker marker, MouseEventArgs e)
        {
            MarkerTag tag = (MarkerTag)marker.Tag;
            if (currentMarker == null)
            {
                currentMarker = marker;
                MarkerTag currentTag = (MarkerTag)currentMarker.Tag;
                currentTag.IsSelected = true;
                getPicture(currentMarker);
            }
            else
            {
                if (tag == null)
                {
                    selectedMarkersOverlay.Clear();
                    mOverlayDict.Remove("selected");
                }
                else
                {
                    if (tag.IsSelected == true) //deselects marker
                    {
                        selectedMarkersOverlay.Clear();
                        mOverlayDict.Remove("selected");
                        tag.IsSelected = false;
                        currentMarker = null;
                    }
                    else
                    {
                        selectedMarkersOverlay.Clear(); //selects a new marker
                        mOverlayDict.Remove("selected");
                        currentMarker = marker;
                        MarkerTag currentTag = (MarkerTag)currentMarker.Tag;
                        currentTag.IsSelected = true;
                        getPicture(currentMarker);
                    }
                }
            }
        }

        private void getPicture(GMapMarker marker)
        {
            MarkerTag tag = (MarkerTag)marker.Tag;
            Bitmap bitmap = ColorTable.getBitmap("Red", tag.Size);
            GMapMarker newMarker = new GMarkerGoogle(marker.Position, bitmap);
            newMarker.LocalPosition = marker.LocalPosition;
            newMarker.Offset = marker.Offset;
            selectedMarkersOverlay.Markers.Add(newMarker);
            gMap.Overlays.Add(selectedMarkersOverlay);
            GMapMarker[] selectedMarkers = selectedMarkersOverlay.Markers.ToArray<GMapMarker>();
            mOverlayDict.Add("selected", selectedMarkers);

            if (tag.Path != null)
            {
                try
                {
                    if (pictureBox.Image != null)
                    {
                        pictureBox.Image.Dispose();
                    }
                    lbPhoto.Text = tag.PhotoName;
                    pictureBox.Image = Image.FromFile(tag.Path);
                }
                catch (FileNotFoundException ex)
                {
                    lbPhoto.Text = ex.Message;
                }
            }
            else
            {
                string bucket = "central-waikato";
                string url = "https://centralwaikato2019.s3.ap-southeast-2.amazonaws.com/" + tag.PhotoName + ".jpg";
                var buffer = new byte[1024 * 8]; // 8k buffer.
                MemoryStream data = new MemoryStream();
                int offset = 0;
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(url);
                    var response = request.GetResponse();
                    int bytesRead = 0;
                    using (var responseStream = response.GetResponseStream())
                    {
                        while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            data.Write(buffer, 0, bytesRead);
                            offset += bytesRead;
                        }
                    }
                    image = Image.FromStream(data);
                    data.Close();
                    lbPhoto.Text = tag.ToString();
                    pictureBox.Image = image;
                }
                catch (WebException ex)
                {
                    lbPhoto.Text = ex.Message;
                }
            }
        }


        private void gMap_onEnter(object sender, EventArgs e)
        {
            txtConsole.Clear();
            txtConsole.AppendText("Enter");
            mouseInBounds = true;
            if (mZoom)
            {
                Cursor = Cursors.Cross;
                Cursor.Show();
            }
            else if (mArrow)
            {
                Cursor = Cursors.Arrow;
                Cursor.Show();
            }
        }

        private void gMap_onLeave(object sender, EventArgs e)
        {
            txtConsole.Clear();
            txtConsole.AppendText("Leave");
            Cursor = Cursors.Arrow;
            mouseInBounds = false;
        }

        private void btnZoom_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.Cross;
            mZoom = true;
            mArrow = false;
        }

        private void btnArrow_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.Arrow;
            mZoom = false;
            mArrow = true;
        }

        private void lbScale_Click(object sender, EventArgs e)
        {

        }

       


        #endregion

        #region Form Events

        private void PhotosToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ImportDataForm importForm = new ImportDataForm("photos");
            mRecordDict = new Dictionary<string, Record>();
            importForm.mParent = this;
            importForm.Show();
            importForm.importData += readGeoTagCallback;
        }


        private void fileMenuOpen_Click(object sender, ToolStripItemClickedEventArgs e)
        {
        }

        private void markersMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void listLayers_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListView ls = sender as ListView;
            if (ls.FocusedItem.Checked) {
                string overlayName = ls.FocusedItem.Text;
                //GMapOverlay overlay = gMap.Overlays
            }
        }

        private void listLayers_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            mOverlay = gMap.Overlays.ElementAt(e.Index);
            if (mOverlay.IsVisibile == false)
            {
                mOverlay.IsVisibile = true;
            }
            else
            {
                mOverlay.IsVisibile = false;
            }
        }

        private void menuQuit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Really Quit?", "Exit", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                Application.Exit();
            }
        }

        private void menuSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "EXIF Data|*.exf";
            saveDialog.Title = "Save an EXIF data File";
            DialogResult result = saveDialog.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(saveDialog.FileName))
            {
                string inPath = saveDialog.FileName;
                Serializer s = new Serializer(mLayerAttributes);
                int saveResult = s.serialize(inPath);
                if (saveResult == 1)
                {
                    string message = "File " + inPath + "\n\nSaved succesfully";
                    MsgBox("File Saved", message, MessageBoxButtons.OK);
                } else
                {
                    MsgBox("Save Error", "File failed to save" , MessageBoxButtons.OK);
                }
            }
        }

        private void EXIFGeoTagger_Load(object sender, EventArgs e)
        {

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 AboutBox = new AboutBox1();
            AboutBox.Show();
        }

        private void dataFiledatToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
        }

        private void eXIFDataFiledatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportDataForm importForm = new ImportDataForm("exf");
            mRecordDict = new Dictionary<string, Record>();
            importForm.mParent = this;
            importForm.Show();
            importForm.importData += exfImportCallback;

        }

        #endregion

        #region Callbacks
        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="layer"></param>
        /// <param name="color"></param>
        

        public void exfImportCallback(string folderPath, string layer, string color)
        {
            Serializer s = new Serializer(folderPath);
            mLayerAttributes = s.deserialize();
            mRecordDict = mLayerAttributes.Data;
            max_lat = mLayerAttributes.MaxLat;
            min_lat = mLayerAttributes.MinLat;
            max_lng = mLayerAttributes.MaxLng;
            min_lng = mLayerAttributes.MinLng;
            plotLayer(layer, color);
        }

        private void menuRunGeoTag_Click(object sender, EventArgs e)
        {
            GeotagForm geotagForm = new GeotagForm();
            geotagForm.mParent = this;
            geotagForm.Show();
            geotagForm.writeGeoTag += writeGeoTagCallback;
        }

        public async void writeGeoTagCallback(string dbPath, string inPath, string outPath, string layer, string color, Boolean allRecords)
        {
            ThreadUtil t = new ThreadUtil();
            connectionString = string.Format("Provider={0}; Data Source={1}; Jet OLEDB:Engine Type={2}", "Microsoft.Jet.OLEDB.4.0", dbPath, 5);
            connection = new OleDbConnection(connectionString);
            connection.Open();
            resetMinMax();
            t.setMinMax += setMinMax;
            fileQueue = await t.buildQueue(inPath);
            mRecordDict = await t.readFromDatabase(dbPath, allRecords);
            
            mRecordDict = await t.writeGeoTag(mRecordDict, fileQueue, inPath, outPath);
            //await geoTagComplete(layer, color);
            setLayerAttributes();
            zoomToMarkers();
            plotLayer(layer, color);
            connection.Close();
        }

        public async void readGeoTagCallback(string inPath, string layer, string color)
        {
            ThreadUtil t = new ThreadUtil();
            t.setMinMax += setMinMax;
            t.addRecord += addRecord;
            t.geoTagComplete += geoTagComplete;
            fileQueue = await t.buildQueue(inPath);
            GMapOverlay overlay = new GMapOverlay(layer);
            resetMinMax();
            overlay = await t.readGeoTag(fileQueue, inPath, layer, color);
            
            setLayerAttributes();
            zoomToMarkers();
            refreshUI(overlay, color);
        }

        public async void geoTagComplete(int geotagCount, int stationaryCount, int errorCount)
        {
            await Task.Run(() =>
            {
                string title = "Finished";
                string message = "Geotagging complete\n" + geotagCount + " of " + "fix" + " photos geotagged\n"
                    + "Photos with no geomark: " + stationaryCount + "\n" + "Photos with no gps point: " + errorCount + "\n";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result = MessageBox.Show(message, title, buttons);
                if (result == DialogResult.Yes)
                {
                    Close();
                }
            });
        }

        #endregion

        #region Threading
        private void cancelImport(object sender, EventArgs e)
        {
            if (_cts != null)
                _cts.Cancel();
        }

       

        private void MsgBox(string title, string message, MessageBoxButtons buttons)
        {
            DialogResult result = MessageBox.Show(message, title, buttons);
            if (result == DialogResult.Yes)
            {
                Close();
            }
        }

        

        

        #endregion

        #region HELPERFUNCTIONS

        private void setMinMax(double lat, double lng)
        {
            int maxLat = lat.CompareTo(max_lat);
            int minLat = lat.CompareTo(min_lat);
            int maxLng = lng.CompareTo(max_lng);
            int minLng = lng.CompareTo(min_lng);

            if (maxLat > 0)
                max_lat = lat;
            if (minLat < 0)
                min_lat = lat;
            if (maxLng > 0)
                max_lng = lng;
            if (minLng < 0)
                min_lng = lng;
        }

        private void addRecord(string photo, Record record)
        {
            mRecordDict.Add(photo, record);
        }
        private void setLayerAttributes() {
            mLayerAttributes = new LayerAttributes();
            mLayerAttributes.Data = mRecordDict;
            mLayerAttributes.setMinMax(max_lat, min_lat, max_lng, min_lng);
        }

        private static void resetMinMax()
        {
            min_lat = 90;
            max_lat = -90;
            min_lng = 180;
            max_lng = -180;
        }

        private RectLatLng polygonToRect(List<PointLatLng> points)
        {
            double lat = (points[0].Lat + points[2].Lat) / 2;
            double lon = (points[0].Lng + points[2].Lng) / 2;
            if (points[0].Lat > points[3].Lat)
            {
                if (points[0].Lng < points[1].Lng) //top left -> bottom right
                {
                    lat = lat - (lat - points[0].Lat);
                    lon = lon - (lon - points[3].Lng);
                }
                else //top right -> bottom left
                {
                    lat = lat + (lat - points[3].Lat);
                    lon = lon + (lon - points[0].Lng);
                }
            }
            else
            {
                if (points[0].Lng > points[1].Lng) //bottom right -> top left
                {
                    lat = lat + (lat - points[0].Lat);
                    lon = lon + (lon - points[3].Lng);
                }
                else
                {
                    lat = lat - (lat - points[3].Lat); //bottom left -> top right
                    lon = lon - (lon - points[0].Lng);
                }
            }

            double width = Math.Abs(Math.Abs(points[1].Lng) - Math.Abs(points[0].Lng));
            double height = Math.Abs(Math.Abs(points[3].Lat) - Math.Abs(points[0].Lat));
            RectLatLng rect = new RectLatLng(lat, lon, width, height);
            return rect;
        }
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

        private DateTime byteToDate(byte[] b)
        {

            int year = byteToDateInt(b, 0, 4);
            string dateTime = Encoding.UTF8.GetString(b);
            int month = byteToDateInt(b, 5, 2);
            int day = byteToDateInt(b, 8, 2);
            int hour = byteToDateInt(b, 11, 2);
            int min = byteToDateInt(b, 14, 2);
            int sec = byteToDateInt(b, 17, 2);
            return new DateTime(year, month, day, hour, min, sec); 

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
            } catch (FormatException e)
            {
                return -1;
            }
        }

        private double byteToDecimal(byte[] b) //type 5
        {
            double numerator = BitConverter.ToInt32(b, 0);
            double denominator = BitConverter.ToInt32(b, 4);

            return Math.Round(numerator/ denominator, 2);
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

        private void PictureBox_Click(object sender, EventArgs e)
        {

        }

        private void MenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void BtnLeft_Click(object sender, EventArgs e)
        {

        }

        private void BtnRight_Click(object sender, EventArgs e)
        {

        }

        private void PictureBox_Click_1(object sender, EventArgs e)
        {

        }
    } //end class   
} //end namespace
