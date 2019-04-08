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

        string folderPath = "C:\\Road Inspection\\Thumbnails";
        string geoRefPath = "C:\\Road Inspection\\GeoRef";

        Assembly myAssembly;
        Stream myStream;
        Bitmap bmpMarker;
        GMapOverlay markers;

        public EXIFGeoTagger()
        {
            InitializeComponent();
            mRecordDict = new Dictionary<string, Record>();
         
        }

        private void fileMenuOpen_Click(object sender, ToolStripItemClickedEventArgs e)
        {
            btnBrowse_Click(sender, e);
        }



        private void btnMarkers_Click (object sender, EventArgs e)
        {
           
            markers = new GMapOverlay("markers");
          
            markers = buildMarker("EXIFGeotaggerv0._1.OpenCamera8px.png");
           
            gMap.Overlays.Add(markers);
            txtConsole.Clear();
            txtConsole.AppendText("Built markers...");

        }

        private GMapOverlay buildMarker(String icon)
        {
            markers = new GMapOverlay("markers");
            myAssembly = Assembly.GetExecutingAssembly();
            myStream = myAssembly.GetManifestResourceStream(icon);
            bmpMarker = (Bitmap)Image.FromStream(myStream);
            if (mRecordDict != null)
            {
                foreach (KeyValuePair<string, Record> record in mRecordDict)
                {
                    Double lat = record.Value.Latitude;
                    Double lon = record.Value.Longitude;
                    GMapMarker marker = new GMarkerGoogle(new PointLatLng(lat, lon), bmpMarker);
                    marker.Tag = record.Value.PhotoName + "\n" + record.Value.TimeStamp;
                    //marker = new GMarkerGoogle(new PointLatLng(lat, lon), GMarkerCross.DefaultPen);
                    markers.Markers.Add(marker);
                }
            }
            return markers;
        }

        //private void gMap_Scroll((object sender, EventArgs e) {

        //}


        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "mdb files|*.mdb";
            openFileDialog.FilterIndex = 2;
            openFileDialog.Title = "Browse Files";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.DefaultExt = "mdb";
            openFileDialog.ShowDialog();

            //mRecords = new ArrayList();

            this.txtFilePath.Text = openFileDialog.FileName;
            mDBPath = openFileDialog.FileName;

            
            mFiles = Directory.GetFiles(folderPath);
            foreach (string file in mFiles)
            {
                string path = Path.GetFileNameWithoutExtension(file); //filename without extension of photo
                Record r = new Record(path);
                mRecordDict.Add(path, r);
                txtConsole.AppendText(mRecordDict.Count + " added to dictonary");
                txtConsole.Clear();
            }
            //txtConsole.AppendText("Exctracted " + mFiles.Length + " files" + Environment.NewLine);
            //txtConsole.AppendText("Built dictionary..." + Environment.NewLine);
            //txtConsole.AppendText(mRecordDict.Count + " keys added" + Environment.NewLine);

            string connectionString = string.Format("Provider={0}; Data Source={1}; Jet OLEDB:Engine Type={2}",
                                "Microsoft.Jet.OLEDB.4.0", mDBPath, 5);

            OleDbConnection connection = new OleDbConnection(connectionString);
            string connectionStr = connection.ConnectionString;

            string strSQL = "SELECT * FROM PhotoList;";

            OleDbCommand command = new OleDbCommand(strSQL, connection);
            // Open the connection and execute the select command.  
            int recordCount = 0;
            try
            {
                // Open connecton  
                connection.Open();
                String[] photoPath = new String[mFiles.Length];
                
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
                        buildDictionary(i, photo, row);
                        i++;
                    }
                    recordCount = i;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            txtConsole.AppendText("Exctracted " + mFiles.Length + " photos" + Environment.NewLine);
            txtConsole.AppendText("Built dictionary..." + Environment.NewLine);
            txtConsole.AppendText(mRecordDict.Count + " keys added" + Environment.NewLine);
            txtConsole.AppendText(recordCount + " keys populated" + Environment.NewLine);
         

        }

        private void buildDictionary(int i, String photo, Object[] row)
        {
            try
            {
                Record r = mRecordDict[photo];
                r.Latitude = (double)row[2];
                r.Longitude = (double)row[3];
                r.Altitude = (double)row[4];
                r.Bearing = Convert.ToDouble(row[5]);
                r.Velocity = Convert.ToDouble(row[6]);
                r.Satellites = Convert.ToInt32(row[7]);
                r.PDop = Convert.ToDouble(row[8]);
                r.Inspector = Convert.ToString(row[9]);
                r.TimeStamp = Convert.ToDateTime(row[11]);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        private void btnGeotag_Click(object sender, EventArgs e)
        {
            txtConsole.Clear();
            txtConsole.AppendText(Environment.NewLine);
            txtConsole.Clear();

            var watch = System.Diagnostics.Stopwatch.StartNew();
            foreach (string filePath in mFiles)
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

                Record r = mRecordDict[Path.GetFileNameWithoutExtension(filePath)];

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

                image.Save("C:\\androidapp\\GeoRef" + "\\"+ Path.GetFileName(filePath));
                image.Dispose();
                txtConsole.AppendText("Geotagged: " + Path.GetFileName(filePath));
                txtConsole.Clear();

            }
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            txtConsole.AppendText("Geotag Finished in: " + elapsedMs + " ms" + Environment.NewLine);
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
            //marker.Position.ToString
            MessageBox.Show(id);
        }

        private void gMap_OnMapZoomChanged()
        {
            txtConsole.Clear();
            txtConsole.AppendText(gMap.Zoom.ToString());
            if((int)gMap.Zoom < 12)
            {
                markers = buildMarker("EXIFGeotaggerv0._1.OpenCamera4px.png");
          
            } else if ((int)gMap.Zoom < 16 && (int)gMap.Zoom >= 12)
            {
                markers = buildMarker("EXIFGeotaggerv0._1.OpenCamera8px.png");

            } else if ((int)gMap.Zoom < 18 && (int)gMap.Zoom >= 16)
            {
                markers = buildMarker("EXIFGeotaggerv0._1.OpenCamera12px.png");

            } else if ((int) gMap.Zoom < 20 && (int) gMap.Zoom >= 18)
            {
                markers = buildMarker("EXIFGeotaggerv0._1.OpenCamera16px.png");
            } else
            {
                markers = buildMarker("EXIFGeotaggerv0._1.OpenCamera24px.png");
            }
            gMap.Overlays.Clear();
            gMap.Overlays.Add(markers);

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
    }
}
