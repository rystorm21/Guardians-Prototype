using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EV
{
    public class LoadScreenFunction : MonoBehaviour
    {
        public void LoadNextScene()
        {
            GameManager.instance.LoadNextScene();
        }
    }
}

