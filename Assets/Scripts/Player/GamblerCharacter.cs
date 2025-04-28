using UnityEngine;
using System.Collections;  // For non-generic IEnumerator
using System.Collections.Generic;
using System;
using PDXUnderground.Core;
using PDXUnderground.Core.Interfaces;

namespace PDXUnderground.Player
{
    /// <summary>
    /// Main player character class that implements the buzz and card systems
    /// </summary>
    public class GamblerCharacter : MonoBehaviour, IGamblerCharacter
    {
        #region Inspector Fields
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;
        
        [Header("Buzz System")]
        [SerializeField] private float maxBuzzLevel = 100f;
        [SerializeField] private float currentBuzzLevel = 75f;
        [SerializeField] private IBuzzSystem.BuzzState currentBuzzState = IBuzzSystem.BuzzState.Normal;
        [SerializeField] private float lowBuzzThreshold = 0.3f;
        [SerializeField] private float criticalBuzzThreshold = 0.1f;
        
        [Header("Card System")]
        [SerializeField] private int maxHandSize = 5;
        [SerializeField] private float cardDrawCooldown = 2f;
        #endregion
        
        #region Private Fields
        private List<ICardSystem.Card> currentHand = new List<ICardSystem.Card>();
        private Dictionary<string, float> abilityCooldowns = new Dictionary<string, float>();
        #endregion
        
        #region Events
        // IBuzzSystem Events
        public event Action<float, float> OnBuzzChanged;
        public event Action<IBuzzSystem.BuzzState> OnBuzzStateChanged;
        
        // ICardSystem Events
        public event Action<ICardSystem.Card> OnCardUsed;
        public event Action<ICardSystem.Card> OnCardDrawn;
        public event Action<List<ICardSystem.Card>> OnHandChanged;
        
        // IGamblerCharacter Events
        public event Action<Vector3> OnCriticalHit;
        public event Action<string, float> OnAbilityUsed;
        
        // Additional character events
        public event Action<float, float> OnHealthChanged;
        #endregion
        
        #region Unity Lifecycle Methods
        private void Awake()
        {
            // Initialize state
            UpdateBuzzState();
        }
        
        private void Start()
        {
            // Initial draw of cards
            while (currentHand.Count < maxHandSize)
            {
                DrawRandomCard();
            }
            
            // Notify UI of initial state
            OnBuzzChanged?.Invoke(currentBuzzLevel, maxBuzzLevel);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnBuzzStateChanged?.Invoke(currentBuzzState);
            OnHandChanged?.Invoke(GetCurrentHand());
        }
        
        private void Update()
        {
            // Update ability cooldowns
            UpdateCooldowns();
        }
        #endregion
        
        #region IBuzzSystem Implementation
        public float MaxBuzz => maxBuzzLevel;
        public float CurrentBuzz => currentBuzzLevel;
        public IBuzzSystem.BuzzState CurrentBuzzState => currentBuzzState;
        
        public void SetBuzz(float level)
        {
            float oldLevel = currentBuzzLevel;
            currentBuzzLevel = Mathf.Clamp(level, 0f, maxBuzzLevel);
            
            if (oldLevel != currentBuzzLevel)
            {
                OnBuzzChanged?.Invoke(currentBuzzLevel, maxBuzzLevel);
                UpdateBuzzState();
            }
        }
        
        public void AdjustBuzz(float amount)
        {
            SetBuzz(currentBuzzLevel + amount);
        }
        
        public void UpdateBuzz(float amount)
        {
            AdjustBuzz(amount);
            // Trigger any additional combat-specific buzz effects if needed
            if (currentBuzzState == IBuzzSystem.BuzzState.Critical)
            {
                // Notify of critical state during combat
                OnBuzzStateChanged?.Invoke(currentBuzzState);
            }
        }
        
