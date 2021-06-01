using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV 
{
    public class SpecialAbilityAction : GameAction
    {
        public override void OnActionStart(SessionManager sessionManager, Turn turn)
        {
            // insert logic to find out what type of special ability it is for this character. It could be self, pbaoe, or targeted
            GridCharacter currentCharacter = sessionManager.currentCharacter;
            int selection = currentCharacter.character.abilitySelected;
            Ability abilitySelected = currentCharacter.character.abilityPool[selection].ability;
            
            Debug.Log(currentCharacter.character.characterName + " Ability " + abilitySelected + " selected.");
        }
        public override void OnActionTick(SessionManager sessionManager, Turn turn, Node node, RaycastHit hit)
        {
            // insert stuff here
        }

        public override void OnHighlightCharacter(SessionManager sessionManager, Turn turn, Node node)
        {
        }

        public override void OnDoAction(SessionManager sessionManager, Turn turn, Node node, RaycastHit hit)
        {
        }
    }
}
