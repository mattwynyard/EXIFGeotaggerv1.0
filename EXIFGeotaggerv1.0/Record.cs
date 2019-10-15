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
using System.Data;

namespace EXIFGeotagger
{
    [Serializable()]
    public class Record
    {
        public string id;
        public string photo;
        public string photoRename;
        public double latitude;
        public double longitude;
        public double altitude;
        public double bearing;
        public double velocity;
        public int satellites;
        public double pdop;
        public string inspector;
        public DateTime timestamp;
        public bool geomark;
        public bool geotag;
        public bool uploaded;
        public string path;

       
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

        public string PhotoRename
        {
            get
            {
                return photoRename;
            }
            set
            {
                photoRename = value;
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

        public string Bucket { get; set; }

        public string Key { get; set; }

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

        public string Side { get; set; }

        public int TACode { get; set; }

        public int Road { get; set; }

        public int Carriageway { get; set; }

        public int ERP { get; set; }

        public int FaultID { get; set; }

        
    }
}
