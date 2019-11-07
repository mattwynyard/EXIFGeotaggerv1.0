using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXIFGeotagger
{
    [Serializable()]
    class LayerAttributes
    {

        Dictionary<string, Record> mData;
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

        public ConcurrentDictionary<string, Record> getData()
        {

            ConcurrentDictionary<string, Record> dict = new ConcurrentDictionary<string, Record>(mData);
            return dict;
        }

        public void setData(ConcurrentDictionary<string, Record> dict)
        {
            mData = dict.ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}
