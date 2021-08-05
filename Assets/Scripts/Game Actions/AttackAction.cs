using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    public enum attackType
    {
        Ranged,
        Melee,
        Ability
    }

    public class AttackAction : GameAction
    {
        public static Vector3 lastRangedTargetLocation; // for the last target shot
        public static GridCharacter lastAttacker;
        public static GridCharacter lastTarget;
        public static bool attackInProgress;
        public static bool hitByMelee;
        public static int attackAccuracy;
        public static int diceRoll;

        #region Base Action Methods
        public override void OnDoAction(SessionManager sessionManager, Turn turn, Node node, RaycastHit hit)
        {
            GridCharacter currentCharacter = sessionManager.currentCharacter;
            int weaponType = currentCharacter.character.weaponSelected;

            if (currentCharacter.ActionPoints >= AttackCost(weaponType))
            {
                IHittable iHit = hit.transform.GetComponentInParent<IHittable>();
                diceRoll = RollDDice(sessionManager);
                if (iHit != null)
                {
                    int attackDistance = Mathf.FloorToInt(Vector3.Distance(currentCharacter.transform.position, node.worldPosition));
                    if (AttackRange(sessionManager, weaponType) >= attackDistance)
                    {
                        if (!attackInProgress)
                        {
                            currentCharacter.transform.LookAt(hit.transform);
                            iHit.OnHit(currentCharacter);
                            currentCharacter.ActionPoints -= AttackCost(weaponType);
                            lastTarget = node.character;
                            PlayAttackAnimation(weaponType, turn, hit.transform);
                            if (diceRoll >= 0)
                                AttackSuccessful(sessionManager, weaponType);
                            else
                                Debug.Log("attack missed!");
                        }
                    }
                    else
                    {
                        Debug.Log("Target out of range!");
                    }
                }
            }
            else
            {
                Debug.Log("Not enough action points!");
            }
        }

        public override void OnHighlightCharacter(SessionManager sessionManager, Turn turn, Node node)
        {
        }

        public override bool IsActionValid(SessionManager sessionManager, Turn turn)
        {
            if (sessionManager.currentCharacter == null)
            {
                return false;
            }
            return true;
        }

        public override void OnActionTick(SessionManager sessionManager, Turn turn, Node node, RaycastHit hit)
        {
            if (node != null) 
            {
                RaycastToTarget(sessionManager.currentCharacter, hit);
                if (node.character == null || node.character.owner == turn.player)
                {
                    sessionManager.SetAction("MoveAction");
                }
                else
                {
                    attackAccuracy = GetAttackAccuracy(sessionManager.currentCharacter, node.character, false);
                }
            }
            
            if (sessionManager.currentCharacter.ActionPoints == 0)
            {
                sessionManager.APCheck();
            }
        }
        #endregion

        #region Accuracy Calculation

        private void SetAttackerDefender(GridCharacter currentCharacter, Turn turn, Transform projectileTarget)
        {
            lastRangedTargetLocation = projectileTarget.position;
            lastAttacker = currentCharacter;
        }

        public static int GetAttackAccuracy(GridCharacter attacker, GridCharacter defender, bool ignoreCover)
        {
            int coverDefense = 0;
            coverDefense = CheckTargetCover(attacker, defender);
            if (attacker.character.abilityInUse != null)
                if (attacker.character.abilityInUse.ignoreCover)
                    coverDefense = 0;

            ToggleHighlighterText(defender, true);
            int targetDistance = Mathf.RoundToInt(Vector3.Distance(attacker.transform.position, defender.transform.position));
            int effectiveRange = attacker.character.rangeEffectiveRange;
            int accuracy = Mathf.RoundToInt((attacker.character.attackAccuracy + attacker.character.buffAcc - attacker.character.debuffAcc) - (defender.character.defense + defender.character.buffDefense - defender.character.debuffDefense + coverDefense));
            int closeRange = 5;

            if (targetDistance > effectiveRange)
            {
                // penalty for exceeding effective range
                accuracy -= (targetDistance - effectiveRange) * 5;
            }
            if (targetDistance < closeRange)
            {
                // accuracy bonus for close-range attack
                accuracy += (closeRange - targetDistance) * 5;
            }
            if (accuracy > 100)
                accuracy = 100;
            if (accuracy < 0)
                accuracy = 0;
            defender.accuracyText.text = accuracy + "%";
            if (SpecialAbilityAction.buffAbilitySelected)
            {
                ToggleHighlighterText(defender, false);
                return 1;
            }
            return accuracy;
        }

        public static int CheckTargetCover(GridCharacter attacker, GridCharacter defender)
        {
            Vector3 attackerPosition = attacker.transform.position;
            Vector3 defenderPosition = defender.transform.position;
            Color rayColor = Color.blue;
            float distance = Vector3.Distance(attackerPosition, defenderPosition);
            
            if (distance < 2)
                return 0;
            int coverDefense = 0;
            Debug.DrawLine(attackerPosition, defenderPosition, rayColor);

            RaycastHit[] coverHits;
            coverHits = Physics.RaycastAll(attackerPosition, defenderPosition - attackerPosition, distance);
            if (coverHits.Length > 0)
            {
                for (int i = 0; i < coverHits.Length; i++)
                {
                    if (coverHits[i].transform.gameObject.tag != "Player" && coverHits[i].transform.gameObject.tag != "Enemy")
                    {
                        if (defender.character.covered)
                        {
                            if (coverHits[i].transform.gameObject == defender.character.coveredBy)
                            {
                                int coverHigh = 50;
                                int coverLow = 25;
                                if (defender.character.coveredBy.tag == "Cover-Low")
                                    coverDefense = coverLow;
                                if (defender.character.coveredBy.tag == "Cover-High")
                                    coverDefense = coverHigh;
                            }
                        }
                    }
                }
            }
            return coverDefense;
        }

        int AttackRange(SessionManager sessionManager, int weaponType)
        {
            int weaponRange;
            if (weaponType == ((int)attackType.Ranged))
            {
                weaponRange = sessionManager.currentCharacter.character.rangedAttackRange;
            }
            else
            {
                weaponRange = sessionManager.currentCharacter.character.meleeAttackRange;
            }
            return weaponRange;
        }

        int AttackCost(int weaponType)
        {
            int apCost = 4;
            if (weaponType == ((int)attackType.Melee))
                apCost--;
            return apCost;
        }

        public static int RollDDice(SessionManager sessionManager)
        {
            if (SpecialAbilityAction.buffAbilitySelected)
                return 100;
            int diceRoll = Random.Range(1, 101);
            int result = attackAccuracy - diceRoll;
            // Debug.Log("accuracy: " + attackAccuracy + ", diceRoll: " + diceRoll); // for testing purposes
            return result;
        }
        #endregion

        #region Damage Dealing / Status effects
    
        public static void AttackSuccessful(SessionManager sessionManager, int weaponType)
        {
            if (lastTarget != null)
            {
                float damageDealt = DamageDealt(sessionManager, weaponType, lastAttacker, lastTarget, null);
                lastTarget.character.hitPoints -= Mathf.RoundToInt(damageDealt);
                lastTarget.healthBar.SetHealth(lastTarget.character.hitPoints);
            }
            if (lastTarget.character.hitPoints <=0)
            {
                Debug.Log(lastTarget.character.name + " defeated");
                lastTarget.Death();
            }
        }

        public static float DamageDealt(SessionManager sessionManager, int weaponType, GridCharacter attacker, GridCharacter defender, Ability abilitySelected)
        {
            float damageDealt = 0;
            float braceRes = 25f;

            if (weaponType == ((int)attackType.Ranged))
                damageDealt = attacker.character.attackDamage + (attacker.character.attackDamage * attacker.character.buffRangeDmg);
            if (weaponType == ((int)attackType.Melee))
                damageDealt = attacker.character.attackDamage + (attacker.character.attackDamage * attacker.character.buffMeleeDmg);
            if (weaponType == ((int)attackType.Ability))
            {
                damageDealt = (attacker.character.attackDamage * abilitySelected.damageModifier);
                if (attacker.character.GetArchetype() == ((int)Characters.Archetype.Tanker))
                    damageDealt += damageDealt * attacker.character.buffMeleeDmg;
                else
                    damageDealt += damageDealt * attacker.character.buffRangeDmg;
            }

            if (defender.character.braced)
                damageDealt = damageDealt + (damageDealt * ((1f - defender.character.damageResist + defender.character.debuffDmgRes - braceRes) * .01f));
            else
                damageDealt = damageDealt + (damageDealt * ((1f - defender.character.damageResist + defender.character.debuffDmgRes) * .01f));

            //Debug.Log(damageDealt);
            return damageDealt;
        }

        #endregion

        #region Utilities

        private void PlayAttackAnimation(int attackType, Turn turn, Transform projectileTarget)
        {
            GridCharacter currentCharacter = turn.player.stateManager.CurrentCharacter;
            Vector3 shootOrigin = GameObject.Find(currentCharacter.character.name + "/metarig/IKHand.R").transform.position;
            SetAttackerDefender(currentCharacter, turn, projectileTarget);

            AttackAction.attackInProgress = true;
            switch (attackType)
            {
                case 1:
                    // play melee attack animation
                    if (currentCharacter.character.fightingStyle == 1)
                    {
                        currentCharacter.PlayAnimation("SniperMeleeAttack1");
                    }
                    else
                    {
                        currentCharacter.PlayAnimation("MeleeAttack1"); // trying somethin
                    }
                    if (diceRoll > 0)
                    {
                        hitByMelee = true;
                        lastTarget.PlayAnimation("Death");
                    }
                    break;

                default:
                    currentCharacter.PlayAnimation("AttackRanged");
                    GameObject.Instantiate(currentCharacter.character.projectile, shootOrigin, Quaternion.identity);
                    attackInProgress = true;
                    break;
            }
        }

        public static void ToggleHighlighterText(GridCharacter character, bool toggle)
        {
            character.accuracyText.gameObject.SetActive(toggle); // chance to hit this character - accuracy
            character.highlighter.SetActive(toggle); // highlight enemy info
        }

        void RaycastToTarget(GridCharacter character, RaycastHit mouseHit)
        {
            if (character != null) 
            {
                Vector3 origin = character.transform.position + Vector3.up * 1f;
                Vector3 direction = mouseHit.point - origin;
                Debug.DrawRay(origin, direction, Color.red);
            }
        }

        #endregion
    }
}
