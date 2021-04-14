using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    public class AttackAction : GameAction
    {
        public static Vector3 lastRangedTarget; // for the last target shot
        public static bool attackInProgress;

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
            }
            
            if (sessionManager.currentCharacter.ActionPoints == 0)
            {
                sessionManager.APCheck();
            }
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
                    break;

                default:
                    lastRangedTarget = projectileTarget.position;
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

        public override void OnDoAction(SessionManager sessionManager, Turn turn, Node node, RaycastHit hit)
        {
            int currentPlayerAP = turn.player.stateManager.currentCharacter.ActionPoints;
            int weaponType = sessionManager.currentCharacter.character.weaponSelected;
            int apCost = AttackCost(weaponType);
            int weaponRange = AttackRange(sessionManager, weaponType);

            if (currentPlayerAP >= apCost)
            {
                IHittable iHit = hit.transform.GetComponentInParent<IHittable>();
                if (iHit != null)
                {
                    int attackDistance = Mathf.FloorToInt(Vector3.Distance(turn.player.stateManager.currentCharacter.transform.position, node.worldPosition));
                    if (weaponRange >= attackDistance && !attackInProgress) 
                    {
                        turn.player.stateManager.currentCharacter.transform.LookAt(hit.transform);
                        iHit.OnHit(turn.player.stateManager.currentCharacter);
                        currentPlayerAP -= apCost;
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
