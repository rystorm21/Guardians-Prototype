using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class EventManager : MonoBehaviour
{
    public delegate void OnButtonClick();
    public delegate void MidMouseButtonHold();

    public static event OnButtonClick onButtonClick;
    public static event MidMouseButtonHold midMouseButtonHold;

    private void Update()
    {
        // Only register if it's not over a button / UI object
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (onButtonClick != null)
                {
                    onButtonClick();
                }
            }
            if (Input.GetMouseButton(2)){
                if (midMouseButtonHold != null)
                {
                    midMouseButtonHold();
                }
            }
        }
    }
}
