using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    public class MoveCharacterOnPath : StateActions
    {

        bool isInit;
        float time;
        float rotationT;
        float speed;
        Node startNode;
        Node targetNode;
        Quaternion targetRotation;
        Quaternion startRotation;
        int index;
        bool firstInit;

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

                float distance = Vector3.Distance(startNode.worldPosition, targetNode.worldPosition);
                speed = character.GetSpeed() / distance;

                Vector3 direction = targetNode.worldPosition - startNode.worldPosition;
                targetRotation = Quaternion.LookRotation(direction);
                startRotation = character.transform.rotation;
                if (!firstInit)
                {
                    character.PlayMovementAnimation();
                    firstInit = true;
                }
            }

            time += states.delta * speed;
            rotationT += states.delta * character.rotateSpeed;

            if (rotationT > 1)
                rotationT = 1;
            character.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, rotationT);

            if (time > 1)
            {
                isInit = false;
                character.currentNode.character = null;
                character.currentNode = targetNode;
                character.currentNode.character = character;
                index++;

                if (index > character.currentPath.Count - 1)
                {
                    // we moved on to our path
                    time = 1;
                    index = 0;

                    states.SetStartingState();
                    character.PlayIdleAnimation();
                    firstInit = false;
                }
            }

            Vector3 targetPos = Vector3.Lerp(startNode.worldPosition, targetNode.worldPosition, time);
            character.transform.position = targetPos;
        }
    }
}