using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using Random = System.Random;


namespace Navigation2D.NavMath.PolygonClipping
{
    
//From the report "Efficient clipping of arbitrary polygons"
//Assumes there are no degeneracies (each vertex of one polygon does not lie on an edge of the other polygon)
    public static class GreinerHormann
    {
        public static List<List<Vector2>> ClipPolygons(List<Vector2> polyVector, List<Vector2> clipPolyVector, bool union)
        {
            List<List<Vector2>> finalPoly = new List<List<Vector2>>();
    
            //Step 0. Create the data structure needed
            List<ClipVertex> poly = InitDataStructure(polyVector, true);
            
            List<ClipVertex> clipPoly = InitDataStructure(clipPolyVector, true);
    
            //Step 1. Find intersection points
            //Need to test if we have found an intersection point, if none is found, the polygons dont intersect, or one polygon is inside the other
            bool hasFoundIntersection = false;

            var polyEdges = new List<ClipEdge>();
            for (int i = 0; i < poly.Count; i++)
            {
                polyEdges.Add(new ClipEdge()
                {
                    vertexes = new List<Tuple<ClipVertex, float>>()
                    {
                        new(poly[i], 0),
                        new(poly[NavMath.ClampListIndex(i+1, poly.Count)], 1)
                    }
                });
            }

            bool shown = false;
            int outerCounter = 0;
            var clipEdges = new List<ClipEdge>();
            for (int i = 0; i < clipPoly.Count; i++)
            {
                clipEdges.Add(new ClipEdge()
                {
                    vertexes = new List<Tuple<ClipVertex, float>>()
                    {
                        new(clipPoly[i], 0),
                        new(clipPoly[NavMath.ClampListIndex(i+1, clipPoly.Count)], 1)
                    }
                });
            }


            for (int i = 0; i < polyEdges.Count; i++)
            {
                var polyEdge = polyEdges[i];
                
                for (int j = 0; j < clipEdges.Count; j++)
                {
                    var clipEdge = clipEdges[j];
                    
                    ClipVertex polyCur = poly[i];
                    ClipVertex clipCur = clipPoly[j];
                    

                    //Are these lines intersecting?
                    var intersectionPoints = new List<Vector2>();
                    
                    var type = Intersections.GetIntersectionType(
                        polyEdge.vertexes[0].Item1.coordinate, 
                        polyEdge.vertexes[^1].Item1.coordinate, 
                        clipEdge.vertexes[0].Item1.coordinate, 
                        clipEdge.vertexes[^1].Item1.coordinate, 
                        out intersectionPoints);

                    if (type != IntersectionType.None)
                    {
                        polyCur = polyEdge.GetClosestVertex(intersectionPoints[0]);
                        clipCur = clipEdge.GetClosestVertex(intersectionPoints[intersectionPoints.Count > 1 ? 1 : 0]);
                    }


                    ClipVertex vertexOnPolygon;
                    ClipVertex vertexOnClipPolygon;

                    switch (type)
                    {
                        case IntersectionType.XIntersection:
                            vertexOnPolygon = InsertIntersectionVertex(intersectionPoints[0], polyCur);
                            polyEdge.AddVertex(vertexOnPolygon);
                            vertexOnClipPolygon = InsertIntersectionVertex(intersectionPoints[0], clipCur);
                            clipEdge.AddVertex(vertexOnClipPolygon);
                            vertexOnPolygon.neighbor = vertexOnClipPolygon;
                            vertexOnClipPolygon.neighbor = vertexOnPolygon;
                            hasFoundIntersection = true;
                            break;
                        case IntersectionType.TIntersectionL:
                            vertexOnPolygon = polyCur;
                            vertexOnClipPolygon = GetVertexCopy(vertexOnPolygon);
                            clipEdge.AddVertex(vertexOnPolygon);
                            vertexOnClipPolygon.isIntersection = true;
                            vertexOnPolygon.isIntersection = true;

                            vertexOnClipPolygon.neighbor = vertexOnPolygon;
                            vertexOnPolygon.neighbor = vertexOnClipPolygon;

                            vertexOnClipPolygon.prev = clipCur;
                            vertexOnClipPolygon.next = clipCur.next;
                            clipCur.next = vertexOnClipPolygon;
                            vertexOnClipPolygon.next.prev = vertexOnClipPolygon;
                            hasFoundIntersection = true;
                            break;
                        case IntersectionType.TIntersectionR:
                            vertexOnPolygon = clipCur;
                            vertexOnClipPolygon = GetVertexCopy(clipCur);
                            polyEdge.AddVertex(vertexOnClipPolygon);
                            vertexOnClipPolygon.isIntersection = true;
                            vertexOnPolygon.isIntersection = true;
                            vertexOnClipPolygon.neighbor = vertexOnPolygon;
                            vertexOnPolygon.neighbor = vertexOnClipPolygon;

                            vertexOnClipPolygon.prev = polyCur;
                            vertexOnClipPolygon.next = polyCur.next;
                            polyCur.next = vertexOnClipPolygon;
                            vertexOnClipPolygon.next.prev = vertexOnClipPolygon;
                            hasFoundIntersection = true;
                            break;
                        case IntersectionType.VIntersection:
                            vertexOnPolygon = polyCur;
                            vertexOnClipPolygon = clipCur;
                            vertexOnClipPolygon.isIntersection = true;
                            vertexOnPolygon.isIntersection = true;
                            vertexOnPolygon.neighbor = vertexOnClipPolygon;
                            vertexOnClipPolygon.neighbor = vertexOnPolygon;
                            hasFoundIntersection = true;
                            break;
                        case IntersectionType.XOverlap:
                            vertexOnClipPolygon = GetVertexCopy(polyCur);
                            clipEdge.AddVertex(vertexOnClipPolygon);
                            vertexOnPolygon = GetVertexCopy(clipCur);
                            polyEdge.AddVertex(vertexOnClipPolygon);
                            vertexOnClipPolygon.isIntersection = true;
                            vertexOnPolygon.isIntersection = true;

                            vertexOnClipPolygon.neighbor = vertexOnPolygon;
                            vertexOnPolygon.neighbor = vertexOnClipPolygon;

                            vertexOnClipPolygon.prev = clipCur;
                            vertexOnClipPolygon.next = clipCur.next;
                            clipCur.next = vertexOnClipPolygon;
                            vertexOnClipPolygon.next.prev = vertexOnClipPolygon;

                            vertexOnPolygon.prev = polyCur;
                            vertexOnPolygon.next = polyCur.next;
                            polyCur.next = vertexOnPolygon;
                            vertexOnPolygon.next.prev = polyCur;

                            hasFoundIntersection = true;
                            break;
                        case IntersectionType.TOverlapL:
                            vertexOnPolygon = GetVertexCopy(polyCur);
                            clipEdge.AddVertex(vertexOnPolygon);
                            vertexOnPolygon.isIntersection = true;

                            vertexOnPolygon.neighbor = polyCur;
                            polyCur.neighbor = vertexOnPolygon;
                            vertexOnPolygon.prev = clipCur;
                            vertexOnPolygon.next = clipCur.next;
                            vertexOnPolygon.next.prev = vertexOnPolygon;
                            clipCur.next = vertexOnPolygon;

                            hasFoundIntersection = true;
                            break;
                        case IntersectionType.TOverlapR:
                            vertexOnPolygon = GetVertexCopy(clipCur);
                            polyEdge.AddVertex(vertexOnPolygon);
                            vertexOnPolygon.isIntersection = true;

                            vertexOnPolygon.neighbor = clipCur;
                            clipCur.neighbor = vertexOnPolygon;
                            vertexOnPolygon.prev = polyCur;
                            vertexOnPolygon.next = polyCur.next;
                            vertexOnPolygon.next.prev = vertexOnPolygon;
                            polyCur.next = vertexOnPolygon;
                            hasFoundIntersection = true;
                            break;
                        case IntersectionType.VOverlap:
                            vertexOnPolygon = polyCur;

                            vertexOnClipPolygon = clipCur;
                            vertexOnPolygon.neighbor = vertexOnClipPolygon;
                            vertexOnClipPolygon.neighbor = vertexOnPolygon;
                            vertexOnClipPolygon.isIntersection = true;
                            vertexOnPolygon.isIntersection = true;
                            hasFoundIntersection = true;
                            break;
                    }
                }
            }

            //If the polygons are intersecting
            if (hasFoundIntersection)
            {
                MarkEntryExit(poly, clipPoly, union);
                //MarkEntryExit(clipPoly, poly, union);

                var outsidePolyVertices = GetNewClippedPolygon(poly, true);
                foreach (var p in outsidePolyVertices)
                {
                    var lis = p;
                    for (int i = 0; i < lis.Count-1; i++)
                    {
                        Debug.DrawLine(lis[i], lis[i+1], Color.red, 5f);
                    }
                    
                    Debug.DrawLine(lis[^1], lis[0], Color.red, 5f);
                    
                    Debug.Log(lis.Count);
                }
            }
            //Check if one polygon is inside the other
            else
            {
                //Is the polygon inside the clip polygon?
                //Depending on the type of boolean operation, we might get a hole
                if (IsPolygonInsidePolygon(polyVector, clipPolyVector))
                {
                    Debug.Log("Poly is inside clip poly");
                }
                else if (IsPolygonInsidePolygon(clipPolyVector, polyVector))
                {
                    Debug.Log("Clip poly is inside poly");
                }
                else
                {
                    Debug.Log("Polygons are not intersecting");
                }
            }
    
            return finalPoly;
        }

