using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    public class MoveCharacterOnPath : StateActions
    {

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
            GridCharacter character = states.currentCharacter;

            if (!isInit)
            {
                if (character == null || index > character.currentPath.Count -1)
                {
                    states.SetStartingState();
                    return;
                }

                isInit = true;
                startNode = character.currentNode;
                targetNode = character.currentPath[index];
                float time_ = time - 1;
                time_ = Mathf.Clamp01(time_);
                time = time_;
                MoveCharacter(sessionManager, character);
            }

            time += states.delta * speed;
            RotateCharacter(character, states);

            if (time > 1)
            {
                isInit = false;
                character.currentNode.character = null;
                character.currentNode = targetNode;
                character.currentNode.character = character;
                character.ActionPoints -= moveCost; // decrement AP for every step, 2 if diagonal

                index++;

                if (index > character.currentPath.Count - 1)
                {
                    // we moved on to our path, so return to starting state & play idle animation
                    time = 1;
                    index = 0;

                    states.SetStartingState();
                    character.PlayIdleAnimation();
                    firstInit = false;
                    sessionManager.HighlightAroundCharacter(character);
                    character.currentNode.isWalkable = false;       // make currently occupied square not walkable
                    if (character.ActionPoints == 0) 
                    {
                        // implement functionality to check if AP is gone from all characters, and if so, end the turn.
                        Debug.Log(character.name + " all AP Used");   
                    }
                }
            }

            Vector3 targetPos = Vector3.Lerp(startNode.worldPosition, targetNode.worldPosition, time);
            character.transform.position = targetPos;
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