using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV 
{
    public class GridCharacter : MonoBehaviour, ISelectable, IDeselect, IHighlight, IDeHighlight, IDetectable, IHittable
    {
        public PlayerHolder owner;
        public Characters.Character character; // Character is in the EV.Characters namespace

        public GameObject highlighter;
        public bool isSelected;

        private int _actionPoints;
        public float walkSpeed = 1.5f;
        public float crouchSpeed = 1.0f;
        public float runSpeed = 5f;
        public float rotateSpeed = 5f;
        public bool isBraced;
        public bool isRunning;

        [HideInInspector]
        public Node currentNode;
        [HideInInspector]
        public List<Node> currentPath;

        public Animator animator;

        public float GetSpeed()
        {
            float r = walkSpeed;
            if (isBraced)
            {
                r = crouchSpeed;
            }
            if (isRunning)
            {
                r = runSpeed;
            }
            return r;
        }

        public int ActionPoints
        {
            get { return _actionPoints; }
            set { _actionPoints = value; }
        }
    
        
        public void LoadPath(List<Node> path)
        {
            currentPath = path;
        }

        public void OnStartTurn()
        {
            _actionPoints = character.StartingAP;
            SetRun();
        }

        // initialize Character: 1- register this character with the PlayerHolder. 2- Set the player highlighter to false. 3- get the animator component from the child 4- Disable root motion
        public void OnInit()
        {
            owner.RegisterCharacter(this);
            highlighter.SetActive(false);
            animator = GetComponentInChildren<Animator>();
            animator.applyRootMotion = false;
        }

        #region Stance Handling
        public void SetBrace()
        {
            ResetStance();
            isBraced = true;
            PlayAnimation("Idle Crouch"); // change this to a more 'defensive' posture
        }

        public void SetRun()
        {
            ResetStance();
            isRunning = true;
        }
        public void ResetStance()
        {
            isRunning = false;
            isBraced = false;
        }
        #endregion

        #region Animations
        public void PlayMovementAnimation()
        {
            if (isRunning)
            {
                PlayAnimation("Run");
            }
            else
            {
                PlayAnimation("Movement");
            }
        }

        public void PlayIdleAnimation()
        {
            if (isBraced) 
            {
                PlayAnimation("Idle Crouch");
            }
            else
            {
                PlayAnimation("Idle");
            }
        }

        public void PlayAnimation(string targetAnim)
        {
            animator.CrossFade(targetAnim, 0.1f);
        }
        #endregion

        #region Interfaces
        public void OnSelect(PlayerHolder player)
        {
            highlighter.SetActive(true);
            isRunning = true; // set default movement to running
            isSelected = true;
            player.stateManager.currentCharacter = this;
        }

        public void OnDeselect(PlayerHolder player)
        {
            isSelected = false;
            highlighter.SetActive(false);
        }

        public void OnHighlight(PlayerHolder player)
        {
            highlighter.SetActive(true);
        }

        public void OnDeHighlight(PlayerHolder player, bool endTurnButton)
        {
            if (!isSelected || endTurnButton == true)
            {
                highlighter.SetActive(false);
            }
        }

        public Node OnRaycastHit() 
        {
            return currentNode;
        }

        public void OnHit(GridCharacter character)
        {
            PlayAnimation("Death");
        }
        #endregion
    }
}

