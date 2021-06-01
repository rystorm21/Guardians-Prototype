using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    [CreateAssetMenu(fileName = "New Ability Object", menuName = "Abilities/PBAoE Ability")]
    public class AbilityPBAoE : Ability
    {
        public void Awake()
        {
            type = AbilityType.PBAoE;
        }
    }
}

