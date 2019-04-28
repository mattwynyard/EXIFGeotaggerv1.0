﻿using System;
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
        protected String photo;
        protected double latitude;
        protected double longitude;
        protected double altitude;
        protected double bearing;
        protected double velocity;
        protected int satellites;
        protected double pdop;
        protected String inspector;
        protected DateTime timestamp;
        protected Boolean geomark;

        public Record()
        {
        }

        public Record(String photo)
        {
            this.photo = photo;
        }

        public String PhotoName
        {
            get
            {
                return photo;
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
                this.id = value;
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
        public Boolean GeoMark
        {
            get
            {
                return geomark;
            }
            set
            {
                this.geomark = value;
            }
        }
    }
}
