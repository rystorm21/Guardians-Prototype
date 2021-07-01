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
        public static bool attackHits;
        public static bool hitByMelee;
        public static int attackAccuracy;

        public override bool IsActionValid(SessionManager sessionManager, Turn turn)
        {
            if (turn.player.stateManager.CurrentCharacter == null)
            {
                return false;
            }
            return true;
        }

        public override void OnActionTick(SessionManager sessionManager, Turn turn, Node node, RaycastHit hit)
        {
            if (node != null) 
            {
                RaycastToTarget(turn.player.stateManager.CurrentCharacter, hit);
                if (node.character == null || node.character.owner == turn.player)
                {
                    sessionManager.SetAction("MoveAction");
                }
                else
                {
                    attackAccuracy = GetAttackAccuracy(sessionManager, node);
                }
            }
            
            if (sessionManager.currentCharacter.ActionPoints == 0)
            {
                sessionManager.APCheck();
            }
        }

        public static int GetAttackAccuracy(SessionManager sessionManager, Node node) 
        {
            if (SpecialAbilityAction.buffAbilitySelected)
                return 1;

            GridCharacter attacker = sessionManager.currentCharacter;
            GridCharacter defender = node.character;
            defender.accuracyText.gameObject.SetActive(true); // chance to hit this character - accuracy
            defender.highlighter.SetActive(true); // highlight enemy info

            int targetDistance = Mathf.RoundToInt(Vector3.Distance(attacker.transform.position, defender.transform.position));
            int effectiveRange = attacker.character.rangeEffectiveRange;
            int accuracy = Mathf.RoundToInt((attacker.character.attackAccuracy + attacker.character.buffAcc - attacker.character.debuffAcc) - (defender.character.defense + defender.character.buffDefense - defender.character.debuffDefense));
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
            return accuracy;
        }

        private void SetAttackerDefender(GridCharacter currentCharacter, Turn turn, Transform projectileTarget)
        {
            lastRangedTargetLocation = projectileTarget.position;
            lastAttacker = currentCharacter;
        }

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
                    if (turn.player.stateManager.CurrentCharacter.character.fightingStyle == 1)
                    {
                        currentCharacter.PlayAnimation("SniperMeleeAttack1");
                    }
                    else 
                    {
                        currentCharacter.PlayAnimation("MeleeAttack1"); // trying somethin
                    }
                    if (attackHits)
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

        int AttackCost(int weaponType)
        {
            int apCost = 4;
            if (weaponType == ((int)attackType.Melee))
                apCost--;
            return apCost;
        }

        int AttackRange(SessionManager sessionManager, int weaponType)
        {
            int weaponRange = 0;
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

        public static int RollDDice(SessionManager sessionManager)
        {
            if (SpecialAbilityAction.buffAbilitySelected)
                return 100;
            int diceRoll = Random.Range(1,101);
            int result = attackAccuracy - diceRoll;
            // Debug.Log("accuracy: " + attackAccuracy + ", diceRoll: " + diceRoll); // for testing purposes
            if (result >=0)
                attackHits = true;
            else
                attackHits = false;
            return result;
        }

        public static void AttackSuccessful(SessionManager sessionManager, int weaponType)
        {
            if (lastTarget != null)
            {
                float damageDealt = DamageDealt(sessionManager, weaponType, lastAttacker, lastTarget, null);
                lastTarget.character.hitPoints -= Mathf.RoundToInt(damageDealt);
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
                damageDealt = damageDealt + (damageDealt * ((1f - defender.character.damageResist + defender.character.debuffDmgRes -25f) * .01f));
            else
                damageDealt = damageDealt + (damageDealt * ((1f - defender.character.damageResist + defender.character.debuffDmgRes) * .01f));

            Debug.Log(damageDealt);
            return damageDealt;
        }

        public override void OnDoAction(SessionManager sessionManager, Turn turn, Node node, RaycastHit hit)
        {
            int currentPlayerAP = turn.player.stateManager.CurrentCharacter.ActionPoints;
            int weaponType = sessionManager.currentCharacter.character.weaponSelected;
            int apCost = AttackCost(weaponType);
            int weaponRange = AttackRange(sessionManager, weaponType);

            if (currentPlayerAP >= apCost)
            {
                IHittable iHit = hit.transform.GetComponentInParent<IHittable>();
                int diceRoll = RollDDice(sessionManager);
                if (iHit != null)
                {
                    int attackDistance = Mathf.FloorToInt(Vector3.Distance(turn.player.stateManager.CurrentCharacter.transform.position, node.worldPosition));
                    if (weaponRange >= attackDistance) 
                    {
                        if (!attackInProgress)
                        {
                            turn.player.stateManager.CurrentCharacter.transform.LookAt(hit.transform);
                            iHit.OnHit(turn.player.stateManager.CurrentCharacter);
                            currentPlayerAP -= apCost;
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

            turn.player.stateManager.CurrentCharacter.ActionPoints = currentPlayerAP;
        }

        public override void OnHighlightCharacter(SessionManager sessionManager, Turn turn, Node node)
        {
        }

        void RaycastToTarget(GridCharacter character, RaycastHit mouseHit)
        {
            if (character != null) 
            {
                Vector3 origin = character.transform.position + Vector3.up * 1.56f;
                Vector3 direction = mouseHit.point - origin;
                Debug.DrawRay(origin, direction, Color.red);
            }
        }
    }
}
