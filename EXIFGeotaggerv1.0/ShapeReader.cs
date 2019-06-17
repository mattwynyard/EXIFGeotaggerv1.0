using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using GMap.NET;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ShapeFile;

namespace ShapeFile
{

    public struct Point
    {
        public double x;
        public double y;
    }

    public struct MultiPoint
    {
        public double[] box;
        public int num;
        public PointLatLng[] points;
    }

    public struct BoundingBox
    {
        public double xMin;
        public double yMin;
        public double xMax;
        public double zMin;
        public double mMin;
        public double yMax;
        public double zMax;
        public double mMax;

    }

    /// <summary>
    ///Box - The Bounding Box for the PolyLine stored in the order Xmin, Ymin, Xmax, Ymax.
    ///NumParts - The number of parts in the PolyLine. NumPoints The total number of points for all parts.
    ///Parts - An array of length NumParts. Stores, for each PolyLine, the index of its first point in the 
    ///         points array.Array indexes are with respect to 0.
    ///Points - An array of length NumPoints. The points for each part in the PolyLine are stored end to end.
    ///         The points for Part 2 follow the points for Part 1, and so on.The parts array holds the array 
    ///         index of the starting point for each part. There is no delimiter in the points array between parts.
    /// </summary>
    public struct PolyLineZ
    {
        public double[] box;
        public int numParts;
        public int numPoints;
        public int[] parts;
        public PointLatLng[] points;
        public double[] zRange;
        public double[] zArray;
        public double[] mRange;
        public double[] mArray;
    }

    public struct PolyLine
    {
        public double[] box;
        public int numParts;
        public int numPoints;
        public int[] parts;
        public PointLatLng[] points;
    }


    
    class ShapeReader
    {
        public event ErrorDelegate errorHandler;
        public delegate void ErrorDelegate(string error, string  message);
        private String path;
        private byte[] shpData;
        private ESRIShapeFile s;
        public ShapeReader(string path)
        {
            this.path = path;
        }

        public void read()
        {
            double lat, lng;
            int errorCount = 0;
            int numRecords = 0;
            int recordNumber;
            s = new ESRIShapeFile();
            shpData = File.ReadAllBytes(path);
            string prj = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path) + ".prj";
            byte[] b = new byte[4];
            int offset = 0;
            Array.Copy(shpData, offset, b, 0, 4);
            string hex = ByteToHexString(b);
            if (ByteToHexString(b) != "0x0000270A")
            {
                invalidFile(hex);
            }
            offset = 24;
            b = new byte[4];
            Array.Copy(shpData, offset, b, 0, 4);
            int size = byteToInt32(b);
            UInt32 size_uint32 = littleEndiantoBigEndian((UInt32)size);
            size = (int)size_uint32; //16 bit words
            s.Size = size * 2; //bytes
            offset = 32;
            Array.Copy(shpData, offset, b, 0, 4);
            Int32 type = byteToInt32(b);
            offset += 4;
            s.ShapeType = type;
            //bounding box
            b = new byte[8];
            Array.Copy(shpData, offset, b, 0, 8); //xMin
            offset += 8;
            double xMin = byteToDouble(b); 
            Array.Copy(shpData, offset, b, 0, 8); //yMin
            offset += 8;
            double yMin = byteToDouble(b);

            Array.Copy(shpData, offset, b, 0, 8); //zMin
            offset += 8;
            double zMin = byteToDouble(b);
            Array.Copy(shpData, offset, b, 0, 8); //mMin
            offset += 8;
            double mMin = byteToDouble(b);

            PointLatLng pMin = NZTMtoLatLong(yMin, xMin);

            Array.Copy(shpData, offset, b, 0, 8);
            offset += 8;
            double xMax = byteToDouble(b);
            Array.Copy(shpData, offset, b, 0, 8);
            offset += 8;
            double yMax = byteToDouble(b);
            PointLatLng pMax = NZTMtoLatLong(yMin, xMin);
            BoundingBox box = new BoundingBox();
            
            box.xMin = pMin.Lng;
            box.yMin = pMin.Lat;
            box.xMax = pMax.Lng;
            box.yMax = pMax.Lat;
            s.Box = box;

