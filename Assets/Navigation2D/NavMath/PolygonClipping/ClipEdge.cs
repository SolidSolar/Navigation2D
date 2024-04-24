using System;
using System.Collections.Generic;
using UnityEngine;

namespace Navigation2D.NavMath.PolygonClipping
{
    public class ClipEdge
    {
        public List<Tuple<ClipVertex, float>> vertexes;

        public void AddVertex(ClipVertex vertex)
        {
            int smallestPos = 1;
            for (int i = 0; i < vertexes.Count; i++)
            {
                if(Vector2.Distance(vertexes[smallestPos].Item1.coordinate, vertexes[i].Item1.coordinate) >= 
                   Vector2.Distance(vertexes[smallestPos].Item1.coordinate, vertex.coordinate))
                    break;
                
                if (Vector2.Distance(vertexes[smallestPos].Item1.coordinate, vertex.coordinate) >= Vector2.Distance(vertexes[i].Item1.coordinate, vertex.coordinate))
                {
                    smallestPos = i;
                }
            }
            vertexes.Insert(smallestPos, new(vertex, 
                Vector2.Distance(vertex.coordinate, vertexes[0].Item1.coordinate)/ 
                Vector2.Distance(vertexes[1].Item1.coordinate, vertexes[0].Item1.coordinate)               ));
        }
        
        public ClipVertex GetClosestVertex(Vector2 position)
        {
            ClipVertex smallest = vertexes[0].Item1;
            for (int i = 0; i < vertexes.Count; i++)
            {
                if(Vector2.Distance(smallest.coordinate, vertexes[i].Item1.coordinate)> Vector2.Distance(smallest.coordinate, position))
                    break;
                if (Vector2.Distance(smallest.coordinate, position) >= Vector2.Distance(vertexes[i].Item1.coordinate, position))
                {
                    smallest = vertexes[i].Item1;
                }
            }

            return smallest;
        }
    }
}