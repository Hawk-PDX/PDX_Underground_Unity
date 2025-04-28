using System;
using System.Collections.Generic;
using UnityEngine;

namespace PDXUnderground.Models
{
    /// <summary>
    /// Character data model that contains all persistent data for a character.
    /// This is a pure data model without Unity MonoBehaviour dependencies.
    /// It contains character stats, inventory, and status effects.
    /// </summary>
    [Serializable]
    public class CharacterData : ISerializationCallbackReceiver
    {
        #region Interfaces
        
        /// <summary>
        /// Logging interface for character data operations
        /// </summary>
        public interface ILogger
        {
            void Log(string message);
            void LogWarning(string message);
            void LogError(string message);
        }

        /// <summary>
        /// Reference to logger (optional)
        /// </summary>
        public ILogger Logger { get; set; }
        
        #endregion
        
        #region Properties and Fields
        
        // Database primary key
        [HideInInspector]
        public int Id { get; set; }

        // Reference to player who owns this character
        [HideInInspector]
        public int PlayerId { get; set; }

        // Basic character information
        [Header("Character Information")]
        public string CharacterName;
        public string CharacterClass;
        
        [Header("Equipment Types")]
        public string WeaponType;
        public string ArmorType;

        // Character progression
        [Header("Level & Experience")]
        public int Level = 1;
        public int Experience = 0;
        public int ExperienceToNextLevel => CalculateExpToNextLevel();

        // Health, mana and energy stats
        [Header("Health, Mana & Energy")]
        public int CurrentHealth;
        public int MaxHealth;
        public int CurrentMana;
        public int MaxMana;
        public int CurrentEnergy;
        public int MaxEnergy;

        // Status effect management
        [SerializeField]
        private List<StatusEffect> activeStatusEffects = new List<StatusEffect>();
        
        // Event fired when status effects change
        public event Action OnStatusEffectsChanged;
        
        /// <summary>
        /// Returns all active status effects on the character
        /// </summary>
        public IReadOnlyList<StatusEffect> ActiveStatusEffects => activeStatusEffects;
        
        /// <summary>
        /// Dictionary for quick lookups of status effects by name
        /// </summary>
        [NonSerialized]
        private Dictionary<string, StatusEffect> _statusEffectLookup = new Dictionary<string, StatusEffect>();

        // Timestamps for database
        public DateTime CreatedAt;
        public DateTime LastUpdated;

        // Reference to character stats
        public CharacterStats Stats;

        // Reference to inventory - not serialized directly
        [NonSerialized]
        private List<Item> _inventory;
        public List<Item> Inventory
        {
            get { return _inventory ?? (_inventory = new List<Item>()); }
            private set { _inventory = value; }
        }

        // Special abilities this character has unlocked
        [NonSerialized]
        private List<int> _abilityIds;
        public List<int> AbilityIds
        {
            get { return _abilityIds ?? (_abilityIds = new List<int>()); }
            private set { _abilityIds = value; }
        }

        // Serialization helpers for Unity
        [SerializeField]
        private string _serializedInventory;
        [SerializeField]
        private string _serializedAbilities;
        
        #endregion

        #region Constructors and Initialization
        
        /// <summary>
        /// Initializes a new character data with default values
        /// </summary>
        public CharacterData()
        {
            _inventory = new List<Item>();
            _abilityIds = new List<int>();
            activeStatusEffects = new List<StatusEffect>();
            _statusEffectLookup = new Dictionary<string, StatusEffect>();
        }

        /// <summary>
        /// Initializes the character data with default values
        /// </summary>
        public void Initialize()
        {
            // Initialize collections
            if (_inventory == null) _inventory = new List<Item>();
            if (_abilityIds == null) _abilityIds = new List<int>();
            
            // Initialize stats
            if (Stats == null) Stats = new CharacterStats();
            
            // Initialize default values if new character
            if (CurrentHealth <= 0)
                CurrentHealth = MaxHealth > 0 ? MaxHealth : 100;
            
            if (CurrentMana <= 0)
                CurrentMana = MaxMana > 0 ? MaxMana : 100;
                
            if (CurrentEnergy <= 0)
                CurrentEnergy = MaxEnergy > 0 ? MaxEnergy : 100;
                
            // Initialize status effect lookup
            RefreshStatusEffectLookup();
        }
        
        #endregion

        #region Character Logic

        /// <summary>
        /// Calculate experience needed for next level using a formula
        /// </summary>
        private int CalculateExpToNextLevel()
        {
            // Simple level progression formula (can be adjusted)
            return 100 * Level * (Level + 1) / 2;
        }

