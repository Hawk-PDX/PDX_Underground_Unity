using System;
using System.Collections.Generic;
using UnityEngine;
using PDXUnderground.Models.Interfaces;

namespace PDXUnderground.Core.Interfaces
{
    /// <summary>
    /// Interface for card-based gameplay systems
    /// </summary>
    public interface ICardSystem
    {
        // Properties
        ICard[] CurrentHand { get; }
        ICard[] Deck { get; }
        int MaxHandSize { get; }
        
        // Events
        event Action<ICard> OnCardUsed;
        event Action<ICard> OnCardDiscarded;
        event Action<ICard> OnCardDrawn;
        event Action<List<ICard>> OnHandChanged;
        event Action<string, float> OnAbilityUsed;
        event Action<Vector3> OnCriticalHit;
        
        // Card operations
        void UseCard(ICard card);
        bool UseCard(int cardIndex);
        bool UseCard(int cardIndex, Vector3 direction);
        void DiscardCard(ICard card);
        void DrawCard();
        void ShuffleDeck();
        
        // Card queries
        bool CanUseCard(ICard card);
        float GetCardCooldown(ICard card);
        float GetAbilityCooldown(string abilityName);
        bool IsCardOnCooldown(ICard card);
        List<ICard> GetCurrentHand();
    }
}
