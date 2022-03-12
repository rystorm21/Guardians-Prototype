using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    public class MoveCharacterOnPath : StateActions
    {
        public static bool moveComplete;
        bool isInit;
        bool firstInit;
        float time;
        float rotationT;
        float speed;
        Node startNode;
        Node targetNode;
        Quaternion targetRotation;
        Quaternion startRotation;
        int index;

        int moveCost;

        public override void Execute(StateManager states, SessionManager sessionManager, Turn turn)
        {
            moveComplete = false;
            GridCharacter character = states.CurrentCharacter;
            sessionManager.moveInProgress = true;
            sessionManager.moveButton.SetActive(false);
            //Debug.Log(character + " " + character.currentPath.Count + " " + index);
            if (!isInit)
            {
                if (character.currentPath == null)
                {
                    character.PathfindDelay(states, sessionManager, character);
                }
                if (character == null || character.currentPath == null || index > character.currentPath.Count - 1)
                {
                    sessionManager.moveInProgress = false;
                    states.SetStartingState();
                    return;
                }
                MoveCharacter(sessionManager, character);
            }

            time += states.delta * speed;
            RotateCharacter(character, states);

            if (time > 1)
            {
                isInit = false;
                CheckForInactive(character);

                character.ActionPoints -= moveCost; // decrement AP for every step, 2 if diagonal
                index++;
                if (character.currentPath != null)
                {
                    if (index > character.currentPath.Count - 1)
                    {
                        // we moved on to our path, so return to starting state & play idle animation
                        moveComplete = true;
                        MoveComplete(states, sessionManager, turn);
                    }
                }
            }

            Vector3 targetPos = Vector3.Lerp(startNode.worldPosition, targetNode.worldPosition, time);
            character.transform.position = targetPos;
        }

        private void CheckForInactive(GridCharacter character)
        {
            if (!character.currentNode.inactiveCharWasHere) // prevents player from making a character unclickable when being replaced
                character.currentNode.character = null;
            character.currentNode = targetNode;
            if (!character.currentNode.inactiveCharWasHere) // ditto with the above, needed to be in order
                character.currentNode.character = character;
        }

        private void MoveComplete(StateManager states, SessionManager sessionManager, Turn turn)
        {
            GridCharacter character = states.CurrentCharacter;
            time = 1;
            index = 0;

            character.character.covered = character.character.IsCovered(sessionManager, character);
            states.SetStartingState();
            character.PlayIdleAnimation();
            firstInit = false;
            sessionManager.HighlightAroundCharacter(character, null, 0);
            sessionManager.moveInProgress = false;
            character.currentNode.isWalkable = false;       // make currently occupied square not walkable
            character.currentNode.inactiveCharWasHere = false;
            if (character.ActionPoints == 0)
            {
                sessionManager.APCheck();
            }
            sessionManager.SetAction("MoveAction");
            if (sessionManager.turns[sessionManager.TurnIndex].name == "PlayerTurn")
                sessionManager.ResetAbilityEnemyUI();
        }

        private void RotateCharacter(GridCharacter character, StateManager states)
        {
            rotationT += states.delta * character.rotateSpeed;

            if (rotationT > 1)
                rotationT = 1;

            character.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, rotationT);
        }

        private void MoveCharacter(SessionManager sessionManager, GridCharacter character)
        {
            isInit = true;
            startNode = character.currentNode;
            targetNode = character.currentPath[index];
            float time_ = time - 1;
            time_ = Mathf.Clamp01(time_);
            time = time_;

            float distance = Vector3.Distance(startNode.worldPosition, targetNode.worldPosition);
            speed = character.GetSpeed() / distance;

            Vector3 direction = targetNode.worldPosition - startNode.worldPosition;
            direction.y = 0; // optional -2d of course
            targetRotation = Quaternion.LookRotation(direction);
            startRotation = character.transform.rotation;

            if (direction.x != 0 && direction.z != 0)
            {
                moveCost = 2;
            }
            else
            {
                moveCost = 1;
            }

            if (!firstInit)
            {
                character.currentNode.isWalkable = true;
                sessionManager.ClearReachableTiles();
                character.PlayMovementAnimation();
                firstInit = true;
            }
            // currentPath.count is the length of the distance traveled in nodes
        }
    }
}