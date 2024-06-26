﻿using UnityEngine;

namespace Navigation2D.Components
{
    public class Navigation2DObstacle : MonoBehaviour
    {
        public string Area => _area;
        [SerializeField] private string _area;

        #if UNITY_EDITOR
        public Collider2D[] GetColliders()
        {
            return GetComponents<Collider2D>();
        }
        #endif
    }
}