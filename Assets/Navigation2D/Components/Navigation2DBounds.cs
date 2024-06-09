using UnityEngine;

namespace Navigation2D.Components
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Navigation2DBounds : MonoBehaviour
    {
        public string Area => _area;
        [SerializeField] private string _area;
        
        #if UNITY_EDITOR
        public BoxCollider2D GetCollider()
        {
            return GetComponent<BoxCollider2D>();
        }
        #endif
    }
}