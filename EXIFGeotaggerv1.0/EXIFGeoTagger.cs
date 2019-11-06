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
using System.Data.OleDb;
using System.Data;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using Amazon;
using Amazon.S3.Model;
using System.Collections.Concurrent;
using ShapeFile;
using System.Diagnostics;
using System.Text.RegularExpressions;
using OpenCV;

public delegate Image getAWSImage();

namespace EXIFGeotagger //v0._1
{
    public partial class EXIFGeoTagger : Form
    {

        public delegate Task<GMapOverlay> ReadGeoTagDelegate(string folderPath, string layer, Color color);

        Stopwatch stopWatch;

        private readonly double BUFFER = 0.1; //degrees to enlarge encompassing rectangle to handle boundary conditions

        private string connectionString;

        private Image image; //photo in photo viewer
        public string mDBPath;
        public string mLayer; //imported layer
        public Color mlayerColour;
        public String mlayerColourHex;
        private AWSConnection client;
        private OleDbConnection connection;
        private Dictionary<string, GMapMarker[]> mOverlayDict;
        private Dictionary<string, QuadTree> mQuadTreeDict;
        private QuadTree qt;
        private GMapOverlay mOverlay; //the currently active overlay
        private GMapOverlay selectedMarkersOverlay; //overlay containing selected markers
        private GMapMarker selectedMarker; //the current marker selected by user
        private GMapMarker[] selectedMarkersList;
        private GMapOverlay mSelectedOverlay;
        private GMapOverlay zoomToMarkersOverlay;
        private GMapRoute mSelectedRoute;
        //index of currently layer in checkbox
        private int mSelectedOverlayIndex;
        private string mSelectedLayer;
        private Boolean isLayerSelected = false;
        private Dictionary<string, Record> mRecordDict;
        private ConcurrentDictionary<string, Record> mConRecordDict;

        private Dictionary<string, ESRIShapeFile> mShapeDict;
        private static readonly Object obj = new Object();
        private LayerAttributes mLayerAttributes;
        public string[] mFiles; //array containing absolute paths of photos.
        public string outFolder; //folder path to save geotag photos

        public Boolean allRecords;

        private ListViewItem currentListItem;
        private Boolean listIsFocused;
        //Tools
        private Boolean mZoom = false;
        private Boolean mArrow = true;

        private int layerCount;
        private ImageList imageList;

        private ContextMenu mapContextMenu;
        private MenuItem itemCopy;
        private Boolean mouseDown = false;
        private Boolean panning = false;
        private PointLatLng topLeft;
        private PointLatLng bottomRight;
        private List<PointLatLng> zoomRect;
        private GMapOverlay zoomOverlay; //overlay containing zoom rectangle
        private GMapPolygon rect;
        private List<GMapMarker> selection; //currently selected marker group

        //private QuadTree qt;

        private static double min_lat;
        private static double min_lng;
        private static double max_lat;
        private static double max_lng;

        private Boolean mouseInBounds;

        private PointLatLng point; //cursor point


        private BlockingCollection<string> fileQueue;
        private TreeNode rootNode;

        private string currentBucket;
        private string currentKey;
        private string mSelectedFile;

        string elapsedTime; //Stopwatch timer

        /// <summary>
        /// Class constructor to intialize form
        /// </summary>
        public EXIFGeoTagger()
        {
            InitializeComponent();
            menuRunGeoTag.Enabled = true;         
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
            gMap.OnMapZoomChanged += gMap_OnMapZoomChanged;
           
            gMap.MouseMove += gMap_MouseMove;
            gMap.DragButton = MouseButtons.Right;
            gMap.OnMapZoomChanged += gMap_OnMapZoomChanged;
            gMap.MouseDown += gMap_MouseDown;
            gMap.MouseUp += gMap_MouseUp;
            gMap.MouseClick += gMap_MouseClick;
            gMap.MapScaleInfoEnabled = true;
            gMap.PreviewKeyDown += gMap_KeyDown;
            gMap.Enter += gMap_onEnter;
            gMap.OnMarkerDoubleClick += gMap_onMarkerDoubleClick;
            gMap.OnRouteClick += gMap_OnRouteClick;
            gMap.Leave += gMap_onLeave;
            mOverlayDict = new Dictionary<string, GMapMarker[]>();
            mConRecordDict = new ConcurrentDictionary<string, Record>();
            mShapeDict = new Dictionary<string, ESRIShapeFile>();
            //layerItem = new ListViewItem();
            imageList = new ImageList();
            layerCount = 0;
            selectedMarkersOverlay = new GMapOverlay("selected");
            zoomToMarkersOverlay = new GMapOverlay("zoomTo");
            mQuadTreeDict = new Dictionary<string, QuadTree>();
            mapContextMenu  = new ContextMenu();


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
            stream.Close();
            ListViewItem layerItem = new ListViewItem(newOverlay.Id, layerCount);
            layerCount++;

            layerItem.Text = newOverlay.Id;
            layerItem.Checked = true;

            listLayers.SmallImageList = imageList;
            listLayers.Items.Add(layerItem);

            newOverlay.IsVisibile = true;
            mOverlay.IsVisibile = true;
            //zoomToMarkers();
            
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
            int id = 0;
            if (mRecordDict != null)
            {
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
                    //PointXY p = new PointXY(lon, lat);
                    
                    GMapMarker marker = new GMarkerGoogle(new PointLatLng(lat, lon), bitmap);

                    if (qt != null)
                    {
                        qt.insert(marker);
                    }
                    marker.Tag = tag;
          
                    marker = setToolTip(marker);
                    overlay.Markers.Add(marker);
                    id++;
                }
            }
            mQuadTreeDict.Add(overlay.Id, qt);
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
            marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
            return marker;
        }

