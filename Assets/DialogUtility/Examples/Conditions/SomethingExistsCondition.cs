using DialogUtilitySpruce.API;
using UnityEngine;

namespace DialogUtilitySpruce.Examples
{
    [CreateAssetMenu(menuName = "DialogUtility/Examples/SomethingExistsCondition")]
    public class SomethingExistsCondition : Condition
    {
        public string[] objectName;
        public override bool IsTrue(DialogNode node, int index)
        {
            return GameObject.Find(objectName[index]);
        }
    }
}