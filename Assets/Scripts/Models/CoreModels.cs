using UnityEngine;

namespace PDXUnderground.Models
{
    // Game State Models
    public enum GameState
    {
        MainMenu,
        Loading,
        Playing,
        Paused,
        GameOver
    }

    public enum TimeOfDay
    {
        Dawn,
        Day,        // Add Day for direct reference
        Morning,
        Noon,
        Afternoon,
        Dusk,
        Night,
        Midnight
    }

    public enum EnvironmentType
    {
        Streets,
        Tunnels,
        Speakeasy
    }

    // Card System Models
    public class CardModel
    {
        public string name;
        public CardType type;
        public int energyCost;
        public int cooldown;
        public int damage;
    }

    // Buzz System Models
    public enum BuzzState
    {
        Normal,
        Low,
        Critical
    }

    public class BuzzModel
    {
        public float currentLevel;
        public float maxLevel;
        public BuzzState state;
    }

    // Player Model
    public class PlayerModel
    {
        public float currentBuzzLevel;
        public float maxBuzzLevel;
        public BuzzState buzzState;
        public CardType[] currentHand;
    }

    // Environment Model
    public class EnvironmentModel
    {
        public TimeOfDay timeOfDay;
        public float weatherIntensity;
        public bool isNight;
        public Color ambientColor;
    }
}
