using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using PDXUnderground.UI;
using PDXUnderground.Player;
using PDXUnderground.Core.Interfaces;
namespace PDXUnderground.Test
{
    /// <summary>
    /// Controller for the test scene that sets up the environment and provides
    /// test functionality for the Gambler character and UI.
    /// </summary>
    public class TestSceneController : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private Canvas debugCanvas;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject uiCanvasPrefab;
        [SerializeField] private GameObject debugPanelPrefab;
        
        [Header("Test Settings")]
        [SerializeField] private bool spawnPlayerAtStart = true;
        [SerializeField] private bool spawnUIAtStart = true;
        [SerializeField] private bool enableDebugDisplay = true;
        
        [Header("Test Card Settings")]
        [SerializeField] private List<CardData> testCards = new List<CardData>();
        
        // Private references
        private GameObject playerInstance;
        private GamblerCharacter gamblerCharacter;
        private GamblerUI gamblerUI;
        private DebugPanel debugPanel;
        
        [System.Serializable]
        private class CardData
        {
            public string name = "Test Card";
            public string description = "This is a test card";
            public float energyCost = 10f;
            public float cooldown = 3f;
            public float damage = 0f;
            public ICardSystem.CardType cardType = ICardSystem.CardType.Attack;
            public ICardSystem.SpecialEffect specialEffect = ICardSystem.SpecialEffect.None;
            public int suit = 0;
            public int rank = 1;
        }
        
        private void Start()
        {
            if (spawnPlayerAtStart)
            {
                SpawnPlayer();
            }
            
            if (spawnUIAtStart)
            {
                SpawnUI();
            }
            
            if (enableDebugDisplay)
            {
                CreateDebugDisplay();
            }
        }
        
        private void Update()
        {
            // Process test inputs
            ProcessTestInputs();
            
            // Update debug display
            if (debugPanel != null && gamblerCharacter != null)
            {
                debugPanel.UpdateDisplay(gamblerCharacter);
            }
        }
        
        #region Scene Setup Methods
        
        /// <summary>
        /// Spawns the player character with the GamblerCharacter component
        /// </summary>
        public void SpawnPlayer()
        {
            if (playerInstance != null)
            {
                Destroy(playerInstance);
            }
            
            // Spawn player at designated spawn point
            Vector3 spawnPosition = playerSpawnPoint != null ? 
                playerSpawnPoint.position : new Vector3(0f, 1f, 0f);
            
            playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            playerInstance.name = "Player_Gambler";
            
            // Add GamblerCharacter component if it doesn't exist
            gamblerCharacter = playerInstance.GetComponent<GamblerCharacter>();
            if (gamblerCharacter == null)
            {
                gamblerCharacter = playerInstance.AddComponent<GamblerCharacter>();
            }
            
            // Initialize the character with test cards
            InitializeGamblerWithTestCards();
            
            Debug.Log("Player spawned with GamblerCharacter component");
        }
        
        /// <summary>
        /// Spawns the UI canvas with the GamblerUI component
        /// </summary>
        public void SpawnUI()
        {
            // Instantiate UI canvas
            GameObject uiInstance = Instantiate(uiCanvasPrefab);
            uiInstance.name = "GamblerUICanvas";
            
            // Get or add GamblerUI component
            gamblerUI = uiInstance.GetComponent<GamblerUI>();
            if (gamblerUI == null)
            {
                gamblerUI = uiInstance.AddComponent<GamblerUI>();
            }
            
            Debug.Log("UI canvas spawned with GamblerUI component");
        }
        
