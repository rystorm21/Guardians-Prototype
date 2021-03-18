using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;

public class TurnSystem : MonoBehaviour
{
    public GameState _currentState;

    public List<PlayerObject> _playerGroup;
    public List<PlayerObject> _enemyGroup;
    public GameObject _moveTile;
    public GameObject _moveTileSprint;

    public TextMeshProUGUI _turnText;
    public TextMeshProUGUI _selectedPlayerText;
    public TextMeshProUGUI _gameStateText;
    public TextMeshProUGUI _attackTargetText;
    public GameObject _attackConfirmationMenu;
    public Sprite[] _characterPortraits;

    private GameObject _mainCamera;
    private Vector3 _cameraOffset = new Vector3(0, 7, 7);
    private Image _charPortrait;

    bool _playerMoveStart;
    bool _waiting;
    bool _turnOver;
    bool _cameraOverrideFlag = false;
    bool _autoActive;
    bool _sprintModeActive;
    int _activePlayerIndex;
    int _lastPlayerIndex;

    public GameState GameState
    {
        get { return _currentState; }
        set { _currentState = value; }
    }

    public List<PlayerObject> PlayerGroup
    {
        get { return _playerGroup; }
    }

    public List<PlayerObject> EnemyGroup
    {
        get { return _enemyGroup; }
    }

    public int ActivePlayerIndex
    {
        get { return _activePlayerIndex; }
    }

    public PlayerObject ActivePlayer
    {
        get { return _playerGroup[_activePlayerIndex]; }
    }

    public bool Waiting
    {
        get { return _waiting; }
        set { _waiting = value; }
    }

    private void OnEnable()
    {
        EventManager.midMouseButtonHold += CameraOverride;
    }

    private void OnDisable()
    {
        EventManager.midMouseButtonHold -= CameraOverride;
    }


