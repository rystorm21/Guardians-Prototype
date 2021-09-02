using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PixelCrushers.DialogueSystem;

namespace EV
{
    public class EnemyBar : MonoBehaviour
    {
        public GameObject buttonTemplate;
        GameObject cameraHolder;
        Vector3 originalCamHolderPosition;
        Vector3 originalCameraRotation;
        List<GameObject> buttonList = new List<GameObject>();
        List<GridCharacter> enemies;
        SessionManager sessionManager;
        GridCharacter character;
        GridCharacter enemyTarget;
        GameObject button;
        bool enemyClicked;

        public bool timerStarted = false;
        public float timeStartedLerping;
        public float lerpTime;
        bool returnToOrigin;

        [Serializable]
        public struct Game
        {
            public string Name;
            public Sprite Icon;
        }

        [SerializeField] Game[] allGames;

        private void OnEnable()
        {
            sessionManager = GameObject.Find("Grid Manager").GetComponent<SessionManager>();
            cameraHolder = GameObject.Find("Camera Holder");
            character = sessionManager.currentCharacter;
            enemies = sessionManager.turns[1].player.characters;

            if (sessionManager != null && sessionManager.currentCharacter != null)
            {
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (character.character.rangedAttackRange >= Vector3.Distance(character.transform.position, enemies[i].transform.position))
                    {
                        button = Instantiate(buttonTemplate, transform);
                        buttonList.Add(button);
                        button.GetComponent<Button>().AddEventListener(i, ItemClicked);
                    }
                }
            }
        }

        private void OnDisable()
        {
            if (buttonList != null)
            {
                foreach (var button in buttonList)
                {
                    Destroy(button);
                }
            }
        }

        void ItemClicked(int itemIndex)
        {
            // if (Camera.main.transform.localEulerAngles.x > 54.9 && Camera.main.transform.localEulerAngles.x < 55.1)
            // {
            //     originalCameraRotation = Camera.main.transform.localEulerAngles;
            //     originalCamHolderPosition = cameraHolder.transform.position;
            //     Debug.Log("Camera position stored");
            // }
            Debug.Log("Attacking " + enemies[itemIndex].name);
            enemyTarget = enemies[itemIndex];
            MoveAction.DisplayEnemyAcc(sessionManager);
            enemyTarget.highlighter.SetActive(true);
            enemyClicked = true;
            // if (SessionManager.currentGameState != GameState.Combat || sessionManager.moveInProgress)
            //     return;
            // sessionManager.currentCharacter.character.abilitySelected = itemIndex;
            // sessionManager.SetAction("SpecialAbilityAction");
        }

        // Thinking about not implementing the camera parts. Realizing it's probably going to slow the gameplay down, enemy units will generally take more hits than XCOM units.
        // - Shadowrun didn't implement that and it was fine :)
        // private void Update() 
        // {
        //     if (returnToOrigin)
        //     {
        //         Return();
        //     }
        //     else
        //     {
        //         if (enemyClicked)
        //         {
        //             if (!timerStarted)
        //                 StartLerping();
        //             cameraHolder.transform.position = Lerp(cameraHolder.transform.position, character.transform.position, timeStartedLerping, lerpTime);
        //             Camera.main.transform.LookAt(enemyTarget.transform);
        //             CheckIfCompleted(cameraHolder.transform.position, character.transform.position);
        //         }
        //         if (Input.GetKeyDown(KeyCode.T))
        //         {
        //             returnToOrigin = true;
        //             timerStarted = false;
        //         }
        //     }
        // }

        private void Return()
        {
            if (!timerStarted)
                StartLerping();
            cameraHolder.transform.position = Lerp(cameraHolder.transform.position, originalCamHolderPosition, timeStartedLerping, lerpTime);
            Camera.main.transform.localEulerAngles = originalCameraRotation;
            CheckIfCompleted(cameraHolder.transform.position, originalCamHolderPosition);
        }

        private void StartLerping()
        {
            timeStartedLerping = Time.time;
            timerStarted = true;
        }

        public Vector3 Lerp(Vector3 start, Vector3 end, float timeStartedLerping, float lerpTime = 1)
        {
            float timeSinceStarted = Time.time - timeStartedLerping;
            float percentageComplete = timeSinceStarted / lerpTime;
            var result = Vector3.Lerp(start, end, percentageComplete);
            return result;
        }

        private void CheckIfCompleted(Vector3 position, Vector3 destination)
        {
            if (position == destination)
            {
                enemyClicked = false;
                timerStarted = false;
                returnToOrigin = false;
            }
        }
    }
}
