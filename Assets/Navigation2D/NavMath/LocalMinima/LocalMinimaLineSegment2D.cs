using UnityEngine;

namespace Navigation2D.NavMath.LocalMinima
{
    public class LocalMinimaLineSegment2D : LineSegment2D
    {
        public bool IsLeftToInterior;
        
        public LocalMinimaLineSegment2D(Vector2 p1, Vector2 p2, PointSortingMode sortingMode) : base(p1, p2, sortingMode)
        {
        }
    }
}