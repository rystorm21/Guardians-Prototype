using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV 
{
    public class State
    {
        public List<StateActions> actions = new List<StateActions>();

        public void Tick(StateManager states, SessionManager sessionManager, Turn turn)
        {
            if (states.forceExit)
                return;
            for (int i = 0; i < actions.Count; i++)
            {
                actions[i].Execute(states, sessionManager, turn);
            }
        }
    }   
}
