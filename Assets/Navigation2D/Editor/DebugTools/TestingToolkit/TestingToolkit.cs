using System.Collections.Generic;
using Navigation2D.NavMath;
using Navigation2D.Data.Utility;
using Navigation2D.NavMath.PolygonOutline;
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

        public static void DrawVisibilityGraph(List<Shape2D> obstacles, Bounds graphBounds = default)
        {
            var graph = new VisibilityGraph();

            foreach (var s in obstacles)
            {
                graph.AddPolygon(new Polygon(s.GlobalPoints.ToArray()));
            }

            if (graphBounds != default)
            {
                graph.AddPolygon(new Polygon(new Vector2[]
                {
                    new(graphBounds.min.x, graphBounds.min.y), new(graphBounds.min.x + graphBounds.extents.x*2, graphBounds.min.y),
                    new(graphBounds.max.x, graphBounds.max.y), new(graphBounds.max.x - graphBounds.extents.x*2, graphBounds.max.y)
                }));
            }

            var adjMatrix = graph.GetAdjacencyMatrix();
            
            foreach (var kvp in adjMatrix)
            {
                foreach (var v in kvp.Value.list)
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

        public static void GenerateVisibilityGraphAndFindRoute(List<Shape2D> obstacles, Vector2 pos1, Vector2 pos2, Bounds graphBounds = default)
        {
            var graph = NavUtility.GenerateVisibilityGraph(obstacles, 0.06f, graphBounds);

            var adjMatrix = graph.GetAdjacencyMatrix();
            foreach (var kvp in adjMatrix)
            {
                foreach (var v in kvp.Value.list)
                {
                    Debug.DrawLine(kvp.Key.Position, v.Position, Color.blue, 15f);
                }
            }
            var  path= graph.GetPath(pos1, pos2);
            
            for (int i = 0; i < path.Length-1; i++)
            {
                Debug.DrawLine(path[i], path[i+1], Color.cyan, 25f);
            }
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
            
            
            var adjMatrix = graph.GetAdjacencyMatrix();
            foreach (var kvp in adjMatrix)
            {
                foreach (var v in kvp.Value.list)
                {
                    Debug.DrawLine(kvp.Key.Position, v.Position, Color.blue, 15f);
                }
            }
        }
    }
}
