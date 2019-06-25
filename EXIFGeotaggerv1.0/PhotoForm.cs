using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Amazon;

namespace EXIFGeotagger
{
    public partial class PhotoForm : Form
    {
        private string mKey;
        private string mBucket;
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.APSoutheast2;
        private AmazonS3Client mClient;
        private Image mImage;

        public PhotoForm(string bucket, string photo)
        {
            InitializeComponent();
            mBucket = bucket;
            mKey = photo;
           

        }

        private void PhotoForm_Load(object sender, EventArgs e)
        {
            mClient = new AmazonS3Client(bucketRegion);
            ReadObjectDataAsync().Wait();
        }

        private async Task ReadObjectDataAsync()
        {
            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = mBucket,
                    Key = mKey
                };
                using (GetObjectResponse response = await mClient.GetObjectAsync(request))
                using (Stream responseStream = response.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    MemoryStream stream = new MemoryStream();

                    responseStream.CopyTo(stream);
                    mImage = Image.FromStream(stream, true);
                    this.pictureBox.Image = mImage;

                }
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered ***. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {

        }
    }
}
