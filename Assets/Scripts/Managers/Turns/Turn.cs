using System.Collections;
using UnityEngine;

namespace EV 
{
    [CreateAssetMenu(menuName = "Game/Turn")]
    public class Turn : ScriptableObject
    {
        public static bool playerTurn;
        public PlayerHolder player;
        [System.NonSerialized]
        int index = 0;
        public Phase[] phases;

        // run phases[0].OnStartPhase, passing sessionmanager and this class
        // Turn is a scriptable object
        // Phase is a scriptable object, and an abstract class. Check Data/Turns/PlayerTurn to see the Phases[] elements

        public int Index 
        {
            get { return index;}
        }

        public bool Execute(SessionManager sessionManager) 
        {
            bool result = false;
            
            phases[index].OnStartPhase(sessionManager, this);   // doesn't do anything 3/24/21
            
            if (phases[index].IsComplete(sessionManager, this)) // automatically returns false so below bracket isn't run
            {
                phases[index].OnEndPhase(sessionManager, this);
                index++;
            }
            if (index > phases.Length -1)                       // if index is greater than the amount of phases, reset it to 0, return result to true, otherwise false
            {
                index = 0;
                result = true;                                      
            }
            return result;
        }
        
        public void EndCurrentPhase()
        {
            phases[index].forceExit = true;
        }
    }
}
