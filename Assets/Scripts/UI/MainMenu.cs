using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    bool _gameOptionsActive;
    public GameObject _gameOptionsMenu;

    private void OnEnable() {
        
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
}
