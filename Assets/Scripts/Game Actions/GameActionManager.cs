using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    public class GameActionManager : MonoBehaviour
    {
        #region Init
        public static GameActionManager singleton;
        private void Awake() 
        {
            singleton = this;
        }
        #endregion
        
        #region Action Calls
        // public void CharacterAttacks(GridCharacter attacker, Vector3 targetPoint)
        // {
        //     Vector3 origin = attacker.transform.position + Vector3.up * 1.7f;
        //     Vector3 direction = targetPoint - origin;
        //     Vector3 hitPoint = targetPoint;

        //     RaycastHit hit;
        //     int weaponRange = 50;
        //     if (Physics.Raycast(origin, direction.normalized, out hit, weaponRange))
        //     {
        //         hitPoint = hit.point;
        //         IHittable iHit = hit.transform.GetComponentInParent<IHittable>();
        //         if (iHit != null)
        //         {
        //             iHit.OnHit(attacker);
        //         }
        //     }
        //     Debug.DrawRay(origin, hitPoint - origin, Color.red, 2);
        // }

        public void CharacterHealthUpdate(GridCharacter character)
        {

        }

        public void CharacterBrace(GridCharacter character)
        {

        }

        public void CharacterUpdatesItem(GridCharacter character)
        {

        }

        public void CharacterMoves()
        {

        }
        #endregion
    }
}