using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;

namespace EXIFGeotagger
{
    class OpenCVManager
    {
        public OpenCVManager()
        {
            
        }

        public void mirrorImge()
        {
            Image image = new Bitmap("C:\\Onsite\\opencvTest\\C1_IMG190413_100908_10422.jpg");
            PropertyItem[] propItems = image.PropertyItems;
            PropertyItem propItemLatRef = image.GetPropertyItem(0x0001);
            PropertyItem propItemLat = image.GetPropertyItem(0x0002);
            PropertyItem propItemLonRef = image.GetPropertyItem(0x0003);
            PropertyItem propItemLon = image.GetPropertyItem(0x0004);
            PropertyItem propItemAltRef = image.GetPropertyItem(0x0005);
            PropertyItem propItemAlt = image.GetPropertyItem(0x0006);
            PropertyItem propItemDateTime = image.GetPropertyItem(0x0132);
            image.Dispose();
            Mat src = CvInvoke.Imread("C:\\Onsite\\opencvTest\\C1_IMG190413_100908_10422.jpg", ImreadModes.AnyColor);
            
            Mat dst = src.Clone();
            CvInvoke.Flip(src, dst, FlipType.Horizontal);
            //dst.Save("C:\\Onsite\\opencvTest\\C1_IMG190413_100908_10422_flip.jpg");
           
            Image<Bgr, Byte> img = dst.ToImage<Bgr, Byte>(); //convert Mat to Image
            src.Dispose();
            dst.Dispose();

            MemoryStream ms = new MemoryStream();
            Image bitmap = img.ToBitmap();
            img.Dispose();
            bitmap.Save(ms, ImageFormat.Jpeg);
            bitmap.Dispose();
            ms.Seek(0, SeekOrigin.Begin);

            Image imgNew = Image.FromStream(ms);

            imgNew.SetPropertyItem(propItemLat);
            imgNew.SetPropertyItem(propItemLon);
            imgNew.SetPropertyItem(propItemLatRef);
            imgNew.SetPropertyItem(propItemLonRef);
            imgNew.SetPropertyItem(propItemAlt);
            imgNew.SetPropertyItem(propItemAltRef);
            //image.SetPropertyItem(propItemDir);
            //image.SetPropertyItem(propItemVel);
            //image.SetPropertyItem(propItemPDop);
            //image.SetPropertyItem(propItemSat);
            //image.SetPropertyItem(propItemDateTime);
            //img.Dispose();
            imgNew.Save("C:\\Onsite\\opencvTest\\C1_IMG190413_100908_10422_flip.jpg");
            imgNew.Dispose();
            //cleanup
            
            
            


        }

        public Mat BGRtoHSV(Mat src)
        {
            Mat dst = new Mat();
            CvInvoke.CvtColor(src, dst, ColorConversion.Bgr2Hsv);
            return dst;
        }

        public void Equalise()
        {
            Mat src = CvInvoke.Imread("C:\\Onsite\\opencvTest\\correction_test.jpg", ImreadModes.AnyColor);
            Mat hsv = new Mat();
            CvInvoke.CvtColor(src, hsv, ColorConversion.Bgr2Lab);

            VectorOfMat channels = new VectorOfMat();
            
            CvInvoke.Split(hsv, channels);

            //for (int i = 0; i < 3; i++)
            //{
                Mat hst = new Mat();
                CvInvoke.EqualizeHist(channels[0], hst);
                hst.CopyTo(channels[0]);
                CvInvoke.Merge(channels, hsv);
            Mat dst = new Mat();
            CvInvoke.CvtColor(hsv, dst, ColorConversion.Lab2Bgr);
            //}


            dst.Save("C:\\Onsite\\opencvTest\\correction_test_equalise.jpg");
        }

        public void ClaheCorrection()
        {

            Mat src = CvInvoke.Imread("C:\\Onsite\\opencvTest\\correction_test.jpg", ImreadModes.AnyColor);
            Mat lab = new Mat();
            CvInvoke.CvtColor(src, lab, ColorConversion.Bgr2Lab);

            VectorOfMat channels = new VectorOfMat();
            CvInvoke.Split(lab, channels);
            Mat dst = new Mat();
            Size size = new Size(8, 8);
            CvInvoke.CLAHE(channels[0], 0.9, size, dst);
    
            dst.CopyTo(channels[0]);
            CvInvoke.Merge(channels, lab);
            Mat clahe = new Mat();
            CvInvoke.CvtColor(lab, clahe, ColorConversion.Lab2Bgr);

            clahe.Save("C:\\Onsite\\opencvTest\\correction_test_clahe.jpg");


        }

        public Image byteToImage(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                return Image.FromStream(ms);
            }
        }
    }
}
