using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using PixelCrushers.DialogueSystem;

namespace EV
{
    [CreateAssetMenu(fileName = "New Level Object", menuName = "Level")]
    public class CurrentLevel : Level
    {
        SessionManager sessionManager;
        GameObject mainCamera;
        GameObject playerParent;

        private void Start()
        {
            sessionManager = GameObject.Find("Grid Manager").GetComponent<SessionManager>();
            startingMode = GameState.Dialog;
            postPreDialogMode = GameState.Combat;
        }

        public void SceneFinished()
        {
            sessionManager = GameObject.Find("Grid Manager").GetComponent<SessionManager>();
            //SceneHolder sceneHolder = sessionManager.sceneHolder.GetComponent<SceneHolder>();
            Turn[] turns = sessionManager.turns;
            List<GridCharacter> players = turns[0].player.characters;
            Debug.Log(players.Count);
            for (int i = players.Count - 1; i >= 0; i--)
            {
                turns[0].player.UnRegisterCharacter(players[i]);
            }
            DialogueManager.StopConversation();
            if (loadingScreenDo)
            {
                SceneManager.UnloadSceneAsync(GameManager.instance.currentLevel.thisLevel);
                SceneManager.UnloadSceneAsync("UI");
                SceneManager.LoadSceneAsync("Loading", LoadSceneMode.Additive);
                //Debug.Log(SceneManager.GetActiveScene);
            }
            else 
            {
                //sceneHolder.NextLevel();
                SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Single);
            }
        }

        // public void AddTeammates()
        // {
        //     sessionManager = GameObject.Find("Grid Manager").GetComponent<SessionManager>();
        //     playerParent = GameObject.Find("Players");
        //     mainCamera = GameObject.Find("Main Camera");
        //     int teammateLocationIndex = 0;
        //     foreach (GameObject character in teammatesToAdd)
        //     {
        //         Vector3 unitLocation = teammateLocations[teammateLocationIndex];
        //         Instantiate(character, unitLocation, character.transform.rotation, playerParent.transform);
        //         PlaceNewUnit(character, unitLocation);
        //         teammateLocationIndex++;
        //     }
        // }

        // public void AddEnemies()
        // {
        //     sessionManager = GameObject.Find("Grid Manager").GetComponent<SessionManager>();
        //     playerParent = GameObject.Find("Players");
        //     mainCamera = GameObject.Find("Main Camera");
        //     int teammateLocationIndex = 0;
        //     foreach (GameObject character in enemiesToAdd)
        //     {
        //         Vector3 unitLocation = enemyLocations[teammateLocationIndex];
        //         Instantiate(character, unitLocation, character.transform.rotation, playerParent.transform);
        //         PlaceNewUnit(character, unitLocation);
        //         teammateLocationIndex++;
        //     }
        //     sessionManager.PlaceUnits();
        //     DestroyStateManagers();
        //     sessionManager.InitStateManagers();
        //     SessionManager.currentGameState = GameState.Combat;
        // }

        // void PlaceNewUnit(GameObject character, Vector3 unitLocation)
        // {
        //     GridCharacter unit = character.GetComponent<GridCharacter>();
        //     unit.OnInit();
        //     unit.character.ZeroStatus();
        //     Node node = sessionManager.gridManager.GetNode(unitLocation);
        //     if (node != null)
        //     {
        //          unit.transform.position = node.worldPosition;
        //          node.character = unit;
        //          unit.currentNode = node;
        //          unit.currentNode.isWalkable = false;
        //          unit.character.covered = unit.character.IsCovered(sessionManager, unit);
        //     }
        // }
        
        // void DestroyStateManagers()
        // {
        //     GameObject[] stateMgrs = GameObject.FindGameObjectsWithTag("StateManager");
        //     for (int i = 0; i <= stateMgrs.Length - 1; i++)
        //     {
        //         Debug.Log(stateMgrs[i].name);
        //         Destroy(stateMgrs[i]);
        //     }
        // }
    }
}

