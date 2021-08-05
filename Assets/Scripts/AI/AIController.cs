using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace EV
{
    public class AIController : MonoBehaviour
    {
        [SerializeField]
        SessionManager sessionManager;
        GridCharacter currentCharacter;
        GridCharacter closestCharacter;
        float closestCharacterDistance;
        List<GridCharacter> enemies;
        List<GridCharacter> players;
        bool moveCompleted;
        bool firstInit = true;

        public static List<Node> coverNodes;

        private void Update()
        {
            if (MoveCharacterOnPath.moveComplete)
            {
                Debug.Log("Move Done");
                Init();
                MoveCharacterOnPath.moveComplete = false;
            }
        }

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
            Node closestCover = new Node();
            int flankedBy = FlankedBy(currentCharacter, null, players);
            int safestNode = players.Count;
            sessionManager.HighlightAroundCharacter(currentCharacter, null, 0);
            float closestDistance = currentCharacter.ActionPoints;
            closestCharacterDistance = 100;
            foreach (Node node in coverNodes)
            {
                int nodeSafety = ClearShot(node, players);
                if (nodeSafety < safestNode)
                    safestNode = nodeSafety;
            }
            // Debug.Log(currentCharacter.name + " is flanked by " + flankedBy + " enemies. " + "safest node has " + safestNode + " clear shots");
            // Debug.Log("Closest Character is :" + closestCharacter.name + ", at distance " + closestCharacterDistance);
            foreach (Node node in coverNodes)
            {
                if (ClearShot(node, players) == safestNode)
                {
                    if (Vector3.Distance(closestCharacter.transform.position, node.worldPosition) < currentCharacter.character.rangedAttackRange)
                    {
                        safeNodes.Add(node);
                    }
                }
            }

            if (safeNodes.Count > 0)
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
            // if no safe nodes are added, enemy is out of range
            else 
            {
                Node closestNode = new Node();
                closestCharacterDistance = 100;

                Debug.Log("enemy will advance to closest square");
                foreach(Node node in sessionManager.reachableNodesAI)
                {
                    float distance = Vector3.Distance(node.worldPosition, closestCharacter.transform.position);
                    if (distance < closestCharacterDistance)
                    {
                        if (!closestCharacter.character.KO)
                        {
                            closestCharacterDistance = distance;
                            closestNode = node;
                        }
                    }
                }
                StartCoroutine(MoveAICharacter(closestNode, players, states));
            }
        }

        // This method returns the closest player to the current character. Characters that are KO'ed are not counted.
        GridCharacter ClosestCharacterToPlayer(List<GridCharacter> players)
        {
            GridCharacter closestCharacterToPlayer = new GridCharacter();
            float distance = 100f;

            foreach (GridCharacter player in players)
            {
                if (!player.character.KO)
                {
                    float distanceBetweenChars = Vector3.Distance(player.transform.position, currentCharacter.transform.position);
                    if (distanceBetweenChars < distance)
                    {
                        distance = distanceBetweenChars;
                        closestCharacterToPlayer = player;
                    }
                }
            }
            return closestCharacterToPlayer;
        }

        // This method returns the closest player to the node being checked.
        GridCharacter ClosestCharacterToNode(List <GridCharacter> players, Node node)
        {
            GridCharacter closestCharacterToNode = new GridCharacter();
            float distance = 100f;
            foreach (GridCharacter player in players)
            {
                if (!player.character.KO)
                {
                    float distanceBetweenPoints = Vector3.Distance(player.transform.position, node.worldPosition);
                    if (distanceBetweenPoints < distance)
                    {
                        distance = distanceBetweenPoints;
                        closestCharacterToNode = player;
                    }
                }
            }
            return closestCharacterToNode;
        }

        // checks if reachable cover node is not a flanked position (clear shot), returns true or false
        int ClearShot(Node node, List<GridCharacter> players)
        {
            Collider[] nearbyCover = Physics.OverlapSphere(node.worldPosition, 1);
            Vector3 offset = Vector3.up;
            bool nodeIsCovered = false;
            int clearShots = 0;
            foreach (GridCharacter player in players)
            {
                Vector3 attackerPosition = player.transform.position + offset;
                Vector3 defenderPosition = node.worldPosition + offset;
                RaycastHit[] coverHits;
                coverHits = Physics.RaycastAll(defenderPosition, attackerPosition - defenderPosition, Vector3.Distance(defenderPosition, attackerPosition));
                float distance = 100;
                if (!player.character.KO)
                    distance = Vector3.Distance(player.transform.position, node.worldPosition);
                if (distance < closestCharacterDistance)
                {
                    closestCharacterDistance = distance;
                    closestCharacter = player;
                }
                if (coverHits.Length > 1)
                {
                    foreach (var hit in coverHits)
                    {
                        if (hit.transform.gameObject.tag == "Cover-High" || hit.transform.gameObject.tag == "Cover-Low")
                        {
                            // if this is triggered, then a 'cover' object was hit.
                            // now we have to find out if the cover object is next to the cover node.
                            Collider[] hitColliders = Physics.OverlapSphere(node.worldPosition, 1);
                            if (hitColliders != null)
                            {
                                foreach (Collider neighbor in hitColliders)
                                {
                                    if (neighbor.name == hit.transform.gameObject.name)
                                    {
                                        // if the cover object that was hit is the same as the neighbor's cover, then the position has to be covered.
                                        nodeIsCovered = true;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    // if there's no cover between node and object, it's a clear shot
                    nodeIsCovered = false;
                }
                if (!nodeIsCovered)
                    clearShots++;
            }
            if (clearShots == 0)
            {
                node.tileRenderer.material = sessionManager.reachableTileMaterial;
            }
            return clearShots;
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
            if (selectedTarget == null)
                return;
            AttackAction.lastTarget = selectedTarget;
            Debug.Log("target selected: " + selectedTarget.name);
            if (currentCharacter.ActionPoints > 4)
            {
                StartCoroutine(AttackAICharacter(sessionManager.currentCharacter.transform.position, selectedTarget.transform.position, selectedTarget, selectedTarget.currentNode));
            }
            else
            {
                Debug.Log(currentCharacter.character.name + " move completed");
                currentCharacter.ActionPoints = 0;
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
                {
                    if (Vector3.Distance(target.transform.position, currentCharacter.transform.position) < currentCharacter.character.rangedAttackRange)
                    {
                        possibleTargets.Add(target);
                    }
                }
            }

            if (possibleTargets.Count == 0)
                return null;

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
            // node.tileRenderer.material = sessionManager.abilityTileMaterial; // highlight closest safe node
            sessionManager.moveInProgress = true;
            yield return new WaitForSeconds(1);
            states.SetState("moveOnPath");
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
            yield return new WaitForSeconds(2);
            RaycastHit[] coverHits;
            coverHits = Physics.RaycastAll(attackerPosition, (defenderPosition + Vector3.up) - attackerPosition, distance + 1); // change this
            Debug.DrawRay(attackerPosition, (defenderPosition + Vector3.up) - attackerPosition);
            for (int i = 0; i < coverHits.Length; i++)
            {
                // Debug.Log(coverHits[i].transform.gameObject.name);
                if (coverHits[i].transform.gameObject.name == target.name)
                {
                    index = i;
                }
            }
            sessionManager.SetAction("AttackAction");
            sessionManager.DoAction(node, coverHits[index]);
            sessionManager.HighlightAroundCharacter(currentCharacter, null, 0);
            yield return new WaitForSeconds(2);
            Init();
        }
        #endregion
    }

}