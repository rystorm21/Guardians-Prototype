using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace EV
{
    public class PopUpUI : MonoBehaviour
    {
        public TMP_Text skillName;
        public TMP_Text skillDescription;

        public void DisplaySkill(SessionManager sessionManager, string skill, string description)
        {
            skillName.text = skill;
            skillDescription.text = description;
        }

        public void Deactivate(SessionManager sessionManager)
        {
            if (sessionManager.currentCharacter.character.weaponSelected == 0)
                sessionManager.currentCharacter.PlayAnimation("Idle");
            else
                sessionManager.currentCharacter.PlayAnimation("MeleeIdle");
            sessionManager.currentCharacter.character.abilityInUse = null;
            sessionManager.popUpUI.gameObject.SetActive(false);
            sessionManager.SetAction("MoveAction");
        }
        public void Deactivate(SessionManager sessionManager, bool targeting)
        {
            sessionManager.popUpUI.gameObject.SetActive(false);
        }
        public void Activate(SessionManager sessionManager)
        {
            sessionManager.powerActivated.value = false;
            sessionManager.popUpUI.gameObject.SetActive(true);
        }

        public void PowerActivate(SessionManager sessionManager, bool activated)
        {
            if (!activated)
            {
                Deactivate(sessionManager);
            }
            if (activated)
            {
                // logic for doing action
            }
        }
    }
}

