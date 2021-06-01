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
        public SO.GameEvent updatePortrait;
        public SO.GameEvent updateSpecialAbility;

        [Header("Scriptable Variables")]
        #region Scriptables
        public SO.StringVariable actionPointsText;
        public SO.SpriteVariable characterPortrait;
        public SO.SpriteVariable characterSA;

        public TransformVariable cameraTransform;
        public FloatVariable horizontalInput;
        public FloatVariable verticalInput;
        #endregion

        public void UpdateMouseText(string targetText)
        {
            actionPointsText.value = targetText;
            updateAP.Raise();
        }

        public void UpdateCharacterPortrait(Sprite characterSprite) 
        {
            characterPortrait.value = characterSprite;
            updatePortrait.Raise();
        }

        public void UpdateAbility(Sprite abilitySprite)
        {
            characterSA.value = abilitySprite;
            updateSpecialAbility.Raise();
        }
    }
}
