using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class PlayerController : MonoBehaviour
{
    private List<PlayerObject> _playerGroup;
    private List<PlayerObject> _enemyGroup;
    private TurnSystem _turnSystem;
    public PlayerObject _currentPlayer;
    private bool isTurn;
    GameState _currentState;

    // Start is called before the first frame update
    void Start()
    {
        _turnSystem = GameObject.Find("Turn-basedSystem").GetComponent<TurnSystem>();
        InitializePlayers();
    }

    void InitializePlayers()
    {
        _playerGroup = _turnSystem.PlayerGroup;
        _enemyGroup = _turnSystem.EnemyGroup;
        foreach (PlayerObject player in _turnSystem._playerGroup)
        {
            if (player.playerGameObject.name == gameObject.name) _currentPlayer = player;
        }
    }

    private void OnEnable() {
        EventManager.onButtonClick += SelectLocation;
    }

    private void OnDisable() {
        EventManager.onButtonClick -= SelectLocation;
    }

    private void SelectLocation()
    {
        _currentState = _turnSystem.GameState;

        switch (_currentState)
        {
            case (GameState.PlayerMove):
                PlayerObject occupiedPlayer = null;
                PlayerObject occupiedEnemy = null;
                isTurn = _currentPlayer.isTurn;

                if (isTurn)
                {
                    occupiedPlayer = SpaceIsOccupied(_playerGroup, true);
                    occupiedEnemy = SpaceIsOccupied(_enemyGroup, false);
                    int activePlayer = _turnSystem.ActivePlayerIndex;
                    // Debug.Log(activePlayer); this is triggering properly with player 1

                    if (occupiedEnemy != null){ AttackEnemy();} 
                    if (occupiedPlayer != null || occupiedEnemy != null)
                    {
                        if (occupiedPlayer != null) 
                        {
                            if (occupiedPlayer.moveComplete == false) 
                            {
                                // switch control to this player
                                _turnSystem.SwitchControl(occupiedPlayer);
                            }
                            else {
                                Debug.Log("Player's action points spent");
                            }
                        }
                    }
                    else
                    {
                        if (CursorController.cursorColor != Color.red)
                        {
                            MovePlayer(activePlayer);
                        }
                        else
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

    void MovePlayer(int activePlayer)
    {
        PlayerObject player = _playerGroup[activePlayer];
        player.playerGameObject.GetComponent<NavMeshAgent>().SetDestination(CursorController.cursorPosition);
        player.ActionPoints--;

        if (CursorController.cursorColor == Color.yellow) player.ActionPoints = 0; // yellow cursor = final move or dash distance that uses all remaining moves

        if (player.ActionPoints <= 0)
        {
            Debug.Log("action points spent");
            player.isTurn = false;
            player.moveComplete = true;
        }
    }

    public void AttackEnemy()
    {
        if (_turnSystem.GameState != GameState.EnemyTurn) 
        {
            _turnSystem.GameState = GameState.PlayerAttack;
            int activePlayer = _turnSystem.ActivePlayerIndex;
            PlayerObject player = _playerGroup[activePlayer];
            Transform playerTransform = _playerGroup[activePlayer].playerGameObject.transform;
            Vector3 enemyPosition;
            int targetedEnemyIndex = 0;
            bool enemyClicked = false;

            for (int i = 0; i < _enemyGroup.Count; i++)
            {
                enemyPosition = _enemyGroup[i].playerGameObject.transform.position;
                if (SnapMovement.Snap(enemyPosition) == CursorController.cursorPosition)
                {
                    targetedEnemyIndex = i;
                    enemyClicked = true;
                }
            }

            Transform enemyTransform = _enemyGroup[targetedEnemyIndex].playerGameObject.transform;
            string enemyName = _enemyGroup[targetedEnemyIndex].playerGameObject.name;

            if (enemyClicked) // if enemy was clicked, check if it's in range
            {
                TargetEnemyClicked(player, playerTransform, targetedEnemyIndex);
            }

            if (!enemyClicked) // if the attack button was pressed instead, find the closest enemy if in range
            {
                TargetClosestEnemy(player, playerTransform);
            }
        }
    }

    void TargetEnemyClicked(PlayerObject player, Transform playerTransform, int targetedEnemyIndex)
    {
        PlayerObject enemyTarget = _enemyGroup[targetedEnemyIndex];
        Transform enemyTransform = enemyTarget.playerGameObject.transform;
        string enemyName = _enemyGroup[targetedEnemyIndex].playerGameObject.name;

        if (player.AttackRange > Vector3.Distance(playerTransform.position, enemyTransform.position))
        {
            ConfirmAttack(player, enemyTarget);
        }
        else
        {
            Debug.Log("Target " + enemyName + " is out of range.");
            ReturnToMoveState();
        }
    }

    void TargetClosestEnemy(PlayerObject player, Transform playerTransform)
    {
        float closestEnemyDistance = player.AttackRange;
        PlayerObject targetedEnemy = null;

        for (int i = 0; i < _enemyGroup.Count; i++)
        {
            if (Vector3.Distance(playerTransform.position, _enemyGroup[i].playerGameObject.transform.position) <= closestEnemyDistance)
            {
                closestEnemyDistance = Vector3.Distance(playerTransform.position, _enemyGroup[i].playerGameObject.transform.position);
                targetedEnemy = _enemyGroup[i];
                break;
            }
        }

        // target closest enemy, or tell player no enemies in range.
        if (targetedEnemy != null)
        {
            ConfirmAttack(player, targetedEnemy);
        }
        else
        {
            Debug.Log("No enemies in range");
            ReturnToMoveState();
        }
    }

    void ReturnToMoveState() 
    {
        _currentState = GameState.PlayerMove;
        _turnSystem.GameState = _currentState;
        if (_turnSystem._attackConfirmationMenu.activeInHierarchy)
        {
            _turnSystem._attackConfirmationMenu.SetActive(false);
        }
    }

    void ConfirmAttack(PlayerObject playerObject, PlayerObject enemyObject) 
    {
        _turnSystem._attackConfirmationMenu.SetActive(true);
        _turnSystem._attackTargetText.text = "Attacking Target: " + enemyObject.playerGameObject.name;
        playerObject.playerGameObject.transform.LookAt(enemyObject.playerGameObject.transform);
    }

    public PlayerObject SpaceIsOccupied(List<PlayerObject> players, bool playerClicked)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (CursorController.cursorPosition == SnapMovement.Snap(players[i].playerGameObject.transform.position))
            {
                return players[i];
            }
        }
        return null;
    }
}