        private static List<List<Vector2>>GetNewClippedPolygon(List<ClipVertex> poly, bool UNION)
        {
            List<List<Vector2>> finalPolygon = new List<List<Vector2>>();
            var intersections = new List<ClipVertex>();

            var start = poly[1];
            var cur = start;
            int safety = 0;
            do
            {
                if (cur.type == VertexType.Crossing)
                {
                    intersections.Add(cur);
                }
                
                cur = cur.next;
            } while (cur != start);
            
            Debug.Log(intersections.Count);
            foreach (var I in intersections) {
                if (!I.isIntersection)
                {
                    continue;
                }
                List<Vector2> R = new List<Vector2>();                         // result polygon component

                ClipVertex V = I;                      // start traversal at I
                V.isIntersection = false;            // mark visited vertices
                
                do {
                    int status = V.isEntry;
                    status = status == 1 ? 2 : 1;
                    
                    do {                              // traverse P (or Q) and add vertices to R, until...
                        if ((status == 2) ^ UNION)
                            V = V.next;                  // move forward  from an ENTRY vertex to the next EXIT  vertex
                        else
                            V = V.prev;                  // move backward from an EXIT  vertex to the next ENTRY vertex
                        V.isIntersection = false;        // mark visited vertices
                        // add vertex to result polygon 
                        R.Add(V.coordinate);
                        safety++;
                        if (safety > 1000)
                        {
                            return null;
                        }
                    } while ( V.isEntry != status    // ... we arrive at a vertex with opposite entry/exit flag, or
                              && (V != I) );          // at the initial vertex I
  
                    if (V != I) {
                        V = V.neighbor;               // switch from P to Q or vice versa
                        V.isIntersection = false;        // mark visited vertices
                    }

                } while (V != I);                   // the result polygon component is complete, 
                // if we are back to the initial vertex I
                finalPolygon.Add(R);
            }

            return finalPolygon;
        }

