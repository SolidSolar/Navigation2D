using System;
using System.Collections.Generic;
using Navigation2D.NavMath.SelfBalancedTree;
using UnityEngine;

namespace Navigation2D.NavMath
{
    [Serializable]
    public class VisibilityGraph
    {
        [SerializeField]
        private AVLTree<Edge> _bst = new AVLTree<Edge>();
        [SerializeField]
        private List<Polygon> _polygons = new List<Polygon>();
        [SerializeField]
        private List<Vertex> _allVertices = new List<Vertex>();
        
        [SerializeField]
        private DictionaryOfVertexAndListVertex _adjList = new DictionaryOfVertexAndListVertex();
        [SerializeField]
        private DictionaryOfVertexAndFloat _distances = new DictionaryOfVertexAndFloat();
        [SerializeField]
        private DictionaryOfVertexAndVertex _prevs = new DictionaryOfVertexAndVertex();
        public Dictionary<Vertex, ReferenceSerializableList<Vertex>> GetAdjacencyMatrix()
        {
            return new(_adjList);
        }
        
        public void AddPolygon(Polygon polygon)
        {
            if (_polygons.Contains(polygon))
            {
                return;
            }

            _polygons.Add(polygon);
            foreach (var v in polygon.Vertices)
            {
                _allVertices.Add(v);

                _adjList.Add(v, new ReferenceSerializableList<Vertex>());
            }

            // Place polygon
            foreach (var vertex in polygon.Vertices)
            {
                CalculateVisiblityForVertex(vertex, _allVertices);
            }

            // Recalculate it's touching edges -- it might have obstructed someone's vision
            var touchingVertices = GetTouchingVertices(polygon);

            foreach (var touchingVertex in touchingVertices)
            {
                RemoveEdgesOfVertex(touchingVertex);
            }

            foreach (var touchingVertex in touchingVertices)
            {
                CalculateVisiblityForVertex(touchingVertex, _allVertices);
            }
        }

        private bool IntersectsWith(float v1X, float v1Z, float v2X, float v2Z)
        {
            foreach (var polygon in _polygons)
            {
                if (polygon.IntersectsWith(v1X, v1Z, v2X, v2Z))
                {
                    return true;
                }
            }

            return false;
        }

        
        public Vector2[] GetPath(Vector2 srcPos, Vector2 destPos)
        {
            if (_allVertices.Count < 1 // No vertices in graph
                || !IntersectsWith(srcPos.x, srcPos.y, destPos.x, destPos.y)) // Doesn't intersect with any polygon
            {
                Debug.Log(IntersectsWith(srcPos.x, srcPos.y, destPos.x, destPos.y));
                return new[] { srcPos, destPos };
            }

            // Source and destination points are temporarily in the graph
            var srcVertex = new Vertex(srcPos);
            var destVertex = new Vertex(destPos);
            _adjList.Add(srcVertex, new ReferenceSerializableList<Vertex>());
            _adjList.Add(destVertex, new ReferenceSerializableList<Vertex>());

            CalculateVisiblityForVertex(srcVertex, _allVertices);
            CalculateVisiblityForVertex(destVertex, _allVertices);

            // Manually set destination's neighbors
            foreach (var destNeighbor in _adjList[destVertex].list)
            {
                _adjList[destNeighbor].list.Add(destVertex);
            }

            var allVertsCopy = new HashSet<Vertex>(_allVertices); // TODO: Making it a list will be faster?
            allVertsCopy.Add(srcVertex);
            allVertsCopy.Add(destVertex);

            // Here be Djikstra
            _distances.Clear();
            _prevs.Clear();

            foreach (var vertex in allVertsCopy)
            {
                _distances.Add(vertex, float.MaxValue);
                _prevs.Add(vertex, null);
            }

            _distances[srcVertex] = 0f;

            while (allVertsCopy.Count > 0)
            {
                Vertex u = null;
                var minDist = float.MaxValue;
                foreach (var vertex in allVertsCopy)
                {
                    if (_distances[vertex] < minDist)
                    {
                        minDist = _distances[vertex];
                        u = vertex;
                    }
                }

                if (u == destVertex) // Destination found
                {
                    break;
                }

                allVertsCopy.Remove(u);

                for (var i = 0; i < _adjList[u].list.Count; i++)
                {
                    var v = _adjList[u].list[i];
                
                    if (!allVertsCopy.Contains(v))
                    {
                        continue;
                    }

                    var altDist = _distances[u] + Vector2.Distance(u.Position, v.Position);  // sqr distance
                    if (altDist < _distances[v])
                    {
                        _distances[v] = altDist;
                        _prevs[v] = u;
                    }
                }
            }

            var path = new List<Vector2>();
            var w = destVertex;

            while (_prevs[w] != null)
            {
                path.Insert(0, w.Position);
                w = _prevs[w];
            }
            path.Insert(0, w.Position);

            // Remove temp vertices from graph
            RemoveEdgesOfVertex(srcVertex);
            RemoveEdgesOfVertex(destVertex);
            _adjList.Remove(srcVertex);
            _adjList.Remove(destVertex);

            return path.ToArray();
        }
        
