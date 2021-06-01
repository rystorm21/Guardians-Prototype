using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    [CreateAssetMenu(fileName = "New Ability Object", menuName = "Abilities/Self Ability")]
    public class AbilitySelf : Ability
    {
        public void Awake()
        {
            type = AbilityType.Self;
        }
    }
}

