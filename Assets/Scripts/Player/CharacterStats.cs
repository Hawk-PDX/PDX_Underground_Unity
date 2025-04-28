using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PDXUnderground.Models;

namespace PDXUnderground.Player
{
    /// <summary>
    /// Manages all character statistics including base stats, temporary buffs,
    /// derived stats, and stat modifications from items and effects.
    /// </summary>
    [Serializable]
    public class CharacterStats : MonoBehaviour
    {
        #region Base Stats
        [Header("Primary Stats")]
        [SerializeField] private int _strength = 10;
        [SerializeField] private int _agility = 10;
        [SerializeField] private int _intellect = 10;
        [SerializeField] private int _stamina = 10;
        [SerializeField] private int _charisma = 10;
        [SerializeField] private int _luck = 10;

        [Header("Resources")]
        [SerializeField] private int _maxHealth = 100;
        [SerializeField] private int _currentHealth = 100;
        [SerializeField] private int _maxMana = 50;
        [SerializeField] private int _currentMana = 50;
        [SerializeField] private int _maxEnergy = 100;
        [SerializeField] private int _currentEnergy = 100;

        [Header("Combat Stats")]
        [SerializeField] private float _criticalChance = 0.05f;
        [SerializeField] private float _criticalMultiplier = 1.5f;
        [SerializeField] private float _dodgeChance = 0.05f;
        [SerializeField] private float _parryChance = 0.0f;
        [SerializeField] private float _blockChance = 0.0f;
        [SerializeField] private float _hitChance = 0.9f;
        [SerializeField] private float _damageReduction = 0.0f;
        [SerializeField] private float _magicResistance = 0.0f;
        #endregion

        #region Derived Stats
        [Header("Derived Stats")]
        [SerializeField] private int _attackPower = 0;
        [SerializeField] private int _spellPower = 0;
        [SerializeField] private int _armor = 0;
        [SerializeField] private int _healthRegen = 1;
        [SerializeField] private int _manaRegen = 1;
        [SerializeField] private int _energyRegen = 5;
        #endregion

        #region Public Properties
        // Primary Stats
        public int Strength 
        { 
            get { return _strength + GetTotalStatBonus("strength"); }
            set { _strength = value; RecalculateDerivedStats(); }
        }

        public int Agility
        {
            get { return _agility + GetTotalStatBonus("agility"); }
            set { _agility = value; RecalculateDerivedStats(); }
        }

        public int Intellect
        {
            get { return _intellect + GetTotalStatBonus("intellect"); }
            set { _intellect = value; RecalculateDerivedStats(); }
        }

        public int Stamina
        {
            get { return _stamina + GetTotalStatBonus("stamina"); }
            set { _stamina = value; RecalculateDerivedStats(); }
        }

        public int Charisma
        {
            get { return _charisma + GetTotalStatBonus("charisma"); }
            set { _charisma = value; }
        }

        public int Luck
        {
            get { return _luck + GetTotalStatBonus("luck"); }
            set { _luck = value; RecalculateDerivedStats(); }
        }

        // Resources
        public int MaxHealth
        {
            get { return _maxHealth + GetTotalStatBonus("max_health") + (Stamina * 10); }
            set { _maxHealth = value; }
        }

        public int CurrentHealth
        {
            get { return _currentHealth; }
            set { _currentHealth = Mathf.Clamp(value, 0, MaxHealth); }
        }

        public int MaxMana
        {
            get { return _maxMana + GetTotalStatBonus("max_mana") + (Intellect * 5); }
            set { _maxMana = value; }
        }

        public int CurrentMana
        {
            get { return _currentMana; }
            set { _currentMana = Mathf.Clamp(value, 0, MaxMana); }
        }

        public int MaxEnergy
        {
            get { return _maxEnergy + GetTotalStatBonus("max_energy") + (Agility * 3); }
            set { _maxEnergy = value; }
        }

        public int CurrentEnergy
        {
            get { return _currentEnergy; }
            set { _currentEnergy = Mathf.Clamp(value, 0, MaxEnergy); }
        }

        // Combat Stats
        public float CriticalChance
        {
            get { return _criticalChance + GetTotalStatBonusPercentage("critical_chance") + (Luck * 0.001f); }
            set { _criticalChance = value; }
        }

        public float CriticalMultiplier
        {
            get { return _criticalMultiplier + GetTotalStatBonusPercentage("critical_multiplier"); }
            set { _criticalMultiplier = value; }
        }

        public float DodgeChance
        {
            get { return _dodgeChance + GetTotalStatBonusPercentage("dodge_chance") + (Agility * 0.002f); }
            set { _dodgeChance = value; }
        }

        public float ParryChance
        {
            get { return _parryChance + GetTotalStatBonusPercentage("parry_chance"); }
            set { _parryChance = value; }
        }

