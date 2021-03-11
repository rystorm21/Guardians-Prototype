using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    public GameObject moveCursor;
    public LayerMask whatCanBeClickedOn;
    public static Vector3 cursorPosition;
    public static Color cursorColor;

    // Start is called before the first frame update
    void Start()
    {
        moveCursor = GameObject.Find("Cursor");
        cursorColor = Color.blue;
    }

    // Update is called once per frame
    void Update()
    {
        MoveCursor();
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
        }
    }
}
