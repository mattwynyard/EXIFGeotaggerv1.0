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
using EXIFGeotagger;
using System.Threading;

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
        private CancellationTokenSource cts;

        private static readonly Object obj = new Object();

        public event SetBucketDelegate SetBucket;
        public delegate void SetBucketDelegate(string bucket, string key);

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

        private void cancelImport(object sender, EventArgs e)
        {
            if (cts != null)
                cts.Cancel();
        }

        public async Task<Dictionary<string, List<string>>> getObjectsAsync(List<S3Bucket> buckets)
        { 
            Dictionary<string, List<string>> folderDict = new Dictionary<string, List<string>>();
            ProgressForm progressForm = new ProgressForm("Connecting to Amazon...");
            progressForm.Show();
            progressForm.BringToFront();
            progressForm.cancel += cancelImport;
            cts = new CancellationTokenSource();
            var token = cts.Token;
            var progressHandler1 = new Progress<int>(value =>
            {
                progressForm.ProgressValue = value;
                progressForm.Message = "Getting objects from amazon read, please wait... " + value.ToString() + "% completed";

            });
            var progressValue = progressHandler1 as IProgress<int>;
            int i = 0; 
            foreach (S3Bucket bucket in buckets)
            {
                await Task.Factory.StartNew(() => 
                {
                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }
                    List<string> folders = new List<string>();
                    folderDict.Add(bucket.BucketName, folders);
                    ListObjectsRequest request = new ListObjectsRequest();
                    request.BucketName = bucket.BucketName;                 
                    request.Delimiter = ".jpg";
                    try
                    {
                        ListObjectsResponse response;
                        do
                        {
                            response = mClient.ListObjects(request);
                            IEnumerable<S3Object> f = response.S3Objects.Where(x =>
                                                                (x.Key.EndsWith(@"/") && x.Size == 0) || x.Key.Contains(".exf"));
                            
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
                    lock (obj)
                    {
                        i++;
                    }
                    int percent = (i / buckets.Count) * 100;
                    if (progressValue != null)
                    {
                        progressValue.Report(percent);

                    }
                }, cts.Token);            
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

        public async Task<LayerAttributes> getDataFile(string path)
        {
            MemoryStream stream = new MemoryStream();
            string[] tokens = path.Split('\\');
            int length = tokens.Length;
            string key = null;
            string bucket = tokens[0];
            for (int i = 1; i < tokens.Length; i++)
            {
                if (i == tokens.Length - 1)
                {
                    key += tokens[i];
                }
                else
                {
                    key += tokens[i] + "/";
                }
            }          
            await Task.Run(() =>
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = bucket,
                    Key = key
                };
                using (GetObjectResponse response = mClient.GetObject(request))
                {
                    using (Stream responseStream = response.ResponseStream)
                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        responseStream.CopyTo(stream);
                    }
                }                  
            });
            Serializer s = new Serializer(stream);
            LayerAttributes layerAttributes = s.deserialize();
            s = null;
            stream.Close();
            SetBucket(bucket, key);
            return layerAttributes;
        }
        public async Task<Image> getAWSPicture(string bucket, string key)
        {
            //key = "/" + key;
            MemoryStream stream = new MemoryStream();
            await Task.Run(() =>
            {
                
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = bucket,
                    Key = key
                };
                using (GetObjectResponse response = mClient.GetObject(request))
                {
                    using (Stream responseStream = response.ResponseStream)
                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        responseStream.CopyTo(stream);
                    }
                }          
            });
            Image image = Image.FromStream(stream, true);
            stream.Close();
            return image;
        }
    } //end class
}
