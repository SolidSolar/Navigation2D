using System;
using UnityEngine;

namespace Navigation2D.NavMath.PolygonSelfIntersectionCheck
{
    public class Line2D
    {
        /// <summary>
        /// The line at the horizon (not on the Eucledian plane).
        /// </summary>
        public static readonly Line2D Horizon = new Line2D(0, 0, 1);

        public Line2D(float a, float b, float c)
        {
            this.Coeff = (a, b, c);
            float m = (float)Mathf.Sqrt(a * a + b * b);
            this.IsFinite = m > float.Epsilon;
        }
        /// <summary>
        /// The (a,b,c) coefficients define a line by the equation: <code>a*x+b*y+c=0</code>
        /// </summary>
        public (float a, float b, float c) Coeff { get; }

        /// <summary>
        /// True if line is in finite space, False if line is at horizon.
        /// </summary>
        public bool IsFinite { get; }
        
        public static bool TryJoin(Vector2 a, Vector2 b, out Line2D line)
        {
            float dx = b.x - a.x, dy = b.y - a.y;
            float m = a.x * b.y - a.y * b.x;
            line = new Line2D(-dy, dx, m);
            return line.IsFinite;
        }
        
        public static bool TryMeet(Line2D l, Line2D m, out Vector2 point)
        {
            (float a1, float b1, float c1) = l.Coeff;
            (float a2, float b2, float c2) = m.Coeff;
            
            float d = a1 * b2 - a2 * b1;
            if (d != 0)
            {
                point = new Vector2((b1 * c2 - b2 * c1) / d, (a2 * c1 - a1 * c2) / d);
                return true;
            }
            point = Vector2.zero;
            return false;
        }

        /// <summary>
        /// Check if point belongs to the infinite line.
        /// </summary>
        /// <param name="point">The target point.</param>
        /// <returns>True if point is one the line.</returns>
        public bool Contains(Vector2 point)
        {
            return IsFinite 
                   && Math.Abs(Coeff.a * point.x + Coeff.b * point.y + Coeff.c) <=  float.Epsilon;
        }

        /// <summary>
        /// Projects a target point onto the line.
        /// </summary>
        /// <param name="target">The target point.</param>
        /// <returns>The point on the line closest to the target.</returns>
        /// <remarks>If line is not finite the resulting point has NaN or Inf coordinates.</remarks>
        public Vector2 Project(Vector2 target)
        {
            (float a, float b, float c) = Coeff;
            float m2 = a * a + b * b;
            float px = b * (b * target.x - a * target.y) - a * c;
            float py = a * (a * target.y - b * target.x) - b * c;
            return new Vector2(px / m2, py / m2);
        }
        public override string ToString() => $"({Coeff.a})*x+({Coeff.b})*y+({Coeff.c})=0";
    }
}