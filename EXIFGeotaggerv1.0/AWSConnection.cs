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
        private static readonly string ACCESS_KEY = "AKIA4KM3GYVLI5DLPWA7";
        private static readonly string SECRET_KEY = "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";
        private static readonly string BUCKET = "onsitetest";
        private List<S3Bucket> clientBuckets;

        //public event BucketDelegate getBuckets;
        //public delegate void BucketDelegate(List<S3Bucket> buckets);

        public AWSConnection()
        {
            mClient = new AmazonS3Client(
                    ACCESS_KEY, SECRET_KEY, bucketRegion);

            //if (mClient != null)
            //{
            //    List<S3Bucket> buckets = requestBuckets().Result;
            //    //getObjects.Wait();
            //}
        }

        public void getObjects()
        {
            foreach (S3Bucket bucket in clientBuckets)
            {
                try
                {

                
                var items = mClient.ListObjects(bucket.BucketName);
                }
                catch (AmazonS3Exception ex)
                {

                }
            }
        }
        public async Task<List<S3Bucket>> requestBuckets()
        {
            clientBuckets = new List<S3Bucket>();
            await Task.Run(() => {
                ListBucketsResponse response = mClient.ListBuckets();         
                foreach (S3Bucket b in response.Buckets)
                {
                    string bucket = b.BucketName;
                    DateTime dt = new DateTime(2019, 6, 1);
                    if (b.CreationDate >= dt)
                    {
                        clientBuckets.Add(b);
                    }
                }
                
            });
            return clientBuckets;


        }

        //public async List<S3Bucket> getBuckets()
        //{
        //    List<S3Bucket> buckets = await requestBuckets();
        //    return buckets;
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
