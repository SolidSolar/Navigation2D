using System;
using UnityEngine;

namespace Navigation2D
{
    [Serializable]
    public class Vertex
    {
        [SerializeField]
        public Vector2 Position;
        public float X => _posX;
        public float Y => _posY;

        public float NormalX { get; private set; }
        public float NormalY { get; private set; }

        [SerializeField]
        private float _posX;
        [SerializeField]
        private float _posY;
        [SerializeReference]
        private Edge _edge1;
        [SerializeReference]
        private Edge _edge2;
        
        [SerializeReference]
        public Vertex[] _neighbors = new Vertex[2];
        [SerializeReference]
        private Polygon _ownerPolygon;
        [SerializeField]
        private bool _isConvex;
        
        public Edge Edge1
        {
            get
            {
                return _edge1;
            }
            set
            {
                _edge1 = value;
                _neighbors[0] = Edge1.GetOther(this);
            }
        }

        public Edge Edge2
        {
            get
            {
                return _edge2;
            }
            set
            {
                _edge2 = value;
                _neighbors[1] = Edge2.GetOther(this);
            }
        }

        public Polygon OwnerPolygon => _ownerPolygon;

        public Vertex(Vector2 pos)
        {
            Position = pos;

            _posX = Position.x;
            _posY = Position.y;
        }

        public Vertex(Vector2 pos, Polygon ownerPolygon) : this(pos)
        {
            _ownerPolygon = ownerPolygon;
        }

        public void CalculateNormal()
        {
            var p1From = _neighbors[0].Position - Position;
            var p1To = Position - _neighbors[0].Position;
            var p2From = _neighbors[1].Position - Position;
            var p2To = Position - _neighbors[1].Position;

            _isConvex = Util.CalculateAngle(p2To, p1From) < 180;

            var n = _isConvex
                ? (p1To.normalized + p2To.normalized).normalized
                : (p1From.normalized + p2From.normalized).normalized;

            NormalX = n.x;
            NormalY = n.y;
        }

        public bool IsNeighborWith(Vertex v)
        {
            return _neighbors[0] == v || _neighbors[1] == v;
        }

        public bool IsGoingBetweenNeighbors(float oX, float oZ)
        {
            // Determine if given line segment is between this vertex's neighbors
            var btwn = Util.Left(X, Y, _neighbors[0].X, _neighbors[0].Y, oX, oZ)
                       ^ Util.Left(X, Y, _neighbors[1].X, _neighbors[1].Y, oX, oZ);

            // ... but we want the one going through the polygon
            var dot = (oX - X) * NormalX + (oZ - Y) * NormalY;

            if (_isConvex)
            {
                return btwn && dot < 0;
            }
            else
            {
                return !(btwn && dot > 0);
            }

        }

        public Edge[] GetEdgesOnCwSide(float refX, float refZ) 
        {
            var retVal = new Edge[2];

            var x1 = X - refX;
            var z1 = Y - refZ;

            var x2 = _neighbors[0].X - refX;
            var z2 = _neighbors[0].Y - refZ;

            var x3 = _neighbors[1].X - refX;
            var z3 = _neighbors[1].Y - refZ;

            var cross1 = (x1 * z2) - (x2 * z1);
            var cross2 = (x1 * z3) - (x3 * z1);

            if (cross1 < 0)
            {
                retVal[0] = Edge1;
            }

            if (cross2 < 0)
            {
                retVal[1] = Edge2;
            }

            return retVal;
        }

        public Edge[] GetEdgesOnCCwSide(float refX, float refZ) // Counterclockwise
        {
            var retVal = new Edge[2];

            var x1 = X - refX;
            var z1 = Y - refZ;

            var x2 = _neighbors[0].X - refX;
            var z2 = _neighbors[0].Y - refZ;

            var x3 = _neighbors[1].X - refX;
            var z3 = _neighbors[1].Y - refZ;

            var cross1 = (x1 * z2) - (x2 * z1);
            var cross2 = (x1 * z3) - (x3 * z1);

            if (cross1 > 0)
            {
                retVal[0] = Edge1;
            }

            if (cross2 > 0)
            {
                retVal[1] = Edge2;
            }

            return retVal;
        }

        public override string ToString()
        {
            return Position.ToString();
        }
    }

}
