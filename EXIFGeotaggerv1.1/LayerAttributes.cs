using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXIFGeotagger
{
    [Serializable()]
    class LayerAttributes
    {

        public LayerAttributes()
        {

        }

        public double MaxLat { get; set; }

        public double MinLat { get; set; }

        public double MaxLng { get; set; }

        public double MinLng { get; set; }

        public void setMinMax(double maxLat, double minLat, double maxLng, double minLng)
        {
            MaxLat = maxLat;
            MinLat = minLat;
            MaxLng = maxLng;
            MinLng = minLng;
        }

        public Dictionary<string, Record> Data { get; set; }
    }
}
