using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using PixelCrushers.DialogueSystem;

namespace EV
{
    public enum GameState 
    {
        Noncombat, Combat, GameOver, Dialog
    }

    public enum PlayerTurn
    {
        Player, Enemy
    }
    
    public class SessionManager : MonoBehaviour
    {
        public static GameState currentGameState;
        public static bool combatVictory;
        public KeepAlive keepAlive;
        public bool moveInProgress;
        public bool enemyTurn;
        bool gameOverScreenLoaded;
        
        int _turnIndex;
        public Turn[] turns;
        public Level currentLevel;
        public AIController aIController;
        public GameObject aiBrain;

        public GridManager gridManager;
        public GridCharacter currentCharacter;
        GameObject enemyGroup;
        
        public bool isInit;
        public float delta;

        public PopUpUI popUpUI;
        public GameObject uiCanvas;
        public VariablesHolder gameVariables;
        public LineRenderer reachablePathViz;
        public LineRenderer unReachablePathViz;
        bool isPathfinding;

        public Material defaultTileMaterial;
        public Material reachableTileMaterial;
        public Material reachableEnemyTileMaterial;
        public Material abilityTileMaterial;
        public Material buffAbilityTileMaterial;
        public Material testCoverMaterial;
        public List<Node> reachableNodesAI;
        private List<Node> targetedNodes;

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
            uiCanvas = GameObject.Find("UI Canvas");
            enemyGroup = GameObject.Find("Enemies");
            Application.targetFrameRate = 60;
            InitGameActions();
            gridManager.Init();     // initialize the grid
            PlaceUnits();           // snap units to grid
            InitStateManagers();    // Initialize State Managers
            isInit = true;          // start the Update
            SetAction("MoveAction");
            currentGameState = currentLevel.startingMode;
            popUpUI.Deactivate(this);
            uiCanvas.SetActive(false);
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
                unit.character.ZeroStatus();
                Node node = gridManager.GetNode(unit.transform.position);
                if (node != null)
                {
                    unit.transform.position = node.worldPosition;
                    node.character = unit;
                    unit.currentNode = node;
                    unit.currentNode.isWalkable = false;
                    unit.character.covered = unit.character.IsCovered(this, unit);
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
                Debug.Log("Path is not valid");
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
            ToggleReachablePathViz(currentGameState == GameState.Combat);   // We only want the path viz to be active during combat mode     
            // Ditto with the AP viz text
            if (currentGameState == GameState.Combat)
                gameVariables.UpdateMouseText(actionPointsViz.ToString());
            else
                gameVariables.UpdateMouseText("");
            character.LoadPath(pathActual);
        }

        public void ToggleReachablePathViz(bool toggle)
        {
            reachablePathViz.gameObject.SetActive(toggle);
            unReachablePathViz.gameObject.SetActive(toggle);
        }

        public void ClearPath(StateManager states)
        {
            reachablePathViz.positionCount = 0;
            unReachablePathViz.positionCount = 0;
            if (states.CurrentCharacter != null)
            {
                states.CurrentCharacter.currentPath = null;
            }
        }


        #endregion

        #region Tile Management
        List<Node> highlightedTiles;

        public List<Node> GetNeighborsManhattan(Node center, Node target)
        {
            List<Node> returnVal = new List<Node>();

            for (int x = -1; x <= 1; x++)
            {
                Node node = gridManager.GetNode(center.x + x, center.y, center.z);
                if (node != null)
                {
                    if (target == null)
                    {
                        if (node.isWalkable)
                        {
                            returnVal.Add(node);
                        }
                    }
                    else
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
                    if (target == null)
                    {
                        if (node.isWalkable)
                        {
                            returnVal.Add(node);
                        }
                    }
                    else
                    {
                        returnVal.Add(node);
                    }
                }
            }

            return returnVal;
        }

