using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    public abstract class StateActions : MonoBehaviour
    {
        public abstract void Execute(StateManager states, SessionManager sessionManager, Turn turn);
    }
}