        public float BlockChance
        {
            get { return _blockChance + GetTotalStatBonusPercentage("block_chance"); }
            set { _blockChance = value; }
        }

        public float HitChance
        {
            get { return _hitChance + GetTotalStatBonusPercentage("hit_chance"); }
            set { _hitChance = value; }
        }

        public float DamageReduction
        {
            get { return _damageReduction + GetTotalStatBonusPercentage("damage_reduction") + (Armor * 0.0005f); }
            set { _damageReduction = Mathf.Clamp01(value); }
        }

        public float MagicResistance
        {
            get { return _magicResistance + GetTotalStatBonusPercentage("magic_resistance") + (Intellect * 0.001f); }
            set { _magicResistance = Mathf.Clamp01(value); }
        }

        // Derived Stats
        public int AttackPower
        {
            get { return _attackPower + GetTotalStatBonus("attack_power") + (Strength * 2); }
            set { _attackPower = value; }
        }

        public int SpellPower
        {
            get { return _spellPower + GetTotalStatBonus("spell_power") + (Intellect * 2); }
            set { _spellPower = value; }
        }

        public int Armor
        {
            get { return _armor + GetTotalStatBonus("armor"); }
            set { _armor = value; }
        }

        public int HealthRegen
        {
            get { return _healthRegen + GetTotalStatBonus("health_regen") + (Stamina / 5); }
            set { _healthRegen = value; }
        }

        public int ManaRegen
        {
            get { return _manaRegen + GetTotalStatBonus("mana_regen") + (Intellect / 5); }
            set { _manaRegen = value; }
        }

        public int EnergyRegen
        {
            get { return _energyRegen + GetTotalStatBonus("energy_regen") + (Agility / 5); }
            set { _energyRegen = value; }
        }

        // Additional Properties
        public float HealthPercentage => (float)CurrentHealth / MaxHealth;
        public float ManaPercentage => (float)CurrentMana / MaxMana;
        public float EnergyPercentage => (float)CurrentEnergy / MaxEnergy;
        #endregion

        #region Stat Bonuses
        // Stat bonus structure
        [Serializable]
        public class StatBoost
        {
            public string statName;         // Name of the stat
            public int value;               // Bonus value
            public bool isPercentage;       // Whether it's a percentage or flat bonus
            public float duration;          // Duration in seconds (-1 = permanent)
            public float startTime;         // Time when the boost was applied
            public string source;           // Source of the boost (item, buff, etc.)

            public bool IsExpired => duration > 0 && (Time.time - startTime) > duration;
            public float RemainingTime => duration <= 0 ? -1 : Mathf.Max(0, duration - (Time.time - startTime));
        }

        // List of all active stat bonuses
        private List<StatBoost> statBoosts = new List<StatBoost>();
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Initialize stats
            RecalculateDerivedStats();
        }

        private void Start()
        {
            // Set initial health/mana/energy to maximums
            CurrentHealth = MaxHealth;
            CurrentMana = MaxMana;
            CurrentEnergy = MaxEnergy;
        }

        private void Update()
        {
            // Check for expired stat boosts
            RemoveExpiredStatBoosts();

            // Add regeneration (in a real game, this would be at fixed intervals)
            if (Time.frameCount % 60 == 0)  // Roughly once per second at 60 FPS
            {
                RegenerateResources();
            }
        }
        #endregion

        #region Stat Modification Methods
        /// <summary>
        /// Add a temporary or permanent stat boost
        /// </summary>
        /// <param name="statName">Name of the stat to boost</param>
        /// <param name="value">Amount to boost by</param>
        /// <param name="isPercentage">Whether the boost is a percentage</param>
        /// <param name="duration">Duration in seconds, -1 for permanent</param>
        /// <param name="source">Source of the boost</param>
        /// <returns>True if the boost was applied</returns>
        public bool AddStatBoost(string statName, int value, bool isPercentage, float duration, string source)
        {
            if (string.IsNullOrEmpty(statName) || value == 0)
                return false;

            // Create the boost
            StatBoost boost = new StatBoost
            {
                statName = statName.ToLower(),
                value = value,
                isPercentage = isPercentage,
                duration = duration,
                startTime = Time.time,
                source = source
            };

            // Add to the list
            statBoosts.Add(boost);

            // Recalculate if needed
            RecalculateDerivedStats();

            Debug.Log($"Added {value}{(isPercentage ? "%" : "")} {statName} boost from {source} " +
                      $"for {(duration < 0 ? "permanent" : $"{duration}s")}");

            return true;
        }

        /// <summary>
        /// Remove all stat boosts from a specific source
        /// </summary>
        /// <param name="source">Source of the boosts to remove</param>
        public void RemoveStatBoostsFromSource(string source)
        {
            if (string.IsNullOrEmpty(source))
                return;

            int count = statBoosts.RemoveAll(b => b.source == source);

            if (count > 0)
            {
                Debug.Log($"Removed {count} stat boosts from source: {source}");
                RecalculateDerivedStats();
            }
        }

