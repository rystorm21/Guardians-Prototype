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

        [System.NonSerialized]
        public GameObject highlighter;
        GameObject bladeR;
        GameObject bladeL;
        GameObject braceShield;
        public string teamName;
        public Text accuracyText;

        private int _actionPoints;
        public float walkSpeed = 1.5f;
        public float crouchSpeed = 1.0f;
        public float runSpeed = 5f;
        public float rotateSpeed = 5f;
        public bool isSelected;
        public bool isBraced;
        public bool isRunning;

        private bool isFirstTurn;

        [HideInInspector]
        public Node currentNode;
        [HideInInspector]
        public List<Node> currentPath;

        Animator animator;

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

        #region Init
        // initialize Character: 1- register this character with the PlayerHolder. 2- Set the player highlighter to false. 3- get the animator component from the child 4- Disable root motion
        public void OnInit()
        {
            isFirstTurn = true;
            character.ClearAllStatus();
            character.NonCombatAPCap = 100;
            accuracyText = this.transform.GetChild(0).gameObject.transform.GetChild(0).transform.GetChild(0).GetComponent<Text>(); // playerhighlight must be first child
            highlighter = this.transform.GetChild(0).gameObject;
            braceShield = this.transform.GetChild(1).gameObject;
            bladeL = GameObject.Find(character.characterName + "/Blade.L");
            bladeR = GameObject.Find(character.characterName + "/Blade.R");
            owner.RegisterCharacter(this);
            animator = GetComponentInChildren<Animator>();
            animator.applyRootMotion = false;
            animator.SetInteger("CurrentStance", idleStance);
            highlighter.SetActive(false);
            braceShield.SetActive(false);
            MeleeActivation(false);
            character.weaponSelected = 0;
            teamName = owner.name;
            character.maxHitPoints = SetHitPoints(character.characterArchetype); // just for testing purposes
            character.hitPoints = character.maxHitPoints;
            character.KO = false;
            character.teamLeader = false;
            if (owner.characters[0].gameObject == this.gameObject)
                character.teamLeader = true;
        }

        public int SetHitPoints(int archetype)
        {
            int hitPoints;
            int blaster = 40;
            int defender = 45;
            int tanker = 60;
            int minion = 1;
            int lieutenant = 40;
            int boss = 100;

            switch (archetype)
            {
                case 0:
                    hitPoints = defender;
                    break;

                case 1:
                    hitPoints = blaster;
                    break;

                case 2:
                    hitPoints = tanker;
                    break;
                
                case 3:
                    hitPoints = minion;
                    break;
                    
                case 4: 
                    hitPoints = lieutenant;
                    break;

                case 5: 
                    hitPoints = boss;
                    break;
                default:
                    hitPoints = 0;
                    break;
            }

            return hitPoints;
        }

        public void LoadPath(List<Node> path)
        {
            currentPath = path;
        }

        public void OnStartTurn(SessionManager sessionManager)
        {
            // this.character.covered = this.character.IsCovered(sessionManager); this doesn't work for some reason, marks all as covered at start.
            if (!character.KO)
            {
                _actionPoints = character.StartingAP;
                accuracyText.gameObject.SetActive(false);
                this.highlighter.SetActive(false);
                this.character.abilityInUse = null;
                SetRun();
                if (character.hitPoints > 0)
                {
                    if (AttackAction.diceRoll < 0) // preventing the idle animation from interrupting the hit animation
                        PlayIdleAnimation();
                    else
                    {
                        AttackAction.attackInProgress = true;
                        StartCoroutine("DelayStart");
                    }
                }
            }
            else // if character is knocked out
            {
                _actionPoints = 0;
            }
            if (SessionManager.currentGameState == GameState.Noncombat)
            {
                ActionPoints = character.NonCombatAPCap;
            }
            if (isFirstTurn)
            {
                if (character.teamLeader == true)
                    OnSelect(owner);
                isFirstTurn = false;
            }
            SpecialAbilityAction.buffAbilitySelected = false;
            character.CycleStatus();
            character.ApplyStatus();
        }

        #endregion

        #region Game State Events
        public void Death()
        {
            character.KO = true;
            if (owner.name == "Enemy")
            {
                StartCoroutine(EnemyKilled(this));
            }
            if (AllTeamDead())
            {
                //Exit Combat State
                if (owner.name == "Player1")
                    SessionManager.currentGameState = GameState.GameOver;
                if (owner.name == "Enemy")
                {
                    SessionManager.currentGameState = GameState.Noncombat;
                    SessionManager.combatVictory = true;
                }
            }
        }

        bool AllTeamDead()
        {
            bool allDead = true;
            
            foreach (GridCharacter character in owner.characters)
            {
                if (!character.character.KO)
                    allDead = false;
            }
            return allDead;
        }

        #endregion

        #region Stance Handling
        const int idleStance = 1;
        const int meleeStance = 2;
        const int braceStance = 3;
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
            if (character.weaponSelected == 0)
                character.currentStance = 1;
            if (character.weaponSelected == 1)
                character.currentStance = 2;
            animator.SetInteger("CurrentStance", character.currentStance);
        }

        public void MeleeActivation(bool activate)
        {
            if (bladeL && bladeR)
            {
                bladeL.SetActive(activate);
                bladeR.SetActive(activate);
            }
            if (activate)
                character.currentStance = meleeStance;
            else
                character.currentStance = idleStance;

            animator.SetInteger("CurrentStance", character.currentStance);
        }

        public void BraceActivation(bool activate)
        {
            isBraced = activate;
            braceShield.SetActive(activate);
            if (isBraced)
            {
                character.currentStance = braceStance;
                animator.SetInteger("CurrentStance", character.currentStance);
                character.braced = true;
            }
            else
            {
                character.braced = false;
            }
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
            {
                if (AttackAction.hitByMelee && targetAnim == "Death")
                {
                    StartCoroutine("MeleeDelay");
                }
                else {
                    animator.Play(targetAnim);
                }
            }
            else
                animator.CrossFade(targetAnim, 0.1f);
            
            if (targetAnim.Contains("Attack"))
                StartCoroutine("DelayAttack");
        }
        #endregion

        #region Situational Co-Routines
        IEnumerator EnemyKilled(GridCharacter character)
        {
            bool teamLeaderDown = false;

            if (character.character.teamLeader)
            {
                character.character.teamLeader = false;
                teamLeaderDown = true;
            }
            // Changes team leader if enemy team leader dies. Prevents the turn from getting 'locked' if enemy leader dies before other enemies are selected
            if (teamLeaderDown)
            {
                foreach (GridCharacter newLeader in owner.characters)
                {
                    if (!newLeader.character.KO)
                    {
                        newLeader.character.teamLeader = true;
                        teamLeaderDown = false;
                        break;
                    }
                }
            }
            yield return new WaitForSeconds(1.5f);
            owner.UnRegisterCharacter(this);
            this.currentNode.isWalkable = true;
            this.currentNode.character = null;
            this.gameObject.SetActive(false);
        }

        IEnumerator DelayStart()
        {
            yield return new WaitForSeconds(2);
            PlayIdleAnimation();
            AttackAction.attackInProgress = false;
        }

        IEnumerator DelayAttack()
        {
            yield return new WaitForSeconds(2);
            AttackAction.attackInProgress = false;
        }

        IEnumerator MeleeDelay()
        {
            yield return new WaitForSeconds(.5f);
            animator.Play("Death");
            AttackAction.hitByMelee = false;
        }
        #endregion

        #region Interfaces
        public void OnSelect(PlayerHolder player)
        {
            highlighter.SetActive(true);
            accuracyText.gameObject.SetActive(false);
            isRunning = true; // set default movement to running
            isSelected = true;
            player.stateManager.CurrentCharacter = this;
        }

        public void OnDeselect(PlayerHolder player)
        {
            isSelected = false;
            highlighter.SetActive(false);
        }

        public void OnHighlight(PlayerHolder player)
        {
            if (SessionManager.currentGameState == GameState.Combat)
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

