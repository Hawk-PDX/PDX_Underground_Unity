using UnityEngine;

namespace PDXUnderground.Combat
{
    /// <summary>
    /// Interface for objects that can take damage.
    /// Implement this interface on any GameObject that should respond to damage.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Apply damage to this object
        /// </summary>
        /// <param name="damage">Amount of damage to apply</param>
        /// <returns>Actual damage dealt (after modifiers)</returns>
        float TakeDamage(float damage);
        
        /// <summary>
        /// Get the current health of this object
        /// </summary>
        float GetCurrentHealth();
        
        /// <summary>
        /// Get the maximum health of this object
        /// </summary>
        float GetMaxHealth();
        
        /// <summary>
        /// Check if this object is dead/destroyed
        /// </summary>
        bool IsDead();
    }
}
