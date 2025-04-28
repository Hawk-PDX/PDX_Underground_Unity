using PDXUnderground.Core;

namespace PDXUnderground.Models.Interfaces
{
    public interface ICard
    {
        int Id { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        CardType Type { get; set; }
        int Value { get; set; }
        float EnergyCost { get; set; }
        float Cooldown { get; set; }
        float Damage { get; set; }
        float LastUseTime { get; set; }
        int Suit { get; set; }
        int Rank { get; set; }
        SpecialEffect SpecialEffect { get; set; }
    }
}

