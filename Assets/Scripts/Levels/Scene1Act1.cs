using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    [CreateAssetMenu(fileName = "New Level Object", menuName = "Level")]
    public class Scene1Act1 : Level
    {
        private void Awake() 
        {
            thisLevel = "Act 1 Scene 1";
            startingMode = GameState.Dialog;
        }
    }
}
