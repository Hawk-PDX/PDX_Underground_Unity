using UnityEngine;
using System;
using PDXUnderground.Models;
using PDXUnderground.Core;

namespace PDXUnderground.Core.Interfaces
{
    public interface IGamblerCharacter : IBuzzSystem, ICardSystem
    {
        // Events that override base interfaces
        new event Action<string, float> OnAbilityUsed;
        new event Action<ICard> OnCardUsed;
        new event Action<BuzzState> OnBuzzStateChanged;
        
        // Unique IGamblerCharacter events
        new event Action<Vector3> OnCriticalHit;
        
        // Health system methods
        float GetMaxHealth();
        bool IsDead();
        float Heal(float amount);
        
        // Effects system methods
        void ApplyStun(float duration);
        
        // Buzz-related methods
        void UpdateBuzz(float amount);
        float GetCurrentBuzzPercentage();
        float MaxBuzz { get; }
        
        // Unity Transform access
        Transform transform { get; }
    }
}
