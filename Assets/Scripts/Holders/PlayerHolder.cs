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

        public bool isLocalPlayer;

        [System.NonSerialized]
        public List<GridCharacter> characters = new List<GridCharacter>();

        // Instantiate Player States Manager - Prefab Object in Data/Prefabs
        //   Get component <StateManager>
        //   Set the PlayerHolder on the StateManager to this PlayerHolder
        // This basically creates the list of players that we have in our party
        public void Init()
        {
            // if (characters.Count > 0)
            //     return;
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
                Debug.Log("Unregistered Character " + character.name);
                characters.Remove(character);
            }
        }
    }
}
