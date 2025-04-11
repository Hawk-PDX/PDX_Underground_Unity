using System;
using System.Collections.Generic;
using UnityEngine;

namespace PDXUnderground.Models
{
    /// <summary>
    /// Weapon item class that extends the base Item class.
    /// Adds weapon-specific properties and behaviors.
    /// Maps to the 'weapons' table in the database.
    /// </summary>
    [Serializable]
    public class WeaponItem : Item
    {
        #region Weapon Properties

        [Header("Weapon Properties")]
        [Tooltip("Weapon type (sword, bow, staff, etc.)")]
        public string Type;

        [Tooltip("Base damage before modifiers")]
        public int BaseDamage;

        [Tooltip("Damage type (physical, fire, frost, etc.)")]
        public string DamageType = "physical";

        [Tooltip("Attack speed in attacks per second")]
        [Range(0.1f, 5f)]
        public float AttackSpeed = 1f;

        [Tooltip("Attack range in units")]
        public float Range = 1f;

        [Tooltip("Critical hit chance bonus from this weapon")]
        [Range(0f, 0.5f)]
        public float CriticalChanceBonus = 0f;

        [Tooltip("Critical hit damage multiplier")]
        [Range(1f, 3f)]
        public float CriticalMultiplier = 1.5f;

        // Weapon stats for specific types
        [Header("Weapon Stats")]
        [Tooltip("For ranged weapons: accuracy modifier (0-1)")]
        [Range(0f, 1f)]
        public float Accuracy = 1f;

        [Tooltip("For melee weapons: parry chance (0-1)")]
        [Range(0f, 0.5f)]
        public float ParryChance = 0f;

        [Tooltip("For magic weapons: spell power bonus")]
        public int SpellPower = 0;

        // Weapon durability drain rates
        [Header("Durability")]
        [Tooltip("Durability points lost per attack")]
        public int DurabilityLossPerAttack = 1;

        [Tooltip("Chance for durability to not decrease (0-1)")]
        [Range(0f, 1f)]
        public float DurabilityPreservationChance = 0f;

        #endregion

        #region Calculated Properties

        // Calculated properties
        public float DamagePerSecond => BaseDamage * AttackSpeed;
        public float AverageDamage => BaseDamage * (1f + (CriticalChanceBonus * (CriticalMultiplier - 1f)));

        #endregion

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
            
            // Set item type to weapon if not already set
            if (string.IsNullOrEmpty(ItemType))
            {
                ItemType = "weapon";
            }
            
            // Set weapon type
            WeaponType = Type;
            
            // If maxDurability isn't set, set a default based on weapon type
            if (MaxDurability <= 0)
            {
                switch (Type.ToLower())
                {
                    case "sword":
                    case "axe":
                    case "mace":
                        MaxDurability = 100;
                        break;
                    case "bow":
                    case "crossbow":
                        MaxDurability = 80;
                        break;
                    case "staff":
                    case "wand":
                        MaxDurability = 60;
                        break;
                    default:
                        MaxDurability = 100;
                        break;
                }
                
                CurrentDurability = MaxDurability;
            }
        }

        #endregion

        #region Weapon Functionality

        /// <summary>
        /// Calculate damage for a single attack
        /// </summary>
        /// <param name="character">Character using the weapon</param>
        /// <returns>Final damage amount</returns>
        public virtual int CalculateDamage(Character character)
        {
            if (character == null || IsBroken)
                return 0;
                
            // Get base damage
            float damage = BaseDamage;
            
            // Apply strength bonus for physical weapons
            if (DamageType.ToLower() == "physical" && character.Stats != null)
            {
                // 1% damage increase per point of strength
                damage *= (1f + (character.Stats.Strength * 0.01f));
            }
            
            // Apply intellect bonus for magical weapons
            else if (DamageType.ToLower() != "physical" && character.Stats != null)
            {
                // 1.5% damage increase per point of intellect
                damage *= (1f + (character.Stats.Intellect * 0.015f));
                
                // Add spell power bonus
                damage += SpellPower;
            }
            
            // Apply weapon quality modifier
            damage *= GetRarityDamageMultiplier();
            
            // Apply durability modifier (damage decreases as weapon deteriorates)
            if (HasDurability)
            {
                // At 0% durability, damage is reduced by 50%
                float durabilityMod = 0.5f + (DurabilityPercentage * 0.5f);
                damage *= durabilityMod;
            }
            
            return Mathf.RoundToInt(damage);
        }
        
