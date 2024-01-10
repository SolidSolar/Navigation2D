using System.Collections.Generic;
using Navigation2D.Data.NavMath;
using Navigation2D.Data.Utility;
using UnityEngine;

namespace arcticDEV
{
    /// <summary>
    /// SceneManager here is our own static class, not the SceneManagement.SceneManager
    /// </summary>
    public static class TestingToolkit
    {
        public static void DrawColliderToShapeResult(Collider2D collider2D)
        {
            Shape2D shape = NavUtility.ConvertToPolygonShapes(collider2D)[0];
            List<Vector2> points = shape.Points;
            
            for (int z = 0; z <points.Count-1; z++)
            {
                Debug.DrawLine(points[z] + shape.Center, points[z+1] + shape.Center, Color.red,5f);
            }
            Debug.DrawLine(points[^1] + shape.Center,points[0] + shape.Center, Color.red,5f);
        }
    }
}
