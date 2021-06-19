using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
public class AbilityPanelUI : MonoBehaviour
    {
        public void Deactivate(SessionManager sessionManager)
        {
            sessionManager.abilityPanelUI.gameObject.SetActive(false);
        }
        public void Activate(SessionManager sessionManager)
        {
            sessionManager.abilityPanelUI.gameObject.SetActive(true);
        }
    }
}
