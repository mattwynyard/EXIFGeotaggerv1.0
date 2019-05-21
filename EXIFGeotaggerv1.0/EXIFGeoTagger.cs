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
using System.Threading;
using Amazon;
using System.Net;

public delegate Image getAWSImage();

namespace EXIFGeotagger //v0._1
{
    public partial class EXIFGeoTagger : Form
    {
 
        private Image image; //photo in photo viewer
        public string mDBPath;
        public string mLayer; //imported layer
        public Color mlayerColour;
        public String mlayerColourHex;

        private OleDbConnection connection;
        private Dictionary<string, GMapMarker[]> mOverlayDict;
        private GMapOverlay mOverlay; //the currently active overlay
        private GMapOverlay selectedMarkersOverlay; //overlay containing selected markers
        private GMapMarker currentMarker; //the current marker selected by user
        private GMapOverlay mSelectedOverlay;
        //index of currently layer in checkbox
        private int mSelectedOverlayIndex;
        private Dictionary<string, Record> mRecordDict;
        private  Dictionary<string, Record> mNewRecordDict;
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

        private double min_lat;
        private double min_lng;
        private double max_lat;
        private double max_lng;

        private Boolean mouseInBounds;
        private CancellationTokenSource _cts;

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

        /// <summary>
        /// Intialises a new Record and adds data extracted from access to each relevant field.
        /// The record is then added to the Record Dictionary.
        /// </summary>
        /// <param name="i: the number of records read"></param>
        /// <param name="row: the access record"></param>
        private void buildDictionary(int i, Object[] row)
        {
           try
            {
                Record r = new Record((string)row[1]);
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

                mRecordDict.Add(r.PhotoName, r);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        public void deSerializeData(String path)
        {
            Serializer s = new Serializer(path);
            mRecordDict = s.deserialize();
            plotLayer(mLayer, mlayerColourHex);
        }
       

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
                    for (int i = 0; i < count - 1; i += step)
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
            importForm.layerVariables += photoImportCallback;
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
                String inPath = saveDialog.FileName;
                Serializer s = new Serializer(mRecordDict);
                s.serialize(inPath);
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

        }

        #endregion

        #region Callbacks
        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="layer"></param>
        /// <param name="color"></param>
        public void photoImportCallback(string folderPath, string layer, string color)
        {
            GMapOverlay overlay = new GMapOverlay(layer);
            buildPhotoMarker(overlay, folderPath, color);
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
            await readFromDatabase(dbPath, allRecords);
            await writeGeoTag(inPath, outPath);
            mRecordDict = mNewRecordDict;
            mNewRecordDict = null;
            plotLayer(layer, color);
        }


        #endregion

        #region Threading
        private void cancelImport(object sender, EventArgs e)
        {
            if (_cts != null)
                _cts.Cancel();
        }

        /// <summary>
        /// Reads exif data from each photo and builds markers on map
        /// </summary>
        /// <param name="overlay"> the overlay the markers are being added to</param>
        /// <param name="folderPath">the location of the photos</param>
        /// <param name="color">the color off the markers</param>
        private async void buildPhotoMarker(GMapOverlay overlay, string folderPath, string color)
        {
            string[] files = Directory.GetFiles(folderPath);
            ProgressForm progressForm = new ProgressForm("Importing Photos...");
            progressForm.Show();
            progressForm.BringToFront();
            progressForm.cancel += cancelImport;
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            var progressHandler1 = new Progress<int>(value =>
            {
                progressForm.ProgressValue = value;
                progressForm.Message = "Import in progress, please wait... " + value.ToString() + "% completed";

            });
            var progressValue = progressHandler1 as IProgress<int>;
            try
            {
                await Task.Run(() =>
                {
                    int id = 0;
                    resetMinMax();
                    mRecordDict = new Dictionary<string, Record>();
                    Bitmap bitmap = ColorTable.getBitmap(color, 4);
                    int length = files.Length;
                    int counter = 0;
                    foreach (string file in files)
                    {
                        id++;
                        MarkerTag tag = new MarkerTag(color, id);
                        string photo = Path.GetFileName(file);
                        tag.PhotoName = photo;
                        tag.Path = Path.GetFullPath(file);
                        Image image = new Bitmap(file);
                        Record r = new Record(photo);

                        PropertyItem[] propItems = image.PropertyItems;
                        PropertyItem propItemLatRef = image.GetPropertyItem(0x0001);
                        PropertyItem propItemLat = image.GetPropertyItem(0x0002);
                        PropertyItem propItemLonRef = image.GetPropertyItem(0x0003);
                        PropertyItem propItemLon = image.GetPropertyItem(0x0004);
                        PropertyItem propItemAltRef = image.GetPropertyItem(0x0005);
                        PropertyItem propItemAlt = image.GetPropertyItem(0x0006);
                        //PropertyItem propItemGPSTime = image.GetPropertyItem(0x0007);
                        //PropertyItem propItemGPSSpeedRef = image.GetPropertyItem(0x000C);
                        //PropertyItem propItemGPSSpeed = image.GetPropertyItem(0x000D);
                        PropertyItem propItemDateTime = image.GetPropertyItem(0x0132);

                        image.Dispose();

                        byte[] latBytes = propItemLat.Value;
                        byte[] latRefBytes = propItemLatRef.Value;
                        byte[] lonBytes = propItemLon.Value;
                        byte[] lonRefBytes = propItemLonRef.Value;
                        byte[] altRefBytes = propItemAltRef.Value;
                        byte[] altBytes = propItemAlt.Value;
                        //byte[] gpsTimeBytes = propItemGPSTime.Value;
                        //byte[] gpsSpeedRefBytes = propItemGPSSpeedRef.Value;
                        //byte[] gpsSpeedBytes = propItemGPSSpeed.Value;
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
                            //altitude = -altitude;
                        }
                        r.Latitude = latitude;
                        r.Longitude = longitude;
                        r.Altitude = altitude;
                        r.TimeStamp = dateTime;
                        r.Path = Path.GetFullPath(file);
                        r.Id = id.ToString();

                        mRecordDict.Add(photo, r);

                        
                        setMinMax(latitude, longitude);
                        GMapMarker marker = new GMarkerGoogle(new PointLatLng(latitude, longitude), bitmap);
                        marker.Tag = tag;
                        overlay.Markers.Add(marker);
                        counter++;
                        double percent = ((double)counter / length) * 100;
                        int percentInt = (int)Math.Ceiling(percent);
                        if (progressValue != null)
                        {
                            progressValue.Report(percentInt);

                        }
                        token.ThrowIfCancellationRequested();
                    }
                });
                progressValue.Report(100);
                string title = "Importing complete";
                string message = "Complete";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                MsgBox(message, title, buttons);
                progressForm.Close();
                refreshUI(overlay, color);
            }
            catch (ArgumentException ex)
            {

            }
            catch (NullReferenceException ex)
            {
                txtConsole.AppendText(ex.StackTrace);
            }
            catch (OperationCanceledException)
            {
                string titleCancel = "Cancelled";
                string messageCancel = "Import cancelled";
                MessageBoxButtons buttonsCancel = MessageBoxButtons.OK;
                MsgBox(messageCancel, titleCancel, buttonsCancel);
                progressForm.Close();
            }

        }

