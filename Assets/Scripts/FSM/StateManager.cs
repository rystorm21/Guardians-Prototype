using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV 
{
    public abstract class StateManager : MonoBehaviour
    {
        public State currentState;
        public State startingState;
        public bool forceExit;

        public Node currentNode;
        public Node prevNode;
        public float delta;

        public PlayerHolder playerHolder;
        GridCharacter _currentCharacter;
        public GridCharacter CurrentCharacter
        {
            get { return _currentCharacter; }
            set 
            { 
                if (CurrentCharacter != null) _currentCharacter.OnDeselect(playerHolder);
                _currentCharacter = value;
            }
        }

        protected Dictionary<string, State> allStates = new Dictionary<string, State>();

        private void Start() {
            Init();
        }

        public abstract void Init();

        // if currentState is null, run State(tick) passing this, sessionmanager, and turn
        public void Tick(SessionManager sessionManager, Turn turn) 
        {
            delta = sessionManager.delta;

            if (currentState != null)
            {
                currentState.Tick(this, sessionManager, turn);
            }
            forceExit = false;
        }

        public void SetState(string id)
        {
            State targetState = GetState(id);
            if (targetState == null)
            {
                Debug.LogError("State with id : " + id + " cannot be found! Check your states and ids!");
            }

            // Debug.Log("changed state to " + id);
            currentState = targetState;
        }

        public void SetStartingState() 
        {
            // Debug.Log("changed state to starting state");
            currentState = startingState;
            if (SessionManager.currentGameState == GameState.Noncombat)
            {
                CurrentCharacter.ActionPoints = CurrentCharacter.character.NonCombatAPCap;
            }
        }

        State GetState(string id)
        {
            State result = null;
            allStates.TryGetValue(id, out result);
            return result;
        }
    }
}

