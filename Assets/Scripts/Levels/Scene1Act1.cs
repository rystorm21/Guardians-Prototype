using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using PixelCrushers.DialogueSystem;

namespace EV
{
    [CreateAssetMenu(fileName = "New Level Object", menuName = "Level")]
    public class Scene1Act1 : Level
    {
        SessionManager sessionManager;

        private void Awake() 
        {
            thisLevel = "Act 1 Scene 1";
            startingMode = GameState.Combat;
            postPreDialogMode = GameState.Combat;
            hasPostDialogue = true;
            postDialogueTitle = "S1A1-PostBattle";
            nextScene = "Act1_S2";
        }

        public void SceneFinished()
        {
            sessionManager = GameObject.Find("Grid Manager").GetComponent<SessionManager>();
            Turn[] turns = sessionManager.turns;
            List<GridCharacter> players = turns[0].player.characters;
            Debug.Log(players.Count);
            for (int i = players.Count-1; i >= 0; i--)
            {
                turns[0].player.UnRegisterCharacter(players[i]);
            }
            DialogueManager.StopConversation();
            SceneManager.LoadScene(nextScene);
        }
    }
}
