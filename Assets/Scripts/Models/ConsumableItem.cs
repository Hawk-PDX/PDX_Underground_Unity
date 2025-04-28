using System;
using System.Collections.Generic;
using UnityEngine;
using PDXUnderground.Models;

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

        // Events
        public event Action<ConsumableItem> OnItemUsed;
        // Logger for decoupling from Unity's Debug class
        public static Interfaces.ILogger Logger { get; set; }

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
        public new float Cooldown;

        [Tooltip("Duration of effects (in seconds)")]
        public float EffectDuration;

        [Tooltip("Status effects to apply")]
        public string[] StatusEffects;

        // Stat boost structure
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

        #region Resource Restoration
        
        /// <summary>
        /// Apply energy restoration to a character
        /// </summary>
        /// <param name="character">Character to restore energy for</param>
        /// <returns>Amount of energy restored</returns>
        private int ApplyEnergyRestoration(CharacterData character)
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
                Logger?.Log($"{character.CharacterName} restored {potentialRestore} energy from {ItemName}");
                return potentialRestore;
            }
            
            return 0;
        }

        /// <summary>
        /// Apply health restoration to a character
        /// </summary>
        /// <param name="character">Character to restore health for</param>
        /// <returns>Amount of health restored</returns>
        private int ApplyHealthRestoration(CharacterData character)
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
                Logger?.Log($"{character.CharacterName} restored {potentialRestore} health from {ItemName}");
                return potentialRestore;
            }
            
            return 0;
        }
        
        /// <summary>
        /// Apply mana restoration to a character
        /// </summary>
        /// <param name="character">Character to restore mana for</param>
        /// <returns>Amount of mana restored</returns>
        private int ApplyManaRestoration(CharacterData character)
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
                Logger?.Log($"{character.CharacterName} restored {potentialRestore} mana from {ItemName}");
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
        private bool ApplyEffects(CharacterData character, float currentTime)
        {
            if (character == null)
                return false;
                
            bool effectsApplied = false;
            
            // Apply resource restorations
            int healthRestored = ApplyHealthRestoration(character);
            int manaRestored = ApplyManaRestoration(character);
            int energyRestored = ApplyEnergyRestoration(character);
            
            // Apply status effects
            bool statusEffectsApplied = ApplyStatusEffects(character, currentTime);
            
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
        private bool ApplyStatusEffects(CharacterData character, float currentTime)
        {
            if (StatusEffects == null || StatusEffects.Length == 0 || character == null)
                return false;
                
            bool anyEffectApplied = false;
            
            foreach (string effect in StatusEffects)
            {
                // Apply each status effect to the character
                var statusEffect = new StatusEffect
                {
                    EffectName = effect,
                    Description = $"Effect from {ItemName}",
                    Duration = EffectDuration,
                    Source = ItemName,
                    StartTime = currentTime
                };
                bool applied = character.ApplyStatusEffect(statusEffect, EffectDuration, ItemName);
                
                if (applied)
                {
                    Logger?.Log($"Applied status effect '{effect}' to {character.CharacterName} for {EffectDuration}s");
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
        private bool ApplyTemporaryBoosts(CharacterData character)
        {
            if (TemporaryBoosts == null || TemporaryBoosts.Count == 0 || character == null || character.Stats == null)
                return false;
                
            bool anyBoostApplied = false;
            
            foreach (StatBonus boost in TemporaryBoosts)
            {
                if (string.IsNullOrEmpty(boost.StatType))
                    continue;
                    
                // Apply each temporary stat boost
                character.Stats.AddStatBoost(
                    boost.StatType,
                    boost.Value,
                    boost.IsPercentage,
                    EffectDuration,
                    $"Consumable:{ItemName}"
                );
                Logger?.Log($"Applied {boost.Value}{(boost.IsPercentage ? "%" : "")} {boost.StatType} boost to {character.CharacterName} for {EffectDuration}s");
                anyBoostApplied = true;
            }
            
            return anyBoostApplied;
        }
        
        /// <summary>
        /// Remove expired effects from a character
        /// </summary>
        /// <param name="character">Character to remove effects from</param>
        public void RemoveExpiredEffects(CharacterData character)
        {
            if (character == null)
                return;
                
            // Remove expired status effects
            if (StatusEffects != null && StatusEffects.Length > 0)
            {
                foreach (string effect in StatusEffects)
                {
                    // Let Character handle expired effects removal
                    character.RemoveExpiredStatusEffect();
                }
            }
            
            // Remove expired stat boosts
            if (character.Stats != null && TemporaryBoosts != null && TemporaryBoosts.Count > 0)
            {
                character.Stats.RemoveExpiredStatBoosts(Time.time);
            }
        }

        #endregion

        #region Cooldown Management
        
        /// <summary>
        /// <summary>
        /// Put the item on cooldown for a character
        /// </summary>
        /// <param name="currentTime">Current game time</param>
        private void StartCooldown(int characterId, float currentTime)
        {
            if (Cooldown <= 0f)
                return;
                
            lastUseTimeByCharacter[characterId] = currentTime;
            Logger?.Log($"{ItemName} is now on cooldown for {Cooldown}s");
        }
        
        /// <summary>
        /// Update cooldowns for all characters
        /// </summary>
        public void UpdateCooldowns(float currentTime)
        {
            if (Cooldown <= 0f || lastUseTimeByCharacter.Count == 0)
                return;
                
            // Check for and remove expired cooldowns
            List<int> expiredCooldowns = new List<int>();
            foreach (var entry in lastUseTimeByCharacter)
            {
                if (currentTime - entry.Value >= Cooldown)
                {
                    expiredCooldowns.Add(entry.Key);
                }
            }
            
            // Remove expired cooldowns
            foreach (int characterId in expiredCooldowns)
            {
                lastUseTimeByCharacter.Remove(characterId);
                Logger?.Log($"{ItemName} cooldown expired for character ID {characterId}");
            }
        }

        /// <summary>
        /// Check if the item is on cooldown for a character
        /// </summary>
        /// <param name="characterId">ID of character using the item</param>
        /// <param name="currentTime">Current game time</param>
        /// <returns>True if on cooldown</returns>
        private bool IsOnCooldown(int characterId, float currentTime)
        {
            if (Cooldown <= 0f)
                return false;
                
            if (lastUseTimeByCharacter.TryGetValue(characterId, out float lastUseTime))
            {
                return (currentTime - lastUseTime) < Cooldown;
            }
            
            return false;
        }
        
        /// <summary>
        /// Get the remaining cooldown time for a character
        /// </summary>
        /// <param name="characterId">ID of character using the item</param>
        /// <param name="currentTime">Current game time</param>
        /// <returns>Remaining cooldown time in seconds</returns>
        private float GetRemainingCooldown(int characterId, float currentTime)
        {
            if (Cooldown <= 0f)
                return 0f;
                
            if (lastUseTimeByCharacter.TryGetValue(characterId, out float lastUseTime))
            {
                return Mathf.Max(0f, Cooldown - (currentTime - lastUseTime));
            }
            
            return 0f;
        }
        
        #endregion
        
        #region Item Overrides
        
        /// <summary>
        /// Use the consumable item
        /// </summary>
        /// <param name="character">Character using the item</param>
        /// <returns>True if item was used successfully</returns>
        public override bool Use(CharacterData character, float currentTime)
        {
            // Check for null character or broken item
            if (character == null || IsBroken)
            {
                Logger?.LogWarning($"Cannot use {ItemName}: character is null or item is broken");
                return false;
            }
                
            // Check level requirement
            if (character.Level < RequiredLevel)
            {
                Logger?.Log($"Cannot use {ItemName}: requires level {RequiredLevel}");
                return false;
            }
            
            // Check if item is on cooldown
            if (IsOnCooldown(character.Id, currentTime))
            {
                float remainingTime = GetRemainingCooldown(character.Id, currentTime);
                Logger?.Log($"Cannot use {ItemName}: on cooldown for {remainingTime:F1} more seconds");
                return false;
            }
            
            // Apply all effects
            // Apply all effects
            bool effectsApplied = ApplyEffects(character, currentTime);
            // If any effects were applied
            if (effectsApplied)
            {
                // Put item on cooldown
                StartCooldown(character.Id, currentTime);
                
                // Reduce quantity if consumable
                if (IsConsumable)
                {
                    Quantity--;
                }
                
                // Reduce durability if applicable
                if (MaxDurability > 0)
                {
                    CurrentDurability--;
                }
                
                // Trigger events
                OnItemUsed?.Invoke(this);
                
                Logger?.Log($"{character.CharacterName} used {ItemName}");
                return true;
            }
            
            Logger?.Log($"{ItemName} had no effect on {character.CharacterName}");
            return false;
        }
        
        #endregion
    }
}
