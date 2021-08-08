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

        void Start()
        {
            if (AttackAction.diceRoll > 0)
                target = AttackAction.lastRangedTargetLocation;
            else
            {
                int missDirection = Random.Range(-1, 2);
                if (missDirection == 0) 
                    missDirection += 1; 
                target = AttackAction.lastRangedTargetLocation + (Vector3.up * (Random.Range(2, 3) * missDirection)) + (Vector3.right * (Random.Range(2, 3) * missDirection));
            }
        }

        void Update()
        {
            if (Vector3.Distance(this.gameObject.transform.position, target) > 1f) // have to stop the rotation before it reaches the target or it'll reset to 90 degrees
                transform.LookAt(target);
            
            transform.position = Vector3.MoveTowards(gameObject.transform.position, target + yOffset, speed * Time.deltaTime);
            
            if (transform.position == target + yOffset)
                StartCoroutine("DestroyThis");
        }

        // A little delay for coolness ;)
        IEnumerator DestroyThis() 
        {
            if (AttackAction.diceRoll > 0)
            {
                AttackAction.hitByMelee = false;
                AttackAction.lastTarget.PlayAnimation("Death");
                yield return new WaitForSeconds(.5f);
            }
            Destroy(this.gameObject);
            AttackAction.attackInProgress = false;
        }
    }
}

