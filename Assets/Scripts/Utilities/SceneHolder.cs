using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    public class SceneHolder : MonoBehaviour
    {
        public Level currentLevel;

        public void NextLevel()
        {
            currentLevel = currentLevel.nextLevel;
        }
    }
}

