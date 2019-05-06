using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace EXIFGeotagger
{
    class Serializer
    {
        private Dictionary<string, Record> mData;
        private string mPath;
        private Stream stream;

        public Serializer(String path)
        {
            mPath = path;
           stream = new FileStream(mPath, FileMode.Open, FileAccess.Read);
        }

        public Serializer(Dictionary<string, Record> data)
        {
            mData = data;
        }

        public int serialize(String path)
        {
            Stream stream = null;
            try
            {
                IFormatter formatter = new BinaryFormatter();
                stream = new FileStream(path, FileMode.Create, FileAccess.Write);
                formatter.Serialize(stream, mData);

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

        public Dictionary<string, Record> deserialize()
        {
            IFormatter formatter = new BinaryFormatter();
            Dictionary<string, Record> dict = (Dictionary<string, Record>)formatter.Deserialize(stream);
            return dict;
        }
    }

}
