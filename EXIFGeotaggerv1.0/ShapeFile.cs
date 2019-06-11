using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeFile
{
    class ShapeFile
    {
        public int Size { get; set; }

        public int Version { get; set; }

        public int ShapeType { get; set; }

        public BoundingBox Box { get; set; }

        public MultiPoint[] MultiPoint { get; set; }

}
}