        /// <summary>
        /// Award experience points to the character, level up if threshold reached
        /// </summary>
        /// <param name="amount">Amount of experience to award</param>
        /// <returns>True if level up occurred</returns>
        public bool AddExperience(int amount)
        {
            Experience += amount;
            bool leveledUp = false;

            // Check for level up
            while (Experience >= ExperienceToNextLevel)
            {
                LevelUp();
                leveledUp = true;
            }

            return leveledUp;
        }

        /// <summary>
        /// Increase character's level and update stats
        /// </summary>
        public void LevelUp()
        {
            Level++;
            
            // Update base stats on level up
            MaxHealth += 10 + (int)(Stats.Vitality * 0.5f);
            MaxMana += 5 + (int)(Stats.Intellect * 0.5f);
            MaxEnergy += 5 + (int)(Stats.Agility * 0.5f);
            
            // Restore health, mana and energy on level up
            CurrentHealth = MaxHealth;
            CurrentMana = MaxMana;
            CurrentEnergy = MaxEnergy;
            
            // Mark time of update for database
            LastUpdated = DateTime.Now;
            
            Logger?.Log($"{CharacterName} leveled up to level {Level}!");
        }
        
        /// <summary>
        /// Apply damage to the character
        /// </summary>
        /// <param name="amount">Amount of damage to take</param>
        /// <param name="damageType">Type of damage (physical, fire, frost, etc.)</param>
        /// <param name="attacker">Character causing the damage</param>
        /// <returns>True if character is still alive after damage</returns>
        public bool TakeDamage(int amount, string damageType = "physical", CharacterData attacker = null)
        {
            // Apply damage reduction based on damage type
            float damageReduction = damageType.ToLower() == "physical" 
                ? Stats.CalculateDamageReduction() 
                : Stats.CalculateMagicResistance();
                
            int actualDamage = Mathf.Max(1, (int)(amount * (1 - damageReduction)));
            
            CurrentHealth = Mathf.Max(0, CurrentHealth - actualDamage);
            
            // Check if character is dead
            if (CurrentHealth <= 0)
            {
                Logger?.Log($"{CharacterName} has been defeated by {(attacker != null ? attacker.CharacterName : "unknown")}!");
                // Trigger death event
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Heal the character by the specified amount
        /// </summary>
        /// <param name="amount">Amount to heal</param>
        public void Heal(int amount)
        {
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        }

        /// <summary>
        /// Use mana for abilities
        /// </summary>
        /// <param name="amount">Amount of mana to use</param>
        /// <returns>True if enough mana was available and used</returns>
        public bool UseMana(int amount)
        {
            if (CurrentMana >= amount)
            {
                CurrentMana -= amount;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Restore mana to the character
        /// </summary>
        /// <param name="amount">Amount of mana to restore</param>
        public void RestoreMana(int amount)
        {
            CurrentMana = Mathf.Min(MaxMana, CurrentMana + amount);
        }
        
        /// <summary>
        /// Use energy for abilities or actions
        /// </summary>
        /// <param name="amount">Amount of energy to use</param>
        /// <returns>True if enough energy was available and used</returns>
        public bool UseEnergy(int amount)
        {
            if (CurrentEnergy >= amount)
            {
                CurrentEnergy -= amount;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Restore energy to the character
        /// </summary>
        /// <param name="amount">Amount of energy to restore</param>
        public void RestoreEnergy(int amount)
        {
            CurrentEnergy = Mathf.Min(MaxEnergy, CurrentEnergy + amount);
        }
        
        #endregion
        
        #region Status Effect Management
        
        /// <summary>
        /// Apply a status effect to the character
        /// </summary>
        /// <param name="effect">The status effect to apply</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="source">Source of the effect</param>
        /// <returns>True if the effect was applied, false if blocked or immune</returns>
        public bool AddStatusEffect(StatusEffect effect, float duration, string source)
        {
            if (effect == null)
                return false;
            
            // Check for immunity to this type of effect
            if (IsImmuneToEffect(effect.EffectType))
            {
                Logger?.Log($"{CharacterName} is immune to {effect.EffectName}");
                return false;
            }
            
            // Create the status effect instance
            StatusEffect newEffect = new StatusEffect
            {
                EffectName = effect.EffectName,
                EffectType = effect.EffectType,
                Description = effect.Description,
                Strength = effect.Strength,
                Duration = duration,
                StartTime = Time.time,
                Source = source,
                IsPositive = effect.IsPositive,
                Icon = effect.Icon,
                StatToModify = effect.StatToModify,
                ModifierValue = effect.ModifierValue,
                IsPercentage = effect.IsPercentage
            };
            
            // Handle stat modifications if needed
            if (Stats != null && !string.IsNullOrEmpty(effect.StatToModify))
            {
                Stats.AddStatBoost(
                    effect.StatToModify,
                    effect.ModifierValue,
                    effect.IsPercentage,
                    duration,
                    $"StatusEffect:{effect.EffectName}"
                );
            }
            
            // Add to active effects list
            activeStatusEffects.Add(newEffect);
            
            // Update lookup dictionary
            _statusEffectLookup[newEffect.EffectName.ToLower()] = newEffect;
            
            // Trigger event
            OnStatusEffectsChanged?.Invoke();
            Logger?.Log($"Applied status effect {effect.EffectName} to {CharacterName} for {duration}s");
            return true;
        }

        /// <summary>
        /// Remove all expired status effects from the character
        /// </summary>
        public void RemoveExpiredStatusEffects()
        {
            bool hasExpired = false;
            
            // Check each status effect
            for (int i = activeStatusEffects.Count - 1; i >= 0; i--)
            {
                StatusEffect effect = activeStatusEffects[i];
                
                // Remove if expired
                if (effect.IsExpired(Time.time))
                {
                    Logger?.Log($"Status effect {effect.EffectName} expired on {CharacterName}");
                    string effectName = effect.EffectName.ToLower();
                    activeStatusEffects.RemoveAt(i);
                    _statusEffectLookup.Remove(effectName);
                    hasExpired = true;
                }
            }
            // Trigger event if any effects were removed
            if (hasExpired)
            {
                OnStatusEffectsChanged?.Invoke();
                
                // Also remove any expired stat boosts
                if (Stats != null)
                {
                    Stats.RemoveExpiredStatBoosts(Time.time);
                }
            }
        }
    
        /// <summary>
        /// Remove a specific status effect by name
        /// </summary>
        /// <param name="effectName">Name of effect to remove</param>
        /// <returns>True if effect was found and removed</returns>
        public bool RemoveStatusEffect(string effectName)
        {
            for (int i = activeStatusEffects.Count - 1; i >= 0; i--)
            {
                if (activeStatusEffects[i].EffectName == effectName)
                {
                    StatusEffect effect = activeStatusEffects[i];
                    string effectKey = effect.EffectName.ToLower();
                    activeStatusEffects.RemoveAt(i);
                    _statusEffectLookup.Remove(effectKey);
                    
                    // Remove associated stat boosts
                    if (Stats != null)
                    {
                        Stats.RemoveStatBoostsFromSource($"StatusEffect:{effect.EffectName}");
                    }
                    
                    // Trigger event
                    OnStatusEffectsChanged?.Invoke();
                    return true;
                }
            }
            
            return false;
        }
    
        /// <summary>
        /// Check if character is immune to a type of effect
        /// </summary>
        /// <param name="effectType">Type of effect to check</param>
        /// <returns>True if immune</returns>
        public bool IsImmuneToEffect(string effectType)
        {
            if (string.IsNullOrEmpty(effectType))
                return false;

            // Check current status effects for immunities
            foreach (var effect in activeStatusEffects)
            {
                // If we have an immunity effect for this type
                if (effect.EffectName.ToLower().Contains("immunity") &&
                    effect.EffectType.ToLower() == effectType.ToLower())
                {
                    return true;
                }
            }

            // Check character stats for innate immunities
            if (Stats != null)
            {
                switch (effectType.ToLower())
                {
                    case "poison":
                        return Stats.IsPoisonImmune;
                    case "stun":
                        return Stats.IsStunImmune;
                    case "freeze":
                        return Stats.IsFreezeImmune;
                    default:
                        return false;
                }
            }

            return false;
        }
        
        /// <summary>
        /// Clear all status effects
        /// </summary>
        public void ClearAllStatusEffects()
        {
            if (activeStatusEffects.Count > 0)
            {
                // Remove all associated stat boosts
                if (Stats != null)
                {
                    foreach (var effect in activeStatusEffects)
                    {
                        Stats.RemoveStatBoostsFromSource($"StatusEffect:{effect.EffectName}");
                    }
                }
                
                // Clear the list
                activeStatusEffects.Clear();
                _statusEffectLookup.Clear();
                
                // Trigger event
                OnStatusEffectsChanged?.Invoke();
                
                Logger?.Log($"Cleared all status effects from {CharacterName}");
            }
        }
    
        /// <summary>
        /// Rebuilds the status effect lookup dictionary from the active effects list
        /// </summary>
        private void RefreshStatusEffectLookup()
        {
            _statusEffectLookup.Clear();
            foreach (var effect in activeStatusEffects)
            {
                _statusEffectLookup[effect.EffectName.ToLower()] = effect;
            }
        }
    
        /// <summary>
        /// Checks if the character has a specific status effect
        /// </summary>
        /// <param name="effectName">Name of the effect to check</param>
        /// <returns>True if the effect is active</returns>
        public bool HasStatusEffect(string effectName)
        {
            if (string.IsNullOrEmpty(effectName))
                return false;
                
            return _statusEffectLookup.ContainsKey(effectName.ToLower());
        }
    
        /// <summary>
        /// Gets a status effect by name
        /// </summary>
        /// <param name="effectName">Name of the effect</param>
        /// <returns>The status effect, or null if not found</returns>
        public StatusEffect GetStatusEffect(string effectName)
        {
            if (string.IsNullOrEmpty(effectName))
                return null;
                
            if (_statusEffectLookup.TryGetValue(effectName.ToLower(), out StatusEffect effect))
                return effect;
                
            return null;
        }

        /// <summary>
        /// Apply a status effect to the character (alias for AddStatusEffect)
        /// </summary>
        /// <param name="effect">The status effect to apply</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="source">Source of the effect</param>
        /// <returns>True if the effect was applied, false if blocked or immune</returns>
        public bool ApplyStatusEffect(StatusEffect effect, float duration, string source)
        {
            return AddStatusEffect(effect, duration, source);
        }

        /// <summary>
        /// Remove expired status effect (alias for RemoveExpiredStatusEffects)
        /// </summary>
        public void RemoveExpiredStatusEffect()
        {
            RemoveExpiredStatusEffects();
        }
        
        #endregion
        
        #region Inventory Management
        
        /// <summary>
        /// Adds an item to the character's inventory
        /// </summary>
        /// <param name="item">Item to add</param>
        public void AddToInventory(Item item)
        {
            if (item != null)
            {
                Inventory.Add(item);
                Logger?.Log($"{CharacterName} acquired {item.ItemName}");
            }
        }
        
        /// <summary>
        /// Checks if character can equip a particular item
        /// </summary>
        /// <param name="item">Item to check</param>
        /// <returns>True if equippable</returns>
        public bool CanEquipItem(Item item)
        {
            if (item == null) return false;
            
            // Check level requirement
            if (Level < item.RequiredLevel) return false;
            
            // Check weapon type compatibility
            if (item.ItemType == "weapon" && !string.IsNullOrEmpty(WeaponType))
            {
                return item.WeaponType == WeaponType;
            }
            
            // Check armor type compatibility
            if (item.ItemType == "armor" && !string.IsNullOrEmpty(ArmorType))
            {
                return item.ArmorType == ArmorType;
            }
            
            return true;
        }
        
        #endregion
        
        #region Database Methods
        
        /// <summary>
        /// Save character data to database
        /// </summary>
        public void SaveToDatabase()
        {
            // This will be implemented later with SQLite or server connection
            // For now we'll just mark the last update time
            LastUpdated = DateTime.Now;
            Logger?.Log($"Character {CharacterName} was saved (simulated)");
            // In the finished implementation, this would use:
            // DatabaseManager.Instance.SaveCharacter(this);
        }
        
        /// <summary>
        /// Static method to load a character from the database
        /// </summary>
        /// <param name="characterId">ID of character to load</param>
        /// <returns>CharacterData object</returns>
        public static CharacterData LoadFromDatabase(int characterId)
        {
            // This will be implemented later with SQLite or server connection
            // Cannot use instance logger for static method
            Debug.Log($"Loading character ID {characterId} (simulated)");
            // In the finished implementation, this would use:
            // return DatabaseManager.Instance.GetCharacterById(characterId);
            return null;
        }
        
        #endregion
        
        #region Serialization
        
        /// <summary>
        /// Prepare complex objects for serialization
        /// </summary>
        public void OnBeforeSerialize()
        {
            // Serialize inventory to JSON string
            if (_inventory != null && _inventory.Count > 0)
            {
                _serializedInventory = JsonUtility.ToJson(new SerializableItemList(_inventory));
            }
            
            // Serialize abilities to JSON string
            if (_abilityIds != null && _abilityIds.Count > 0)
            {
                _serializedAbilities = JsonUtility.ToJson(new SerializableIntList(_abilityIds));
            }
        }
        
        /// <summary>
        /// Restore complex objects after deserialization
        /// </summary>
        public void OnAfterDeserialize()
        {
            // Deserialize inventory from JSON string
            if (!string.IsNullOrEmpty(_serializedInventory))
            {
                SerializableItemList itemList = JsonUtility.FromJson<SerializableItemList>(_serializedInventory);
                _inventory = itemList.items;
            }
            
            // Deserialize abilities from JSON string
            if (!string.IsNullOrEmpty(_serializedAbilities))
            {
                SerializableIntList abilityList = JsonUtility.FromJson<SerializableIntList>(_serializedAbilities);
                _abilityIds = abilityList.values;
            }
        }
        
        #endregion
    }
    
    // Helper classes for serialization of lists
    [Serializable]
    public class SerializableIntList
    {
        public List<int> values;
        
        public SerializableIntList(List<int> valueList)
        {
            values = valueList;
        }
    }
    
    [Serializable]
    public class SerializableItemList
    {
        public List<Item> items;

        public SerializableItemList(List<Item> itemList)
        {
            items = itemList;
        }
    }
}
