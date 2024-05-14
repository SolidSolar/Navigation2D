using System.IO;
using Navigation2D.Editor.Data;
using Navigation2D.Editor.Data.DataContainers;
using Navigation2D.NavMath;
using UnityEditor;

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
        
        public static void GenerateNavigationMesh(VisibilityGraph graph)
        {
            if (_container == null)
            {
                _container = SaveUtility.CreateContainer(Path.Combine(DefaultNavigationContainersPath));
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