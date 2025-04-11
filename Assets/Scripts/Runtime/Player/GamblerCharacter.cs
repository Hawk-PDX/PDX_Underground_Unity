using UnityEngine;
using System.Collections.Generic;
using PDXUnderground.Core;

namespace PDXUnderground.Player
{
    /// <summary>
    /// Main character class for the PDX Underground game.
    /// Handles character stats, buzz level, card system, and character state.
    /// </summary>
    public class GamblerCharacter : MonoBehaviour
    {
        #region Character Stats
        [Header("Character Stats")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;
        [SerializeField] private float maxEnergy = 100f;
        [SerializeField] private float currentEnergy;
        [SerializeField] private int accuracy = 10;
        [SerializeField] private int defense = 10;
        
        [Header("Buzz System")]
        [Tooltip("Maximum buzz level")]
        public float maxBuzz = 100f;
        
        [Tooltip("Current buzz level")]
        [SerializeField] private float _currentBuzz = 100f;
        
        [Tooltip("Buzz level where critical effects begin")]
        public float criticalBuzzThreshold = 30f;
        
        [Tooltip("Speed of buzz level decay over time")]
        [SerializeField] private float buzzDecayRate = 5f;
        
        [Tooltip("Natural buzz gain rate over time")]
        [SerializeField] private float naturalBuzzGainRate = 2f;
        
        [Tooltip("Buzz level where penalty effects begin")]
        [SerializeField] private float lowBuzzThreshold = 20f;
        
        [Tooltip("Accuracy penalty percentage when buzz is low")]
        [SerializeField] private float accuracyPenalty = 0.25f;
        
        [Tooltip("Defense penalty percentage when buzz is low")]
        [SerializeField] private float defensePenalty = 0.25f;
        
        [Tooltip("Movement speed multiplier based on buzz level")]
        [SerializeField] private AnimationCurve movementSpeedMultiplier = new AnimationCurve(
            new Keyframe(0f, 0.5f),   // At 0% buzz, 50% speed
            new Keyframe(0.3f, 0.7f), // At 30% buzz, 70% speed
            new Keyframe(0.5f, 1.0f), // At 50% buzz, normal speed
            new Keyframe(0.8f, 1.2f), // At 80% buzz, 120% speed
            new Keyframe(1.0f, 1.3f)  // At 100% buzz, 130% speed
        );
        #endregion
        
        #region Card System
        [Header("Card System")]
        // Card deck system
        private List<Card> deck = new List<Card>();
        private List<Card> hand = new List<Card>();
        private List<Card> discardPile = new List<Card>();
        [SerializeField] private int initialHandSize = 3;
        [SerializeField] private int maxHandSize = 5;
        [SerializeField] private int drawsPerTurn = 1;
        [SerializeField] private List<Card> startingDeck = new List<Card>();

        [System.Serializable]
        public class Card
        {
            public string name;
            public CardType type;
            public float energyCost;
            public float cooldown;
            public float damage;
            public float lastUseTime;

            public enum CardType
            {
                Attack,
                Defense,
                Utility,
                Special
            }
        }

        // Event for when cards are drawn
        public delegate void CardDrawnHandler(Card card);
        public event CardDrawnHandler OnCardDrawn;
        
        // Event for when cards are played
        public delegate void CardPlayedHandler(Card card);
        public event CardPlayedHandler OnCardPlayed;
        #endregion
        
        #region Character State
        [Header("Character State")]
        [Tooltip("Current character state")]
        [SerializeField] private CharacterState _currentState = CharacterState.Normal;
        
        public enum CharacterState
        {
            Normal,
            Critical,
            Incapacitated
        }
        
        // Property for current buzz level
        public float currentBuzz
        {
            get { return _currentBuzz; }
            private set
            {
                float previousBuzz = _currentBuzz;
                _currentBuzz = Mathf.Clamp(value, 0f, maxBuzz);
                
                // Check if we crossed the critical threshold
                if (previousBuzz > criticalBuzzThreshold && _currentBuzz <= criticalBuzzThreshold)
                {
                    EnterCriticalState();
                }
                else if (previousBuzz <= criticalBuzzThreshold && _currentBuzz > criticalBuzzThreshold)
                {
                    ExitCriticalState();
                }
                
                // Apply penalties if buzz is low
                if (_currentBuzz < lowBuzzThreshold)
                {
                    ApplyLowBuzzPenalties();
                }
                else
                {
                    RemoveLowBuzzPenalties();
                }
                
                // Normalize and report the buzz level to any listeners
                float normalizedBuzz = _currentBuzz / maxBuzz;
                OnBuzzLevelChanged?.Invoke(normalizedBuzz);
            }
        }
        
        // Property for current health
        public float CurrentHealth
        {
            get { return currentHealth; }
            set { currentHealth = Mathf.Clamp(value, 0, maxHealth); }
        }
        
        // Property for current energy
        public float CurrentEnergy
        {
            get { return currentEnergy; }
            set { currentEnergy = Mathf.Clamp(value, 0, maxEnergy); }
        }
        
        // Event for when buzz level changes
        public delegate void BuzzLevelChangedHandler(float normalizedBuzzLevel);
        public event BuzzLevelChangedHandler OnBuzzLevelChanged;
        
        // Event for when character state changes
        public delegate void CharacterStateChangedHandler(CharacterState newState);
        public event CharacterStateChangedHandler OnCharacterStateChanged;
        
        // Property for character state
        public CharacterState currentState
        {
            get { return _currentState; }
            private set
            {
                if (_currentState != value)
                {
                    _currentState = value;
                    OnCharacterStateChanged?.Invoke(_currentState);
                }
            }
        }
        
        // Property to check if buzz is at critical level
        public bool IsBuzzCritical => currentBuzz <= criticalBuzzThreshold;
        
        // Character references
        private PlayerController playerController;
        
        // Stat modifiers and boosts
        private Dictionary<string, List<StatBoost>> statBoosts = new Dictionary<string, List<StatBoost>>();
        
        private class StatBoost
        {
            public string statName;
            public int amount;
            public bool isPercentage;
            public float duration;
            public string source;
            public float startTime;
            
            public StatBoost(string statName, int amount, bool isPercentage, float duration, string source)
            {
                this.statName = statName;
                this.amount = amount;
                this.isPercentage = isPercentage;
                this.duration = duration;
                this.source = source;
                this.startTime = Time.time;
            }
            
            public bool IsExpired => duration > 0 && (Time.time - startTime) > duration;
        }
        #endregion
        
        #region Unity Lifecycle
        private void Awake()
        {
            // Get references to attached components
            playerController = GetComponent<PlayerController>();
            
            // Initialize character stats
            currentHealth = maxHealth;
            currentEnergy = maxEnergy;
            
            // Initialize buzz level
            _currentBuzz = maxBuzz / 2;
        }
        
        private void Start()
        {
            // Register this character with the MainGameController
            if (MainGameController.Instance != null)
            {
                // We could register with the game controller here if needed
                Debug.Log("Gambler character initialized and registered with game controller");
            }
            else
            {
                Debug.LogWarning("MainGameController instance not found");
            }
            
            // Initialize card deck
            InitializeDeck();
            ShuffleDeck();
            DrawInitialHand();
        }
        
        private void Update()
        {
            // Update buzz level over time
            if (currentState != CharacterState.Incapacitated)
            {
                // Natural buzz changes
                if (currentBuzz < maxBuzz)
                {
                    currentBuzz += naturalBuzzGainRate * Time.deltaTime;
                }
                
                // Decrease buzz level over time
                currentBuzz -= buzzDecayRate * Time.deltaTime;
                
                // Apply buzz effects
                ApplyBuzzEffects();
                
                // Update stat boosts
                UpdateStatBoosts();
            }
        }
        #endregion
        
        #region Buzz Management
        /// <summary>
        /// Sets the buzz level to a specific value
        /// </summary>
        /// <param name="value">New buzz level</param>
        public void SetBuzzLevel(float value)
        {
            currentBuzz = value;
            Debug.Log($"Buzz level set to {currentBuzz} / {maxBuzz}");
        }
        
        /// <summary>
        /// Adjusts the buzz level by a delta amount
        /// </summary>
        /// <param name="delta">Amount to change buzz level</param>
        public void AdjustBuzzLevel(float delta)
        {
            currentBuzz += delta;
            Debug.Log($"Buzz level adjusted by {delta}, now {currentBuzz} / {maxBuzz}");
        }
        
        /// <summary>
        /// Enter critical buzz state
        private void EnterCriticalState()
        {
            currentState = CharacterState.Critical;
            
            // Apply critical buzz effects
            if (playerController != null)
            {
                // For example, reduce movement speed
                playerController.movementSpeedMultiplier = 0.7f;
            }
            
            Debug.Log("Entered critical buzz state!");
        }
        
        /// <summary>
        /// Exit critical buzz state
        /// </summary>
        private void ExitCriticalState()
        {
            currentState = CharacterState.Normal;
            
            // Remove critical buzz effects
            if (playerController != null)
            {
                // Reset movement speed
                playerController.movementSpeedMultiplier = 1.0f;
            }
            
            Debug.Log("Exited critical buzz state");
        }
        
        /// <summary>
        /// Apply buzz level effects to the character
        /// </summary>
        private void ApplyBuzzEffects()
        {
            if (playerController != null)
            {
                // Calculate movement speed based on buzz level
                float normalizedBuzz = currentBuzz / maxBuzz;
                float speedMultiplier = movementSpeedMultiplier.Evaluate(normalizedBuzz);
                
                // Apply to player controller
                playerController.movementSpeedMultiplier = speedMultiplier;
            }
            
            // Additional buzz effects could be applied here
            // Like screen effects, audio changes, etc.
        }
        
        /// <summary>
        /// Apply penalties for low buzz
        /// </summary>
        private void ApplyLowBuzzPenalties()
        {
            // Reduce accuracy and defense when buzz is low
            AddStatBoost("accuracy", -Mathf.RoundToInt(accuracy * accuracyPenalty), true, -1, "LowBuzz");
            AddStatBoost("defense", -Mathf.RoundToInt(defense * defensePenalty), true, -1, "LowBuzz");
            
            Debug.Log("Low buzz penalties applied");
        }
        
        /// <summary>
        /// Remove penalties for low buzz
        /// </summary>
        private void RemoveLowBuzzPenalties()
        {
            // Remove any penalty boosts with the "LowBuzz" source
            RemoveStatBoostsBySource("LowBuzz");
            
            Debug.Log("Low buzz penalties removed");
        }
        #endregion
        
        #region Card System Management
        /// <summary>
        /// Initialize the card deck with starting cards
        /// </summary>
        private void InitializeDeck()
        {
            // Clear existing deck
            deck.Clear();
            
            // Add starting cards if specified
            if (startingDeck.Count > 0)
            {
                deck.AddRange(startingDeck);
            }
            else
            {
                // Add default starting cards if no custom deck is specified
                for (int i = 0; i < 3; i++)
                {
                    Card basicAttack = new Card
                    {
                        name = "Basic Attack",
                        type = Card.CardType.Attack,
                        energyCost = 1,
                        cooldown = 0,
                        damage = 5,
                        lastUseTime = 0
                    };
                    deck.Add(basicAttack);
                }
                
                for (int i = 0; i < 2; i++)
                {
                    Card basicDefense = new Card
                    {
                        name = "Basic Defense",
                        type = Card.CardType.Defense,
                        energyCost = 1,
                        cooldown = 0,
                        damage = 0,
                        lastUseTime = 0
                    };
                    deck.Add(basicDefense);
                }
                
                Card utilityCard = new Card
                {
                    name = "Quick Buzz",
                    type = Card.CardType.Utility,
                    energyCost = 2,
                    cooldown = 10,
                    damage = 0,
                    lastUseTime = 0
                };
                deck.Add(utilityCard);
            }
            
            Debug.Log($"Deck initialized with {deck.Count} cards");
        }
        
        /// <summary>
        /// Shuffle the deck
        /// </summary>
        private void ShuffleDeck()
        {
            // Fisher-Yates shuffle algorithm
            System.Random rng = new System.Random();
            int n = deck.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Card temp = deck[k];
                deck[k] = deck[n];
                deck[n] = temp;
            }
            
            Debug.Log("Deck shuffled");
        }
        
