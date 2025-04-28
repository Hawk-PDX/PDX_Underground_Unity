using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PDXUnderground.Core.Interfaces;
using PDXUnderground.Core;
using PDXUnderground.Core.Models;
using PDXUnderground.Core.Data;  // Additional namespace for model data
using PDXUnderground.UI;

namespace PDXUnderground.Utilities
{
    /// <summary>
    /// Runtime debugging tool for PDX Underground
    /// Provides visual debugging, performance monitoring, and development tools
    /// Only included in development builds
    /// </summary>
    public class PDXUndergroundDebugger : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Debug UI")]
        [SerializeField] private GameObject debugPanel;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private TextMeshProUGUI eventLogText;
        [SerializeField] private Transform visualDebugContainer;
        [SerializeField] private GameObject boundingBoxPrefab;
        
        [Header("Development Tools")]
        [SerializeField] private Slider buzzLevelSlider;
        [SerializeField] private Button normalBuzzButton;
        [SerializeField] private Button lowBuzzButton;
        [SerializeField] private Button criticalBuzzButton;
        [SerializeField] private Button[] environmentButtons;
        
        [Header("Card Testing")]
        [SerializeField] private Button drawCardButton;
        [SerializeField] private Button useCardButton;
        [SerializeField] private TMP_Dropdown cardTypeDropdown;
        
        [Header("Settings")]
        [SerializeField] private int maxLogEntries = 50;
        [SerializeField] private float updateInterval = 0.5f;
        [SerializeField] private bool showBoundingBoxes = true;
        [SerializeField] private bool trackPerformance = true;
        [SerializeField] private KeyCode toggleKey = KeyCode.F12;
        #endregion
        
        #region Private Variables
        // System references
        private IBuzzSystem buzzSystem;
        private ICardSystem cardSystem;
        private IEnvironmentSystem environmentSystem;
        private UI.BuzzUIController buzzUI;
        
        // Debug state
        private bool debugVisible = false;
        private List<DebugLogEntry> eventLog = new List<DebugLogEntry>();
        private Dictionary<GameObject, GameObject> boundingBoxes = new Dictionary<GameObject, GameObject>();
        private Dictionary<string, PerformanceMetric> performanceMetrics = new Dictionary<string, PerformanceMetric>();
        
        // Timing and updating
        private float lastUpdateTime = 0f;
        private float fpsUpdateTime = 0f;
        private int frameCount = 0;
        private float currentFps = 0f;
        
        // Test cards for development
        private ICardSystem.Card[] testCards;
        
        // Static instance for global access
        private static PDXUndergroundDebugger _instance;
        public static PDXUndergroundDebugger Instance => _instance;
        #endregion
        
        #region Unity Lifecycle Methods
        private void Awake()
        {
            // Setup singleton pattern
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            
            // Keep debugger across scenes
            DontDestroyOnLoad(gameObject);
            
            // Initialize debugging components
            InitializeDebugger();
        }
        
        private void Start()
        {
            // Initialize test cards
            InitializeTestCards();
            
            // Find game systems
            FindGameSystems();
            
            // Setup UI controls
            SetupUIControls();
            
            // Hide debug UI initially
            ToggleDebugUI(false);
            
            // Log debugger startup
            LogEvent("Debugger initialized", LogType.Info);
        }
        
        private void Update()
        {
            // Toggle debug UI with key press
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleDebugUI(!debugVisible);
            }
            
            // Only update when visible
            if (!debugVisible) return;
            
            // Calculate FPS
            CalculateFPS();
            
            // Update debug info at interval
            if (Time.unscaledTime - lastUpdateTime > updateInterval)
            {
                UpdateDebugInfo();
                lastUpdateTime = Time.unscaledTime;
            }
            
