using System;
using System.Collections.Generic;
using UnityEngine;

namespace Navigation2D.NavMath
{
    public static class NavMath
    {
        public static List<LineSegment2D> PointsToSegmentList(List<Vector2> points, PointSortingMode sortingMode = PointSortingMode.NoSorting)
        {
            List<LineSegment2D> segments = new();
            for (int i = 0; i < points.Count-1; i++)
            {
                segments.Add(new LineSegment2D(new(points[i].x, points[i].y), new(points[i+1].x, points[i+1].y), PointSortingMode.IncreasingXY));
            }
            segments.Add(new LineSegment2D(new(points[^1].x, points[^1].y), new(points[0].x, points[0].y), PointSortingMode.IncreasingXY));

            return segments;
        }
        public static void SortPoints(Vector2 p1, Vector2 p2, PointSortingMode sortingMode)
        {
            int res1 = Math.Sign(p1.x - p2.x);
            int res2 = Math.Sign(p1.y - p2.y);
            switch (sortingMode)
            {
                case PointSortingMode.DescendingXY:
                    if(res1 == -1)
                        MathfExt.Swap(ref p1, ref p2);
                    else if(res1 == 0 && res2 < 0)
                        MathfExt.Swap(ref p1, ref p2);
                    break;
                case PointSortingMode.DescendingYX:
                    if(res2 == -1)
                        MathfExt.Swap(ref p1, ref p2);
                    else if(res2 == 0 && res1 < 0)
                        MathfExt.Swap(ref p1, ref p2);
                    break;
                case PointSortingMode.IncreasingXY:
                    if(res1 == 1)
                        MathfExt.Swap(ref p1, ref p2);
                    else if(res1 == 0 && res2 > 0)
                        MathfExt.Swap(ref p1, ref p2);
                    break;
                case PointSortingMode.IncreasingYX:
                    if(res2 == 1)
                        MathfExt.Swap(ref p1, ref p2);
                    else if(res2 == 0 && res1 > 0)
                        MathfExt.Swap(ref p1, ref p2);
                    break;
                default:
                    return;
            }
        }

        public static int ComparePoints(Vector2 p1, Vector2 p2, PointSortingMode sortingMode)
        {
            int res1 = Math.Sign(p1.x - p2.x);
            int res2 = Math.Sign(p1.y - p2.y);
            switch (sortingMode)
            {
                case PointSortingMode.DescendingXY:
                    return res1 == 0 ? -res2 : -res1;
                case PointSortingMode.DescendingYX:
                    return res2 == 0 ? -res1 : -res2;
                case PointSortingMode.IncreasingXY:
                    return res1 == 0 ? res2 : res1;
                case PointSortingMode.IncreasingYX:
                    return res2 == 0 ? res1 : res2;
                default:
                    return 0;
            }
        }
        
        public const float EPSILON = 0.00001f;


        public static float GetSignedTriangleArea(Vector2 a, Vector2 b, Vector2 c)
        {
            return (b.x - a.x) * (c.y - a.y) - (b.y - a.y)*(c.x - a.x);
        }

        public static int ClampListIndex(int index, int listSize)
        {
            if (index >= listSize)
                return index - listSize;
            if (index < 0)
                return listSize + index;
            return index;
        }
    }
}