        /// <summary>
        /// Draw the initial hand of cards
        /// </summary>
        private void DrawInitialHand()
        {
            // Draw cards up to the initial hand size
            for (int i = 0; i < initialHandSize && deck.Count > 0; i++)
            {
                DrawCard();
            }
        }
        
        /// <summary>
        /// Draw a card from the deck
        /// </summary>
        public Card DrawCard()
        {
            // Check if we need to reshuffle
            if (deck.Count == 0 && discardPile.Count > 0)
            {
                // Move cards from discard pile back to deck
                deck.AddRange(discardPile);
                discardPile.Clear();
                
                // Shuffle deck
                ShuffleDeck();
                
                Debug.Log("Discard pile reshuffled into deck");
            }
            
            // Check if we can draw
            if (deck.Count == 0)
            {
                Debug.LogWarning("Cannot draw card: Deck is empty");
                return null;
            }
            
            // Check if hand is full
            if (hand.Count >= maxHandSize)
            {
                Debug.LogWarning("Cannot draw card: Hand is full");
                return null;
            }
            
            // Draw top card
            Card drawnCard = deck[0];
            deck.RemoveAt(0);
            hand.Add(drawnCard);
            
            // Notify listeners
            OnCardDrawn?.Invoke(drawnCard);
            
            Debug.Log($"Drew card: {drawnCard.name}");
            return drawnCard;
        }
        
