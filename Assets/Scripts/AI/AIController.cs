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

        public static List<Node> coverNodes;

        public void Init()
        {
            List<GridCharacter> enemies = sessionManager.turns[1].player.characters;
            List<GridCharacter> players = sessionManager.turns[0].player.characters;
            bool allDone = true;

            foreach (var character in enemies)
            {
                if (character.ActionPoints > 0 && !character.character.aiMoveComplete)
                {
                    allDone = false;
                    currentCharacter = character;
                    MoveDecision(character, players);
                    break;
                }
            }

            if (allDone)
            {
                Debug.Log("enemy turn completed.");
            }
        }
        
        private void Update() 
        {
            if (moveCompleted)
            {
                Debug.Log(currentCharacter + " move completed");
                moveCompleted = false;
                Init();
            }
        }

        void MoveDecision(GridCharacter enemy, List<GridCharacter> players)
        {
            // enemy will analyze the battlefield and decide where to go. Enemy may decide to stay put. 
            // enemy will first check if it's in a vulnerable position (flanked)
            int flankedBy = FlankedBy(enemy, null, players);
            enemy.character.flankedBy = flankedBy;
            if (flankedBy > 0)
            {
                // find a safer location to flee to
                FindSafeCover(players);
            }
            else
            {
                // take aggressive action
                AttackDecision(players);
            }
        }

        // Thinking... Thinking...
        // Find a covered node that's in movement range
        void FindSafeCover(List<GridCharacter> players)
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
                StartCoroutine(MoveAICharacter(closestCover, players));
            }
        }

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

        void AttackDecision(List<GridCharacter> players)
        {
            if (currentCharacter.ActionPoints > 4)
            {
                StartCoroutine(AttackAICharacter());
            }
            else
            {
                currentCharacter.character.aiMoveComplete = true;
                moveCompleted = true;
            }
        }

        void EndTurn()
        {

        }

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

        IEnumerator MoveAICharacter(Node node, List<GridCharacter> players)
        {
            StateManager states = sessionManager.turns[sessionManager.TurnIndex].player.stateManager;
            yield return new WaitForSeconds(1);
            sessionManager.PathfinderCall(currentCharacter, node);
            node.tileRenderer.material = sessionManager.abilityTileMaterial; // highlight closest safe node
            
            yield return new WaitForSeconds(1);
            states.SetState("moveOnPath");
            yield return new WaitForSeconds(1);
            AttackDecision(players);
        }

        IEnumerator AttackAICharacter()
        {
            Debug.Log("attack yo");
            yield return new WaitForSeconds(1);
            currentCharacter.ActionPoints -= 4;
            sessionManager.HighlightAroundCharacter(currentCharacter, null, 0);
            yield return new WaitForSeconds(1);
            if (currentCharacter.ActionPoints > 4)
                AttackDecision(players);
        }
    }

}