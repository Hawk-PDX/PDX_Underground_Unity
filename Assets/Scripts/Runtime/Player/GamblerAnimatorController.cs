using UnityEngine;
using PDXUnderground.Combat;
using PDXUnderground.Core.Interfaces;

namespace PDXUnderground.Player
{
    /// <summary>
    /// Controls animations for the Gambler character
    /// Handles triggering ability animations and responding to combat/card actions
    /// </summary>
    public class GamblerAnimatorController : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private GamblerCharacter gamblerCharacter;
        
        // Animation parameter names
        private const string TRIGGER_FLICK = "Flick";
        private const string TRIGGER_SLASH = "Slash";
        private const string TRIGGER_SLASH1 = "Slash1";
        private const string TRIGGER_SLASH2 = "Slash2";
        private const string TRIGGER_SLASH3 = "Slash3";
        private const string TRIGGER_TAKE_DAMAGE = "TakeDamage";
        private const string TRIGGER_CRITICAL = "Critical";
        private const string TRIGGER_DRAW_CARD = "DrawCard";
        private const string TRIGGER_PLAY_CARD = "PlayCard";
        
        private const string BOOL_IS_CRITICAL = "IsCritical";
        private const string FLOAT_BUZZ_LEVEL = "BuzzLevel";
        
        /// <summary>
        /// Initialize references and event subscriptions
        /// </summary>
        private void Awake()
        {
            // Get references
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
            
            if (gamblerCharacter == null)
            {
                gamblerCharacter = GetComponent<GamblerCharacter>();
            }
            
            // Verify we have required components
            if (animator == null)
            {
                Debug.LogError("GamblerAnimatorController requires an Animator component");
                enabled = false;
                return;
            }
            
            if (gamblerCharacter == null)
            {
                Debug.LogError("GamblerAnimatorController requires a GamblerCharacter component");
                enabled = false;
                return;
            }
        }
        
        /// <summary>
        /// Register for character events
        /// </summary>
        private void OnEnable()
        {
            if (gamblerCharacter != null)
            {
                gamblerCharacter.OnBuzzLevelChanged += HandleBuzzLevelChanged;
                gamblerCharacter.OnCharacterStateChanged += HandleCharacterStateChanged;
                gamblerCharacter.OnCardDrawn += HandleCardDrawn;
                gamblerCharacter.OnCardPlayed += HandleCardPlayed;
            }
        }
        
        /// <summary>
        /// Unregister from events
        /// </summary>
        private void OnDisable()
        {
            if (gamblerCharacter != null)
            {
                gamblerCharacter.OnBuzzLevelChanged -= HandleBuzzLevelChanged;
                gamblerCharacter.OnCharacterStateChanged -= HandleCharacterStateChanged;
                gamblerCharacter.OnCardDrawn -= HandleCardDrawn;
                gamblerCharacter.OnCardPlayed -= HandleCardPlayed;
            }
        }
        
        /// <summary>
        /// Handle buzz level changes
        /// </summary>
        private void HandleBuzzLevelChanged(float normalizedBuzzLevel)
        {
            if (animator != null)
            {
                animator.SetFloat(FLOAT_BUZZ_LEVEL, normalizedBuzzLevel);
                animator.SetBool(BOOL_IS_CRITICAL, normalizedBuzzLevel <= 0.3f); // 30% threshold for critical state
            }
        }
        
        /// <summary>
        /// Handle character state changes
        /// </summary>
        private void HandleCharacterStateChanged(IBuzzSystem.BuzzState newState)
        {
            if (animator != null)
            {
                if (newState == IBuzzSystem.BuzzState.Critical)
                {
                    animator.SetTrigger(TRIGGER_CRITICAL);
                }
            }
        }
        
        /// <summary>
        /// Handle card drawn event
        /// </summary>
    private void HandleCardDrawn(ICardSystem.Card card)
        {
            if (animator != null)
            {
                animator.SetTrigger(TRIGGER_DRAW_CARD);
            }
        }
        
        /// <summary>
        /// Handle card played event
        /// </summary>
        private void HandleCardPlayed(ICardSystem.Card card)
        {
            if (animator != null)
            {
                animator.SetTrigger(TRIGGER_PLAY_CARD);
                
                // Trigger ability-specific animations based on card name
                if (card.name.ToLower().Contains("flick") || card.name.ToLower().Contains("throw"))
                {
                    animator.SetTrigger(TRIGGER_FLICK);
                }
                else if (card.name.ToLower().Contains("slash") || card.name.ToLower().Contains("slice"))
                {
                    animator.SetTrigger(TRIGGER_SLASH);
                }
            }
        }
        
        /// <summary>
        /// Trigger Flick animation
        /// </summary>
        public void TriggerFlickAnimation()
        {
            if (animator != null)
            {
                animator.SetTrigger(TRIGGER_FLICK);
            }
        }
        
        /// <summary>
        /// Trigger Slash animation
        /// </summary>
        /// <param name="comboIndex">Index in the combo (0, 1, or 2)</param>
        public void TriggerSlashAnimation(int comboIndex = 0)
        {
            if (animator != null)
            {
                switch (comboIndex)
                {
                    case 0:
                        animator.SetTrigger(TRIGGER_SLASH1);
                        break;
                    case 1:
                        animator.SetTrigger(TRIGGER_SLASH2);
                        break;
                    case 2:
                        animator.SetTrigger(TRIGGER_SLASH3);
                        break;
                    default:
                        animator.SetTrigger(TRIGGER_SLASH);
                        break;
                }
            }
        }
        
        /// <summary>
        /// Trigger take damage animation
        /// </summary>
        public void TriggerTakeDamageAnimation()
        {
            if (animator != null)
            {
                animator.SetTrigger(TRIGGER_TAKE_DAMAGE);
            }
        }
    }
}

