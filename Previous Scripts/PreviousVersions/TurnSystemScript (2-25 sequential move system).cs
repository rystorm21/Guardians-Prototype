using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnSystemScript : MonoBehaviour
{
    public List<TurnClass> playerGroup;
    public List<TurnClass> enemyGroup;

    bool playerTurn = true;

    // Start is called before the first frame update
    void Start()
    {
        ResetTurns(playerGroup);
    }

    // Update is called once per frame
    void Update()
    {
        if (playerTurn) {
            UpdateTurns(playerGroup);
        } else {
            UpdateTurns(enemyGroup);
        }
    }

    // Resets all the turns so the first player's move is up next (for player or enemy) - start of round
    void ResetTurns(List<TurnClass> currentGroup) 
    {   
        for (int i=0; i < currentGroup.Count; i++) 
        {
            if (i == 0) 
            {
                currentGroup[i].isTurn = true;
                currentGroup[i].moveComplete = false;
            }
            else
            {   
                currentGroup[i].isTurn = false;
                currentGroup[i].moveComplete = false;
            }
        }
    }

    void UpdateTurns(List<TurnClass> currentGroup) 
    {
        for (int i = 0; i < currentGroup.Count; i++) 
        {   
            if (!currentGroup[i].moveComplete)
            {
                if (Input.GetKeyDown(KeyCode.Tab)) {
                    currentGroup[i].isTurn = false;
                    break;
                }
                currentGroup[i].isTurn = true;
                break;
            }
            else if (i == (currentGroup.Count -1) && currentGroup[i].moveComplete)
            {
                playerTurn = !playerTurn;
                if (playerTurn){
                    ResetTurns(playerGroup);
                }
                else {
                    ResetTurns(enemyGroup);
                }
            }
        }
    }
}

[System.Serializable]
public class TurnClass 
{
    public GameObject playerGameObject;
    public bool isTurn = false;
    public bool moveComplete = false;
}