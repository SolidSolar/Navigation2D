using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Testing/NavMath/PolygonDataset", fileName = "PolygonDataset")]
public class PolygonDataset : ScriptableObject
{
    public const string kPolygonDatasetPath = "Assets/Tests/NavMathTests/PolygonDataset.asset";
    
    public List<PolygonDatasetItem> items;
    
    [Serializable]
    public struct PolygonDatasetItem
    {
        public List<Vector2> PointsList;
        public bool SelfIntersecting;
    }
}
