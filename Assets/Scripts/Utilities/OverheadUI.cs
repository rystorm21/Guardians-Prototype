using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;   

public class OverheadUI : MonoBehaviour
{
    public Text nameLabel;
    Vector3 namePos;

    void Update()
    {
        namePos = Camera.main.WorldToScreenPoint(this.transform.position + (Vector3.up * .5f));
        nameLabel.transform.position = namePos;
    }
}
