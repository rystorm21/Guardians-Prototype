using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    public class AIController : MonoBehaviour
    {
        [SerializeField]
        SessionManager sessionManager;
        GridCharacter currentCharacter;
        List<GridCharacter> enemies;
        List<GridCharacter> players;
        bool moveCompleted;
        bool firstInit = true;

        public static List<Node> coverNodes;

        public void Init()
        {
            List<GridCharacter> enemies = sessionManager.turns[1].player.characters;
            List<GridCharacter> players = sessionManager.turns[0].player.characters;
            StateManager states = sessionManager.turns[sessionManager.TurnIndex].player.stateManager;
            
            if (firstInit)
            {
                foreach (var character in enemies)
                {
                    character.character.aiMoveComplete = false;
                }
            }

            bool allDone = true;

            foreach (var character in enemies)
            {
                if (character.ActionPoints > 0 && !character.character.aiMoveComplete)
                {
                    allDone = false;
                    firstInit = false;
                    sessionManager.turns[sessionManager.TurnIndex].player.stateManager.CurrentCharacter = character;
                    sessionManager.currentCharacter = character;
                    currentCharacter = character;
                    MoveDecision(character, players, states);
                    break;
                }
            }

            if (allDone)
            {
                Debug.Log("enemy turn completed.");
                firstInit = true;
                sessionManager.moveInProgress = false;
                sessionManager.EndTurn();
            }
        }
  
        #region Move Decision Making
        void MoveDecision(GridCharacter enemy, List<GridCharacter> players, StateManager states)
        {
            // enemy will analyze the battlefield and decide where to go. Enemy may decide to stay put. 
            // enemy will first check if it's in a vulnerable position (flanked)
            int flankedBy = FlankedBy(enemy, null, players);
            enemy.character.flankedBy = flankedBy;
            

            if (flankedBy > 0)
            {
                // find a safer location to flee to
                FindSafeCover(players, states);
            }
            else
            {
                // take aggressive action
                AttackDecision(players);
            }
        }

        // Find a covered node that's in movement range
        void FindSafeCover(List<GridCharacter> players, StateManager states)
        {
            List<Node> safeNodes = new List<Node>();
            int flankedBy = FlankedBy(currentCharacter, null, players);
            sessionManager.HighlightAroundCharacter(currentCharacter, null, 0);
            Node closestCover = new Node();
            float closestDistance = currentCharacter.ActionPoints;
            
            foreach (Node node in coverNodes)
            {
                if (ClearShot(node, players) == false)
                {
                    safeNodes.Add(node);
                }
            }
            if (safeNodes != null)
            {
                foreach (Node node in safeNodes)
                {
                    float distanceToNode = Vector3.Distance(currentCharacter.transform.position, node.worldPosition);
                    if (distanceToNode < closestDistance)
                    {
                        closestDistance = distanceToNode;
                        closestCover = node;
                    }
                }
                StartCoroutine(MoveAICharacter(closestCover, players, states));
            }
        }

        // checks if reachable cover node is not a flanked position (clear shot), returns true or false
        bool ClearShot(Node node, List<GridCharacter> players)
        {
            bool clearShot = false;
            foreach (GridCharacter player in players)
            {
                if (!Physics.Linecast(player.transform.position, node.worldPosition))
                {
                    clearShot = true;
                }
            }
            if (!clearShot)
            {
                node.tileRenderer.material = sessionManager.reachableTileMaterial;
            }
            return clearShot;
        }

        // checks how many players are flanking the enemy's current position & returns the value.
        int FlankedBy(GridCharacter enemy, Node node, List<GridCharacter> players)
        {
            int flanked = 0;
            foreach (GridCharacter player in players)
            {
                if (AttackAction.CheckTargetCover(player, enemy) == 0)
                {
                    flanked++;
                }
            }
            return flanked;
        }
        #endregion

        #region Attack Decision Making
        void AttackDecision(List<GridCharacter> players)
        {
            int highThreatLevel = FindHighestThreatLevel(players);
            GridCharacter selectedTarget = FindTarget(players, highThreatLevel);
            AttackAction.lastTarget = selectedTarget;
            Debug.Log("target selected: " + selectedTarget.name);
            if (currentCharacter.ActionPoints > 4)
            {
                StartCoroutine(AttackAICharacter(sessionManager.currentCharacter.transform.position, selectedTarget.transform.position, selectedTarget, selectedTarget.currentNode));
            }
            else
            {
                Debug.Log(currentCharacter.character.name + " move completed");
                currentCharacter.character.aiMoveComplete = true;
                moveCompleted = true;
                Init();
            }
        }

        int FindHighestThreatLevel(List<GridCharacter> players)
        {
            int threatLevel = 0;
            foreach (GridCharacter target in players)
            {
                if (target.character.threatLevel > threatLevel && !target.character.KO)
                {
                    threatLevel = target.character.threatLevel;
                }
            }
            return threatLevel;
        }

        GridCharacter FindTarget(List<GridCharacter> players, int highestThreat)
        {
            GridCharacter targetSelected = new GridCharacter();
            List<GridCharacter> possibleTargets = new List<GridCharacter>();
            int index = 0;

            foreach(GridCharacter target in players)
            {   
                if (target.character.threatLevel == highestThreat && !target.character.KO)
                    possibleTargets.Add(target);
            }

            System.Random random = new System.Random();
            index = random.Next(possibleTargets.Count);
            targetSelected = possibleTargets[index];

            return targetSelected;
        }
        #endregion

        #region Move Co-Routines
        IEnumerator MoveAICharacter(Node node, List<GridCharacter> players, StateManager states)
        {
            yield return new WaitForSeconds(1);
            sessionManager.PathfinderCall(currentCharacter, node);
            //node.tileRenderer.material = sessionManager.abilityTileMaterial; // highlight closest safe node
            
            yield return new WaitForSeconds(1);
            states.SetState("moveOnPath");
            yield return new WaitForSeconds(1);
            AttackDecision(players);
        }
        #endregion

        #region Attack Co-Routines
        IEnumerator AttackAICharacter(Vector3 attackerPosition, Vector3 defenderPosition, GridCharacter target, Node node)
        {
            MoveAction.DisplayEnemyAcc(sessionManager);
            AttackAction.attackAccuracy = AttackAction.GetAttackAccuracy(sessionManager.currentCharacter, target, false);
            float distance = Vector3.Distance(attackerPosition, defenderPosition);
            int index = 0;
            RaycastHit[] coverHits;
            coverHits = Physics.RaycastAll(attackerPosition, defenderPosition - attackerPosition, distance + 1);
            for (int i = 0; i < coverHits.Length; i++)
            {
                if (coverHits[i].transform.gameObject.name == target.name)
                    index = i;
            }
            yield return new WaitForSeconds(2);

            sessionManager.SetAction("AttackAction");
            sessionManager.DoAction(node, coverHits[index]);
            sessionManager.HighlightAroundCharacter(currentCharacter, null, 0);
            yield return new WaitForSeconds(1);
            Init();
        }
        #endregion
    }

}