        /// <summary>
        /// Creates a debug display panel for monitoring character stats
        /// </summary>
        private void CreateDebugDisplay()
        {
            if (debugCanvas == null)
            {
                // Create debug canvas if it doesn't exist
                GameObject canvasObj = new GameObject("DebugCanvas");
                debugCanvas = canvasObj.AddComponent<Canvas>();
                debugCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            
            // Create debug panel
            GameObject panelObj = Instantiate(debugPanelPrefab, debugCanvas.transform);
            debugPanel = panelObj.GetComponent<DebugPanel>();
            if (debugPanel == null)
            {
                debugPanel = panelObj.AddComponent<DebugPanel>();
            }
            
            Debug.Log("Debug display created");
        }
        
        /// <summary>
        /// Initializes the Gambler character with test cards
        /// </summary>
        private void InitializeGamblerWithTestCards()
        {
            if (gamblerCharacter == null) return;
            
            // Create test cards from the defined card data
            List<ICardSystem.Card> cards = new List<ICardSystem.Card>();
            foreach (var cardData in testCards)
            {
                ICardSystem.Card card = new ICardSystem.Card
                {
                    name = cardData.name,
                    description = cardData.description,
                    energyCost = cardData.energyCost,
                    cooldown = cardData.cooldown,
                    type = cardData.cardType,
                    damage = cardData.damage,
                    specialEffect = cardData.specialEffect,
                    suit = cardData.suit,
                    rank = cardData.rank
                };
                cards.Add(card);
            }
            
            // Add default cards if no test cards were defined
            if (cards.Count == 0)
            {
                // Add some default test cards
                cards.Add(new ICardSystem.Card
                {
                    name = "Quick Strike",
                    description = "Deal 15 damage to target",
                    energyCost = 10f,
                    cooldown = 2f,
                    type = ICardSystem.CardType.Attack,
                    damage = 15f
                });
                
                cards.Add(new ICardSystem.Card
                {
                    name = "Fortify",
                    description = "Gain 20 temporary defense",
                    energyCost = 15f,
                    cooldown = 3f,
                    type = ICardSystem.CardType.Defense,
                    damage = 20f
                });
                
                cards.Add(new ICardSystem.Card
                {
                    name = "Recover",
                    description = "Heal 10 health",
                    energyCost = 20f,
                    cooldown = 4f,
                    type = ICardSystem.CardType.Utility,
                    specialEffect = ICardSystem.SpecialEffect.Heal,
                    damage = 10f
                });
                
                cards.Add(new ICardSystem.Card
                {
                    name = "Wild Card",
                    description = "Deal 25 damage but lose 10 health",
                    energyCost = 30f,
                    cooldown = 5f,
                    type = ICardSystem.CardType.Special,
                    damage = 25f
                });
            }
            
            // Initialize gambler with the cards
            gamblerCharacter.InitializeDeck(cards);
            
            // Draw initial hand
            gamblerCharacter.DrawInitialHand();
        }
        
        #endregion
        
        #region Test Input Handling
        
        /// <summary>
        /// Processes test inputs for manipulating the Gambler character
        /// </summary>
        private void ProcessTestInputs()
        {
            if (gamblerCharacter == null) return;
            
            // Card usage - number keys 1-5
            if (Input.GetKeyDown(KeyCode.Alpha1)) UseCard(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) UseCard(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) UseCard(2);
            if (Input.GetKeyDown(KeyCode.Alpha4)) UseCard(3);
            if (Input.GetKeyDown(KeyCode.Alpha5)) UseCard(4);
            
            // Draw a card
            if (Input.GetKeyDown(KeyCode.D)) gamblerCharacter.DrawCard();
            
            // Test damage - H key
            if (Input.GetKeyDown(KeyCode.H)) TakeDamage(10f);
            
            // Test healing - J key
            if (Input.GetKeyDown(KeyCode.J)) Heal(10f);
            
            // Modify buzz - Up/Down arrows
            if (Input.GetKey(KeyCode.UpArrow)) ChangeBuzz(10f * Time.deltaTime);
            if (Input.GetKey(KeyCode.DownArrow)) ChangeBuzz(-10f * Time.deltaTime);
            
            // Reset health - R key
            if (Input.GetKeyDown(KeyCode.R)) ResetHealth();
            
            // Reset buzz - B key
            if (Input.GetKeyDown(KeyCode.B)) ResetBuzz();
            
            // Draw new hand - N key
            if (Input.GetKeyDown(KeyCode.N)) gamblerCharacter.DrawNewHand();
        }
        
        /// <summary>
        /// Uses a card at the specified index
        /// </summary>
        private void UseCard(int index)
        {
            if (gamblerCharacter != null)
            {
                gamblerCharacter.UseCard(index);
            }
        }
        
        /// <summary>
        /// Makes the character take damage
        /// </summary>
        private void TakeDamage(float amount)
        {
            if (gamblerCharacter != null)
            {
                gamblerCharacter.TakeDamage(amount);
                Debug.Log($"Player took {amount} damage. Health: {gamblerCharacter.GetCurrentHealth()}/{gamblerCharacter.GetMaxHealth()}");
            }
        }
        
        /// <summary>
        /// Heals the character
        /// </summary>
        private void Heal(float amount)
        {
            if (gamblerCharacter != null)
            {
                gamblerCharacter.Heal(amount);
                Debug.Log($"Player healed {amount}. Health: {gamblerCharacter.GetCurrentHealth()}/{gamblerCharacter.GetMaxHealth()}");
            }
        }
        
        /// <summary>
        /// Changes the character's buzz by the specified amount
        /// </summary>
        private void ChangeBuzz(float amount)
        {
            if (gamblerCharacter != null)
            {
                gamblerCharacter.ModifyBuzz(amount);
            }
        }
        
        /// <summary>
        /// Resets the character's health to max
        /// </summary>
        private void ResetHealth()
        {
            if (gamblerCharacter != null)
            {
                gamblerCharacter.ResetHealth();
                Debug.Log($"Health reset to max: {gamblerCharacter.GetMaxHealth()}");
            }
        }
        
        /// <summary>
        /// Resets the character's buzz to max
        /// </summary>
        private void ResetBuzz()
        {
            if (gamblerCharacter != null)
            {
                gamblerCharacter.ResetBuzz();
                Debug.Log($"Buzz reset to max: {gamblerCharacter.GetMaxBuzz()}");
            }
        }
        
        #endregion
    }