            //body of file
            //int cursor = 0;
            b = new byte[4];
            offset = 100;
            List<MultiPoint> mpointList = new List<MultiPoint>();
            List<PolyLineZ> polyZList = new List<PolyLineZ>();

            while (offset < size * 2)
            {
                
                //record numer
                Array.Copy(shpData, offset, b, 0, 4);
                int record = byteToInt32(b);
                UInt32 record_uint32 = littleEndiantoBigEndian((UInt32)record);
                record = (int)record_uint32;
                recordNumber = record;
                offset += 4;

                //data length 16bit words
                Array.Copy(shpData, offset, b, 0, 4);
                offset += 4;
                int length = byteToInt32(b); //16 bit words
                UInt32 length_uint32 = littleEndiantoBigEndian((UInt32)length);
                length = (int)length_uint32;
                //shape type
                Array.Copy(shpData, 108, b, 0, 4);
                int shapeType = byteToInt32(b);
                offset += 4;
                if (length > 2) //handle empty shape
                {
                    if (shapeType == 3) //multipoint
                    {
                        PolyLineZ pl = new PolyLineZ();
                        pl.box = getBoundingBox(shpData, ref offset);
                        b = new byte[4];
                        Array.Copy(shpData, offset, b, 0, 4);
                        offset += 4;
                        pl.numParts = byteToInt32(b); //number of parts
                    }


                    else if (shapeType == 8) //multipoint
                    {
                        MultiPoint mPoint = new MultiPoint();
                        //bounding box 
                        mPoint.box = getBoundingBox(shpData, ref offset);
                        //num points
                        b = new byte[4];
                        Array.Copy(shpData, offset, b, 0, 4);
                        offset += 4;
                        int numPoints = byteToInt32(b);
                        mPoint.num = numPoints;
                        //Points
                        PointLatLng[] points = processMultiPoint(shpData, ref offset, numPoints);
                        //offset += 16 * numPoints;
                        mPoint.points = points;
                        mpointList.Add(mPoint);

                    }
                    else if (shapeType == 13) //polylineZ
                    {
                        PolyLineZ pl = new PolyLineZ();
                        pl.box = getBoundingBox(shpData, ref offset);
                        b = new byte[4];
                        Array.Copy(shpData, offset, b, 0, 4);
                        offset += 4;
                        pl.numParts = byteToInt32(b); //number of parts
                        b = new byte[4];
                        Array.Copy(shpData, offset, b, 0, 4);
                        offset += 4;
                        pl.numPoints = byteToInt32(b); //number of points
                        pl.parts = getnumParts(shpData, ref offset, pl.numParts); //parts
                                                                                  //try
                                                                                  //{
                        pl.points = processMultiPoint(shpData, ref offset, pl.numPoints);
                        b = new byte[8];
                        Array.Copy(shpData, offset, b, 0, 8);
                        offset += 8;
                        double _zMin = byteToDouble(b);
                        b = new byte[8];
                        Array.Copy(shpData, offset, b, 0, 8);
                        offset += 8;
                        double _zMax = byteToDouble(b);

                        pl.zArray = getZArray(shpData, ref offset, pl.numPoints);
                        polyZList.Add(pl);
                        numRecords++;
                    }
                    
                }
                else
                {
                    errorCount++;
                }
            }
            s.MultiPoint = mpointList.ToArray();
            s.PolyLineZ = polyZList.ToArray();
        }

