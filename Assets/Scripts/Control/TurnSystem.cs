using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class TurnSystem : MonoBehaviour
{
    public GameState currentState;

    public List<PlayerObject> _playerGroup;
    public List<PlayerObject> _enemyGroup;
    public GameObject playerController;

    public TextMeshProUGUI _turnText;
    public TextMeshProUGUI _selectedPlayerText;

    private GameObject _mainCamera;
    private Vector3 _cameraOffset = new Vector3(0, 7, 7);

    bool _playerTurn = true;
    bool _turnOver;
    bool _cameraOverrideFlag = false;
    bool _autoActive;
    int _activePlayer;

    // Start is called before the first frame update
    void Start()
    {
        currentState = GameState.PlayerMove;
        _mainCamera = GameObject.Find("Main Camera");
        ResetTurns(_playerGroup);
    }

    private void OnEnable()
    {
        EventManager.onButtonClick += SelectLocation;
        EventManager.midMouseButtonHold += CameraOverride;
    }

    private void OnDisable()
    {
        EventManager.onButtonClick -= SelectLocation;
        EventManager.midMouseButtonHold -= CameraOverride;
    }

    void CameraOverride()
    {
        _cameraOverrideFlag = true;
    }

    // Update is called once per frame
    void Update()
    {   
        switch(currentState)
        {
            case (GameState.PlayerMove):
                if (_playerTurn)
                {
                    UpdateTurns(_playerGroup, _playerTurn);
                }
                else
                {
                    UpdateTurns(_enemyGroup, _playerTurn);
                }

                break;
            case (GameState.PlayerAttack):
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    currentState = GameState.PlayerMove;
                    Debug.Log("PlayerMove State");
                }
                break;
        }
    }

    // Resets all the moves and sets the default first move to player 1
    void ResetTurns(List<PlayerObject> currentGroup) 
    {   
        foreach (PlayerObject character in currentGroup) {
            character.isTurn = false;
            character.moveComplete = false;
            character.ActionPoints = 4;
        }
        currentGroup[0].isTurn = true;            
    }

    void UpdateTurns(List<PlayerObject> currentGroup, bool isPlayerTurn) 
    {
        _turnText.gameObject.SetActive(!_playerTurn);                 // display "enemy turn" only if it's the enemy's turn
        Transform currentPlayerTransform;

        switch(isPlayerTurn) 
        {
            case true:
                // instructions for player movement here.
                _turnOver = true;                                       // set a flag for all turns over to true. If still true at end of loop, player's turn is over
                _autoActive = true;                                     // flag for auto-setting the next player move. prevents player getting 'stuck' at selected if player tabs
                _activePlayer = FindActivePlayer(currentGroup);         // finds the active player & determines if the turn is over (all moves completed)
                currentPlayerTransform = _playerGroup[_activePlayer].playerGameObject.transform;    // stores the active player's Transform

                // These next 3 lines could probably be ported over to the PlayerController.
                // - This code is more with the player object and not managing anything turn-based.
                DisplayName(currentPlayerTransform.GetChild(0).name);
                CalculateDistance();
                FollowCamera(currentPlayerTransform.position);

                if (_autoActive)                                         // however, if there's no active player and the turn isn't over:
                {   
                    _activePlayer = AutoPlayerSelect(currentGroup);      // .. auto select an available player
                }

                if (Input.GetKeyDown(KeyCode.Tab))                       // switch the active player when keycode tab is pressed
                {
                    TabNextPlayer(currentGroup);
                }

                if (_turnOver) {                                         // if all moves have been used, then switch control back to enemy
                    _playerTurn = !_playerTurn;                          //   player turn is over
                    ResetTurns(_enemyGroup);                             //   switch control back to the enemy
                }
                break;

            // Enemies can move sequentially for all i care ;)
            case false:
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
                        _playerTurn = !_playerTurn;
                        ResetTurns(_playerGroup);
                    }
                }
                break;
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
        if (_playerTurn && currentState == GameState.PlayerMove) 
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

    private void CalculateDistance()
    {
        float moveDistance = Vector3.Distance(_playerGroup[_activePlayer].playerGameObject.transform.position, CursorController.cursorPosition);
        float playerMoveLeft = _playerGroup[_activePlayer].MoveDist;
        float actions = _playerGroup[_activePlayer].ActionPoints / 2;

        if (moveDistance > (playerMoveLeft * actions))
        {
            CursorController.cursorColor = Color.red;
        }
        else if (moveDistance > playerMoveLeft || actions == 1)
        {
            CursorController.cursorColor = Color.yellow;
        }
        else
        {
            CursorController.cursorColor = Color.blue;
        }
    }

    private void SelectLocation()
    {
        switch(currentState)
        {
            case (GameState.PlayerMove):
                bool occupiedPlayer = false;
                bool occupiedEnemy = false;

                if (_playerTurn)
                {
                    occupiedPlayer = SpaceIsOccupied(_playerGroup);
                    occupiedEnemy = SpaceIsOccupied(_enemyGroup);
                    if (occupiedEnemy) currentState = (GameState.PlayerAttack);
                    if (occupiedPlayer == true || occupiedEnemy == true)
                    {
                        Debug.Log("Space is occupied");
                    }
                    else
                    {   if (CursorController.cursorColor != Color.red)
                        {
                            _playerGroup[_activePlayer].playerGameObject.GetComponent<NavMeshAgent>().SetDestination(CursorController.cursorPosition);
                            _playerGroup[_activePlayer].ActionPoints--;
                            if (CursorController.cursorColor == Color.yellow) _playerGroup[_activePlayer].ActionPoints--; // decrease action points by 2 if player dashes

                            if (_playerGroup[_activePlayer].ActionPoints == 0)
                            {
                                Debug.Log("action points spent");
                                _playerGroup[_activePlayer].isTurn = false;
                                _playerGroup[_activePlayer].moveComplete = true;
                            }
                        } else
                        {
                            Debug.Log("Out of movement range");
                        }
                    }
                }
                break;

            case (GameState.PlayerAttack):
                AttackEnemy();
                break;

        }
    }

    public void AttackEnemy()
    {
        currentState = GameState.PlayerAttack;
        Transform player = _playerGroup[_activePlayer].playerGameObject.transform;
        int targetedEnemyIndex = 0;
        bool enemyClicked = false;

        for (int i = 0; i < _enemyGroup.Count; i++)
        {
            if (SnapMovement.Snap(_enemyGroup[i].playerGameObject.transform.position) == CursorController.cursorPosition)
            {
                targetedEnemyIndex = i;
                enemyClicked = true;
                break;
            }
        }

        Transform enemy = _enemyGroup[targetedEnemyIndex].playerGameObject.transform;
        string enemyName = _enemyGroup[targetedEnemyIndex].playerGameObject.name;

        if (enemyClicked) // if enemy was clicked, check if it's in range
        {
            if (_playerGroup[_activePlayer].AttackRange > Vector3.Distance(player.position, enemy.position))
            {
                Debug.Log("Target " + enemyName + " in range!");
                player.LookAt(enemy);
            }
            else
            {
                Debug.Log("Target " + enemyName + " is out of range.");
            }
        }

        if (!enemyClicked) // if the attack button was pressed instead, find the closest enemy if in range
        {
            float closestEnemyDistance = _playerGroup[_activePlayer].AttackRange;
            int closestEnemyIndex = 0;
            bool enemyInRangeFound = false;

            // find if there's an enemy in range
            for (int i = 0; i < _enemyGroup.Count; i++)
            {
                if (Vector3.Distance(player.position, _enemyGroup[i].playerGameObject.transform.position) <= closestEnemyDistance)
                {
                    closestEnemyIndex = i;
                    closestEnemyDistance = Vector3.Distance(player.position, _enemyGroup[i].playerGameObject.transform.position);
                    enemyInRangeFound = true;
                    Debug.Log(closestEnemyDistance);
                }
            }

            // target closest enemy, or tell player no enemies in range.
            if (enemyInRangeFound)
            {
                Debug.Log("Target: " + _enemyGroup[closestEnemyIndex].playerGameObject.name + " in range!");
                player.LookAt(_enemyGroup[closestEnemyIndex].playerGameObject.transform);
            }
            else
            {
                Debug.Log("No enemies in range");
                currentState = GameState.PlayerMove;
            }
        }
    }

    bool SpaceIsOccupied(List<PlayerObject> players)
    {
        bool isOccupied = false;
        foreach (var player in players)
        {
            if (CursorController.cursorPosition == SnapMovement.Snap(player.playerGameObject.transform.position))
            {
                isOccupied = true;
            }
        }
        return isOccupied;
    }



    void TabNextPlayer(List<PlayerObject> currentGroup) 
    {
        int movesLeft = 0;
        int lastPlayer = _activePlayer;
        _cameraOverrideFlag = false;

        currentGroup[_activePlayer].isTurn = false;          // remove current player turn
        if (_activePlayer != (currentGroup.Count - 1) && !currentGroup[_activePlayer + 1].moveComplete)
        {
            currentGroup[_activePlayer + 1].isTurn = true;   // if not at the end of the list and next player's move isn't complete, set turn to next player
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

    void DisplayName(string nameToDisplay)
    {
        _selectedPlayerText.text = nameToDisplay;
    }
}