        public List<Node> GetNeighborsDiagonal(Node center, Node target)
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
                            if (target == null)
                            {
                                if (node.isWalkable)
                                {
                                    returnVal.Add(node);
                                }
                            }
                            else
                            {
                                returnVal.Add(node);
                            }
                        }
                    }

                }
            }
            return returnVal;
        }

        public void HighlightAroundCharacter(GridCharacter character, Node target, int radius)
        {
            // We only want the highlighting to happen when in combat mode. 
            if (currentGameState == GameState.Noncombat || currentGameState == GameState.Dialog)
                return;

            currentCharacter = character;
            Node centerNode = character.currentNode;

            List<Node> reachableNodes = new List<Node>();
            List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();

            int showRadius = character.ActionPoints;
            if (target != null)
            {
                showRadius = radius;
                centerNode = target;
            }
            reachableNodes.Add(centerNode);
            openSet.Add(centerNode);

            centerNode.Steps = 0;
            int steps = 0; 

            OnHighlightCharacter(centerNode);

            while (openSet.Count > 0) // for (int i = 0; i < c.actionPoints-1; i++) <- this also worked lol
            {
                Node currentNode = openSet[0];
                steps = currentNode.Steps;

                if(currentNode.Steps <= showRadius)
                {
                    foreach (Node node in GetNeighborsManhattan(currentNode, target))
                    {
                        if (!closedSet.Contains(node))
                        {
                            int newStepCost = steps + 1;

                            if (newStepCost <= showRadius)
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
                    foreach (Node node in GetNeighborsDiagonal(currentNode, target))
                    {
                        if (!closedSet.Contains(node))
                        {
                            int newStepCost = steps + 2;

                            if (newStepCost <= showRadius)
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
            reachableNodesAI = reachableNodes;
            UpdateListToReachableMaterial(reachableNodes, target);
        }

        void UpdateListToReachableMaterial(List<Node> list, Node target)
        {
            ClearReachableTiles();
            List<Node> coverNodes = new List<Node>();
            foreach (Node node in list)
            {
                if (node.tileRenderer != null)
                    if (target == null)
                    {
                        if (turns[TurnIndex].name == "EnemyTurn")
                            node.tileRenderer.material = reachableEnemyTileMaterial;
                        else
                            node.tileRenderer.material = reachableTileMaterial;
                    }
                    else
                    {
                        if (SpecialAbilityAction.buffAbilitySelected)
                            node.tileRenderer.material = buffAbilityTileMaterial;
                        else
                            node.tileRenderer.material = abilityTileMaterial;
                    }
                if (turns[TurnIndex].name == "EnemyTurn")
                    AIFindCoverNodes(node, coverNodes);
            }

            highlightedTiles = list;
            targetedNodes = highlightedTiles;
        }

        public void ClearReachableTiles()
        {
            if (highlightedTiles == null)
                return;
            foreach (Node node in highlightedTiles)
            {
                if (node.tileRenderer != null)
                    node.tileRenderer.material = defaultTileMaterial;
            }
            highlightedTiles.Clear();
        }

        public List<Node> GetTargetNodes()
        {
            return targetedNodes;
        }

        // For enemy AI - Reaching cover nodes
        void AIFindCoverNodes(Node node, List<Node> coverNodes)
        {
            List<Node> test = this.GetNeighborsManhattan(node, node);
            foreach (Node testNode in test)
            {
                if (!testNode.isWalkable && !testNode.character)
                {
                    if (node.tileRenderer != null)
                    {
                        node.tileRenderer.material = testCoverMaterial;
                        coverNodes.Add(node);
                    }
                }
            }
            AIController.coverNodes = coverNodes;
        }

        #endregion

        #region Turn Management
        // Only runs after Initialization is complete
        // Run turns[0].execute(Sessionmanager)
        //    - if true, increment turnindex (shouldn't yet, we only have 1 turn 3/24/21)

        bool isAttack;

        private void Update()
        {
            if (currentGameState == GameState.GameOver)
            {
                if (!gameOverScreenLoaded)
                    StartCoroutine("GameOverScreen");
            }
            else 
            {
                if (!isInit)
                    return;

                if (combatVictory)
                {
                    StartCoroutine(CombatVictory());
                    EndTurn();
                }

                delta = Time.deltaTime;

                if (turns[_turnIndex].Execute(this))
                {
                    if (currentGameState == GameState.Combat) // Cycle through turns if current gamestate is Combat Mode
                    {
                        combatVictory = false;
                        _turnIndex++;
                        if (_turnIndex > turns.Length - 1)
                        {
                            _turnIndex = ((int)PlayerTurn.Player);
                        }
                    }
                    else if (currentGameState == GameState.Noncombat) // Stay with player turn if gamestate is Noncombat
                    {
                        _turnIndex = ((int)PlayerTurn.Player);
                        Debug.Log("Non-combat Mode");
                    }

                }
            }
        }

        public void EndTurn()
        {
            if (currentGameState == GameState.Noncombat || moveInProgress)
                return;
            
            popUpUI.Deactivate(this);
                        
            // deHighlight the current player when the turn ends
            if (currentCharacter != null)
            {
                currentCharacter.OnDeHighlight(turns[TurnIndex].player, true);
                ClearReachableTiles();
                ClearPath(turns[TurnIndex].player.stateManager);
            }
            turns[_turnIndex].EndCurrentPhase();
        }

        public void APCheck() 
        {
            int APPool = 0;
            
            foreach (var character in turns[TurnIndex].player.characters)
            {
                APPool += character.ActionPoints;
            }
            
            if (APPool == 0)
            {
                EndTurn();
            }
        }
        #endregion 

        #region State Management

        IEnumerator CombatVictory()
        {
            combatVictory = false;
            Debug.Log("All enemies defeated!");
            if (!currentCharacter.character.teamLeader)
                currentCharacter.OnDeselect(currentCharacter.owner);
            if (currentLevel.hasPostDialogue)
                DialogueManager.StartConversation(currentLevel.postDialogueTitle, currentCharacter.transform, currentCharacter.transform);
            yield return new WaitForSeconds(2);
            NonCombatModeEnter();
        }

        public void StartingMode()
        {
            currentGameState = currentLevel.postPreDialogMode;
            if (currentGameState == GameState.Combat)
            {
                InitiateCombatMode();
            }
        }

        public void InitiateCombatMode()
        {
            uiCanvas.SetActive(true);
            foreach (GridCharacter character in currentCharacter.owner.characters)
            {
                character.ActionPoints = character.character.agility;
                if (character == character.character.teamLeader)
                    currentCharacter = character;
                character.ActionPoints = character.character.StartingAP;
                character.gameObject.SetActive(true);
                character.currentNode.inactiveCharWasHere = false;
                character.currentNode.isWalkable = false;
            }
            currentCharacter.OnSelect(turns[0].player);
            enemyGroup.SetActive(true);
            currentGameState = GameState.Combat;
            currentCharacter.isSelected = true;
            HighlightAroundCharacter(currentCharacter, null, 0);
        }

        public void NonCombatModeEnter()
        {
            currentGameState = GameState.Noncombat;
            // uiCanvas.SetActive(false);
            foreach (GridCharacter character in currentCharacter.owner.characters)
            {
                if (character.character.teamLeader)
                {
                    gameVariables.UpdateCharacterPortrait(character.character.characterPortrait);
                    gameVariables.UpdateAbilities(this);
                    character.OnSelect(currentCharacter.owner);
                    character.highlighter.SetActive(false);
                    currentCharacter = character;
                    currentCharacter.character.weaponSelected = 0;
                    currentCharacter.SetRun();
                    currentCharacter.PlayIdleAnimation();
                    currentCharacter.ActionPoints = currentCharacter.character.NonCombatAPCap;
                }
                else
                {
                    // Takes other players off map
                    // character.currentNode.isWalkable = true;
                    // character.currentNode.inactiveCharWasHere = true;
                    // character.gameObject.SetActive(false);
                }
            }
        }

        IEnumerator GameOverScreen()
        {
            gameOverScreenLoaded = true;
            yield return new WaitForSeconds(2f);
            Debug.Log("Game Over"); // game over dog
            SceneManager.LoadScene("GameOverScreen", LoadSceneMode.Additive);
        }

        #endregion

        #region Events
        public SO.IntVariable stanceInt;
        public SO.IntVariable attackType;
        public SO.IntVariable specialAbilitySelect;
        public SO.BoolVariable powerActivated;

        public void SetWeaponForCurrentPlayer()
        {
            if (currentGameState != GameState.Combat)
                return;
            
            popUpUI.Deactivate(this);
            switch(attackType.value)
            {
                case 0:
                    currentCharacter.character.weaponSelected = 0;
                    currentCharacter.PlayIdleAnimation();
                    break;
                case 1:
                    currentCharacter.character.weaponSelected = 1;
                    currentCharacter.PlaySelectMeleeWeapon();
                    break;
            }
        }

        public void SetStanceForCurrentPlayer()
        {
            if (currentGameState != GameState.Combat)
                return;
            popUpUI.Deactivate(this);
            switch(stanceInt.value)
            {
                case 0:
                    currentCharacter.ResetStance();
                    break;
                
                case 1:
                    SetAction("MoveAction");
                    if (currentCharacter.ActionPoints >= 2)
                    {
                        currentCharacter.SetBrace();
                        currentCharacter.ActionPoints = 0;
                        ClearReachableTiles();
                        HighlightAroundCharacter(currentCharacter, null, 0);
                    }
                    else {
                        Debug.Log("Not enough AP to brace");
                    }

                    if (currentCharacter.ActionPoints == 0)
                    {
                        APCheck();
                    }
                    break;
            }
        }

        public void SpecialAbility()
        {
            if (currentGameState != GameState.Combat || moveInProgress)
                return;
            currentCharacter.character.abilitySelected = specialAbilitySelect.value;
            SetAction("SpecialAbilityAction");
        }

        public void PowerActivated()
        {
            popUpUI.PowerActivate(this, powerActivated.value);
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
            if (isInit)
                return;
            gameActions.Add("MoveAction", new MoveAction());
            gameActions.Add("AttackAction", new AttackAction());
            gameActions.Add("SpecialAbilityAction", new SpecialAbilityAction()); // added 5/30/21
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

