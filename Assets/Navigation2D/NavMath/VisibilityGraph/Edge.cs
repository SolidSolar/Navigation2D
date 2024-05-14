using System;
using UnityEngine;

namespace Navigation2D
{
    [Serializable]
    public class Edge : IComparable<Edge>
    {
        // Used when sorting edges according to their distance to a reference point
        public float DistanceToReference;

        [NonSerialized]
        private Vertex _vertex1;
        [NonSerialized]
        private Vertex _vertex2;

        // Caching for fast access
        [SerializeField]
        private Vector2 _v1;
        [SerializeField]
        private Vector2 _v2;
        public Edge(Vertex vertex1, Vertex vertex2)
        {
            _vertex1 = vertex1;
            _vertex2 = vertex2;

            RecacheVertexPositions();   
        }

        public void RecacheVertexPositions()
        {
            _v1.x = _vertex1.X;
            _v1.y = _vertex1.Y;
            _v2.x = _vertex2.X;
            _v2.y = _vertex2.Y;
        }

        public Vertex GetOther(Vertex v)
        {
            if (v.Position == _v1)
            {
                return _vertex2;
            }

            if (v.Position == _v2)
            {
                return _vertex1;
            }
            return null;
        }

        public float DistanceTo(float pX, float pZ)
        {
            return Util.PointLineSegmentDistance(_v1.x, _v1.y, _v2.x, _v2.y, pX, pZ);
        }

        public bool IntersectsWith(float o1X, float o1Z, float o2X, float o2Z)
        {
            // Edge intersection excludes vertices.
            // Nudge positions a little towards each other,
            // so that they won't overlap with this edge's vertices
            o1X += Mathf.Sign(o2X - o1X) * 0.0001f;
            o1Z += Mathf.Sign(o2Z - o1Z) * 0.0001f;
            o2X -= Mathf.Sign(o2X - o1X) * 0.0001f;
            o2Z -= Mathf.Sign(o2Z - o1Z) * 0.0001f;

            // Stolen from: http://yunus.hacettepe.edu.tr/~burkay.genc/courses/bca608/slides/week3.pdf
            return (Util.Left(_v1.x, _v1.y, _v2.x, _v2.y, o1X, o1Z)
                    ^ Util.Left(_v1.x, _v1.y, _v2.x, _v2.y, o2X, o2Z))
                   &&
                   (Util.Left(o1X, o1Z, o2X, o2Z, _v1.x, _v1.y)
                    ^ Util.Left(o1X, o1Z, o2X, o2Z,_v2.x, _v2.y));
        }

        public bool IntersectsWith(float oX, float oZ, float dirX, float dirZ, out float t)
        {
            return Util.RayLineIntersection(oX, oZ, dirX, dirZ, _v1.x, _v1.y, _v2.x, _v2.y,  out t);
        }

        public int CompareTo(Edge other)
        {
            if (other == null)
            {
                return 1;
            }

            return DistanceToReference.CompareTo(other.DistanceToReference);
        }
    }
}