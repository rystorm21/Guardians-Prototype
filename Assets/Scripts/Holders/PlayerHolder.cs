using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    [CreateAssetMenu(menuName = "Game/Player Holder")]
    public class PlayerHolder : ScriptableObject
    {
        [System.NonSerialized]
        public StateManager stateManager;
        [System.NonSerialized]
        GameObject stateManagerObject;
        public GameObject _stateManagerPrefab;

        [System.NonSerialized]
        public List<GridCharacter> characters = new List<GridCharacter>();

        public void Init()
        {
            stateManagerObject = Instantiate(_stateManagerPrefab) as GameObject;
            stateManager = stateManagerObject.GetComponent<StateManager>();
            stateManager.playerHolder = this;
        }

        public void RegisterCharacter(GridCharacter character)
        {
                characters.Add(character);
        }

        public void UnRegisterCharacter(GridCharacter character)
        {
            if(characters.Contains(character))
            {
                characters.Remove(character);
            }
        }
    }
}
