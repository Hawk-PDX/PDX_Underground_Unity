using System;
using System.Collections.Generic;
using UnityEngine;

namespace PDXUnderground.Models
{
    /// <summary>
    /// Consumable item class that extends the base Item class.
    /// Adds properties and behaviors for consumable items like potions, food, etc.
    /// Maps to the 'consumables' table in the database.
    /// </summary>
    [Serializable]
    public class ConsumableItem : Item
    {
        #region Properties

        // Event when item is used
        public event Action<ConsumableItem> OnItemUsed;

        // Dictionary to track cooldowns per character
        private Dictionary<int, float> lastUseTimeByCharacter = new Dictionary<int, float>();

        // Resource restoration
        [Header("Resource Restoration")]
        [Tooltip("Amount of health to restore")]
        public int HealthRestoration;

        [Tooltip("Amount of mana to restore")]
        public int ManaRestoration;

        [Tooltip("Amount of energy to restore")]
        public int EnergyRestoration;

        // Effect properties
        [Header("Effect Properties")]
        [Tooltip("Cooldown between uses (in seconds)")]
        public float Cooldown;

        [Tooltip("Duration of effects (in seconds)")]
        public float EffectDuration;

        [Tooltip("Status effects to apply")]
        public string[] StatusEffects;

        // Stat boost structure
        [Serializable]
        public struct StatBonus
        {
            public string StatType;
            public int Value;
            public bool IsPercentage;
        }

