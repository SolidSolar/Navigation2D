using System;
using Navigation2D.NavMath.PolygonSelfIntersectionCheck;
using UnityEngine;

namespace Navigation2D.NavMath
{
    public class LineSegment2D : IComparable<LineSegment2D>
    {
        public enum Orientation { Colinear, Clockwise, Counterclockwise };
        
        public LineSegment2D(Vector2 p1, Vector2 p2, PointSortingMode sortingMode)
        {
            P1 = p1;
            P2 = p2;
            SortingMode = sortingMode;
            _sortPoints();
        }
        
        public Vector2 P1;
        public Vector2 P2;
        public PointSortingMode SortingMode;

        private void _sortPoints()
        {
            int res1 = Math.Sign(P1.x - P2.x);
            int res2 = Math.Sign(P1.y - P2.y);
            switch (SortingMode)
            {
                case PointSortingMode.DescendingXY:
                    if(res1 == -1)
                        MathfExt.Swap(ref P1, ref P2);
                    else if(res1 == 0 && res2 < 0)
                        MathfExt.Swap(ref P1, ref P2);
                    break;
                case PointSortingMode.DescendingYX:
                    if(res2 == -1)
                        MathfExt.Swap(ref P1, ref P2);
                    else if(res2 == 0 && res1 < 0)
                        MathfExt.Swap(ref P1, ref P2);
                    break;
                case PointSortingMode.IncreasingXY:
                    if(res1 == 1)
                        MathfExt.Swap(ref P1, ref P2);
                    else if(res1 == 0 && res2 > 0)
                        MathfExt.Swap(ref P1, ref P2);
                    break;
                case PointSortingMode.IncreasingYX:
                    if(res2 == 1)
                        MathfExt.Swap(ref P1, ref P2);
                    else if(res2 == 0 && res1 > 0)
                        MathfExt.Swap(ref P1, ref P2);
                    break;
                default:
                    return;
            }
        }
        
        public static Orientation GetOrientation(Vector2 p, Vector2 q, Vector2 r)
        {
            double val = (q.y - p.y) * (r.x - q.x) -
                         (q.x - p.x) * (r.y - q.y);
            
            if (val == 0) return Orientation.Colinear;

            return (val > 0) ? Orientation.Clockwise : Orientation.Counterclockwise;
        }

        public virtual bool TryIntersect(LineSegment2D other, out Vector2 point, bool inclusive = true)
        {
            point = Vector2.zero;
            if (Math.Abs(P1.x - other.P2.x) <  float.Epsilon && Math.Abs(P1.y - other.P2.y) <  float.Epsilon ||
                Math.Abs(P2.x - other.P1.x) <  float.Epsilon && Math.Abs(P2.y - other.P1.y) <  float.Epsilon ||
                Math.Abs(P1.x - other.P1.x) <  float.Epsilon && Math.Abs(P1.y - other.P1.y) <  float.Epsilon ||
                Math.Abs(P2.x - other.P2.x) <  float.Epsilon && Math.Abs(P2.y - other.P2.y) <  float.Epsilon)
            {
                return false;
            }

            
            if (Line2D.TryJoin(P1, P2, out Line2D thisLine) 
                && Line2D.TryJoin(other.P1, other.P2, out Line2D otherLine))
            {
                if (Line2D.TryMeet(thisLine, otherLine, out point))
                {
                    bool contains = Contains(point, inclusive) && other.Contains(point, inclusive);
                    return contains;
                }
            }
            
            return false;
        }

        public bool Contains(Vector2 target, bool inclusive = true)
        {
            var Epsilon = 0.0000001f;
            
            if (Line2D.TryJoin(P1, P2, out var line))
            {
                target = line.Project(target);
                Vector2 dir = P2 - P1;
                dir.Normalize();
                float t = Vector2.Dot(dir, target - P1) / Vector2.Dot(dir, P2 - P1);
                return inclusive ? t >= 0 && t <= 1 : t >  Epsilon && t < 1 -  Epsilon;
            }
            return false;
        }
        
        public virtual int CompareTo(LineSegment2D other)
        {
            if( this == other ) 
            {
                return 0;
            }

            var res = 0;
            if (P1.x < other.P1.x)
            {
                return -other.CompareTo(this);
            }
            if (P1.x == other.P1.x)
            {
                res = NavMath.ComparePoints(P1, other.P1, PointSortingMode.IncreasingXY);
                if (res != 0)
                {
                    return res;
                }
                else
                {
                    // same start points, compare end points
                    switch (GetOrientation(other.P1, other.P2, P2))
                    {
                        case Orientation.Clockwise:
                            return -1;
                        case Orientation.Counterclockwise:
                            return 1;
                        default:
                            res = NavMath.ComparePoints(P2, other.P2, PointSortingMode.IncreasingXY);
                            break;
                    }
                }
            }
            else
            {
                switch (GetOrientation(other.P1, other.P2, P1))
                {
                    case Orientation.Clockwise:
                        return -1;
                    case Orientation.Counterclockwise:
                        return 1;
                }
            }

            return res != 0? res : -1;
        }

        public override string ToString()
        {
            return P1 + " " + P2;
        }
    }
    
    public enum PointSortingMode
    {
        DescendingXY,
        DescendingYX,
        IncreasingXY,
        IncreasingYX,
        NoSorting
    }
}