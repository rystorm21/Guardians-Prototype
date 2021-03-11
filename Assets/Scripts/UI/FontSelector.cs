using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FontSelector : MonoBehaviour
{
    int _normalFontSize = 18;
    int _largeFontSize = 24;

    TextMeshProUGUI _newGameText;
    TextMeshProUGUI _newGameText2;
    TextMeshProUGUI _newGameText3;
    TextMeshProUGUI _newGameText4;

    public void DropdownItemSelected()
    {
        int selection = GetComponent<TMP_Dropdown>().value;

        _newGameText = GameObject.Find("NewGame.Text").GetComponent<TextMeshProUGUI>();
        _newGameText2 = GameObject.Find("LoadGame.Text").GetComponent<TextMeshProUGUI>();
        _newGameText3 = GameObject.Find("Options.Text").GetComponent<TextMeshProUGUI>();
        _newGameText4 = GameObject.Find("Exit.Text").GetComponent<TextMeshProUGUI>();

        switch(selection)
        {
            case(0):
            _newGameText.fontSize = _normalFontSize;
            _newGameText2.fontSize = _normalFontSize;
            _newGameText3.fontSize = _normalFontSize;
            _newGameText4.fontSize = _normalFontSize;
            break;
        
            case(1):
            _newGameText.fontSize = _largeFontSize;
            _newGameText2.fontSize = _largeFontSize;
            _newGameText3.fontSize = _largeFontSize;
            _newGameText4.fontSize = _largeFontSize;
            break;
        }
    }
}
