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

            // Run the Execute() method of all StateActions
            foreach(StateActions action in actions)
            {
                action.Execute(states, sessionManager, turn);
            }
        }
    }   
}

// Original Code, replaced by the foreach loop.
// if there's no force exit, run through all the actions and execute them.
// for (int i = 0; i < actions.Count; i++)
// {
//    actions[i].Execute(states, sessionManager, turn);
// }