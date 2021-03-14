using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerObject
{
    public GameObject playerGameObject;
    public bool isTurn = false;
    public bool moveComplete = false;
    private int actionPoints;
    private int moveDist = 6;
    private int attackRange = 6;

    public int ActionPoints
    {
        get { return actionPoints; }
        set { actionPoints = value; }
    }

    public int MoveDist
    {
        get { return moveDist; }
        set { moveDist = value; }
    }

    public int AttackRange
    {
        get { return attackRange; }
        set { attackRange = value; }
    }
}