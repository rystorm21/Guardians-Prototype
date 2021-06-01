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
        public GameObject prefab;
        public AbilityType type;
        [TextArea(15, 20)]
        public string description;
    }
}

