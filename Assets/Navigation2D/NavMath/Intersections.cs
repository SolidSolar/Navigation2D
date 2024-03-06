using System;
using System.Collections.Generic;
using UnityEngine;

namespace Navigation2D.NavMath.PolygonClipping
{
    public static class Intersections
    {
        public static IntersectionType GetIntersectionType(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2, 
            out List<Vector2> intersectionPoints)
        {
            var epsilon = 0.0001f;
            IntersectionType type;
            
            intersectionPoints = new List<Vector2>();

            Vector2 r = (p2 - p1);
            Vector2 s = (q2 - q1);
            
            var p1q1q2 = NavMath.GetSignedTriangleArea(p1, q1, q2);
            var p2q1q2 = NavMath.GetSignedTriangleArea(p2, q1, q2);
            var q1p1p2 = NavMath.GetSignedTriangleArea(q1, p1, p2);
            var q2p1p2 = NavMath.GetSignedTriangleArea(q2, p1, p2);
            

            if (Mathf.Abs(p1q1q2) < epsilon &&Mathf.Abs(q1p1p2) < epsilon  )
            {
                var t0 = Vector2.Dot((q1 - p1), r)/ Vector2.Dot(r, r);
                var t1 = Vector2.Dot((p1 - q1), s)/ Vector2.Dot(s, s);
                
                type = IntersectionType.None;

                if (Mathf.Abs(t0) < epsilon && Mathf.Abs(t1) < epsilon)
                {
                    type = IntersectionType.VOverlap;
                    intersectionPoints.Add(p1);
                    intersectionPoints.Add(q1);
                } else
                if (0 < t0 && t0 < 1 && 0 < t1 && t1 < 1)
                {
                    type = IntersectionType.XOverlap;
                    intersectionPoints.Add(p1);
                    intersectionPoints.Add(q1);
                }
                else if ((t0 < 0 || t0 >= 1) && (0 < t1 && t1 < 1))
                {
                    
                    type = IntersectionType.TOverlapL;
                    intersectionPoints.Add(p1);
                    
                }
                else if ((t1 < 0 || t1 >= 1) && (0 < t0 && t0 < 1))
                {
                    type = IntersectionType.TOverlapR;
                    intersectionPoints.Add(q1);
                }
                
                return type;
            }
            
            float t = p1q1q2 / (p1q1q2 - p2q1q2);
            float u =  q1p1p2 / (q1p1p2 - q2p1p2);

            if (t>=0  && t <= 1 && u>=0 && u <=1)
            {
                var point = (1 - u) * q1 + u*q2;
                intersectionPoints = new List<Vector2>
                {
                    point
                };
                
                type = IntersectionType.XIntersection;

                if (Mathf.Abs(t) < epsilon && Mathf.Abs(u) < epsilon )
                {
                    type = IntersectionType.VIntersection;
                } else if (t == 0 && 0 < u && u < 1)
                {
                    type = IntersectionType.TIntersectionL;
                }else if (Mathf.Abs(u) < epsilon && 0 < t && t < 1)
                {
                    type = IntersectionType.TIntersectionR;
                }

                if (p2 == q2 || p2 == q1 || q2 == p1)
                {
                    type = IntersectionType.None;
                }
                

                return type;
            }

            intersectionPoints = new List<Vector2>();
            type = IntersectionType.None;
            return type;
        }
        
        //Line segment-line segment intersection in 2d space by using the dot product
        //p1 and p2 belongs to line 1, and p3 and p4 belongs to line 2 
        public static bool AreLineSegmentsIntersectingDotProduct(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            bool isIntersecting = false;

            if (IsPointsOnDifferentSides(p1, p2, p3, p4) && IsPointsOnDifferentSides(p3, p4, p1, p2))
            {
                isIntersecting = true;
            }

            return isIntersecting;
        }

        //Are the points on different sides of a line?
        private static bool IsPointsOnDifferentSides(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            bool isOnDifferentSides = false;
	
            //The direction of the line
            Vector3 lineDir = p2 - p1;

            //The normal to a line is just flipping x and z and making z negative
            Vector3 lineNormal = new Vector3(-lineDir.z, lineDir.y, lineDir.x);
	
            //Now we need to take the dot product between the normal and the points on the other line
            float dot1 = Vector3.Dot(lineNormal, p3 - p1);
            float dot2 = Vector3.Dot(lineNormal, p4 - p1);

            //If you multiply them and get a negative value then p3 and p4 are on different sides of the line
            if (dot1 * dot2 < 0f)
            {
                isOnDifferentSides = true;
            }

            return isOnDifferentSides;
        }
        
        //Whats the coordinate of an intersection point between two lines in 2d space if we know they are intersecting
        //http://thirdpartyninjas.com/blog/2008/10/07/line-segment-intersection/
        public static Vector2 GetLineLineIntersectionPoint(Vector2 l1_p1, Vector2 l1_p2, Vector2 l2_p1, Vector2 l2_p2)
        {
            float denominator = (l2_p2.y - l2_p1.y) * (l1_p2.x - l1_p1.x) - (l2_p2.x - l2_p1.x) * (l1_p2.y - l1_p1.y);

            float u_a = ((l2_p2.x - l2_p1.x) * (l1_p1.y - l2_p1.y) - (l2_p2.y - l2_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;

            Vector2 intersectionPoint = l1_p1 + u_a * (l1_p2 - l1_p1);

            return intersectionPoint;
        }
        
        public static bool IsPointInPolygon(List<Vector2> polygonPoints, Vector2 point)
        {
            //Step 1. Find a point outside of the polygon
            //Pick a point with a x position larger than the polygons max x position, which is always outside
            Vector2 maxXPosVertex = polygonPoints[0];

            for (int i = 1; i < polygonPoints.Count; i++)
            {
                if (polygonPoints[i].x > maxXPosVertex.x)
                {
                    maxXPosVertex = polygonPoints[i];
                }
            }

            //The point should be outside so just pick a number to make it outside
            Vector2 pointOutside = maxXPosVertex + new Vector2(10f, 0f);

            //Step 2. Create an edge between the point we want to test with the point thats outside
            Vector2 l1_p1 = point;
            Vector2 l1_p2 = pointOutside;

            //Step 3. Find out how many edges of the polygon this edge is intersecting
            int numberOfIntersections = 0;

            for (int i = 0; i < polygonPoints.Count; i++)
            {
                //Line 2
                Vector2 l2_p1 = polygonPoints[i];

                int iPlusOne = NavMath.ClampListIndex(i + 1, polygonPoints.Count);

                Vector2 l2_p2 = polygonPoints[iPlusOne];

                numberOfIntersections += AreLineSegmentsIntersectingDotProduct(l1_p1, l1_p2, l2_p1, l2_p2) ? 1 : 0;
            }

            //Step 4. Is the point inside or outside?
            bool isInside = true;

            //The point is outside the polygon if number of intersections is even or 0
            if (numberOfIntersections == 0 || numberOfIntersections % 2 == 0)
            {
                isInside = false;
            }

            return isInside;
        }
    }

    public enum IntersectionType
    {
        XIntersection,
        TIntersectionL,
        TIntersectionR,
        VIntersection,
        XOverlap,
        TOverlapL,
        TOverlapR,
        VOverlap,
        None
    }
}