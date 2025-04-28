using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using PDXUnderground.Core;
using PDXUnderground.Core.Interfaces;
using PDXUnderground.Models;
using PDXUnderground.Combat;
using PDXUnderground.UI;

namespace PDXUnderground.Player
{
    /// <summary>
    /// Main character class for the PDX Underground game.
    /// Handles character stats, buzz level, card system, and character state.
    /// </summary>
    public class GamblerCharacter : MonoBehaviour, IDamageable, IBuzzSystem, ICardSystem
    {
        // ICardSystem Events
        public event Action<ICard> OnCardUsed;
        public event Action<ICard> OnCardDrawn;
        public event Action<ICard> OnCardDiscarded;
        public event Action<string, float> OnAbilityUsed;
        public event Action<List<ICard>> OnHandChanged;
        public event Action<Vector3> OnCriticalHit;

        // ICardSystem Properties
        ICard[] ICardSystem.CurrentHand => hand.ToArray();
        ICard[] ICardSystem.Deck => deck.ToArray();
        int ICardSystem.MaxHandSize => maxHandSize;
        
        #region ICardSystem Implementation
        
        /// <summary>
        /// Uses the specified card
        /// </summary>
        /// <param name="card">Card to use</param>
        public void UseCard(ICard card)
        {
            if (!CanUseCard(card)) return;

            UseCardEffect(card);
            hand.Remove(card);
            discardPile.Add(card);
            
            OnCardUsed?.Invoke(card);
            OnHandChanged?.Invoke(hand);
        }

        public void DiscardCard(ICard card)
        {
            if (hand.Contains(card))
            {
                hand.Remove(card);
                discardPile.Add(card);
                OnCardDiscarded?.Invoke(card);
                OnHandChanged?.Invoke(hand);
            }
        }

        public void UseCard(ICard card, Vector3 direction)
        {
            if (!CanUseCard(card)) return;

            UseCardEffect(card, direction);
            hand.Remove(card);
            discardPile.Add(card);
            
            OnCardUsed?.Invoke(card);
            OnHandChanged?.Invoke(hand);
        }

        /// <summary>
        /// Draws a card from the deck
        /// </summary>
        public void DrawCard()
        {
            // Check hand size
            if (hand.Count >= maxHandSize)
                return;

            // Reshuffle discard pile if deck is empty
            if (deck.Count == 0 && discardPile.Count > 0)
            {
                deck.AddRange(discardPile);
                discardPile.Clear();
                ShuffleDeck();
            }

            if (deck.Count == 0)
                return;

            ICard drawnCard = deck[0];
            deck.RemoveAt(0);
            hand.Add(drawnCard);
            // Trigger events
            OnCardDrawn?.Invoke(drawnCard);
            OnHandChanged?.Invoke(hand);
        }

        /// <summary>
        /// Shuffles the deck (interface implementation)
        /// </summary>
        public void ShuffleDeck()
        {
            // Fisher-Yates shuffle
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                ICard temp = deck[i];
                deck[i] = deck[j];
                deck[j] = temp;
            }
        }
        
        /// <summary>
        /// Uses a card at the specified index in the hand
        /// </summary>
        /// <inheritdoc/>
        bool ICardSystem.UseCard(int cardIndex) => UseCard(cardIndex, Vector3.zero);

        /// <inheritdoc/>
        bool ICardSystem.UseCard(int cardIndex, Vector3 direction)
        {
            if (cardIndex < 0 || cardIndex >= hand.Count || hand[cardIndex] == null)
                return false;

            ICard card = hand[cardIndex];
            if (!CanUseCard(card))
                return false;

            UseCardEffect(card, direction);
            hand.Remove(card);
            discardPile.Add(card);

            OnCardUsed?.Invoke(card);
            OnHandChanged?.Invoke(hand);
            return true;
        }

        /// <inheritdoc/>
        void ICardSystem.DrawCard() => DrawCard();

        /// <inheritdoc/>
        List<ICard> ICardSystem.GetCurrentHand() => GetCurrentHand();

        /// <inheritdoc/>
        float ICardSystem.GetAbilityCooldown(string abilityName) => GetAbilityCooldown(abilityName);

        /// <inheritdoc/>
        bool ICardSystem.CanUseCard(ICard card) => CanUseCard(card);

        /// <inheritdoc/>
        float ICardSystem.GetCardCooldown(ICard card) => GetCardCooldown(card);

        /// <inheritdoc/>
        bool ICardSystem.IsCardOnCooldown(ICard card) => IsCardOnCooldown(card);

