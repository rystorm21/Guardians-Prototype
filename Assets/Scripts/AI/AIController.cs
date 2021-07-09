using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    public class AIController : MonoBehaviour
    {
        [SerializeField]
        SessionManager sessionManager;
        List<GridCharacter> enemies;
        List<GridCharacter> players;

        public void Init()
        {
            List<GridCharacter> enemies = sessionManager.turns[sessionManager.TurnIndex].player.characters;
            List<GridCharacter> players = sessionManager.turns[0].player.characters;
            foreach (var character in enemies)
            {
                if (character.ActionPoints > 0)
                {
                    MoveDecision(character, players);
                }
            }
        }

        void MoveDecision(GridCharacter enemy, List<GridCharacter> players)
        {
            // enemy will analyze the battlefield and decide where to go. Enemy may decide to stay put. 
            // enemy will first check if it's in a vulnerable position (flanked)
            int flankedBy = FlankedBy(enemy, players);
            if (flankedBy > 0)
            {
                // find a safer location to flee to
                PossibleMoves();
            }
            else
            {
                // take aggressive action
                AttackDecision();
            }
        }

        // Thinking... Thinking...
        // Find a covered node that's in movement range
        void PossibleMoves()
        {
            sessionManager.HighlightAroundCharacter(sessionManager.currentCharacter, null, 0);
        }

        void AttackDecision()
        {

        }

        void EndTurn()
        {

        }

        int FlankedBy(GridCharacter enemy, List<GridCharacter> players)
        {
            int flanked = 0;
            foreach (GridCharacter player in players)
            {
                if (AttackAction.CheckTargetCover(player, enemy) == 0)
                {
                    flanked++;
                }
            }
            return flanked;
        }
    }

}