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
        public MarkerTag(int id)
        {
            
        }

        public MarkerTag(String color, int id)
        {
            Color = color;
            //ID = id;
        }

        public Record Record { get; set; }

        public int ID { get; set;  }
        public string Color { get; set; }

        public String PhotoName { get; set; }

        public int Size { get; set; }

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
