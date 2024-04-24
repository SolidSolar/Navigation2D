using UnityEngine;

namespace Navigation2D.NavMath.PolygonClipping
{
    public class ClipVertex
    {
        public Vector2 coordinate;
        public bool newlyAdded = false;
        public ClipVertex next;
        public ClipVertex prev;

        public ClipVertex nextPoly;

        public bool isIntersection = false;
        
        //0 - null status, 1 - true, 2 - false
        public int isEntry = 0;
        
        public VertexType type = VertexType.None;
        public OverlapType overlapType = OverlapType.None;
        public IntersectionType intersectionType = IntersectionType.None;
        public Vector2 intersectionPoint;
        
        public ClipVertex neighbor;

        public bool isTakenByFinalPolygon;

        public ClipVertex(Vector2 coordinate)
        {
            this.coordinate = coordinate;
        }
    }

    public enum VertexType
    {
        Crossing,
        Bouncing,
        None
    }

    public enum OverlapType
    {
        LeftOn,
        RightOn,
        OnOn,
        OnLeft,
        OnRight,
        None
    }
}