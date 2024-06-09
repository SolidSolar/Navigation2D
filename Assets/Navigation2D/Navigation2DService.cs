using System.Collections.Generic;
using System.IO;
using Navigation2D.Editor.Data.DataContainers;
using UnityEngine;

namespace Navigation2D.Data
{
    public class Navigation2DService
    {
        private static Dictionary<string, NavigationDataContainer> _cachedContainers = new ();
        
        public static readonly string DefaultNavigationContainersPath = Path.Combine("Navigation2D", "Navigation2DContainers");

        public static Vector2[] GetPath(Vector2 a, Vector2 b, string area, string agent)
        {
            NavigationDataContainer container = null;
            if (!_cachedContainers.ContainsKey(area))
            {
                container = Resources.Load<NavigationDataContainer>(Path.Combine(DefaultNavigationContainersPath, area + "_" + agent));
                if (container)
                {
                    _cachedContainers.Add(area, container);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                container = _cachedContainers[area];
            }

            return container.VisibilityGraph.GetPath(a, b);
        }
    }
}