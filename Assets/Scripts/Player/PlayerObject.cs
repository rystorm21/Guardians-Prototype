using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class PlayerObject
{
    public GameObject playerGameObject;
    public bool isTurn = false;
    public bool moveComplete = false;
    private int actionPoints;
    private int moveDist = 6;
    private int attackRange = 6;
    private int hitPoints = 60;

    public int HitPoints 
    {
        get { return hitPoints; }
    }

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