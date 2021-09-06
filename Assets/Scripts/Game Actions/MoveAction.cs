using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
            }
            //DisplayEnemyAcc(sessionManager);
        }

        public override void OnDoAction(SessionManager sessionManager, Turn turn, Node node, RaycastHit hit)
        {
            StateManager states = turn.player.stateManager;
            // Detects path
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                if (states.CurrentCharacter != null)
                {
                    if (!AttackAction.attackInProgress)
                    {
                        sessionManager.moveButton.SetActive(true);
                        PathDetection(states, sessionManager, node);
                    }
                }
            }

            if (node == null)
                return;

            // Selects Character
            if (node.character != null)
            {
                sessionManager.moveButton.SetActive(false);
                if (node.character.owner == states.playerHolder)
                {
                    if (node.character != sessionManager.currentCharacter && !node.inactiveCharWasHere)
                    {
                        node.character.OnSelect(states.playerHolder);
                        states.prevNode = null;
                        sessionManager.ClearPath(states);
                        sessionManager.HighlightAroundCharacter(node.character, null, 0);
                        sessionManager.gameVariables.UpdateCharacterPortrait(sessionManager.currentCharacter.character.characterPortrait);
                        sessionManager.ResetAbilityEnemyUI();
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
            if (node.character != null)
            {
                if (node.character.owner != null)
                {
                    if (node.character.owner == states.playerHolder)
                    {
                        node.character.OnHighlight(states.playerHolder);
                        previousCharacter = node.character;
                        if (AIController.aiActive)
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
            }
        }

        public override void OnActionStop(SessionManager sessionManager, Turn turn)
        {
            sessionManager.ClearPath(turn.player.stateManager);
            sessionManager.ClearReachableTiles();
        }

        public override void OnActionStart(SessionManager sessionManager, Turn turn)
        {
            DisplayEnemyAcc(sessionManager);
            if (turn.player.stateManager.CurrentCharacter != null)
                sessionManager.HighlightAroundCharacter(turn.player.stateManager.CurrentCharacter, null, 0);
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

        public static void DisplayEnemyAcc(SessionManager sessionManager)
        {
            int opponentTeam;
            
            if (sessionManager.currentCharacter == null)
                sessionManager.currentCharacter = sessionManager.turns[sessionManager.TurnIndex].player.characters[0];

            if (sessionManager.TurnIndex == 0)
                opponentTeam = 1;
            else
                opponentTeam = 0;
            foreach(GridCharacter character in sessionManager.turns[opponentTeam].player.characters)
            {
                AttackAction.GetAttackAccuracy(sessionManager.currentCharacter, character, false);
                character.highlighter.SetActive(false);
            }
        }
    }
}
