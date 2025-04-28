using UnityEngine;
using PDXUnderground.Models.Interfaces;
using PDXUnderground.Core;

namespace PDXUnderground.Models
{
    public class Card : ICard
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public CardType Type { get; set; }
        public int Value { get; set; }
        public float EnergyCost { get; set; }  // Changed from int to float
        public float Cooldown { get; set; }    // Changed from int to float
        public float Damage { get; set; }      // Changed from int to float
        public float LastUseTime { get; set; }
        public int Suit { get; set; }
        public int Rank { get; set; }
        public SpecialEffect SpecialEffect { get; set; }  // Renamed from Effect to match interface
    }
}
