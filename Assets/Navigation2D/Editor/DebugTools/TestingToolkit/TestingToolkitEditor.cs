using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Navigation2D.Data.Utility;
using Navigation2D.NavMath.PolygonSelfIntersectionCheck;
using Navigation2D.Editor.NavigationEditor;
using Navigation2D.NavMath;
using Navigation2D.NavMath.PolygonClipping;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Navigation2D.Editor.DebugTools
{
    public class TestingToolkitEditor : EditorWindow
    {
        private ScrollView _list;
        [MenuItem("arcticDEV/Testing toolkit")]
        public static void OpenMergeGraphWindow()
        {
            var window = GetWindow<TestingToolkitEditor>();
            window.titleContent = new UnityEngine.GUIContent("Testing toolkit");
        }
        private void OnEnable()
        {
            _list = new ScrollView();
            rootVisualElement.Add(_list);
            
            var methods = GetType().GetRuntimeMethods()
                .Where(m => m.GetCustomAttributes(typeof(TestingToolkitItemAttribute), false).Length > 0)
                .ToArray();
            
            foreach (var method in methods)
            {
                method.Invoke(this, null);
            }
        }

        [TestingToolkitItem]
        void GenerateMesh()
        {
            Button button = new Button(() =>
            {
                var colliders = Selection.objects.Where(x=>((GameObject)x).GetComponent<Collider2D>())?.Select(x=> ((GameObject)x).GetComponent<Collider2D>()).ToList();
                var bounds = colliders[^1];
                colliders.RemoveAt(colliders.Count-1);
                var graph = new VisibilityGraph();
                var obstacles = colliders
                    .Select(x => new Shape2D(x.transform.position, ((PolygonCollider2D) x).points.ToList())).ToList();
                var graphBounds = bounds.bounds;
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
                Navigation2DEditorService.GenerateNavigationMesh(graph);
            });
            button.text = "Create mesh container";
            _list.Add(button);
        }

        [TestingToolkitItem]
        void GenerateShape()
        {
            Button button = new Button(() =>
            {
                var graph = Navigation2DEditorService.OpenContainer("randomName.asset");
                var adjMatrix = graph.GetAdjacencyMatrix();
                foreach (var kvp in adjMatrix)
                {
                    foreach (var v in kvp.Value.list)
                    {
                        Debug.DrawLine(kvp.Key.Position, v.Position, Color.blue, 15f);
                    }
                }
            });
            button.text = "Generate shape";
            _list.Add(button);
        }

        [TestingToolkitItem]
        void IsIntersecting()
        {
            Button button = new Button(() =>
            {
                var collider = ((GameObject) Selection.activeObject)?.GetComponent<PolygonCollider2D>();
                
                if(!collider)
                    return;
                
                Debug.Log($"Polygon self-intersects: {PolygonSelfIntersectionCheck.HasIntersections(collider.points.ToList())}");
            });
            button.text = "Is intersecting";
            _list.Add(button);
        }
        
        [TestingToolkitItem]
        void LogNumbers()
        {
            Button button = new Button(() =>
            {
                var collider = ((GameObject) Selection.activeObject)?.GetComponent<PolygonCollider2D>();
                
                if(!collider)
                    return;
                
                var pLength = collider.pathCount;
                List<List<Vector2>> pointLists = new();
                for (int i = 0; i < pLength; i++)
                {
                    pointLists.Add(collider.GetPath(i).ToList());
                }

                string str = "";
                foreach (var shape in pointLists)
                {
                    foreach (var v in shape)
                    {
                        str += $"{v.x}, {v.y}\n";
                    }
                }
                Debug.Log(str);
            });
            
            button.text = "LogNumbers";
            _list.Add(button);
        }

        [TestingToolkitItem]
        void MergeShapes()
        {
            Button button = new Button(() =>
            {
                var colliders = Selection.objects?.Select(x=> ((GameObject)x).GetComponent<PolygonCollider2D>()).ToList();

                List<Vector2> activeList = colliders[0].points.ToList();
                colliders.RemoveAt(0);
                while (colliders.Count > 0)
                {
                    var _next = colliders[0].points.ToList();;
                    var finalPoly = GreinerHormann.ClipPolygons(activeList, _next, true);

                    //var lis = finalPoly[0];
                    //for (int i = 0; i < lis.Count-1; i++)
                    //{
                    //    Debug.DrawLine(lis[i], lis[i+1], Color.red, 25f);
                    //}
                    
                    foreach (var l in finalPoly)
                    {
                        activeList.AddRange(l);
                    }
                    //
                    //activeList = GreinerHormann.ClipPolygons(activeList, _next, BooleanOperation.Union)[0];
                    colliders.RemoveAt(0);
                }
                
                //activeList = activeList.GroupBy(x => x).Select(y => y.First()).ToList();
                
                
                //Debug.DrawLine(lis[^1], lis[0], Color.red, 25f);

                
            });
            
            button.text = "MergeShapes";
            _list.Add(button);
        }
        
        [TestingToolkitItem]
        void GenerateVisibilityGraph()
        {
            Button button = new Button(() =>
            {
                var colliders = Selection.objects?.Select(x=> ((GameObject)x).GetComponent<Collider2D>()).ToList();
                var bounds = colliders[^1];
                colliders.RemoveAt(colliders.Count-1);
                TestingToolkit.DrawVisibilityGraph(colliders.Select(x=>new Shape2D(x.transform.position,  ((PolygonCollider2D)x).points.ToList())).ToList(), bounds.bounds);
            });
            
            button.text = "GenerateVisibilityGraph";
            _list.Add(button);
        }
        
        [TestingToolkitItem]
        void GenerateOutlineShape()
        {
            Button button = new Button(() =>
            {
                var colliders = Selection.objects?.Select(x=> ((GameObject)x).GetComponent<PolygonCollider2D>()).ToList();
                
                TestingToolkit.DrawOutlineShape(new Shape2D( colliders[0].transform.position, colliders[0].points.ToList()));
            });
            
            button.text = "GenerateOutlineShape";
            _list.Add(button);
        }
        
        [TestingToolkitItem]
        void GenerateGrahAndFindRoute()
        {
            Button button = new Button(() =>
            {
                var colliders = Selection.objects?.Where(x=>((GameObject)x).GetComponent<Collider2D>()).Select(x=> ((GameObject)x).GetComponent<Collider2D>()).ToList();
                var bounds = colliders[^1];
                colliders.RemoveAt(colliders.Count-1);
                var shapes = new List<Shape2D>();
                foreach (var c in colliders)
                {
                    var res = NavUtility.ConvertToPolygonShapes(c);
                    shapes.AddRange(res);
                }
                
                
                TestingToolkit.GenerateVisibilityGraphAndFindRoute(shapes, 
                    ((GameObject)Selection.objects[^3]).transform.position, 
                    ((GameObject)Selection.objects[^2]).transform.position, bounds.bounds);
            });
            
            button.text = "GenerateGrahAndFindRoute";
            _list.Add(button);
        }
        
        [TestingToolkitItem]
        void OutlineMergeVisibility()
        {
            Button button = new Button(() =>
            {
                var colliders = Selection.objects?.Select(x=> ((GameObject)x).GetComponent<PolygonCollider2D>()).ToList();
                TestingToolkit.OutlineMergeVisibiltiy(colliders.Select(x=>new Shape2D( x.transform.position, x.points.ToList())).ToList());
            });
            
            button.text = "OutlineMergeVisibility";
            _list.Add(button);
        }

        [TestingToolkitItem]
        void TestLineNumbers()
        {
            Button button = new Button(() =>
            {
                List<Vector2> points = new();
                var number = Intersections.GetIntersectionType(new Vector2(0, 0), new Vector2(1, 1), new Vector2(1, 1),
                    new Vector2(2, 2), out points);
                Debug.Log($"number of intersections: {number}\n" +
                          $"intersections: \n" +
                          String.Join("\n", points.Select(x=>$"{x}").ToList()));
                number = Intersections.GetIntersectionType(new Vector2(0, 0), new Vector2(1, 1), new Vector2(1, 1),
                    new Vector2(2, 4), out points);
                Debug.Log($"number of intersections: {number}\n" +
                          $"intersections: \n" +
                          String.Join("\n", points.Select(x=>$"{x}").ToList()));
                number = Intersections.GetIntersectionType(new Vector2(0, 0), new Vector2(2, 2), new Vector2(1, 1),
                    new Vector2(3, 3), out points);
                Debug.Log($"number of intersections: {number}\n" +
                          $"intersections: \n" +
                          String.Join("\n", points.Select(x=>$"{x}").ToList()));
                number = Intersections.GetIntersectionType(new Vector2(0, 0), new Vector2(2, 2), new Vector2(0, 2),
                    new Vector2(2, 0), out points);
                Debug.Log($"number of intersections: {number}\n" +
                          $"intersections: \n" +
                          String.Join("\n", points.Select(x=>$"{x}").ToList()));
            });
            
            button.text = "Get to and t1";
            _list.Add(button);
        }
    }
}
