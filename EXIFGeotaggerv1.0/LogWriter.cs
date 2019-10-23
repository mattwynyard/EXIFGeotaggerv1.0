using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXIFGeotagger
{
    class LogWriter
    {
        private Dictionary<string, object> noPhoto;
        private Dictionary<string, object> noRecord;
        private Dictionary<string, object> errors;
        private object[] dictionaries;
        private List<string> lines;
        private GeotagReport report;

        public LogWriter()
        {

        }

        public LogWriter(GeotagReport report)
        {
            this.report = report;
            lines = new List<string>();
            noPhoto = report.NoPhotoDictionary.ToDictionary(noPhoto => noPhoto.Key, noPhoto => noPhoto.Value as object);
            noRecord = report.NoRecordDictionary.ToDictionary(noRecord => noRecord.Key, noRecord => noRecord.Value as object);
            errors = report.ErrorDictionary.ToDictionary(errors => errors.Key, errors => errors.Value as object);
        }

        public void Save()
        {
            WriteHeader(report);
            lines.Add("Records with no photo");
            WriteDictionary(noPhoto, "record");
            lines.Add("Photos with no record");
            WriteDictionary(noRecord, "string");
            lines.Add("Exceptions thrown\n");
            WriteDictionary(errors, "exception");
            string path = report.Path + "\\log.txt";
            _Save(path, lines.ToArray());
        }


        private void WriteHeader(GeotagReport report)
        {
            lines.Add(report.ProcessedRecords + " of " + report.TotalRecords + " processed.\n");
            lines.Add(report.GeotagCount + " photos geotagged.\n");
            lines.Add("Time taken: " + report.Time.ToString() + "\n\n");

        }

        private void WriteDictionary(Dictionary<string, object> dict, String type)
        { 

            foreach(var item in dict)
            {
                if (type == "record")
                {
                    Record r = item.Value as Record;
                    lines.Add(r.ToString());
                } else if (type == "exception")
                {
                    Exception r = item.Value as Exception;
                    lines.Add(r.ToString());
                } else
                {
                    lines.Add(item.Value as string);
                }
            }
        }

        private void _Save(string path, string[] lines)
        {
            File.WriteAllLines(path, lines);
        }
    }
}
