using System.Collections;
using UnityEngine;

namespace EV 
{
    [CreateAssetMenu(menuName = "Game/Turn")]
    public class Turn : ScriptableObject
    {
        public PlayerHolder player;
        [System.NonSerialized]
        int _index = 0;
        public Phase[] phases;

        public bool Execute(SessionManager sessionManager) 
        {
            bool result = false;

            phases[_index].OnStartPhase(sessionManager, this);

            if (phases[_index].IsComplete(sessionManager, this))
            {
                phases[_index].OnEndPhase(sessionManager, this);
                _index++;
            }
            if (_index > phases.Length -1)
            {
                _index = 0;
                result = true;
            }
            return result;
        }
        
        public void EndCurrentPhase()
        {
            phases[_index].forceExit = true;
        }
    }
}
