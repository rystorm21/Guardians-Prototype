using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// review: Session 9 51:30
namespace EV
{
    public class Pathfinder
    {
        #region variables
        GridManager gridManager;
        GridCharacter character;
        Node startNode;
        Node endNode;

        public volatile float timer;
        public volatile bool jobDone = false;

        public delegate void PathfindingComplete(List<Node> n, GridCharacter character);
        public PathfindingComplete completeCallback;
        List<Node> targetPath;

        #endregion

        public Pathfinder(GridCharacter c, Node start, Node target, PathfindingComplete callback, GridManager gridManager)
        {
            this.gridManager = gridManager;
            character = c;
            startNode = start;
            endNode = target;
            completeCallback = callback;
        }

        public void FindPath()
        {
            targetPath = FindPathActual();
            jobDone = true;
        }

        public void NotifyComplete()
        {
            if (completeCallback != null) 
            {
                completeCallback(targetPath, character);
            }
        }

        List<Node> FindPathActual()
        {
            List<Node> foundPath = new List<Node>();
            List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();

            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet[0];

                for (int i = 0; i < openSet.Count; i++)
                {
                    if (openSet[i].fCost < currentNode.fCost || (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                    {
                        if (!currentNode.Equals(openSet[i]))
                        {
                            currentNode = openSet[i];
                        }
                    }
                }
                
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                if (currentNode.Equals(endNode))
                {
                    foundPath = RetracePath(startNode, currentNode);
                    break;
                }

                foreach(Node neighbor in GetNeighbors(currentNode)) 
                {
                    if (!closedSet.Contains(neighbor))
                    {
                        float newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);

                        if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                        {
                            neighbor.gCost = newMovementCostToNeighbor;
                            neighbor.hCost = GetDistance(neighbor, endNode);
                            neighbor.parentNode = currentNode;
                            if (!openSet.Contains(neighbor))
                            {
                                openSet.Add(neighbor);
                            }
                        }
                    }
                }
            }
            return foundPath;
        }

        int GetDistance(Node posA, Node posB)
        {
            int distX = Mathf.Abs(posA.x - posB.x);
            int distZ = Mathf.Abs(posA.z - posB.z);

            if (distX > distZ)
            {
                return 14 * distZ + 10 * (distX - distZ);
            }

            return 14 * distX + 10 * (distZ - distX);
        }

        List<Node> GetNeighbors(Node node)
        {
            List<Node> retList = new List<Node>();

            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 && z == 0)
                        continue;

                    int _x = x + node.x;
                    int _y = node.y;
                    int _z = z + node.z;

                    Node n = GetNode(_x, _y, _z); 
                    if (n != null) // modification due to nullreferenceExceptions when trying to assign Node newNode = GetNeighbor(n)
                    {
                        Node newNode = GetNeighbor(n);

                        if (newNode != null)
                        {
                            retList.Add(newNode);
                        }
                    }
                }
            }
            return retList;
        }

        Node GetNode(int x, int y, int z) 
        {
            return gridManager.GetNode(x, y, z);
        }

        Node GetNeighbor(Node currentNode)
        {
            Node retVal = null;
            if (currentNode != null) 
            {
                if (currentNode.isWalkable)
                {
                    Node aboveNode = GetNode(currentNode.x, currentNode.y + 1, currentNode.z);

                    if (aboveNode == null || aboveNode.isAir || character.isCrouched)
                    {
                        retVal = currentNode;
                    }
                    else
                    {
                        if (character.isCrouched)
                        {
                            retVal = currentNode;
                        }
                    }
                }
            }
            return retVal;
        }

        List<Node> RetracePath(Node start, Node end)
        {
            List<Node> path = new List<Node>();
            Node currentNode = endNode;
            while (currentNode != start)
            {
                path.Add(currentNode);
                currentNode = currentNode.parentNode;
            }
            path.Reverse();
            return path;
        }
    }
}