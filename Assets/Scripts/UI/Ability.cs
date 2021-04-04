using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace EV
{
    public abstract class Ability : ScriptableObject
    {
        public string abilityName = "New Ability";
        public string abilitySprite;
        public AudioClip abilitySound;
        public int abilityBaseCoolDown = 1;

        public abstract void Initialize (GameObject gameObject);
        public abstract void TriggerAbility();
    }
}