using System;
using System.Collections.Generic;
using UnityEngine;

namespace PDXUnderground.Models
{
    /// <summary>
    /// Character class representing a playable character in the game.
    /// Maps to the 'characters' table in the database.
    /// 
    /// Migration notes from Python:
    /// - Converted Python dictionary properties to C# properties
    /// - Added serialization attributes for Unity Inspector and save system
    /// - Implemented ISerializationCallbackReceiver for complex object serialization
    /// </summary>
    [Serializable]
    public class Character : MonoBehaviour, ISerializationCallbackReceiver
    {
        // Database primary key
        [HideInInspector]
        public int Id;

        // Reference to player who owns this character
        [HideInInspector]
        public int PlayerId;

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

        // Health and mana stats
        [Header("Health & Mana")]
        public int CurrentHealth;
        public int MaxHealth;
        public int CurrentMana;
        public int MaxMana;

        // Timestamps for database
        [HideInInspector]
        public DateTime CreatedAt;
        [HideInInspector]
        public DateTime LastUpdated;

        // Reference to character stats component
        [HideInInspector]
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
        [SerializeField, HideInInspector]
        private string _serializedInventory;
        [SerializeField, HideInInspector]
        private string _serializedAbilities;

        #region Unity Lifecycle

        private void Awake()
        {
            // Initialize character if not loaded from save
            if (Stats == null)
            {
                Stats = GetComponent<CharacterStats>() ?? gameObject.AddComponent<CharacterStats>();
            }
        }

        private void Start()
        {
            // Initialize default values if new character
            if (CurrentHealth <= 0)
            {
                CurrentHealth = MaxHealth > 0 ? MaxHealth : 100;
            }

            if (CurrentMana <= 0)
            {
                CurrentMana = MaxMana > 0 ? MaxMana : 100;
            }
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
            
            // Restore health and mana on level up
            CurrentHealth = MaxHealth;
            CurrentMana = MaxMana;
            
            // Mark time of update for database
            LastUpdated = DateTime.Now;
            
            Debug.Log($"{CharacterName} leveled up to level {Level}!");
        }

        /// <summary>
        /// Apply damage to the character
        /// </summary>
        /// <param name="amount">Amount of damage to take</param>
        /// <returns>True if character is still alive after damage</returns>
        public bool TakeDamage(int amount)
        {
            // Apply damage reduction based on stats
            float damageReduction = Stats.CalculateDamageReduction();
            int actualDamage = Mathf.Max(1, (int)(amount * (1 - damageReduction)));
            
            CurrentHealth = Mathf.Max(0, CurrentHealth - actualDamage);
            
            // Check if character is dead
            if (CurrentHealth <= 0)
            {
                Debug.Log($"{CharacterName} has been defeated!");
                // Trigger death event or animation
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Restore health to the character
        /// </summary>
        /// <param name="amount">Amount of health to restore</param>
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
        /// Adds an item to the character's inventory
        /// </summary>
        /// <param name="item">Item to add</param>
        public void AddToInventory(Item item)
        {
            if (item != null)
            {
                Inventory.Add(item);
                Debug.Log($"{CharacterName} acquired {item.ItemName}");
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
            Debug.Log($"Character {CharacterName} was saved (simulated)");
            
            // In the finished implementation, this would use:
            // DatabaseManager.Instance.SaveCharacter(this);
        }
        
        /// <summary>
        /// Static method to load a character from the database
        /// </summary>
        /// <param name="characterId">ID of character to load</param>
        /// <returns>Character object</returns>
        public static Character LoadFromDatabase(int characterId)
        {
            // This will be implemented later with SQLite or server connection
            Debug.Log($"Loading character ID {characterId} (simulated)");
            
            // Placeholder for database loading
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
    public class SerializableItemList
    {
        public List<Item> items;

        public SerializableItemList(List<Item> itemList)
        {
            items = itemList;
        }
    }

    [Serializable]
    public class SerializableIntList
    {
        public List<int> values;

        public SerializableIntList(List<int> valueList)
        {
            values = valueList;
        }
    }
}

