using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public float _volume;
    AudioSource _backgroundSong;

    private void OnEnable() 
    {
        _backgroundSong = GameObject.Find("Main Camera").GetComponent<AudioSource>();
    }

    public void onChange()
    {
        _volume = GetComponent<Slider>().value;
        _backgroundSong.volume = _volume;
    }
}
