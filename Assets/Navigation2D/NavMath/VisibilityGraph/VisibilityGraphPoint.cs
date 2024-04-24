using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Navigation2D.NavMath.VisibilityGraph
{
    public class VisibilityGraphPoint
    {
        public Vector2 Point;
        public List<VisibilityGraphSegment> AdjacentSegments;

        public List<VisibilityGraphSegment> GetClockwiseAdjacentSegments(Vector2 origin)
        {
            return AdjacentSegments.Where(x => !_isLeft(origin, Point, x.P1 == Point ? x.P2 : x.P1)).ToList();
        }
        
        public List<VisibilityGraphSegment> GetCounterClockwiseAdjacentSegments(Vector2 origin)
        {
            return AdjacentSegments.Where(x => _isLeft(origin, Point, x.P1 == Point ? x.P2 : x.P1)).ToList();
        }
        
        private bool _isLeft(Vector2 a, Vector2 b, Vector2 c) {
            return (b.x - a.x)*(c.y - a.y) - (b.y - a.y)*(c.x - a.x) > 0;
        }
    }
}