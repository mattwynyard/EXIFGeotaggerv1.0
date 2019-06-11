using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
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
        public Point[] points;
    }

    public struct BoundingBox
    {
        public double xMin;
        public double yMin;
        public double xMax;
        public double yMax;

    }
    class ShapeReader
    {
        public event ErrorDelegate errorHandler;
        public delegate void ErrorDelegate(string error, string  message);
        String path;
        byte[] shpData;
        public ShapeReader(string path)
        {
            this.path = path;
        }

        public void read()
        {
            double lat, lng;
            ShapeFile s = new ShapeFile();
            shpData = File.ReadAllBytes(path);
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
            b = new byte[8];
            Array.Copy(shpData, offset, b, 0, 8);
            offset += 8;
            double xMin = byteToDouble(b);
            Array.Copy(shpData, offset, b, 0, 8);
            offset += 8;
            double yMin = byteToDouble(b);
            Point pMin = NZTMtoLatLong(yMin, xMin);



            Array.Copy(shpData, offset, b, 0, 8);
            offset += 8;
            double xMax = byteToDouble(b);
            Array.Copy(shpData, offset, b, 0, 8);
            offset += 8;
            double yMax = byteToDouble(b);
            Point pMax = NZTMtoLatLong(yMin, xMin);
            BoundingBox box = new BoundingBox();
            
            box.xMin = pMin.x;
            box.yMin = pMin.y;
            box.xMax = pMax.x;
            box.yMax = pMax.y;
            s.Box = box;

            //body of file
            //int cursor = 0;
            b = new byte[4];
            offset = 100;
            List<MultiPoint> mpointList = new List<MultiPoint>();
            int numRecords = 100000;
            //for (int i = 0; i < numRecords; i++)
            //{
            while (offset < size * 2)
            { 
                Array.Copy(shpData, offset, b, 0, 4);
                int record = byteToInt32(b);
                UInt32 record_uint32 = littleEndiantoBigEndian((UInt32)record);
                record = (int)record_uint32;
                offset += 4;

                Array.Copy(shpData, offset, b, 0, 4);
                offset += 4;
                int length = byteToInt32(b); //16 bit words
                UInt32 length_uint32 = littleEndiantoBigEndian((UInt32)length);
                length = (int)length_uint32;
                Array.Copy(shpData, 108, b, 0, 4);
                int shapeType = byteToInt32(b);
                offset += 4;

                if (shapeType == 8) //multipoint
                {
                    MultiPoint mPoint = new MultiPoint();
                    double[] multiBox = new double[4];
                    //bounding box
                    b = new byte[8];
                    Array.Copy(shpData, offset, b, 0, 8);
                    multiBox[0] = byteToDouble(b); //xMin
                    offset += 8;
                    Array.Copy(shpData, offset, b, 0, 8);
                    multiBox[1] = byteToDouble(b); //yMin
                    offset += 8;
                    Array.Copy(shpData, offset, b, 0, 8);
                    multiBox[2] = byteToDouble(b); //xMax
                    offset += 8;
                    Array.Copy(shpData, offset, b, 0, 8);
                    multiBox[3] = byteToDouble(b); //yMax
                    offset += 8;
                    Point min = NZTMtoLatLong(multiBox[1], multiBox[0]);
                    Point max = NZTMtoLatLong(multiBox[3], multiBox[2]);
                    multiBox[0] = min.x;
                    multiBox[1] = min.y;
                    multiBox[2] = max.x;
                    multiBox[3] = max.y;
                    mPoint.box = multiBox;
                    //num points
                    b = new byte[4];
                    Array.Copy(shpData, offset, b, 0, 4);
                    offset += 4;
                    int numPoints = byteToInt32(b);
                    mPoint.num = numPoints;
                    //Points
                    Point[] points = processMultiPoint(shpData, offset, numPoints);
                    offset += 16 * numPoints;
                    mPoint.points = points;
                    mpointList.Add(mPoint);
                }
                else if(shapeType == 13) //ploylineZ
                {
                    MultiPoint mPoint = new MultiPoint();
                }
            }
            s.MultiPoint = mpointList.ToArray();
        }

        public void readDBF()
        {
            
            string dbPath = Path.GetDirectoryName(path)+ "\\";
            string fileName = Path.GetFileNameWithoutExtension(path) + ".dbf";
            string constr = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + dbPath + ";Extended Properties=dBASE IV";

            using (OleDbConnection con = new OleDbConnection(constr))
            {
                var sql = "SELECT * FROM " + "data.dbf";
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
        private Point[] processMultiPoint(byte[] source, int offset, int numPoints)
        {
            Point[] mp = new Point[numPoints];
            byte[] dest = new byte[numPoints * 8];
            for (int i = 0; i < numPoints; i++)
            {
                Array.Copy(source, offset, dest, 0, 8);
                double x = byteToDouble(dest);
                Array.Copy(source, offset + 8, dest, 0, 8);
                double y = byteToDouble(dest);
                Point p = NZTMtoLatLong(y, x);
                mp[i] = p;
                offset += 16;
            }
            return mp;
        }
        private Point NZTMtoLatLong(double y, double x)
        {
            double lat, lng;
            unsafe
            {
                Transformation.nztm_geod(y, x, &lat, &lng);
            }
            lat = lat * Transformation.rad2deg;
            lng = lng * Transformation.rad2deg;
            Point p = new Point();
            p.x = lng;
            p.y = lat;
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
            errorHandler("Error Reading File", "Invalid Shape File\n" + "Bytes read = " + hex);
        }

    }
}

