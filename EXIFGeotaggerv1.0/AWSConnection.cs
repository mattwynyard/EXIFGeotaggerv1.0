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


namespace Amazon
{

    class AWSConnection
    {
        private string mKey;
        private string mBucket;
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.APSoutheast2;
        private static AmazonS3Client mClient;
        private Image mImage;

        //public AWSConnection()
        //{
        //    mClient = new AmazonS3Client(
        //            accessKey, secretKey, bucketRegion);
        //}

        public AWSConnection(string bucket, string photo)
        {
            mBucket = bucket;
            mKey = photo;
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
                    //this.pictureBox.Image = mImage;

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
    }
}
