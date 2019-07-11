using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EXIFGeotagger;

namespace ShapeFile
{
    class ShapeWriter
    {

        Dictionary<string, Record> data;
        /// <summary>
        /// Default constructor
        /// </summary>
        public ShapeWriter() {

        }

        public ShapeWriter(LayerAttributes data)
        {
            this.data = data.Data;
        }

        public void readEXF()
        {

        }
    }
}
