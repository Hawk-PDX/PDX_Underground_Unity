using System;
using UnityEngine;

namespace PDXUnderground.Core.Interfaces
{
    /// <summary>
    /// Interface for managing character buzz/intoxication mechanics.
    /// Provides abstraction of buzz-related functionality to break circular dependencies.
    /// </summary>
    public interface IBuzzSystem
    {
        // Properties
        /// <summary>
        /// Current buzz level
        /// </summary>
        float CurrentBuzzLevel { get; }
        
        /// <summary>
        /// Maximum buzz level
        /// </summary>
        float MaxBuzzLevel { get; }
        
        /// <summary>
        /// Current buzz state
        /// </summary>
        BuzzState CurrentBuzzState { get; }

        // Events
        /// <summary>
        /// Event triggered when buzz level changes
        /// </summary>
        event Action<float, float> OnBuzzChanged;
        
        /// <summary>
        /// Event triggered when buzz state changes
        /// </summary>
        event Action<BuzzState, BuzzState> OnBuzzStateChanged;
        
        /// <summary>
        /// Event triggered when entering critical buzz state
        /// </summary>
        event Action OnEnterCriticalBuzzState;
        
        // Methods
        /// <summary>
        /// Increases buzz level by the specified amount
        /// </summary>
        /// <param name="amount">Amount to increase (typically 0-100)</param>
        void IncreaseBuzz(float amount);
        
        /// <summary>
        /// Decreases buzz level by the specified amount
        /// </summary>
        /// <param name="amount">Amount to decrease (typically 0-100)</param>
        void DecreaseBuzz(float amount);
        
        /// <summary>
        /// Adjust the character's buzz level by the specified amount
        /// </summary>
        /// <param name="amount">Amount to adjust (positive or negative)</param>
        void AdjustBuzz(float amount);
        
        /// <summary>
        /// Sets buzz level to a specific value
        /// </summary>
        /// <param name="value">New buzz level value (typically 0-100)</param>
        void SetBuzzLevel(float value);
        
        /// <summary>
        /// Set the buzz state directly
        /// </summary>
        /// <param name="state">New buzz state</param>
        void SetBuzzState(BuzzState state);
        
        /// <summary>
        /// Applies the current buzz effects to the character
        /// </summary>
        void ApplyBuzzEffects();
        
        /// <summary>
        /// Gets the current visual distortion amount based on buzz level
        /// </summary>
        /// <returns>Distortion amount (0-1)</returns>
        float GetVisualDistortionAmount();
        
        /// <summary>
        /// Gets the current movement impairment amount based on buzz level
        /// </summary>
        /// <returns>Movement impairment amount (0-1)</returns>
        float GetMovementImpairmentAmount();
    }
}
