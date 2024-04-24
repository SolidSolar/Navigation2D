using UnityEngine;

namespace Navigation2D.NavMath.VisibilityGraph
{
    public class VisibilityGraphSegment : LineSegment2D
    {
        public bool PartOfVisualGraph = false;
        
        public VisibilityGraphSegment(Vector2 p1, Vector2 p2, PointSortingMode sortingMode) : base(p1, p2, sortingMode)
        {
        }
    }
}