using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    Button newGameButton;

    // Start is called before the first frame update
    void Start()
    {
        newGameButton = GameObject.Find("NewGame.Button").GetComponent<Button>();
    }

    public void NewGameClicked() 
    {
        SceneManager.LoadScene("Combat");
    }
    
}
