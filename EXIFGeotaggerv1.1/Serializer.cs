using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace EXIFGeotagger
{
    class Serializer
    {
        private Dictionary<string, Record> mData;
        private string mPath;
        private Stream stream;
        private LayerAttributes mLayer;

        public Serializer(String path)
        {
            mPath = path;
           stream = new FileStream(mPath, FileMode.Open, FileAccess.Read);
        }

        public Serializer(Dictionary<string, Record> data)
        {
            mData = data;
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
                stream = new FileStream(path, FileMode.Append, FileAccess.Write);

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

        //public Dictionary<string, Record> deserialize()
        //{
        //    IFormatter formatter = new BinaryFormatter();
        //    Dictionary<string, Record> dict = (Dictionary<string, Record>)formatter.Deserialize(stream);
        //    return dict;
        //}

        public LayerAttributes deserialize()
        {
            IFormatter formatter = new BinaryFormatter();
            LayerAttributes layer = (LayerAttributes)formatter.Deserialize(stream);
            stream.Close();
            return layer;
        }
    }

}