        //Get the clipped polygons: either the intersection or the !intersection
        //We might end up with more than one polygon and they are connected via clipvertex nextpoly
    
        //Is a polygon One inside polygon Two?
        private static bool IsPolygonInsidePolygon(List<Vector2> polyOne, List<Vector2> polyTwo)
        {
            bool isInside = false;
        
            for (int i = 0; i < polyOne.Count; i++)
            {
                if (Intersections.IsPointInPolygon(polyTwo, polyOne[i]))
                {
                    //Is inside if at least one point is inside the polygon (in this case because we run this method after we have tested
                    //if the polygons are intersecting)
                    isInside = true;
    
                    break;
                }
            }
    
            return isInside;
        }
    
        //Find the the first entry vertex in a polygon
        private static ClipVertex FindFirstEntryVertex(List<ClipVertex> poly)
        {
            ClipVertex thisVertex = poly[1];

            var iterVertex = thisVertex.next;

            var iter = 0;

            while (iterVertex != thisVertex)
            {
                if (thisVertex.isIntersection && thisVertex.type != VertexType.Bouncing && !thisVertex.isTakenByFinalPolygon)
                {
                    return iterVertex;
                }
                iterVertex = iterVertex.next;


                iter++;
                if (iter > 1000)
                {
                    Debug.LogError("Infinite loop on finding first vertex");
                    return thisVertex;
                }
            }
            
            return thisVertex;
        }
    
