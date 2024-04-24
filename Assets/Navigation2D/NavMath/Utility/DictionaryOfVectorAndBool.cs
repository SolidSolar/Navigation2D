using System;
using UnityEngine;

namespace Navigation2D.NavMath.VisibilityGraph
{
    [Serializable]
    public class DictionaryOfVectorAndBool : SerializableDictionary<Vector2, bool>
    {
    }
    
    [Serializable]
    public class DictionaryOfVectorAndDictionary : SerializableDictionary<Vector2, DictionaryOfVectorAndBool>
    {
    }
}