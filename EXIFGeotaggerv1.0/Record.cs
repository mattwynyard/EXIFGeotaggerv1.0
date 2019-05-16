using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.MapProviders;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Serialization;
//using System.Runtime.Serialization.Formatters.Soap;
using System.Runtime.Serialization.Formatters.Binary;

namespace EXIFGeotagger
{
    [Serializable()]
    class Record
    {
        protected string id;
        protected string photo;
        protected double latitude;
        protected double longitude;
        protected double altitude;
        protected double bearing;
        protected double velocity;
        protected int satellites;
        protected double pdop;
        protected string inspector;
        protected DateTime timestamp;
        protected bool geomark;
        protected bool geotag;
        protected bool uploaded;
        protected string path;

        public Record()
        {
        }

        public Record(string photo)
        {
            this.photo = photo;
            geotag = false;
            uploaded = false;
        }

        public string PhotoName
        {
            get
            {
                return photo;
            }
            set
            {
                photo = value;
            }
        }

        public string Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        public double Latitude
        {
            get
            {
                return latitude;
            }
            set
            {
                latitude = value;
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
                longitude = value;
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
                altitude = value;
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
                bearing = value;
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
                velocity = value;
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
                satellites = value;
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
                pdop = value;
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
                inspector = value;
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
                timestamp = value;
            }
        }
        public Boolean GeoMark
        {
            get
            {
                return geomark;
            }
            set
            {
                geomark = value;
            }
        }

        public Boolean GeoTag { get; set; }

        public Boolean Uploaded { get; set; }

        public string Path { get; set; }
    }
}
