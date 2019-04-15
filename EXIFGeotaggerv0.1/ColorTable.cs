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
                {"ffff8000", "EXIFGeotaggerv0._1.BitMap.OpenCameraOrange"},
                {"ff80ff80", "EXIFGeotaggerv0._1.BitMap.OpenCameraLightLime"},
                {"ff00ff80", "EXIFGeotaggerv0._1.BitMap.OpenCameraSpringGreen"},
                {"ff80ffff", "EXIFGeotaggerv0._1.BitMap.OpenCameraElectricBlue"},
                {"ff0080ff", "EXIFGeotaggerv0._1.BitMap.OpenCameraDodgerBlue"},
                {"ffff80c0", "EXIFGeotaggerv0._1.BitMap.OpenCameraTeaRose"},
                {"ffff80ff", "EXIFGeotaggerv0._1.BitMap.OpenCameraFuschiaPink"}
            };
        }
    }
}
