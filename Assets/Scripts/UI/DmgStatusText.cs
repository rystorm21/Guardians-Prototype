using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DmgStatusText : MonoBehaviour
{
    public Text damageText;
    public Text statusText;

    private void Awake() 
    {
        ZeroText();
        damageText.fontSize = 26;
        statusText.fontSize = 26;
    }

    private void ZeroText()
    {
        damageText.text = "";
        statusText.text = "";
        damageText.CrossFadeAlpha(1.0f, .05f, false);
        statusText.CrossFadeAlpha(1.0f, .05f, false);
    }

    private void FadeText()
    {
        damageText.CrossFadeAlpha(0, .5f, false);
        statusText.CrossFadeAlpha(0, .5f, false);
    }

    public void UpdateText(string _damageText, string _statusText)
    {
        StartCoroutine(TextFlash(_damageText, _statusText));
    }

    IEnumerator TextFlash(string _damageText, string _statusText)
    {
        if (_damageText == "Miss")
            damageText.color = Color.white;
        else
            damageText.color = Color.red;
        damageText.text = _damageText;
        statusText.text = _statusText;
        yield return new WaitForSeconds(1);
        FadeText();
        yield return new WaitForSeconds(1);
        ZeroText();
    }
}
