using System;
using System.Collections.Generic;
using UnityEngine;

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
        public string Type;

        [Tooltip("Base defense value before modifiers")]
        public int BaseDefense;

        [Tooltip("Which equipment slot this armor occupies")]
        public string SlotType = "body"; // head, body, legs, hands, feet

        [Tooltip("Physical damage reduction percentage")]
        [Range(0f, 0.5f)]
        public float PhysicalResistance = 0f;

        [Tooltip("Magic damage reduction percentage")]
        [Range(0f, 0.5f)]
        public float MagicalResistance = 0f;

        [Tooltip("Fire damage reduction percentage")]
        [Range(0f, 0.5f)]
        public float FireResistance = 0f;

        [Tooltip("Cold damage reduction percentage")]
        [Range(0f, 0.5f)]
        public float ColdResistance = 0f;

        [Tooltip("Lightning damage reduction percentage")]
        [Range(0f, 0.5f)]
        public float LightningResistance = 0f;

        [Tooltip("Poison damage reduction percentage")]
        [Range(0f, 0.5f)]
        public float PoisonResistance = 0f;

        // Armor durability drain rates
        [Header("Durability")]
        [Tooltip("Durability points lost when taking damage")]
        public int DurabilityLossPerHit = 1;

        [Tooltip("Chance for durability to not decrease (0-1)")]
        [Range(0f, 1f)]
        public float DurabilityPreservationChance = 0f;

        // Set based on armor type
        [HideInInspector]
        public float MovementPenalty = 0f;

        [HideInInspector]
        public float StaminaDrainMultiplier = 1f;

        #endregion

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
            
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
        /// Calculate total defense value
        /// </summary>
        /// <param name="character">Character wearing the armor</param>
        /// <returns>Final defense value</returns>
        public int CalculateDefense(Character character)
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

        public override bool Equip(Character character)
        {
            if (!base.Equip(character))
                return false;
                
            // Add armor-specific stat bonuses
            if (character.Stats != null)
            {
                // Add damage reduction bonuses
                if (PhysicalResistance > 0)
                {
                    character.Stats.AddStatBoost(
                        "defense",
                        Mathf.RoundToInt(PhysicalResistance * 100),
                        true,
                        -1, // Permanent until unequipped
                        $"Armor:{ItemName}"
                    );
                }
                
                // Add magical resistance bonuses
                if (MagicalResistance > 0)
                {
                    character.Stats.AddStatBoost(
                        "magic_resistance",
                        Mathf.RoundToInt(MagicalResistance * 100),
                        true,
                        -1,
                        $"Armor:{ItemName}"
                    );
                }
                
                // Apply movement penalty if any
                if (MovementPenalty > 0)
                {
                    character.Stats.AddStatBoost(
                        "movement_speed",
                        -Mathf.RoundToInt(MovementPenalty * 100),
                        true,
                        -1,
                        $"Armor:{ItemName}"
                    );
                }
            }
            
            Debug.Log($"{character.CharacterName} equipped {ItemName} with {CalculateDefense(character)} defense");
            return true;
        }
        
        public override bool Unequip(Character character)
        {
            if (!base.Unequip(character))
                return false;
                
            Debug.Log($"{character.CharacterName} unequipped {ItemName} armor");
            return true;
        }

        #endregion
    }
}

