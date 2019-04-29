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
        private string icon;
        private static int mSize; //size in pixels of icon
        private static Bitmap bitmap;

        Assembly assembly = Assembly.GetExecutingAssembly();

        public MarkerTag()
        {

        }

        public MarkerTag(String color)
        {
            this.Color = color;
            //this.icon = ColorTable.ColorTableDict[this.Color] + "_" + size.ToString() + "px.png";
        }

        public String Color { get; set; }

        public String PhotoName { get; set; }

        public int Size
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
                this.icon = ColorTable.ColorTableDict[this.Color] + "_" + mSize.ToString() + "px.png";
            }
        }

        public Bitmap getBitmap()
        {
            return bitmap;
        }

        public void setBitmap()
        {
            Stream stream = assembly.GetManifestResourceStream(icon);
            bitmap = (Bitmap)Image.FromStream(stream);
        }

        public override string ToString()
        {
            return PhotoName;
        }
    }
}
