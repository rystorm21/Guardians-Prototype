using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV.Characters
{
    [CreateAssetMenu(menuName = "Characters/Character")]
    public class Character : ScriptableObject
    {
        private int nonCombatAPCap = 100;

        public string characterName;
        public Sprite characterPortrait;
        public int characterArchetype;
        public GameObject projectile;
        
        public int hitPoints;
        public int agility = 10;
        public int rangedAttackRange;
        public int rangeEffectiveRange;
        public int attackAccuracy;
        public int attackDamage;
        public int defense;
        public int meleeAttackRange;
        public int weaponSelected;

        public int fightingStyle;
        public int currentStance = 1;
        public bool teamLeader = false;
        public bool braced = false;
        public bool KO = false;
        
        public int StartingAP 
        {
            get { return agility; }
        }

        public int NonCombatAPCap
        {
            get { return nonCombatAPCap; }
            set { nonCombatAPCap = value; }
        }
    }
}

