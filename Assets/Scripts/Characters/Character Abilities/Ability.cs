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
        public bool buff;
        public int apCost;
        public float damageModifier;
        public float healingModifier;
        public float buffAcc;
        public float buffActionPoints;
        public float buffDefense;
        public float buffMeleeDmg;
        public float buffRangeDmg;
        public float buffDmgRes;
        public float debuffAcc;
        public float debuffActionPoints;
        public float debuffDefense;
        public float debuffMeleeDmg;
        public float debuffRangeDmg;
        public float debuffDmgRes;
        public int duration;
        public int durationCountdown;
        public int coolDown;
        public int radius;
        [TextArea(15, 20)]
        public string description;
    }
}

