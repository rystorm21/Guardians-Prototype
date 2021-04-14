using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV 
{
    [CreateAssetMenu(menuName = "Phases/States Tick")]
    public class StatesTickPhase : Phase
    {
        public override bool IsComplete(SessionManager sessionManager, Turn turn)
        {
            if (forceExit)
            {
                return true;
            }
            turn.player.stateManager.Tick(sessionManager, turn);
            return false;
        }

        public override void OnStartPhase(SessionManager sessionManager, Turn turn)
        {

        }
    }   
}
