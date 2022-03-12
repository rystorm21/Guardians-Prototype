using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    public class PlayerAttackAction : GameAction
    {
        public static bool attackInProgress;
        public static float originalFieldOfView;
        public GameObject cameraMarker;
        Camera targetCam;
        GridCharacter selectedTarget;
        bool attackFinished;

        public override void OnDoAction(SessionManager sessionManager, Turn turn, Node node, RaycastHit hit)
        {
        }
        
        public override void OnActionStart(SessionManager sessionManager, Turn turn)
        {
            sessionManager.ClearReachableTiles();
            selectedTarget = EnemyBar.selectedTarget;
            sessionManager.currentCharacter.transform.LookAt(selectedTarget.transform.position);
            cameraMarker = sessionManager.currentCharacter.cameraMarker;
            cameraMarker.transform.LookAt(selectedTarget.transform);
            sessionManager.gameVariables.UpdateMouseText("");
            sessionManager.popUpUI.Activate(sessionManager);
            sessionManager.popUpUI.DisplaySkill(sessionManager, "Attack", "Click activate to confirm attack", 
                AttackAction.GetAttackAccuracy(sessionManager.currentCharacter, selectedTarget, false),
                sessionManager.currentCharacter.character.attackDamage,
                selectedTarget.character.characterName);
            MoveAction.DisplayEnemyAcc(sessionManager);
            selectedTarget.highlighter.SetActive(true);
            //SetCameraToEnemy(sessionManager);
        }

        public override void OnActionTick(SessionManager sessionManager, Turn turn, Node node, RaycastHit hit)
        {
            if (sessionManager.powerActivated.value)
            {   
                if (!attackFinished)
                {
                    attackInProgress = true;
                    DoAttack(sessionManager, hit);
                }
            }
            // if (targetCam != null)
            // {
            //     LerpTargetCam();
            // }
        }

        public void DoAttack(SessionManager sessionManager, RaycastHit hit)
        {
            int index = 0;
            Vector3 defenderPosition = selectedTarget.transform.position;
            Vector3 attackerPosition = sessionManager.currentCharacter.transform.position;
            float distance = Vector3.Distance(attackerPosition, defenderPosition);
            AttackAction.attackAccuracy = AttackAction.GetAttackAccuracy(sessionManager.currentCharacter, selectedTarget, false);

            RaycastHit[] enemySearch;
            enemySearch = Physics.RaycastAll(attackerPosition, (defenderPosition + Vector3.up) - attackerPosition, distance + 1); // change this
            Debug.DrawRay(attackerPosition, (defenderPosition + Vector3.up) - attackerPosition);
            for (int i = 0; i < enemySearch.Length; i++)
            {
                if (enemySearch[i].transform.gameObject.name == selectedTarget.name)
                {
                    index = i;
                }
            }

            sessionManager.SetAction("AttackAction");
            sessionManager.DoAction(selectedTarget.currentNode, enemySearch[index]);
        }

        public override void OnActionStop(SessionManager sessionManager, Turn turn)
        {
            sessionManager.popUpUI.Deactivate(sessionManager, true);
        }

        public override void OnHighlightCharacter(SessionManager sessionManager, Turn turn, Node node)
        {
        }

        // public void LerpTargetCam()
        // {
        //     targetCam.transform.position = Vector3.Lerp(targetCam.transform.position, cameraMarker.transform.position, Time.deltaTime * 5);
        //     targetCam.transform.LookAt(selectedTarget.transform);
        // }

        // private void SetCameraToEnemy(SessionManager sessionManager)
        // {
        //     GameObject camHolder = GameObject.Find("Camera Holder");
        //     Transform camCoordinates;
        //     float targetingFieldOfView = 30;

        //     if (targetCam != null)
        //     {
        //         camCoordinates = targetCam.transform;
        //         GameObject.Destroy(targetCam.gameObject);
        //     }
        //     else
        //     {
        //         camCoordinates = Camera.main.transform;
        //         originalFieldOfView = Camera.main.fieldOfView;
        //     }
        //     Camera.main.fieldOfView = targetingFieldOfView;
        //     targetCam = GameObject.Instantiate(sessionManager.cameraPrefab, camCoordinates.position, camCoordinates.rotation);
        //     targetCam.transform.parent = camHolder.transform;
        //     targetCam.fieldOfView = targetingFieldOfView;
        //     GameObject.Destroy(targetCam.GetComponent<AudioListener>());
        // }
    }
}