    /// <summary>
    /// Debug panel for displaying Gambler character stats
    /// </summary>
    public class DebugPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private TextMeshProUGUI cardsText;
        [SerializeField] private TextMeshProUGUI eventsText;
        
        private List<string> eventLog = new List<string>();
        private int maxEventLogEntries = 10;
        
        private void Start()
        {
            if (statsText == null)
            {
                // Create stats text if it doesn't exist
                GameObject statsObj = new GameObject("StatsText");
                statsObj.transform.SetParent(transform);
                RectTransform statsRect = statsObj.AddComponent<RectTransform>();
                statsRect.anchorMin = new Vector2(0, 1);
                statsRect.anchorMax = new Vector2(1, 1);
                statsRect.pivot = new Vector2(0.5f, 1);
                statsRect.offsetMin = new Vector2(10, -200);
                statsRect.offsetMax = new Vector2(-10, -10);
                
                statsText = statsObj.AddComponent<TextMeshProUGUI>();
                statsText.fontSize = 16;
                statsText.color = Color.white;
                statsText.text = "Stats will appear here...";
            }
            
            if (cardsText == null)
            {
                // Create cards text if it doesn't exist
                GameObject cardsObj = new GameObject("CardsText");
                cardsObj.transform.SetParent(transform);
                RectTransform cardsRect = cardsObj.AddComponent<RectTransform>();
                cardsRect.anchorMin = new Vector2(0, 0.5f);
                cardsRect.anchorMax = new Vector2(1, 1);
                cardsRect.pivot = new Vector2(0.5f, 0.5f);
                cardsRect.offsetMin = new Vector2(10, 10);
                cardsRect.offsetMax = new Vector2(-10, -200);
                
                cardsText = cardsObj.AddComponent<TextMeshProUGUI>();
                cardsText.fontSize = 16;
                cardsText.color = Color.white;
                cardsText.text = "Cards will appear here...";
            }
            
            if (eventsText == null)
            {
                // Create events text if it doesn't exist
                GameObject eventsObj = new GameObject("EventsText");
                eventsObj.transform.SetParent(transform);
                RectTransform eventsRect = eventsObj.AddComponent<RectTransform>();
                eventsRect.anchorMin = new Vector2(0, 0);
                eventsRect.anchorMax = new Vector2(1, 0.5f);
                eventsRect.pivot = new Vector2(0.5f, 0);
                eventsRect.offsetMin = new Vector2(10, 10);
                eventsRect.offsetMax = new Vector2(-10, -10);
                
                eventsText = eventsObj.AddComponent<TextMeshProUGUI>();
                eventsText.fontSize = 16;
                eventsText.color = Color.white;
                eventsText.text = "Events will appear here...";
            }
        }
        
        /// <summary>
        /// Updates the debug display with current character stats
        /// </summary>
        public void UpdateDisplay(GamblerCharacter gambler)
        {
            if (gambler == null) return;
            
            // Update stats display
            UpdateStatsDisplay(gambler);
            
            // Update cards display
            UpdateCardsDisplay(gambler);
        }
        
