using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    [CreateAssetMenu(menuName = "Game Variables Holder")]
    public class VariablesHolder : ScriptableObject
    {
        public float cameraMoveSpeed = 15;

        [Header("Game Events")]
        public SO.GameEvent updateAP;

        [Header("Scriptable Variables")]
        #region Scriptables
        public SO.StringVariable actionPointsText;
        public TransformVariable cameraTransform;
        public FloatVariable horizontalInput;
        public FloatVariable verticalInput;
        #endregion

        public void UpdateMouseText(string targetText)
        {
            actionPointsText.value = targetText;
            updateAP.Raise();
        }
    }
}
