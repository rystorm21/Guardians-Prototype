using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EV
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        SessionManager sessionManager;
        public Level currentLevel;
        public Level nextLevel;

        private void Awake()
        {
            instance = this;
        }

        public void Init()
        {
            sessionManager = GameObject.Find("Grid Manager").GetComponent<SessionManager>();
            currentLevel = sessionManager.currentLevel;
            nextLevel = currentLevel.nextLevel;
        }

        public void LoadNextScene()
        {
            Scene sceneToLoad = SceneManager.GetSceneByName(nextLevel.thisLevel);
            SceneManager.LoadScene(nextLevel.thisLevel, LoadSceneMode.Additive);
            SceneManager.UnloadSceneAsync("Loading");
        }
    }
}

