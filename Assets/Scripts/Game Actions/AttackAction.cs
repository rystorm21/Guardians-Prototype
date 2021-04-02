using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    public class AttackAction : GameAction
    {
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
        }

        public override void OnDoAction(SessionManager sessionManager, Turn turn, Node node, RaycastHit hit)
        {
            int apCost = 4;
            int currentPlayerAP = turn.player.stateManager.currentCharacter.ActionPoints;

            if (currentPlayerAP >= apCost)
            {
                IHittable iHit = hit.transform.GetComponentInParent<IHittable>();
                if (iHit != null)
                {
                    iHit.OnHit(turn.player.stateManager.currentCharacter);
                    currentPlayerAP -= apCost;
                }
            }
            else
            {
                Debug.Log("Not enough action points!");
            }

            if (currentPlayerAP == 0) {
                Debug.Log("All AP used");
            }

            turn.player.stateManager.currentCharacter.ActionPoints = currentPlayerAP;
        }

        public override void OnHighlightCharacter(SessionManager sessionManager, Turn turn, Node node)
        {
        }

        void RaycastToTarget(GridCharacter character, RaycastHit mouseHit)
        {
            Vector3 origin = character.transform.position + Vector3.up * 1.56f;
            Vector3 direction = mouseHit.point - origin;
            Debug.DrawRay(origin, direction, Color.red);
        }
    }
}
