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
                //TODO make better way of accessing bitmap name/path upon version update i.e changing to v1.1
                {"ffff8080", "EXIFGeotaggerv1.0.BitMap.OpenCameraPink"},
                {"ffffff80", "EXIFGeotaggerv1.0.BitMap.OpenCameraLemon"},
                {"ffff8000", "EXIFGeotaggerv1.0.BitMap.OpenCameraOrange"},
                {"ff80ff80", "EXIFGeotaggerv1.0.BitMap.OpenCameraLightLime"},
                {"ff00ff80", "EXIFGeotaggerv1.0.BitMap.OpenCameraSpringGreen"},
                {"ff80ffff", "EXIFGeotaggerv1.0.BitMap.OpenCameraElectricBlue"},
                {"ff0080ff", "EXIFGeotaggerv1.0.BitMap.OpenCameraDodgerBlue"},
                {"ffff80c0", "EXIFGeotaggerv1.0.BitMap.OpenCameraTeaRose"},
                {"ffff80ff", "EXIFGeotaggerv1.0.BitMap.OpenCameraFuschiaPink"}
            };
        }
    }
}
