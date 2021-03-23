using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    [CreateAssetMenu(menuName = "Game Variables Holder")]
    public class VariablesHolder : ScriptableObject
    {
        public float cameraMoveSpeed = 15;

        [Header("Scriptable Variables")]
        #region Scriptables
        public TransformVariable cameraTransform;
        public FloatVariable horizontalInput;
        public FloatVariable verticalInput;
        #endregion
    }
}
