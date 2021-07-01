using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    [CreateAssetMenu(menuName = "Phases/TurnStartPhase Phase")]
    public class TurnStartPhase : Phase
    {
        public override bool IsComplete(SessionManager sessionManager, Turn turn)
        {
            return true;
        }

        public override void OnStartPhase(SessionManager sessionManager, Turn turn)
        {
            List<Ability> decrementor = new List<Ability>();
            
            // just to show which turn it is - debugging purposes
            Debug.Log(turn.name); 
            
            foreach (GridCharacter character in turn.player.characters)
            {
                // initializes the characters stats / AP for all characters
                if (character.character.appliedStatus.Count > 0)
                {
                    for (int i = 0; i < character.character.appliedStatus.Count; i++)
                    {
                        if (!decrementor.Contains(character.character.appliedStatus[i]))
                        {
                            // ensures ability / buffs only get decremented once
                            decrementor.Add(character.character.appliedStatus[i]);
                            character.character.appliedStatus[i].durationCountdown--;
                        }
                    }
                }
                character.character.ZeroStatus();
                character.OnStartTurn();
                if (character.owner.isLocalPlayer)
                {
                    if (turn.player.stateManager.CurrentCharacter == character)
                    {
                        sessionManager.HighlightAroundCharacter(character, null, 0);
                    }
                }
                character.currentNode.isWalkable = false;
            }

            if (turn.player.stateManager.CurrentCharacter == null)
            {
                AutoSetPlayer(sessionManager, turn);
            }
        }

        private void AutoSetPlayer(SessionManager sessionManager, Turn turn) 
        {
            GridCharacter defaultPlayer = turn.player.characters[0];
            sessionManager.currentCharacter = defaultPlayer;
            sessionManager.HighlightAroundCharacter(defaultPlayer, null, 0);
            sessionManager.currentCharacter.isSelected = true;
        }
    }
}