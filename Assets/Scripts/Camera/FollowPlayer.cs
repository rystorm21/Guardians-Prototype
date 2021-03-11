using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public GameObject player; 
    private Vector3 offset = new Vector3(0,7,7);

    // Update is called once per frame
    void Update()
    {
        FollowCamera();
    }

    void FollowCamera() {
        transform.position = player.transform.position + offset;
    }
}