            // Update visual debugging
            if (showBoundingBoxes)
            {
                UpdateVisualDebugging();
            }
        }
        
        private void OnDestroy()
        {
            // Clean up any event subscriptions
            UnsubscribeFromEvents();
            
            // Clean up visual debugging
            ClearBoundingBoxes();
            
            if (_instance == this)
            {
                _instance = null;
            }
        }
        #endregion
        
        #region Initialization Methods
        /// <summary>
        /// Initialize debugger components
        /// </summary>
        private void InitializeDebugger()
        {
            // Check if we should be enabled
            #if !DEVELOPMENT_BUILD && !UNITY_EDITOR
            gameObject.SetActive(false);
            return;
            #endif
            
            // Initialize the circular log buffer
            eventLog = new List<DebugLogEntry>(maxLogEntries);
            
            // Subscribe to application log messages
            Application.logMessageReceived += HandleUnityLog;
        }
        
        /// <summary>
        /// Find all relevant game systems
        /// </summary>
        private void FindGameSystems()
        {
            // Find buzz system
            buzzSystem = FindObjectOfType<PDXUnderground.Player.GamblerCharacter>() as IBuzzSystem;
            if (buzzSystem != null)
            {
                buzzSystem.OnBuzzChanged += HandleBuzzChanged;
                buzzSystem.OnBuzzStateChanged += HandleBuzzStateChanged;
                LogEvent("Found IBuzzSystem", LogType.Info);
            }
            else
            {
                LogEvent("IBuzzSystem not found", LogType.Warning);
            }
            
            // Find card system
            cardSystem = FindObjectOfType<PDXUnderground.Player.GamblerCharacter>() as ICardSystem;
            if (cardSystem != null)
            {
                cardSystem.OnCardUsed += HandleCardUsed;
                cardSystem.OnCardDrawn += HandleCardDrawn;
                LogEvent("Found ICardSystem", LogType.Info);
            }
            else
            {
                LogEvent("ICardSystem not found", LogType.Warning);
            }
            
            // Find environment system
            environmentSystem = FindObjectOfType<PDXUnderground.Core.MainGameController>() as IEnvironmentSystem;
            if (environmentSystem != null)
            {
                environmentSystem.OnEnvironmentChanged += HandleEnvironmentChanged;
                LogEvent("Found IEnvironmentSystem", LogType.Info);
            }
            else
            {
                LogEvent("IEnvironmentSystem not found", LogType.Warning);
            }
            
                buzzUI = FindObjectOfType<UI.BuzzUIController>();
            if (buzzUI != null)
            {
                LogEvent("Found BuzzUIController", LogType.Info);
            }
            else
            {
                LogEvent("BuzzUIController not found", LogType.Warning);
            }
        }
        
        /// <summary>
        /// Setup the UI control buttons and sliders
        /// </summary>
        private void SetupUIControls()
        {
            // Setup buzz level slider
            if (buzzLevelSlider != null && buzzSystem != null)
            {
                buzzLevelSlider.onValueChanged.AddListener(value => {
                    StartCoroutine(MeasurePerformance("SetBuzzLevel", () => {
                        buzzSystem.SetBuzzLevel(value * 100f);
                    }));
                });
            }
            
            // Setup buzz state buttons
            if (normalBuzzButton != null && buzzSystem != null)
            {
                normalBuzzButton.onClick.AddListener(() => {
                    buzzSystem.SetBuzzState(IBuzzSystem.BuzzState.Normal);
                    LogEvent("Set buzz state: Normal", LogType.Debug);
                });
            }
            
            if (lowBuzzButton != null && buzzSystem != null)
            {
                lowBuzzButton.onClick.AddListener(() => {
                    buzzSystem.SetBuzzState(IBuzzSystem.BuzzState.Low);
                    LogEvent("Set buzz state: Low", LogType.Debug);
                });
            }
            
            if (criticalBuzzButton != null && buzzSystem != null)
            {
                criticalBuzzButton.onClick.AddListener(() => {
                    buzzSystem.SetBuzzState(IBuzzSystem.BuzzState.Critical);
                    LogEvent("Set buzz state: Critical", LogType.Debug);
                });
            }
            
            // Setup environment buttons
            if (environmentButtons != null && environmentButtons.Length > 0 && environmentSystem != null)
            {
                for (int i = 0; i < environmentButtons.Length && i < 3; i++)
                {
                    int envIndex = i;
                    if (environmentButtons[i] != null)
                    {
                        environmentButtons[i].onClick.AddListener(() => {
                            // Environment types: 0=streets, 1=tunnels, 2=speakeasy
                            string envName = envIndex == 0 ? "Streets" : (envIndex == 1 ? "Tunnels" : "Speakeasy");
                            LogEvent($"Set environment: {envName}", LogType.Debug);
                            
                            // Mock environment change event since we don't have direct setter
                            if (environmentSystem.OnEnvironmentChanged != null)
                            {
                                environmentSystem.OnEnvironmentChanged.Invoke(envIndex, envName);
                            }
                        });
                    }
                }
            }
            
            // Setup card testing controls
            if (drawCardButton != null && cardSystem != null)
            {
                drawCardButton.onClick.AddListener(() => {
                    if (testCards != null && testCards.Length > 0)
                    {
                        int index = cardTypeDropdown != null ? cardTypeDropdown.value : 0;
                        index = Mathf.Clamp(index, 0, testCards.Length - 1);
                        StartCoroutine(MeasurePerformance("DrawCard", () => {
                            cardSystem.DrawCard(testCards[index]);
                        }));
                        LogEvent($"Drew test card: {testCards[index].name}", LogType.Debug);
                    }
                });
            }
            
            if (useCardButton != null && cardSystem != null)
            {
                useCardButton.onClick.AddListener(() => {
                    var hand = cardSystem.GetCurrentHand();
                    if (hand != null && hand.Count > 0)
                    {
                        StartCoroutine(MeasurePerformance("UseCard", () => {
                            cardSystem.UseCard(hand[0]);
                        }));
                        LogEvent($"Used card: {hand[0].name}", LogType.Debug);
                    }
                    else
                    {
                        LogEvent("No cards in hand to use", LogType.Warning);
                    }
                });
            }
            
            // Setup card type dropdown
            if (cardTypeDropdown != null)
            {
                cardTypeDropdown.ClearOptions();
                List<string> options = new List<string>
                {
                    "Attack Card",
                    "Defense Card",
                    "Utility Card",
                    "Special Card"
                };
                cardTypeDropdown.AddOptions(options);
            }
        }
        
        /// <summary>
        /// Initialize test cards for development
        /// </summary>
        private void InitializeTestCards()
        {
            testCards = new ICardSystem.Card[4];
            
            // Create test cards of each type
            testCards[0] = new ICardSystem.Card() { name = "Test Attack Card", type = ICardSystem.CardType.Attack, energyCost = 2, cooldown = 3, damage = 15 };
            testCards[1] = new ICardSystem.Card() { name = "Test Defense Card", type = ICardSystem.CardType.Defense, energyCost = 1, cooldown = 5, damage = 0 };
            testCards[2] = new ICardSystem.Card() { name = "Test Utility Card", type = ICardSystem.CardType.Utility, energyCost = 1, cooldown = 2, damage = 0 };
            testCards[2] = new ICardSystem.Card() { name = "Test Utility Card", type = ICardSystem.CardType.Utility, energyCost = 1, cooldown = 2, damage = 0 };
            testCards[3] = new ICardSystem.Card() { name = "Test Special Card", type = ICardSystem.CardType.Special, energyCost = 3, cooldown = 8, damage = 10 };
        }

        /// <summary>
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            // Unsubscribe from application logs
            Application.logMessageReceived -= HandleUnityLog;
            
            // Unsubscribe from game system events
            if (buzzSystem != null)
            {
                buzzSystem.OnBuzzChanged -= HandleBuzzChanged;
                buzzSystem.OnBuzzStateChanged -= HandleBuzzStateChanged;
            }
            
            if (cardSystem != null)
            {
                cardSystem.OnCardUsed -= HandleCardUsed;
                cardSystem.OnCardDrawn -= HandleCardDrawn;
            }
            
            if (environmentSystem != null)
            {
                environmentSystem.OnEnvironmentChanged -= HandleEnvironmentChanged;
            }
        }
        #endregion
        
        #region Event Handlers
        /// <summary>
        /// Handle buzz level changes
        /// </summary>
        private void HandleBuzzChanged(float currentBuzz, float maxBuzz)
        {
            LogEvent($"Buzz changed: {currentBuzz}/{maxBuzz}", LogType.Debug);
        }
        
        /// <summary>
        /// Handle buzz state changes
        /// </summary>
        private void HandleBuzzStateChanged(IBuzzSystem.BuzzState newState)
        {
            LogEvent($"Buzz state changed: {newState}", LogType.Debug);
        }
        
        /// <summary>
        /// Handle card use
        /// </summary>
        private void HandleCardUsed(ICardSystem.Card card)
        {
            LogEvent($"Card used: {card.name} ({card.type})", LogType.Debug);
        }
        
        /// <summary>
        /// Handle card draw
        /// </summary>
        private void HandleCardDrawn(ICardSystem.Card card)
        {
            LogEvent($"Card drawn: {card.name} ({card.type})", LogType.Debug);
        }
        
        /// <summary>
        /// Handle environment changes
        /// </summary>
        private void HandleEnvironmentChanged(int environmentType, string environmentName)
        {
            LogEvent($"Environment changed: {environmentName} (Type: {environmentType})", LogType.Debug);
        }
        
        /// <summary>
        /// Handle Unity log messages
        /// </summary>
        private void HandleUnityLog(string logString, string stackTrace, UnityEngine.LogType logType)
        {
            LogType type = LogType.Info;
            
            // Convert Unity log types to our log types
            switch (logType)
            {
                case UnityEngine.LogType.Error:
                case UnityEngine.LogType.Exception:
                case UnityEngine.LogType.Assert:
                    type = LogType.Error;
                    break;
                case UnityEngine.LogType.Warning:
                    type = LogType.Warning;
                    break;
                case UnityEngine.LogType.Log:
                    type = LogType.Info;
                    break;
            }
            
            // Only log PDXUnderground related messages to avoid spam
            if (logString.Contains("PDXUnderground"))
            {
                LogEvent(logString, type);
            }
        }
        #endregion
        
        #region Debug UI Methods
        /// <summary>
        /// Update the debug information display
        /// </summary>
        private void UpdateDebugInfo()
        {
            if (statsText == null) return;
            
            StringBuilder stats = new StringBuilder();
            
            // Basic info
            stats.AppendLine($"<b>PDX Underground Debugger</b> | FPS: {currentFps:F1}");
            stats.AppendLine($"Memory: {SystemInfo.systemMemorySize / 1024:F1} GB | Time: {Time.time:F1}s");
            stats.AppendLine();
            
            // Game state
            stats.AppendLine("<b>Game State:</b>");
            
            // Buzz system info
            if (buzzSystem != null)
            {
                stats.AppendLine($"Buzz State: {buzzSystem.CurrentBuzzState}");
                stats.AppendLine($"Buzz Level: {buzzSystem.CurrentBuzzLevel:F1}/{buzzSystem.MaxBuzzLevel:F1} ({buzzSystem.CurrentBuzzLevel / buzzSystem.MaxBuzzLevel * 100:F1}%)");
            }
            else
            {
                stats.AppendLine("Buzz System: Not Found");
            }
            
            // Card system info
            if (cardSystem != null)
            {
                var hand = cardSystem.GetCurrentHand();
                stats.AppendLine($"Cards in Hand: {(hand != null ? hand.Count : 0)}");
                if (hand != null && hand.Count > 0)
                {
                    stats.AppendLine("  Card Types:");
                    Dictionary<ICardSystem.CardType, int> cardCounts = new Dictionary<ICardSystem.CardType, int>();
                    foreach (var card in hand)
                    {
                        if (!cardCounts.ContainsKey(card.type))
                            cardCounts[card.type] = 0;
                        cardCounts[card.type]++;
                    }
                    
                    foreach (var kvp in cardCounts)
                    {
                        stats.AppendLine($"    {kvp.Key}: {kvp.Value}");
                    }
                }
            }
            else
            {
                stats.AppendLine("Card System: Not Found");
            }
            
            // Environment info
            if (environmentSystem != null)
            {
                stats.AppendLine($"Environment: {environmentSystem.CurrentEnvironmentType}");
            }
            else
            {
                stats.AppendLine("Environment System: Not Found");
            }
            
            stats.AppendLine();
            
            // Performance metrics
            if (trackPerformance && performanceMetrics.Count > 0)
            {
                stats.AppendLine("<b>Performance Metrics:</b>");
                foreach (var kvp in performanceMetrics)
                {
                    if (kvp.Value != null)
                    {
                        stats.AppendLine($"{kvp.Key}: {kvp.Value.averageMs:F2}ms (min: {kvp.Value.minMs:F2}ms, max: {kvp.Value.maxMs:F2}ms)");
                    }
                }
                stats.AppendLine();
            }
            
            // UI stats
            if (buzzUI != null)
            {
                stats.AppendLine("<b>UI Information:</b>");
                stats.AppendLine($"UI Card Elements: {buzzUI.GetComponentsInChildren<UI.CardPrefab>().Length}");
                stats.AppendLine($"UI Active Elements: {buzzUI.gameObject.activeInHierarchy}");
            }
            
            // Update text
            statsText.text = stats.ToString();
            
            // Update event log
            UpdateEventLog();
        }
        
        /// <summary>
        /// Toggle the debug UI visibility
        /// </summary>
        private void ToggleDebugUI(bool visible)
        {
            debugVisible = visible;
            
            // Toggle panel visibility
            if (debugPanel != null)
            {
                debugPanel.SetActive(visible);
            }
            
            // Toggle bounding boxes
            if (visualDebugContainer != null)
            {
                visualDebugContainer.gameObject.SetActive(visible && showBoundingBoxes);
            }
            
            // Log the state change
            LogEvent($"Debug UI {(visible ? "shown" : "hidden")}", LogType.Debug);
        }
        
        /// <summary>
        /// Calculate frames per second
        /// </summary>
        private void CalculateFPS()
        {
            frameCount++;
            float timeElapsed = Time.unscaledTime - fpsUpdateTime;
            
            // Update FPS every 0.5 seconds
            if (timeElapsed >= 0.5f)
            {
                currentFps = frameCount / timeElapsed;
                frameCount = 0;
                fpsUpdateTime = Time.unscaledTime;
            }
        }
        
        /// <summary>
        /// Update the event log display
        /// </summary>
        private void UpdateEventLog()
        {
            if (eventLogText == null) return;
            
            StringBuilder logText = new StringBuilder();
            logText.AppendLine("<b>Event Log:</b>");
            
            // Show the most recent events first, limited by maxLogEntries
            int count = Mathf.Min(eventLog.Count, maxLogEntries);
            for (int i = 0; i < count; i++)
            {
                int index = eventLog.Count - 1 - i;
                if (index >= 0 && index < eventLog.Count)
                {
                    DebugLogEntry entry = eventLog[index];
                    
                    // Colorize based on log type
                    string color = "white";
                    switch (entry.type)
                    {
                        case LogType.Error:
                            color = "red";
                            break;
                        case LogType.Warning:
                            color = "yellow";
                            break;
                        case LogType.Debug:
                            color = "cyan";
                            break;
                        case LogType.Info:
                            color = "white";
                            break;
                    }
                    
                    logText.AppendLine($"<color={color}>[{entry.time:HH:mm:ss}] {entry.message}</color>");
                }
            }
            
            eventLogText.text = logText.ToString();
        }
        #endregion
        
        #region Visual Debugging Methods
        /// <summary>
        /// Update visual debugging elements
        /// </summary>
        private void UpdateVisualDebugging()
        {
            if (visualDebugContainer == null || !showBoundingBoxes) return;
            
            // Clear old bounding boxes
            ClearBoundingBoxes();
            
            // Add bounding boxes for important UI elements
            if (buzzUI != null)
            {
                // Find UI elements to debug
                var cardPrefabs = buzzUI.GetComponentsInChildren<UI.CardPrefab>(true);
                foreach (var cardPrefab in cardPrefabs)
                {
                    if (cardPrefab.gameObject.activeInHierarchy)
                    {
                        DrawBoundingBox(cardPrefab.gameObject, Color.green);
                    }
                }
                
                // Find important UI elements using tags or names
                var buzzUIElements = buzzUI.GetComponentsInChildren<RectTransform>(true);
                foreach (var element in buzzUIElements)
                {
                    if (element.gameObject.activeInHierarchy && 
                        (element.name.Contains("Meter") || 
                         element.name.Contains("Icon") || 
                         element.name.Contains("Button")))
                    {
                        DrawBoundingBox(element.gameObject, Color.yellow);
                    }
                }
            }
        }
        
        /// <summary>
        /// Draw a bounding box around a GameObject
        /// </summary>
        /// <param name="target">Target GameObject</param>
        /// <param name="color">Box color</param>
        private void DrawBoundingBox(GameObject target, Color color)
        {
            if (target == null || boundingBoxPrefab == null || visualDebugContainer == null) return;
            
            try
            {
                // Create bounding box if it doesn't exist
                if (!boundingBoxes.TryGetValue(target, out GameObject boundingBox) || boundingBox == null)
                {
                    boundingBox = Instantiate(boundingBoxPrefab, visualDebugContainer);
                    boundingBoxes[target] = boundingBox;
                }
                
                // Position the bounding box
                RectTransform targetRect = target.GetComponent<RectTransform>();
                if (targetRect != null)
                {
                    // For UI elements
                    RectTransform boxRect = boundingBox.GetComponent<RectTransform>();
                    if (boxRect != null)
                    {
                        // Copy the target rectangle
                        boxRect.position = targetRect.position;
                        boxRect.sizeDelta = targetRect.sizeDelta;
                        boxRect.anchorMin = targetRect.anchorMin;
                        boxRect.anchorMax = targetRect.anchorMax;
                        boxRect.pivot = targetRect.pivot;
                        boxRect.rotation = targetRect.rotation;
                        boxRect.scale = targetRect.scale;
                    }
                }
                else
                {
                    // For 3D objects
                    Renderer renderer = target.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        boundingBox.transform.position = renderer.bounds.center;
                        boundingBox.transform.localScale = renderer.bounds.size;
                    }
                    else
                    {
                        // Default fallback
                        boundingBox.transform.position = target.transform.position;
                    }
                }
                
                // Set color
                Image image = boundingBox.GetComponent<Image>();
                if (image != null)
                {
                    image.color = new Color(color.r, color.g, color.b, 0.3f);
                }
                
                // Add name label
                TextMeshProUGUI label = boundingBox.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    label.text = target.name;
                    label.color = color;
                }
                
                // Ensure it's active
                boundingBox.SetActive(true);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error drawing bounding box: {e.Message}");
            }
        }
        
        /// <summary>
        /// Clear all bounding boxes
        /// </summary>
        private void ClearBoundingBoxes()
        {
            foreach (var box in boundingBoxes.Values)
            {
                if (box != null)
                {
                    Destroy(box);
                }
            }
            
            boundingBoxes.Clear();
        }
        #endregion
        
        #region Performance Tracking Methods
        /// <summary>
        /// Measure the performance of an action
        /// </summary>
        /// <param name="name">Name of the action</param>
        /// <param name="action">Action to measure</param>
        /// <returns>Coroutine enumerator</returns>
        private IEnumerator MeasurePerformance(string name, Action action)
        {
            if (!trackPerformance) 
            {
                action?.Invoke();
                yield break;
            }
            
            float startTime = Time.realtimeSinceStartup;
            
            // Execute the action
            action?.Invoke();
            
            // Wait a frame to include any frame-end processing
            yield return null;
            
            // Calculate time
            float endTime = Time.realtimeSinceStartup;
            float elapsedMs = (endTime - startTime) * 1000f;
            
            // Update metrics
            UpdatePerformanceMetrics(name, elapsedMs);
        }
        
        /// <summary>
        /// Update performance metrics for an action
        /// </summary>
        /// <param name="name">Name of the action</param>
        /// <param name="timeMs">Time in milliseconds</param>
        private void UpdatePerformanceMetrics(string name, float timeMs)
        {
            if (!performanceMetrics.TryGetValue(name, out PerformanceMetric metric))
            {
                metric = new PerformanceMetric
                {
                    count = 0,
                    totalMs = 0,
                    minMs = float.MaxValue,
                    maxMs = float.MinValue,
                    averageMs = 0
                };
                performanceMetrics[name] = metric;
            }
            
            // Update min and max
            metric.minMs = Mathf.Min(metric.minMs, timeMs);
            metric.maxMs = Mathf.Max(metric.maxMs, timeMs);
            
            // Update running average
            metric.count++;
            metric.totalMs += timeMs;
            metric.averageMs = metric.totalMs / metric.count;
            
            // Log significant performance issues
            if (timeMs > 100f) // Significant frame drop if >100ms
            {
                LogEvent($"Performance warning: {name} took {timeMs:F2}ms", LogType.Warning);
            }
        }
        
        /// <summary>
        /// Log performance data for all tracked metrics
        /// </summary>
        private void LogPerformanceData()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Performance Metrics:");
            
            foreach (var kvp in performanceMetrics)
            {
                sb.AppendLine($"  {kvp.Key}: Avg {kvp.Value.averageMs:F2}ms (Min: {kvp.Value.minMs:F2}ms, Max: {kvp.Value.maxMs:F2}ms, Count: {kvp.Value.count})");
            }
            
            LogEvent(sb.ToString(), LogType.Info);
        }
        #endregion
        
        #region Logging Methods
        /// <summary>
        /// Log an event to the debug log
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="type">Log type</param>
        public void LogEvent(string message, LogType type)
        {
            // Create log entry
            DebugLogEntry entry = new DebugLogEntry
            {
                message = message,
                type = type,
                time = DateTime.Now
            };
            
            // Add to circular buffer
            if (eventLog.Count >= maxLogEntries)
            {
                // Remove oldest entry
                eventLog.RemoveAt(0);
            }
            
            eventLog.Add(entry);
            
            // Output to console for non-debug logs
            if (type != LogType.Debug)
            {
                switch (type)
                {
                    case LogType.Error:
                        Debug.LogError($"[PDXDebugger] {message}");
                        break;
                    case LogType.Warning:
                        Debug.LogWarning($"[PDXDebugger] {message}");
                        break;
                    default:
                        Debug.Log($"[PDXDebugger] {message}");
                        break;
                }
            }
            
            // Update the log display if visible
            if (debugVisible)
            {
                UpdateEventLog();
            }
        }
        #endregion
        
        #region Utility Methods
        /// <summary>
        /// Gets the current state of the game systems as a string
        /// </summary>
        /// <returns>Game state description</returns>
        public string GetGameStateDescription()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("PDX Underground Game State:");
            
            // Buzz system
            if (buzzSystem != null)
            {
                sb.AppendLine($"- Buzz State: {buzzSystem.CurrentBuzzState}");
                sb.AppendLine($"- Buzz Level: {buzzSystem.CurrentBuzzLevel:F1}/{buzzSystem.MaxBuzzLevel:F1}");
            }
            
            // Card system
            if (cardSystem != null)
            {
                var hand = cardSystem.GetCurrentHand();
                sb.AppendLine($"- Cards in Hand: {(hand != null ? hand.Count : 0)}");
            }
            
            // Environment
            if (environmentSystem != null)
            {
                sb.AppendLine($"- Environment: {environmentSystem.CurrentEnvironmentType}");
            }
            
            return sb.ToString();
        }
        
        /// <summary>
        /// Global access to add a log entry from anywhere in the codebase
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="type">Log type</param>
        public static void Log(string message, LogType type = LogType.Info)
        {
            if (Instance != null)
            {
                Instance.LogEvent(message, type);
            }
            else
            {
                // Fallback if instance not available
                Debug.Log($"[PDXDebugger] {message}");
            }
        }
        #endregion
        
        #region Helper Structs and Enums
        /// <summary>
        /// Performance metric data structure
        /// </summary>
        private struct PerformanceMetric
        {
            public int count;
            public float totalMs;
            public float minMs;
            public float maxMs;
            public float averageMs;
        }
        
        /// <summary>
        /// Debug log entry data structure
        /// </summary>
        private struct DebugLogEntry
        {
            public string message;
            public LogType type;
            public DateTime time;
        }
        
        /// <summary>
        /// Log entry type
        /// </summary>
        public enum LogType
        {
            Debug,
            Info,
            Warning,
            Error
        }
        #endregion
    }
}