        //Create the data structure needed
        private static List<ClipVertex> InitDataStructure(List<Vector2> polyVector, bool reverse)
        {
            List<ClipVertex> poly = new List<ClipVertex>();
    
            for (int i = 0; i < polyVector.Count; i++)
            {
                poly.Add(new ClipVertex(polyVector[i]));
            }
            if(reverse)
                poly.Reverse();
            //Connect the vertices
            for (int i = 0; i < poly.Count; i++)
            {
                int iPlusOne = NavMath.ClampListIndex(i + 1, poly.Count);
                int iMinusOne = NavMath.ClampListIndex(i - 1, poly.Count);
    
                poly[i].next = poly[iPlusOne];
                poly[i].prev = poly[iMinusOne];
            }
            
            return poly;
        }
    
        //Insert intersection vertex at correct position in the list
        private static ClipVertex InsertIntersectionVertex(Vector2 intersectionPoint, ClipVertex currentVertex)
        {
            ClipVertex intersectionVertex = new(intersectionPoint);
            intersectionVertex.newlyAdded = true;
            intersectionVertex.isIntersection = true;
    
            ClipVertex insertAfterThisVertex = currentVertex;
    
            intersectionVertex.next = insertAfterThisVertex.next;
    
            intersectionVertex.prev = insertAfterThisVertex;
    
            insertAfterThisVertex.next.prev = intersectionVertex;
    
            insertAfterThisVertex.next = intersectionVertex;
    
            return intersectionVertex;
        }

        private static ClipVertex GetVertexCopy(ClipVertex currentVertex)
        {
            return new ClipVertex(currentVertex.coordinate)
            {
                isIntersection = currentVertex.isIntersection,
                newlyAdded = true
            };
        }
    
