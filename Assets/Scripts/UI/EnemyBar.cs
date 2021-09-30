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
        public static GridCharacter selectedTarget;

        public GameObject buttonTemplate;
        List<GameObject> buttonList = new List<GameObject>();
        List<GridCharacter> enemies;
        SessionManager sessionManager;
        GridCharacter character;
        GameObject button;

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
            selectedTarget = enemies[itemIndex];
            sessionManager.SetAction("PlayerAttackAction");
        }
    }
}
