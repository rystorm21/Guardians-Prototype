using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EV
{
    public class LoadScreenFunction : MonoBehaviour
    {
        GameObject currentSceneHolderObject;
        SceneHolder sceneHolder;
        Level currentLevel;

        private void Start() 
        {
            currentSceneHolderObject = GameObject.Find("CurrentSceneHolder");
            sceneHolder = currentSceneHolderObject.GetComponent<SceneHolder>();
            currentLevel = currentSceneHolderObject.GetComponent<SceneHolder>().currentLevel;
        }

        public void LoadNextScene()
        {
            sceneHolder.NextLevel();
            SceneManager.LoadScene(currentLevel.nextScene);
        }
    }
}

