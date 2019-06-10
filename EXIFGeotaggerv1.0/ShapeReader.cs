using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EXIFGeotagger
{
    class ShapeReader
    {
        public event ErrorDelegate errorHandler;
        public delegate void ErrorDelegate(string error, string  message);
        String path;
        byte[] shpData;
        public ShapeReader(string path)
        {
            this.path = path;
        }

        public void read()
        {
            shpData = File.ReadAllBytes(path);
            byte[] b = new byte[4];
            Array.Copy(shpData, 0, b, 0, 4);
            string hex = ByteToHexString(b);
            if (ByteToHexString(b) != "0x0000270A")
            {
                invalidFile(hex);
            }

             b = new byte[4];
            Array.Copy(shpData, 24, b, 0, 4);
             int size = byteToInt32(b);
            Array.Copy(shpData, 32, b, 0, 4);
            int type = byteToInt32(b);
            b = new byte[8];
            Array.Copy(shpData, 36, b, 0, 8);
            double xMin = ByteToDouble(b);
            Array.Copy(shpData, 44, b, 0, 8);
            double yMin = ByteToDouble(b);



        }

        private double ByteToDouble(byte[] b)
        {
            return BitConverter.ToDouble(b, 0);
        }
        private Int32 byteToInt32(byte[] b)
        {
            return BitConverter.ToInt32(b, 0);

        }

        private string ByteToHexString(byte[] b)
        {
            string hex = BitConverter.ToString(b).Replace("-", "");
            return "0x" + hex;
        }

  
        private void invalidFile(string hex)
        {
            errorHandler("Error Reading File", "Invalid Shape File\n" + "Bytes read = " + hex);
        }

    }
}

