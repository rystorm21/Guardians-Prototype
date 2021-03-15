using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CursorController : MonoBehaviour
{
    private TextMeshPro moveCostTMP;
    private GameObject moveCursor;
    private TurnSystem _turnSystem;

    public LayerMask whatCanBeClickedOn;
    public static Vector3 cursorPosition;
    public static Color cursorColor;
    public static string moveCostText;

    // Start is called before the first frame update
    void Start()
    {
        _turnSystem = GameObject.Find("Turn-basedSystem").GetComponent<TurnSystem>();
        moveCostTMP = GameObject.Find("MoveCostText").GetComponent<TextMeshPro>();
        moveCursor = GameObject.Find("Cursor");
        cursorColor = Color.blue;
    }

    // Update is called once per frame
    void Update()
    {
        MoveCursor();
    }

    void GetCoordinates() 
    {

    }

    void MoveCursor()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, 100, whatCanBeClickedOn))
        {
            cursorPosition = SnapMovement.Snap(hitInfo.point);
            moveCursor.transform.position = cursorPosition;
            moveCursor.GetComponent<Renderer>().material.color = cursorColor;
            moveCostTMP.text = moveCostText;
        }
    }
}