        //Mark entry exit points
        private static void MarkEntryExit(List<ClipVertex> poly, List<ClipVertex> clipPoly, bool union)
        {
            var vertex = new ClipVertex(Vector2.Lerp(poly[0].coordinate, poly[1].coordinate, 0.5f))
            {
                prev = poly[0],
                next = poly[1],
                isIntersection = false
            };
            poly[0].next = vertex;
            poly[1].prev = vertex;
            poly.Insert(1, vertex);
            
            List<ClipVertex> pIdx = new List<ClipVertex> ();
            List<ClipVertex> qIdx = new List<ClipVertex>();
            
            var start = poly[1];
            var cur = start;
            
            do
            {
                if (cur.isIntersection)
                {
                    pIdx.Add(cur);
                }
                cur = cur.next;
            } while (start != cur);
            
            start = clipPoly[1];
            cur = start;

            do
            {
                if (cur.isIntersection)
                {
                    qIdx.Add(cur);
                }

                cur = cur.next;
            } while (start != cur);

            var chain = false;
            ClipVertex pChainStart = null;
            ClipVertex qChainStart = null;
            int entry = 1;
            bool shown = false;
            foreach (var item in pIdx)
            {
                var p = item;
                var q = item.neighbor;

                if ( (p.next.isIntersection && (p.next.neighbor == q.prev || p.next.neighbor == q.next) ||
                    p.prev.isIntersection && (p.prev.neighbor == q.prev || p.prev.neighbor == q.next)) || chain)
                {
                    //OnOn
                    if (p.next.neighbor == q.next && p.prev.neighbor == q.prev ||
                        p.next.neighbor == q.prev && p.prev.neighbor == q.next)
                    {
                        p.overlapType = OverlapType.OnOn;
                        q.overlapType = OverlapType.OnOn;
                    }
                    else
                    if (p.next.neighbor == q.next &&
                        !IsChainLeftTurn(p.prev.coordinate, p.coordinate, p.next.coordinate, q.prev.coordinate) ||
                        p.next.neighbor == q.prev &&
                        !IsChainLeftTurn(p.prev.coordinate, p.coordinate, p.next.coordinate, q.next.coordinate))
                    {
                        p.overlapType = OverlapType.LeftOn;
                        q.overlapType = OverlapType.LeftOn;
                    }
                    else
                    //RightOn
                    if (p.next.neighbor == q.next &&
                        IsChainLeftTurn(p.prev.coordinate, p.coordinate, p.next.coordinate,
                            q.prev.coordinate) ||
                        p.next.neighbor == q.prev &&
                        IsChainLeftTurn(p.prev.coordinate, p.coordinate, p.next.coordinate,
                            q.next.coordinate))
                    {
                        p.overlapType = OverlapType.RightOn;
                        q.overlapType = OverlapType.RightOn;
                    }
                    else
                    //OnLeft
                    if (p.prev.neighbor == q.prev &&
                        !IsChainLeftTurn(p.prev.coordinate, p.coordinate, p.next.coordinate, q.next.coordinate) ||
                        p.prev.neighbor == q.next &&
                        !IsChainLeftTurn(p.prev.coordinate, p.coordinate, p.next.coordinate, q.prev.coordinate)
                    )
                    {
                        p.overlapType = OverlapType.OnLeft;
                        q.overlapType = OverlapType.OnLeft;
                    }
                    else
                    //OnRight
                    if (p.prev.neighbor == q.prev &&
                        IsChainLeftTurn(p.prev.coordinate, p.coordinate, p.next.coordinate, q.next.coordinate) ||
                        p.prev.neighbor == q.next &&
                        IsChainLeftTurn(p.prev.coordinate, p.coordinate, p.next.coordinate, q.prev.coordinate)
                    )
                    {
                        p.overlapType = OverlapType.OnRight;
                        q.overlapType = OverlapType.OnRight;
                    }
                }

                bool overlap = p.overlapType != OverlapType.None;
                if (!overlap)
                {
                    var t1 = IsChainLeftTurn(
                        p.prev.coordinate, p.coordinate, 
                        p.next.coordinate, q.prev.coordinate);
                    var t2 = IsChainLeftTurn(
                        p.prev.coordinate, p.coordinate, 
                        p.next.coordinate, q.next.coordinate);

                    if (t1 != t2)
                    {
                        p.type = VertexType.Crossing;
                        q.type = VertexType.Crossing;
                        DebugPoint(p.coordinate + UnityEngine.Random.Range(0, 0.01f)*Vector2.right, entry == 1 ? Color.black : Color.cyan);
                        p.isEntry = entry;
                        q.isEntry = entry;
                        entry = entry == 1 ? 2 : 1;
                    }
                    else
                    {
                        p.type = VertexType.Bouncing;
                        q.type = VertexType.Bouncing;
                    }
                    
                }
                else
                {
                    if (p.overlapType != OverlapType.OnOn && chain)
                    {
                        if (pChainStart.overlapType == OverlapType.LeftOn &&
                            p.overlapType != OverlapType.OnLeft ||
                            pChainStart.overlapType == OverlapType.RightOn &&
                            p.overlapType != OverlapType.OnRight||
                            qChainStart.overlapType == OverlapType.LeftOn &&
                            q.overlapType != OverlapType.OnLeft ||
                            qChainStart.overlapType == OverlapType.RightOn &&
                            q.overlapType != OverlapType.OnRight)
                        {
                            Debug.Log("two-sided case");
                            var curPVertex = pChainStart;
                            var curQVertex = qChainStart;
                           
                            while (curPVertex != p)
                            {
                                curPVertex.type = VertexType.Bouncing;
                                curPVertex = curPVertex.next;
                            }
                            while (curQVertex != q)
                            {
                                curQVertex.type = VertexType.Bouncing;
                                curQVertex = curQVertex.next;
                            }
                            
                            pChainStart.type = VertexType.Crossing;
                            qChainStart.type = VertexType.Crossing;
                            DebugPoint(pChainStart.coordinate + UnityEngine.Random.Range(0, 0.03f)*Vector2.right, entry == 1? Color.black : Color.cyan);
                            pChainStart.isEntry = entry;
                            qChainStart.isEntry = entry;
                            entry = entry == 1 ? 2 : 1;
                        }
                        else
                        {
                            var curPVertex = pChainStart;
                            var curQVertex = qChainStart;
                            
                            while (curPVertex != p)
                            {
                                curPVertex.type = VertexType.Bouncing;
                                curPVertex = curPVertex.next;
                            }
                            while (curQVertex != q)
                            {
                                curQVertex.type = VertexType.Bouncing;
                                curQVertex = curQVertex.next;
                            }
                        
                            
                            if(union){
                                pChainStart.type = VertexType.Crossing;
                                qChainStart.type = VertexType.Crossing;
                                DebugPoint(pChainStart.coordinate + UnityEngine.Random.Range(0, 0.03f)*Vector2.right, entry == 1? Color.black : Color.cyan);
                                pChainStart.isEntry = entry;
                                qChainStart.isEntry = entry;
                                
                                p.type = VertexType.Crossing;
                                q.type = VertexType.Crossing;
                                DebugPoint(p.coordinate + UnityEngine.Random.Range(0, 0.03f)*Vector2.right, (entry == 1 ? 2 : 1) == 1? Color.black : Color.cyan);
                                p.isEntry = entry == 1 ? 2 : 1;
                                q.isEntry = entry == 1 ? 2 : 1;
                            }
                            
                            p.type = VertexType.Bouncing;
                            q.type = VertexType.Bouncing;
                        }

                        chain = false;
                    }
                    
                    if (!chain)
                    {
                        chain = true;
                        pChainStart = p;
                        qChainStart = q;
                    }
                }

               
            }
    
        }

        private static bool IsChainLeftTurn(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 q)
        {
            var s1 = NavMath.GetSignedTriangleArea(q, p1, p2);
            var s2 = NavMath.GetSignedTriangleArea(q, p2, p3);
            var s3 = NavMath.GetSignedTriangleArea(p1, p2, p3);

            return s3 > 0 ? s1 > 0 && s2 > 0 : s1 > 0 || s2 > 0;
        }


        private static void DebugPoint(Vector2 point, Color color)
        {
            Debug.DrawLine(point - Vector2.right*0.035f, point + Vector2.right*0.07f, color, duration: 3f);
            Debug.DrawLine(point - Vector2.up*0.035f, point + Vector2.up*0.07f, color, duration: 3f);
        }
    }
}