        /// <summary>
        /// Play a card from hand
        /// </summary>
        public bool PlayCard(int cardIndex)
        {
            // Check if index is valid
            if (cardIndex < 0 || cardIndex >= hand.Count)
            {
                Debug.LogWarning($"Invalid card index: {cardIndex}");
                return false;
            }
            
            Card card = hand[cardIndex];
            
            // Check if we have enough energy
            if (currentEnergy < card.energyCost)
            {
                Debug.LogWarning($"Not enough energy to play {card.name}");
                return false;
            }
            
            // Check cooldown
            if (Time.time - card.lastUseTime < card.cooldown)
            {
                Debug.LogWarning($"{card.name} is still on cooldown");
                return false;
            }
            
            // Remove card from hand
            hand.RemoveAt(cardIndex);
            
            // Apply card effects
            ApplyCardEffects(card);
            
            // Update card last use time
            card.lastUseTime = Time.time;
            
            // Use energy
            currentEnergy -= card.energyCost;
            
            // Move to discard pile
            discardPile.Add(card);
            
            // Notify listeners
            OnCardPlayed?.Invoke(card);
            
            Debug.Log($"Played card: {card.name}");
            return true;
        }
        
        /// <summary>
        /// Apply the effects of a played card
        /// </summary>
        private void ApplyCardEffects(Card card)
        {
            switch (card.type)
            {
                case Card.CardType.Attack:
                    // Apply attack effects
                    // This would interact with the combat system
                    Debug.Log($"Applied attack card effect: {card.damage} damage");
                    break;
                    
                case Card.CardType.Defense:
                    // Apply defense boost
                    AddStatBoost("defense", 5, false, 3f, "Card:" + card.name);
                    Debug.Log($"Applied defense card effect");
                    break;
                    
                case Card.CardType.Utility:
                    // Apply utility effect (e.g., buzz boost)
                    AdjustBuzzLevel(10f);
                    Debug.Log($"Applied utility card effect");
                    break;
                    
                case Card.CardType.Special:
                    // Apply special effects
                    Debug.Log($"Applied special card effect");
                    break;
            }
        }
        
