using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Navigation2D.NavMath;
using Navigation2D.NavMath.PolygonSelfIntersectionCheck;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Navigation2D.Data.Utility
{
    public static class NavUtility
    {
        public static List<Shape2D> ConvertToPolygonShapes(Collider2D collider)
        {
            List<Shape2D> shapeResult = new List<Shape2D>();
            List<Vector2> pointsResult = new List<Vector2>();
            if (collider is CapsuleCollider2D)
            {
                Shape2D shape = new Shape2D();
                CapsuleCollider2D c = (CapsuleCollider2D) collider;
                pointsResult.Add(new Vector2(-c.size.x/2, -c.size.y/2));
                pointsResult.Add(new Vector2(-c.size.x/2, c.size.y/2));
                pointsResult.Add(new Vector2(c.size.x/2, c.size.y/2));
                pointsResult.Add(new Vector2(c.size.x/2, -c.size.y/2));
                shape.Points = new List<Vector2>(pointsResult);
                shape.Center = (Vector3)c.offset + collider.transform.position;
                shapeResult.Add(shape);
            }
            if (collider is PolygonCollider2D)
            {
                Shape2D shape = new Shape2D();
                PolygonCollider2D c = (PolygonCollider2D) collider;

                if (PolygonSelfIntersectionCheck.HasIntersections(
                    c.points.ToList()))
                {
                    Debug.LogWarning($"Polygon collider of gameObject {collider.name} is self-intersecting, no shape generated");
                    return new List<Shape2D>();
                }
                
                var mesh = c.CreateMesh(false, true);
                Edge[] edges = BuildManifoldEdges(mesh);
                var u = new List<Vector2>();
                var v = edges[0].vertexIndex[0];
                u.Add(mesh.vertices[v]);
                var cringe = edges.ToList();
                cringe.Remove(edges[0]);
                int a = 0;
                
                while (cringe.Count>0)
                {
                    for (int i = 0; i< cringe.Count; i++)
                    {
                        if (cringe[i].vertexIndex[0] == v)
                        {
                            v = cringe[i].vertexIndex[1];
                            u.Add(mesh.vertices[v]);
                            cringe.Remove(cringe[i]);
                            i = cringe.Count * 2;
                        }
                        if (cringe[i].vertexIndex[1] == v)
                        {
                            v = cringe[i].vertexIndex[0];
                            u.Add(mesh.vertices[v]);
                            cringe.Remove(cringe[i]);
                            i = cringe.Count * 2;
                        }
                    }
    
                    a++;
                    if(a>200)
                        break;
                }
    
                pointsResult = c.points.ToList();
                shape.Points = new List<Vector2>(pointsResult);
                shape.Center = (Vector3)c.offset + collider.transform.position;
                shapeResult.Add(shape);
            }
            if (collider is BoxCollider2D)
            {
                Shape2D shape = new Shape2D();
                BoxCollider2D c = (BoxCollider2D) collider;
                pointsResult.Add(new Vector2(-c.size.x/2, -c.size.y/2));
                pointsResult.Add(new Vector2(-c.size.x/2, c.size.y/2));
                pointsResult.Add(new Vector2(c.size.x/2, c.size.y/2));
                pointsResult.Add(new Vector2(c.size.x/2, -c.size.y/2));
                shape.Points = new List<Vector2>(pointsResult);
                shape.Center = (Vector3)c.offset + collider.transform.position;
                shapeResult.Add(shape);
            }
            if (collider is TilemapCollider2D)
            {
                Tilemap tilemap = collider.GetComponent<Tilemap>();
                var cellSize = tilemap.cellSize/2f;
                foreach (var position in tilemap.cellBounds.allPositionsWithin) {
                    if (!tilemap.HasTile(position)) {
                        continue;
                    }
                    pointsResult.Clear();
                    Shape2D shape = new Shape2D();
                    Vector3Int cellPosition = tilemap.WorldToCell(position);
                    Vector3 v = position;
                    v = new Vector3(v.x *cellSize.x * 2, v.y * cellSize.y * 2, 0);
                    Vector3 _base =  new Vector3(tilemap.tileAnchor.x *cellSize.x*2 , tilemap.tileAnchor.y*cellSize.y*2, 0);
                    Vector3 p = _base + new Vector3(cellSize.x, cellSize.y, 0);
                    pointsResult.Add(p);
                    p = _base + new Vector3(cellSize.x, -cellSize.y, 0);
                    pointsResult.Add(p);
                    p = _base + new Vector3(-cellSize.x, -cellSize.y, 0);
                    pointsResult.Add(p);
                    p = _base + new Vector3(-cellSize.x, cellSize.y, 0);
                    pointsResult.Add(p);
                    shape.Points = new List<Vector2>(pointsResult);
                    shape.Center = v;
                    shapeResult.Add(shape);
                }
            }
            return shapeResult;
        }
        
        public static Edge[] BuildManifoldEdges(Mesh mesh)
        {
            // Build a edge list for all unique edges in the mesh
            Edge[] edges = BuildEdges(mesh.vertexCount, mesh.triangles);
    
            // We only want edges that connect to a single triangle
            ArrayList culledEdges = new ArrayList();
            foreach (Edge edge in edges)
            {
                if (edge.faceIndex[0] == edge.faceIndex[1])
                {
                    culledEdges.Add(edge);
                }
            }
    
            return culledEdges.ToArray(typeof(Edge)) as Edge[];
        }
 
        /// Builds an array of unique edges
        /// This requires that your mesh has all vertices welded. However on import, Unity has to split
        /// vertices at uv seams and normal seams. Thus for a mesh with seams in your mesh you
        /// will get two edges adjoining one triangle.
        /// Often this is not a problem but you can fix it by welding vertices 
        /// and passing in the triangle array of the welded vertices.
        public static Edge[] BuildEdges(int vertexCount, int[] triangleArray)
        {
            int maxEdgeCount = triangleArray.Length;
            int[] firstEdge = new int[vertexCount + maxEdgeCount];
            int nextEdge = vertexCount;
            int triangleCount = triangleArray.Length / 3;
    
            for (int a = 0; a < vertexCount; a++)
                firstEdge[a] = -1;
    
            // First pass over all triangles. This finds all the edges satisfying the
            // condition that the first vertex index is less than the second vertex index
            // when the direction from the first vertex to the second vertex represents
            // a counterclockwise winding around the triangle to which the edge belongs.
            // For each edge found, the edge index is stored in a linked list of edges
            // belonging to the lower-numbered vertex index i. This allows us to quickly
            // find an edge in the second pass whose higher-numbered vertex index is i.
            Edge[] edgeArray = new Edge[maxEdgeCount];
    
            int edgeCount = 0;
            for (int a = 0; a < triangleCount; a++)
            {
                int i1 = triangleArray[a * 3 + 2];
                for (int b = 0; b < 3; b++)
                {
                    int i2 = triangleArray[a * 3 + b];
                    if (i1 < i2)
                    {
                        Edge newEdge = new Edge();
                        newEdge.vertexIndex[0] = i1;
                        newEdge.vertexIndex[1] = i2;
                        newEdge.faceIndex[0] = a;
                        newEdge.faceIndex[1] = a;
                        edgeArray[edgeCount] = newEdge;
    
                        int edgeIndex = firstEdge[i1];
                        if (edgeIndex == -1)
                        {
                            firstEdge[i1] = edgeCount;
                        }
                        else
                        {
                            while (true)
                            {
                                int index = firstEdge[nextEdge + edgeIndex];
                                if (index == -1)
                                {
                                    firstEdge[nextEdge + edgeIndex] = edgeCount;
                                    break;
                                }
    
                                edgeIndex = index;
                            }
                        }
    
                        firstEdge[nextEdge + edgeCount] = -1;
                        edgeCount++;
                    }
    
                    i1 = i2;
                }
            }
 
             // Second pass over all triangles. This finds all the edges satisfying the
             // condition that the first vertex index is greater than the second vertex index
             // when the direction from the first vertex to the second vertex represents
             // a counterclockwise winding around the triangle to which the edge belongs.
             // For each of these edges, the same edge should have already been found in
             // the first pass for a different triangle. Of course we might have edges with only one triangle
             // in that case we just add the edge here
             // So we search the list of edges
             // for the higher-numbered vertex index for the matching edge and fill in the
             // second triangle index. The maximum number of comparisons in this search for
             // any vertex is the number of edges having that vertex as an endpoint.
    
             for (int a = 0; a < triangleCount; a++)
             {
                 int i1 = triangleArray[a * 3 + 2];
                 for (int b = 0; b < 3; b++)
                 {
                     int i2 = triangleArray[a * 3 + b];
                     if (i1 > i2)
                     {
                         bool foundEdge = false;
                         for (int edgeIndex = firstEdge[i2]; edgeIndex != -1; edgeIndex = firstEdge[nextEdge + edgeIndex])
                         {
                             Edge edge = edgeArray[edgeIndex];
                             if ((edge.vertexIndex[1] == i1) && (edge.faceIndex[0] == edge.faceIndex[1]))
                             {
                                 edgeArray[edgeIndex].faceIndex[1] = a;
                                 foundEdge = true;
                                 break;
                             }
                         }
    
                         if (!foundEdge)
                         {
                             Edge newEdge = new Edge();
                             newEdge.vertexIndex[0] = i1;
                             newEdge.vertexIndex[1] = i2;
                             newEdge.faceIndex[0] = a;
                             newEdge.faceIndex[1] = a;
                             edgeArray[edgeCount] = newEdge;
                             edgeCount++;
                         }
                     }
    
                     i1 = i2;
                 }
             }
    
             Edge[] compactedEdges = new Edge[edgeCount];
             for (int e = 0; e < edgeCount; e++)
                 compactedEdges[e] = edgeArray[e];
    
             return compactedEdges;
        }
 
    }
}