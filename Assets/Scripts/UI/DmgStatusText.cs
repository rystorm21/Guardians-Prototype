using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DmgStatusText : MonoBehaviour
{
    public Text damageText;
    public Text statusText;
    private Coroutine updateText;

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
    }

    private void FadeText()
    {
        damageText.CrossFadeAlpha(1.0f, .05f, false);
        statusText.CrossFadeAlpha(1.0f, .05f, false);
        damageText.text = "";
        statusText.text = "";
    }

    public void UpdateText(string _damageText, string _statusText)
    {
        if (damageText.text != "")
        {
            StopCoroutine(updateText);
        }

       updateText = StartCoroutine(TextFlash(_damageText, _statusText));
    }

    IEnumerator TextFlash(string _damageText, string _statusText)
    {
        bool isNumber = int.TryParse(_damageText, out int result);
        if (!isNumber)
            damageText.color = Color.white;
        else
            damageText.color = Color.red;
        damageText.text = _damageText;
        statusText.text = _statusText;
        yield return new WaitForSeconds(2);
        FadeText();
    }
}
