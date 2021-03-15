using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyMoveTiles : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) 
    {
        // check if a player is moving
        if (!PlayerController.IsMoving) {
            Destroy(this.gameObject);
        }
    }
}
