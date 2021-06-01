using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    [CreateAssetMenu(fileName = "New Ability Object", menuName = "Abilities/Ranged Ability")]
    public class AbilityRanged : Ability
    {
        public void Awake()
        {
            type = AbilityType.Ranged;
        }
    }
}
