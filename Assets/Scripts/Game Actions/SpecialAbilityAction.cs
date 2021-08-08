using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace EV 
{
    public class SpecialAbilityAction : GameAction
    {
        public static bool buffAbilitySelected;
        public static bool abilityInProgress;
        public static Node targetedNode;
        public static GameObject explosion;
        public GameObject shootPrefab;
        public GameObject channelPrefab;
        SessionManager sessionManager;
        GridCharacter currentCharacter;
        Ability abilitySelected;
        Node lastNode;
        int selection;
        bool targetingMode;
        bool abilityFinished;
        Vector3 yOffset = new Vector3(0, 1f, 0);

        #region Base Methods
        public override void OnActionStart(SessionManager _sessionManager, Turn turn)
        {
            abilityFinished = false;
            sessionManager = _sessionManager;
            currentCharacter = sessionManager.currentCharacter;
            selection = currentCharacter.character.abilitySelected;
            abilitySelected = currentCharacter.character.abilityPool[selection].ability;
            currentCharacter.character.abilityInUse = abilitySelected;
            lastNode = sessionManager.currentCharacter.currentNode;
            sessionManager.ClearReachableTiles();
            buffAbilitySelected = abilitySelected.buff;
            MoveAction.DisplayEnemyAcc(sessionManager);

            if (currentCharacter.ActionPoints < abilitySelected.apCost)
            {
                // if trying to use an ability without enough AP
                Debug.Log("Not enough AP to use this ability.");
                ExitMode();
                return;
            }
            sessionManager.currentCharacter.currentNode.tileRenderer.material = sessionManager.defaultTileMaterial;
            currentCharacter.PlayAnimation(abilitySelected.windupAnimation);
            if (abilitySelected.channelPrefab != null)
                channelPrefab = GameObject.Instantiate(abilitySelected.channelPrefab, currentCharacter.transform.position + (Vector3.up * abilitySelected.animOrigin), Quaternion.identity);

            switch (abilitySelected.type.ToString())
            {
                case "Self":
                    sessionManager.HighlightAroundCharacter(currentCharacter, currentCharacter.currentNode, abilitySelected.radius);
                    break;
                case "PBAoE":
                    sessionManager.HighlightAroundCharacter(currentCharacter, currentCharacter.currentNode, abilitySelected.radius);
                    break;
                case "Ranged":
                    break;
                case "RangedAoe":
                    break;
            }
            sessionManager.gameVariables.UpdateMouseText("");
            sessionManager.popUpUI.Activate(sessionManager);
            sessionManager.popUpUI.DisplaySkill(sessionManager, abilitySelected.abilityName, abilitySelected.description);
        }

        public override void OnActionStop(SessionManager sessionManager, Turn turn)
        {
            DestroyChannelPrefab();
            ClearLastTargetTile();
            abilityInProgress = false;
            buffAbilitySelected = false;
            currentCharacter.character.abilityInUse = null;
            targetingMode = false;
            sessionManager.APCheck();
        }

        public override void OnActionTick(SessionManager sm, Turn turn, Node node, RaycastHit hit)
        {
            // if ability is in progress and not finished...
            if (abilityInProgress && !abilityFinished)
            {
                // if a shot is fired (shoot prefab), move it towards the target and explode it. If a ray is shot, bascially do the same thing
                if (shootPrefab != null)
                {
                    shootPrefab.transform.position = Vector3.MoveTowards(shootPrefab.transform.position, targetedNode.worldPosition + yOffset, abilitySelected.projectileSpeed * Time.deltaTime);
                    if (Vector3.Distance(targetedNode.worldPosition + yOffset, shootPrefab.transform.position) < 1)
                    {
                        ShotExplosion(sm, targetedNode);
                    }
                    if (abilitySelected.rayAbility)
                    {
                        UnityEngine.Object.Destroy(shootPrefab, abilitySelected.animationDuration);
                    }
                }
                // if an ability is in progress but shootPrefab is null, that means the object has been destroyed. Play the explosion animation.
                else
                {
                    ShotExplosion(sm, targetedNode);
                }
            }
            // if the ability is in progress and finished, check to see if the explosion is null. If that's the case, then we can exit and return to MoveAction Mode.
            if (abilityInProgress && abilityFinished)
            {
                if (explosion == null)
                {
                    ExitMode(); //
                }
            }

            if (sessionManager.powerActivated.value && !abilityInProgress)
            {
                if (!abilityFinished)
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

        public override void OnDoAction(SessionManager sm, Turn turn, Node node, RaycastHit hit)
        {
            if (node != null && !abilityInProgress)
            {
                if (Vector3.Distance(currentCharacter.currentNode.worldPosition, node.worldPosition) > abilitySelected.range)
                {
                    Debug.Log("target out of range.");
                    Debug.Log("Distance: " + Vector3.Distance(currentCharacter.currentNode.worldPosition, node.worldPosition));
                    return;
                }
                if (targetingMode && !abilityInProgress)
                {
                    string abilityTypeInUse;
                    abilityTypeInUse = abilitySelected.type.ToString();
                    BlastRadius(sm, currentCharacter.owner.name, node, false);
                    sessionManager.currentCharacter.ActionPoints -= abilitySelected.apCost;
                }
            }
        }
        #endregion

        #region Deciding Ability Types
        void AbilityType(SessionManager sessionManager)
        {
            targetingMode = false;
            switch (abilitySelected.type.ToString())
            {
                case "Self":
                    // insert logic for self-based abilities
                    targetingMode = true;
                    StatusAbility(currentCharacter, true);
                    sessionManager.currentCharacter.ActionPoints -= abilitySelected.apCost;
                    break;
                case "PBAoE":
                    // insert logi)c for player-based aoe abilities
                    targetingMode = true;
                    BlastRadius(sessionManager, currentCharacter.owner.name, currentCharacter.currentNode, false);
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
            target.healthBar.SetHealth(target.character.hitPoints);
            if (abilitySelected.duration == 0)
                return;
            target.character.ApplyStatus(abilitySelected);
            target.character.AddAppliedStatus(abilitySelected);
        }

        void StatusAbility(GridCharacter target, bool self)
        {
            abilityInProgress = true;
            explosion = GameObject.Instantiate(abilitySelected.explosionEffectPrefab, target.transform.position, Quaternion.identity);
            DestroyChannelPrefab();
            abilityFinished = true;
            UnityEngine.Object.Destroy(explosion, 2.0f);
            StatusAbility(target);
        }

        void DestroyChannelPrefab()
        {
            if (channelPrefab)
                UnityEngine.Object.Destroy(channelPrefab);
        }
        #endregion        

        #region Targeting
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
                    currentCharacter.transform.LookAt(node.worldPosition);
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

        void BlastRadius(SessionManager sessionManager, string teamName, Node targetNode, bool buff)
        {
            abilityInProgress = true;
            Debug.Log("ability in progress");
            if (abilitySelected.actionAnimation != "")
                currentCharacter.PlayAnimation(abilitySelected.actionAnimation);
            if (abilitySelected.shotAbility || abilitySelected.rayAbility)
            {
                ShootIt(targetNode);
            }
            if (abilitySelected.type.ToString() == "PBAoE")
            {
                ShotExplosion(sessionManager, targetedNode);
                DestroyChannelPrefab();
            }

        }
        
        void VerifyHits()
        {
            string teamName = currentCharacter.owner.name;
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
                        AttackAction.attackAccuracy = AttackAction.GetAttackAccuracy(sessionManager.currentCharacter, node.character, abilitySelected.ignoreCover);
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

        void ShootIt(Node node)
        {
            if (node != null)
            {
                currentCharacter.transform.LookAt(node.worldPosition);
                shootPrefab = GameObject.Instantiate(abilitySelected.shootPrefab, currentCharacter.transform.position + (Vector3.up * abilitySelected.animOrigin) + (Vector3.forward), currentCharacter.transform.rotation);
                DestroyChannelPrefab();
                targetedNode = node;
            }
        }

        void ShotExplosion(SessionManager sessionManager, Node node)
        {
            Vector3 explosionTarget = new Vector3();            
            if (abilitySelected.shotAbility)
            {
                explosionTarget = shootPrefab.transform.position;
                UnityEngine.Object.Destroy(shootPrefab);
            }
            if (abilitySelected.rayAbility)
            {
                explosionTarget = node.worldPosition;
            }
            if (abilitySelected.type.ToString() == "PBAoE")
            {
                explosionTarget = currentCharacter.currentNode.worldPosition;
            }
            explosion = GameObject.Instantiate(abilitySelected.explosionEffectPrefab, explosionTarget, Quaternion.identity);
            abilityFinished = true;
            UnityEngine.Object.Destroy(explosion, 2.0f);
            VerifyHits();
            sessionManager.ClearReachableTiles();
        }

        void AbilityHit(SessionManager sessionManager, Node node, GridCharacter attacker)
        {
            GridCharacter defender = node.character;
            if (!buffAbilitySelected)
            {
                float damageDealt = AttackAction.DamageDealt(sessionManager, 2, attacker, defender, abilitySelected);
                defender.character.hitPoints -= Mathf.RoundToInt(damageDealt);
                defender.healthBar.SetHealth(defender.character.hitPoints);
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
        #endregion

        void ClearLastTargetTile()
        {
            if (lastNode.tileRenderer != null)
                lastNode.tileRenderer.material = sessionManager.defaultTileMaterial;
        }

        public void ExitMode()
        {
            sessionManager.currentCharacter.character.abilitySelected = 0;
            sessionManager.popUpUI.Deactivate(sessionManager);
        }
    }
}
