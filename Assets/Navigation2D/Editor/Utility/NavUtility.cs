using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Navigation2D;
using Navigation2D.NavMath;
using Navigation2D.NavMath.PolygonClipping;
using Navigation2D.NavMath.PolygonMerge;
using Navigation2D.NavMath.PolygonOutline;
using Navigation2D.NavMath.PolygonSelfIntersectionCheck;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Navigation2D.Data.Utility
{
    public static class NavUtility
    {
        public static List<Shape2D> ConvertToPolygonShapes(Collider2D collider)
        {
            List<Shape2D> shapeResult = new List<Shape2D>();
            List<Vector2> pointsResult = new List<Vector2>();
            if (collider is CapsuleCollider2D)
            {
                Shape2D shape = new Shape2D();
                CapsuleCollider2D c = (CapsuleCollider2D) collider;
                pointsResult.Add(new Vector2(-c.size.x/2, -c.size.y/2));
                pointsResult.Add(new Vector2(-c.size.x/2, c.size.y/2));
                pointsResult.Add(new Vector2(c.size.x/2, c.size.y/2));
                pointsResult.Add(new Vector2(c.size.x/2, -c.size.y/2));
                shape.Points = new List<Vector2>(pointsResult);
                shape.Center = (Vector3)c.offset + collider.transform.position;
                shapeResult.Add(shape);
            }
            if (collider is PolygonCollider2D)
            {
                Shape2D shape = new Shape2D();
                PolygonCollider2D c = (PolygonCollider2D) collider;

                if (!PolygonSelfIntersectionCheck.HasIntersections(
                    c.points.ToList()))
                {
                    for (int i = 0; i < c.pathCount; i++)
                    {
                        pointsResult = c.GetPath(i).ToList();
                        shape.Points = new List<Vector2>(pointsResult);
                        shape.Center = (Vector3) c.offset + collider.transform.position;
                        shapeResult.Add(shape);
                    }
                }
                else
                {
                    Debug.LogWarning($"Polygon collider of gameObject {collider.name} is self-intersecting, no shape generated");
                }
            }
            if (collider is BoxCollider2D)
            {
                Shape2D shape = new Shape2D();
                BoxCollider2D c = (BoxCollider2D) collider;
                pointsResult.Add(new Vector2(-c.size.x/2, -c.size.y/2));
                pointsResult.Add(new Vector2(-c.size.x/2, c.size.y/2));
                pointsResult.Add(new Vector2(c.size.x/2, c.size.y/2));
                pointsResult.Add(new Vector2(c.size.x/2, -c.size.y/2));
                shape.Points = new List<Vector2>(pointsResult);
                shape.Center = (Vector3)c.offset + collider.transform.position;
                shapeResult.Add(shape);
            }
            if (collider is TilemapCollider2D)
            {
                Tilemap tilemap = collider.GetComponent<Tilemap>();
                var cellSize = tilemap.cellSize/2f;
                foreach (var position in tilemap.cellBounds.allPositionsWithin) {
                    if (!tilemap.HasTile(position)) {
                        continue;
                    }
                    pointsResult.Clear();
                    Shape2D shape = new Shape2D();
                    Vector3Int cellPosition = tilemap.WorldToCell(position);
                    Vector3 v = position;
                    v = new Vector3(v.x *cellSize.x * 2, v.y * cellSize.y * 2, 0);
                    Vector3 _base =  new Vector3(tilemap.tileAnchor.x *cellSize.x*2 , tilemap.tileAnchor.y*cellSize.y*2, 0);
                    Vector3 p = _base + new Vector3(cellSize.x, cellSize.y, 0);
                    pointsResult.Add(p);
                    p = _base + new Vector3(cellSize.x, -cellSize.y, 0);
                    pointsResult.Add(p);
                    p = _base + new Vector3(-cellSize.x, -cellSize.y, 0);
                    pointsResult.Add(p);
                    p = _base + new Vector3(-cellSize.x, cellSize.y, 0);
                    pointsResult.Add(p);
                    shape.Points = new List<Vector2>(pointsResult);
                    shape.Center = v;
                    shapeResult.Add(shape);
                }
            }

            return shapeResult;
            
        }

        public static VisibilityGraph GenerateVisibilityGraph(List<Shape2D> shapes, float outlineRadius, Bounds graphBounds = default)
        {
            
            for (int i = 0; i < shapes.Count; i++)
            {
                shapes[i] = PolygonOutline.GetPolygonOutline(shapes[i], outlineRadius);
            }
            
           // var result = shapes.SelectMany((x, i) => shapes.Skip(i + 1),
           //     (x, y) =>
           //     {
           //         var finalPoly = GreinerHormann.ClipPolygons(x.GlobalPoints, y.GlobalPoints, true)
           //             .Select(x => new Shape2D(Vector2.zero, x)).ToList();
           //         if (finalPoly.Count == 0)
           //         {
           //             finalPoly = new List<Shape2D>() {x, y};
           //         }
           //             
           //         return finalPoly;
           //     });
           // var visibilityShapes = new List<Shape2D>();
           // foreach (var l in result)
           // {
           //     foreach (var item in l)
           //     {
           //         visibilityShapes.Add(item);
           //     }
           // }
            
            var graph = new VisibilityGraph();

            foreach (var s in shapes)
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
            
            return graph;
        }
    }
}