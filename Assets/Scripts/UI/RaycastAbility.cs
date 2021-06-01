using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    // For LoS Abilities
    [CreateAssetMenu(menuName = "Abilities/RaycastAbility")]
    public class RaycastAbility : Ability2
    {
        public int Damage = 1;
        public float weaponRange = 50;
        public Color laserColor = Color.white;

        public override void Initialize(GameObject obj)
        {

        }

        public override void TriggerAbility()
        {
        
        }
    }
}

