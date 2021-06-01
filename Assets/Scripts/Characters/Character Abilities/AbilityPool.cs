using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    [CreateAssetMenu(fileName = "New Ability Pool", menuName = "/Ability System/AbilityPool")]
    public class AbilityPool : ScriptableObject
    {
        public List<AbilitySlot> abilityContainer = new List<AbilitySlot>();

        public void AddAbility()
        {
            // insert functionality here
            Debug.Log("You've learned a new ability!");
        }
    }

    [System.Serializable]
    public class AbilitySlot
    {
        public Ability ability;
        public Sprite abilityIcon;

        public AbilitySlot(Ability _ability)
        {
            ability = _ability;
        }
    }
}
