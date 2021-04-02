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

        // run phases[0].OnStartPhase, passing sessionmanager and this class
        // Turn is a scriptable object
        // Phase is a scriptable object, and an abstract class. Check Data/Turns/PlayerTurn to see the Phases[] elements

        public int Index 
        {
            get { return _index;}
        }

        public bool Execute(SessionManager sessionManager) 
        {
            bool result = false;
            
            phases[_index].OnStartPhase(sessionManager, this);   // doesn't do anything 3/24/21
            
            if (phases[_index].IsComplete(sessionManager, this)) // automatically returns false so below bracket isn't run
            {
                phases[_index].OnEndPhase(sessionManager, this);
                _index++;
            }
            if (_index > phases.Length -1)                       // if index is greater than the amount of phases, reset it to 0, return result to true, otherwise false
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
