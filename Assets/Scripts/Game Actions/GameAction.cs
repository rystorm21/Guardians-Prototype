using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    public abstract class GameAction
    {
        public virtual void OnActionStart(SessionManager sessionManager, Turn turn)
        {
            
        }

        public virtual void OnActionStop(SessionManager sessionManager, Turn turn)
        {

        }

        public virtual bool IsActionValid(SessionManager sessionManager, Turn turn)
        {
            if (!turn.player.isLocalPlayer)
                return false;

            return true;
        }

        public abstract void OnActionTick(SessionManager sessionManager, Turn turn, Node node, RaycastHit hit);
        public abstract void OnHighlightCharacter(SessionManager sessionManager, Turn turn, Node node);
        public abstract void OnDoAction(SessionManager sessionManager, Turn turn, Node node, RaycastHit hit);
    }
}

