
        [Tooltip("Affects magic resistance and mana regeneration")]
        [Range(1, 100)]
        public int Wisdom = 10;

        [Tooltip("Affects NPC interactions and prices")]
        [Range(1, 100)]
        public int Charisma = 10;

        [Tooltip("Affects critical hit chance and random events")]
        [Range(1, 100)]
        public int Luck = 10;

        {
            // Base discount: 0% + 0.3% per point of charisma
            float baseDiscount = Charisma * 0.003f;
            
            // Apply boost modifiers
            float charismaBoost = GetStatBoostModifier("charisma");
            float discountBoost = GetStatBoostModifier("merchant_discount");
            
            // Cap at 50% to keep merchants profitable
            return Mathf.Min(0.5f, baseDiscount * (1 + charismaBoost) + discountBoost);
        }
        
        /// <summary>
        /// Calculate maximum health based on vitality and level
        /// </summary>
        /// <param name="level">Character level</param>
        /// <returns>Maximum health points</returns>
        public int CalculateMaxHealth(int level)
        {
            // Base health: BaseHealth + (10 per vitality) + (5 per level)
            int baseMaxHealth = BaseHealth + (Vitality * 10) + (level * 5);
            
            // Apply boost modifiers
            float vitalityBoost = GetStatBoostModifier("vitality");
            float healthBoost = GetStatBoostModifier("max_health");
            
            return Mathf.RoundToInt(baseMaxHealth * (1 + vitalityBoost) * (1 + healthBoost));
        }
        
        /// <summary>
        /// Calculate maximum mana based on intellect and level
        /// </summary>
        /// <param name="level">Character level</param>
        /// <returns>Maximum mana points</returns>
        public int CalculateMaxMana(int level)
        {
            // Base mana: BaseMana + (8 per intellect) + (3 per level)
            int baseMaxMana = BaseMana + (Intellect * 8) + (level * 3);
            
            // Apply boost modifiers
            float intellectBoost = GetStatBoostModifier("intellect");
            float manaBoost = GetStatBoostModifier("max_mana");
            
            return Mathf.RoundToInt(baseMaxMana * (1 + intellectBoost) * (1 + manaBoost));
        }
        
        #endregion
        
        #region Stat Boost Management
        
        /// <summary>
        /// Update temporary stat boosts and remove expired ones
        /// </summary>
        private void UpdateTemporaryBoosts()
        {
            bool hasChanged = false;
            
            // Remove expired boosts
            for (int i = activeBoosts.Count - 1; i >= 0; i--)
            {
                if (activeBoosts[i].IsExpired)
                {
                    Debug.Log($"Stat boost to {activeBoosts[i].BoostType} from {activeBoosts[i].Source} has expired");
                    activeBoosts.RemoveAt(i);
                    hasChanged = true;
                }
            }
            
            // If stats have changed, trigger the event
            if (hasChanged && OnStatsChanged != null)
            {
                OnStatsChanged.Invoke();
            }
        }
        
        /// <summary>
        /// Add a temporary or permanent stat boost
        /// </summary>
        /// <param name="boostType">Type of stat to boost (e.g. "strength", "dodge")</param>
        /// <param name="value">Amount of boost</param>
        /// <param name="isPercentage">Whether boost is a percentage or flat value</param>
        /// <param name="duration">Duration in seconds (0 or negative for permanent)</param>
        /// <param name="source">Source of the boost (ability name, item, etc.)</param>
        public void AddStatBoost(string boostType, int value, bool isPercentage = false, float duration = 10f, string source = "Unknown")
        {
            // Create new boost
            StatBoost newBoost = new StatBoost
            {
                BoostType = boostType.ToLower(),
                Value = value,
                IsPercentage = isPercentage,
                Duration = duration,
                StartTime = Time.time,
                Source = source
            };
            
            // Add to active boosts
            activeBoosts.Add(newBoost);
            
            // Log the boost
            Debug.Log($"Added {value}{(isPercentage ? "%" : "")} {boostType} boost from {source} " +
                     $"for {(duration <= 0 ? "permanent" : duration + "s")}");
            
            // Trigger stats changed event
            if (OnStatsChanged != null)
            {
                OnStatsChanged.Invoke();
            }
        }
        
        /// <summary>
        /// Remove all stat boosts from a specific source
        /// </summary>
        /// <param name="source">Source to remove boosts from</param>
        public void RemoveStatBoostsFromSource(string source)
        {
            bool hasChanged = false;
            
            // Remove boosts from the specified source
            for (int i = activeBoosts.Count - 1; i >= 0; i--)
            {
                if (activeBoosts[i].Source == source)
                {
                    activeBoosts.RemoveAt(i);
                    hasChanged = true;
                }
            }
            
            if (hasChanged)
            {
                Debug.Log($"Removed all stat boosts from {source}");
                
                // Trigger stats changed event
                if (OnStatsChanged != null)
                {
                    OnStatsChanged.Invoke();
                }
            }
        }
        
        /// <summary>
        /// Remove all stat boosts of a specific type
        /// </summary>
        /// <param name="boostType">Type of boost to remove</param>
        public void RemoveStatBoosts(string boostType)
        {
            string lowerBoostType = boostType.ToLower();
            bool hasChanged = false;
            
            // Remove boosts of the specified type
            for (int i = activeBoosts.Count - 1; i >= 0; i--)
            {
                if (activeBoosts[i].BoostType == lowerBoostType)
                {
                    activeBoosts.RemoveAt(i);
                    hasChanged = true;
                }
            }
            
            if (hasChanged)
            {
                Debug.Log($"Removed all {boostType} stat boosts");
                
                // Trigger stats changed event
                if (OnStatsChanged != null)
                {
                    OnStatsChanged.Invoke();
                }
            }
        }
        
        /// <summary>
        /// Get the total modifier for a specific stat type from all active boosts
        /// </summary>
        /// <param name="statType">Type of stat to get modifiers for</param>
        /// <returns>Total modifier value (for percentage calculations)</returns>
        private float GetStatBoostModifier(string statType)
        {
            if (activeBoosts == null || activeBoosts.Count == 0)
                return 0f;
                
            string lowerStatType = statType.ToLower();
            float totalFlatBonus = 0f;
            float totalPercentBonus = 0f;
            
            // Sum up all active boosts for this stat type
            foreach (StatBoost boost in activeBoosts)
            {
                if (boost.BoostType == lowerStatType)
                {
                    if (boost.IsPercentage)
                    {
                        totalPercentBonus += boost.Value / 100f;
                    }
                    else
                    {
                        totalFlatBonus += boost.Value;
                    }
                }
            }
            
            // For percentage calculations, convert flat bonuses to percentage
            // based on a reference value appropriate for the stat
            int referenceValue = GetReferenceValueForStat(lowerStatType);
            float flatAsPercent = (referenceValue > 0) ? totalFlatBonus / referenceValue : 0;
            
            return totalPercentBonus + flatAsPercent;
        }
        
        /// <summary>
        /// Get a reference value for converting flat bonuses to percentages
        /// </summary>
        /// <param name="statType">Type of stat</param>
        /// <returns>Reference value</returns>
        private int GetReferenceValueForStat(string statType)
        {
            // Return appropriate reference values based on stat type
            switch (statType)
            {
                case "strength":
                case "vitality":
                case "agility":
                case "intellect":
                case "wisdom":
                case "charisma":
                case "luck":
                    return 50; // Base reference for primary stats
                    
                case "health":
                case "max_health":
                    return 500; // Base reference for health
                    
                case "mana":
                case "max_mana":
                    return 300; // Base reference for mana
                    
                case "defense":
                case "damage":
                    return 100; // Base reference for defense/damage
                    
                default:
                    return 50; // Default reference value
            }
        }
        
        /// <summary>
        /// Clear all stat boosts
        /// </summary>
        public void ClearAllStatBoosts()
        {
            if (activeBoosts.Count > 0)
            {
                activeBoosts.Clear();
                Debug.Log("Cleared all stat boosts");
                
                // Trigger stats changed event
                if (OnStatsChanged != null)
                {
                    OnStatsChanged.Invoke();
                }
            }
        }
        
        /// <summary>
        /// Apply item stat bonuses from equipped items
        /// </summary>
        /// <param name="item">Item being equipped</param>
        public void ApplyItemStatBonuses(Item item)
        {
            if (item == null || !item.IsEquippable || !item.Equipped)
                return;
                
            // Apply each stat bonus from the item
            foreach (var statBonus in item.StatBonuses)
            {
                AddStatBoost(
                    statBonus.StatType, 
                    statBonus.Value, 
                    statBonus.IsPercentage, 
                    -1, // Permanent until unequipped
                    $"Item:{item.ItemName}"
                );
            }
        }
        
        /// <summary>
        /// Remove item stat bonuses when an item is unequipped
        /// </summary>
        /// <param name="item">Item being unequipped</param>
        public void RemoveItemStatBonuses(Item item)
        {
            if (item == null)
                return;
                
            RemoveStatBoostsFromSource($"Item:{item.ItemName}");
        }
        
        #endregion
        
        #region Persistence
        
        /// <summary>
        /// Save character stats to player prefs (temporary solution until database is implemented)
        /// </summary>
        /// <param name="characterId">Character ID for save identification</param>
        public void SaveToPlayerPrefs(int characterId)
        {
            string saveKey = $"CharStats_{characterId}";
            
            // Create save data object
            var saveData = new Dictionary<string, object>
            {
                {"Vitality", Vitality},
                {"Strength", Strength},
                {"Agility", Agility},
                {"Intellect", Intellect},
                {"Wisdom", Wisdom},
                {"Charisma", Charisma},
                {"Luck", Luck},
                {"BaseHealth", BaseHealth},
                {"BaseMana", BaseMana},
                {"MaxEnergy", MaxEnergy},
                {"CurrentEnergy", CurrentEnergy}
            };
            
            // Save to player prefs
            string jsonData = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString(saveKey, jsonData);
            PlayerPrefs.Save();
            
            Debug.Log($"Saved character stats for character {characterId}");
        }
        
        /// <summary>
        /// Load character stats from player prefs
        /// </summary>
        /// <param name="characterId">Character ID for save identification</param>
        /// <returns>True if load was successful</returns>
        public bool LoadFromPlayerPrefs(int characterId)
        {
            string saveKey = $"CharStats_{characterId}";
            
            if (PlayerPrefs.HasKey(saveKey))
            {
                string jsonData = PlayerPrefs.GetString(saveKey);
                Dictionary<string, object> saveData = JsonUtility.FromJson<Dictionary<string, object>>(jsonData);
                
                // Load values
                if (saveData != null)
                {
                    Vitality = Convert.ToInt32(saveData["Vitality"]);
                    Strength = Convert.ToInt32(saveData["Strength"]);
                    Agility = Convert.ToInt32(saveData["Agility"]);
                    Intellect = Convert.ToInt32(saveData["Intellect"]);
                    Wisdom = Convert.ToInt32(saveData["Wisdom"]);
                    Charisma = Convert.ToInt32(saveData["Charisma"]);
                    Luck = Convert.ToInt32(saveData["Luck"]);
                    BaseHealth = Convert.ToInt32(saveData["BaseHealth"]);
                    BaseMana = Convert.ToInt32(saveData["BaseMana"]);
                    MaxEnergy = Convert.ToInt32(saveData["MaxEnergy"]);
                    CurrentEnergy = Convert.ToInt32(saveData["CurrentEnergy"]);
                    
                    Debug.Log($"Loaded character stats for character {characterId}");
                    return true;
                }
            }
            
            Debug.Log($"No saved stats found for character {characterId}");
            return false;
        }
        
        /// <summary>
        /// Save character stats to database
        /// </summary>
        public void SaveToDatabase()
        {
            // Placeholder for database functionality
            // Will be implemented when database connection is set up
            Debug.Log("SaveToDatabase: This will be implemented with SQLite integration");
            
            // Example implementation:
            // DatabaseManager.Instance.SaveCharacterStats(Id, CharacterId, Vitality, Strength, 
            //     Agility, Intellect, Wisdom, Charisma, Luck, BaseHealth, BaseMana);
        }
        
        /// <summary>
        /// Load character stats from database
        /// </summary>
        /// <param name="characterId">Character ID to load stats for</param>
        /// <returns>True if load was successful</returns>
        public bool LoadFromDatabase(int characterId)
        {
            // Placeholder for database functionality
            // Will be implemented when database connection is set up
            Debug.Log("LoadFromDatabase: This will
        [Tooltip("Maximum energy for special abilities")]
        public int MaxEnergy = 100;

        [Tooltip("Current energy level")]
        public int CurrentEnergy = 100;

        // Derived stats (calculated from base stats)
        [Header("Derived Stats")]
        [Tooltip("Chance to dodge attacks")]
        public float DodgeChance => CalculateDodgeChance();

        [Tooltip("Chance to land critical hits")]
        public float CriticalChance => CalculateCriticalChance();

        [Tooltip("Damage multiplier for critical hits")]
        public float CriticalMultiplier => CalculateCriticalMultiplier();

        [Tooltip("Damage reduction from physical attacks")]
        public float DamageReduction => CalculateDamageReduction();

        [Tooltip("Magic resistance percentage")]
        public float MagicResistance => CalculateMagicResistance();

        [Tooltip("Rate of mana regeneration per second")]
        public float ManaRegenRate => CalculateManaRegeneration();

        [Tooltip("Maximum item weight/count the character can carry")]
        public int CarryingCapacity => CalculateCarryingCapacity();

        [Tooltip("Attack speed modifier (1.0 is base)")]
        public float AttackSpeed => CalculateAttackSpeed();

        [Tooltip("Movement speed modifier (1.0 is base)")]
        public float MovementSpeed => CalculateMovementSpeed();

        [Tooltip("Discount percentage when buying from merchants")]
        public float MerchantDiscount => CalculateMerchantDiscount();

        // Reference to the parent character
        [HideInInspector]
        public Character ParentCharacter;

        // Stat Boost Definition
        [Serializable]
        public class StatBoost
        {
            /// <summary>
            /// Which stat is being boosted (lowercase)
            /// </summary>
            public string BoostType;
            
            /// <summary>
            /// Numerical value of the boost
            /// </summary>
            public int Value;
            
            /// <summary>
            /// Whether this is a flat value or percentage
            /// </summary>
            public bool IsPercentage;
            
            /// <summary>
            /// Duration in seconds (0 or negative for permanent)
            /// </summary>
            public float Duration;
            
            /// <summary>
            /// When the boost was applied
            /// </summary>
            public float StartTime;
            
            /// <summary>
            /// Source of the boost (ability, item, etc.)
            /// </summary>
            public string Source;

            /// <summary>
            /// Checks if this boost has expired
            /// </summary>
            public bool IsExpired => Duration > 0 && (Time.time > StartTime + Duration);
            
            /// <summary>
            /// Remaining time in seconds
            /// </summary>
            public float RemainingTime => Duration <= 0 ? float.PositiveInfinity : 
                Mathf.Max(0, (StartTime + Duration) - Time.time);
        }

        // Active stat boosts
        [SerializeField, HideInInspector]
        private List<StatBoost> activeBoosts = new List<StatBoost>();
        
        /// <summary>
        /// Read-only access to active stat boosts
        /// </summary>
        public IReadOnlyList<StatBoost> ActiveBoosts => activeBoosts;
        
        /// <summary>
        /// Event triggered when stats change
        /// </summary>
        public event Action OnStatsChanged;

        #region Unity Lifecycle
        
        private void Awake()
        {
            // Find parent character if not set
            if (ParentCharacter == null)
            {
                ParentCharacter = GetComponent<Character>();
            }
        }

        private void Update()
        {
            // Update temporary stat boosts, expire if needed
            UpdateTemporaryBoosts();
        }

        #endregion

        #region Stat Calculations

        /// <summary>
        /// Calculate a character's dodge chance based on agility
        /// </summary>
        /// <returns>Dodge chance as percentage (0-1)</returns>
        public float CalculateDodgeChance()
        {
            // Base dodge formula: 5% + 0.25% per point of agility
            float baseChance = 0.05f + (Agility * 0.0025f);
            
            // Apply boost modifiers
            float agilityBoost = GetStatBoostModifier("agility");
            float dodgeBoost = GetStatBoostModifier("dodge");
            
            // Cap at 75% to prevent becoming unhittable
            return Mathf.Min(0.75f, baseChance * (1 + agilityBoost) + dodgeBoost);
        }
        
        /// <summary>
        /// Calculate a character's critical hit chance based on luck
        /// </summary>
        /// <returns>Critical hit chance as percentage (0-1)</returns>
        public float CalculateCriticalChance()
        {
            // Base crit formula: 3% + 0.2% per point of luck
            float baseChance = 0.03f + (Luck * 0.002f);
            
            // Apply boost modifiers
            float luckBoost = GetStatBoostModifier("luck");
            float critBoost = GetStatBoostModifier("critical_chance");
            
            // Cap at 80% to prevent guaranteed crits
            return Mathf.Min(0.8f, baseChance * (1 + luckBoost) + critBoost);
        }
        
        /// <summary>
        /// Calculate critical hit damage multiplier
        /// </summary>
        /// <returns>Damage multiplier for critical hits</returns>
        public float CalculateCriticalMultiplier()
        {
            // Base multiplier: 1.5x + 0.01x per 2 points of strength
            float baseMultiplier = 1.5f + (Strength * 0.005f);
            
            // Apply boost modifiers
            float critMultBoost = GetStatBoostModifier("critical_multiplier");
            
            return baseMultiplier * (1 + critMultBoost);
        }
        
        /// <summary>
        /// Calculate carrying capacity for inventory based on strength
        /// </summary>
        /// <returns>Number of items/weight the character can carry</returns>
        public int CalculateCarryingCapacity()
        {
            // Base capacity: 10 + 2 per point of strength
            int baseCapacity = 10 + (Strength * 2);
            
            // Apply boost modifiers
            float strengthBoost = GetStatBoostModifier("strength");
            float capacityBoost = GetStatBoostModifier("carrying_capacity");
            
            return Mathf.RoundToInt(baseCapacity * (1 + strengthBoost) + (baseCapacity * capacityBoost));
        }
        
        /// <summary>
        /// Calculate damage reduction from vitality
        /// </summary>
        /// <returns>Damage reduction as percentage (0-1)</returns>
        public float CalculateDamageReduction()
        {
            // Base reduction: 5% + 0.15% per point of vitality
            float baseReduction = 0.05f + (Vitality * 0.0015f);
            
            // Apply boost modifiers
            float vitalityBoost = GetStatBoostModifier("vitality");
            float defenseBoost = GetStatBoostModifier("defense");
            
            // Cap at 75% to prevent becoming invulnerable
            return Mathf.Min(0.75f, baseReduction * (1 + vitalityBoost) + defenseBoost);
        }
        
        /// <summary>
        /// Calculate magic resistance from wisdom
        /// </summary>
        /// <returns>Magic resistance as percentage (0-1)</returns>
        public float CalculateMagicResistance()
        {
            // Base reduction: 2% + 0.2% per point of wisdom
            float baseResistance = 0.02f + (Wisdom * 0.002f);
            
            // Apply boost modifiers
            float wisdomBoost = GetStatBoostModifier("wisdom");
            float resistanceBoost = GetStatBoostModifier("magic_resistance");
            
            // Cap at 75% to prevent becoming immune
            return Mathf.Min(0.75f, baseResistance * (1 + wisdomBoost) + resistanceBoost);
        }
        
        /// <summary>
        /// Calculate mana regeneration rate
        /// </summary>
        /// <returns>Mana points regenerated per second</returns>
        public float CalculateManaRegeneration()
        {
            // Base regen: 0.5 + 0.1 per point of wisdom
            float baseRegen = 0.5f + (Wisdom * 0.1f);
            
            // Apply boost modifiers
            float wisdomBoost = GetStatBoostModifier("wisdom");
            float manaRegenBoost = GetStatBoostModifier("mana_regeneration");
            
            return baseRegen * (1 + wisdomBoost) + manaRegenBoost;
        }
        
        /// <summary>
        /// Calculate attack speed multiplier
        /// </summary>
        /// <returns>Attack speed multiplier (1.0 is base speed)</returns>
        public float CalculateAttackSpeed()
        {
            // Base speed: 1.0 + 0.005 per point of agility
            float baseSpeed = 1.0f + (Agility * 0.005f);
            
            // Apply boost modifiers
            float agilityBoost = GetStatBoostModifier("agility");
            float attackSpeedBoost = GetStatBoostModifier("attack_speed");
            
            return baseSpeed * (1 + agilityBoost) * (1 + attackSpeedBoost);
        }
        
        /// <summary>
        /// Calculate movement speed multiplier
        /// </summary>
        /// <returns>Movement speed multiplier (1.0 is base speed)</returns>
        public float CalculateMovementSpeed()
        {
            // Base speed: 1.0 + 0.003 per point of agility
            float baseSpeed = 1.0f + (Agility * 0.003f);
            
            // Apply boost modifiers
            float agilityBoost = GetStatBoostModifier("agility");
            float moveSpeedBoost = GetStatBoostModifier("movement_speed");
            
            return baseSpeed * (1 + agilityBoost * 0.5f) * (1 + moveSpeedBoost);
        }
        
        /// <summary>
        /// Calculate merchant discount percentage based on charisma
        /// </summary>
        /// <returns>Discount percentage (0-0.5)</returns>
        public float CalculateMerchantDiscount()
        {
            // Base discount: 0% + 0.3% per point of charisma
            float baseDiscount = Charisma * 0.003f;

