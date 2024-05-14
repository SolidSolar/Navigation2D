using System;
using System.Collections.Generic;
using UnityEngine;

namespace Navigation2D.NavMath
{
    [Serializable]
    public class DictionaryOfVectorAndBool : SerializableDictionary<Vector2, bool>
    {
    }
    
    [Serializable]
    public class DictionaryOfVectorAndDictionary : SerializableDictionary<Vector2, DictionaryOfVectorAndBool>
    {
    }
    
    [Serializable]
    public class DictionaryOfVertexAndListVertex : ReferenceValueSerializableDictionary<Vertex, ReferenceSerializableList<Vertex>>
    {
    }

    [Serializable]
    public class ReferenceSerializableList<T>
    {
        [SerializeReference]
        public List<T> list = new();
    }
    
    [Serializable]
    public class DictionaryOfVertexAndFloat : ReferenceValueSerializableDictionary<Vertex, float>
    {
    }
    
    [Serializable]
    public class DictionaryOfVertexAndVertex : ReferenceSerializableDictionary<Vertex, Vertex>
    {
    }
}