        /// <summary>
        /// Updates the stats display with current character stats
        /// </summary>
        private void UpdateStatsDisplay(GamblerCharacter gambler)
        {
            if (statsText == null) return;
            
            string statsInfo = $"<b>GAMBLER STATS</b>\n" +
                $"Health: {gambler.GetCurrentHealth():F1}/{gambler.GetMaxHealth():F1}\n" +
                $"Buzz: {gambler.GetCurrentBuzzPercentage() * 100:F1}% ({gambler.GetCurrentBuzzPercentage() * gambler.GetMaxBuzz():F1}/{gambler.GetMaxBuzz():F1})\n" +
                $"Buzz State: <color={(GetBuzzStateColor(gambler.GetCurrentBuzzState()))}>{gambler.GetCurrentBuzzState()}</color>\n" +
                $"Defense: {gambler.GetCurrentDefense():F1}\n";
                
            statsText.text = statsInfo;
        }
        
        /// <summary>
        /// Updates the cards display with the current hand and deck information
        /// </summary>
        private void UpdateCardsDisplay(GamblerCharacter gambler)
        {
            if (cardsText == null) return;
            
            var currentHand = gambler.GetCurrentHand();
            
            string cardInfo = $"<b>CARDS</b>\n" +
                $"Deck Size: {gambler.GetDeckSize()}\n" +
                $"Discard Pile: {gambler.GetDiscardPileSize()}\n" +
                $"Current Hand ({currentHand.Count}):\n";
                
            // Add information about each card in hand
            for (int i = 0; i < currentHand.Count; i++)
            {
                ICardSystem.Card card = currentHand[i];
                string cardColor = GetCardTypeColor(card.type);
                
                // Format: [1] Attack Card (10 Buzz) - "Description" - [READY/COOLDOWN: X]
                bool isOnCooldown = (Time.time - card.lastUseTime) < card.cooldown;
                float remainingCooldown = Mathf.Max(0, card.cooldown - (Time.time - card.lastUseTime));
                string cooldownStatus = isOnCooldown 
                    ? $"<color=#FF6666>COOLDOWN: {remainingCooldown:F1}</color>" 
                    : "<color=#66FF66>READY</color>";
                    
                cardInfo += $"[{i+1}] <color={cardColor}>{card.name}</color> ({card.energyCost} Energy) - \"{card.description}\" - [{cooldownStatus}]\n";
            }
            
            cardsText.text = cardInfo;
        }
        
        /// <summary>
        /// Logs an event and updates the event display
        /// </summary>
        public void LogEvent(string eventMessage)
        {
            // Add timestamp
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
            string logEntry = $"[{timestamp}] {eventMessage}";
            
            // Add to log
            eventLog.Add(logEntry);
            
            // Trim log if it's getting too large
            while (eventLog.Count > maxEventLogEntries)
            {
                eventLog.RemoveAt(0);
            }
            
            // Update display
            UpdateEventDisplay();
        }
        
        /// <summary>
        /// Updates the event display with the current event log
        /// </summary>
        private void UpdateEventDisplay()
        {
            if (eventsText == null) return;
            
            string eventInfo = $"<b>EVENT LOG</b>\n";
            
            // Add all events in the log
            foreach (string logEntry in eventLog)
            {
                eventInfo += $"{logEntry}\n";
            }
            
            eventsText.text = eventInfo;
        }
        
        /// <summary>
        /// Gets the color string for a buzz state
        /// </summary>
        private string GetBuzzStateColor(IBuzzSystem.BuzzState state)
        {
            switch (state)
            {
                case IBuzzSystem.BuzzState.Normal:
                    return "#CCAA44"; // Gold
                case IBuzzSystem.BuzzState.Low:
                    return "#CC7733"; // Orange
                case IBuzzSystem.BuzzState.Critical:
                    return "#CC3333"; // Red
                default:
                    return "#FFFFFF"; // White
            }
        }
        
        /// <summary>
        /// Gets the color string for a card type
        /// </summary>
        private string GetCardTypeColor(ICardSystem.CardType cardType)
        {
            switch (cardType)
            {
                case ICardSystem.CardType.Attack:
                    return "#CC3333"; // Red
                case ICardSystem.CardType.Defense:
                    return "#3366CC"; // Blue
                case ICardSystem.CardType.Utility:
                    return "#33CC66"; // Green
                case ICardSystem.CardType.Special:
                    return "#CCAA44"; // Gold
                default:
                    return "#FFFFFF"; // White
            }
        }
    }
}
