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
        private static string icon;
        private static int mSize; //size in pixels of icon
        private static Bitmap bitmap;

        private static Assembly assembly = Assembly.GetExecutingAssembly();

        public MarkerTag()
        {

        }

        public MarkerTag(String color)
        {
            Color = color;

        }

        public static String Color { get; set; }

        public String PhotoName { get; set; }

        public static int Size
        {
            get
            {
                return mSize;
            }
            set
            {
                if (icon != null)
                {
                    icon = null;
                }
                mSize = value;
                icon = ColorTable.ColorTableDict[Color] + "_" + mSize.ToString() + "px.png";
            }
        }

        public static Bitmap getBitmap()
        {
            return bitmap;
        }

        public static void setBitmap()
        {
        
            //string[] resources = this.GetType().Assembly.GetManifestResourceNames();
            Stream stream = assembly.GetManifestResourceStream(icon);
            bitmap = (Bitmap)Image.FromStream(stream);
        }

        public override string ToString()
        {
            return PhotoName;
        }
    }
}
