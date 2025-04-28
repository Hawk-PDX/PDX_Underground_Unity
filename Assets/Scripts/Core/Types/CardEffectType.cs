using UnityEngine;

namespace PDXUnderground.Core
{
    /// <summary>
    /// Defines the type of effect a card produces when used
    /// </summary>
    public enum CardEffectType
    {
        /// <summary>
        /// No effect
        /// </summary>
        None = 0,

        /// <summary>
        /// Deals damage or offensive effects
        /// </summary>
        Attack,

        /// <summary>
        /// Provides defensive benefits
        /// </summary>
        Defense,

        /// <summary>
        /// Heals or restores resources
        /// </summary>
        Heal,

        /// <summary>
        /// Provides utility effects
        /// </summary>
        Utility,

        /// <summary>
        /// Special or unique effects
        /// </summary>
        Special,

        /// <summary>
        /// Blocks or prevents damage
        /// </summary>
        Block
    }
}

