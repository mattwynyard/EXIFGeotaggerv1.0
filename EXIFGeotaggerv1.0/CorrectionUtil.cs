using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXIFGeotagger
{
    static class CorrectionUtil
    {

        private static String mFilename;
        //public CorrectionUtil()
        //{

        //}

        //public CorrectionUtil(String filename)
        //{
        //    mFilename = filename;
        //}

        public static void ClaheCorrection(String inpath)
        {

            Mat src = CvInvoke.Imread(inpath, ImreadModes.AnyColor);
            Mat lab = new Mat();
            CvInvoke.CvtColor(src, lab, ColorConversion.Bgr2Lab);
            src.Dispose();

            VectorOfMat channels = new VectorOfMat();
            CvInvoke.Split(lab, channels);
            Mat dst = new Mat();
            Size size = new Size(8, 8);
            CvInvoke.CLAHE(channels[0], 0.5, size, dst);

            dst.CopyTo(channels[0]);
            dst.Dispose();
            CvInvoke.Merge(channels, lab);
            Mat clahe = new Mat();
            CvInvoke.CvtColor(lab, clahe, ColorConversion.Lab2Bgr);
            lab.Dispose();
            String dir = Path.GetDirectoryName(inpath); 
            String outpath = dir + "\\" + Path.GetFileNameWithoutExtension(inpath) + "_clahe.jpg";
            clahe.Save(outpath);

            clahe.Dispose();
            //clahe.Save("C:\\EXIFGeotagger\\opencv\\contrast_equalise_clahe.jpg");


        }
    }
}