        /// <summary>
        /// Applies the effect of the used card
        /// </summary>
        private void UseCardEffect(ICard card, Vector3? direction = null)
        {
            if (card == null)
            {
                Debug.LogWarning("Attempted to use null card");
                return;
            }

            // Track card use for synergies
            lastCardType = card.Type;
            cardsPlayedThisTurn++;

            // Consume energy with possible discounts
            float energyCost = card.EnergyCost;
            if (hasEnergyDiscount) 
            {
                energyCost *= 0.7f; // 30% discount
                hasEnergyDiscount = false;
            }
            currentEnergy = Mathf.Max(0, currentEnergy - Mathf.RoundToInt(energyCost));
            
            // Update last use time
            card.LastUseTime = Time.time;
            
            // Apply card effect based on type with balanced values
            switch (card.Type)
            {
                case CardType.Attack:
                    float damageMultiplier = 1f + (0.15f * Mathf.Min(cardsPlayedThisTurn-1, 3));
                    if (direction.HasValue && flickAbility != null)
                    {
                        flickAbility.damage = Mathf.RoundToInt(15 * damageMultiplier);
                        flickAbility.UseAbility();
                        OnAbilityUsed?.Invoke("Flick", flickAbility.cooldownTime);
                        
                        // Critical hit energy refund
                        if (UnityEngine.Random.value < criticalChance)
                        {
                            currentEnergy = Mathf.Min(maxEnergy, currentEnergy + 10);
                            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
                            OnCriticalHit?.Invoke(direction.Value);
                        }
                    }
                    else if (slashAbility != null) 
                    {
                        slashAbility.damage = Mathf.RoundToInt(20 * damageMultiplier);
                        slashAbility.UseAbility();
                        OnAbilityUsed?.Invoke("Slice", slashAbility.cooldownTime);
                    }
                    break;

                case CardType.Defense:
                    // Apply defense boost with improved duration
                    AddStatBoost("defense", (int)(defense * 0.4f), true, 8f, "DefenseCard");
                    OnAbilityUsed?.Invoke("Defense", 8f);
                    
                    // Enable energy discount for next attack
                    hasEnergyDiscount = true;
                    
                    // Damage reduction if played after attack
                    if (lastCardType == CardType.Attack)
                    {
                        AddStatBoost("damageReduction", 20, true, 5f, "DefenseChain");
                    }
                    break;
                    
                case CardType.Utility:
                    // Handle utility effects with scaling
                    HandleUtilityCard(card);
                    break;
                    
                case CardType.Special:
                    // Handle special card effects
                    HandleSpecialCard(card);
                    break;
            }
        }
            }
        }

        /// <summary>
        /// Initializes the deck with the provided cards
        /// </summary>
        /// <summary>
        /// Initializes the deck with the provided cards
        /// </summary>
        private void InitializeDeck(List<ICard> startingCards)
        {
            deck.Clear();
            if (startingCards == null) return;
            
            foreach (var card in startingCards)
            {
                if (card != null) 
                {
                    deck.Add(card);
                }
            }
        // Track card synergies
        private CardType lastCardType;
        private int cardsPlayedThisTurn;
        private bool hasEnergyDiscount;

        private void HandleUtilityCard(ICard card) 
        {
            switch (card.SpecialEffect)
            {
                case SpecialEffect.Heal:
                    float healAmount = 20 * card.EnergyCost; // 20 HP per energy point
                    currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
                    OnHealthChanged?.Invoke(currentHealth, maxHealth);
                    break;

                case SpecialEffect.EnergyBoost:
                    float energyGain = 15 * card.EnergyCost; // 15 energy per energy point
                    currentEnergy = Mathf.Min(maxEnergy, currentEnergy + energyGain);
                    OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
                    break;

                case SpecialEffect.None:
                    // Boost next card effect if chaining utilities
                    if (lastCardType == CardType.Utility)
                    {
                        if (card.EnergyCost > 15) // Only boost significant plays
                        {
                            AddStatBoost("damage", 10, true, 10f, "UtilityChain");
                        }
                    }
                    break;
            }
            OnAbilityUsed?.Invoke("Utility", card.Cooldown);
        }

        /// <summary>
        /// Handles special card effects
        /// </summary>
        /// <param name="card">Card containing the special effect</param>
        private void HandleSpecialCard(ICard card)
        {
            if (card == null) 
            {
                Debug.LogWarning("Attempted to handle null special card");
                return;
            }

            switch (card.SpecialEffect)
            {
                case SpecialEffect.Stun:
                    float stunDuration = 2.5f * (card.EnergyCost / 25f);
                    StartCoroutine(StunCoroutine(stunDuration));
                    OnAbilityUsed?.Invoke("Stun", stunDuration);
                    break;
                    
                case SpecialEffect.Burn:
                    float burnDamage = 5 * (card.EnergyCost / 25f);
                    AddStatBoost("burn", (int)burnDamage, false, 5f, "BurnEffect");
                    OnAbilityUsed?.Invoke("Burn", 5f);
                    break;

                case SpecialEffect.Chaos:
                    // Random effect
                    float randomEffect = UnityEngine.Random.value;
                    if (randomEffect < 0.33f) 
                    {
                        // Energy boost
                        currentEnergy = Mathf.Min(maxEnergy, currentEnergy + 15);
                        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
                    }
                    else if (randomEffect < 0.66f)
                    {
                        // Health boost
                        currentHealth = Mathf.Min(maxHealth, currentHealth + 25);
                        OnHealthChanged?.Invoke(currentHealth, maxHealth);
                    }
                    else 
                    {
                        // Damage boost
                        AddStatBoost("damage", 10, true, 10f, "ChaosBoost");
                    }
                    break;
                    
                default:
                    Debug.LogWarning($"Unhandled special effect: {card.SpecialEffect}");
                    break;
            }
        }
        /// Gets a copy of the current hand
        /// </summary>
        private List<ICard> GetCurrentHand()
        {
            return hand != null ? new List<ICard>(hand) : new List<ICard>();
        }

        /// <summary>
        /// Gets remaining cooldown time for an ability
        /// </summary>
        private float GetAbilityCooldown(string abilityName)
        {
            if (string.IsNullOrEmpty(abilityName)) return 0;
            return abilityName switch
            {
                "Flick" => flickAbility?.GetCooldownRemaining() ?? 0,
                "Slice" => slashAbility?.GetCooldownRemaining() ?? 0,
                _ => 0
            };
        }

        /// <summary>
        /// Gets remaining cooldown time for a card
        /// </summary>
        private float GetCardCooldown(ICard card)
        {
            if (card == null || card.LastUseTime == 0) return 0;
            return Mathf.Max(0, card.Cooldown - (Time.time - card.LastUseTime));
        }

        /// <summary>
        /// Checks if a card can be used
        /// </summary> 
        private bool CanUseCard(ICard card)
        {
            if (card == null) return false;
            return currentEnergy >= card.EnergyCost && GetCardCooldown(card) <= 0;
        }

        /// <summary>
        /// Checks if a card is on cooldown
        /// </summary>
        private bool IsCardOnCooldown(ICard card)
        {
            return GetCardCooldown(card) > 0;
        }

        #endregion
        #region Utility Methods
        /// <summary>
        /// Coroutine to stun the character for a specified duration
        /// </summary>
        /// <param name="duration">Stun duration in seconds</param>
        /// <returns>IEnumerator for coroutine execution</returns>
        public IEnumerator StunCoroutine(float duration)
        {
            // Store the original state
            CharacterState originalState = _currentState;
            
            // Set character to stunned/incapacitated state
            _currentState = CharacterState.Incapacitated;
            
            // Disable player controller if it exists
            if (playerController != null)
            {
                playerController.enabled = false;
            }
            
            // Wait for the stun duration
            yield return new WaitForSeconds(duration);
            
            // Restore original state if character wasn't killed during stun
            if (currentHealth > 0)
            {
                _currentState = originalState;
            }
            
            // Re-enable player controller
            if (playerController != null)
            {
                playerController.enabled = true;
            }
        }
        
        private void DrawInitialHand()
        {
            for (int i = 0; i < initialHandSize; i++)
            {
                DrawCard();
            }
        }
        
        private void InitializeCombatAbilities()
        {
            // Add Flick ability if missing
            if (flickAbility == null && autoAddAbilities)
            {
                flickAbility = gameObject.AddComponent<RangedFlickAbility>();
                flickAbility.cooldownTime = 2f; // Standard cooldown for ranged attack
                Debug.Log("Added Flick ability");
            }
            
            // Add Slash ability if missing
            if (slashAbility == null && autoAddAbilities)
            {
                slashAbility = gameObject.AddComponent<MeleeSlashAbility>();
                slashAbility.cooldownTime = 1.5f; // Shorter cooldown for melee attack
                Debug.Log("Added Slash ability");
            }
            
            // Configure abilities
            if (flickAbility != null)
            {
                flickAbility.enabled = true;
                // Configure additional flick properties
                flickAbility.Initialize(20f, 15f, 2f); // range, damage, cooldown
            }
            
            if (slashAbility != null)
            {
                slashAbility.enabled = true;
                // Configure additional slash properties
                slashAbility.Initialize(3f, 25f, 1.5f); // range, damage, cooldown
            }
        }
        
        private void AddStatBoost(string statName, int amount, bool isPercentage, float duration, string source)
        {
            if (!statBoosts.ContainsKey(statName))
            {
                statBoosts[statName] = new List<StatBoost>();
            }
            
            // Remove existing boost from same source if it exists
            statBoosts[statName].RemoveAll(b => b.source == source);
            
            // Add new boost
            statBoosts[statName].Add(new StatBoost(statName, amount, isPercentage, duration, source));
        }

        private void RemoveStatBoost(string source)
        {
            foreach (var boosts in statBoosts.Values)
            {
                boosts.RemoveAll(b => b.source == source);
            }
        }

        private int GetStatBoostTotal(string statName)
        {
            if (!statBoosts.ContainsKey(statName))
                return 0;
                
            int total = 0;
            statBoosts[statName].RemoveAll(b => b.IsExpired);
            
            foreach (var boost in statBoosts[statName])
            {
                if (boost.isPercentage)
                {
                    switch (statName)
                    {
                        case "accuracy":
                            total += (int)(accuracy * (boost.amount / 100f));
                            break;
                        case "defense":
                            total += (int)(defense * (boost.amount / 100f));
                            break;
                    }
                }
                else
                {
                    total += boost.amount;
                }
            }
            
            return total;
        }
        
        private class StatBoost
        {
            public string statName;
            public int amount;
            public bool isPercentage;
            public float duration;
            public string source;
            public float startTime;
            
            public StatBoost(string statName, int amount, bool isPercentage, float duration, string source)
            {
                this.statName = statName;
                this.amount = amount;
                this.isPercentage = isPercentage;
                this.duration = duration;
                this.source = source;
                this.startTime = Time.time;
            }
            
            public bool IsExpired => duration > 0 && (Time.time - startTime) > duration;
        }
        
        /// <summary>
        /// Resets the character to its initial state
        /// </summary>
        /// <remarks>
        /// Clears all card effects, resets health/energy/buzz,
        /// and recreates the starting deck and hand
        /// </remarks>
        public void ResetCharacter()
        public void ResetCharacter()
        {
            // Reset health and energy
            currentEnergy = maxEnergy;
            
            // Reset buzz
            SetBuzzLevel(maxBuzz / 2);
            
            // Reset state
            _currentState = CharacterState.Normal;
            // Clear all stat boosts
            statBoosts.Clear();
            // Reset deck and hand
            if (startingDeck != null)
            {
                InitializeDeck(startingDeck.Where(c => c != null)
                    .Select(card => (ICard)card).ToList());
            }
            else
            {
                InitializeDeck(new List<ICard>());
            }
            
            if (discardPile != null) discardPile.Clear();
            if (hand != null) hand.Clear();
            cardsPlayedThisTurn = 0;
            hasEnergyDiscount = false;
            DrawInitialHand();
            // Re-enable player controller if it exists
            if (playerController != null)
            {
                playerController.enabled = true;
            }
            
            Debug.Log("Character reset to initial state");
        }
        
        #endregion
        
        #region Public Helper Methods
        
        /// <summary>
        /// Modifies the character's buzz level by the specified amount
        /// </summary>
        /// <param name="amount">Amount to change (can be positive or negative)</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if amount would exceed buzz boundaries</exception>
        public void ModifyBuzz(float amount)
        {
            if (amount == 0) return;
            AdjustBuzz(amount);
        }
        /// <summary>
        /// Resets the character's health to maximum
        /// </summary>
        public void ResetHealth()
        {
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
        
        /// <summary>
        /// Resets the character's buzz level to maximum
        /// </summary>
        public void ResetBuzz()
        {
            SetBuzzLevel(maxBuzz);
        }
        
        
        
        /// <summary>
        /// Discards current hand and draws a fresh set of cards
        /// </summary>
        public void DrawNewHand()
        {
            // Discard current hand
            foreach (var card in hand)
            {
                DiscardCard(card);
            }
            hand.Clear();

            // Draw new cards
            DrawInitialHand();
        }
        #endregion
    }
} // End of namespace
