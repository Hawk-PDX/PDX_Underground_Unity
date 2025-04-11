using System;
using System.Collections.Generic;
using UnityEngine;

namespace PDXUnderground.Models
{
    /// <summary>
    /// Base Item class representing game items.
    /// Maps to the 'items' table in the database.
    /// 
    /// Migration notes from Python:
    /// - Converted Python dictionary-based items to proper C# class hierarchy
    /// - Added serialization attributes for Unity Inspector
    /// - Implemented inheritance for specialized item types
    /// </summary>
    [Serializable]
    public class Item : MonoBehaviour
    {
        #region Database Fields
        
        // Database primary key
        [HideInInspector]
        public int Id;
        
        #endregion
    }
}
            public string StatType;
            
            [Tooltip("Amount to modify the stat")]
            public int Value;
            
            [Tooltip("Is this a percentage boost?")]
            public bool IsPercentage;
        }
        
        [Header("Stat Bonuses")]
        [Tooltip("Stat bonuses provided when equipped")]
        public List<StatBonus> StatBonuses = new List<StatBonus>();
        
        #endregion
        
        #region Unity Lifecycle
        
        protected virtual void Awake()
        {
            // Initialize durability if not set
            if (HasDurability && CurrentDurability <= 0)
            {
                CurrentDurability = MaxDurability;
            }
        }
        
        #endregion
        
        #region Item Functionality
        
        /// <summary>
        /// Use the item (to be overridden by derived classes)
        /// </summary>
        /// <param name="character">Character using the item</param>
        /// <returns>True if item was used successfully</returns>
        public virtual bool Use(Character character)
        {
            if (character == null || IsBroken)
                return false;
                
            // Base items are not directly usable unless they're consumable
            if (IsConsumable)
            {
                // Trigger the item used event
                OnItemUsed?.Invoke(this);
                
                // Reduce quantity
                Quantity--;
                
                return true;
            }
            
            Debug.Log($"{ItemName} cannot be used directly");
            return false;
        }
        
        /// <summary>
        /// Equip the item (to be overridden by derived classes)
        /// </summary>
        /// <param name="character">Character equipping the item</param>
        /// <returns>True if item was equipped successfully</returns>
        public virtual bool Equip(Character character)
        {
            if (character == null || !IsEquippable || IsBroken)
                return false;
                
            // Check level requirement
            if (character.Level < RequiredLevel)
            {
                Debug.Log($"{character.CharacterName} is not high enough level to equip {ItemName}");
                return false;
            }
            
            // Apply stat bonuses
            if (character.Stats != null)
            {
                character.Stats.ApplyItemStatBonuses(this);
            }
            
            // Mark as equipped
            Equipped = true;
            Debug.Log($"{character.CharacterName} equipped {ItemName}");
            
            return true;
        }
        
        /// <summary>
        /// Unequip the item
        /// </summary>
        /// <param name="character">Character unequipping the item</param>
        /// <returns>True if item was unequipped successfully</returns>
        public virtual bool Unequip(Character character)
        {
            if (character == null || !Equipped)
                return false;
                
            // Remove stat bonuses
            if (character.Stats != null)
            {
                character.Stats.RemoveItemStatBonuses(this);
            }
            
            // Mark as unequipped
            Equipped = false;
            Debug.Log($"{character.CharacterName} unequipped {ItemName}");
            
            return true;
        }
        
        /// <summary>
        /// Reduce item durability
        /// </summary>
        /// <param name="amount">Amount to reduce</param>
        /// <returns>True if item is now broken</returns>
        public bool ReduceDurability(int amount)
        {
            if (!HasDurability)
                return false;
                
            CurrentDurability -= amount;
            
            if (CurrentDurability <= 0)
            {
                Debug.Log($"{ItemName} has broken!");
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Repair item to full durability
        /// </summary>
        /// <returns>Amount of durability restored</returns>
        public int Repair()
        {
            if (!HasDurability)
                return 0;
                
            int amountRestored = MaxDurability - CurrentDurability;
            CurrentDurability = MaxDurability;
            
            Debug.Log($"{ItemName} has been repaired for {amountRestored} durability points");
            return amountRestored;
        }
        
        #endregion
        
        #region Rarity Functions
        
        /// <summary>
        /// Get a multiplier for item value based on rarity
        /// </summary>
        /// <returns>Value multiplier</returns>
        public float GetRarityValueMultiplier()
        {
            switch (Rarity.ToLower())
            {
                case "common": return 1.0f;
                case "uncommon": return 2.0f;
                case "rare": return 4.0f;
                case "epic": return 8.0f;
                case "legendary": return 16.0f;
                default: return 1.0f;
            }
        }
        
        /// <summary>
        /// Get the color associated with the item's rarity
        /// </summary>
        /// <returns>Color for the item rarity</returns>
        public Color GetRarityColor()
        {
            switch (Rarity.ToLower())
            {
                case "common": return Color.white;
                case "uncommon": return Color.green;
                case "rare": return Color.blue;
                case "epic": return new Color(0.5f, 0f, 0.5f);  // Purple
                case "legendary": return Color.yellow;
                default: return Color.white;
            }
        }
        
        /// <summary>
        /// Get a rarity tier as an integer
        /// </summary>
        /// <returns>Rarity tier (1-5)</returns>
        public int GetRarityTier()
        {
            switch (Rarity.ToLower())
            {
                case "common": return 1;
                case "uncommon": return 2;
                case "rare": return 3;
                case "epic": return 4;
                case "legendary": return 5;
                default: return 1;
            }
        }
        
        #endregion
        
        #region Persistence
        
        /// <summary>
        /// Save item data to player prefs
        /// </summary>
        /// <param name="slotId">Inventory slot ID</param>
        public void SaveToPlayerPrefs(string slotId)
        {
            string saveKey = $"Item_{slotId}";
            
            // Create save data dictionary
            var saveData = new Dictionary<string, object>
            {
                {"Id", Id},
                {"ItemName", ItemName},
                {"Quantity", Quantity},
                {"Equipped", Equipped},
                {"CurrentDurability", CurrentDurability},
                {"ItemType", ItemType},
                {"Rarity", Rarity}
            };
            
            // Convert to JSON and save
            string jsonData = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString(saveKey, jsonData);
            PlayerPrefs.Save();
            
            Debug.Log($"Saved item {ItemName} to slot {slotId}");
        }
        
        /// <summary>
        /// Save item to database
        /// </summary>
        public void SaveToDatabase()
        {
            // Placeholder for database functionality
            Debug.Log("SaveToDatabase: This will be implemented with SQLite integration");
            
            // Example implementation:
            // DatabaseManager.Instance.SaveItem(Id, ItemName, Description, ItemType, 
            //     Rarity, BaseValue, RequiredLevel, Stackable, MaxStackSize, etc.);
        }
        
        #endregion
        
        #region Factory Methods
        
        /// <summary>
        /// Create a new item from template data
        /// </summary>
        /// <param name="templateId">Template ID from database</param>
        /// <returns>New item instance</returns>
        public static Item CreateFromTemplate(int templateId)
        {
            // This would normally load from database
            Debug.Log($"CreateFromTemplate: This will load template {templateId} from database");
            
            // Example implementation:
            // var template = DatabaseManager.Instance.GetItemTemplate(templateId);
            // if (template != null) {
            //    var item = new Item();
            //    // Copy template properties...
            //    return item;
            // }
            return null;
        }
        
        /// <summary>
        /// Generate a random item with specified parameters
        /// </summary>
        /// <param name="minLevel">Minimum item level</param>
        /// <param name="maxLevel">Maximum item level</param>
        /// <param name="itemType">Specific item type or null for random</param>
        /// <param name="rarityChances">Probability distribution for rarities</param>
        /// <returns>Randomly generated item</returns>
        public static Item GenerateRandomItem(int minLevel, int maxLevel, string itemType = null, float[] rarityChances = null)
        {
            // This is a placeholder for a random item generator
            Debug.Log($"GenerateRandomItem: Would generate random item level {minLevel}-{maxLevel}");
            
            // In a full implementation, this would:
            // 1. Select random item type if not specified
            // 2. Roll for rarity based on rarityChances
            // 3. Select random base stats appropriate for level and type
            // 4. Generate appropriate modifiers based on rarity
            return null;
        }
        
        #endregion
    }
}

