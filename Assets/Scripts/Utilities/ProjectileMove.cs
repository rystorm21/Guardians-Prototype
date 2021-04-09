using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV 
{
    // Script for LoS projectiles, takes static value for location from AttackAction
    public class ProjectileMove : MonoBehaviour
    {
        [SerializeField]
        private float speed = 25f;
        Vector3 target;
        Vector3 yOffset = new Vector3(0, 1f, 0);

        // Start is called before the first frame update
        void Start()
        {
            Debug.Log(AttackAction.lastRangedTarget);
            target = AttackAction.lastRangedTarget;
        }

        // Update is called once per frame
        void Update()
        {
            transform.LookAt(target);
            transform.position = Vector3.MoveTowards(gameObject.transform.position, target + yOffset, speed * Time.deltaTime);
            Debug.Log(transform.position);
            if (transform.position == target + yOffset)
                Destroy(this.gameObject);
        }
    }
}

