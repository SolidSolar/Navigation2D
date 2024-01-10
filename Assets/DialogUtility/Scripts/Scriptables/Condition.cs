using DialogUtilitySpruce.API;
using UnityEngine;

namespace DialogUtilitySpruce
{
    /// <summary>
    /// base class for all condition scriptables.
    /// </summary>
    public abstract class Condition : ScriptableObject
    {
        public abstract bool IsTrue(DialogNode node, int index);
    }
}