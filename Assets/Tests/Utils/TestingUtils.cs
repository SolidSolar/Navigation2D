using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TestingUtils : MonoBehaviour
{
    public static void AddShape2DToPolygonDataset(List<Vector2> points, bool selfIntersecting)
    {
        var dataset = AssetDatabase.LoadAssetAtPath<PolygonDataset>(PolygonDataset.kPolygonDatasetPath);
        EditorUtility.SetDirty(dataset);
        Undo.RecordObject(dataset, $"Add {(selfIntersecting ? "self-intersecting" : "non self-intersecting")} item to polygon dataset");
        dataset.items.Add(new PolygonDataset.PolygonDatasetItem{PointsList = points, SelfIntersecting = selfIntersecting});
        AssetDatabase.SaveAssets();
        EditorUtility.ClearDirty(dataset);
        
    }
}
