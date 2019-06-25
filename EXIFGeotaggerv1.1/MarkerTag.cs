using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Reflection;
using System.IO;

namespace EXIFGeotagger //v0._1
{
    class MarkerTag
    {
        public string path;
        public MarkerTag()
        {

        }

        public MarkerTag(String color, int id)
        {
            Color = color;
            ID = id;
            IsSelected = false;
        }

        public Record Record { get; set; }

        public int ID { get; set; }
        public string Color { get; set; }

        public String PhotoName { get; set; }

        public int Size { get; set; }

        public string Path { get; set; }

        public Boolean IsSelected { get; set; }

        public Dictionary<string, string> Dictionary { get; set; }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.Append(PhotoName + '\n');
            s.Append(Record.TimeStamp.ToString() + '\n');
            s.Append("Accuracy: " + Math.Round(Record.PDop, 1) + " m" + '\n');
            s.Append("Satellites: " + Record.Satellites + '\n');
            return s.ToString();
        }
    }
}