        private void UpdateBuzzState()
        {
            IBuzzSystem.BuzzState newState;
            float buzzPercentage = currentBuzzLevel / maxBuzzLevel;
            
            if (buzzPercentage <= criticalBuzzThreshold)
            {
                newState = IBuzzSystem.BuzzState.Critical;
            }
            else if (buzzPercentage <= lowBuzzThreshold)
            {
                newState = IBuzzSystem.BuzzState.Low;
            }
            else
            {
                newState = IBuzzSystem.BuzzState.Normal;
            }
            if (newState != currentBuzzState)
            {
                currentBuzzState = newState;
                OnBuzzStateChanged?.Invoke(currentBuzzState);
            }
        }
        #endregion
        
        #region ICardSystem Implementation
        public void DrawCard(ICardSystem.Card card)
        {
            if (currentHand.Count < maxHandSize)
            {
                currentHand.Add(card);
                OnCardDrawn?.Invoke(card);
                OnHandChanged?.Invoke(GetCurrentHand());
            }
        }
        
        public void UseCard(ICardSystem.Card card)
        {
            if (currentHand.Contains(card))
            {
                // Check if we have enough buzz to use this card
                if (currentBuzzLevel >= card.energyCost)
                {
                    // Use the card - modify buzz level
                    // Use the card - modify buzz level
                    AdjustBuzz(-card.energyCost);
                    // Set cooldown
                    abilityCooldowns[card.name] = card.cooldown;
                    
                    // Check for critical hit effect
                    bool isCritical = UnityEngine.Random.value < 0.2f; // 20% chance for critical
                    if (isCritical && card.type == ICardSystem.CardType.Attack)
                    {
                        OnCriticalHit?.Invoke(transform.position);
                    }
                    
                    // Remove card from hand
                    currentHand.Remove(card);
                    
                    
                    // Notify of card use
                    OnCardUsed?.Invoke(card);
                    OnAbilityUsed?.Invoke(card.name, card.cooldown);
                }
            }
        }
        
        /// <summary>
        /// Use the card at the specified index in the current hand
        /// </summary>
        /// <param name="cardIndex">Index of the card to use</param>
        public void UseCard(int cardIndex)
        {
            if (cardIndex >= 0 && cardIndex < currentHand.Count)
            {
                UseCard(currentHand[cardIndex]);
            }
        }
        
        public List<ICardSystem.Card> GetCurrentHand()
        {
            return new List<ICardSystem.Card>(currentHand);
        }
        
        private void DrawRandomCard()
        {
            // Check if we're on cooldown
            if (abilityCooldowns.TryGetValue("DrawCard", out float cooldown) && cooldown > 0)
            {
                Debug.Log($"Card draw on cooldown. Remaining: {cooldown:F1} seconds");
                return;
            }

            // Set the cooldown
            abilityCooldowns["DrawCard"] = cardDrawCooldown;

            // In a real implementation, this would draw from a deck
            // For now, we'll just create a random card
            ICardSystem.Card newCard = GenerateRandomCard();
            DrawCard(newCard);
        }
        
        private ICardSystem.Card GenerateRandomCard()
        {
            string[] cardNames = {"Slash", "Block", "Heal", "Special"};
            string[] descriptions = {
                "Deal damage to an enemy",
                "Block incoming damage",
                "Recover buzz level",
                "Special ability"
            };
            
            int cardType = UnityEngine.Random.Range(0, 4);
            float energy = UnityEngine.Random.Range(5f, 25f);
            float cooldown = UnityEngine.Random.Range(1f, 5f);
            
            ICardSystem.Card card = new ICardSystem.Card
            {
                name = cardNames[cardType],
                description = descriptions[cardType],
                type = (ICardSystem.CardType)cardType,
                energyCost = energy,
                cooldown = cooldown,
                damage = UnityEngine.Random.Range(1, 10),
                suit = UnityEngine.Random.Range(0, 4),
                rank = UnityEngine.Random.Range(1, 13),
                specialEffect = ICardSystem.SpecialEffect.None
            };
            return card;
        }
        #endregion
        
