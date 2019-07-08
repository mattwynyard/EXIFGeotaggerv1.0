using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using GMap.NET;
using GMap.NET.WindowsForms;

namespace EXIFGeotagger
{
    public struct PointXY
    {
        public double x;
        public double y;

        public PointXY(double _x, double _y)
        {
            x = _x;
            y = _y;
        }

    }

    public struct RectangleXY
    {
        public PointXY topLeft;
        public PointXY topRight;
        public PointXY bottomLeft;
        public PointXY bottomRight;
        public double width;
        public double height;

        public RectangleXY(PointXY _topLeft, double _width, double _height)
        {
            topLeft = _topLeft;
            width = _width;
            height = _height;
            topRight = new PointXY(topLeft.x + width, topLeft.y);
            bottomLeft = new PointXY(topLeft.x, topLeft.y - height);
            bottomRight = new PointXY(topLeft.x + width, topLeft.y - height);
        }

        public RectangleXY(PointXY _topLeft, PointXY _topRight, PointXY _bottomRight, PointXY _bottomLeft)
        {
            topLeft = _topLeft;
            topRight = _topRight;
            bottomLeft = _bottomLeft;
            bottomRight = _bottomRight;
            width = topRight.x - topLeft.x;
            height = topLeft.y - bottomLeft.y;
        }

        public Boolean contains(PointXY point)
        {
            if ((point.x > topLeft.x) && (point.x < (topLeft.x + width)))
            {
                if ((point.y < topLeft.y) && (point.y > (topLeft.y - height)))
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            else
            {
                return false;
            }
        }

        public Boolean contains(GMapMarker marker)
        {
            if ((marker.Position.Lng > topLeft.x) && (marker.Position.Lng < (topLeft.x + width)))
            {
                if ((marker.Position.Lat < topLeft.y) && (marker.Position.Lat > (topLeft.y - height)))
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            else
            {
                return false;
            }
        }

        public Boolean intersects(RectangleXY other)
        {
            if (topRight.y < other.bottomLeft.y || this.bottomLeft.y > other.topRight.y)
            {
                return false;
            }
            if (this.topRight.x < other.bottomLeft.x || this.bottomLeft.y > other.topRight.y)
            {
                return false;
            }
            return true;
        }
    }
    class QuadTree
    {
        // Arbitrary constant to indicate how many elements can be stored in this quad tree node
        const int QT_NODE_CAPACITY = 10;

        // Axis-aligned bounding box stored as a center with half-dimensions
        // to represent the boundaries of this quad tree
        RectangleXY boundary;

        // Points in this quad tree node
        List<GMapMarker> points = new List<GMapMarker>();


        // Children
        QuadTree northWest;
        QuadTree northEast;
        QuadTree southWest;
        QuadTree southEast;

        // Methods

        public QuadTree(RectangleXY rect)
        {
            boundary = rect;
        }

        private void subdivide()
        {
            PointXY ne = new PointXY(boundary.topLeft.x + (boundary.width / 2), boundary.topLeft.y);
            PointXY se = new PointXY(boundary.topLeft.x + (boundary.width / 2), boundary.topLeft.y - (boundary.height / 2));
            PointXY sw = new PointXY(boundary.topLeft.x, boundary.topLeft.y - (boundary.height / 2));

            northWest = new QuadTree(new RectangleXY(boundary.topLeft, boundary.width / 2, boundary.height / 2));
            northEast = new QuadTree(new RectangleXY(ne, boundary.width / 2, boundary.height / 2));
            southEast = new QuadTree(new RectangleXY(se, boundary.width / 2, boundary.height / 2));
            southWest = new QuadTree(new RectangleXY(sw, boundary.width / 2, boundary.height / 2));
        }


        public Boolean insert(GMapMarker marker)
        {
            // Ignore objects that do not belong in this quad tree
            if (!boundary.contains(marker))
            {
                return false;
            }
            // If there is space in this quad tree and if doesn't have subdivisions, add the object here
            if (points.Count < QT_NODE_CAPACITY && northWest == null)
            {
                points.Add(marker);
                return true;
            }
            else // Otherwise, subdivide and then add the point to whichever node will accept it
            {
                if (northWest == null)
                {
                    subdivide();
                }
                //We have to add the points/data contained into this quad array to the new quads if we want that only 
                //the last node holds the data 
                if (northWest.insert(marker))
                {
                    return true;
                }
                else if (northEast.insert(marker))
                {
                    return true;
                }
                else if (southWest.insert(marker))
                {
                    return true;
                }
                else if (southEast.insert(marker))
                {
                    return true;
                }
                // Otherwise, the point cannot be inserted for some unknown reason (this should never happen)
                else
                {
                    return false;
                }
            }
        }
        //public Boolean insert(PointXY p)
        //{
        //    // Ignore objects that do not belong in this quad tree
        //    if (!boundary.contains(p))
        //    {
        //        return false;
        //    }
        //    // If there is space in this quad tree and if doesn't have subdivisions, add the object here
        //    if (points.Count < QT_NODE_CAPACITY && northWest == null)
        //    {
        //        points.Add(p);
        //        return true;
        //    }
        //    else // Otherwise, subdivide and then add the point to whichever node will accept it
        //    {
        //        if (northWest == null)
        //        {
        //            subdivide();
        //        }
        //        //We have to add the points/data contained into this quad array to the new quads if we want that only 
        //        //the last node holds the data 
        //        if (northWest.insert(p))
        //        {
        //            return true;
        //        }
        //        else if (northEast.insert(p))
        //        {
        //            return true;
        //        }
        //        else if (southWest.insert(p))
        //        {
        //            return true;
        //        }
        //        else if (southEast.insert(p))
        //        {
        //            return true;
        //        }
        //        // Otherwise, the point cannot be inserted for some unknown reason (this should never happen)
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //}

        public List<GMapMarker> queryRange(RectangleXY range)
        {
            List<GMapMarker> pointsInRange = new List<GMapMarker>();

            // Automatically abort if the range does not intersect this quad
            if (!boundary.intersects(range))
            {
                return pointsInRange; // empty list
            }
            // Check objects at this quad level
            foreach (var point in points)
            {
                if (range.contains(point))
                {
                    pointsInRange.Add(point);
                }

            }

            // Terminate here, if there are no children
            if (northWest == null)
            {
                return pointsInRange;
            }
            else
            {
                // Otherwise, add the points from the children
                pointsInRange.AddRange(northWest.queryRange(range));
                pointsInRange.AddRange(northEast.queryRange(range));
                pointsInRange.AddRange(southWest.queryRange(range));
                pointsInRange.AddRange(southEast.queryRange(range));
            }
            return pointsInRange;
        }


    }
}


