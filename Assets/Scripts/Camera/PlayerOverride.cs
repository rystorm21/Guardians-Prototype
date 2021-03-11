using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOverride : MonoBehaviour
{
    private Vector3 dragOrigin;
    public float dragSpeed = .2f;

    private void OnEnable()
    {
        EventManager.midMouseButtonHold += CameraOverride;
    }

    private void OnDisable()
    {
        EventManager.midMouseButtonHold -= CameraOverride;
    }

    void CameraOverride()
    {
        if (Input.GetMouseButtonDown(2)) {
            dragOrigin = Input.mousePosition;
            return;
        }

        Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
        Vector3 move = new Vector3(pos.x *dragSpeed, 0, pos.y * dragSpeed);

        transform.Translate(move, Space.World);
    }
}
