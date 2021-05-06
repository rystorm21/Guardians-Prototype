using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    public class AttackAction : GameAction
    {
        public static Vector3 lastRangedTargetLocation; // for the last target shot
        public static GridCharacter lastAttacker;
        public static GridCharacter lastTarget;
        public static bool attackInProgress;
        public static bool attackHits;
        public static bool hitByMelee;
        private int attackAccuracy;

        public override bool IsActionValid(SessionManager sessionManager, Turn turn)
        {
            if (turn.player.stateManager.currentCharacter == null)
            {
                return false;
            }
            return true;
        }

        public override void OnActionTick(SessionManager sessionManager, Turn turn, Node node, RaycastHit hit)
        {
            if (node != null) 
            {
                RaycastToTarget(turn.player.stateManager.currentCharacter, hit);
                if (node.character == null || node.character.owner == turn.player)
                {
                    sessionManager.SetAction("MoveAction");
                }
                else
                {
                    node.character.highlighter.SetActive(true); // highlight enemy info
                    node.character.accuracyText.gameObject.SetActive(true); // chance to hit this character - accuracy
                    attackAccuracy = GetAttackAccuracy(sessionManager, node);
                    node.character.accuracyText.text = attackAccuracy.ToString() + "%";
                }
            }
            
            if (sessionManager.currentCharacter.ActionPoints == 0)
            {
                sessionManager.APCheck();
            }
        }

        private int GetAttackAccuracy(SessionManager sessionManager, Node node) 
        {
            Vector3 attacker = sessionManager.currentCharacter.transform.position;
            Vector3 defender = node.character.transform.position;
            int targetDistance = Mathf.RoundToInt(Vector3.Distance(attacker, defender));
            int effectiveRange = sessionManager.currentCharacter.character.rangeEffectiveRange;
            int accuracy = sessionManager.currentCharacter.character.attackAccuracy - node.character.character.defense;
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
            return accuracy;
        }

        private void SetAttackerDefender(GridCharacter currentCharacter, Turn turn, Transform projectileTarget)
        {
            lastRangedTargetLocation = projectileTarget.position;
            lastAttacker = currentCharacter;
        }

        private void PlayAttackAnimation(int attackType, Turn turn, Transform projectileTarget) 
        {
            GridCharacter currentCharacter = turn.player.stateManager.currentCharacter;
            Vector3 shootOrigin = GameObject.Find(currentCharacter.character.name + "/metarig/IKHand.R").transform.position;
            SetAttackerDefender(currentCharacter, turn, projectileTarget);

            AttackAction.attackInProgress = true;
            switch (attackType)
            {
                case 1:
                    // play melee attack animation
                    if (turn.player.stateManager.currentCharacter.character.fightingStyle == 1)
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
            if (weaponType == 1)
                apCost--;
            return apCost;
        }

        int AttackRange(SessionManager sessionManager, int weaponType)
        {
            int weaponRange = 0;
            if (weaponType == 0) 
            {
                weaponRange = sessionManager.currentCharacter.character.rangedAttackRange;
            }
            else 
            {
                weaponRange = sessionManager.currentCharacter.character.meleeAttackRange;
            }
            return weaponRange;
        }

        int RollDDice(SessionManager sessionManager)
        {
            int diceRoll = Random.Range(1,101);
            int result = attackAccuracy - diceRoll;
            // Debug.Log("Accuracy:" + attackAccuracy + " Roll:" + diceRoll); 
            if (result >=0)
                attackHits = true;
            else
                attackHits = false;
            return result;
        }

        public void AttackSuccessful(SessionManager sessionManager)
        {
            if (lastTarget != null)
            {
                float damageDealt = lastAttacker.character.attackDamage;
                if (lastTarget.character.braced)
                    damageDealt = damageDealt * .75f;
                lastTarget.character.hitPoints -= Mathf.RoundToInt(damageDealt);
            }
            if (lastTarget.character.hitPoints <=0)
            {
                Debug.Log(lastTarget.character.name + " defeated");
                lastTarget.Death();
            }
        }

        public override void OnDoAction(SessionManager sessionManager, Turn turn, Node node, RaycastHit hit)
        {
            int currentPlayerAP = turn.player.stateManager.currentCharacter.ActionPoints;
            int weaponType = sessionManager.currentCharacter.character.weaponSelected;
            int apCost = AttackCost(weaponType);
            int weaponRange = AttackRange(sessionManager, weaponType);

            if (currentPlayerAP >= apCost)
            {
                IHittable iHit = hit.transform.GetComponentInParent<IHittable>();
                int diceRoll = RollDDice(sessionManager);
                if (iHit != null)
                {
                    int attackDistance = Mathf.FloorToInt(Vector3.Distance(turn.player.stateManager.currentCharacter.transform.position, node.worldPosition));
                    if (weaponRange >= attackDistance) 
                    {
                        if (!attackInProgress)
                        {
                            turn.player.stateManager.currentCharacter.transform.LookAt(hit.transform);
                            iHit.OnHit(turn.player.stateManager.currentCharacter);
                            currentPlayerAP -= apCost;
                            lastTarget = node.character;
                            PlayAttackAnimation(weaponType, turn, hit.transform);
                            if (diceRoll >= 0)
                                AttackSuccessful(sessionManager);
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

            turn.player.stateManager.currentCharacter.ActionPoints = currentPlayerAP;
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
