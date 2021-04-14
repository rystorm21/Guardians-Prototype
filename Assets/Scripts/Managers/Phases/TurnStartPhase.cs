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
            foreach (GridCharacter character in turn.player.characters)
            {
                // initializes the characters stats / AP for all characters
                character.OnStartTurn();

                if (character.owner.isLocalPlayer)
                {
                    if (turn.player.stateManager.currentCharacter == character)
                    {
                        sessionManager.HighlightAroundCharacter(character);
                    }
                }
                character.currentNode.isWalkable = false;
            }

            if (turn.player.stateManager.currentCharacter == null)
            {
                GridCharacter defaultPlayer = turn.player.characters[0];
                turn.player.stateManager.currentCharacter = defaultPlayer;
                sessionManager.HighlightAroundCharacter(defaultPlayer);
                sessionManager.currentCharacter.isSelected = true;
            }
        }
    }
}