using System;
using System.Collections.Generic;
using UnityEngine;
using PDXUnderground.Models;

namespace PDXUnderground.Models
{
    /// <summary>
    /// Armor item class that extends the base Item class.
    /// Adds armor-specific properties and behaviors.
    /// Maps to the 'armor' table in the database.
    /// </summary>
    [Serializable]
    public class ArmorItem : Item
    {
        #region Armor Properties

        [Header("Armor Properties")]
        [Tooltip("Armor type (light, medium, heavy, shield)")]
        [SerializeField]
        private string _type;
        
        /// <summary>
        /// Armor type (light, medium, heavy, shield)
        /// </summary>
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        [Tooltip("Base defense value before modifiers")]
        [SerializeField]
        private int _baseDefense;
        
        /// <summary>
        /// Base defense value before modifiers
        /// </summary>
        public int BaseDefense
        {
            get { return _baseDefense; }
            set { _baseDefense = value; }
        }

        /// <summary>
        /// Magic defense value for the armor
        /// </summary>
        public float MagicDefense { get; set; }

        /// <summary>
        /// Armor class specification (alternative to Type property)
        /// </summary>
        public string ArmorClass { get; set; }

        /// <summary>
        /// General damage reduction percentage
        /// </summary>
        public float DamageReduction { get; set; }

        /// <summary>
        /// Magic damage resistance percentage
        /// </summary>
        public float MagicResistance { get; set; }

        [Tooltip("Which equipment slot this armor occupies")]
        [SerializeField]
        private string _slotType = "body"; // head, body, legs, hands, feet
        /// <summary>
        /// Which equipment slot this armor occupies (head, body, legs, hands, feet).
        /// This property hides the base SlotType property with armor-specific implementation.
        /// </summary>
        public new string SlotType 
        {
            get { return _slotType; }
            set { _slotType = value; }
        }

        [Tooltip("Physical damage reduction percentage")]
        [Range(0f, 0.5f)]
        [SerializeField]
        private float _physicalResistance = 0f;
        
        /// <summary>
        /// Physical damage reduction percentage
        /// </summary>
        public float PhysicalResistance
        {
            get { return _physicalResistance; }
            set { _physicalResistance = Mathf.Clamp(value, 0f, 0.5f); }
        }

        [Tooltip("Magic damage reduction percentage")]
        [Range(0f, 0.5f)]
        [SerializeField]
        private float _magicalResistance = 0f;
        
        /// <summary>
        /// Magic damage reduction percentage
        /// </summary>
        public float MagicalResistance
        {
            get { return _magicalResistance; }
            set { _magicalResistance = Mathf.Clamp(value, 0f, 0.5f); }
        }

        [Tooltip("Fire damage reduction percentage")]
        [Range(0f, 0.5f)]
        [SerializeField]
        private float _fireResistance = 0f;
        
        /// <summary>
        /// Fire damage reduction percentage
        /// </summary>
        public float FireResistance
        {
            get { return _fireResistance; }
            set { _fireResistance = Mathf.Clamp(value, 0f, 0.5f); }
        }

        [Tooltip("Cold damage reduction percentage")]
        [Range(0f, 0.5f)]
        [SerializeField]
        private float _coldResistance = 0f;
        
        /// <summary>
        /// Cold damage reduction percentage
        /// </summary>
        public float ColdResistance
        {
            get { return _coldResistance; }
            set { _coldResistance = Mathf.Clamp(value, 0f, 0.5f); }
        }

        [Tooltip("Lightning damage reduction percentage")]
        [Range(0f, 0.5f)]
        [SerializeField]
        private float _lightningResistance = 0f;
        
        /// <summary>
        /// Lightning damage reduction percentage
        /// </summary>
        public float LightningResistance
        {
            get { return _lightningResistance; }
            set { _lightningResistance = Mathf.Clamp(value, 0f, 0.5f); }
        }

        [Tooltip("Poison damage reduction percentage")]
        [Range(0f, 0.5f)]
        [SerializeField]
        private float _poisonResistance = 0f;
        
        /// <summary>
        /// Poison damage reduction percentage
        /// </summary>
        public float PoisonResistance
        {
            get { return _poisonResistance; }
            set { _poisonResistance = Mathf.Clamp(value, 0f, 0.5f); }
        }

        [Tooltip("Ice damage reduction percentage (alternative name)")]
        [Range(0f, 0.5f)]
        [SerializeField]
        private float _iceResistance = 0f;
        
        /// <summary>
        /// Ice damage reduction percentage (alternative name)
        /// </summary>
        public float IceResistance
        {
            get { return _iceResistance; }
            set { _iceResistance = Mathf.Clamp(value, 0f, 0.5f); }
        }
        // Armor durability drain rates
        [Header("Durability")]
        [Tooltip("Durability points lost when taking damage")]
        [SerializeField]
        private int _durabilityLossPerHit = 1;
        
        /// <summary>
        /// Durability points lost when taking damage
        /// </summary>
        public int DurabilityLossPerHit
        {
            get { return _durabilityLossPerHit; }
            set { _durabilityLossPerHit = Mathf.Max(1, value); }
        }

