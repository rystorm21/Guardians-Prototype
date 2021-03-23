using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    public abstract class Phase : ScriptableObject
    {
        public string phaseName;
        //[System.NonSerialized]
        public bool forceExit;

        [System.NonSerialized]
        protected bool isInit;

        public abstract bool IsComplete(SessionManager sessionManager, Turn turn);

        public abstract void OnStartPhase(SessionManager sessionManager, Turn turn);

        public virtual void OnEndPhase(SessionManager sessionManager, Turn turn)
        {
            isInit = false;
            forceExit = false;
        }
    }
}
