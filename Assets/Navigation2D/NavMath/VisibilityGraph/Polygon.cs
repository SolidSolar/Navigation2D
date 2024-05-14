using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Navigation2D
{
    [Serializable]
    public class Polygon
    {
        [SerializeReference]
        public List<Vertex> Vertices;
        [SerializeReference]
        public Edge[] Edges;
        public float RightmostX;
        public float TopmostZ;
        public float BottommostZ;

        public Polygon(Vector2[] vertices)
        {
            BuildPolygon(vertices);
        }

        public void BuildPolygon(Vector2[] vertices)
        {
            RightmostX = float.MinValue;
            TopmostZ = float.MinValue;
            BottommostZ = float.MaxValue;
            Vertices = new List<Vertex>();
            Edges = new Edge[vertices.Length];

            foreach (var v in vertices)
            {
                if (v.x > RightmostX)
                {
                    RightmostX = v.x;
                }

                if (v.y > TopmostZ)
                {
                    TopmostZ = v.y;
                }
                if (v.y < BottommostZ)
                {
                    BottommostZ = v.y;
                }

                Vertices.Add(new Vertex(v, this));
            }

            for (var i = 1; i < Vertices.Count; i++)
            {
                var e = new Edge(Vertices[i - 1], Vertices[i]);
                Vertices[i - 1].Edge1 = e;
                Vertices[i].Edge2 = e;
                Edges[i - 1] = e;
            }

            var eLast = new Edge(Vertices.Last(), Vertices[0]);
            Vertices.Last().Edge1 = eLast;
            Vertices[0].Edge2 = eLast;

            // Edges are in place, calculate normals
            for (var i = 0; i < Vertices.Count; i++)
            {
                Vertices[i].CalculateNormal();
            }

            Edges[vertices.Length - 1] = eLast;
        }
        
        public bool IntersectsWith(float v1X, float v1Z, float v2X, float v2Z)
        {
            for (int i = 0; i < Edges.Length; i++)
            {
                if (Edges[i].IntersectsWith(v1X, v1Z, v2X, v2Z))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
