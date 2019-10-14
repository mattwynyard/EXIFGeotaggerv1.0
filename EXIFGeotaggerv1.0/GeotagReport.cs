using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXIFGeotagger
{
    class GeotagReport
    {

        public GeotagReport()
        {

        }

        public ConcurrentDictionary<string, Record> RecordDictionary { get; set; }

        public ConcurrentDictionary<string, Exception> ErrorDictionary { get; set; }

        public ConcurrentDictionary<string, string> NoRecordDictionary { get; set; }

        public ConcurrentDictionary<string, Record> NoPhotoDictionary { get; set; }

        public int GeotagCount { get; set; }

        public int ProcessedRecords { get; set; }
        public int TotalRecords { get; set; }

        public TimeSpan Time { get; set; }
    }
}