        private void MsgBox(string title, string message, MessageBoxButtons buttons)
        {
            DialogResult result = MessageBox.Show(message, title, buttons);
            if (result == DialogResult.Yes)
            {
                Close();
            }
        }

        private async Task writeGeoTag(string inPath, string outPath)
        {
            ProgressForm progressForm = new ProgressForm("Writing geotags to photos...");
            string[] _files = Directory.GetFiles(inPath);
            progressForm.Show();
            progressForm.BringToFront();
            progressForm.cancel += cancelImport;
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            var progressHandler1 = new Progress<int>(value =>
            {
                progressForm.ProgressValue = value;
                progressForm.Message = "Geotagging, please wait... " + value.ToString() + "% completed";

            });
            var progressValue = progressHandler1 as IProgress<int>;
            try
            {
                await Task.Run(() =>
                {
                    Record r;
                    resetMinMax();
                    int length = _files.Length;
                    geoTagCount = 0;
                    errorCount = 0;
                    stationaryCount = 0;
                    mNewRecordDict = new Dictionary<string, Record>();
                    foreach (string _file in _files)
                    {
                        Bitmap image = new Bitmap(_file);
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
                            r = mRecordDict[Path.GetFileNameWithoutExtension(_file)];

                            if (r.GeoMark)
                            {
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
                                string photoName = Path.GetFileNameWithoutExtension(_file);
                                string photoSQL = "SELECT Photo_Geotag FROM PhotoList WHERE Photo_Camera = '" + photoName + "';";
                                OleDbCommand commandGetPhoto = new OleDbCommand(photoSQL, connection);
                                string photo = (string)commandGetPhoto.ExecuteScalar();
                                r.PhotoName = photo; //new photo name 
                                string geotagSQL = "UPDATE PhotoList SET PhotoList.GeoTag = True WHERE Photo_Camera = '" + photoName + "';";
                                OleDbCommand commandGeoTag = new OleDbCommand(geotagSQL, connection);
                                commandGeoTag.ExecuteNonQuery();
                                string path = outPath + "\\" + photo + ".jpg";
                                string pathSQL = "UPDATE PhotoList SET Path = '" + path + "' WHERE Photo_Camera = '" + photoName + "';";
                                OleDbCommand commandPath = new OleDbCommand(pathSQL, connection);
                                commandPath.ExecuteNonQuery();

                                image.Save(path);
                                r.Path = path;
                                r = mRecordDict[Path.GetFileNameWithoutExtension(_file)];
                                mNewRecordDict.Add(photo, r);
                                
                                setMinMax(r.Latitude, r.Longitude);
                                image.Dispose();
                                image = null;
                            }
                            else
                            {
                                stationaryCount++;
                            }
                        }
                        catch (KeyNotFoundException ex)
                        {
                            errorCount++;
                            image.Dispose();
                            image = null;
                        }
                        geoTagCount++;
                        double percent = ((double)geoTagCount / length) * 100;
                        int percentInt = (int)Math.Ceiling(percent);
                        if (progressValue != null)
                        {
                            progressValue.Report(percentInt);

                        }
                        token.ThrowIfCancellationRequested();
                    }
                    //photoIcon.Dispose();
                    
                });
            }
            catch (ArgumentException ex)
            {

            }
            catch (NullReferenceException ex)
            {
                txtConsole.AppendText(ex.StackTrace);
            }
            catch (OperationCanceledException)
            {
                string titleCancel = "Cancelled";
                string messageCancel = "Import cancelled";
                MessageBoxButtons buttonsCancel = MessageBoxButtons.OK;
                MsgBox(messageCancel, titleCancel, buttonsCancel);
                progressForm.Close();
            }
            string title = "Finished";
            string message = "Geotagging complete\n" + (geoTagCount - errorCount) + " of " + _files.Length + " photos geotagged\n"
                + "Photos with no geomark: " + stationaryCount + "\n" + "Photos with no gps point: " + errorCount + "\n";
            MessageBoxButtons buttons = MessageBoxButtons.OK;
            DialogResult result = MessageBox.Show(message, title, buttons);
            if (result == DialogResult.Yes)
            {
                Close();
            }
            progressForm.Close();
            connection.Close();
            zoomToMarkers();
        }

