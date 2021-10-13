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
            transform.LookAt(transform.position + cam.forward);
        }
    }
}

