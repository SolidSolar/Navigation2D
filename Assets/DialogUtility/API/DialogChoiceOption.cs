using System;
using UnityEngine;

namespace DialogUtilitySpruce.API
{
    [Serializable]
    public class DialogChoiceOption
    {
        public DialogChoiceOption(PortConditionData data, int index)
        {
            this.data = data;
            this.index = index;
        }

        public int Index => index;
        public Condition Condition => data.condition;
        
        [SerializeField]
        private PortConditionData data;
        [SerializeField]
        private int index;
    }
}