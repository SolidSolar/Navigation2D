using System;
using System.Collections.Generic;
using UnityEngine;

namespace Navigation2D.NavMath.VisibilityGraph
{
    [Serializable]
    public class VisibilityGraphData
    {
        public DictionaryOfVectorAndDictionary AdjacencyDictionary = new();
    }
}