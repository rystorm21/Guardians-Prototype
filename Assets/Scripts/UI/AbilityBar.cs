using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EV
{
    public static class ButtonExtension 
    {
        public static void AddEventListener<T>(this Button button, T param, Action<T> OnClick)
        {
            button.onClick.AddListener(delegate(){OnClick(param);});
        }
    }
    
    public class AbilityBar : MonoBehaviour
    {
        public GameObject buttonTemplate;
        SessionManager sessionManager;
        EV.Characters.Character character;
        List<GameObject> buttonList = new List<GameObject>();
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

            if (sessionManager != null && sessionManager.currentCharacter != null)
            {
                character = sessionManager.currentCharacter.character;
                for (int i = 0; i < character.abilityPool.Count; i++)
                {
                    button = Instantiate(buttonTemplate, transform);
                    button.transform.GetChild(1).GetComponent<Text>().text = "SA" + i;
                    button.transform.GetChild(0).GetComponent<Image>().sprite = character.abilityPool[i].ability.abilityIcon;
                    buttonList.Add(button);
                    button.GetComponent<Button>().AddEventListener(i, ItemClicked);
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
            if (SessionManager.currentGameState != GameState.Combat || sessionManager.moveInProgress)
                return;
            sessionManager.currentCharacter.character.abilitySelected = itemIndex;
            sessionManager.SetAction("SpecialAbilityAction");
        }
    }
}

