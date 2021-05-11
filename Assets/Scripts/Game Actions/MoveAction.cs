using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    public class MoveAction : GameAction
    {

        GridCharacter previousCharacter;

        public override void OnActionTick(SessionManager sessionManager, Turn turn, Node node, RaycastHit hit)
        {
            StateManager states = turn.player.stateManager;
            IDetectable detectable = hit.transform.GetComponent<IDetectable>(); // Node detectable = the node at that hit point (using interface)

            if (detectable != null) // then we probably hit a character or something sitting on a node
            {
                node = detectable.OnRaycastHit(); // makes it so you can hover the mouse over the collider instead of only the tile to detect a character
            } 
            else 
            {
                if (previousCharacter != null) 
                {
                    previousCharacter.OnDeHighlight(states.playerHolder, false);
                }
                sessionManager.gameVariables.UpdateCharacterPortrait(sessionManager.currentCharacter.character.characterPortrait);
            }

            if (node != null)
            {
                if (node.character != null)
                {
                    sessionManager.OnHighlightCharacter(node);
                }
                if (states.CurrentCharacter != null && node.character == null)
                {
                    PathDetection(states, sessionManager, node);
                }
            }
        }

        public override void OnDoAction(SessionManager sessionManager, Turn turn, Node node, RaycastHit hit)
        {
            StateManager states = turn.player.stateManager;

            // Moves Character
            if (states.CurrentCharacter != null)
            {
                if (states.CurrentCharacter.currentPath != null)
                {
                    if (states.CurrentCharacter.currentPath.Count > 0)
                    {
                        if (!AttackAction.attackInProgress)
                        {
                            states.SetState("moveOnPath");
                            return;
                        }
                    }
                }
            }

            if (node == null)
                return;

            // Selects Character
            if (node.character != null)
            {
                if (node.character.owner == states.playerHolder)
                {
                    if (node.character != sessionManager.currentCharacter)
                    {
                        node.character.OnSelect(states.playerHolder);
                        states.prevNode = null;
                        sessionManager.ClearPath(states);
                        sessionManager.HighlightAroundCharacter(node.character);
                        sessionManager.gameVariables.UpdateCharacterPortrait(sessionManager.currentCharacter.character.characterPortrait);
                    }
                }
                else
                {

                }
            }
        }

        public override void OnHighlightCharacter(SessionManager sessionManager, Turn turn, Node node)
        {
            StateManager states = turn.player.stateManager;
            if (node.character.owner == states.playerHolder)
            {
                node.character.OnHighlight(states.playerHolder);
                previousCharacter = node.character;
                sessionManager.ClearPath(states);
            }
            
            else // you highlighted an enemy unit
            {
                if (states.CurrentCharacter != null) // you have a character selected
                {
                    //attack
                    sessionManager.gameVariables.UpdateMouseText("Attack");
                    sessionManager.SetAction("AttackAction");
                }
                else
                {
                    //indicate
                    //sessionManager.gameVariables.UpdateMouseText("Enemy");
                }
            }
        }

        public override void OnActionStop(SessionManager sessionManager, Turn turn)
        {
            sessionManager.ClearPath(turn.player.stateManager);
            sessionManager.ClearReachableTiles();
        }

        public override void OnActionStart(SessionManager sessionManager, Turn turn)
        {
            if (turn.player.stateManager.CurrentCharacter != null)
                sessionManager.HighlightAroundCharacter(turn.player.stateManager.CurrentCharacter);
        }

        void PathDetection(StateManager states, SessionManager sessionManager, Node node)
        {
            states.currentNode = node;

            if (states.currentNode != null)
            {
                if (states.currentNode != states.prevNode || states.prevNode == null)
                {
                    states.prevNode = states.currentNode;
                    sessionManager.PathfinderCall(states.CurrentCharacter, states.currentNode);
                }
            }
        }
    }
}
