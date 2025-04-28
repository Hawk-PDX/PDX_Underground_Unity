using System;
using System.Collections.Generic;

namespace PDXUnderground.Models
{
    [Serializable]
    public class CharacterStats
    {
        // Base stats
        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Vitality { get; set; }
        public int Intellect { get; set; }
        public int Wisdom { get; set; }
        public int Charisma { get; set; }
        public int Luck { get; set; }
        public int Agility { get; set; }

        // Derived stats
        public int MaxHealth { get; set; }
        public int MaxMana { get; set; }
        public int MaxEnergy { get; set; }
        public int CurrentHealth { get; set; }
        public int CurrentMana { get; set; }
        public int CurrentEnergy { get; set; }

        // Combat stats
        public float PhysicalDamage { get; set; }
        public float MagicalDamage { get; set; }
        public float DamageReduction { get; set; }
        public float MagicResistance { get; set; }

        // Immunity flags
        public bool IsPoisonImmune { get; set; }
        public bool IsStunImmune { get; set; }
        public bool IsFreezeImmune { get; set; }

        // Temporary stat boosts
        private Dictionary<string, List<StatBoost>> activeBoosts = new Dictionary<string, List<StatBoost>>();

        [Serializable]
        public class StatBoost
        {
            public string StatType { get; set; }
            public float Value { get; set; }
            public bool IsPercentage { get; set; }
            public float Duration { get; set; }
            public float StartTime { get; set; }
            public string Source { get; set; }

            public bool IsExpired(float currentTime)
            {
                return Duration > 0 && (currentTime > StartTime + Duration);
            }
        }

        // Stat boost management
        public void AddStatBoost(string statType, float value, bool isPercentage, float duration, string source)
        {
            if (!activeBoosts.ContainsKey(statType))
            {
                activeBoosts[statType] = new List<StatBoost>();
            }

            var boost = new StatBoost
            {
                StatType = statType,
                Value = value,
                IsPercentage = isPercentage,
                Duration = duration,
                StartTime = 0, // Will be set by game system
                Source = source
            };

            activeBoosts[statType].Add(boost);
        }

        public void RemoveExpiredStatBoosts(float currentTime)
        {
            foreach (var boosts in activeBoosts.Values)
            {
                boosts.RemoveAll(b => b.IsExpired(currentTime));
            }
        }

        public void RemoveStatBoostsFromSource(string source)
        {
            foreach (var boosts in activeBoosts.Values)
            {
                boosts.RemoveAll(b => b.Source == source);
            }
        }

        public void RemoveStatBoost(string source, string statType)
        {
            if (activeBoosts.ContainsKey(statType))
            {
                activeBoosts[statType].RemoveAll(b => b.Source == source);
            }
        }

        public void ClearAllStatBoosts()
        {
            activeBoosts.Clear();
        }

        // Stat calculations
        public float CalculateDodgeChance()
        {
            return Math.Min(0.75f, 0.05f + (Agility * 0.003f));
        }

        public float CalculateCriticalChance()
        {
            return Math.Min(0.8f, 0.03f + (Luck * 0.002f));
        }

        public float CalculateCriticalMultiplier()
        {
            return 1.5f + (Strength * 0.005f);
        }

        public int CalculateCarryingCapacity()
        {
            return 10 + (Strength * 2);
        }

        public float CalculateDamageReduction()
        {
            return Math.Min(0.75f, 0.05f + (Vitality * 0.0015f));
        }

        public float CalculateMagicResistance()
        {
            return Math.Min(0.75f, 0.02f + (Wisdom * 0.002f));
        }

        public float CalculateManaRegeneration()
        {
            return 0.5f + (Wisdom * 0.1f);
        }

        public float CalculateMovementSpeed()
        {
            return 1.0f + (Agility * 0.003f);
        }

        public float CalculateMerchantDiscount()
        {
            return Math.Min(0.5f, Charisma * 0.003f);
        }
    }
}
