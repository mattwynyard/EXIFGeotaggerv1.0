using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace EXIFGeotagger
{
    /// <summary>
    /// Class to serialize and deserialize exif data from within photo
    /// 
    /// </summary>
    class Serializer
    {
        private Dictionary<string, Record> mData;
        private string mPath;
        private Stream mStream;
        private LayerAttributes mLayer;

        /// <summary>
        /// Contratructor 
        /// </summary>
        /// <param name="path">the path to the exf file to create</param>
        public Serializer(String path)
        {
            mPath = path;
           mStream = new FileStream(mPath, FileMode.OpenOrCreate, FileAccess.Read);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream">the data stream received from AWS</param>
        public Serializer(MemoryStream stream)
        {

            mStream = stream;
        }

        public Serializer(LayerAttributes layer)
        {
            mLayer = layer;
        }


        public int serialize(String path)
        {
            Stream stream = null;
            try
            {
                IFormatter formatter = new BinaryFormatter();
                
                stream = new FileStream(path, FileMode.Create, FileAccess.Write);

                formatter.Serialize(stream, mLayer);

            } catch (IOException ex)
            {
                stream.Close();
                return 0;
            }
            finally
            {
                stream.Close();
            }
           return 1; 
        }

        public LayerAttributes deserialize()
        {
            IFormatter formatter = new BinaryFormatter();
           
            formatter.Binder = new CustomizedBinder();
            mStream.Position = 0;
            LayerAttributes layer = formatter.Deserialize(mStream) as LayerAttributes;

            mStream.Close();
            return layer;
        }

        sealed class CustomizedBinder : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                Type typeToDeserialize = null;
                // Get the current assembly
                string currentAssembly = Assembly.GetExecutingAssembly().FullName;
                // Create the new type and return it
                typeToDeserialize = Type.GetType(string.Format("{0}, {1}", typeName, currentAssembly));
                return typeToDeserialize;
            }

            
        }
    }

}
