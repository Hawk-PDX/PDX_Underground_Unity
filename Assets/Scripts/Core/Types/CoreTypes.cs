using System;
using UnityEngine;

namespace PDXUnderground.Core
{
    /// <summary>
    /// Interface for all damageable entities in the game
    /// </summary>
    public interface IDamageable
    {
        float TakeDamage(float damage);
        float Heal(float amount);
        float GetCurrentHealth();
        float GetMaxHealth();
        bool IsDead();

        event Action<float, float> OnHealthChanged;
        event Action OnDeath;
    }

    /// <summary>
    /// Defines the type of card
    /// </summary>
    public enum CardType
    {
        None = 0,
        Attack,
        Defense,
        Special,
        Utility,
        Trap,
        Heal,
        Buff,
        Debuff
    }


    /// <summary>
    /// Defines the type of environment
    /// </summary>
    public enum EnvironmentType
    {
        Streets = 0,
        Interior,
        Underground,
        Rooftops,
        Special
    }

    /// <summary>
    /// Defines special effects that can be applied
    /// </summary>
    public enum SpecialEffect
    {
        None = 0,
        Burn,
        Freeze,
        Poison,
        Stun,
        Bleed,
        Weaken,
        Strengthen,
        Shield,
        Regenerate,
        Heal,
        EnergyBoost,
        Chaos
    }

    /// <summary>
    /// Defines the time of day
    /// </summary>
    public enum TimeOfDay
    {
        Midnight = 0,
        Dawn,
        Morning,
        Day,
        Noon,
        Afternoon,
        Dusk,
        Night
    }
}

