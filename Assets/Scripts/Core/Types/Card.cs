using UnityEngine;
using PDXUnderground.Core.Interfaces;
using PDXUnderground.Models.Interfaces;
using PDXUnderground.Core;  // Explicit reference for clarity

namespace PDXUnderground.Core
{
    /// <summary>
    /// Represents a game card in the PDX Underground card system
    /// </summary>
    public class Card : ICard
    {
        /// <summary>
        /// Unique identifier for the card
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the card
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the card's effect
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Type of card (Attack, Defense, Utility, Special)
        /// </summary>
        public CardType Type { get; set; }

        /// <summary>
        /// Numerical value of the card
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Energy cost to use this card
        /// </summary>
        public float EnergyCost { get; set; }

        /// <summary>
        /// Cooldown time in seconds before the card can be used again
        /// </summary>
        public float Cooldown { get; set; }

        /// <summary>
        /// Damage value (if applicable)
        /// </summary>
        public float Damage { get; set; }

        /// <summary>
        /// Time when the card was last used
        /// </summary>
        public float LastUseTime { get; set; }

        /// <summary>
        /// Card suit (0=spades, 1=hearts, 2=diamonds, 3=clubs)
        /// </summary>
        public int Suit { get; set; }

        /// <summary>
        /// Card rank (1-13 where Ace=1, Jack=11, Queen=12, King=13)
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// Special effect applied by the card
        /// </summary>
        public SpecialEffect SpecialEffect { get; set; }
    }
}
