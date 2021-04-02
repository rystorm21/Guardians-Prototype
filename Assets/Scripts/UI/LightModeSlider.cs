using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LightModeSlider : MonoBehaviour
{
    public int _lightMode;
    
    public void OnChange()
    {
        _lightMode = Mathf.RoundToInt(GetComponent<Slider>().value);
        Image background = GameObject.Find("Background").GetComponent<Image>();

        switch(_lightMode)
        {
            case 0:
            background.color = Color.black;
            break;

            case 1:
            background.color = Color.gray;
            break;
        }
    }
}
