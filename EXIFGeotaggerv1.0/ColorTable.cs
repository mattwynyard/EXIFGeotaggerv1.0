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
    public static class ColorTable
    {
        public static readonly IDictionary<string, string> ColorTableDict;


        static ColorTable()
        {
            ColorTableDict = new Dictionary<string, string>()
            {
                {"ffff8080", "EXIFGeotagger.BitMap.OpenCameraPink"},
                {"ffffff80", "EXIFGeotagger.BitMap.OpenCameraLemon"},
                {"ffff8000", "EXIFGeotagger.BitMap.OpenCameraOrange"},
                {"ff80ff80", "EXIFGeotagger.BitMap.OpenCameraLightLime"},
                {"ff00ff80", "EXIFGeotagger.BitMap.OpenCameraSpringGreen"},
                {"ff80ffff", "EXIFGeotagger.BitMap.OpenCameraElectricBlue"},
                {"ff0080ff", "EXIFGeotagger.BitMap.OpenCameraDodgerBlue"},
                {"ffff80c0", "EXIFGeotagger.BitMap.OpenCameraTeaRose"},
                {"ffff80ff", "EXIFGeotagger.BitMap.OpenCameraFuschiaPink"}
            };
        }
    }
}