        [Tooltip("Chance for durability to not decrease (0-1)")]
        [Range(0f, 1f)]
        [SerializeField]
        private float _durabilityPreservationChance = 0f;
        
        /// <summary>
        /// Chance for durability to not decrease (0-1)
        /// </summary>
        public float DurabilityPreservationChance
        {
            get { return _durabilityPreservationChance; }
            set { _durabilityPreservationChance = Mathf.Clamp01(value); }
        }

        // Set based on armor type
        [HideInInspector]
        [SerializeField]
        private float _movementPenalty = 0f;
        
        /// <summary>
        /// Movement speed penalty imposed by this armor
        /// </summary>
        public float MovementPenalty
        {
            get { return _movementPenalty; }
            set { _movementPenalty = value; }
        }

        [HideInInspector]
        [SerializeField]
        private float _staminaDrainMultiplier = 1f;
        
        /// <summary>
        /// Multiplier affecting stamina drain rate while wearing this armor
        /// </summary>
        public float StaminaDrainMultiplier
        {
            get { return _staminaDrainMultiplier; }
            set { _staminaDrainMultiplier = Mathf.Max(1f, value); }
        }

        /// <summary>
        /// Current durability as a percentage (0-1)
        /// </summary>
        public float DurabilityPercentage
        {
            get
            {
                if (MaxDurability <= 0) return 1f;
                return Mathf.Clamp01((float)CurrentDurability / MaxDurability);
            }
        }

        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// Unity lifecycle method called when the component is first initialized.
        /// This initializes armor-specific defaults and properties.
        /// </summary>
        protected void Awake()
        {
            // Initialize armor properties
            
            // Set item type to armor if not already set
            if (string.IsNullOrEmpty(ItemType))
            {
                ItemType = "armor";
            }
            
            // Set armor type
            ArmorType = Type;
            
            // If maxDurability isn't set, set a default based on armor type
            if (MaxDurability <= 0)
            {
                switch (Type.ToLower())
                {
                    case "light":
                        MaxDurability = 60;
                        MovementPenalty = 0f;
                        StaminaDrainMultiplier = 1.0f;
                        break;
                    case "medium":
                        MaxDurability = 100;
                        MovementPenalty = 0.05f;
                        StaminaDrainMultiplier = 1.1f;
                        break;
                    case "heavy":
                        MaxDurability = 150;
                        MovementPenalty = 0.15f;
                        StaminaDrainMultiplier = 1.25f;
                        break;
                    case "shield":
                        MaxDurability = 120;
                        MovementPenalty = 0.05f;
                        StaminaDrainMultiplier = 1.1f;
                        break;
                    default:
                        MaxDurability = 80;
                        MovementPenalty = 0f;
                        StaminaDrainMultiplier = 1.0f;
                        break;
                }
                
                CurrentDurability = MaxDurability;
            }
        }

        #endregion

        #region Armor Functionality

        /// <summary>
        /// Calculate damage reduction for a specific damage type
        /// </summary>
        /// <param name="damageType">Type of damage being reduced</param>
        /// <returns>Damage reduction as a percentage (0-1)</returns>
        public float GetDamageReduction(string damageType)
        {
            if (IsBroken)
                return 0f;
                
            // Base reduction starts with physical or magical
            float reduction = 0f;
            
            switch (damageType.ToLower())
            {
                case "physical":
                    reduction = PhysicalResistance;
                    break;
                case "fire":
                    reduction = FireResistance;
                    break;
                case "cold":
                case "frost":
                case "ice":
                    reduction = ColdResistance;
                    break;
                case "lightning":
                case "electric":
                    reduction = LightningResistance;
                    break;
                case "poison":
                case "toxic":
                    reduction = PoisonResistance;
                    break;
                default:
                    // For other magic types
                    reduction = MagicalResistance;
                    break;
            }
            
            // Apply durability penalty (at 0% durability, effectiveness is halved)
            if (HasDurability)
            {
                reduction *= (0.5f + (DurabilityPercentage * 0.5f));
            }
            
            // Apply rarity bonus
            reduction *= GetRarityArmorMultiplier();
            
            return reduction;
        }
        
        /// <summary>
        /// Calculate total defense value for this armor when worn by a character.
        /// Takes into account character stats, armor quality and durability.
        /// </summary>
        /// <param name="character">Character wearing the armor</param>
        /// <returns>Final defense value as an integer</returns>
        public int GetDefense(CharacterData character)
        {
            if (IsBroken)
                return 0;
                
            float defense = BaseDefense;
            
            // Apply vitality bonus if character has stats
            if (character != null && character.Stats != null)
            {
                // 0.5% defense increase per point of vitality
                defense *= (1f + (character.Stats.Vitality * 0.005f));
            }
            
            // Apply quality modifier
            defense *= GetRarityArmorMultiplier();
            
            // Apply durability modifier
            if (HasDurability)
            {
                // At 0% durability, defense is reduced by 50%
                float durabilityMod = 0.5f + (DurabilityPercentage * 0.5f);
                defense *= durabilityMod;
            }
            
            return Mathf.RoundToInt(defense);
        }
        