        //private void rebuildMarkers(GMapOverlay overlay, int size)
        //{

        //}

        private RectangleXY getBoundaryRectangle()
        {
            Rectangle bounds = gMap.Bounds;
            PointLatLng _upperLeft = gMap.FromLocalToLatLng(bounds.X, bounds.Y);
            PointLatLng _upperRight = gMap.FromLocalToLatLng(bounds.Right, bounds.Y);
            PointLatLng _bottomRight = gMap.FromLocalToLatLng(bounds.Right, bounds.Bottom);
            PointLatLng _bottomLeft = gMap.FromLocalToLatLng(bounds.X, bounds.Bottom);

            PointXY upperLeft = new PointXY(_upperLeft.Lng, _upperLeft.Lat);
            PointXY upperRight = new PointXY(_upperRight.Lng, _upperRight.Lat);
            PointXY bottomRight = new PointXY(_bottomRight.Lng, _bottomRight.Lat);
            PointXY bottomLeft = new PointXY(_bottomLeft.Lng, _bottomLeft.Lat);
            RectangleXY rect = new RectangleXY(upperLeft, upperRight, bottomRight, bottomLeft);
            return rect;
        }

        private void buildMarkers(GMapOverlay overlay, int size)
        {
            QuadTree qt = mQuadTreeDict[overlay.Id];
            RectangleXY rect = getBoundaryRectangle();
            List<GMapMarker> markersList = qt.queryRange(rect);
            GMapMarker[] markers = markersList.ToArray();
            MarkerTag tag = (MarkerTag)markers[0].Tag;
            Bitmap bitmap = ColorTable.getBitmap(tag.Color, size);

            for (int i = 0; i < markers.Length; i ++)
            {                
                tag = (MarkerTag)markers[i].Tag;
                tag.Size = size;
                GMapMarker newMarker = new GMarkerGoogle(markers[i].Position, bitmap);

                if (markers[i].Tag != null)
                {
                    newMarker.Tag = markers[i].Tag;
                }
                newMarker = setToolTip(newMarker);
                overlay.Markers.Add(newMarker);


            }
        }

