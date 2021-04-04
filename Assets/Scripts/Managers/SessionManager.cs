using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace EV
{
    public class SessionManager : MonoBehaviour
    {
        int _turnIndex;

        public Turn[] turns;

        public GridManager gridManager;
        public GridCharacter currentCharacter;
        
        public bool isInit;
        public float delta;

        public VariablesHolder gameVariables;
        public LineRenderer reachablePathViz;
        public LineRenderer unReachablePathViz;
        bool isPathfinding;

        public Material defaultTileMaterial;
        public Material reachableTileMaterial;

        public int TurnIndex
        {
            get { return _turnIndex; }
        }

        public Turn GetTurn
        {
            get { return turns[TurnIndex]; }
        }

        #region Init
        private void Start()
        {
            InitGameActions();
            gridManager.Init();     // initialize the grid
            PlaceUnits();           // snap units to grid
            InitStateManagers();    // Initialize State Managers
            isInit = true;          // start the Update
            SetAction("MoveAction");
        }

        // Create an array of GridCharacters (all objects with the "GridCharacter script), ie Player Characters")
        // Initialize each unit (register character to PlayerHolder)
        //   Get the node from the unit's current coordinates
        //      set the unit's position to the node's position, node's character attribute is set to this unit, unit's current node is this node.
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

        // Add all characters to our party (PlayerHolder)
        void InitStateManagers()
        {
            foreach (Turn turn in turns)
            {
                turn.player.Init();
            }
        }

        #endregion

        #region Pathfinding

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

        void PathfinderCallback(List<Node> path, GridCharacter character)
        {
            isPathfinding = false;
            if (path == null)
            {
                // Debug.Log("Path is not valid");
                return;
            }

            List<Node> pathActual = new List<Node>();
            List<Vector3> reachablePositions = new List<Vector3>();
            List<Vector3> unreachablePositions = new List<Vector3>();
            Vector3 offset = Vector3.up * .1f;

            int actionPoints = character.ActionPoints;
            int actionPointsViz = 0;

            if (actionPoints > 0)
            {
                reachablePositions.Add(character.currentNode.worldPosition + offset);
            }

            if (path.Count - 1 > actionPoints)
            {
                if (actionPoints <= 0)
                {
                    unreachablePositions.Add(character.currentNode.worldPosition + offset);
                } 
                else 
                {
                    unreachablePositions.Add(path[character.ActionPoints - 1].worldPosition + offset);
                }
            }

            for (int i = 0; i < path.Count; i++)
            {
                Node prevNode = character.currentNode;
                
                if (i != 0)
                {
                    prevNode = path[i-1];
                }

                Vector3 direction = path[i].worldPosition - prevNode.worldPosition;
                if (direction.x != 0 && direction.z != 0)
                {
                    actionPoints -= 2;
                    actionPointsViz += 2;
                }
                else
                {
                    actionPoints -= 1;
                    actionPointsViz += 1;
                }
                
                if (actionPoints >= 0)
                {
                    pathActual.Add(path[i]);
                    reachablePositions.Add(path[i].worldPosition + offset);
                }
                else 
                {
                    unreachablePositions.Add(path[i].worldPosition + offset);
                }
            }

            if (pathActual.Count > 0)
            {
                if (unreachablePositions.Count > 0) 
                {
                    List<Vector3> newPos = new List<Vector3>();
                    newPos.Add(pathActual[pathActual.Count - 1].worldPosition + offset);
                    newPos.AddRange(unreachablePositions);
                    unreachablePositions = newPos;
                }
            }

            reachablePathViz.positionCount = pathActual.Count + 1;
            reachablePathViz.SetPositions(reachablePositions.ToArray());
            unReachablePathViz.positionCount = unreachablePositions.Count;
            unReachablePathViz.SetPositions(unreachablePositions.ToArray());

            gameVariables.UpdateMouseText(actionPointsViz.ToString());
            character.LoadPath(pathActual);
        }

        public void ClearPath(StateManager states)
        {
            reachablePathViz.positionCount = 0;
            unReachablePathViz.positionCount = 0;
            if (states.currentCharacter != null)
            {
                states.currentCharacter.currentPath = null;
            }
        }


        #endregion

        #region Tile Management
        List<Node> highlightedTiles;

        List<Node> GetNeighborsManhattan(Node center)
        {
            List<Node> returnVal = new List<Node>();

            for (int x = -1; x <= 1; x++)
            {
                Node node = gridManager.GetNode(center.x + x, center.y, center.z);
                if (node != null)
                {
                    if (node.isWalkable)
                    {
                        returnVal.Add(node);
                    }
                }
            }

            for (int z = -1; z <= 1; z++)
            {
                Node node = gridManager.GetNode(center.x, center.y, center.z + z);
                if (node != null)
                {
                    if (node.isWalkable)
                    {
                        returnVal.Add(node);
                    }
                }
            }

            return returnVal;
        }

        List<Node> GetNeighborsDiagonal(Node center)
        {
            List<Node> returnVal = new List<Node>();

            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 || z == 0) 
                    {
                        Node node = gridManager.GetNode(center.x + x, center.y, center.z + z);
                        if (node != null)
                        {
                            if (node.isWalkable)
                            {
                                returnVal.Add(node);
                            }
                        }
                    }

                }
            }
            return returnVal;
        }

        public void HighlightAroundCharacter(GridCharacter character)
        {
            currentCharacter = character;
            Node centerNode = character.currentNode;

            List<Node> reachableNodes = new List<Node>();
            List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();

            reachableNodes.Add(centerNode);
            openSet.Add(centerNode);
            centerNode.Steps = 0;
            int steps = 0; 

            OnHighlightCharacter(centerNode);

            while (openSet.Count > 0) // for (int i = 0; i < c.actionPoints-1; i++) <- this also worked lol
            {
                Node currentNode = openSet[0];
                steps = currentNode.Steps;

                if(currentNode.Steps <= character.ActionPoints)
                {
                    foreach (Node node in GetNeighborsManhattan(currentNode))
                    {
                        if (!closedSet.Contains(node))
                        {
                            int newStepCost = steps + 1;

                            if (newStepCost <= character.ActionPoints)
                            {
                                if (!openSet.Contains(node))
                                {
                                    openSet.Add(node);
                                    node.Steps = newStepCost;
                                    reachableNodes.Add(node);
                                }
                            }
                        }
                    }
                    foreach (Node node in GetNeighborsDiagonal(currentNode))
                    {
                        if (!closedSet.Contains(node))
                        {
                            int newStepCost = steps + 2;

                            if (newStepCost <= character.ActionPoints)
                            {
                                if (!openSet.Contains(node))
                                {
                                    openSet.Add(node);
                                    node.Steps = newStepCost;
                                    reachableNodes.Add(node);
                                }
                            }
                        }
                    }
                }
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);
            }
            UpdateListToReachableMaterial(reachableNodes);
        }

        void UpdateListToReachableMaterial(List<Node> list)
        {
            ClearReachableTiles();

            foreach (Node node in list)
            {
                node.tileRenderer.material = reachableTileMaterial;
                node.dddText.gameObject.SetActive(true);
            }

            highlightedTiles = list;
        }

        public void ClearReachableTiles()
        {
            if (highlightedTiles == null)
                return;
            foreach (Node node in highlightedTiles)
            {
                node.tileRenderer.material = defaultTileMaterial;
                node.dddText.gameObject.SetActive(false);
            }
            highlightedTiles.Clear();
        }
        #endregion

        #region Turn Management
        // Only runs after Initialization is complete
        // Run turns[0].execute(Sessionmanager)
        //    - if true, increment turnindex (shouldn't yet, we only have 1 turn 3/24/21)
        private void Update()
        {
            if (!isInit)
                return;

            delta = Time.deltaTime;
            if (turns[_turnIndex].Execute(this)) // in its current iteration, should just run turn[0] constantly 3/24/21
            {
                _turnIndex++;
                if (_turnIndex > turns.Length - 1)
                {
                    _turnIndex = 0;
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                if (!isAttack)
                {
                    isAttack = true;
                    SetAction("AttackAction");
                }
                else
                {
                    isAttack = false;
                    SetAction("MoveAction");
                }
            }
        }

        bool isAttack;

        public void Melee()
        {
            Debug.Log("Melee button pressed");
        }

        public void EndTurn()
        {
            // deHighlight the current player when the turn ends
            if (currentCharacter != null) 
            {
                currentCharacter.OnDeHighlight(turns[TurnIndex].player, true);
                ClearReachableTiles();
                ClearPath(turns[TurnIndex].player.stateManager);
            }
            turns[_turnIndex].EndCurrentPhase();
        }
        #endregion

        #region Events
        public SO.IntVariable stanceInt;
        public SO.IntVariable attackType;

        public void SetWeaponForCurrentPlayer()
        {
            switch(attackType.value)
            {
                case 0:
                    Debug.Log("Ranged Weapon Selected");
                    break;
                case 1:
                    Debug.Log("Melee Weapon Selected");
                    break;
            }
        }

        public void SetStanceForCurrentPlayer()
        {
            switch(stanceInt.value)
            {
                case 0:
                    turns[TurnIndex].player.stateManager.currentCharacter.ResetStance();
                    break;
                
                case 1:
                if (currentCharacter.ActionPoints >= 2)
                {
                        turns[TurnIndex].player.stateManager.currentCharacter.SetBrace();
                        currentCharacter.ActionPoints = 0;
                        ClearReachableTiles();
                        HighlightAroundCharacter(currentCharacter);
                }
                else {
                    Debug.Log("Not enough AP to brace");
                }
                break;
            }
        }
        #endregion

        #region  Game Actions Management
        Dictionary<string, GameAction> gameActions = new Dictionary<string, GameAction>();
        GameAction _gameAction;

        public GameAction GameAction
        {
            get { return _gameAction; }
        }

        void InitGameActions()
        {
            gameActions.Add("MoveAction", new MoveAction());
            gameActions.Add("AttackAction", new AttackAction());
        }

        public void SetAction(string targetAction)
        {
            GameAction gameAction = null;
            gameActions.TryGetValue(targetAction, out gameAction);

            if (gameAction.IsActionValid(this, GetTurn))
            {
                if (_gameAction != null) 
                    _gameAction.OnActionStop(this, GetTurn);
                _gameAction = gameAction;
                _gameAction.OnActionStart(this, GetTurn);
            }
            else
            {
                Debug.Log("Action is not valid");
            }
        }

        public void OnHighlightCharacter(Node node)
        {
            if (GameAction == null)
                return;
            
            GameAction.OnHighlightCharacter(this, GetTurn, node);
        }

        public void DoAction(Node node, RaycastHit hit)
        {
            if (GameAction == null)
                return;
            
            GameAction.OnDoAction(this, GetTurn, node, hit);
        }

        public void OnActionTick(Node node, RaycastHit hit)
        {
            if (GameAction == null)
                return; 

            GameAction.OnActionTick(this, GetTurn, node, hit);
        }

        #endregion
    }
}
