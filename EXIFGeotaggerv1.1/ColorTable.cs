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
        public static readonly Dictionary<string, string> ColorTableDict;
        public static readonly Dictionary<string, string> ColorCrossDict;
        private static Assembly assembly = Assembly.GetExecutingAssembly();


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
                {"ffff80ff",  "EXIFGeotagger.BitMap.OpenCameraFuschiaPink" },
                {"Red", "EXIFGeotagger.BitMap.OpenCameraRed" }

            };

            ColorCrossDict = new Dictionary<string, string>()
            {
                {"ffff8080", "EXIFGeotagger.BitMap.Crossff8080"},
                {"ffffff80", "EXIFGeotagger.BitMap.Openffff80"},
                {"ffff8000", "EXIFGeotagger.BitMap.Openff8000"},
                {"ff80ff80", "EXIFGeotagger.BitMap.OpenCameraLightLime80ff80"},
                {"ff00ff80", "EXIFGeotagger.BitMap.OpenCameraSpringGreen00ff80"},
                {"ff80ffff", "EXIFGeotagger.BitMap.OpenCameraElectricBlue80ffff"},
                {"ff0080ff", "EXIFGeotagger.BitMap.OpenCameraDodgerBlue0080ff"},
                {"ffff80c0", "EXIFGeotagger.BitMap.OpenCameraTeaRoseff80c0"},
                {"ffff80ff",  "EXIFGeotagger.BitMap.OpenCameraFuschiaPinkff80ff" },
                {"Red", "EXIFGeotagger.BitMap.OpenCameraRed" }

            };
        }

        public static Bitmap getBitmap(string color, int size)
        {
            Stream stream = null;
            try
            {
                String icon = ColorTableDict[color] + "_" + size.ToString() + "px.png";
                stream = assembly.GetManifestResourceStream(icon);

            }
            catch (ArgumentNullException)
            {

            }
            return (Bitmap)Image.FromStream(stream);
        }

        public static Bitmap getBitmap(Dictionary<string, string> dictionary, string color, int size)
        {
            Stream stream = null;
            try
            {
                String icon = dictionary[color] + "_" + size.ToString() + "px.png";
                stream = assembly.GetManifestResourceStream(icon);
                
            } catch (ArgumentNullException)
            {

            }
            return (Bitmap)Image.FromStream(stream);
        }
    }
}
