using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Navigation2D.NavMath
{
    [Serializable]
    public class Shape2D
    {
        public Shape2D(Vector2 center, List<Vector2> points)
        {
            Center = center;
            Points = points;
            GlobalPoints = points.Select(x => x + center).ToList();
        }
        public Shape2D()
        {
            
        }
        public Vector2 Center;
        public List<Vector2> Points;
        public List<Vector2> GlobalPoints;
        
    }

}