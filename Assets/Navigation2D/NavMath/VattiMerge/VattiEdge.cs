namespace Navigation2D.NavMath.VattiMerge
{
    public class VattiEdge
    {
        public float BotX;
        public float TopY;
        public float Dx; // the reciprocal of the slope of the edge 
        public bool IsClip;
        public bool IsLeft;
        public bool IsOutput; //does this edge go to output poly
        public VattiPolygon Polygon;
    }
}