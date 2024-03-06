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
        
        //The value we use to avoid floating point precision issues
        //http://sandervanrossen.blogspot.com/2009/12/realtime-csg-part-1.html
        //Unity has a built-in Mathf.Epsilon;
        //But it's better to use our own so we can test different values
        public const float EPSILON = 0.00001f;



        //Test if a float is the same as another float
        public static bool AreFloatsEqual(float a, float b)
        {
            float diff = a - b;

            float e = NavMath.EPSILON;

            if (diff < e && diff > -e)
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        //Remap value from range 1 to range 2
        public static float Remap(float value, float r1_low, float r1_high, float r2_low, float r2_high)
        {
            float remappedValue = r2_low + (value - r1_low) * ((r2_high - r2_low) / (r1_high - r1_low));

            return remappedValue;
        }

        public static float GetSignedTriangleArea(Vector2 a, Vector2 b, Vector2 c)
        {
            return (b.x - a.x) * (c.y - a.y) - (b.y - a.y)*(c.x - a.x);
        }

        //Clamp list indices
        //Will even work if index is larger/smaller than listSize, so can loop multiple times
        public static int ClampListIndex(int index, int listSize)
        {
            if (index >= listSize)
                return index - listSize;
            if (index < 0)
                return listSize + index;
            return index;
        }



        // Returns the determinant of the 2x2 matrix defined as
        // | x1 x2 |
        // | y1 y2 |
        //det(a_normalized, b_normalized) = sin(alpha) so it's similar to the dot product
        //Vector alignment dot det
        //Same:            1   0
        //Perpendicular:   0  -1
        //Opposite:       -1   0
        //Perpendicular:   0   1
        public static float Det2(float x1, float x2, float y1, float y2)
        {
            return x1 * y2 - y1 * x2;
        }

        public static float Det2(Vector2 a, Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }



        //Calculate the angle between two vectors 
        //This angle should be measured in 360 degrees (Vector3.Angle is measured in 180 degrees)
        //Should maybe be moved to _Geometry??

        //In 3d space [radians]
        //https://stackoverflow.com/questions/5188561/signed-angle-between-two-3d-vectors-with-same-origin-within-the-same-plane
        //https://math.stackexchange.com/questions/2906314/how-to-calculate-angle-between-two-vectors-in-3d-with-clockwise-or-counter-clock
        public static float AngleFromToCCW(Vector3 from, Vector3 to, Vector3 upRef)
        {
            //This is only working in 2d space
            //float angleDegrees = Quaternion.FromToRotation(to.ToVector3(), from.ToVector3()).eulerAngles.y;

            from = Vector3.Normalize(from);
            to = Vector3.Normalize(to);
            upRef = Vector3.Normalize(upRef);

            float angleRad = AngleBetween(from, to, shouldNormalize: false);

            //To get 0-2pi (360 degrees) we can use the determinant [a, b, u] = (a x b) dot u
            //Where u is a reference up vector

            //Remember that the cross product is not alwayspointing up - it can change to down depending on how the vectors are aligned
            //Which is why we need a fixed reference up
            Vector3 cross = Vector3.Cross(from, to);

            float determinant = Vector3.Dot(Vector3.Cross(from, to), upRef);

            //Debug.Log(determinant);

            if (determinant >= 0f)
            {
                return angleRad;
            }
            else
            {
                return (Mathf.PI * 2f) - angleRad;
            }
        }

        //The angle between two vectors 0 <= angle <= 180
        //Same as Vector3.Angle() but we are using MyVector3
        public static float AngleBetween(Vector3 from, Vector3 to, bool shouldNormalize = true)
        {
            //from and to should be normalized
            //But sometimes they are already normalized and then we dont need to do it again
            if (shouldNormalize)
            {
                from = Vector3.Normalize(from);
                to = Vector3.Normalize(to);
            }

            //dot(a_normalized, b_normalized) = cos(alpha) -> acos(dot(a_normalized, b_normalized)) = alpha
            float dot = Vector3.Dot(from, to);

            //This shouldn't happen but may happen because of floating point precision issues
            dot = Mathf.Clamp(dot, -1f, 1f);

            float angleRad = Mathf.Acos(dot);

            return angleRad;
        }


        //In 2d space [radians]
        //If you want to calculate the angle from vector a to b both originating from c, from is a-c and to is b-c
        public static float AngleFromToCCW(Vector2 from, Vector2 to, bool shouldNormalize = false)
        {
            from = from.normalized;
            to = to.normalized;

            float angleRad = AngleBetween(from, to, shouldNormalize = false);

            //The determinant is similar to the dot product
            //The dot product is always 0 no matter in which direction the perpendicular vector is pointing
            //But the determinant is -1 or 1 depending on which way the perpendicular vector is pointing (up or down)
            //AngleBetween goes from 0 to 180 so we can now determine if we need to compensate to get 360 degrees
            if (Det2(from, to) > 0f)
            {
                return angleRad;
            }
            else
            {
                return (Mathf.PI * 2f) - angleRad;
            }
        }

        //The angle between two vectors 0 <= angle <= 180
        //Same as Vector2.Angle() but we are using MyVector2
        public static float AngleBetween(Vector2 from, Vector2 to, bool shouldNormalize = true)
        {
            //from and to should be normalized
            //But sometimes they are already normalized and then we dont need to do it again
            if (shouldNormalize)
            {
                from = from.normalized;
                to = to.normalized;
            }

            //dot(a_normalized, b_normalized) = cos(alpha) -> acos(dot(a_normalized, b_normalized)) = alpha
            float dot = Vector2.Dot(from, to);

            //This shouldn't happen but may happen because of floating point precision issues
            dot = Mathf.Clamp(dot, -1f, 1f);

            float angleRad = Mathf.Acos(dot);

            return angleRad;
        }



        //Add value to average
        //http://www.bennadel.com/blog/1627-create-a-running-average-without-storing-individual-values.htm
        //count - how many values does the average consist of
        public static float AddValueToAverage(float oldAverage, float valueToAdd, float count)
        {
            float newAverage = ((oldAverage * count) + valueToAdd) / (count + 1f);

            return newAverage;
        }



        //Round a value to nearest int value determined by stepValue
        //So if stepValue is 5, we round 11 to 10 because we want to go in steps of 5
        //such as 0, 5, 10, 15
        public static int RoundValue(float value, float stepValue)
        {
            int roundedValue = (int)(Mathf.Round(value / stepValue) * stepValue);

            return roundedValue;
        }
    }
}