        public ESRIShapeFile getShape()
        {
            return s;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source">the shapefile byte data</param>
        /// <param name="offset">current read position of cursor (passed by reference)</param>
        /// <param name="n">the number of points for all parts</param>
        /// <returns></returns>
        public double[] getZArray(byte[] source, ref int offset, int n)
        {
            double[] zArray = new double[n];
            byte[] b = new byte[8];
            for (int i = 0; i < n; i++)
            {
                Array.Copy(shpData, offset, b, 0, 8);
                zArray[i] = byteToDouble(b);
                offset += 8;
            }
            return zArray;
        }
        public Point[] getPolyLineZPoints(byte[] source, ref int offset, int n)
        {
            Point[] p = new Point[n];
            return p;
        }

        public int[] getnumParts(byte[] source, ref int offset, int n)
        {
            int[] dest = new int[n];
            for (int i = 0; i < n; i++)
            {
                byte[] b = new byte[4];
                Array.Copy(shpData, offset, b, 0, 4);
                dest[i] = byteToInt32(b);
                offset += 4;
            }

            return dest;
        }

        public double[] getBoundingBox(byte[] source, ref int offset)
        {
            byte[] b = new byte[8];
            double[] dest = new double[4];
            Array.Copy(shpData, offset, b, 0, 8);
            dest[0] = byteToDouble(b); //xMin
            offset += 8;
            Array.Copy(shpData, offset, b, 0, 8);
            dest[1] = byteToDouble(b); //yMin
            offset += 8;
            Array.Copy(shpData, offset, b, 0, 8);
            dest[2] = byteToDouble(b); //xMax
            offset += 8;
            Array.Copy(shpData, offset, b, 0, 8);
            dest[3] = byteToDouble(b); //yMax
            offset += 8;
            PointLatLng min = NZTMtoLatLong(dest[1], dest[0]);
            PointLatLng max = NZTMtoLatLong(dest[3], dest[2]);
            dest[0] = min.Lng;
            dest[1] = min.Lat;
            dest[2] = max.Lng;
            dest[3] = max.Lat;
            return dest;
        }

        public void readDBF()
        {
            
            string dbPath = Path.GetDirectoryName(path)+ "\\";
            string fileName = Path.GetFileNameWithoutExtension(path) + ".dbf";
            string constr = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + dbPath + ";Extended Properties=dBASE IV";

            using (OleDbConnection con = new OleDbConnection(constr))
            {
                //var sql = "SELECT * FROM " + "dat_a.dbf";
                var sql = "SELECT * FROM " + dbPath + fileName;
                OleDbCommand cmd = new OleDbCommand(sql, con);
                con.Open();
                DataTable dt = new DataTable();

                OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                da.Fill(dt);

                foreach (DataColumn column in dt.Columns)
                {
                    string data = column.ToString();
                }
                foreach (DataRow row in dt.Rows)
                {
                    string data = row[0].ToString();
                }
            }
        }

        /// <summary>
        /// Reads a number of points (2 doubles) for one record from shapefile
        /// </summary>
        /// <param name="source"> the orginal shapefile</param>
        /// <param name="dest"> temporary byte array</param>
        /// <param name="offset">shapefile read cursor</param>
        /// <param name="numPoints">the number of points to process</param>
        /// <returns> a array of points</returns>
        private PointLatLng[] processMultiPoint(byte[] source, ref int offset, int numPoints)
        {
            PointLatLng[] mp = new PointLatLng[numPoints];
            byte[] dest = new byte[numPoints * 8];
            for (int i = 0; i < numPoints; i++)
            {
                Array.Copy(source, offset, dest, 0, 8);
                double x = byteToDouble(dest);
                Array.Copy(source, offset + 8, dest, 0, 8);
                double y = byteToDouble(dest);
                PointLatLng p = NZTMtoLatLong(y, x);
                mp[i] = p;
                offset += 16;
            }
            return mp;
        }
        private PointLatLng NZTMtoLatLong(double y, double x)
        {
            double lat, lng;
            unsafe
            {
                Transformation.nztm_geod(y, x, &lat, &lng);
            }
            lat = lat * Transformation.rad2deg;
            lng = lng * Transformation.rad2deg;
            PointLatLng p = new PointLatLng();
            p.Lng = lng;
            p.Lat = lat;
            return p;
        }


        private UInt32 littleEndiantoBigEndian(UInt32 x)
        {
            return ((x >> 24) & 0xff) | ((x >> 8) & 0xff00) | ((x << 8) & 0xff0000) | ((x << 24) & 0xff000000);
        }

        private double byteToDouble(byte[] b)
        {
            return BitConverter.ToDouble(b, 0);
        }
        private int byteToInt32(byte[] b)
        {
            return BitConverter.ToInt32(b, 0);

        }

        private string ByteToHexString(byte[] b)
        {
            string hex = BitConverter.ToString(b).Replace("-", "");
            return "0x" + hex;
        }

  
        private void invalidFile(string hex)
        {
            errorHandler("Error Reading File", "Invalid Shape File\n" + "Bytes number read = " + hex);
        }

    }
}

