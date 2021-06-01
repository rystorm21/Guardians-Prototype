using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    [CreateAssetMenu(fileName = "New Ability Object", menuName = "Abilities/Ranged AOE Ability")]
    public class AbilityRangedAoE : Ability
    {
        public void Awake()
        {
            type = AbilityType.RangedAoe;
        }
    }
}

