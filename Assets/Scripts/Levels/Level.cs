using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    public class Level : ScriptableObject
    {        
        public string thisLevel;
        public GameState startingMode;
        public GameState postPreDialogMode;
        public bool hasPostDialogue;
        public string postDialogueTitle;
        public string nextScene;
    }
}

