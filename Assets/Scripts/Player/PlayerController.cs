using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class PlayerController : MonoBehaviour
{
    private List<PlayerObject> _playerGroup;
    private List<PlayerObject> _enemyGroup;
    private TurnSystem _turnSystem;
    private NavMeshAgent _playerNavMesh;
    private PlayerObject _targetedEnemy;
    private bool _isTurn;
    private float[] _currentSpeed; // caching purposes
    private int _activePlayer;
    private Vector3[] _lastPosition;
    private static bool _isMoving;
    private static bool _noDraw;
    GameState _currentState;
    public PlayerObject _currentPlayer;

    //VFX
    public GameObject firePoint;
    public GameObject projectile;
    
    public PlayerObject TargetedEnemy
    {
        get { return _targetedEnemy; }
    }

    public static bool IsMoving 
    {
        get { return _isMoving; }
    }

    public static bool NoDraw
    {
        get { return _noDraw; }
    }

    // Start is called before the first frame update
    void Start()
    {
        _turnSystem = GameObject.Find("Turn-basedSystem").GetComponent<TurnSystem>();
        _playerNavMesh = GetComponent<NavMeshAgent>();
        InitializePlayers();
        _currentSpeed = new float[_playerGroup.Count];
        _lastPosition = new Vector3[_playerGroup.Count];
        _noDraw = false;
    }

    void InitializePlayers()
    {
        _playerGroup = _turnSystem.PlayerGroup;
        _enemyGroup = _turnSystem.EnemyGroup;
        // already instantiated all player objects here.
        foreach (PlayerObject player in _turnSystem._playerGroup)
        {
            if (player.playerGameObject.name == gameObject.name) _currentPlayer = player;
        }
        _isTurn = _currentPlayer.isTurn;
    }

    private void Update() {
        if (_isTurn) 
        {
            _isMoving = CheckIfMoving();
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
                _isTurn = _currentPlayer.isTurn;
                if (!IsMoving)
                {
                    if (_isTurn)
                    {
                        occupiedPlayer = SpaceIsOccupied(_playerGroup, true);
                        occupiedEnemy = SpaceIsOccupied(_enemyGroup, false);
                        int activePlayer = _turnSystem.ActivePlayerIndex;

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
                                MovePlayer();
                            }
                            else
                            {
                                Debug.Log("Out of movement range");
                            }
                        }
                    }
                }
                break;

            case (GameState.PlayerAttack):
                AttackEnemy();
                break;

        }
    }

    void MovePlayer()
    {
        _activePlayer = _turnSystem.ActivePlayerIndex; // need this for the IEnumerator
        PlayerObject player = _playerGroup[_activePlayer];
        player.playerGameObject.GetComponent<NavMeshAgent>().SetDestination(CursorController.cursorPosition);
        _turnSystem.Waiting = false;
        _turnSystem.MoveGrid(CursorController.cursorPosition);

        if (CursorController.cursorColor == Color.yellow) player.ActionPoints -= int.Parse(CursorController.moveCostText); // yellow cursor = more than 1 action cost
        else { player.ActionPoints--; }
        if (player.ActionPoints <= 0)
        {
            StartCoroutine("LastActionPoints");
        }
    }

    IEnumerator LastActionPoints()
    {
        Debug.Log("action points spent");
        _noDraw = true;
        PlayerObject player = _playerGroup[_activePlayer];
        Transform childTransform = player.playerGameObject.transform.GetChild(0).transform;
        
        yield return new WaitForSeconds(2);
        player.isTurn = false;
        player.moveComplete = true;
        if (childTransform.localPosition.x != 0) { childTransform.localPosition = new Vector3(0,0,0); } // Andromeda keeps going off center, do this to fix it4
        _noDraw = false;
    }

    public void AttackEnemy()
    {
        if (_turnSystem.GameState != GameState.EnemyTurn && _turnSystem.ActivePlayer.ActionPoints > 0) 
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
        _targetedEnemy = enemyObject;
        _turnSystem._attackConfirmationMenu.SetActive(true);
        _turnSystem._attackTargetText.text = "Attacking Target: " + enemyObject.playerGameObject.name;
        playerObject.playerGameObject.transform.LookAt(enemyObject.playerGameObject.transform);
    }

    public void DoAttack() 
    {
        _activePlayer = _turnSystem.ActivePlayerIndex;
        _turnSystem.ActivePlayer.ActionPoints--;
        StartCoroutine("SpawnAttack");

        if (_turnSystem.ActivePlayer.ActionPoints == 0) 
        {
            StartCoroutine("LastActionPoints");
            ReturnToMoveState();
        }
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

    // checks to see if any player is moving. If any player is moving, break out of the loop and return true
    public bool CheckIfMoving()
    {
        int players = _turnSystem._playerGroup.Count;

        for (int i = 0; i < players; i++) {
            // check every player's speed, if a single player's speed is above zero, then a player is moving, return true.
            _currentSpeed[i] = (_turnSystem._playerGroup[i].playerGameObject.transform.position - _lastPosition[i]).magnitude;
            _lastPosition[i] = _turnSystem.PlayerGroup[i].playerGameObject.transform.position;
            if (_currentSpeed[i] > 0) {
                return true;
            }
        }
        return false;
    }

    IEnumerator SpawnAttack() 
    {
        _turnSystem.ActivePlayer.playerGameObject.GetComponentInChildren<Animator>().SetTrigger("attack");
        yield return new WaitForSeconds(0.75f);
        string playerName = _turnSystem.ActivePlayer.playerGameObject.transform.GetChild(0).name;
        Vector3 offset  = new Vector3(0,.5f,0);
        firePoint = _turnSystem.ActivePlayer.playerGameObject.transform.Find(playerName + "/metarig/IKHand.R").gameObject;
        
        if (firePoint != null)
        {
            Instantiate(projectile, firePoint.transform.position + offset, Quaternion.identity);
        }
    }
}