        #region Health System
        /// <summary>
        /// Get the maximum health of the character
        /// </summary>
        /// <returns>Maximum health value</returns>
        public float GetMaxHealth()
        {
            return maxHealth;
        }
        
        public float GetCurrentHealth()
        {
            return currentHealth;
        }
        
        /// <summary>
        /// Set the character's health to a specific value, clamped to max health
        /// </summary>
        /// <param name="health">New health value</param>
        public void SetHealth(float health)
        {
            float oldHealth = currentHealth;
            currentHealth = Mathf.Clamp(health, 0f, maxHealth);
            
            if (oldHealth != currentHealth)
            {
                OnHealthChanged?.Invoke(currentHealth, maxHealth);
            }
        }
        
        /// <summary>
        /// Modify the character's health by the specified amount
        /// </summary>
        /// <param name="amount">Amount to adjust (positive or negative)</param>
        public void ModifyHealth(float amount)
        {
            SetHealth(currentHealth + amount);
        }

        /// <summary>
        /// Heal the character by the specified amount and return the new health value
        /// </summary>
        public float Heal(float amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning("Heal was called with a negative value. Use ModifyHealth for damage.");
                return currentHealth;
            }
            
            ModifyHealth(amount);
            return currentHealth;
        }
        
        /// <summary>
        /// Check if the character is dead (health <= 0)
        /// </summary>
        public bool IsDead()
        {
            return currentHealth <= 0f;
        }
        
        /// <summary>
        /// Apply a stun effect for the specified duration
        /// </summary>
        public void ApplyStun(float duration)
        {
            // Here we would typically add a status effect component or set a timer
            // For now we'll just invoke an event and log this
            Debug.Log($"Character stunned for {duration} seconds");
            
            // In a real implementation, we might disable character controls
            // StartCoroutine(StunCoroutine(duration));
        }
        
        /// <summary>
        /// Coroutine to handle stun effect
        /// </summary>
        private IEnumerator StunCoroutine(float duration)
        {
            // Disable character controls here
            
            yield return new WaitForSeconds(duration);
            
            // Re-enable character controls here
        }
        #endregion
        
        #region Utility Methods
        /// <summary>
        /// Get the current buzz level as a percentage of max buzz
        /// </summary>
        /// <returns>Buzz percentage (0-1)</returns>
        public float GetCurrentBuzzPercentage()
        {
            return currentBuzzLevel / maxBuzzLevel;
        }
        
        /// <summary>
        /// Get the maximum buzz level
        /// </summary>
        /// <returns>Maximum buzz value</returns>
        public float GetMaxBuzz()
        {
            return maxBuzzLevel;
        }

        /// <summary>
        /// Gets the current buzz state of the character
        /// </summary>
        /// <returns>Current buzz state</returns>
        public IBuzzSystem.BuzzState GetCurrentBuzzState()
        {
            return currentBuzzState;
        }

        /// <summary>
        /// Gets the cooldown remaining for a specific ability
        /// </summary>
        /// <param name="abilityName">Name of the ability to check</param>
        /// <returns>Remaining cooldown time, 0 if not on cooldown</returns>
        public float GetAbilityCooldown(string abilityName)
        {
            if (abilityCooldowns.TryGetValue(abilityName, out float cooldown))
            {
                return cooldown;
            }
            return 0f;
        }
        
        /// <summary>
        /// Update all ability cooldowns based on elapsed time
        /// </summary>
        private void UpdateCooldowns()
        {
            List<string> completedCooldowns = new List<string>();
            
            foreach (var ability in abilityCooldowns)
            {
                float newCooldown = ability.Value - Time.deltaTime;
                
                if (newCooldown <= 0)
                {
                    completedCooldowns.Add(ability.Key);
                }
                else
                {
                    abilityCooldowns[ability.Key] = newCooldown;
                }
            }
            
            // Remove completed cooldowns
            foreach (string ability in completedCooldowns)
            {
                abilityCooldowns.Remove(ability);
            }
        }
        #endregion
    }
}
