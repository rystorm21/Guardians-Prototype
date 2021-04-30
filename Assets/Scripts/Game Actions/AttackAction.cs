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

        private void PlayAttackAnimation(int attackType, Turn turn, Transform projectileTarget) 
        {
            GridCharacter currentCharacter = turn.player.stateManager.currentCharacter;
            Vector3 shootOrigin = GameObject.Find(currentCharacter.character.name + "/metarig/IKHand.R").transform.position;
            Animator animator = currentCharacter.animator;

            switch (attackType)
            {
                case 1:
                    // play melee attack animation
                    if (turn.player.stateManager.currentCharacter.character.fightingStyle == 1)
                    {
                        animator.CrossFade("SniperMeleeAttack1", 0.1f);
                    }
                    else 
                    {
                        animator.CrossFade("MeleeAttack1", 0.1f);
                    }
                    break;

                default:
                    lastRangedTargetLocation = projectileTarget.position;
                    lastAttacker = currentCharacter;
                    animator.CrossFade("AttackRanged", 0.1f);
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
            Debug.Log("Accuracy:" + attackAccuracy + " Roll:" + diceRoll); 
            if (result >=0)
                attackHits = true;
            else
                attackHits = false;
            return result;
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
                    if (weaponRange >= attackDistance && !attackInProgress) 
                    {
                        turn.player.stateManager.currentCharacter.transform.LookAt(hit.transform);
                        iHit.OnHit(turn.player.stateManager.currentCharacter);
                        currentPlayerAP -= apCost;
                        lastTarget = node.character;
                        PlayAttackAnimation(weaponType, turn, hit.transform);
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
