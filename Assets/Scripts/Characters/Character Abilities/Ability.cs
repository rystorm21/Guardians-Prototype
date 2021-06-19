using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV 
{
    public enum AbilityType
    {
        Ranged,
        RangedAoe,
        Self,
        PBAoE,
        Passive
    }

    public class Ability : ScriptableObject
    {
        public string abilityName;
        public GameObject prefab;
        public AbilityType type;
        public int apCost;
        public int duration;
        public int coolDown;
        public int radius;
        [TextArea(15, 20)]
        public string description;
    }
}

