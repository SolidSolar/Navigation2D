using System;
using UnityEngine;

namespace Navigation2D.NavMath.PolygonSelfIntersectionCheck
{
    class PolygonLineSegment2D : LineSegment2D
    {
        // p1 is the leftmost point, for vertical segments it is the bottom point.
        public override string ToString()
        {
            return String.Format("{0}->{1}", P1, P2);
        }

        public bool Contains(Vector2 target, bool inclusive = true)
        {
            if (Line2D.TryJoin(P1, P2, out var line))
            {
                target = line.Project(target);
                Vector2 dir = P2 - P1;
                dir.Normalize();
                float t = Vector2.Dot(dir, target - P1) / Vector2.Dot(dir, P2 - P1);
                return inclusive ? t >= 0 && t <= 1 : t >  float.Epsilon && t < 1 -  float.Epsilon;
            }
            return false;
        }
        
        public bool TryIntersect(PolygonLineSegment2D other, out Vector2 point, bool inclusive = true)
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
        

        public PolygonLineSegment2D(Vector2 p1, Vector2 p2, PointSortingMode sortingMode) : base(p1, p2, sortingMode)
        {
        }
    }
}
