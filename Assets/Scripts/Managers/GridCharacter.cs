using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV 
{
    public class GridCharacter : MonoBehaviour, ISelectable, IDeselect, IHighlight, IDeHighlight, IDetectable
    {
        public PlayerHolder owner;

        public GameObject highlighter;
        public bool isSelected;

        public float walkSpeed = 1.5f;
        public float crouchSpeed = 1.0f;
        public float runSpeed = 5f;
        public float rotateSpeed = 5f;
        public bool isCrouched;
        public bool isRunning;

        [HideInInspector]
        public Node currentNode;
        [HideInInspector]
        public List<Node> currentPath;

        public Animator animator;

        public float GetSpeed()
        {
            float r = walkSpeed;
            if (isCrouched)
            {
                r = crouchSpeed;
            }
            if (isRunning)
            {
                r = runSpeed;
            }
            return r;
        }
        
        #region Stance Handling
        public void SetCrouch()
        {
            ResetStance();
            isCrouched = true;
        }

        public void SetRun()
        {
            ResetStance();
            isRunning = true;
        }
        public void ResetStance()
        {
            isRunning = false;
            isCrouched = false;
        }
        #endregion
        
        public void LoadPath(List<Node> path)
        {
            currentPath = path;
        }

        public void OnInit()
        {
            owner.RegisterCharacter(this);
            highlighter.SetActive(false);
            animator = GetComponentInChildren<Animator>();
            animator.applyRootMotion = false;
        }

        #region Animations
        public void PlayMovementAnimation()
        {
            /*
            if (isCrouched)
            {
                animator.CrossFade("Crouch Movement", 0.2f);
            } Not using this in my game */
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
            if (isCrouched) 
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

        public void OnDeHighlight(PlayerHolder player)
        {
            if (!isSelected)
            {
                highlighter.SetActive(false);
            }
        }

        public Node OnDetect() 
        {
            return currentNode;
        }
        #endregion
    }
}