        /// <summary>
        /// Add a stat boost to a character stat
        /// </summary>
        private void AddStatBoost(string statName, int amount, bool isPercentage, float duration, string source)
        {
            // Initialize the boost list for this stat if it doesn't exist
            if (!statBoosts.ContainsKey(statName))
            {
                statBoosts[statName] = new List<StatBoost>();
            }
            
            // Create and add the boost
            StatBoost boost = new StatBoost(statName, amount, isPercentage, duration, source);
            statBoosts[statName].Add(boost);
            
            Debug.Log($"Added {(isPercentage ? "percentage" : "flat")} stat boost to {statName}: {amount} from {source}");
        }
        
        /// <summary>
        /// Remove all stat boosts with a specific source
        /// </summary>
        private void RemoveStatBoostsBySource(string source)
        {
            foreach (string statName in statBoosts.Keys.ToList())
            {
                statBoosts[statName].RemoveAll(boost => boost.source == source);
            }
        }
        
        /// <summary>
        /// Update stat boosts, removing expired ones
        /// </summary>
        private void UpdateStatBoosts()
        {
            foreach (string statName in statBoosts.Keys.ToList())
            {
                statBoosts[statName].RemoveAll(boost => boost.IsExpired);
            }
        }
        
        /// <summary>
        /// Get the total boost value for a stat
        /// </summary>
        public int GetStatBoostTotal(string statName)
        {
            if (!statBoosts.ContainsKey(statName))
            {
                return 0;
            }
            
            int flatBonus = 0;
            float percentageBonus = 0;
            
            foreach (StatBoost boost in statBoosts[statName])
            {
                if (boost.isPercentage)
                {
                    percentageBonus += boost.amount / 100f;
                }
                else
                {
                    flatBonus += boost.amount;
                }
            }
            
            int baseValue = 0;
            switch (statName.ToLower())
            {
                case "accuracy":
                    baseValue = accuracy;
                    break;
                case "defense":
                    baseValue = defense;
                    break;
                default:
                    return flatBonus;
            }
            
            return flatBonus + Mathf.RoundToInt(baseValue * percentageBonus);
        }
        
        #region Character Actions
        /// <summary>
        /// Consume an item to restore buzz
        /// </summary>
        /// <param name="buzzAmount">Amount of buzz to restore</param>
        public void ConsumeItem(float buzzAmount)
        {
            currentBuzz += buzzAmount;
            Debug.Log($"Consumed item, restored {buzzAmount} buzz. Current buzz: {currentBuzz} / {maxBuzz}");
        }
        
        /// <summary>
        /// Use a special ability
        /// </summary>
        /// <param name="abilityName">Name of the ability to use</param>
        public void UseAbility(string abilityName)
        {
            // This would trigger special abilities
            Debug.Log($"Used ability: {abilityName}");
            
            // Example: Abilities might cost buzz
            AdjustBuzzLevel(-5f);
        }
        #endregion
    }
}
