using System.Collections.Generic;
using System.Linq;
using Navigation;
using Navigation2D.NavMath;
using Navigation2D.Data.Utility;
using Navigation2D.NavMath.PolygonClipping;
using Navigation2D.NavMath.PolygonOutline;
using Navigation2D.NavMath.VisibilityGraph;
using UnityEngine;

namespace Navigation2D.Editor.DebugTools
{
    /// <summary>
    /// SceneManager here is our own static class, not the SceneManagement.SceneManager
    /// </summary>
    public static class TestingToolkit
    {
        public static void DrawColliderToShapeResult(Collider2D collider2D)
        {
            List<Shape2D> shapes = NavUtility.ConvertToPolygonShapes(collider2D);
            foreach (var shape in shapes)
            {
                List<Vector2> points = shape.Points;

                for (int z = 0; z < points.Count - 1; z++)
                {
                    Debug.DrawLine(points[z] + shape.Center, points[z + 1] + shape.Center, Color.red, 10f);
                }

                Debug.DrawLine(points[^1] + shape.Center, points[0] + shape.Center, Color.red, 5f);
            }
        }

        public static void DrawVisibilityGraph(List<Shape2D> obstacles)
        {
            var graph = new VisibilityGraph();

            foreach (var s in obstacles)
            {
                graph.AddPolygon(new Polygon(s.GlobalPoints.ToArray()));
            }
            
            foreach (var kvp in graph._adjList)
            {
                foreach (var v in kvp.Value)
                {
                    Debug.DrawLine(kvp.Key.Position, v.Position, Color.blue, 15f);
                }
            }
        }

        public static void DrawOutlineShape(Shape2D shape)
        {
            var result =  PolygonOutline.GetPolygonOutline(shape, 0.05f);
            for (int z = 0; z < result.Points.Count - 1; z++)
            {
                Debug.DrawLine(result.Points[z] + result.Center, result.Points[z + 1] + result.Center, Color.red, 10f);
            }
            Debug.DrawLine(result.Points[^1] + result.Center, result.Points[0] + result.Center, Color.red, 10f);
        }

        public static void OutlineMergeVisibiltiy(List<Shape2D> shapes)
        {
            List<Shape2D> finalShapes = new List<Shape2D>();

            foreach (var s in shapes)
            {
                finalShapes.Add(PolygonOutline.GetPolygonOutline(s, 0.05f));
            }

            //for (int i = 0; i < finalShapes.Count-1; i++)
            //{
            //    var finalPoly = GreinerHormann.ClipPolygons(finalShapes[i].GlobalPoints, finalShapes[i+1].GlobalPoints, true);
//
            //    if (finalPoly.Count > 0)
            //    {
            //        finalShapes[i].Points = finalPoly[0];
            //    }
//
            //    finalShapes.Remove(finalShapes[i + 1]);
            //}
            
            var graph = new VisibilityGraph();

            foreach (var s in finalShapes)
            {
                graph.AddPolygon(new Polygon(s.GlobalPoints.ToArray()));
            }
            
            
            
            foreach (var kvp in graph._adjList)
            {
                foreach (var v in kvp.Value)
                {
                    Debug.DrawLine(kvp.Key.Position, v.Position, Color.blue, 15f);
                }
            }
        }
    }
}