using System;
using UnityEngine;

namespace PDXUnderground.Models
{
    /// <summary>
    /// Base Item class representing game items.
    /// Maps to the 'items' table in the database.
    /// 
    /// Migration notes from Python:
    /// - Converted Python dictionary-based items to proper C# class hierarchy
    /// - Added serialization attributes for Unity Inspector
    /// - Implemented inheritance for specialized item types
    /// </summary>
    [Serializable]
    public class Item : MonoBehaviour
    {
        // Database primary key
        [HideInInspector]
        public int Id;
        
        // Basic item information
        [Header("Basic Information")]
        public string ItemName;
        [TextArea(3, 5)]
        public string Description;
        
        // Item classification
        [Header("Classification")]
        [Tooltip("weapon, armor, consumable, quest, misc")]
        public string ItemType;
        [Tooltip("common, uncommon, rare, epic, legendary")]
        public string Rarity;
        
        // Item attributes
        [Header("Attributes")]
        [Tooltip("Base value in game currency")]
        public int BaseValue;
        [Tooltip("Minimum level required to use this item")]
        public int RequiredLevel = 1;
        [Tooltip("Whether multiple items can stack in one inventory slot")]
        public bool Stackable = false;
        [Tooltip("Maximum number of items in a stack (if stackable)")]
        public int MaxStackSize = 1;
        
