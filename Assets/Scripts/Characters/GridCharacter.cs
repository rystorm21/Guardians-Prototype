using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EV 
{
    public class GridCharacter : MonoBehaviour, ISelectable, IDeselect, IHighlight, IDeHighlight, IDetectable, IHittable
    {
        public PlayerHolder owner;
        public Characters.Character character; // Character is in the EV.Characters namespace

        public GameObject highlighter;
        public GameObject bladeR;
        public GameObject bladeL;
        public GameObject braceShield;
        public string teamName;
        public Text accuracyText;
        public bool isSelected;

        private int _actionPoints;
        public float walkSpeed = 1.5f;
        public float crouchSpeed = 1.0f;
        public float runSpeed = 5f;
        public float rotateSpeed = 5f;
        public bool isBraced;
        public bool isRunning;

        public bool isCurrentlyMoving;

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
            accuracyText.gameObject.SetActive(false);
            SetRun();
            PlayIdleAnimation();
        }

        // initialize Character: 1- register this character with the PlayerHolder. 2- Set the player highlighter to false. 3- get the animator component from the child 4- Disable root motion
        public void OnInit()
        {
            accuracyText = GameObject.Find("AccuracyText").GetComponent<Text>();
            bladeL = GameObject.Find(character.characterName + "/Blade.L");
            bladeR = GameObject.Find(character.characterName + "/Blade.R");
            braceShield = GameObject.Find("Shield");
            owner.RegisterCharacter(this);
            animator = GetComponentInChildren<Animator>();
            animator.applyRootMotion = false;
            highlighter.SetActive(false);
            braceShield.SetActive(false);
            MeleeActivation(false);
            character.weaponSelected = 0;
            teamName = owner.name;
        }

        #region Stance Handling
        public void SetBrace()
        {
            ResetStance();
            BraceActivation(true);
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
            BraceActivation(false);
        }

        public void MeleeActivation(bool activate)
        {
            if (bladeL && bladeR)
            {
                bladeL.SetActive(activate);
                bladeR.SetActive(activate);
            }
        }

        public void BraceActivation(bool activate)
        {
            isBraced = activate;
            braceShield.SetActive(activate);
        }
        #endregion

        #region Animations
        public void PlaySelectMeleeWeapon()
        {
            if (!isBraced)
                PlayAnimation("SelectMeleeWeapon");
            MeleeActivation(true);
        }
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
            if (character.weaponSelected == 1)
            {
                PlayAnimation("MeleeIdle");
                MeleeActivation(true);
            }
            else if (isBraced)
            {
                PlayAnimation("Idle Crouch");
                MeleeActivation(false);
                BraceActivation(true);
                return;
            }
            else
            {
                PlayAnimation("Idle");
                MeleeActivation(false);
            }
        }

        public void PlayAnimation(string targetAnim)
        {
            if (targetAnim == "SelectMeleeWeapon" || targetAnim == "Death")
                animator.Play(targetAnim);
            else
                animator.CrossFade(targetAnim, 0.1f);
        }
        #endregion

        #region Interfaces
        public void OnSelect(PlayerHolder player)
        {
            highlighter.SetActive(true);
            accuracyText.gameObject.SetActive(false);
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
            //PlayAnimation("Death"); // old implementation (worked fine unless all ap was used with this attack)
        }
        #endregion
    }
}

