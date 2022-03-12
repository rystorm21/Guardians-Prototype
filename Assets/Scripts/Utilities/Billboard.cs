using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    public class Billboard : MonoBehaviour
    {
        public Transform cam;

        private void Awake() 
        {
            cam = GameObject.Find("Main Camera").GetComponent<Transform>();
        }

        private void LateUpdate() 
        {
            if (cam == null)
                Awake();
            if (GameObject.FindGameObjectWithTag("TargetingCam"))
            {
                transform.LookAt(GameObject.FindGameObjectWithTag("TargetingCam").transform.position);
            }
            else
            {
                transform.LookAt(transform.position + cam.forward);
            }
        }
    }
}

