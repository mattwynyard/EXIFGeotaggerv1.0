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

        ///// <summary>
        ///// Checks if point is with a rectangel
        ///// </summary>
        ///// <param name="point"> the point to check</param>
        ///// <returns>true if point is with in rectangle</returns>
        //public Boolean contains(PointXY point)
        //{
        //    if ((point.x > topLeft.x) && (point.x < (topLeft.x + width)))
        //    {
        //        if ((point.y < topLeft.y) && (point.y > (topLeft.y - height)))
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}


        /// <summary>
        /// Checks is map marker is within a rectangle
        /// </summary>
        /// <param name="marker">the marker to check</param>
        /// <returns>treu if point is within rectangle</returns>
        public Boolean contains(GMapMarker marker)
        {
            if ((marker.Position.Lng >= topLeft.x) && (marker.Position.Lng <= (topLeft.x + width)))
            {
                if ((marker.Position.Lat <= topLeft.y) && (marker.Position.Lat >= (topLeft.y - height)))
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

        /// <summary>
        /// Checks if a rectangle intersects a rectangle
        /// </summary>
        /// <param name="other">the second rectangle</param>
        /// <returns>true if rectangles intersect</returns>
        public Boolean intersects(RectangleXY other)
        {
            if (topRight.y < other.bottomLeft.y || bottomLeft.y > other.topRight.y)
            {
                return false;
            }
            if (topRight.x < other.bottomLeft.x || bottomLeft.x > other.topRight.x)
            {
                return false;
            }
            return true;
        }
    }
    class QuadTree
    {
        // Arbitrary constant to indicate how many elements can be stored in this quad tree node
        const int QT_NODE_CAPACITY = 4;

        // Axis-aligned bounding box stored as a center with half-dimensions
        // to represent the boundaries of this quad tree
        RectangleXY boundary;

        // Points in this quad tree node
        List<GMapMarker> points = new List<GMapMarker>();


        // Children
        public QuadTree northWest;
        public QuadTree northEast;
        public QuadTree southWest;
        public QuadTree southEast;

        // Methods

        public QuadTree(RectangleXY rect)
        {
            boundary = rect;
        }


        private void subdivide(QuadTree parent)
        {
            PointXY ne = new PointXY(parent.boundary.topLeft.x + (parent.boundary.width / 2.0), parent.boundary.topLeft.y);
            PointXY se = new PointXY(parent.boundary.topLeft.x + (parent.boundary.width / 2.0), parent.boundary.topLeft.y - (parent.boundary.height / 2.0));
            PointXY sw = new PointXY(parent.boundary.topLeft.x, parent.boundary.topLeft.y - (parent.boundary.height / 2.0));

            parent.northWest = new QuadTree(new RectangleXY(parent.boundary.topLeft, parent.boundary.width / 2.0, parent.boundary.height / 2.0));
            parent.northEast = new QuadTree(new RectangleXY(ne, boundary.width / 2.0, parent.boundary.height / 2.0));
            parent.southEast = new QuadTree(new RectangleXY(se, parent.boundary.width / 2.0, parent.boundary.height / 2.0));
            parent.southWest = new QuadTree(new RectangleXY(sw, parent.boundary.width / 2.0, parent.boundary.height / 2.0));
        }


        public int count()
        {
            int count = points.Count();
            if (northWest == null) //there are no children terminate
            {
                return count;
            }
            else
            {
                count += countRecursive(northEast);
                count += countRecursive(northWest);
                count += countRecursive(southEast);
                count += countRecursive(southWest);
            }
            return count;
        }

        public int countRecursive(QuadTree parent)
        {
            int count = parent.points.Count();
            if (parent.northWest != null)
            {
                count += countRecursive(parent.northEast);
                count += countRecursive(parent.northWest);
                count += countRecursive(parent.southEast);
                count += countRecursive(parent.southWest);
            }
            return count;
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
                    subdivide(this);
                }
                //We have to add the points/data contained into this quad array to the new quads if we want that only 
                //the last node holds the data 
                if (northWest.insert(marker))
                {
                    return true;
                }
                if (northEast.insert(marker))
                {
                    return true;
                }
                if (southWest.insert(marker))
                {
                    return true;
                }
                if (southEast.insert(marker))
                {
                    return true;
                }
                // Otherwise, the point cannot be inserted for some unknown reason (this should never happen)
                return false;
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
                pointsInRange.AddRange(queryRangeRecursive(northWest, range));
                pointsInRange.AddRange(queryRangeRecursive(northEast, range));
                pointsInRange.AddRange(queryRangeRecursive(southEast, range));
                pointsInRange.AddRange(queryRangeRecursive(southWest, range));
            }
            return pointsInRange;
        }

        public List<GMapMarker> queryRangeRecursive(QuadTree parent, RectangleXY range)
        {
            List<GMapMarker> pointsInRange = new List<GMapMarker>();

            // Automatically abort if the range does not intersect this quad
            if (!parent.boundary.intersects(range))
            {
                return pointsInRange; // empty list
            }
            // Check objects at this quad level
            foreach (var point in parent.points)
            {
                if (range.contains(point))
                {
                    pointsInRange.Add(point);
                }

            }
            if (parent.northWest == null)
            {
                return pointsInRange;
            }
            else
            {
                // Otherwise, add the points from the children
                pointsInRange.AddRange(queryRangeRecursive(parent.northWest, range));
                pointsInRange.AddRange(queryRangeRecursive(parent.northEast, range));
                pointsInRange.AddRange(queryRangeRecursive(parent.southEast, range));
                pointsInRange.AddRange(queryRangeRecursive(parent.southWest, range));
            }
            return pointsInRange;
        }
    }
}


