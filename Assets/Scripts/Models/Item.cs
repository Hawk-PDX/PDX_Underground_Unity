using System;
using PDXUnderground.Models;

namespace PDXUnderground.Models
{
    /// <summary>
    /// Represents an item in the game that can be collected, used, equipped, or traded.
    /// </summary>
    [Serializable]
    public class Item
    {
        #region Basic Properties
        /// <summary>
        /// Unique identifier for the item instance.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Name of the item.
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// Description of the item.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Type of item (weapon, armor, consumable, etc.).
        /// </summary>
        public string ItemType { get; set; }

        /// <summary>
        /// Rarity level of the item.
        /// </summary>
        public string Rarity { get; set; }

        /// <summary>
        /// Base value of the item in game currency.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Weight of the item affecting inventory capacity.
        /// </summary>
        public float Weight { get; set; }

        /// <summary>
        /// Minimum character level required to use this item.
        /// </summary>
        public int RequiredLevel { get; set; }
        #endregion

        #region Stack Properties
        /// <summary>
        /// Determines if multiple instances of this item can be stacked in a single inventory slot.
        /// </summary>
        public bool IsStackable { get; set; }

        /// <summary>
        /// Maximum number of items that can be stacked together if stackable.
        /// </summary>
        public int MaxStackSize { get; set; }

        /// <summary>
        /// Current number of items in this stack.
        /// </summary>
        public int Quantity { get; set; }
        #endregion

        #region Durability Properties
        /// <summary>
        /// Determines if this item can be damaged or degraded with use.
        /// </summary>
        public bool HasDurability { get; set; }

        /// <summary>
        /// Maximum durability value when the item is in perfect condition.
        /// </summary>
        public int MaxDurability { get; set; }

        /// <summary>
        /// Current durability value of the item.
        /// </summary>
        public int CurrentDurability { get; set; }

        /// <summary>
        /// Indicates if the item is broken and cannot be used until repaired.
        /// </summary>
        public bool IsBroken => HasDurability && CurrentDurability <= 0;
        #endregion

        #region Equipment Properties
        /// <summary>
        /// Determines if this item can be equipped by a character.
        /// </summary>
        public bool IsEquippable { get; set; }

        /// <summary>
        /// Indicates if this item is currently equipped by a character.
        /// </summary>
        public bool Equipped { get; set; }

        /// <summary>
        /// The equipment slot this item occupies when equipped (head, chest, weapon, etc.).
        /// </summary>
        public string SlotType { get; set; }

        /// <summary>
        /// Specific weapon category if this is a weapon (sword, axe, bow, etc.).
        /// </summary>
        public string WeaponType { get; set; }

        /// <summary>
        /// Armor weight class if this is armor (light, medium, heavy).
        /// </summary>
        public string ArmorType { get; set; }
        #endregion

        #region Consumable Properties
        /// <summary>
        /// Determines if this item can be consumed for an effect.
        /// </summary>
        public bool IsConsumable { get; set; }

        /// <summary>
        /// Determines if this consumable item can be used multiple times.
        /// </summary>
        public bool IsReusable { get; set; }

        /// <summary>
        /// Cooldown time in seconds between uses if reusable.
        /// </summary>
        public float Cooldown { get; set; }
        #endregion

        #region Item Usage Methods
        /// <summary>
        /// Base method for using an item. Should be overridden by specific item types.
        /// </summary>
        /// <param name="character">The character using the item</param>
        /// <param name="currentTime">Current game time for cooldown calculations</param>
        /// <returns>True if item was successfully used, false otherwise</returns>
        public virtual bool Use(CharacterData character, float currentTime)
        {
            if (IsBroken || character == null)
                return false;

            if (character.Level < RequiredLevel)
                return false;

            if (IsConsumable && Quantity <= 0)
                return false;

            return true;
        }

        /// <summary>
        /// Called when this item is equipped by a character.
        /// Base implementation does nothing - should be overridden by specific item types.
        /// </summary>
        /// <param name="character">The character equipping the item</param>
        public virtual void OnEquip(CharacterData character)
        {
            // Base implementation does nothing
        }

        /// <summary>
        /// Called when this item is unequipped by a character.
        /// Base implementation does nothing - should be overridden by specific item types.
        /// </summary>
        /// <param name="character">The character unequipping the item</param>
        public virtual void OnUnequip(CharacterData character)
        {
            // Base implementation does nothing
        }
        #endregion

        #region Item Creation Methods
        /// <summary>
        /// Creates a new copy of this item with reset equipped state and quantity.
        /// </summary>
        /// <returns>A new Item instance that is a copy of this item</returns>
        public virtual Item CreateCopy()
        {
            var copy = (Item)MemberwiseClone();
            copy.Equipped = false;
            copy.Quantity = 1;
            return copy;
        }

        /// <summary>
        /// Creates a new item instance from an item template.
        /// </summary>
        /// <param name="template">The template containing base item properties</param>
        /// <returns>A new Item instance based on the template, or null if template is null</returns>
        public static Item CreateFromTemplate(ItemTemplate template)
        {
            if (template == null)
                return null;

            // Validate essential template properties
            if (string.IsNullOrEmpty(template.ItemName))
                throw new ArgumentException("ItemTemplate must have a valid ItemName");

            if (string.IsNullOrEmpty(template.ItemType))
                throw new ArgumentException("ItemTemplate must have a valid ItemType");

            var item = new Item
            {
                ItemName = template.ItemName,
                Description = template.Description ?? string.Empty,
                ItemType = template.ItemType,
                Rarity = template.Rarity ?? "Common",
                Value = Math.Max(0, template.Value),
                Weight = Math.Max(0, template.Weight),
                RequiredLevel = Math.Max(0, template.RequiredLevel),
                IsStackable = template.IsStackable,
                MaxStackSize = Math.Max(1, template.MaxStackSize),
                Quantity = 1,
                HasDurability = template.HasDurability,
                MaxDurability = template.HasDurability ? Math.Max(1, template.MaxDurability) : 0,
                CurrentDurability = template.HasDurability ? Math.Max(1, template.MaxDurability) : 0,
                IsEquippable = template.IsEquippable,
                SlotType = template.IsEquippable ? (template.SlotType ?? string.Empty) : string.Empty,
                WeaponType = template.IsEquippable ? (template.WeaponType ?? string.Empty) : string.Empty,
                ArmorType = template.IsEquippable ? (template.ArmorType ?? string.Empty) : string.Empty,
                IsConsumable = template.IsConsumable,
                IsReusable = template.IsConsumable && template.IsReusable,
                Cooldown = template.IsConsumable ? Math.Max(0, template.Cooldown) : 0
            };

            return item;
        }
        #endregion
    }
}
