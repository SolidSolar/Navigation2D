using System.Collections.Generic;
using UnityEngine;

namespace DialogUtilitySpruce.Editor
{
    public class UndoTrackedObjectsList : List<Object>
    {
        public static UndoTrackedObjectsList Instance { get; private set; }

        public static void Initialize()
        {
            Instance = new UndoTrackedObjectsList();
        }

        public static void Destroy()
        {
            Instance = null;
        }
    }
}