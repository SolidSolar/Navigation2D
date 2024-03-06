using System;
using System.Collections.Generic;
using Navigation2D.NavMath.PolygonClipping;
using NUnit.Framework;
using UnityEngine;

namespace Tests.NavMathTests
{
    public class LineSegmentsIntersectionCheck
    {
        [Test]
        public void RunLineSegments()
        {
            var linesSet =new List<dynamic>{
                new
                {
                    l_1 = new Vector2(0, 0), 
                    l_2 = new Vector2(1, 1),
                    r_1 = new Vector2(0, 1),
                    r_2 = new Vector2(1, 0),
                    type = IntersectionType.XIntersection,
                    intersections = 1,
                    intersectionPoints = new List<Vector2>
                    {
                        new (0.5f, 0.5f)
                    }
                },
                new
                {
                    l_1 = new Vector2(0, 1.5f), 
                    l_2 = new Vector2(1.5f, 3.16f),
                    r_1 = new Vector2(0, 0),
                    r_2 = new Vector2(0, 3),
                    intersections = 1,
                    type = IntersectionType.TIntersectionL,
                    intersectionPoints = new List<Vector2>
                    {
                        new(0, 1.5f)
                    }
                },
                new
                {
                    l_1 = new Vector2(0f, 1f), 
                    l_2 = new Vector2(3, 1f),
                    r_1 = new Vector2(1.5f, 1f),
                    r_2 = new Vector2(2, 3),
                    intersections = 1,
                    type = IntersectionType.TIntersectionR,
                    intersectionPoints = new List<Vector2>
                    {
                        new (1.5f, 1f)
                    }
                },
                new
                {
                    l_1 = new Vector2(1, 1), 
                    l_2 = new Vector2(3, 3),
                    r_1 = new Vector2(1, 1),
                    r_2 = new Vector2(-3, 2),
                    intersections = 1,
                    type = IntersectionType.VIntersection,
                    intersectionPoints = new List<Vector2>
                    {
                        new (1, 1)
                    }
                },
                
                new
                {
                    l_1 = new Vector2(1, 1), 
                    l_2 = new Vector2(3, 1),
                    r_1 = new Vector2(2, 1),
                    r_2 = new Vector2(0, 1),
                    intersections = 2,
                    type = IntersectionType.XOverlap,
                    intersectionPoints = new List<Vector2>
                    {
                        new (1, 1),
                        new (2, 1)
                    }
                },
                new
                {
                    l_1 = new Vector2(1, 1), 
                    l_2 = new Vector2(3, 1),
                    r_1 = new Vector2(0, 1),
                    r_2 = new Vector2(2, 1),
                    intersections = 1,
                    type = IntersectionType.TOverlapL,
                    intersectionPoints = new List<Vector2>
                    {
                        new (1, 1)
                    }
                },
                new
                {
                    l_1 = new Vector2(1, 1), 
                    l_2 = new Vector2(2, 1),
                    r_1 = new Vector2(0, 1),
                    r_2 = new Vector2(3, 1),
                    intersections = 1,
                    type = IntersectionType.TOverlapL,
                    intersectionPoints = new List<Vector2>
                    {
                        new (1, 1)
                    }
                },
                new
                {
                    l_1 = new Vector2(1, 1), 
                    l_2 = new Vector2(2, 1),
                    r_1 = new Vector2(3, 1),
                    r_2 = new Vector2(0, 1),
                    intersections = 1,
                    type = IntersectionType.TOverlapL,
                    intersectionPoints = new List<Vector2>
                    {
                        new (1, 1)
                    }
                },
                new
                {
                    l_1 = new Vector2(1, 1), 
                    l_2 = new Vector2(2, 1),
                    r_1 = new Vector2(1, 1),
                    r_2 = new Vector2(0, 1),
                    intersections = 2,
                    type = IntersectionType.VOverlap,
                    intersectionPoints = new List<Vector2>
                    {
                        new (1, 1),
                        new (1, 1)
                    }
                },
                new
                {
                    l_1 = new Vector2(1, 1), 
                    l_2 = new Vector2(3, 1),
                    r_1 = new Vector2(1, 1),
                    r_2 = new Vector2(2, 1),
                    intersections = 2,
                    type = IntersectionType.VOverlap,
                    intersectionPoints = new List<Vector2>
                    {
                        new (1, 1),
                        new (1, 1)
                    }
                },
                new
                {
                    l_1 = new Vector2(1, 1), 
                    l_2 = new Vector2(2, 1),
                    r_1 = new Vector2(1, 1),
                    r_2 = new Vector2(3, 1),
                    intersections = 2,
                    type = IntersectionType.VOverlap,
                    intersectionPoints = new List<Vector2>
                    {
                        new (1, 1),
                        new (1, 1)
                    }
                },
            };

            for (int i = 0; i < linesSet.Count; i++)
            {
                var lines = linesSet[i];
                var type = Intersections.GetIntersectionType(lines.l_1, lines.l_2, lines.r_1, lines.r_2, out List<Vector2> points);
                Assert.IsTrue(type == lines.type && points.Count == lines.intersections && !points.Exists(
                    x => !((List<Vector2>) lines.intersectionPoints).Exists(y => x == y)), 
                    $"test index is {i}\n" +
                    $"expected points are {String.Join("\n", lines.intersectionPoints)}\n" +
                    $"received points are {String.Join("\n", points)}\n" +
                    $"expected overlap type is {lines.type}\n" +
                    $"received overlap type is {type}");
            }
        }
    }
}