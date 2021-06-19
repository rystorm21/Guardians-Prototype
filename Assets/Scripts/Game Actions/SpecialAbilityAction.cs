using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace EV 
{
    public class SpecialAbilityAction : GameAction
    {
        SessionManager sessionManager;
        GridCharacter currentCharacter;
        Ability abilitySelected;
        Node lastNode;
        int selection;
        bool targetingMode;

        public override void OnActionStart(SessionManager sm, Turn turn)
        {
            sessionManager = sm;
            currentCharacter = sessionManager.currentCharacter;
            selection = currentCharacter.character.abilitySelected;
            abilitySelected = currentCharacter.character.abilityPool[selection].ability;
            lastNode = sessionManager.currentCharacter.currentNode;

            if (sessionManager.currentCharacter.ActionPoints < abilitySelected.apCost)
            {
                // if trying to use an ability without enough AP
                sessionManager.SetAction("MoveAction");
                return;
            }
            sessionManager.currentCharacter.currentNode.tileRenderer.material = sessionManager.defaultTileMaterial;

            switch (abilitySelected.type.ToString())
            {
                case "Self":
                    sessionManager.currentCharacter.currentNode.tileRenderer.material = sessionManager.abilityTileMaterial;
                    break;
                case "PBAoE":
                    // insert logic for player-based aoe abilities
                    break;
                case "Ranged":
                    // insert logic for ranged abilities
                    break;
                case "RangedAoe":
                    // insert logic for ranged aoe abilities
                    break;
            }
            sessionManager.gameVariables.UpdateMouseText("");
            sessionManager.popUpUI.Activate(sessionManager);
            sessionManager.popUpUI.DisplaySkill(sessionManager, abilitySelected.abilityName, abilitySelected.description);
        }

        public override void OnActionTick(SessionManager sm, Turn turn, Node node, RaycastHit hit)
        {
            // insert stuff here
            if (sessionManager.powerActivated.value)
            {
                AbilityType();
                if (targetingMode)
                    sessionManager.popUpUI.Deactivate(sessionManager, targetingMode);
                else
                    sessionManager.popUpUI.Deactivate(sessionManager);
            }
            if (Input.GetMouseButtonDown(1))
            {
                ExitMode();
            }
        }

        public override void OnHighlightCharacter(SessionManager sm, Turn turn, Node node)
        {
        }

        public override void OnDoAction(SessionManager sm, Turn turn, Node node, RaycastHit hit)
        {
            // triggered by mouseclick on a non-UI location (DetectMousePosition.cs)
            Debug.Log("Shot fired");
            sessionManager.currentCharacter.ActionPoints -= abilitySelected.apCost;
            ExitMode();
        }

        void AbilityType()
        {
            targetingMode = false;

            switch (abilitySelected.type.ToString())
            {
                case "Self":
                    // insert logic for self-based abilities
                    Debug.Log("Self ability type used: " + abilitySelected.abilityName.ToString());
                    sessionManager.currentCharacter.ActionPoints -= abilitySelected.apCost;
                    break;
                case "PBAoE":
                    // insert logic for player-based aoe abilities
                    Debug.Log("Kaboom probably");
                    sessionManager.currentCharacter.ActionPoints -= abilitySelected.apCost;
                    break;
                case "Ranged":
                    // insert logic for ranged abilities
                    TargetingMode(abilitySelected.radius);
                    break;
                case "RangedAoe":
                    // insert logic for ranged aoe abilities
                    TargetingMode(abilitySelected.radius);
                    break;
            }
        }
        
        // Targeting mode for ranged / ranged AoE Attacks
        void TargetingMode(int radius)
        {
            targetingMode = true;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Debug.Log(radius);

            if (Physics.Raycast(ray, out hit, 1000))                                 // if the raycast hits something
            {
                Node node = sessionManager.gridManager.GetNode(hit.point);          // get the node at the hit point 
                if (node != null)
                {
                    ClearLastTargetTile();
                    if (node.tileRenderer != null)
                        node.tileRenderer.material = sessionManager.abilityTileMaterial;
                    lastNode = node;
                }
                if (EventSystem.current.IsPointerOverGameObject()) // don't register the click if it's on a GUI object
                {
                    if (node != null)
                        if (node.tileRenderer != null)
                            node.tileRenderer.material = sessionManager.defaultTileMaterial;
                }
            }
        }

        void ClearLastTargetTile()
        {
            if (lastNode.tileRenderer != null)
                lastNode.tileRenderer.material = sessionManager.defaultTileMaterial;
        }

        void ExitMode()
        {
            ClearLastTargetTile();
            sessionManager.SetAction("MoveAction");
            sessionManager.popUpUI.Deactivate(sessionManager);
            targetingMode = false;
        }
    }
}