        private void rebuildMarkers(GMapOverlay overlay, int size)
        {
            overlay.Markers.Clear();
            try
            {
                //GMapMarker[] markers = mOverlayDict[overlay.Id];
                QuadTree qt = mQuadTreeDict[overlay.Id];
                RectangleXY rect = getBoundaryRectangle();
                List<GMapMarker> markersList = qt.queryRange(rect);
                GMapMarker[] markers = markersList.ToArray();
                MarkerTag tag = (MarkerTag)markers[0].Tag;
                if (tag == null)
                {
                    if (overlay.Id == "zoomTo")
                    {
                        GMapMarker[] zoomToMarkers = mOverlayDict["zoomTo"];
                        int zoomCount = zoomToMarkers.Length;
                        for (int j = 0; j < zoomCount; j += 1)
                        {
                            GMapMarker newMarker = new GMarkerGoogle(zoomToMarkers[j].Position, GMarkerGoogleType.blue_small);
                            //newMarker = setToolTip(newMarker);
                            overlay.Markers.Add(newMarker);
                        }
                    } else
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
                }
                else
                {
                    markers = mOverlayDict[overlay.Id];
                    Rectangle bounds = gMap.Bounds;
                    List<PointLatLng> boundary = new List<PointLatLng>();
                    PointLatLng upperLeft = gMap.FromLocalToLatLng(bounds.X, bounds.Y);
                    boundary.Add(upperLeft);
                    PointLatLng upperRight = gMap.FromLocalToLatLng(bounds.Right, bounds.Y);
                    boundary.Add(upperRight);
                    PointLatLng bottomRight = gMap.FromLocalToLatLng(bounds.Right, bounds.Bottom);
                    boundary.Add(bottomRight);
                    PointLatLng bottomLeft = gMap.FromLocalToLatLng(bounds.X, bounds.Bottom);
                    boundary.Add(bottomLeft);
                    int count = markers.Length;
                    int step;
                    if (count > 100000)
                    {
                        step = getStep(size);
                    }
                        else
                    {
                        step = 1;
                    }
                Dictionary<string, string> dict = tag.Dictionary;
                    Bitmap bitmap = null;
                    if (dict == null)
                    {
                        bitmap = ColorTable.getBitmap(tag.Color, size);
                    } else
                    {
                        bitmap = ColorTable.getBitmap(dict, tag.Color, size);
                    }
                    for (int i = 0; i < count; i+=step)
                    {
                        if (gMap.isPointInBoundary(boundary, markers[i].Position.Lat.ToString(), markers[i].Position.Lng.ToString()))
                        {
                            tag = (MarkerTag)markers[i].Tag;
                            tag.Size = size;
                            GMapMarker newMarker = new GMarkerGoogle(markers[i].Position, bitmap);

                            if (markers[i].Tag != null)
                            {
                                newMarker.Tag = markers[i].Tag;
                            }
                            newMarker = setToolTip(newMarker);
                            overlay.Markers.Add(newMarker);
                        }
                       
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
            addListItem(ColorTable.ColorTableDict, overlay, color);
            zoomToMarkers();
        }

        private void addListItem(IDictionary dictionary, GMapOverlay overlay, string color)
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(dictionary[color] + "_24px.png");
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
            stream.Close();
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
            RectLatLng rect = polygonToRect(zoomRect);
            try
            {
                gMap.SetZoomToFitRect(rect);
            } catch (Exception ex)
            {
                string s = ex.StackTrace;
            }
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

            foreach (GMapOverlay overlay in overlays)
            {
                if (overlay.Id != "zoom")
                {
                    if ((int)gMap.Zoom < 11)
                    {
                        //rebuildMarkers(overlay, 4);
                        buildMarkers(overlay, 4);
                    }
                    else if ((int)gMap.Zoom < 15 && (int)gMap.Zoom >= 11)
                    {

                        //rebuildMarkers(overlay, 4);
                        buildMarkers(overlay, 4);
                    }
                    else if ((int)gMap.Zoom < 18 && (int)gMap.Zoom >= 15)
                    {
                        //rebuildMarkers(overlay, 8);
                        buildMarkers(overlay, 4);
                    }
                    else
                    {
                        //rebuildMarkers(overlay, 12);
                        buildMarkers(overlay, 4);
                    }
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

        /// <summary>
        /// Called when user clicks mouse button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gMap_MouseDown(object sender, MouseEventArgs e)
        {
            //mapContextMenu.MenuItems.Clear();
            mouseDown = true;
                zoomOverlay = new GMapOverlay("zoom");

                topLeft = gMap.FromLocalToLatLng(e.X, e.Y);
                zoomRect = new List<PointLatLng>();
                var point = gMap.FromLocalToLatLng(e.X, e.Y);
                txtConsole.Clear();
                txtConsole.AppendText("latitude: " + Math.Round(point.Lat, 6) + " longitude: " + Math.Round(point.Lng, 6));
            //}           
        }

        private void gMap_onMapDrag()
        {
            gMap_OnMapZoomChanged();
            panning = true;
        }

        private void gMap_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
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
            lbPosition.Text = "latitude: " + Math.Round(point.Lat, 6) + " longitude: " + Math.Round(point.Lng, 6);
        }

        private void gMap_MouseUp(object sender, MouseEventArgs e)
        {

            if (!panning && e.Button == MouseButtons.Right)
            {
                point = gMap.FromLocalToLatLng(e.X, e.Y);
               
                itemCopy = mapContextMenu.MenuItems.Add("Copy position to clipboard");
                ContextMenu = mapContextMenu;
                itemCopy.Click += copyToClipBoard;
            }

            if (e.Button == MouseButtons.Left)
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
                else if (mouseDown && !mZoom)
                {
                    List<GMapMarker> selection = new List<GMapMarker>();
                    if (rect != null)
                    {
                        PointXY topLeft = new PointXY(rect.Points[0].Lng, rect.Points[0].Lat);
                        PointXY topRight = new PointXY(rect.Points[1].Lng, rect.Points[1].Lat);
                        PointXY bottomRight = new PointXY(rect.Points[2].Lng, rect.Points[2].Lat);
                        PointXY bottomLeft = new PointXY(rect.Points[3].Lng, rect.Points[3].Lat);
                        RectangleXY selectionBox = new RectangleXY(topLeft, topRight, bottomRight, bottomLeft);
                        if (qt != null)
                        {
                            selection = qt.queryRange(selectionBox);
                            if (selection.Count != 0)
                            {
                                markerGroupSelect(selection);
                            }
                            else
                            {
                                clearOverlay(selectedMarkersOverlay);
                            }
                        }
                        zoomOverlay.Polygons.Remove(rect);
                        gMap.Overlays.Remove(zoomOverlay);
                        rect = null;                  
                    } else
                    {
                        clearOverlay(selectedMarkersOverlay);
                    }                
                }
            }
            
            mouseDown = false;
            if (panning)
            {
                panning = false;
            }
            
        }

        private void clearOverlay(GMapOverlay overlay)
        {
            overlay.Clear(); //selects a new marker
            gMap.Overlays.Remove(overlay);
            mOverlayDict.Remove(overlay.Id);
        }

        /// <summary>
        /// Selects a group of markers when user drags box over icons. Markers are added to overlay
        /// </summary>
        /// <param name="markers"> The list of GMapMarker obtained from quad tree that the user selected</param>
        private void markerGroupSelect(List<GMapMarker> markers)
        {
            clearOverlay(selectedMarkersOverlay);
            foreach (var marker in markers)
            {
                MarkerTag tag = (MarkerTag)marker.Tag;
                tag.IsSelected = true;
                Bitmap bitmap = ColorTable.getBitmap("Red", tag.Size);
                GMapMarker newMarker = new GMarkerGoogle(marker.Position, bitmap);
                selectedMarkersOverlay.Markers.Add(newMarker);
            }
            gMap.Overlays.Add(selectedMarkersOverlay);
            GMapMarker[] selectedMarkers = selectedMarkersOverlay.Markers.ToArray<GMapMarker>();
            selectedMarkersList = selectedMarkers;
            mOverlayDict.Add("selected", selectedMarkers);
            gMap_OnMapZoomChanged();

        }

        /// <summary>
        /// Clears marker selection from map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gMap_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            mapContextMenu.MenuItems.Clear();
            if (mouseInBounds && e.Button == MouseButtons.Left)
            {               
                if (panning)
                {
                    panning = false;
                }
                if (selectedMarker != null)
                {                 
                    MarkerTag currentTag = (MarkerTag)selectedMarker.Tag;
                    currentTag.IsSelected = false;
                    selectedMarkersOverlay.Clear();
                    mOverlayDict.Remove("selected");
                    selectedMarker = null;
                } else if (selectedMarkersList != null) {
                    clearOverlay(selectedMarkersOverlay);
                    selectedMarkersList = null;
                }
            }
            
        }

        private void copyToClipBoard(object sender, EventArgs e)
        {
            string lat = point.Lat.ToString();
            string lng = point.Lng.ToString();
            System.Windows.Clipboard.SetText(lat + " " + lng);
        }

        //private void gMap_OnMouseMoved(object sender, System.Windows.Forms.MouseEventArgs e)
        //{
        //    var point = gMap.FromLocalToLatLng(e.X, e.Y);

        //    lbPosition.Text = "latitude: " + Math.Round(point.Lat, 6) + " longitude: " + Math.Round(point.Lng, 6);
        //    //gMap.MouseHover += gMap_OnMouseHoverChanged;
        //}

        private void gMap_onMarkerDoubleClick(GMapMarker marker, System.Windows.Forms.MouseEventArgs e)
        {
            
        }

        private void gMap_OnMarkerClick(GMapMarker marker, System.Windows.Forms.MouseEventArgs e)
        {
            MarkerTag tag = (MarkerTag)marker.Tag;
            if (selectedMarker == null)
            {
                selectedMarker = marker;
                MarkerTag currentTag = (MarkerTag)selectedMarker.Tag;
                currentTag.IsSelected = true;
                getPicture(selectedMarker);
            }
            else
            {
                if (tag == null)
                {
                    selectedMarkersOverlay.Clear();
                    gMap.Overlays.Remove(selectedMarkersOverlay);
                    mOverlayDict.Remove("selected");
                }
                else
                {
                    if (tag.IsSelected == true) //deselects marker
                    {
                        selectedMarkersOverlay.Clear();
                        gMap.Overlays.Remove(selectedMarkersOverlay);
                        mOverlayDict.Remove("selected");
                        tag.IsSelected = false;
                        selectedMarker = null;
                    }
                    else
                    {
                        selectedMarkersOverlay.Clear(); //selects a new marker
                        gMap.Overlays.Remove(selectedMarkersOverlay);
                        mOverlayDict.Remove("selected");
                        selectedMarker = marker;
                        MarkerTag currentTag = (MarkerTag)selectedMarker.Tag;
                        currentTag.IsSelected = true;
                        getPicture(selectedMarker);
                    }
                }
            }
        }

        private void gMap_OnRouteClick(GMapRoute route, System.Windows.Forms.MouseEventArgs e)
        {

            GMapOverlay overaly = route.Overlay;
        }

        private async void getPicture(GMapMarker marker)
        {
            MarkerTag tag = (MarkerTag)marker.Tag;
            Bitmap bitmap = ColorTable.getBitmap("Red", tag.Size);
            GMapMarker newMarker = new GMarkerGoogle(marker.Position, bitmap);
            newMarker.LocalPosition = marker.LocalPosition;
            //newMarker.Offset = marker.Offset;
            selectedMarkersOverlay.Markers.Add(newMarker);
            gMap.Overlays.Add(selectedMarkersOverlay);
            GMapMarker[] selectedMarkers = selectedMarkersOverlay.Markers.ToArray<GMapMarker>();
            mOverlayDict.Add("selected", selectedMarkers);
            if (tag.Record.Bucket != null)
            {
                Image image = await client.getAWSPicture(tag.Record.Bucket, tag.Record.Key);
                if (pictureBox.Image != null)
                {
                    pictureBox.Image.Dispose();
                }
                pictureBox.Image = image;
            }
            else
            {
                try
                {
                    if (pictureBox.Image != null)
                    {
                        pictureBox.Image.Dispose();
                    }
                    lbPhoto.Text = tag.Path;
                    pictureBox.Image = Image.FromFile(tag.Path);
                }
                catch (FileNotFoundException ex)
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
                Cursor = System.Windows.Forms.Cursors.Cross;
                System.Windows.Forms.Cursor.Show();
            }
            else if (mArrow)
            {
                Cursor = System.Windows.Forms.Cursors.Arrow;
                System.Windows.Forms.Cursor.Show();
            }
        }

        private void gMap_onLeave(object sender, EventArgs e)
        {
            txtConsole.Clear();
            txtConsole.AppendText("Leave");
            Cursor = System.Windows.Forms.Cursors.Arrow;
            mouseInBounds = false;
        }

        private void btnZoom_Click(object sender, EventArgs e)
        {
            Cursor = System.Windows.Forms.Cursors.Cross;
            mZoom = true;
            mArrow = false;
        }

        private void btnArrow_Click(object sender, EventArgs e)
        {
            Cursor = System.Windows.Forms.Cursors.Arrow;
            mZoom = false;
            mArrow = true;
        }

        private void lbScale_Click(object sender, EventArgs e)
        {

        }

        #endregion

        #region Form Events


        private void OnKeyDownHandler(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string coordinate = txtLatLng.Text;
                String[] coordinates = isValidCoordinate(coordinate);
                if (coordinates != null) {
                    PointLatLng location = new PointLatLng(Convert.ToDouble(coordinates[0]), Convert.ToDouble(coordinates[1]));
                    gMap.Position = location;
                    gMap.Zoom = 20;
                    GMapMarker marker = new GMarkerGoogle(location, GMarkerGoogleType.blue_small);
                    GMapMarker[] markerArr = { marker };

                    zoomToMarkersOverlay.Markers.Add(marker);
                    
                    mOverlayDict.Add("zoomTo", markerArr);
                    gMap.Overlays.Add(zoomToMarkersOverlay);
                }
            }
        }

        /// <summary>
        /// Checks (using regex) if user entered a valid lat long coordinate in text box
        /// </summary>
        /// <param name="coordinate">the user string to parse</param>
        /// <returns>the valid coordinate as a string[] or null if coordinate not valid</returns>
        private string[] isValidCoordinate(String coordinate)
        {
            string[] coordinates = coordinate.Split(' ');
            var regexLat = new Regex(@"^(\+|-)?(?:90(?:(?:\.0{1,15})?)|(?:[0-9]|[1-8][0-9])(?:(?:\.[0-9]{1,15})?))$");
            var regexLng = new Regex(@"^(\+|-)?(?:180(?:(?:\.0{1,15})?)|(?:[0-9]|[1-9][0-9] |1[0-7][0-9])(?:(?:\.[0-9]{1,15})?))$");
            Boolean lat = regexLat.IsMatch(coordinates[0]);
            Boolean lng = regexLng.IsMatch(coordinates[1]);
            if (lat && lng)
            {
                return coordinates;
            } else
            {
                return null;
            }


        }

        private void txtLatLng_Enter(object sender, EventArgs e)
        {
            txtLatLng.Text = "";
            txtLatLng.ForeColor = Color.Black;
            txtLatLng.Font = new Font(txtLatLng.Font, System.Drawing.FontStyle.Regular);
            mOverlayDict.Remove("zoomTo");
            zoomToMarkersOverlay.Clear();
            gMap.Overlays.Remove(zoomToMarkersOverlay);

        }
        

        //    private void PhotosToolStripMenuItem1_Click(object sender, EventArgs e)
        //{
        //    ImportDataForm importForm = new ImportDataForm("photos");
        //    mRecordDict = new Dictionary<string, Record>();
        //    importForm.mParent = this;
        //    importForm.Show();
        //    importForm.importData += readGeoTagCallback;
        //}


        private void fileMenuOpen_Click(object sender, ToolStripItemClickedEventArgs e)
        {
        }

        private void markersMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void listLayers_MouseEnter(object sender, EventArgs e)
        {
            listIsFocused = true;
        }

        private void listLayers_MouseLeave(object sender, EventArgs e)
        {
            listIsFocused = false;
        }

        private void listLayers_ItemMouseHover(object sender, ListViewItemMouseHoverEventArgs e)
        {
            Boolean itemIsHover = true;
        }

        private void listLayers_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            ListView ls = sender as ListView;
            if (currentListItem != null)
            {
                if (e.Button == MouseButtons.Right)
                {
                    ContextMenu contextMenu = new ContextMenu();
                    var itemTable = contextMenu.MenuItems.Add("Open Attributes Table");
                    var itemDelete = contextMenu.MenuItems.Add("Delete Layer");
                    var itemZoom = contextMenu.MenuItems.Add("Zoom to Layer");
                    listLayers.ContextMenu = contextMenu;
                    itemDelete.Click += contextMenu_ItemDelete;
                    itemTable.Click += contextMenu_ItemTableClick;
                }
                if (e.Button == MouseButtons.Left)
                {

                    
                }
            }
        }

