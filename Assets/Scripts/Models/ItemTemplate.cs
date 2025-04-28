using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace PDXUnderground.Models
{
    /// <summary>
    /// Defines a template for creating game items.
    /// ItemTemplate serves as a blueprint from which individual Item instances can be created.
    /// </summary>
    [Serializable]
    [XmlRoot("ItemTemplate")]
    public class ItemTemplate
    {
        #region Basic Properties
        /// <summary>
        /// Unique identifier for this item template.
        /// </summary>
        [XmlElement("Id")]
        public int Id { get; set; }

        /// <summary>
        /// Display name of items created from this template.
        /// </summary>
        [XmlElement("ItemName")]
        public string ItemName { get; set; }

        /// <summary>
        /// Detailed description of items created from this template.
        /// </summary>
        [XmlElement("Description")]
        public string Description { get; set; }

        /// <summary>
        /// Category of the item (weapon, armor, consumable, etc.).
        /// </summary>
        [XmlElement("ItemType")]
        public string ItemType { get; set; }

        /// <summary>
        /// Rarity level of the item (common, rare, epic, etc.).
        /// </summary>
        [XmlElement("Rarity")]
        public string Rarity { get; set; }

        /// <summary>
        /// Base monetary value of the item.
        /// </summary>
        [XmlElement("Value")]
        public int Value { get; set; }

        /// <summary>
        /// Weight of the item in inventory.
        /// </summary>
        [XmlElement("Weight")]
        public float Weight { get; set; }

        /// <summary>
        /// Minimum character level required to use this item.
        /// </summary>
        [XmlElement("RequiredLevel")]
        public int RequiredLevel { get; set; }
        #endregion

        #region Stack Properties
        /// <summary>
        /// Determines if multiple instances of this item can be stacked in a single inventory slot.
        /// </summary>
        [XmlElement("IsStackable")]
        public bool IsStackable { get; set; }

        /// <summary>
        /// Maximum number of items that can be stacked together if stackable.
        /// </summary>
        [XmlElement("MaxStackSize")]
        public int MaxStackSize { get; set; } = 1;
        #endregion

        #region Durability Properties
        /// <summary>
        /// Determines if this item can be damaged or degraded with use.
        /// </summary>
        [XmlElement("HasDurability")]
        public bool HasDurability { get; set; }

        /// <summary>
        /// Maximum durability value when the item is in perfect condition.
        /// </summary>
        [XmlElement("MaxDurability")]
        public int MaxDurability { get; set; }
        #endregion

        #region Equipment Properties
        /// <summary>
        /// Determines if this item can be equipped by a character.
        /// </summary>
        [XmlElement("IsEquippable")]
        public bool IsEquippable { get; set; }

        /// <summary>
        /// The equipment slot this item occupies when equipped (head, chest, weapon, etc.).
        /// </summary>
        [XmlElement("SlotType")]
        public string SlotType { get; set; }

        /// <summary>
        /// Specific weapon category if this is a weapon (sword, axe, bow, etc.).
        /// </summary>
        [XmlElement("WeaponType")]
        public string WeaponType { get; set; }

        /// <summary>
        /// Armor weight class if this is armor (light, medium, heavy).
        /// </summary>
        [XmlElement("ArmorType")]
        public string ArmorType { get; set; }
        #endregion

        #region Consumable Properties
        /// <summary>
        /// Determines if this item can be consumed for an effect.
        /// </summary>
        [XmlElement("IsConsumable")]
        public bool IsConsumable { get; set; }

        /// <summary>
        /// Determines if this consumable item can be used multiple times.
        /// </summary>
        [XmlElement("IsReusable")]
        public bool IsReusable { get; set; }

        /// <summary>
        /// Cooldown time in seconds between uses if reusable.
        /// </summary>
        [XmlElement("Cooldown")]
        public float Cooldown { get; set; }
        #endregion

        #region Stats and Effects
        /// <summary>
        /// Dictionary of stat modifiers provided by this item.
        /// Keys represent stat names and values represent stat modifier values.
        /// </summary>
        [XmlIgnore] // Dictionary is not directly XML serializable
        private Dictionary<string, float> _stats = new Dictionary<string, float>();
        
        /// <summary>
        /// Dictionary of stat modifiers provided by this item.
        /// Keys represent stat names and values represent stat modifier values.
        /// </summary>
        [XmlIgnore]
        public Dictionary<string, float> Stats 
        { 
            get { return _stats; } 
            set { _stats = value ?? new Dictionary<string, float>(); } 
        }
        
        /// <summary>
        /// List of effect identifiers that this item provides.
        /// </summary>
        [XmlIgnore] // Lists are not directly XML serializable
        private List<string> _effects = new List<string>();
        
        /// <summary>
        /// List of effect identifiers that this item provides.
        /// </summary>
        [XmlIgnore]
        public List<string> Effects 
        { 
            get { return _effects; } 
            set { _effects = value ?? new List<string>(); } 
        }
        
        /// <summary>
        /// Serializable format of Stats for XML serialization.
        /// </summary>
        [XmlArray("Stats")]
        [XmlArrayItem("Stat")]
        public StatEntry[] StatsArray
        {
            get
            {
                var entries = new StatEntry[Stats.Count];
                int i = 0;
                foreach (var kvp in Stats)
                {
                    entries[i++] = new StatEntry { Name = kvp.Key, Value = kvp.Value };
                }
                return entries;
            }
            set
            {
                Stats.Clear();
                if (value != null)
                {
                    foreach (var entry in value)
                    {
                        Stats[entry.Name] = entry.Value;
                    }
                }
            }
        }
        
        /// <summary>
        /// Serializable format of Effects for XML serialization.
        /// </summary>
        [XmlArray("Effects")]
        [XmlArrayItem("Effect")]
        public string[] EffectsArray
        {
            get { return Effects.ToArray(); }
            set
            {
                Effects.Clear();
                if (value != null)
                {
                    Effects.AddRange(value);
                }
            }
        }
        #endregion
        
        #region Methods
        /// <summary>
        /// Creates a deep copy of this item template.
        /// </summary>
        /// <returns>A new ItemTemplate instance that is a copy of this template</returns>
        public ItemTemplate Clone()
        {
            var clone = (ItemTemplate)MemberwiseClone();
            clone.Stats = new Dictionary<string, float>(Stats);
            clone.Effects = new List<string>(Effects);
            return clone;
        }
        
        /// <summary>
        /// Validates this template for consistency and completeness.
        /// </summary>
        /// <returns>True if the template is valid, false otherwise</returns>
        public bool Validate()
        {
            // Essential fields validation
            if (string.IsNullOrEmpty(ItemName)) return false;
            if (string.IsNullOrEmpty(ItemType)) return false;
            
            // Numeric validation
            if (Value < 0) Value = 0;
            if (Weight < 0) Weight = 0;
            if (RequiredLevel < 0) RequiredLevel = 0;
            
            // Stack validation
            if (IsStackable && MaxStackSize < 1) MaxStackSize = 1;
            
            // Durability validation
            if (HasDurability && MaxDurability < 1) MaxDurability = 1;
            
            return true;
        }
        #endregion
    }
    
    /// <summary>
    /// Helper class for XML serialization of dictionary entries.
    /// </summary>
    [Serializable]
    public class StatEntry
    {
        /// <summary>
        /// The name/key of the stat.
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }
        
        /// <summary>
        /// The value of the stat.
        /// </summary>
        [XmlAttribute("value")]
        public float Value { get; set; }
    }
}