        private async Task readFromDatabase(string path, Boolean allRecords)
        {
            mRecordDict = new Dictionary<string, Record>();
            ProgressForm progressForm = new ProgressForm("Reading from database...");
            progressForm.Show();
            progressForm.BringToFront();
            progressForm.cancel += cancelImport;
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
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
                            buildDictionary(i, row);
                            i++;
                            double percent = ((double)i / length) * 100;
                            int percentInt = (int)Math.Ceiling(percent);
                            if (progressValue != null)
                            {
                                progressValue.Report(percentInt);

                            }
                            token.ThrowIfCancellationRequested();
                        }
                    }

                });
                progressValue.Report(100);
                string title = "Importing complete";
                string message = "Complete";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                MsgBox(message, title, buttons);
                progressForm.Close();
               
            }
            catch (ArgumentException ex)
            {

            }
            catch (NullReferenceException ex)
            {
                txtConsole.AppendText(ex.StackTrace);
            }
            catch (OperationCanceledException)
            {
                string titleCancel = "Cancelled";
                string messageCancel = "Read cancelled";
                MessageBoxButtons buttonsCancel = MessageBoxButtons.OK;
                MsgBox(messageCancel, titleCancel, buttonsCancel);
                progressForm.Close();
            }
        }

        #endregion

        #region HELPERFUNCTIONS

        private void setMinMax(double lat, double lng)
        {
            if (lat > max_lat)
            {
                max_lat = lat;
            }
            if (lat < min_lat)
            {
                min_lat = lat;
            }
            if (lng > max_lng)
            {
                max_lng = lng;
            }
            if (lng < min_lng)
            {
                min_lng = lng;
            }
        }

        private void resetMinMax()
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
    } //end class   
} //end namespace
