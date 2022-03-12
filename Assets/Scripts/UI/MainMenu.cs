using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    bool _gameOptionsActive;
    public GameObject _gameOptionsMenu;

    private void OnEnable() {
        
        SceneManager.LoadSceneAsync("Persistent Scene", LoadSceneMode.Additive);
        _gameOptionsMenu = GameObject.Find("GameOptionsMenu");
        _gameOptionsMenu.SetActive(false);
    }

    public void GameOptionsMenu() 
    {
        _gameOptionsMenu.SetActive(true);
    }

    public void ReturnToMain()
    {
        _gameOptionsMenu.SetActive(false);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Act1_S1", LoadSceneMode.Additive);
        SceneManager.UnloadSceneAsync("MainMenu");
    }
}
