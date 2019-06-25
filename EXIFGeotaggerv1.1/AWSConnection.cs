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
using System.Reflection;
using Amazon.Runtime.CredentialManagement;
using Amazon.Runtime;

namespace Amazon
{

    class AWSConnection
    {
        private string mKey;
        private string mBucket;
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.APSoutheast2;
        private static AmazonS3Client mClient;
        private Image mImage;
        private static readonly string BUCKET = "onsitetest";
        private List<S3Bucket> clientBuckets;

        private static readonly Object obj = new Object();

        //public event BucketDelegate getBuckets;
        //public delegate void BucketDelegate(List<S3Bucket> buckets);

        public AWSConnection()
        {
            try
            {
                var chain = new CredentialProfileStoreChain();
                AWSCredentials awsCredentials;
                if (chain.TryGetAWSCredentials("shared_profile", out awsCredentials))
                {
                    mClient = new AmazonS3Client(awsCredentials, bucketRegion);
                }  
            } 
            catch (TargetInvocationException ex) //TODO exception catch not working
            {
                string title = "Amazon S3 Exception";
                string message = ex.Message;
                MessageBoxButtons buttons = MessageBoxButtons.OK;

                DialogResult result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Error);
                if (result == DialogResult.Yes)
                {
                    //Close();
                }
            }
        }

        public async Task<Dictionary<string, List<string>>> getObjectsAsync()
        { 
            Dictionary<string, List<string>> folderDict = new Dictionary<string, List<string>>();
            foreach (S3Bucket bucket in clientBuckets)
            {
                await Task.Factory.StartNew(() => 
                { 
                    List<string> folders = new List<string>();
                    folderDict.Add(bucket.BucketName, folders);
                    ListObjectsRequest request = new ListObjectsRequest();
                    request.BucketName = bucket.BucketName;
                    try
                    {
                        ListObjectsResponse response;
                        do
                        {
                            response = mClient.ListObjects(request);
                            response.Prefix = bucket.BucketName;
                            response.Delimiter = "/";
                            IEnumerable<S3Object> f = response.S3Objects.Where(x =>
                                                                x.Key.EndsWith(@"/") && x.Size == 0);

                            foreach (S3Object x in f)
                            {
                                folders.Add(x.Key);                                
                            }
                            if (response.IsTruncated)
                            {
                                request.Marker = response.NextMarker;
                            }
                            else
                            {
                                request = null;
                            }
                        } while (request != null);
                        
                    }
                    catch (AmazonS3Exception exAWS)
                    {

                    }
                    catch (Exception ex)
                    {

                    }

                folderDict[bucket.BucketName] = folders;
                });

            }
            return folderDict;
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
                //Console.WriteLine("Error encountered ***. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                //Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }
        }
    }
}