        /// <summary>
        /// Determine if an attack is a critical hit
        /// </summary>
        /// <param name="character">Character using the weapon</param>
        /// <returns>True if attack is a critical hit</returns>
        public virtual bool IsCriticalHit(Character character)
        {
            if (character == null || character.Stats == null)
                return false;
                
            // Base critical chance from character stats
            float critChance = character.Stats.CriticalChance;
            
            // Add weapon's critical chance bonus
            critChance += CriticalChanceBonus;
            
            // Roll for critical
            return UnityEngine.Random.value < critChance;
        }
        
        /// <summary>
        /// Calculate critical hit damage
        /// </summary>
        /// <param name="baseDamage">Base damage before critical</param>
        /// <param name="character">Character using the weapon</param>
        /// <returns>Critical damage amount</returns>
        public virtual int CalculateCriticalDamage(int baseDamage, Character character)
        {
            // Start with weapon's critical multiplier
            float multiplier = CriticalMultiplier;
            
            // Add character's critical multiplier if available
            if (character != null && character.Stats != null)
            {
                multiplier = Mathf.Max(multiplier, character.Stats.CriticalMultiplier);
            }
            
            return Mathf.RoundToInt(baseDamage * multiplier);
        }
        
        /// <summary>
        /// Attack with this weapon (to be called during combat)
        /// </summary>
        /// <param name="attacker">Character attacking with the weapon</param>
        /// <param name="target">Target being attacked</param>
        /// <returns>Damage dealt</returns>
        public virtual int Attack(Character attacker, Character target)
        {
            if (attacker == null || target == null || IsBroken)
                return 0;
                
            // Roll for hit (based on accuracy and target dodge)
            float hitChance = Accuracy;
            if (target.Stats != null)
            {
                hitChance -= target.Stats.DodgeChance;
            }
            
            // Ensure minimum hit chance of 5%
            hitChance = Mathf.Clamp(hitChance, 0.05f, 1f);
            
            // Check if attack hits
            if (UnityEngine.Random.value > hitChance)
            {
                Debug.Log($"{attacker.CharacterName}'s attack with {ItemName} missed {target.CharacterName}!");
                return 0;
            }
            
            // Calculate base damage
            int damage = CalculateDamage(attacker);
            
            // Check for critical hit
            bool isCritical = IsCriticalHit(attacker);
            if (isCritical)
            {
                damage = CalculateCriticalDamage(damage, attacker);
                Debug.Log($"{attacker.CharacterName} scores a critical hit with {ItemName} for {damage} damage!");
            }
            
            // Apply damage reduction from target
            if (target.Stats != null)
            {
                float reduction = DamageType.ToLower() == "physical" 
                    ? target.Stats.DamageReduction 
                    : target.Stats.MagicResistance;
                    
                damage = Mathf.RoundToInt(damage * (1f - reduction));
            }
            
            // Apply damage to target
            target.TakeDamage(damage, DamageType, attacker);
            
            // Reduce durability
            if (HasDurability && UnityEngine.Random.value > DurabilityPreservationChance)
            {
                ReduceDurability(DurabilityLossPerAttack);
            }
            
            return damage;
        }
        
        /// <summary>
        /// Get damage multiplier based on rarity
        /// </summary>
        /// <returns>Damage multiplier</returns>
        private float GetRarityDamageMultiplier()
        {
            switch (Rarity.ToLower())
            {
                case "common": return 1.0f;
                case "uncommon": return 1.2f;
                case "rare": return 1.4f;
                case "epic": return 1.7f;
                case "legendary": return 2.0f;
                default: return 1.0f;
            }
        }

        #endregion

        #region Item Overrides

        public override bool Equip(Character character)
        {
            if (!base.Equip(character))
                return false;
                
            // Add weapon-specific stat bonuses
            if (character.Stats != null)
            {
                // Add critical chance bonus
                if (CriticalChanceBonus > 0)
                {
                    character.Stats.AddStatBoost(
                        "critical_chance",
                        

