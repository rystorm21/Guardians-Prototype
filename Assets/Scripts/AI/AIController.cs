using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace EV
{
    public enum EnemyState
    {
        CharSelect = 0,
        MoveDecision = 1,
        Moving = 2,
        AttackDecision = 3,
        Attacking = 4
    }
    
    public class AIController : MonoBehaviour
    {
        [SerializeField]
        SessionManager sessionManager;
        StateManager states;
        public EnemyState enemyState;
        List<GridCharacter> enemies;
        List<GridCharacter> players;
        GridCharacter currentCharacter;
        GridCharacter closestCharacter;
        GridCharacter selectedTarget;
        float closestCharacterDistance;

        Node moveTargetNode;
        bool foundSafeCover;
        bool outOfRange;

        public static List<Node> coverNodes;

        public void Init()
        {
            enemies = sessionManager.turns[1].player.characters;
            players = sessionManager.turns[0].player.characters;
            states = sessionManager.turns[sessionManager.TurnIndex].player.stateManager;
            enemyState = EnemyState.CharSelect;
            StartCoroutine(EnemyFSM());
        }

        #region AI FSM
        IEnumerator EnemyFSM()
        {
            while(true)
            {
                yield return StartCoroutine(enemyState.ToString());
            }
        }

        IEnumerator CharSelect()
        {
            bool allDone = true;

            foreach (var character in enemies)
            {
                if (character.ActionPoints > 0)
                {
                    allDone = false;
                    sessionManager.turns[sessionManager.TurnIndex].player.stateManager.CurrentCharacter = character;
                    sessionManager.currentCharacter = character;
                    currentCharacter = character;
                    enemyState = EnemyState.MoveDecision;
                    break;
                }
            }

            if (allDone)
            {
                Debug.Log("enemy turn completed.");
                StopAllCoroutines();
                sessionManager.EndTurn();
            }
            yield return null;
        }

        IEnumerator MoveDecision()
        {
            FindSafeCover(players, states);
            if (foundSafeCover)
                enemyState = EnemyState.Moving;
            if (moveTargetNode == null && !outOfRange)
                enemyState = EnemyState.AttackDecision;
            sessionManager.PathfinderCall(currentCharacter, moveTargetNode);
            sessionManager.moveInProgress = true;
            yield return new WaitForSeconds(.05f);
        }

        IEnumerator Moving()
        {
            states.SetState("moveOnPath");
            if (!sessionManager.moveInProgress)
            {
                enemyState = EnemyState.AttackDecision;
            }
            yield return new WaitForSeconds(.5f);
        }

        IEnumerator AttackDecision()
        {
            int highThreatLevel = FindHighestThreatLevel(players);
            selectedTarget = FindTarget(players, highThreatLevel);
            if (selectedTarget == null)
                enemyState = EnemyState.MoveDecision;
            AttackAction.lastTarget = selectedTarget;
            // Debug.Log("target selected: " + selectedTarget.name);
            if (currentCharacter.ActionPoints >= 4)
                enemyState = EnemyState.Attacking;
            else
            {
                Debug.Log(currentCharacter.character.name + " move completed");
                currentCharacter.ActionPoints = 0;
                enemyState = EnemyState.CharSelect;
            }
            yield return null;
        }

        IEnumerator Attacking()
        {
            Vector3 attackerPosition = currentCharacter.transform.position;
            Vector3 defenderPosition = AttackAction.lastTarget.transform.position;
            GridCharacter target = AttackAction.lastTarget;
            Node node = target.currentNode;

            MoveAction.DisplayEnemyAcc(sessionManager);
            AttackAction.attackAccuracy = AttackAction.GetAttackAccuracy(sessionManager.currentCharacter, target, false);
            float distance = Vector3.Distance(attackerPosition, defenderPosition);
            int index = 0;
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
            yield return new WaitForSeconds(2f);
            if (!AttackAction.attackInProgress)
                enemyState = EnemyState.AttackDecision;
        }
        #endregion

        #region Move Decision Making

        // Find a covered node that's in movement range
        void FindSafeCover(List<GridCharacter> players, StateManager states)
        {
            int howManyFlankedBy = FlankedBy(currentCharacter, null, players);
            moveTargetNode = null;
            currentCharacter.character.flankedBy = howManyFlankedBy;

            if (howManyFlankedBy == 0)
                return;

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
                moveTargetNode = closestCover;
                outOfRange = false;
                foundSafeCover = true;
            }
            // if no safe nodes are added, enemy is out of range
            else
            {
                outOfRange = true;
                Node closestNode = new Node();
                closestCharacterDistance = 100;

                Debug.Log("enemy will advance to closest square");
                foreach (Node node in sessionManager.reachableNodesAI)
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
                moveTargetNode = closestNode;
                foundSafeCover = true;
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
        GridCharacter ClosestCharacterToNode(List<GridCharacter> players, Node node)
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

            foreach (GridCharacter target in players)
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
    }

}