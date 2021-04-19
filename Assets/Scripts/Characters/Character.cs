using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV.Characters
{
    [CreateAssetMenu(menuName = "Characters/Character")]
    public class Character : ScriptableObject
    {
        public string characterName;
        public Sprite characterPortrait;
        public GameObject projectile;
        
        public int agility = 10;
        public int rangedAttackRange;
        public int rangeEffectiveRange;
        public int rangeAccuracy;
        public int meleeAccuracy;
        public int defense;
        public int meleeAttackRange;
        public int weaponSelected;
        
        public int StartingAP 
        {
            get { return agility; }
        }
    }
}

