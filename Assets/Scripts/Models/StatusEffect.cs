using System;
using UnityEngine;

namespace PDXUnderground.Models
{
    /// <summary>
    /// Represents a status effect that can be applied to characters.
    /// Status effects can modify stats, deal damage over time, or apply other gameplay effects.
    /// </summary>
    [Serializable]
    public class StatusEffect
    {
        /// <summary>
        /// Name of the status effect (e.g., "poison", "strength", "slow")
        /// </summary>
        public string EffectName { get; set; }
        
        /// <summary>
        /// Type/category of the effect (e.g., "buff", "debuff", "dot")
        /// </summary>
        public string EffectType { get; set; }
        
        /// <summary>
        /// Human-readable description of what the effect does
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Potency or strength of the effect
        /// </summary>
        public int Strength { get; set; }
        
        /// <summary>
        /// Duration of the effect in seconds (negative or zero for permanent effects)
        /// </summary>
        public float Duration { get; set; }
        
        /// <summary>
        /// Time when the effect was applied (using game time)
        /// </summary>
        public float StartTime { get; set; }
        
        /// <summary>
        /// Source of the effect (item, spell, enemy, etc.)
        /// </summary>
        public string Source { get; set; }
        
        /// <summary>
        /// Whether this is a beneficial effect
        /// </summary>
        public bool IsPositive { get; set; }
        
        /// <summary>
        /// Visual representation of the effect
        /// </summary>
        public Sprite Icon { get; set; }
        
        /// <summary>
        /// For stat modification effects, the stat being modified
        /// </summary>
        public string StatToModify { get; set; }
        
        /// <summary>
        /// For stat modification effects, the amount of modification
        /// </summary>
        public int ModifierValue { get; set; }
        
        /// <summary>
        /// Whether the modifier is a percentage or flat value
        /// </summary>
        public bool IsPercentage { get; set; }

        /// <summary>
        /// Remaining duration of the effect in seconds
        /// </summary>
        public float RemainingTime => Duration <= 0 ? float.PositiveInfinity : 
            Mathf.Max(0, (StartTime + Duration) - Time.time);

        /// <summary>
        /// Checks if the status effect has expired based on a provided time
        /// </summary>
        /// <param name="currentTime">Current game time</param>
        /// <returns>True if the effect has expired</returns>
        public bool IsExpired(float currentTime)
        {
            return Duration > 0 && (currentTime > StartTime + Duration);
        }
        
        /// <summary>
        /// Gets the remaining time of the effect
        /// </summary>
        /// <param name="currentTime">Current game time</param>
        /// <returns>Remaining duration in seconds, or infinity for permanent effects</returns>
        public float GetRemainingTime(float currentTime)
        {
            return Duration <= 0 ? float.PositiveInfinity : 
                Math.Max(0, (StartTime + Duration) - currentTime);
        }
        
        /// <summary>
        /// Creates a copy of this status effect
        /// </summary>
        /// <returns>A new StatusEffect instance with the same properties</returns>
        public StatusEffect Clone()
        {
            return (StatusEffect)MemberwiseClone();
        }
    }
}