        // Visual representation
        [Header("Visual Representation")]
        [Tooltip("Path to item icon for UI")]
        public string IconPath;
        [Tooltip("Path to 3D model (if applicable)")]
        public string ModelPath;
        
        // Hidden attributes for specialized items
        [HideInInspector] public string WeaponType;  // For weapons
        [HideInInspector] public string ArmorType;   // For armor
        
        // Current state (not saved to database)
        [NonSerialized] private int _quantity = 1;
        [NonSerialized] private bool _equipped = false;
        [NonSerialized] private int _currentDurability;
        [NonSerialized] private int _maxDurability;
        
        // Public accessors with range constraints
        public int Quantity 
        { 
            get => _quantity;
            set => _quantity = Mathf.Clamp(value, 0, Stackable ? MaxStackSize : 1);
        }
        
        public bool Equipped
        {
            get => _equipped;
            set => _equipped = value;
        }
        
        public int CurrentDurability
        {
            get => _currentDurability;
            set => _currentDurability = Mathf.Clamp(value, 0, MaxDurability);
        }
        
        public int MaxDurability
        {
            get => _maxDurability;
            set => _maxDurability = value;
        }
        
        // Calculated properties
        public bool HasDurability => MaxDurability > 0;
        public float DurabilityPercentage => HasDurability ? (float)CurrentDurability / MaxDurability : 1f;
        public bool IsBroken => HasDurability && CurrentDurability <= 0;
        public int SellValue => Mathf.RoundToInt(BaseValue * GetRarityValueMultiplier() * DurabilityPercentage);
        public Color RarityColor => GetRarityColor();
        
        #region Unity Lifecycle
        
        protected virtual void Awake()
        {
            // Initialize durability if not set
            if (HasDurability && CurrentDurability <= 0)
            {
                CurrentDurability = MaxDurability;
            }
        }
        
        #endregion
        
        #region Item Functionality
        
        /// <summary>
        /// Use the item (to be overridden by derived classes)
        /// </summary>
        /// <param name="character">Character using the item</param>
        /// <returns>True if item was used successfully</returns>
        public virtual bool Use(Character character)
        {
            // Base items are not directly usable
            Debug.Log($"{ItemName} cannot be used directly");
            return false;
        }
        
        /// <summary>
        /// Equip the item (to be overridden by derived classes)
        /// </summary>
        /// <param name="character">Character equipping the item</param>
        /// <returns>True if item was equipped successfully</returns>
        public virtual bool Equip(Character character)
        {
            if (character.CanEquipItem(this))
            {
                Equipped = true;
                return true;
            }
            
            Debug.Log($"{character.CharacterName} cannot equip {ItemName}");
            

