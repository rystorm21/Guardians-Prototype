using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class PlayerController : MonoBehaviour
{
    private TurnSystem _turnSystem;
    private bool isTurn;
    public PlayerObject _currentPlayer;
    GameState currentState;

    // Start is called before the first frame update
    void Start()
    {
        _turnSystem = GameObject.Find("Turn-basedSystem").GetComponent<TurnSystem>();
        InitializePlayers();
    }

    void InitializePlayers()
    {
        foreach (PlayerObject player in _turnSystem._playerGroup)
        {
            if (player.playerGameObject.name == gameObject.name) _currentPlayer = player;
        }
    }

    private void OnEnable() {
        EventManager.onButtonClick += HelloWorld;
    }

    private void OnDisable() {
        EventManager.onButtonClick -= HelloWorld;
    }

    void HelloWorld()
    {
        isTurn = _currentPlayer.isTurn;
        if (isTurn) 
        {
            Debug.Log("You just moved: " + _currentPlayer.playerGameObject.transform.GetChild(0).name); 
        }
    }
}