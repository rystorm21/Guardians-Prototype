using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;   

namespace EV 
{
    public enum OverHeadUIProperties
    {
        name, hitpoints, accuracy, damage
    }

    public class OverheadUI : MonoBehaviour
    {
        private Text textEnemyName;
        public Text textHP;
        public Text textAcc;
        public Text textDmg;
        private GameObject enemyInfoPanel;
        private GameObject playerHighlight;
        private GridCharacter playerCharacter;
        private GridCharacter enemyTarget;
        private Transform thisObjectChildTransform;
        private float horizontalOffset = 90f;
        private float veritcalOffset = 40;
        Vector3 namePos;
        Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        void Update()
        {
            thisObjectChildTransform = this.gameObject.transform.GetChild(0).gameObject.transform;
            if (thisObjectChildTransform.GetChild(0).transform.childCount > 0)
            {
                SetGameObjects();
                SetTextFields();
                DisplayText();
            }
        }

        private void SetGameObjects()
        {
            playerHighlight = thisObjectChildTransform.parent.gameObject;
            enemyTarget = playerHighlight.transform.parent.gameObject.GetComponent<GridCharacter>();
            playerCharacter = enemyTarget.character.attackedBy;
            enemyInfoPanel = thisObjectChildTransform.GetChild(0).gameObject;
        }

        private void SetTextFields()
        {
            textEnemyName = enemyInfoPanel.transform.GetChild(((int)OverHeadUIProperties.name)).GetChild(0).transform.GetComponent<Text>();
            textHP = enemyInfoPanel.transform.GetChild(((int)OverHeadUIProperties.hitpoints)).transform.GetComponent<Text>();
            textAcc = enemyInfoPanel.transform.GetChild(((int)OverHeadUIProperties.accuracy)).transform.GetComponent<Text>();
            textDmg = enemyInfoPanel.transform.GetChild(((int)OverHeadUIProperties.damage)).transform.GetComponent<Text>();
        }

        private void DisplayText()
        {
            namePos = mainCamera.WorldToScreenPoint(playerHighlight.transform.position);
            AttackAction.GetAttackAccuracy(playerCharacter, enemyTarget, false);
            enemyInfoPanel.transform.position = namePos + (Vector3.right * horizontalOffset) + (Vector3.down * veritcalOffset);

            textEnemyName.text = enemyTarget.character.characterName;
            textHP.text = "HP: " + enemyTarget.character.hitPoints + " / " + enemyTarget.character.maxHitPoints;
            textAcc.text = "Acc: " + enemyTarget.character.accuracyToBeHit + "%";
            textDmg.text = "Dmg: " + enemyTarget.character.incomingDamage;
        }
    }
}

