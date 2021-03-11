using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SnapMovement
{
    public static Vector3 Snap(Vector3 position)
    {
        Vector3 snapped = new Vector3(Mathf.Round(position.x), Mathf.Round(position.y), Mathf.Round(position.z));
        return snapped;
    }
}
