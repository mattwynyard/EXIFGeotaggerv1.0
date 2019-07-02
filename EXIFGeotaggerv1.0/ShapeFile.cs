using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeFile
{
    class ESRIShapeFile
    {
        public int Size { get; set; }

        public int Version { get; set; }

        public int ShapeType { get; set; }

        public BoundingBox Box { get; set; }

        public MultiPoint[] MultiPoint { get; set; }

        public Point[] Point { get; set; }

        public PolyLineZ[] PolyLineZ { get; set; }

        public PolyLine[] PolyLine { get; set; }

        public DataTable DataTable { get; set; }

    }
}
