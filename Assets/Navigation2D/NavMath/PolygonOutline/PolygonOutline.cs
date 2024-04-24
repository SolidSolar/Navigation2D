using System.Collections.Generic;
using UnityEngine;

namespace Navigation2D.NavMath.PolygonOutline
{
    public class PolygonOutline
    {
        private const float angleStep = 90;
        
        public static Shape2D GetPolygonOutline(Shape2D shape, float radius)
        {
            var resultPoints = new List<Vector2>();
            for (int i = 0; i < shape.Points.Count; i++)
            {
                var iMin1 = NavMath.ClampListIndex(i - 1, shape.Points.Count);
                var iPlus1 = NavMath.ClampListIndex(i + 1, shape.Points.Count);
                float angle =Vector2.SignedAngle(shape.Points[iMin1] - shape.Points[i],shape.Points[iPlus1] - shape.Points[i]);
                
                var outerAngle = angle;

                if (outerAngle <0)
                {
                    outerAngle = -outerAngle;
                    var dist = radius / Mathf.Sin(outerAngle * Mathf.Deg2Rad /2);
                    Vector2 newPoint = shape.Points[i] - (Vector2)(Quaternion.AngleAxis(outerAngle / 2 , Vector3.back) *
                                       (shape.Points[i] - shape.Points[iMin1]).normalized * dist);
                    resultPoints.Add(newPoint);
                }
                else
                {
                    var a = shape.Points[i] + Vector2.Perpendicular(shape.Points[i] - shape.Points[iMin1]).normalized * radius;
                    var b =  shape.Points[i] - Vector2.Perpendicular(shape.Points[i] - shape.Points[iPlus1]).normalized * radius;
                    resultPoints.Add(a);
                    outerAngle = Vector2.Angle(a- shape.Points[i] , b - shape.Points[i] );
                    var steps = (int) ((outerAngle) / angleStep) + 1;
                    var anglePerStep = outerAngle/(steps+1);
                    var oldVec = a;
                    for (int v = 0; v < steps; v++)
                    {
                        var dist = radius /  Mathf.Cos(anglePerStep * Mathf.Deg2Rad/2);
                        Vector2 newPoint = shape.Points[i] + (Vector2)(Quaternion.AngleAxis(anglePerStep, Vector3.back) *
                                                                       (oldVec - shape.Points[i]).normalized * dist);
                        oldVec = newPoint;
                        resultPoints.Add(newPoint);
                    }
                    resultPoints.Add(b);
                }
            }
            var result = new Shape2D(shape.Center, resultPoints);
            return result;
        }
    }
}