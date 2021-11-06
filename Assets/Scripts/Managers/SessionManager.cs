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
        public static int pathCount;
        public static bool combatVictory;
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
        public GameObject moveButton;
        
        public bool isInit;
        public float delta;

        public PopUpUI popUpUI;
        public GameObject uiCanvas;
        public GameObject uiAbilityBar;
        public GameObject uiEnemyBar;
        public VariablesHolder gameVariables;
        public LineRenderer reachablePathViz;
        bool isPathfinding;

        public Material defaultTileMaterial;
        public Material reachableTileMaterial;
        public Material reachableEnemyTileMaterial;
        public Material abilityTileMaterial;
        public Material buffAbilityTileMaterial;
        public Material testCoverMaterial;
        public Material lowCoverMaterial;
        public Material highCoverMaterial;
        public List<Node> reachableNodesAI;
        public GameObject sceneHolder;
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
        private void Awake()
        {
            sceneHolder = GameObject.Find("CurrentSceneHolder");
            currentLevel = sceneHolder.GetComponent<SceneHolder>().currentLevel;
            DontDestroyOnLoad (sceneHolder.transform.gameObject);
        }
        private void Start()
        {
            popUpUI = GameObject.Find("PopUpUI").GetComponent<PopUpUI>();

            uiCanvas = GameObject.Find("UI Canvas");
            moveButton = GameObject.Find("Button-Move");
            uiAbilityBar = uiCanvas.transform.GetChild(0).gameObject;
            uiEnemyBar = uiCanvas.transform.GetChild(1).gameObject;
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
            moveButton.SetActive(false);
            uiCanvas.SetActive(false);
        }

        // Create an array of GridCharacters (all objects with the "GridCharacter script), ie Player Characters")
        // Initialize each unit (register character to PlayerHolder)
        //   Get the node from the unit's current coordinates
        //      set the unit's position to the node's position, node's character attribute is set to this unit, unit's current node is this node.
        public void PlaceUnits()
        {
            GridCharacter[] units = GameObject.FindObjectsOfType<GridCharacter>();
            Debug.Log("PlaceUnits: " + units.Length);
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
        public void InitStateManagers()
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
            Vector3 offset = Vector3.up * .1f;

            int actionPoints = character.ActionPoints;
            int actionPointsViz = 0;

            if (actionPoints > 0)
            {
                reachablePositions.Add(character.currentNode.worldPosition + offset);
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
            }

            if (!AIController.aiActive)
            {
                reachablePathViz.positionCount = pathActual.Count + 1;
                reachablePathViz.SetPositions(reachablePositions.ToArray());
            }
   
            ToggleReachablePathViz(currentGameState == GameState.Combat);   // We only want the path viz to be active during combat mode     
            // Ditto with the AP viz text
            
            if (currentGameState == GameState.Combat)
            {
                if (actionPointsViz <= currentCharacter.ActionPoints)
                    gameVariables.UpdateMouseText(actionPointsViz.ToString());
                else
                    gameVariables.UpdateMouseText(currentCharacter.ActionPoints.ToString());
            }
            else
                gameVariables.UpdateMouseText("");

            if (pathActual.Count != 0)
            {
                ShowCoverIcon(pathActual[pathActual.Count-1]);            
            }
            character.LoadPath(pathActual);
        }

        void ShowCoverIcon(Node currentNode)
        {
            int coverDetect = currentCharacter.character.NodeCovered(this, currentNode);
            ClearReachableTiles();
            HighlightAroundCharacter(this.currentCharacter, null, 0);
            if (coverDetect != 0)
            {
                if (coverDetect == 1 && currentNode.tileRenderer != null)
                {
                    currentNode.tileRenderer.material = lowCoverMaterial;
                }
                if (coverDetect == 2 && currentNode.tileRenderer != null)
                {
                    currentNode.tileRenderer.material = highCoverMaterial;
                }
            }
        }

        public void ToggleReachablePathViz(bool toggle)
        {
            reachablePathViz.gameObject.SetActive(toggle);
        }

        public void ClearPath(StateManager states)
        {
            reachablePathViz.positionCount = 0;
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
            if (target != null)
            {
                ShowAccuracySpecialAbility(reachableNodes);
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

        // Shows accuracy if Area of effect ability is hovered over an enemy
        void ShowAccuracySpecialAbility(List<Node> reachableNodes)
        {
            foreach (Node node in reachableNodes)
            {
                if (node.character)
                {
                    if (currentCharacter.teamName != node.character.teamName)
                    {
                        if (currentCharacter.character.abilityInUse.ignoreCover)
                            AttackAction.GetAttackAccuracy(currentCharacter, node.character, true);
                        else
                            AttackAction.GetAttackAccuracy(currentCharacter, node.character, false);
                    }
                }
            }
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

                if (Input.GetKeyDown(KeyCode.T))
                {
                    Turn[] turns = this.turns;
                    List<GridCharacter> players = turns[0].player.characters;
                    Debug.Log(players.Count);
                    for (int i = players.Count - 1; i >= 0; i--)
                    {
                        turns[0].player.UnRegisterCharacter(players[i]);
                    }
                    DialogueManager.StopConversation();
                    SceneManager.UnloadSceneAsync(currentLevel.thisLevel);
                    SceneManager.LoadSceneAsync(currentLevel.nextScene, LoadSceneMode.Additive);
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
            
            AIController.aiActive = false;
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
            if (!currentCharacter.character.teamLeader)
                currentCharacter.OnDeselect(currentCharacter.owner);
            if (currentLevel.hasPostDialogue)
            {
                uiCanvas.SetActive(false);
                DialogueManager.StartConversation(currentLevel.postDialogueTitle, currentCharacter.transform, currentCharacter.transform);
            }
            else
            {   
                StartCoroutine(NextLevel());
            }
            yield return new WaitForSeconds(2);
            NonCombatModeEnter();
        }

        IEnumerator NextLevel()
        {
            yield return new WaitForSeconds(1);
            Turn[] turns = this.turns;
            List<GridCharacter> players = turns[0].player.characters;
            Debug.Log(players.Count);
            for (int i = players.Count - 1; i >= 0; i--)
            {
                turns[0].player.UnRegisterCharacter(players[i]);
            }
            SceneManager.LoadScene(currentLevel.nextScene, LoadSceneMode.Single);
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
        public SO.BoolVariable powerActivated;
        public SO.BoolVariable moveButtonClicked;

        public void MoveButtonClicked()
        {
            Debug.Log("move pressed");
            StateManager states = turns[0].player.stateManager;
            gameVariables.UpdateMouseText("");
            states.SetState("moveOnPath");
        }

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
                        moveButton.SetActive(false);
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
            gameActions.Add("PlayerAttackAction", new PlayerAttackAction()); // added 9/2/21
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

        #region UIControl
        public void TogglePanels(bool toggle)
        {
            GridCharacter[] units = GameObject.FindObjectsOfType<GridCharacter>();
            foreach (GridCharacter unit in units)
            {
                unit.uiStatusPanel.SetActive(toggle);
                if (!toggle)
                    unit.highlighter.SetActive(toggle);
                else
                    if (unit == currentCharacter)
                        unit.highlighter.SetActive(toggle);
            }
        }

        public void ResetAbilityEnemyUI()
        {
            uiAbilityBar.SetActive(false);
            uiAbilityBar.SetActive(true);
            uiEnemyBar.SetActive(false);
            uiEnemyBar.SetActive(true);
            GridCharacter[] units = GameObject.FindObjectsOfType<GridCharacter>();
            foreach (GridCharacter unit in units)
            {
                unit.uiStatusBar.SetActive(false);
                unit.uiStatusBar.SetActive(true);
            }
        }

        // public void MoveButtonPressed()
        // {
        //     StateManager states = turns[0].player.stateManager;
        //     gameVariables.UpdateMouseText("");
        //     states.SetState("moveOnPath");
        // }

        public void PopupUIExit()
        {
            SpecialAbilityAction.timeToVerify = false;
            currentCharacter.character.abilitySelected = 0;
            popUpUI.Deactivate(this);
            ResetAbilityEnemyUI();
        }
        #endregion
    }
}

