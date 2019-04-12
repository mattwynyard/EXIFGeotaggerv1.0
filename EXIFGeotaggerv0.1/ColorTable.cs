using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Reflection;
using System.IO;

namespace EXIFGeotaggerv0._1
{
    public static class ColorTable
    {
        public static readonly IDictionary<string, string> ColorTableDict;


        static ColorTable()
        {
            ColorTableDict = new Dictionary<string, string>()
            {
                {"ffff8080", "EXIFGeotaggerv0._1.BitMap.OpenCameraPink"},
                {"ffffff80", "EXIFGeotaggerv0._1.BitMap.OpenCameraLemon"},
                { "ffff8000", "EXIFGeotaggerv0._1.BitMap.OpenCameraOrange"}
            };
        }
    }
}
