using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace EV
{
    public abstract class Ability2 : ScriptableObject // changed to ability2 from ability - lost in the sauce. test if it works.
    {
        public string abilityName = "New Ability";
        public string abilitySprite;
        public AudioClip abilitySound;
        public int abilityBaseCoolDown = 1;

        public abstract void Initialize (GameObject gameObject);
        public abstract void TriggerAbility();
    }
}