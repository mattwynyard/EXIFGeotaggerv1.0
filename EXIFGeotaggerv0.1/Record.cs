using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXIFGeotaggerv0._1
{
    class Record
    {
        String photo;
        double latitude;
        double longitude;
        double altitude;
        double bearing;
        double velocity;
        int satellites;
        double pdop;
        String inspector;
        DateTime timestamp;

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

        public int[] setEXIFCoordinate(String coordinate)
        {
            double coord = 0;
            int multiplier = 10000;
            if (coordinate.Equals("latitude"))
            {
                coord = Math.Abs(this.latitude);
            }

            int d = (int)coord;
            coord -= d;
            coord *= 60;
            int m = (int)coord;
            coord -= m;
            coord *= 60;
            int s = (int)Math.Round(coord * multiplier);

            int[] values = { d, 1, m, 1, s, multiplier };
            return values;
        }

        public double Latitude
        {
            get
            {
                return latitude;
            }
            set
            {
                this.latitude = value;
            }
        }

        public double Longitude
        {
            get
            {
                return longitude;
            }
            set
            {
                this.longitude = value;
            }
        }

        public double Altitude
        {
            get
            {
                return altitude;
            }
            set
            {
                this.altitude = value;
            }
        }

        public double Bearing
        {
            get
            {
                return bearing;
            }
            set
            {
                this.bearing = value;
            }
        }

        public double Velocity
        {
            get
            {
                return velocity;
            }
            set
            {
                this.velocity = value;
            }
        }

        public int Satellites
        {
            get
            {
                return satellites;
            }
            set
            {
                this.satellites = value;
            }
        }

        public double PDop
        {
            get
            {
                return pdop;
            }
            set
            {
                this.pdop = value;
            }
        }

        public String Inspector
        {
            get
            {
                return inspector;
            }
            set
            {
                this.inspector = value;
            }
        }

        public DateTime TimeStamp
        {
            get
            {
                return timestamp;
            }
            set
            {
                this.timestamp = value;
            }
        }

    }


}
