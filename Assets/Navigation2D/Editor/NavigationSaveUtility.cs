using System.IO;
using System.Linq;
using Navigation2D.Editor.Data.DataContainers;
using UnityEditor;
using UnityEngine;

namespace Navigation2D.Editor.Data
{
    public class NavigationSaveUtility
    {
        public static NavigationSaveUtility GetNavigationSaveUtilityInstance()
        {
            return new NavigationSaveUtility();
        }
        
        public void SaveContainer(NavigationDataContainer container)
        {
            var path = AssetDatabase.GetAssetPath(container);
            
            //file lost? w/e wrong scenario is
            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog("NavigationDataContainer path not found.", "File might be lost or corrupted", "Ok, whatever");
            }

            _cleanContainer(container);
            
            AssetDatabase.SaveAssets();
        }

        public NavigationDataContainer CreateContainer(string path, string name)
        {
            string projectFolder = Path.GetDirectoryName(Application.dataPath);
            projectFolder = Path.Join(projectFolder, path);
            if (!Directory.Exists(projectFolder))
            {
                Directory.CreateDirectory(projectFolder);
            }
            
            AssetDatabase.Refresh();

            
            if (!AssetDatabase.IsValidFolder(path))
            {
                EditorUtility.DisplayDialog("Path is invalid", "path " + path + " is not valid", "Ok, whatever");
                return null;
            }
            
            var container = ScriptableObject.CreateInstance<NavigationDataContainer>();
            container.name = name;
            AssetDatabase.CreateAsset(container,Path.Join(path, name+".asset"));

            AssetDatabase.SaveAssets();
            return container;
        }

        public NavigationDataContainer LoadContainer(string path)
        {
            NavigationDataContainer container = null;
            
            if (!string.IsNullOrEmpty(path))
            {
                container = AssetDatabase.LoadAssetAtPath<NavigationDataContainer>(path);
            }
            
            if (!container)
            {
                EditorUtility.DisplayDialog("NavigationDataContainer not found at path: " + path, "File might not exist", "Ok, whatever");
                return null;
            }
            
            _cleanContainer(container);
            
            return container;
        }

        private void _cleanContainer(NavigationDataContainer container)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(container));
            if (assets.Length > 0)
            {
                foreach (var obj in assets)
                {
                    if (!obj){
                        Object.DestroyImmediate(obj, true);
                    }
                }
            }
        }

        private void _setContainerDirty(NavigationDataContainer container, bool setDirty)
        {
            if (setDirty)
            {
                EditorUtility.SetDirty(container);
            }
            else
            {
                EditorUtility.ClearDirty(container);
            }
            
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(container));
            if (assets.Length > 0)
            {
                foreach (var obj in assets)
                {
                    if (setDirty)
                    {
                        EditorUtility.SetDirty(obj);
                    }
                    else
                    {
                        EditorUtility.ClearDirty(obj);
                    }
                }
            }
        }
    }
}