    // Start is called before the first frame update
    void Start()
    {
        _currentState = GameState.PlayerMove;
        _mainCamera = GameObject.Find("Main Camera");
        _attackConfirmationMenu = GameObject.Find("AttackUI");
        _attackTargetText = GameObject.Find("TMP.AttackTarget").GetComponent<TextMeshProUGUI>();
        _attackConfirmationMenu.SetActive(false);
        _charPortrait = GameObject.Find("CharacterPortrait").GetComponent<Image>();
        ResetTurns(_playerGroup);

        _playerMoveStart = false;
    }
    // Update is called once per frame
    void Update()
    {   
        switch(GameState)
        {
            case (GameState.PlayerMove):
                _gameStateText.text = "Game State: PlayerMove";
                UpdateTurns(_playerGroup);
                break;
            case (GameState.PlayerAttack):
                _gameStateText.text = "Game State: Attack";
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    GameState = GameState.PlayerMove;
                    _attackConfirmationMenu.SetActive(false);
                }
                break;
            case (GameState.EnemyTurn):
                _gameStateText.text = "Game State: Enemy Turn";
                UpdateTurns(_enemyGroup);
            break;
        }
    }

    // Resets all the moves and sets the default first move to player 1
    void ResetTurns(List<PlayerObject> currentGroup) 
    {   
        foreach (PlayerObject character in currentGroup) {
            character.isTurn = false;
            character.moveComplete = false;
            character.ActionPoints = 2;
            character.playerGameObject.GetComponent<NavMeshAgent>().enabled = false;
            _playerMoveStart = false;
        }
        currentGroup[0].isTurn = true;
        currentGroup[0].playerGameObject.GetComponent<NavMeshAgent>().enabled = true;
        if (_currentState == GameState.EnemyTurn) { EraseGrid(); }            
    }

    void UpdateTurns(List<PlayerObject> currentGroup) 
    {
        _turnText.gameObject.SetActive(GameState == GameState.EnemyTurn);                 // display "enemy turn" only if it's the enemy's turn
        Transform currentPlayerTransform;

        switch(GameState) 
        {
            case GameState.PlayerMove:
                // instructions for player movement here.
                _turnOver = true;                                       // set a flag for all turns over to true. If still true at end of loop, player's turn is over
                _autoActive = true;                                     // flag for auto-setting the next player move. prevents player getting 'stuck' at selected if player tabs
                int lastplayer = _activePlayerIndex;

                if (PlayerController.IsMoving)
                {
                    _playerMoveStart = PlayerController.IsMoving;
                }
    
                _activePlayerIndex = FindActivePlayer(currentGroup);    // finds the active player & determines if the turn is over (all moves completed)
                currentPlayerTransform = _playerGroup[_activePlayerIndex].playerGameObject.transform;    // stores the active player's Transform

                DisplayInfo(currentPlayerTransform.GetChild(0).name, ActivePlayerIndex);
                CalculateDistance();
                FollowCamera(currentPlayerTransform.position);
                if (!PlayerController.NoDraw) { MoveGrid(_playerGroup[ActivePlayerIndex].playerGameObject.transform.position); }

                if (_autoActive)                                         // however, if there's no active player and the turn isn't over:
                {   
                   _activePlayerIndex = AutoPlayerSelect(currentGroup);      // .. auto select an available player
                }

                if (Input.GetKeyDown(KeyCode.Tab) && !PlayerController.IsMoving)                       // switch the active player when keycode tab is pressed
                {
                    TabNextPlayer(currentGroup);
                }

                if (lastplayer != _activePlayerIndex) { SetNavMeshAgent(_playerGroup);}

                if (_turnOver) {                                         // if all moves have been used, then switch control back to enemy
                    GameState = GameState.EnemyTurn;
                    ResetTurns(_enemyGroup);                             //   switch control back to the enemy
                }

                // allowable move grid functionality, so no duplicates
                if (_lastPlayerIndex == _activePlayerIndex) {
                    Waiting = true;
                }
                else { 
                    Waiting = false; 
                    _lastPlayerIndex = _activePlayerIndex;
                }

                if (PlayerController.IsMoving) { EraseGrid(); }
                // redraw the grid if a player that started moving ended their move
                if (!PlayerController.IsMoving && _playerMoveStart == true)
                {
                    _playerMoveStart = false;
                    Waiting = false;
                }
                break;

            // Enemies can move sequentially for all i care ;)
            case GameState.EnemyTurn:
                GameState = GameState.EnemyTurn;
                _cameraOverrideFlag = false;
                for (int i = 0; i < currentGroup.Count; i++)
                {
                    if (!currentGroup[i].moveComplete)
                    {

                        _selectedPlayerText.text = _enemyGroup[i].playerGameObject.name;
                        currentGroup[i].isTurn = true;
                        FollowCamera(currentGroup[i].playerGameObject.transform.position);
                        break;
                    }
                    else if (i == (currentGroup.Count - 1) && currentGroup[i].moveComplete)
                    {
                        GameState = GameState.PlayerMove;
                        ResetTurns(_playerGroup);
                    }
                }
                break;
        }
    }

    void SetNavMeshAgent(List<PlayerObject> currentGroup)
    {
        for (int i = 0; i < currentGroup.Count; i++)
        {
            if (currentGroup[i].isTurn) { currentGroup[i].playerGameObject.GetComponent<NavMeshAgent>().enabled = true;}
            else { currentGroup[i].playerGameObject.GetComponent<NavMeshAgent>().enabled = false; }
        }
    }

    int FindActivePlayer(List<PlayerObject> currentGroup)
    {
        int active = 0;

        for (int i = 0; i < currentGroup.Count; i++)
        {                                                    // search the entire list to find.... 
            if (currentGroup[i].isTurn)
            {                                                // if it's a curren't player's turn to move
                active = i;                                  //   set the active player to player whose turn it is
                _autoActive = false;                         //   no need to automatically select a player if one's already active
            }
            if (!currentGroup[i].moveComplete)
            {                                                // if any player's move isn't completed yet
                _turnOver = false;                           //   then the player turn isn't over
            }
        }
        return active;
    }

    public void EndTurn() 
    {
        if (GameState == GameState.PlayerMove) 
        {
            // Ask to confirm end turn

            foreach (PlayerObject character in _playerGroup)
            {
                character.isTurn = false;
                character.moveComplete = true;
                character.ActionPoints = 0;
            }   
        }
    }
    #region Show Allowable Move Grid
    // Displays a grid of 'allowable moves', depending on user's action points and movement rate
    public void MoveGrid(Vector3 drawPosition)
    {
        PlayerObject currentPlayer = _playerGroup[_activePlayerIndex];
        float moveRadius;
        if (!Waiting && (_currentState == GameState.PlayerMove)) 
        {
            moveRadius = _playerGroup[_activePlayerIndex].MoveDist;
            EraseGrid();
            DrawGrid(drawPosition, moveRadius, false, currentPlayer.ActionPoints);
            Waiting = true;
            _sprintModeActive = false;
        }
        if (Waiting && !_sprintModeActive) 
        {
            moveRadius = _playerGroup[_activePlayerIndex].MoveDist;
            if (Vector3.Distance(CursorController.cursorPosition, drawPosition) > moveRadius && currentPlayer.ActionPoints > 1)
            {
                DrawGrid(drawPosition, moveRadius, true, currentPlayer.ActionPoints);
                _sprintModeActive = true;
            }
        }   
    }

    // Clear the grid after a move, or when players are switched
    void EraseGrid()
    {
        GameObject[] oldGrid = GameObject.FindGameObjectsWithTag("PlayerHighlight");
        if (oldGrid != null) 
        {
            foreach (GameObject oldHighlight in oldGrid)
            {
                Destroy(oldHighlight);
            }
        }
    }

    // Draw the movement grid
    void DrawGrid (Vector3 playerPosition, float moveRadius, bool sprinting, int actionPoints)
    {
        float basePlayerMove = _playerGroup[_activePlayerIndex].MoveDist;

        if (sprinting) { EraseGrid(); moveRadius *= actionPoints; }
        for (int z = -(int)moveRadius + 1; z < moveRadius; z++)
        {
            for (int x = -(int)moveRadius + 1; x < moveRadius; x++)
            {
                Vector3 location = playerPosition + (Vector3.right * x) + (Vector3.forward * z);
                float distance = Vector3.Distance(playerPosition, location);
                if (distance < moveRadius)
                {
                    if (distance < basePlayerMove) {
                        Instantiate(_moveTile, playerPosition + (Vector3.right * x) + (Vector3.forward * z), Quaternion.identity);
                    }
                    else {
                        Instantiate(_moveTileSprint, playerPosition + (Vector3.right * x) + (Vector3.forward * z), Quaternion.identity);
                    }
                }
            }
        }
    }
    #endregion

    private void CalculateDistance()
    {
        Vector3 playerPosition = _playerGroup[_activePlayerIndex].playerGameObject.transform.position;
        float moveDistance = Vector3.Distance(playerPosition, CursorController.cursorPosition);
        float playerMoveRate = _playerGroup[_activePlayerIndex].MoveDist;
        float turnCost = Mathf.Ceil(moveDistance / playerMoveRate);
        float actions = _playerGroup[_activePlayerIndex].ActionPoints;

        if (turnCost > actions)
        {
            CursorController.cursorColor = Color.red;
            CursorController.moveCostText = "";
        }
        else if (turnCost > 1)
        {
            CursorController.cursorColor = Color.yellow;
            CursorController.moveCostText = turnCost.ToString();
        }
        else
        {
            CursorController.cursorColor = Color.blue;
            CursorController.moveCostText = turnCost.ToString();
        }
    }

    void TabNextPlayer(List<PlayerObject> currentGroup) 
    {
        int movesLeft = 0;
        int lastPlayer = _activePlayerIndex;
        _cameraOverrideFlag = false;

        currentGroup[_activePlayerIndex].isTurn = false;          // remove current player turn
        if (_activePlayerIndex != (currentGroup.Count - 1) && !currentGroup[_activePlayerIndex + 1].moveComplete)
        {
            currentGroup[_activePlayerIndex + 1].isTurn = true;   // if not at the end of the list and next player's move isn't complete, set turn to next player
        }
        else
        {
            foreach (PlayerObject player in currentGroup)
            {    // search the list for remaining moves left
                if (!player.moveComplete) { movesLeft++; }
            }
            for (int i = 0; i < currentGroup.Count; i++)
            {    // Start the search from zero and set
                if (movesLeft == 1)
                {                                               // if player is down to their last turn:
                    if (!currentGroup[i].moveComplete)
                    {                                           // look for a player without a completed move
                        currentGroup[i].isTurn = true;          // set the first player without a move to active player
                        break;                                  // break out of the loop to prevent setting multiple player turns
                    }
                }
                else
                {
                    if (i != lastPlayer && !currentGroup[i].moveComplete) // search until it finds another player with a move left, but is not the same player as currently active
                    {
                        currentGroup[i].isTurn = true;
                        break;
                    }
                }
            }
        }
    }

    // Change control to clicked player, if it has moves remaining
    public void SwitchControl(PlayerObject selectedPlayer)
    {
        for (int i=0; i < _playerGroup.Count; i++) 
        {
            if (_playerGroup[i] == selectedPlayer) 
            {
                _playerGroup[ActivePlayerIndex].isTurn = false;
                _playerGroup[i].isTurn = true;
            }
        }
    }

    // Searches the player list and sets next player without a move to the current selection
    int AutoPlayerSelect(List<PlayerObject> group)
    {
        _cameraOverrideFlag = false;
        int active = 0;
        foreach (PlayerObject player in group)
        {
            if (!player.moveComplete) { group[active].isTurn = true; break; }
            else { active++; }
        }
        _autoActive = false;
        return active;
    }

    void FollowCamera(Vector3 focus)
    {
        if (!_cameraOverrideFlag) 
        {
            float cameraSpeed = 5.0f;
            _mainCamera.transform.position = Vector3.Lerp(_mainCamera.transform.position, focus + _cameraOffset, cameraSpeed * Time.deltaTime);
        }
    }

    void DisplayInfo(string nameToDisplay, int activePlayer)
    {
        _selectedPlayerText.text = nameToDisplay;
        _charPortrait.sprite = _characterPortraits[activePlayer];
    }

    public void CancelAttack()
    {
        _attackConfirmationMenu.SetActive(false);
        _currentState = GameState.PlayerMove;
    }

    void CameraOverride()
    {
        _cameraOverrideFlag = true;
    }
}