using System.Collections.Generic;
using System.IO;
using System.Linq;
using Navigation2D.Components;
using Navigation2D.Data.Utility;
using Navigation2D.Editor.Data;
using Navigation2D.Editor.Data.DataContainers;
using Navigation2D.NavMath;
using Navigation2D.NavMath.PolygonOutline;
using UnityEditor;
using UnityEngine;

namespace Navigation2D.Editor.NavigationEditor
{
    public class Navigation2DEditorService
    {
        public static ConfigurationDataContainer Configuration
        {
            get
            {
                if (_configInstance == null)
                {
                    _configInstance =
                        AssetDatabase.LoadAssetAtPath<ConfigurationDataContainer>(ConfigurationContainerPath);
                }

                return _configInstance;
            }
        }

        private static ConfigurationDataContainer _configInstance;
        private static NavigationSaveUtility SaveUtility { get; set; } = NavigationSaveUtility.GetNavigationSaveUtilityInstance();
        private static NavigationDataContainer _container;

        public static readonly string DefaultNavigationContainersPath = Path.Combine("Assets", "Resources", "Navigation2D", "Navigation2DContainers");
        public static readonly string ConfigurationContainerPath = Path.Combine("Assets", "Navigation2D", "Data",  "DataContainers","ConfigurationDataContainer.asset");

        public static VisibilityGraph OpenContainer(string fileName)
        {
            var path = Path.Combine(DefaultNavigationContainersPath, fileName);
            _container = SaveUtility.LoadContainer(path);
            return _container.VisibilityGraph;
        }
        
        public static void GenerateNavigationMesh(string area, string agent)
        {
            var obstacles = GameObject.FindObjectsOfType<Navigation2DObstacle>().Where(x=>x.Area == area).ToList();
            var boundsList = GameObject.FindObjectsOfType<Navigation2DBounds>().ToList();
            if (obstacles.Count == 0)
            {
                return;
            }

            var bounds = boundsList.FirstOrDefault(x => x.Area == area);
            
            var colliderShapes = obstacles.Select(x => x.GetColliders());
            List<Shape2D> shapes = new List<Shape2D>();

            foreach (var c in colliderShapes)
            {
                shapes.AddRange(c.Select(x=>new Shape2D(x.transform.position, ((PolygonCollider2D) x).points.ToList())));
            }

            for (int index = 0; index < shapes.Count; index++)
            {
                shapes[index] = PolygonOutline.GetPolygonOutline(shapes[index],
                    Configuration.agentRadius[Configuration.agentNames.IndexOf(agent)]);
            }

            var graph = new VisibilityGraph();
            var graphBounds = bounds.GetCollider().bounds;
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

            if (_container == null)
            {
                _container = SaveUtility.CreateContainer(Path.Combine(DefaultNavigationContainersPath), area + "_"+agent);
            }

            EditorUtility.SetDirty(_container);
            Undo.RecordObject(_container, "Writing VisibilityGraph value");
            _container.VisibilityGraph = graph;
            SaveUtility.SaveContainer(_container);
        }

        public static void UnloadContainer()
        {
            _container = null;
        }
    }
}