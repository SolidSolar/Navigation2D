using UnityEngine;

namespace DialogUtilitySpruce.API
{
    public class Character
    {
        public Character(CharacterData data)
        {
            Name = data.Name;
            Icon = data.icon;
        }

        public string Name { get; }
        public Sprite Icon { get; }
    }
}