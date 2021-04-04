using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV.Characters
{
    [CreateAssetMenu(menuName = "Characters/Character")]
    public class Character : ScriptableObject
    {
        public string characterName;
        public Sprite portrait;
        
        public int agility = 10;
        public int rangedAttackRange;
        public int rangeEffectiveRange;
        public int meleeAttackRange;
        
        public int StartingAP 
        {
            get { return agility; }
        }
    }
}

