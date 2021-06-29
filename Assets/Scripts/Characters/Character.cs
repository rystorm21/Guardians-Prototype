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
        public Inventory inventory;
        public List<AbilitySlot> abilityPool;
        public List<Ability> appliedBuffs; // make private when done testing
        private List<Ability> appliedDebuffs;
        public int maxHitPoints;
        public int hitPoints;
        public int agility = 10;
        public int rangedAttackRange;
        public int rangeEffectiveRange;
        public int attackAccuracy;
        public int attackDamage;
        public int defense;
        public int damageResist;
        public int meleeAttackRange;
        public int weaponSelected;
        public int abilitySelected;

        public int fightingStyle;
        public int currentStance = 1;
        public bool teamLeader = false;
        public bool braced = false;
        public bool KO = false;

        // change these to private later maybe
        public float buffAcc;
        public float buffActionPoints;
        public float buffDefense;
        public float buffMeleeDmg;
        public float buffRangeDmg;
        public float buffDmgRes;
        
        public int StartingAP 
        {
            get { return agility; }
        }

        public int NonCombatAPCap
        {
            get { return nonCombatAPCap; }
            set { nonCombatAPCap = value; }
        }

        public List<Ability> GetAppliedBuffs()
        {
            return appliedBuffs;
        }

        public void AddAppliedBuff(Ability buff)
        {
            if (buff.duration > 0)
                appliedBuffs.Add(buff);
        }

        public List<Ability> GetAppliedDebuffs()
        {
            return appliedDebuffs;
        }

        public void AddAppliedDebuff(Ability debuff)
        {
            appliedDebuffs.Add(debuff);
        }

        #region Status Effect Management
        public void ClearAllStatus()
        {
            appliedBuffs.Clear();
            appliedDebuffs.Clear();
        }

        public void CycleStatus()
        {
            List<Ability> buffsToRemove = new List<Ability>();
            foreach (Ability buff in appliedBuffs)
            {
                if (buff.durationCountdown == 0)
                    buffsToRemove.Add(buff);
            }
            if (buffsToRemove.Count > 0)
            {
                foreach (Ability buff in buffsToRemove)
                {
                    appliedBuffs.Remove(buff);
                }
            }
        }

        public void ApplyBuffs()
        {
            foreach (Ability buff in appliedBuffs)
            {
                buffAcc += buff.buffAcc;
                buffActionPoints += buff.buffActionPoints;
                buffDefense += buff.buffDefense;
                buffMeleeDmg += buff.buffMeleeDmg;
                buffRangeDmg += buff.buffRangeDmg;
                buffDmgRes += buff.buffDmgRes;
            }
        }

        public void ApplyBuffs(Ability buff)
        {
            buffAcc += buff.buffAcc;
            buffActionPoints += buff.buffActionPoints;
            buffDefense += buff.buffDefense;
            buffMeleeDmg += buff.buffMeleeDmg;
            buffRangeDmg += buff.buffRangeDmg;
            buffDmgRes += buff.buffDmgRes; 
        }

        public void ZeroBuffs()
        {
            buffAcc = 0;
            buffActionPoints = 0;
            buffDefense = 0;
            buffMeleeDmg = 0;
            buffRangeDmg = 0;
            buffDmgRes = 0;
        }

        public void ApplyDebuffs()
        {

        }
        #endregion
    }
}

