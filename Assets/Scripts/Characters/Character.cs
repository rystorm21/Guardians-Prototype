using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV.Characters
{
    public enum Archetype
    {
        Defender,
        Blaster,
        Tanker
    }

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
        public List<Ability> appliedStatus; // make private when done testing
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

        public float debuffAcc;
        public float debuffActionPoints;
        public float debuffDefense;
        public float debuffMeleeDmg;
        public float debuffRangeDmg;
        public float debuffDmgRes;
        
        public int StartingAP 
        {
            get { return agility; }
        }

        public int NonCombatAPCap
        {
            get { return nonCombatAPCap; }
            set { nonCombatAPCap = value; }
        }

        public List<Ability> GetAppliedStatus()
        {
            return appliedStatus;
        }

        public void AddAppliedStatus(Ability status)
        {
            status.durationCountdown = status.duration;
            if (!appliedStatus.Contains(status))
                appliedStatus.Add(status);
        }

        #region Status Effect Management
        public void ClearAllStatus()
        {
            appliedStatus.Clear();
        }

        public void CycleStatus()
        {
            List<Ability> statusToRemove = new List<Ability>();
            foreach (Ability ability in appliedStatus)
            {
                if (ability.durationCountdown == 0)
                    statusToRemove.Add(ability);
            }
            if (statusToRemove.Count > 0)
            {
                foreach (Ability buff in statusToRemove)
                {
                    appliedStatus.Remove(buff);
                }
            }
        }

        public void ApplyStatus()
        {
            foreach (Ability status in appliedStatus)
            {
                ApplyAll(status);
            }
        }

        public void ApplyStatus(Ability status)
        {
            if (!appliedStatus.Contains(status))
                ApplyAll(status);
        }

        public void ApplyAll(Ability status)
        {
            buffAcc += status.buffAcc;
            buffActionPoints += status.buffActionPoints;
            buffDefense += status.buffDefense;
            buffMeleeDmg += status.buffMeleeDmg;
            buffRangeDmg += status.buffRangeDmg;
            buffDmgRes += status.buffDmgRes;

            debuffAcc += status.debuffAcc;
            debuffActionPoints += status.debuffActionPoints;
            debuffDefense += status.debuffDefense;
            debuffMeleeDmg += status.debuffMeleeDmg;
            debuffRangeDmg += status.debuffRangeDmg;
            debuffDmgRes += status.debuffDmgRes;
        }

        public void ZeroStatus()
        {
            buffAcc = 0;
            buffActionPoints = 0;
            buffDefense = 0;
            buffMeleeDmg = 0;
            buffRangeDmg = 0;
            buffDmgRes = 0;

            debuffAcc = 0;
            debuffActionPoints = 0;
            debuffDefense = 0;
            debuffMeleeDmg = 0;
            debuffRangeDmg = 0;
            debuffDmgRes = 0;  
            
        }
        #endregion

        public int GetArchetype()
        {
            return characterArchetype;
        }
    }
}