        /// <summary>
        /// Handle taking damage to armor durability
        /// </summary>
        /// <param name="damage">Amount of damage received</param>
        /// <returns>True if armor is now broken</returns>
        public bool TakeDamage(int damage)
        {
            if (!HasDurability)
                return false;
                
            // Chance to avoid durability loss
            if (UnityEngine.Random.value < DurabilityPreservationChance)
                return false;
                
            // Calculate durability loss based on damage
            int durabilityLoss = DurabilityLossPerHit;
            
            // For larger hits, increase durability loss
            if (damage > 20)
            {
                durabilityLoss += Mathf.FloorToInt((damage - 20) / 10);
            }
            
            return ReduceDurability(durabilityLoss);
        }

        /// <summary>
        /// Reduce the durability of this armor
        /// </summary>
        /// <param name="amount">Amount to reduce durability by</param>
        /// <returns>True if item is now broken</returns>
        public bool ReduceDurability(int amount)
        {
            if (!HasDurability)
                return false;

            CurrentDurability = Mathf.Max(0, CurrentDurability - amount);
            
            // Check if the item is now broken
            if (CurrentDurability <= 0)
            {
                Debug.Log($"{ItemName} has broken!");
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Get armor effectiveness multiplier based on rarity
        /// </summary>
        /// <returns>Armor multiplier</returns>
        private float GetRarityArmorMultiplier()
        {
            switch (Rarity.ToLower())
            {
                case "common": return 1.0f;
                case "uncommon": return 1.1f;
                case "rare": return 1.2f;
                case "epic": return 1.35f;
                case "legendary": return 1.5f;
                default: return 1.0f;
            }
        }

        #endregion

        #region Item Overrides

        /// <summary>
        /// Override base Use method for armor-specific behavior
        /// </summary>
        /// <param name="character">Character using the item</param>
        /// <param name="currentTime">Current game time</param>
        /// <returns>True if successfully used</returns>
        public override bool Use(CharacterData character, float currentTime)
        {
            if (!base.Use(character, currentTime))
                return false;

            // Armor-specific use logic here
            if (HasDurability)
            {
                CurrentDurability--;
            }

            return true;
        }

        /// <summary>
        /// Equipment method for when character equips this armor
        /// </summary>
        /// <param name="character">Character equipping the armor</param>
        public override void OnEquip(CharacterData character)
        {
            if (character?.Stats == null) return;

            // Apply armor stats to character
            character.Stats.DamageReduction += DamageReduction;
            character.Stats.MagicResistance += MagicResistance;
            
            // Apply resistances
            if (FireResistance != 0)
                character.Stats.AddStatBoost("fire_resistance", FireResistance, true, -1, $"Armor:{ItemName}");
            if (IceResistance != 0)
                character.Stats.AddStatBoost("ice_resistance", IceResistance, true, -1, $"Armor:{ItemName}");
            if (LightningResistance != 0)
                character.Stats.AddStatBoost("lightning_resistance", LightningResistance, true, -1, $"Armor:{ItemName}");
            if (PoisonResistance != 0)
                character.Stats.AddStatBoost("poison_resistance", PoisonResistance, true, -1, $"Armor:{ItemName}");
                
            if (MovementPenalty != 0)
                character.Stats.AddStatBoost("movement_speed", -MovementPenalty, true, -1, $"Armor:{ItemName}");
        }

        /// <summary>
        /// Equipment method for when character unequips this armor
        /// </summary>
        /// <param name="character">Character unequipping the armor</param>
        public override void OnUnequip(CharacterData character)
        {
            if (character?.Stats == null) return;

            try
            {
                // Remove armor stats from character
                character.Stats.DamageReduction -= DamageReduction;
                character.Stats.MagicResistance -= MagicResistance;
                
                // Remove resistances
                if (FireResistance != 0)
                    character.Stats.RemoveStatBoost($"Armor:{ItemName}", "fire_resistance");
                if (IceResistance != 0)
                    character.Stats.RemoveStatBoost($"Armor:{ItemName}", "ice_resistance");
                if (LightningResistance != 0)
                    character.Stats.RemoveStatBoost($"Armor:{ItemName}", "lightning_resistance");
                if (PoisonResistance != 0)
                    character.Stats.RemoveStatBoost($"Armor:{ItemName}", "poison_resistance");
                    
                if (MovementPenalty != 0)
                    character.Stats.RemoveStatBoost($"Armor:{ItemName}", "movement_speed");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error when unequipping armor {ItemName}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Create a copy of this armor item with all properties intact
        /// </summary>
        /// <returns>A new ArmorItem that is a copy of this one</returns>
        public override Item CreateCopy()
        {
            var copy = (ArmorItem)base.CreateCopy();
            // Armor-specific properties are copied by MemberwiseClone
            return copy;
        }
        #endregion
    }
}
