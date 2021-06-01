using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    [CreateAssetMenu(fileName = "New Ability Object", menuName = "Abilities/Passive Ability")]
    public class AbilityPassive : Ability
    {
        public void Awake()
        {
            type = AbilityType.Passive;
        }
    }
}
