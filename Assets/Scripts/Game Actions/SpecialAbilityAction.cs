using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace EV 
{
    public class SpecialAbilityAction : GameAction
    {
        public static bool buffAbilitySelected;
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
            sessionManager.ClearReachableTiles();
            buffAbilitySelected = abilitySelected.buff;

            if (currentCharacter.ActionPoints < abilitySelected.apCost)
            {
                // if trying to use an ability without enough AP
                Debug.Log("Not enough AP to use this ability.");
                ExitMode();
                return;
            }
            sessionManager.currentCharacter.currentNode.tileRenderer.material = sessionManager.defaultTileMaterial;

            switch (abilitySelected.type.ToString())
            {
                case "Self":
                    sessionManager.HighlightAroundCharacter(currentCharacter, currentCharacter.currentNode, abilitySelected.radius);
                    break;
                case "PBAoE":
                    // insert logic for player-based aoe abilities
                    sessionManager.HighlightAroundCharacter(currentCharacter, currentCharacter.currentNode, abilitySelected.radius);
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
                AbilityType(sessionManager);
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

        // triggered by mouseclick on a non-UI location (DetectMousePosition.cs)
        public override void OnDoAction(SessionManager sm, Turn turn, Node node, RaycastHit hit)
        {
            if (targetingMode)
            {
                string abilityTypeInUse;
                abilityTypeInUse = abilitySelected.type.ToString();
                BlastRadius(sm, currentCharacter.owner.name, false);
                sessionManager.currentCharacter.ActionPoints -= abilitySelected.apCost;
                ExitMode();
            }
        }

        void AbilityType(SessionManager sessionManager)
        {
            targetingMode = false;

            switch (abilitySelected.type.ToString())
            {
                case "Self":
                    // insert logic for self-based abilities
                    StatusAbility(currentCharacter, true);
                    sessionManager.currentCharacter.ActionPoints -= abilitySelected.apCost;
                    break;
                case "PBAoE":
                    // insert logi)c for player-based aoe abilities
                    BlastRadius(sessionManager, currentCharacter.owner.name, false);
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

        void StatusAbility(GridCharacter target)
        {
            target.character.hitPoints += Mathf.RoundToInt(target.character.maxHitPoints * abilitySelected.healingModifier);
            if (target.character.hitPoints >= target.character.maxHitPoints)
                target.character.hitPoints = target.character.maxHitPoints;
            if (abilitySelected.duration == 0)
                return;
            target.character.ApplyStatus(abilitySelected);
            target.character.AddAppliedStatus(abilitySelected);
            MoveAction.DisplayEnemyAcc(sessionManager);
        }

        void StatusAbility(GridCharacter target, bool self)
        {
            StatusAbility(target);
            ExitMode();
        }        

        // Targeting mode for ranged / ranged AoE Attacks
        void TargetingMode(int radius)
        {
            targetingMode = true;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000))                                 // if the raycast hits something
            {
                Node node = sessionManager.gridManager.GetNode(hit.point);          // get the node at the hit point 
                if (node != null)
                {
                    sessionManager.HighlightAroundCharacter(sessionManager.currentCharacter, node, radius);
                    ClearLastTargetTile();
                    if (node.tileRenderer != null)
                        if (!abilitySelected.buff)
                            node.tileRenderer.material = sessionManager.abilityTileMaterial;
                        else
                            node.tileRenderer.material = sessionManager.buffAbilityTileMaterial;

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

        void BlastRadius(SessionManager sessionManager, string teamName, bool buff)
        {
            string targetTeamName = "";
            if (teamName == "Player1") 
                if (buffAbilitySelected)
                    targetTeamName = "Player1";
                else
                    targetTeamName = "Enemy";

            if (teamName == "Enemy") 
                if (buffAbilitySelected)
                    targetTeamName = "Enemy";
                else
                    targetTeamName = "Player1";

            foreach (Node node in sessionManager.GetTargetNodes())
            {
                if (node.character != null)
                {
                    if (node.character.owner.name == targetTeamName)
                    {
                        AttackAction.attackAccuracy = AttackAction.GetAttackAccuracy(sessionManager, node);
                        int diceRoll = AttackAction.RollDDice(sessionManager);
                        if (diceRoll >= 0)
                        {
                            AbilityHit(sessionManager, node, sessionManager.currentCharacter);
                        }
                        else
                        {
                            // indicate that player missed
                            Debug.Log(abilitySelected.abilityName + " missed " + node.character.name);
                        }
                    }
                }
            }
        }

        void AbilityHit(SessionManager sessionManager, Node node, GridCharacter attacker)
        {
            GridCharacter defender = node.character;
            if (!buffAbilitySelected)
            {
                float damageDealt = AttackAction.DamageDealt(sessionManager, 2, attacker, defender, abilitySelected);
                defender.character.hitPoints -= Mathf.RoundToInt(damageDealt);
                StatusAbility(defender);
                defender.PlayAnimation("Death");

                if (defender.character.hitPoints <= 0)
                {
                    Debug.Log(defender.character.name + " defeated");
                    defender.Death();
                }
            }
            else 
            {
                StatusAbility(defender);
            }
        }

        void ClearLastTargetTile()
        {
            if (lastNode.tileRenderer != null)
                lastNode.tileRenderer.material = sessionManager.defaultTileMaterial;
        }

        void ExitMode()
        {
            buffAbilitySelected = false;
            ClearLastTargetTile();
            sessionManager.SetAction("MoveAction");
            sessionManager.popUpUI.Deactivate(sessionManager);
            targetingMode = false;
        }
    }
}
