using System.IO;
using Navigation2D.Editor.Data;
using Navigation2D.Editor.Data.DataContainers;
using UnityEditor;
using UnityEngine;

namespace Navigation2D.Editor.NavigationEditor
{
    public class Navigation2DEditorService
    {
        private static NavigationSaveUtility SaveUtility { get; set; } = NavigationSaveUtility.GetNavigationSaveUtilityInstance();
        private static NavigationDataContainer _container;

        public static readonly string DefaultNavigationContainersPath = Path.Combine("Assets", "Resources", "Navigation2D", "Navigation2DContainers");

        public static void OpenContainer(string path)
        {
            SaveUtility.LoadContainer(path);
        }
        
        public static void GenerateNavigationMesh()
        {
            if (_container == null)
            {
                _container = SaveUtility.CreateContainer(DefaultNavigationContainersPath);
            }

            EditorUtility.SetDirty(_container);
            Undo.RecordObject(_container, "Writing proofThatThisShitWorks value");
            _container.proofThatThisShitWorks = "this shit does work, wow!";
            
            SaveUtility.SaveContainer(_container);
        }

        public static void UnloadContainer()
        {
            _container = null;
        }
    }
}