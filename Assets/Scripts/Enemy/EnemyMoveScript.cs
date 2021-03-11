using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMoveScript : MonoBehaviour
{
    // trying something
    public TurnSystem _turnSystem;
    public PlayerObject _currentEnemy;
    public bool isTurn = false;

    // Initialize
    void Start()
    {
        _turnSystem = GameObject.Find("Turn-basedSystem").GetComponent<TurnSystem>();
        foreach (PlayerObject tc in _turnSystem._enemyGroup)
        {
            if (tc.playerGameObject.name == gameObject.name) _currentEnemy = tc;
        }
    }

    // Update is called once per frame
    void Update()
    {
        isTurn = _currentEnemy.isTurn;
        if (isTurn)
        {
            // Debug.Log("Enemy Turn");
            StartCoroutine("WaitAndMove");
        }
    }

    IEnumerator WaitAndMove()
    {
        yield return new WaitForSeconds(2f);
        transform.position += Vector3.forward; // replace with enemy AI script
        
        isTurn = false;
        _currentEnemy.isTurn = isTurn;
        _currentEnemy.moveComplete = true;
        StopCoroutine("WaitAndMove");
    }
}
