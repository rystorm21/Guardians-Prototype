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
        public TMP_Text displayAcc;
        public TMP_Text displayDmg;
        public TMP_Text displayTgt;
        GameObject targetCam;

        public void DisplaySkill(SessionManager sessionManager, string skill, string description)
        {
            skillName.text = skill;
            skillDescription.text = description;
            displayAcc.text = "";
            displayDmg.text = "";
            displayTgt.text = "";
        }

        public void DisplaySkill(SessionManager sessionManager, string skill, string description, int accuracy, int damage, string target)
        {
            skillName.text = skill;
            skillDescription.text = description;
            displayAcc.text = "Accuracy: " + accuracy.ToString();
            displayDmg.text = "Damage: " + damage.ToString();
            displayTgt.text = "Target: " + target;
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
            sessionManager.moveButton.SetActive(false);
        }

        public void PowerActivate(SessionManager sessionManager, bool activated)
        {
            targetCam = GameObject.FindGameObjectWithTag("TargetingCam");
            if (!activated)
            {
                Deactivate(sessionManager);
                DestroyTargetCam(targetCam, 0);
            }
            if (activated)
            {
                // logic for doing action
                DestroyTargetCam(targetCam, 3);
            }
        }

        private void DestroyTargetCam(GameObject targetCam, int time)
        {
            if (targetCam != null)
                Destroy(targetCam.gameObject, time);
        }
    }
}

