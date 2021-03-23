using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace EV
{
    public class SessionManager : MonoBehaviour
    {
        int turnIndex;
        public Turn[] turns;

        public GridManager gridManager;
        public bool isInit;

        public float delta;

        public LineRenderer pathViz;
        bool isPathfinding;

        #region Init

        private void Start()
        {
            gridManager.Init();
            PlaceUnits();
            InitStateManagers();
            isInit = true;
        }

        void InitStateManagers()
        {
            foreach (Turn turn in turns)
            {
                turn.player.Init();
            }
        }

        void PlaceUnits()
        {
            GridCharacter[] units = GameObject.FindObjectsOfType<GridCharacter>();
            foreach (GridCharacter unit in units)
            {
                unit.OnInit();

                Node node = gridManager.GetNode(unit.transform.position);
                if (node != null)
                {
                    unit.transform.position = node.worldPosition;
                    node.character = unit;
                    unit.currentNode = node;
                }
            }
        }

        #endregion

        #region Pathfinding Calls
        public void PathfinderCall(GridCharacter character, Node targetNode)
        {
            if (!isPathfinding)
            {
                isPathfinding = true;

                Node start = character.currentNode;
                Node target = targetNode;

                if (start != null && target != null)
                {
                    PathfinderMaster.singleton.RequestPathfind(character,
                        start, target, PathfinderCallback, gridManager);
                }
                else
                {
                    isPathfinding = false;
                }
            }
        }

        void PathfinderCallback(List<Node> p, GridCharacter c)
        {
            isPathfinding = false;
            if (p == null)
            {
                Debug.Log("Path is not valid");
                return;
            }

            pathViz.positionCount = p.Count + 1;
            List<Vector3> allPositions = new List<Vector3>();
            Vector3 offset = Vector3.up * .6f;

            allPositions.Add(c.currentNode.worldPosition + offset);
            for (int i = 0; i < p.Count; i++)
            {
                allPositions.Add(p[i].worldPosition + offset);
            }

            c.LoadPath(p);

            pathViz.SetPositions(allPositions.ToArray());
        }

        public void ClearPath(StateManager states)
        {
            pathViz.positionCount = 0;
            if (states.currentCharacter != null)
            {
                states.currentCharacter.currentPath = null;
            }
        }

        #endregion 

        #region Turn Management
        private void Update()
        {
            if (!isInit)
                return;

            delta = Time.deltaTime;
            
            if (turns[turnIndex].Execute(this))
            {
                turnIndex++;
                if (turnIndex > turns.Length - 1)
                {
                    turnIndex = 0;
                }
            }
        }
        #endregion
    }
}