        public void RemovePolygon(Polygon polygon)
        {
            // Remove the polygon's edges and recalculate the polygon's neighbors
            var touchingVertices = GetTouchingVertices(polygon);

            foreach (var vertex in polygon.Vertices)
            {
                RemoveEdgesOfVertex(vertex);
            }

            _polygons.Remove(polygon);
            foreach (var v in polygon.Vertices)
            {
                _allVertices.Remove(v);
                _adjList.Remove(v);
            }

            foreach (var touchingVertex in touchingVertices)
            {
                CalculateVisiblityForVertex(touchingVertex, _allVertices);
            }

        }

        private void RemoveEdgesOfVertex(Vertex vertex)
        {
            var list = _adjList[vertex];

            for (var i = 0; i < list.list.Count; i++)
            {
                _adjList[list.list[i]].list.Remove(vertex);
            }
        }

        private HashSet<Vertex> GetTouchingVertices(Polygon polygon)
        {
            var retVal = new HashSet<Vertex>();
            foreach (var vertex in polygon.Vertices)
            {
                foreach (var vertexNeighbor in _adjList[vertex].list)
                {
                    if (!polygon.Vertices.Contains(vertexNeighbor))
                    {
                        retVal.Add(vertexNeighbor);
                    }
                }
            }

            return retVal;
        }

        private void CalculateVisiblityForVertex(Vertex pivot, List<Vertex> allVertices)
        {
            var result = _adjList[pivot];
            GetVisibilePoints(pivot, allVertices, ref result.list);
        }

        private void GetVisibilePoints(Vertex v, List<Vertex> allVertices, ref List<Vertex> result)
        {
            var vX = v.X;
            var vZ = v.Y;

            result.Clear();
            var sortedEvents = Util.SortClockwise(vX, vZ, allVertices);

            // Init _bst with all edges that are directly to the right
            for (var i = 0; i < _polygons.Count; i++)
            {
                // If the polygon not in the trajectory of "right" ray,
                // then it's certain that the polygon won't be in _bst initially
                var polygon = _polygons[i];
                if (polygon.RightmostX < vX
                    || polygon.TopmostZ < vZ
                    || polygon.BottommostZ > vZ)
                {
                    continue;
                }

                for (var j = 0; j < polygon.Edges.Length; j++)
                {
                    var edge = polygon.Edges[j];
                    float t;
                    if (edge.IntersectsWith(vX, vZ, 1f, 0f, out t))
                    {
                        edge.DistanceToReference = t;
                        _bst.Add(edge);
                    }
                }
            }

            var sortedEventsLen = sortedEvents.Length;
            for (var i = 0; i < sortedEventsLen; i++)
            {
                var eventPoint = sortedEvents[i];
                if (eventPoint == v)
                {
                    continue;
                }

                if (IsVisible(v, eventPoint))
                {
                    result.Add(eventPoint);
                }

                // Algorithm adds CW edges, then deletes CCW ones
                // The reverse is done here,
                // Reason is because when an edge is added with a ref distance value "d" already exists in the tree
                // Removing the older "d" also removes the newly added one
                var ccwSide = eventPoint.GetEdgesOnCCwSide(vX, vZ);
                for (int j = 0; j < ccwSide.Length; j++)
                {
                    if (ccwSide[j] != null)
                    {
                        _bst.Delete(ccwSide[j]);
                    }
                }
                var cwSide = eventPoint.GetEdgesOnCwSide(vX, vZ);
                for (int j = 0; j < cwSide.Length; j++)
                {
                    if (cwSide[j] != null)
                    {
                        cwSide[j].DistanceToReference = cwSide[j].DistanceTo(vX, vZ); // TODO: This line smells
                        _bst.Add(cwSide[j]);
                    }
                }
            }

            _bst.Clear();
        }

        private bool IsVisible(Vertex from, Vertex to)
        {
            // Neighboring vertices are assumed to be seeing each other
            if (from.IsNeighborWith(to))
            {
                return true;
            }

            var fromX = from.X;
            var fromZ = from.Y;
            var toX = to.X;
            var toZ = to.Y;

            // from-to ray shouldn't go through the polygon
            if (from.OwnerPolygon != null // There will be stray vertices during pathfinding
                && from.IsGoingBetweenNeighbors(toX, toZ))
            {
                return false;
            }

            // Check with if intersecting with owner polygon
            // Nudge a little bit away from polygon, so it won't intersect with neighboring edges
            var nudgedX = fromX - Mathf.Sign(fromX - toX) * 0.0001f;
            var nudgedZ = fromZ - Mathf.Sign(fromZ - toZ) * 0.0001f;
            if (from.OwnerPolygon != null
                && from.OwnerPolygon.IntersectsWith(nudgedX, nudgedZ, toX, toZ))
            {
                return false;
            }

            Edge leftMostEdge;
            _bst.GetMin(out leftMostEdge);

            if (leftMostEdge != null
                && leftMostEdge.IntersectsWith(fromX, fromZ, toX, toZ))
            {
                return false;
            }

            return true;
        }
    }
}