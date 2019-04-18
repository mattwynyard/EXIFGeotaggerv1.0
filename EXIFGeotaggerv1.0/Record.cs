using System;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXIFGeotagger //v0._1
{
    class Record : EXIFMarker
    {
        //private String photo;
        //private double latitude;
        //private double longitude;
        //private double altitude;
        //private double bearing;
        //private double velocity;
        //private int satellites;
        //private double pdop;
        //private String inspector;
        //private DateTime timestamp;

        int[] exifLatitude;
        int[] exifLongitude;
        string exifLatitudeRef;
        string exifLongitudeRef;

        public Record()
        {
        }

        public Record(String photo)
        {
            this.photo = photo;
        }

        public PropertyItem getEXIFNumber(PropertyItem item, String type, int precision)
        {
            int value = 0;
            int multiplier = precision;
            if (type.Equals("altitude"))
            {
                value = (int)Math.Round(Math.Abs(this.Altitude) * multiplier);
            } else if (type.Equals("bearing"))
            {
                value = (int)Math.Round(Math.Abs(this.Bearing) * multiplier);
            }
            else if (type.Equals("velocity"))
            {
                value = (int)Math.Round(Math.Abs(this.Velocity) * multiplier);
            }
            else if (type.Equals("pdop"))
            {
                value = (int)Math.Round(Math.Abs(this.PDop) * multiplier);
            }
            int[] values = { value, multiplier };

            byte[] byteArray = new byte[8];
            int offset = 0;
            foreach (var x in values)
            {
                BitConverter.GetBytes(x).CopyTo(byteArray, offset);
                offset += 4;
            }
            item.Value = byteArray;
            return item; 
        }

        public PropertyItem getEXIFInt(PropertyItem item, int number)
        {
            int value = number;
            item.Value = ASCIIEncoding.ASCII.GetBytes(value.ToString() + "\0");
            item.Type = 2;
            return item;
        }

       public PropertyItem getEXIFAltitudeRef(PropertyItem item)
        {
            int value;
            if (this.altitude < 0)
            {
                value = 0;
            } else
            {
                value = 1;
            }
            int[] values = { value };
            byte[] byteArray = new byte[4];
            BitConverter.GetBytes(values[0]).CopyTo(byteArray, 0);
            item.Value = byteArray;
            return item;
        }

        public PropertyItem getEXIFDateTime(PropertyItem item)
        {
            byte[] bytes = ASCIIEncoding.ASCII.GetBytes(this.timestamp.ToString());
            item.Value = bytes;
            return item;
        }

            public PropertyItem getEXIFCoordinate(String coordinate, PropertyItem item )
        {
            double coord = 0;
            int multiplier = 10000;
            if (coordinate.Equals("latitude"))
            {
                coord = Math.Abs(this.latitude);
            } else
            {
                coord = Math.Abs(this.longitude);
            }

            int d = (int)coord;
            coord -= d;
            coord *= 60;
            int m = (int)coord;
            coord -= m;
            coord *= 60;
            int s = (int)Math.Round(coord * multiplier);

            int[] values = { d, 1, m, 1, s, multiplier };

            byte[] byteArray = new byte[24];
            int offset = 0;
            foreach (var value in values)
            {
                BitConverter.GetBytes(value).CopyTo(byteArray, offset);
                offset += 4;
            }
            item.Type = 5;
            item.Value = byteArray; //write bytes
            return item;
        }

        public PropertyItem getEXIFCoordinateRef(String coordinate, PropertyItem item)
        {
            if (coordinate.Equals("latitude"))
            {
                if (this.latitude < 0)
                {
                    item.Value = ASCIIEncoding.ASCII.GetBytes("S\0");
                }
                else
                {
                    item.Value = ASCIIEncoding.ASCII.GetBytes("N\0");
                }
            }
            else
            {
                if (this.longitude < 0)
                {
                    item.Value = ASCIIEncoding.ASCII.GetBytes("W\0");
                }
                else
                {
                    item.Value = ASCIIEncoding.ASCII.GetBytes("E\0");
                }
            }
            return item;
        }

        //public String PhotoName
        //{
        //    get
        //    {
        //        return photo;
        //    }
        //}

        //public double Latitude
        //{
        //    get
        //    {
        //        return latitude;
        //    }
        //    set
        //    {
        //        this.latitude = value;
        //    }
        //}

        //public double Longitude
        //{
        //    get
        //    {
        //        return longitude;
        //    }
        //    set
        //    {
        //        this.longitude = value;
        //    }
        //}

        //public double Altitude
        //{
        //    get
        //    {
        //        return altitude;
        //    }
        //    set
        //    {
        //        this.altitude = value;
        //    }
        //}

        //public double Bearing
        //{
        //    get
        //    {
        //        return bearing;
        //    }
        //    set
        //    {
        //        this.bearing = value;
        //    }
        //}

        //public double Velocity
        //{
        //    get
        //    {
        //        return velocity;
        //    }
        //    set
        //    {
        //        this.velocity = value;
        //    }
        //}

        //public int Satellites
        //{
        //    get
        //    {
        //        return satellites;
        //    }
        //    set
        //    {
        //        this.satellites = value;
        //    }
        //}

        //public double PDop
        //{
        //    get
        //    {
        //        return pdop;
        //    }
        //    set
        //    {
        //        this.pdop = value;
        //    }
        //}

        //public String Inspector
        //{
        //    get
        //    {
        //        return inspector;
        //    }
        //    set
        //    {
        //        this.inspector = value;
        //    }
        //}

        //public DateTime TimeStamp
        //{
        //    get
        //    {
        //        return timestamp;
        //    }
        //    set
        //    {
        //        this.timestamp = value;
        //    }
        //}

    }


}