        /// <summary>
        /// <summary>
        /// Remove all expired stat boosts
        /// </summary>
        public void RemoveExpiredStatBoosts()
        {
            int count = statBoosts.RemoveAll(b => b.IsExpired);
            
            if (count > 0)
            {
                Debug.Log($"Removed {count} expired stat boosts");
                RecalculateDerivedStats();
            }
        }
        
        /// <summary>
        /// Remove expired stat boosts from a specific source
        /// </summary>
        /// <param name="source">Source of the boosts to check</param>
        public void RemoveExpiredStatBoosts(string source)
        {
            if (string.IsNullOrEmpty(source))
                return;
                
            int count = statBoosts.RemoveAll(b => b.source == source && b.IsExpired);
            
            if (count > 0)
            {
                Debug.Log($"Removed {count} expired stat boosts from source: {source}");
                RecalculateDerivedStats();
            }
        }
        
        /// <summary>
        /// Get the total bonus value for a specific stat (non-percentage)
        /// </summary>
        /// <param name="statName">Name of the stat</param>
        /// <returns>Total bonus value</returns>
        public int GetTotalStatBonus(string statName)
        {
            if (string.IsNullOrEmpty(statName))
                return 0;
                
            statName = statName.ToLower();
            int totalBonus = 0;
            
            // Sum up all non-percentage bonuses for this stat
            foreach (var boost in statBoosts)
            {
                if (boost.statName == statName && !boost.isPercentage && !boost.IsExpired)
                {
                    totalBonus += boost.value;
                }
            }
            
            return totalBonus;
        }
        
        /// <summary>
        /// Get the total percentage bonus for a specific stat
        /// </summary>
        /// <param name="statName">Name of the stat</param>
        /// <returns>Total percentage bonus as a decimal (0.1 = 10%)</returns>
        public float GetTotalStatBonusPercentage(string statName)
        {
            if (string.IsNullOrEmpty(statName))
                return 0f;
                
            statName = statName.ToLower();
            float totalPercentage = 0f;
            
            // Sum up all percentage bonuses for this stat
            foreach (var boost in statBoosts)
            {
                if (boost.statName == statName && boost.isPercentage && !boost.IsExpired)
                {
                    totalPercentage += boost.value / 100f; // Convert percentage to decimal
                }
            }
            
            return totalPercentage;
        }
        
        /// <summary>
        /// Recalculate all derived stats based on primary stats
        /// </summary>
        public void RecalculateDerivedStats()
        {
            // Calculate attack power based on strength
            _attackPower = Strength * 2;
            
            // Calculate spell power based on intellect
            _spellPower = Intellect * 2;
            
            // Calculate armor based on agility
            _armor = Agility * 5;
            
            // Regeneration rates based on primary stats
            _healthRegen = 1 + (Stamina / 5);
            _manaRegen = 1 + (Intellect / 5);
            _energyRegen = 5 + (Agility / 5);
            
            // Additional calculated stats could be added here
        }
        
        /// <summary>
        /// Regenerate health, mana, and energy based on regen rates
        /// </summary>
        private void RegenerateResources()
        {
            // Regenerate health if not at max
            if (CurrentHealth < MaxHealth)
            {
                CurrentHealth += HealthRegen;
            }
            
            // Regenerate mana if not at max
            if (CurrentMana < MaxMana)
            {
                CurrentMana += ManaRegen;
            }
            
            // Regenerate energy if not at max
            if (CurrentEnergy < MaxEnergy)
            {
                CurrentEnergy += EnergyRegen;
            }
        }
        
        /// <summary>
        /// Apply stat bonuses from an item
        /// </summary>
        /// <param name="item">Item to apply bonuses from</param>
        public void ApplyItemStatBonuses(Item item)
        {
            if (item == null || item.StatBonuses == null)
                return;
                
            string source = $"Item:{item.Id}";
            
            // Remove any existing bonuses from this item first
            RemoveStatBoostsFromSource(source);
            
            // Apply each stat bonus from the item
            foreach (Item.StatBonus bonus in item.StatBonuses)
            {
                AddStatBoost(
                    bonus.StatType,
                    bonus.Value,
                    bonus.IsPercentage,
                    -1, // Permanent until item is unequipped
                    source
                );
            }
        }
        
        /// <summary>
        /// Remove stat bonuses from an item
        /// </summary>
        /// <param name="item">Item to remove bonuses from</param>
        public void RemoveItemStatBonuses(Item item)
        {
            if (item == null)
                return;
                
            RemoveStatBoostsFromSource($"Item:{item.Id}");
        }
        #endregion
    }
}