        private void contextMenu_ItemTableClick(object sender, EventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            string layer = listLayers.FocusedItem.Text;
            ESRIShapeFile shape = null;
            try
            {
                shape = mShapeDict[layer];
            }
            catch (KeyNotFoundException ex)
            {

            }
            if (shape != null)
            {
                DataTable table = shape.DataTable;
                ShapeTable tableForm = new ShapeTable(table);
                tableForm.Show();
            }
            

        }


        /// <summary>
        /// Deletes layer from menu items and shape record dictionary and overlay dictionary.
        /// Overaly is then removed from gMap control and refreshed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contextMenu_ItemDelete(object sender, EventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            string layer = listLayers.FocusedItem.Text;
            mShapeDict.Remove(layer);
            if (currentListItem.Focused && currentListItem.Selected)
            {
                
                mOverlayDict.Remove(mSelectedOverlay.Id);
                gMap.Overlays.Remove(mSelectedOverlay);
                mSelectedOverlay.Dispose();
                mOverlay.Dispose();
                currentListItem.Remove();
                if (mRecordDict != null)
                {
                    try
                    {
                        mRecordDict.Remove(layer);
                    }
                    catch (KeyNotFoundException ex)
                    {

                    }
                }
                mSelectedOverlay = null;
                mOverlay = null;
                gMap.Refresh();
                long bytes = GC.GetTotalMemory(true); //force full memory collection
            }
            
        }

        private void listLayers_ItemActivate(object sender, EventArgs e)
        {
            txtConsole.Text = "ïtemActivate\n";
        }

        private void listLayers_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListView ls = sender as ListView;
            currentListItem = ls.FocusedItem;
            mSelectedOverlayIndex = currentListItem.Index;
            try
            {
                mSelectedOverlay = gMap.Overlays.ElementAt(mSelectedOverlayIndex);
            } catch (ArgumentOutOfRangeException ex)
            {

            }
            if (ls.FocusedItem.Checked)
            {


            }
        }


        private void listLayers_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            
            ListView ls = sender as ListView;
            if (currentListItem != null)
            {
                currentListItem.BackColor = Color.White;
            }
            currentListItem = ls.FocusedItem;
            mSelectedLayer = currentListItem.Text;
            try
            {
                qt = mQuadTreeDict[ls.FocusedItem.Text];
            } catch (KeyNotFoundException ex)
            {
            }
            ls.FocusedItem.BackColor = Color.SteelBlue;

        }

        private void listLayers_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            mOverlay = gMap.Overlays.ElementAt(e.Index);
            CheckState cs = e.NewValue;
            if (cs.ToString() == "Unchecked")
            {
                mOverlay.IsVisibile = false; 
            } else
            {
                mOverlay.IsVisibile = true; 
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


        private void importTextCallback(DataTable table, string layer, Color color)
        {
            DataColumnCollection columns = table.Columns;

        }
        public async void importShapeCallback(string path, string layer, Color color)
        {
            Bitmap bitmap = null;
            ShapeReader reader = new ShapeReader(path);
            GMapOverlay overlay = new GMapOverlay(layer);
            ESRIShapeFile shape = new ESRIShapeFile();
            gMap.Overlays.Add(overlay);
            reader.errorHandler += errorHandlerCallback;
            shape = reader.read(shape);

            if (shape.ShapeType == 3 || shape.ShapeType == 13)
            {
                GMapRoute line_layer;
                PolyLineZ[] polyLines = shape.PolyLineZ;
                foreach (PolyLineZ polyLine in polyLines)
                {
                    PointLatLng[] points = polyLine.points;
                    line_layer = new GMapRoute(points, "lines"); //TODO get carriage number
                    line_layer.Stroke = new Pen(color, 2);
                    line_layer.Tag = color;
                    line_layer.IsHitTestVisible = true;
      
                    overlay.Routes.Add(line_layer);
                    if (polyLine.numParts > 1)
                    {
                        int n = polyLine.numParts;
                    }
                }
            }
            else if (shape.ShapeType == 5)
            {
                Polygon[] polygons = shape.Polygon;    
                foreach (var polygon in polygons) 
                {
                    PointLatLng[] points = polygon.points;
                    long start = 0;
                    long end = 0;
                    for (int i = 0; i < polygon.numParts; i++)
                    {
                        start = polygon.parts[i];
                        
                        if (i == polygon.numParts -1)
                        {
                            end = polygon.numPoints - 1;
                        } else
                        {
                            end = polygon.parts[i + 1] - 1;           
                        }
                        long length = end + 1 - start;
                        PointLatLng[] pointPart = new PointLatLng[length];
                        Array.Copy(points, start, pointPart, 0, length);
                        List<PointLatLng> pointsList = pointPart.ToList<PointLatLng>();
                        GMapPolygon polygon_layer = new GMapPolygon(pointsList, "polygons");
                        polygon_layer.Fill = new SolidBrush(Color.FromArgb(50, color));
                        polygon_layer.Stroke = new Pen(color, 1);
                        overlay.Polygons.Add(polygon_layer);
                    }
                }
            }
            else if (shape.ShapeType == 1)
            {
                ShapeFile.Point[] points = shape.Point;
                bitmap = ColorTable.getBitmap(ColorTable.ColorCrossDict, color.Name, 4);
                int id = 0;
                foreach (ShapeFile.Point point in points)
                {
                    PointLatLng pointLatLng = new PointLatLng(point.y, point.x);
                    MarkerTag tag = new MarkerTag(color.Name, id);
                    tag.Dictionary = ColorTable.ColorCrossDict;
                    GMapMarker marker = new GMarkerGoogle(pointLatLng, bitmap);
                    marker.Tag = tag;
                    overlay.Markers.Add(marker);
                    id++;
                }
                GMapMarker[] markersArr = overlay.Markers.ToArray<GMapMarker>();
                mOverlayDict.Add(layer, markersArr);

            }
            else if (shape.ShapeType == 8)
            {
                MultiPoint[] points = shape.MultiPoint;
                bitmap = ColorTable.getBitmap(ColorTable.ColorCrossDict, color.Name, 4);
                int id = 0;
                foreach (MultiPoint point in points)
                {
                    PointLatLng[] pointsLatLng = point.points;
                    foreach (PointLatLng p in pointsLatLng)
                    {
                        MarkerTag tag = new MarkerTag(color.Name, id);
                        tag.Dictionary = ColorTable.ColorCrossDict;
                        tag.Bitmap = bitmap;
                        GMapMarker marker = new GMarkerGoogle(p, tag.Bitmap);
                        marker.Tag = tag;
                        overlay.Markers.Add(marker);
                        id++;
                    }
                }
                GMapMarker[] markersArr = overlay.Markers.ToArray<GMapMarker>();
                mOverlayDict.Add(layer, markersArr);


            }
            addListItem(ColorTable.ColorCrossDict, overlay, color.Name);

            await Task.Run(() =>
            {
                DataTable table = reader.readDBF().Result;
                shape.DataTable = table;
                table = null;
                mShapeDict.Add(layer, shape);
            });       
        }

        public void errorHandlerCallback(string error, string message)
        {
            MessageBox.Show(message, error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="layer"></param>
        /// <param name="color"></param>
        public void exfImportCallback(string folderPath, string layer, Color color)
        {
            Serializer s = new Serializer(folderPath);
            mLayerAttributes = s.deserialize();
            mRecordDict = mLayerAttributes.Data;
              
            max_lat = mLayerAttributes.MaxLat;
            min_lat = mLayerAttributes.MinLat;
            max_lng = mLayerAttributes.MaxLng;
            min_lng = mLayerAttributes.MinLng;
            PointXY topLeft = new PointXY(min_lng - BUFFER, max_lat + BUFFER);
            PointXY topRight = new PointXY(max_lng + BUFFER, max_lat + BUFFER);
            PointXY bottomRight = new PointXY(max_lng + BUFFER, min_lat - BUFFER);
            PointXY bottomLeft = new PointXY(min_lng - BUFFER, min_lat - BUFFER);
            RectangleXY rect = new RectangleXY(topLeft, topRight, bottomRight, bottomLeft);
            qt = new QuadTree(rect);
            //mQuadTreeDict.Add(layer, qt);
            plotLayer(layer, color.Name);
            zoomToMarkers();
        }

        public async void exfDownloadCallback(string layer, Color color)
        {
            //Serializer s = new Serializer(mStream);
            LayerAttributes attributes = await client.getDataFile(mSelectedFile);
            mRecordDict = attributes.Data;

            foreach (KeyValuePair<string, Record> entry in mRecordDict)
            {
                entry.Value.Uploaded = true;
                entry.Value.Bucket = currentBucket;
                entry.Value.Key = currentKey + entry.Value.PhotoName + ".jpg";
            }
            max_lat = attributes.MaxLat;
            min_lat = attributes.MinLat;
            max_lng = attributes.MaxLng;
            min_lng = attributes.MinLng;
            PointXY topLeft = new PointXY(min_lng - BUFFER, max_lat + BUFFER);
            PointXY topRight = new PointXY(max_lng + BUFFER, max_lat + BUFFER);
            PointXY bottomRight = new PointXY(max_lng + BUFFER, min_lat - BUFFER);
            PointXY bottomLeft = new PointXY(min_lng - BUFFER, min_lat - BUFFER);
            RectangleXY rect = new RectangleXY(topLeft, topRight, bottomRight, bottomLeft);
            qt = new QuadTree(rect);
            mQuadTreeDict.Add(layer, qt);
            plotLayer(layer, color.Name);
        }

        private void setBucketCallback(string bucket, string key)
        {
            string[] tokens = key.Split('/');
            int length = tokens.Length;
            string newKey = null;
            for (int i = 0; i < tokens.Length -1; i++)
            {
                newKey += tokens[i] + "/";
            }
                currentBucket = bucket;
            currentKey = newKey;
        }

        //private void menuRunGeoTag_Click(object sender, EventArgs e)
        //{
        //    GeotagForm geotagForm = new GeotagForm();
        //    geotagForm.mParent = this;
        //    geotagForm.Show();
        //    geotagForm.writeGeoTag += writeGeoTagCallback;
        //}

        public async void writeGeoTagCallback(string dbPath, string inPath, string outPath, string layer, string color, Boolean allRecords, Boolean zip, string inspector)
        {
            ThreadUtil t = new ThreadUtil();
            t.geoTagComplete += geoTagComplete;
            resetMinMax();
            t.setMinMax += setMinMax;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            if (zip) {
                t.photoReader(inPath, true);
             } else
            {
                t.photoReader(inPath, false);
            }

            ConcurrentDictionary<string, Record> conDict = await t.writeGeotag(inPath, dbPath, outPath, allRecords, inspector);
            mRecordDict = conDict.ToDictionary(pair => pair.Key, pair => pair.Value);
            if (mRecordDict != null && mRecordDict.Count > 0)
            {
                setLayerAttributes();
                PointXY topLeft = new PointXY(min_lng - BUFFER, max_lat + BUFFER);
                PointXY topRight = new PointXY(max_lng + BUFFER, max_lat + BUFFER);
                PointXY bottomRight = new PointXY(max_lng + BUFFER, min_lat - BUFFER);
                PointXY bottomLeft = new PointXY(min_lng - BUFFER, min_lat - BUFFER);
                RectangleXY rect = new RectangleXY(topLeft, topRight, bottomRight, bottomLeft);
                qt = new QuadTree(rect);
                if (qt != null)
                {
                    mQuadTreeDict.Add(layer, qt);
                }
                plotLayer(layer, color);
            }

        }

        public async void readGeoTagCallback(string inPath, string layer, Color color)
        {
            
            ThreadUtil t = new ThreadUtil();
            t.setMinMax += setMinMax;
            t.addRecord += addRecord;
            t.geoTagComplete += geoTagComplete;
            BlockingCollection<string> fileQueue = t.buildQueue(inPath);
            GMapOverlay overlay = new GMapOverlay(layer);
            resetMinMax();
            overlay = await t.readGeoTag(fileQueue, inPath, layer, color.Name);

            setLayerAttributes();
            //zoomToMarkers();
            PointXY topLeft = new PointXY(min_lng - BUFFER, max_lat + BUFFER);
            PointXY topRight = new PointXY(max_lng + BUFFER, max_lat + BUFFER);
            PointXY bottomRight = new PointXY(max_lng + BUFFER, min_lat - BUFFER);
            PointXY bottomLeft = new PointXY(min_lng - BUFFER, min_lat - BUFFER);
            RectangleXY rect = new RectangleXY(topLeft, topRight, bottomRight, bottomLeft);
            qt = new QuadTree(rect);
            mQuadTreeDict.Add(layer, qt);
            refreshUI(overlay, color.Name);
        }

        /// <summary>
        /// Callback which shows messagebox upon completion of geotegging
        /// report 
        /// </summary>
        /// <param name="report - the geotag report "></param>
        public void geoTagComplete(GeotagReport report)
        {
            Task.Run(() =>
            {
                elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    report.Time.Hours, report.Time.Minutes, report.Time.Seconds,
                    report.Time.Milliseconds / 10);
                string title = "Finished";
                string message = "Geotagging complete\n" + report.GeotagCount + " of " + report.TotalRecords + " photos geotagged\n"
                    + "Photos with no record: " + report.NoRecordDictionary.Count + "\n" + "Records with no photo: " + report.NoPhotoDictionary.Count + "\n"
                    + "Number of exceptions during execution: " + report.ErrorDictionary.Count + "\n"
                    + "Time Taken: " + elapsedTime + "\n"
                    + "\n" + "Select OK to save log file";
                MessageBoxButtons buttons = MessageBoxButtons.OKCancel;
                DialogResult result = MessageBox.Show(new Form { TopMost = true }, message, title, buttons);
                
                if (result == DialogResult.OK)
                {
                    LogWriter csv = new LogWriter(report);
                    csv.Save();
                } else
                {
                    Close();
                }
            });
        }

        #endregion

        #region Threading




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

        /// <summary>
        /// Callback from threadutil
        /// </summary>
        /// <param name="photo"></param>
        /// <param name="record"></param>
        private void addRecord(string photo, Record record)
        {
            mConRecordDict.TryAdd(photo, record);
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
                return 10;
            }
            else if (size == 8)
            {
                return 5;
            }
            else if (size == 10)
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
        private void EXIFGeoTagger_Load(object sender, EventArgs e)
        {

        }

        private void ESRIShapefileshpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportDataForm importForm = new ImportDataForm(this, "shape");
            importForm.Show();

            importForm.importData += importShapeCallback;
            
        }

        
        private void TextcsvToolStripMenuItem_Click(object sender, EventArgs e)
        {

            DelimitedText importForm = new DelimitedText();
            importForm.Show();

            importForm.importData += importTextCallback;
            
        }

        private void ExcelDataMenu_Click(object sender, EventArgs e)
        {

        }

        private async void ConnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            client = new AWSConnection();
            List<S3Bucket> buckets;
            if (client != null)
            {
                stopWatch = new Stopwatch();
                stopWatch.Start();
                buckets = await client.requestBuckets();
                Dictionary<string, List<string>> folderDict = await client.getObjectsAsync(buckets);
                List<string> paths = new List<string>();
                foreach (KeyValuePair<string, List<string>> entry in folderDict)
                {
                    paths = entry.Value;
                    paths = paths.Where(path => !paths.Any(p => p != path && p.StartsWith(path))).ToList();

                    rootNode = new TreeNode(entry.Key);
                    if (entry.Value.Count == 0)
                    {
                        treeBuckets.Nodes.Add(rootNode); //empty bucket
                    }
                    else
                    {
                        List<string> newPaths = new List<string>();
                        foreach (string path in paths)
                        {
                            if (path.Contains(".exf"))
                            {
                                newPaths.Add(path); 
                            } else
                            {
                                newPaths.Add(path.Remove(path.Length - 1)); //remove '/' at end of file path
                            }
                            
                        }
                        treeBuckets.Nodes.Add(MakeTreeFromPaths(newPaths, rootNode.Text, '/'));
                    }
                }
            }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            txtConsole.Text = elapsedTime;
        }

        public void getFileObjects(Dictionary<string, List<string>> folderDict, string rootNode)
        {
            List<string> paths = new List<string>();
            try
            {
                //foreach (KeyValuePair<string, List<string>> entry in folderDict)
                //{
                //    string[] dirArr;
                //    paths = entry.Value;
                //    paths = paths.Where(path => !paths.Any(p => p != path && p.StartsWith(path))).ToList();

                //    //rootNode = new TreeNode(entry.Key);
                //    if (entry.Value.Count == 0)
                //    {
                //        treeBuckets.Nodes.Add(rootNode);
                //    }
                //    else
                //    {
                //        List<string> newPaths = new List<string>();
                //        foreach (string path in paths)
                //        {
                //            newPaths.Add(path.Remove(path.Length - 1));
                //        }
                //        treeBuckets.Nodes.Add(MakeTreeFromPaths(newPaths, rootNode, '/'));
                //        //treeBuckets.Nodes.Add(MakeTreeFromPaths(newPaths, rootNode, '/'));
                //    }

                //}
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            txtConsole.Text = elapsedTime;
        }

        public void getFileObjects(Dictionary<string, List<string>> folderDict)
        {         
            List<string> paths = new List<string>();
            try
            {
                foreach (KeyValuePair<string, List<string>> entry in folderDict)
                {
                    string[] dirArr;
                    paths = entry.Value;
                    paths = paths.Where(path => !paths.Any(p => p != path && p.StartsWith(path))).ToList();
                    rootNode = new TreeNode(entry.Key);
                    if (entry.Value.Count == 0)
                    {
                        treeBuckets.Nodes.Add(rootNode);
                    }
                    else
                    {
                        List<string> newPaths = new List<string>();
                        foreach (string path in paths)
                        {
                            newPaths.Add(path.Remove(path.Length - 1));
                        }
                        treeBuckets.Nodes.Add(MakeTreeFromPaths(newPaths, rootNode.Text, '/'));
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            txtConsole.Text = elapsedTime;
        }

        public TreeNode MakeTreeFromPaths(List<string> paths, string rootNodeName = "", char separator = '/')
        {
            var rootNode = new TreeNode(rootNodeName);
            foreach (var path in paths.Where(x => !string.IsNullOrEmpty(x.Trim())))
            {
                var currentNode = rootNode;
                var pathItems = path.Split(separator);
                foreach (var item in pathItems)
                {
                    var tmp = currentNode.Nodes.Cast<TreeNode>().Where(x => x.Text.Equals(item));
                    currentNode = tmp.Count() > 0 ? tmp.Single() : currentNode.Nodes.Add(item);
                }
            }
            return rootNode;
        }

        private void ListView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AWSSecurityForm importForm = new AWSSecurityForm();
            importForm.Show();
        }

        private void tr(object sender, System.Windows.Forms.MouseEventArgs e)
        {

        }

        private void TreeView__NodeMouseDoubleClick(object sender, EventArgs e)
        {
            var menuItem = treeBuckets.SelectedNode; //as MyProject.MenuItem;
          
            if (menuItem != null)
            {
                MessageBox.Show(menuItem.FullPath);
            }
        }

       
        private async void TreeView__NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            client.SetBucket += setBucketCallback;
            if (e.Node.Text.Contains(".exf"))
            {
                mSelectedFile = e.Node.FullPath;           
                ImportDataForm importForm = new ImportDataForm("stream");
                importForm.mParent = this;
                mRecordDict = new Dictionary<string, Record>();        
                importForm.Show();
                importForm.updateData += exfDownloadCallback;                
            }
          
        }

        private void MirrorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenCVManager cv = new OpenCVManager();
            cv.mirrorImge();
        }

        private void ColorCorrectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenCVManager cv = new OpenCVManager();
            cv.ClaheCorrection();
            
        }

        private void EqualiseHistogramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenCVManager cv = new OpenCVManager();
            cv.Equalise();
        }

        private void GammaCorrectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenCVManager cv = new OpenCVManager();
            cv.GammaCorrection();
        }

        private void writeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GeotagForm geotagForm = new GeotagForm();
            geotagForm.mParent = this;
            geotagForm.Show();
            geotagForm.writeGeoTag += writeGeoTagCallback;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void menuRunGeoTag_Click(object sender, EventArgs e)
        {

        }

        private void readToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportDataForm importForm = new ImportDataForm("photos");
            mRecordDict = new Dictionary<string, Record>();
            importForm.mParent = this;
            importForm.Show();
            importForm.importData += readGeoTagCallback;
        }
    } //end class   
} //end namespace