        [Tooltip("Temporary stat boosts to apply")]
        public List<StatBonus> TemporaryBoosts;

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameTimeUpdate.AddListener(UpdateCooldowns);
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameTimeUpdate.RemoveListener(UpdateCooldowns);
            }
        }

        #endregion

        #region Resource Restoration
        
        /// <summary>
        /// Apply energy restoration to a character
        /// </summary>
        /// <param name="character">Character to restore energy for</param>
        /// <returns>Amount of energy restored</returns>
        private int ApplyEnergyRestoration(Character character)
        {
            if (EnergyRestoration <= 0 || character == null || character.Stats == null)
                return 0;
                
            int currentEnergy = character.Stats.CurrentEnergy;
            int maxEnergy = character.Stats.MaxEnergy;
            
            // Calculate how much energy can actually be restored
            int potentialRestore = Mathf.Min(EnergyRestoration, maxEnergy - currentEnergy);
            
            if (potentialRestore > 0)
            {
                character.Stats.CurrentEnergy += potentialRestore;
                Debug.Log($"{character.CharacterName} restored {potentialRestore} energy from {ItemName}");
                return potentialRestore;
            }
            
            return 0;
        }

        /// <summary>
        /// Apply health restoration to a character
        /// </summary>
        /// <param name="character">Character to restore health for</param>
        /// <returns>Amount of health restored</returns>
        private int ApplyHealthRestoration(Character character)
        {
            if (HealthRestoration <= 0 || character == null || character.Stats == null)
                return 0;
                
            int currentHealth = character.Stats.CurrentHealth;
            int maxHealth = character.Stats.MaxHealth;
            
            // Calculate how much health can actually be restored
            int potentialRestore = Mathf.Min(HealthRestoration, maxHealth - currentHealth);
            
            if (potentialRestore > 0)
            {
                character.Stats.CurrentHealth += potentialRestore;
                Debug.Log($"{character.CharacterName} restored {potentialRestore} health from {ItemName}");
                return potentialRestore;
            }
            
            return 0;
        }
        
        /// <summary>
        /// Apply mana restoration to a character
        /// </summary>
        /// <param name="character">Character to restore mana for</param>
        /// <returns>Amount of mana restored</returns>
        private int ApplyManaRestoration(Character character)
        {
            if (ManaRestoration <= 0 || character == null || character.Stats == null)
                return 0;
                
            int currentMana = character.Stats.CurrentMana;
            int maxMana = character.Stats.MaxMana;
            
            // Calculate how much mana can actually be restored
            int potentialRestore = Mathf.Min(ManaRestoration, maxMana - currentMana);
            
            if (potentialRestore > 0)
            {
                character.Stats.CurrentMana += potentialRestore;
                Debug.Log($"{character.CharacterName} restored {potentialRestore} mana from {ItemName}");
                return potentialRestore;
            }
            
            return 0;
        }

        #endregion

        #region Effect Management

        /// <summary>
        /// Apply all effects from the consumable to a character
        /// </summary>
        /// <param name="character">Character to apply effects to</param>
        /// <returns>True if any effects were applied</returns>
        private bool ApplyEffects(Character character)
        {
            if (character == null)
                return false;
                
            bool effectsApplied = false;
            
            // Apply resource restorations
            int healthRestored = ApplyHealthRestoration(character);
            int manaRestored = ApplyManaRestoration(character);
            int energyRestored = ApplyEnergyRestoration(character);
            
            // Apply status effects
            bool statusEffectsApplied = ApplyStatusEffects(character);
            
            // Apply temporary stat boosts
            bool boostsApplied = ApplyTemporaryBoosts(character);
            
            // Check if any effects were applied
            effectsApplied = healthRestored > 0 || manaRestored > 0 || energyRestored > 0 || 
                             statusEffectsApplied || boostsApplied;
            
            return effectsApplied;
        }
        
        /// <summary>
        /// Apply status effects to a character
        /// </summary>
        /// <param name="character">Character to apply effects to</param>
        /// <returns>True if any status effects were applied</returns>
        private bool ApplyStatusEffects(Character character)
        {
            if (StatusEffects == null || StatusEffects.Length == 0 || character == null)
                return false;
                
            bool anyEffectApplied = false;
            
            foreach (string effect in StatusEffects)
            {
                // Apply each status effect to the character
                bool applied = character.ApplyStatusEffect(effect, EffectDuration, ItemName);
                
                if (applied)
                {
                    Debug.Log($"Applied status effect '{effect}' to {character.CharacterName} for {EffectDuration}s");
                    anyEffectApplied = true;
                }
            }
            
            return anyEffectApplied;
        }
        
        /// <summary>
        /// Apply temporary stat boosts to a character
        /// </summary>
        /// <param name="character">Character to apply boosts to</param>
        /// <returns>True if any stat boosts were applied</returns>
        private bool ApplyTemporaryBoosts(Character character)
        {
            if (TemporaryBoosts == null || TemporaryBoosts.Count == 0 || character == null || character.Stats == null)
                return false;
                
            bool anyBoostApplied = false;
            
            foreach (StatBonus boost in TemporaryBoosts)
            {
                if (string.IsNullOrEmpty(boost.StatType))
                    continue;
                    
                // Apply each temporary stat boost
                bool applied = character.Stats.AddStatBoost(
                    boost.StatType,
                    boost.Value,
                    boost.IsPercentage,
                    EffectDuration,
                    $"Consumable:{ItemName}"
                );
                
                if (applied)
                {
                    Debug.Log($"Applied {boost.Value}{(boost.IsPercentage ? "%" : "")} {boost.StatType} boost to {character.CharacterName} for {EffectDuration}s");
                    anyBoostApplied = true;
                }
            }
            
            return anyBoostApplied;
        }
        
        /// <summary>
        /// Remove expired effects from a character
        /// </summary>
        /// <param name="character">Character to remove effects from</param>
        public void RemoveExpiredEffects(Character character)
        {
            if (character == null)
                return;
                
            // Remove expired status effects
            if (StatusEffects != null && StatusEffects.Length > 0)
            {
                foreach (string effect in StatusEffects)
                {
                    character.RemoveExpiredStatusEffect(effect, ItemName);
                }
            }
            
            // Remove expired stat boosts
            if (character.Stats != null && TemporaryBoosts != null && TemporaryBoosts.Count > 0)
            {
                character.Stats.RemoveExpiredStatBoosts($"Consumable:{ItemName}");
            }
        }

        #endregion

        #region Cooldown Management
        
        /// <summary>
        /// Put the item on cooldown for a character
        /// </summary>
        /// <param name="characterId">ID of character using the item</param>
        private void StartCooldown(int characterId)
        {
            if (Cooldown <= 0f)
                return;
                
            lastUseTimeByCharacter[characterId] = Time.time;
            Debug.Log($"{ItemName} is now on cooldown for {Cooldown}s");
        }
        
        /// <summary>
        /// Update cooldowns for all characters
        /// </summary>
        public void UpdateCooldowns()
        {
            if (Cooldown <= 0f || lastUseTimeByCharacter.Count == 0)
                return;
                
            // Check for and remove expired cooldowns
            List<int> expiredCooldowns = new List<int>();
            foreach (var entry in lastUseTimeByCharacter)
            {
                if (Time.time - entry.Value >= Cooldown)
                {
                    expiredCooldowns.Add(entry.Key);
                }
            }
            
            // Remove expired cooldowns
            foreach (int characterId in expiredCooldowns)
            {
                lastUseTimeByCharacter.Remove(characterId);
                Debug.Log($"{ItemName} cooldown expired for character ID {characterId}");
            }
        }

        #endregion

        #region Item Overrides
        
        /// <summary>
        /// Use the consumable item
        /// </summary>
        /// <param name="character">Character using the item</param>
        /// <returns>True if item was used successfully</returns>
        public override bool Use(Character character)
        {
            // Check for null character or broken item
            if (character == null || IsBroken)
            {
                Debug.LogWarning($"Cannot use {ItemName}: character is null or item is broken");
                return false;
            }
                
            // Check level requirement
            if (character.Level < RequiredLevel)
            {
                Debug.Log($"Cannot use {ItemName}: requires level {RequiredLevel}");
                return false;
            }
            
            // Check if item is on cooldown
            if (IsOnCooldown(character.Id))
            {
                float remaining = GetRemainingCooldown(character.Id);
                Debug.Log($"Cannot use {ItemName}: still on cooldown for {remaining:F1}s");
                return false;
            }
            
            // Apply all effects
            bool effectApplied = ApplyEffects(character);
            
            if (effectApplied)
            {
                // Start cooldown
                StartCooldown(character.Id);
                
                // Trigger the item used event
                OnItemUsed?.Invoke(this);
                
                // Reduce quantity
                Quantity--;
                
                Debug.Log($"{character.CharacterName} used {ItemName}");
                return true;
            }
            
            Debug.Log($"{ItemName} had no effect on {character.CharacterName}");
            return false;
        }
        
        #endregion
    }
}
