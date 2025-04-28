using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using PDXUnderground.Core;
using PDXUnderground.Core.Interfaces;
using PDXUnderground.Effects;
using PDXUnderground.Player;
using PDXUnderground.Controllers;

namespace PDXUnderground.UI
{
    /// <summary>
    /// Controller for the Buzz UI system and card management interface
    /// Handles visualization of player buzz state, card hand, and environment changes
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    public class BuzzUIController : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Buzz Meter Elements")]
        [Tooltip("Fill image for buzz meter")]
        [SerializeField] private Image buzzMeterFill;
        [Tooltip("Background frame for buzz meter")]
        [SerializeField] private Image buzzMeterFrame;
        [Tooltip("Icon indicator for current buzz state")]
        [SerializeField] private Image buzzStateIcon;
        [Tooltip("Text display for current buzz value")]
        [SerializeField] private TextMeshProUGUI buzzValueText;
        
        [Header("Buzz State Visuals")]
        [Tooltip("Color for normal buzz state")]
        [SerializeField] private Color normalBuzzColor = new Color(0.2f, 0.8f, 0.2f, 1f); // Green
        [Tooltip("Color for low buzz state")]
        [SerializeField] private Color lowBuzzColor = new Color(0.9f, 0.5f, 0.1f, 1f); // Orange
        [Tooltip("Color for critical buzz state")]
        [SerializeField] private Color criticalBuzzColor = new Color(0.8f, 0.1f, 0.1f, 1f); // Red
        [Tooltip("Sprite for normal buzz state")]
        [SerializeField] private Sprite normalBuzzIcon;
        [Tooltip("Sprite for low buzz state")]
        [SerializeField] private Sprite lowBuzzIcon;
        [Tooltip("Sprite for critical buzz state")]
        [SerializeField] private Sprite criticalBuzzIcon;
        
        [Header("Card UI Elements")]
        [Tooltip("Card hand display container")]
        [SerializeField] private Transform cardHandContainer;
        [Tooltip("Prefab for card UI elements")]
        [SerializeField] private GameObject cardUIPrefab;
        [Tooltip("Image component for card selection indicator")]
        [SerializeField] private Image cardSelectionIndicator;
        
        [Header("Cooldown Indicators")]
        [Tooltip("Slice ability cooldown indicator")]
        [SerializeField] private Image sliceCooldownIndicator;
        [Tooltip("Flick ability cooldown indicator")]
        [SerializeField] private Image flickCooldownIndicator;
        [Tooltip("Draw card cooldown indicator")]
        [SerializeField] private Image drawCardCooldownIndicator;
        
        [Header("Notification Elements")]
        [Tooltip("Panel for buzz state change notifications")]
        [SerializeField] private GameObject buzzStateNotificationPanel;
        [Tooltip("Text for buzz state notifications")]
        [SerializeField] private TextMeshProUGUI buzzStateNotificationText;
        [Tooltip("Image for buzz state notification icon")]
        [SerializeField] private Image buzzStateNotificationIcon;
        
        [Header("Environmental Effects")]
        [Tooltip("Panel for environment notifications")]
        [SerializeField] private GameObject environmentNotificationPanel;
        [Tooltip("Text for environment notification header")]
        [SerializeField] private TextMeshProUGUI environmentNotificationTitle;
        [Tooltip("Text for environment notification description")]
        [SerializeField] private TextMeshProUGUI environmentNotificationDescription;
        [Tooltip("Image for environment notification icon")]
        [SerializeField] private Image environmentNotificationIcon;
        [Tooltip("Environmental effect overlay for tunnels")]
        [SerializeField] private GameObject tunnelOverlayEffect;
        [Tooltip("Environmental effect overlay for speakeasy")]
        [SerializeField] private GameObject speakeasyOverlayEffect;
        
        [Header("Animation Settings")]
        [Tooltip("Animation curve for UI transitions")]
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [Tooltip("Duration of UI transitions")]
        [SerializeField] private float transitionDuration = 0.5f;
        [Tooltip("Duration of notification display")]
        [SerializeField] private float notificationDuration = 3.0f;
        [Tooltip("Shake intensity for critical state")]
        [SerializeField] private float criticalStateShakeIntensity = 5.0f;
        #endregion
        
        #region Private Variables
        // Interface references
        private IBuzzSystem buzzSystem;
        private ICardSystem cardSystem;
        private IEnvironmentSystem environmentSystem;
        private CardEffectsController cardEffectsController;
        
        // Component cache
        private Canvas mainCanvas;
        private CanvasGroup mainCanvasGroup;
        
        // State tracking
        private IBuzzSystem.BuzzState currentBuzzState = IBuzzSystem.BuzzState.Normal;
        private int currentEnvironmentType = 0; // 0=streets, 1=tunnels, 2=speakeasy
        private float currentBuzzPercentage = 1.0f;
        private Dictionary<string, float> abilityCooldowns = new Dictionary<string, float>();
        
        // Card UI elements
        private List<GameObject> cardUIElements = new List<GameObject>();
        private int selectedCardIndex = -1;
        private Queue<GameObject> cardUIPool = new Queue<GameObject>();
        private const int INITIAL_POOL_SIZE = 10;
        // Animation coroutines
        private Coroutine buzzMeterCoroutine;
        private Coroutine buzzStateCoroutine;
        private Coroutine notificationCoroutine;
        private Coroutine environmentCoroutine;
        
        // Environment effects
        private Dictionary<int, GameObject> environmentOverlays = new Dictionary<int, GameObject>();
        
        // Period-appropriate styling elements
        private Vector2 buzzMeterStartScale;
        #endregion
        
        #region Unity Lifecycle Methods
        private void Awake()
        {
            try
            {
                // Cache required components
                mainCanvas = GetComponent<Canvas>();
                mainCanvasGroup = GetComponent<CanvasGroup>();

                if (mainCanvas == null || mainCanvasGroup == null)
                {
                    Debug.LogWarning("BuzzUIController: Missing required Canvas or CanvasGroup component");
                }
                
                // Find and cache references using interfaces
                buzzSystem = FindObjectOfType<GamblerCharacter>() as IBuzzSystem;
                cardSystem = FindObjectOfType<GamblerCharacter>() as ICardSystem;
                
                var mainController = FindObjectOfType<MainGameController>();
                environmentSystem = mainController as IEnvironmentSystem;
                if (environmentSystem == null && mainController != null)
                {
                    Debug.LogError("Found MainGameController but it doesn't implement IEnvironmentSystem");
                }
                
                cardEffectsController = FindObjectOfType<CardEffectsController>();
            
            if (buzzSystem == null)
            {
                Debug.LogError("BuzzUIController: Could not find IBuzzSystem implementation");
            }
                
            if (cardSystem == null)
            {
                Debug.LogError("BuzzUIController: Could not find ICardSystem implementation");
            }
                
            if (environmentSystem == null)
            {
                Debug.LogError("BuzzUIController: Could not find IEnvironmentSystem implementation");
            }
            
            // Initialize state
            // Initialize state
            buzzMeterStartScale = buzzMeterFrame ? buzzMeterFrame.rectTransform.localScale : Vector2.one;
            // Initialize environment overlays
            if (tunnelOverlayEffect != null)
            {
                environmentOverlays[1] = tunnelOverlayEffect;
            }
            
            if (speakeasyOverlayEffect != null)
            {
                environmentOverlays[2] = speakeasyOverlayEffect;
            }
            
            // Hide notifications initially
            if (buzzStateNotificationPanel)
            {
                buzzStateNotificationPanel.SetActive(false);
            }
            
            if (environmentNotificationPanel)
            {
                environmentNotificationPanel.SetActive(false);
            }
                
            // Initialize all environment effects to inactive
            foreach (var overlay in environmentOverlays.Values)
            {
                if (overlay != null)
                {
                    overlay.SetActive(false);
                }
            }
            InitializeCardPool();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"BuzzUIController: Error during initialization: {e.Message}\n{e.StackTrace}");
            }
        }
        
        /// <summary>
        /// Initializes the card UI object pool for better performance
        /// </summary>
        private void InitializeCardPool()
        {
            if (cardHandContainer == null || cardUIPrefab == null) return;
            
            try
            {
                // Create initial pool of card UI elements
                for (int i = 0; i < INITIAL_POOL_SIZE; i++)
                {
                    GameObject cardUI = Instantiate(cardUIPrefab, cardHandContainer);
                    cardUI.SetActive(false);
                    cardUIPool.Enqueue(cardUI);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"BuzzUIController: Error initializing card pool: {e.Message}");
            }
        }
        private void Start()
        {
            try
            {
                // Validate UI components
                ValidateUIComponents();
                
                // Subscribe to interface events
                if (buzzSystem != null)
                {
                    buzzSystem.OnBuzzChanged += UpdateBuzzMeter;
                    buzzSystem.OnBuzzStateChanged += HandleBuzzStateChanged;
                }
                
                if (cardSystem != null)
                {
                    cardSystem.OnCardUsed += HandleCardUsed;
                    cardSystem.OnCardDrawn += HandleCardDrawn;
                    cardSystem.OnHandChanged += UpdateCardHandUI;
                    cardSystem.OnAbilityUsed += HandleAbilityUsed;
                }
                
                if (environmentSystem != null)
                {
                    environmentSystem.OnEnvironmentChanged += HandleEnvironmentChanged;
                }
                
                if (buzzSystem == null || cardSystem == null || environmentSystem == null)
                {
                    Debug.LogWarning("BuzzUIController couldn't find required interfaces in scene");
                }
                
                // Initialize card UI
                ClearCardUI();
                if (cardSystem != null)
                {
                    UpdateCardHandUI(cardSystem.GetCurrentHand());
                }
                
                // Initialize cooldown indicators
                UpdateCooldownIndicators();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"BuzzUIController: Error during Start: {e.Message}");
            }
        }
        
        /// <summary>
        /// Validates that all required UI components are assigned
        /// </summary>
        private void ValidateUIComponents()
        {
            // Check critical UI components
            List<string> missingComponents = new List<string>();
            
            if (buzzMeterFill == null) missingComponents.Add("buzzMeterFill");
            if (buzzMeterFrame == null) missingComponents.Add("buzzMeterFrame");
            if (buzzStateIcon == null) missingComponents.Add("buzzStateIcon");
            if (cardHandContainer == null) missingComponents.Add("cardHandContainer");
            if (cardUIPrefab == null) missingComponents.Add("cardUIPrefab");
            
            // Log warning for missing components
            if (missingComponents.Count > 0)
            {
                Debug.LogWarning($"BuzzUIController: Missing UI components: {string.Join(", ", missingComponents)}");
            }
            
            // Verify card prefab has CardPrefab component
            if (cardUIPrefab != null && cardUIPrefab.GetComponent<CardPrefab>() == null)
            {
                Debug.LogError("BuzzUIController: Card UI prefab is missing CardPrefab component");
            }
        }
        
        private void Update()
        {
            try
            {
                // Update cooldown indicators
                UpdateCooldownIndicators();
                // Add period-appropriate subtle animation to the buzz meter
                if (buzzMeterFrame != null)
                {
                    float wobble = Mathf.Sin(Time.time * 2f) * 0.01f;
                    buzzMeterFrame.rectTransform.localScale = buzzMeterStartScale + new Vector2(wobble, wobble);
                }
                
                // If in critical state, add a subtle shake to the buzz meter
                if (currentBuzzState == IBuzzSystem.BuzzState.Critical && buzzMeterFrame != null)
                {
                    float shake = Mathf.Sin(Time.time * 12f) * criticalStateShakeIntensity * 0.01f;
                    buzzMeterFrame.rectTransform.anchoredPosition = new Vector2(shake, shake);
                }
            }
            catch (System.Exception e)
            {
                // Don't log errors every frame to prevent log spam
                if (Time.frameCount % 60 == 0) // Log once per second at 60fps
                {
                    Debug.LogWarning($"BuzzUIController: Error during Update: {e.Message}");
                }
            }
        }
        
        private void OnDestroy()
        {
            try
            {
                // Unsubscribe from events
                if (buzzSystem != null)
                {
                    buzzSystem.OnBuzzChanged -= UpdateBuzzMeter;
                    buzzSystem.OnBuzzStateChanged -= HandleBuzzStateChanged;
                }
                
                if (cardSystem != null)
                {
                    cardSystem.OnCardUsed -= HandleCardUsed;
                    cardSystem.OnCardDrawn -= HandleCardDrawn;
                    cardSystem.OnHandChanged -= UpdateCardHandUI;
                    cardSystem.OnAbilityUsed -= HandleAbilityUsed;
                }
                
                if (environmentSystem != null)
                {
                    environmentSystem.OnEnvironmentChanged -= HandleEnvironmentChanged;
                }
                
                // Stop all coroutines
                StopAllCoroutines();
                
                // Clean up card pool
                CleanupCardPool();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"BuzzUIController: Error during cleanup: {e.Message}");
            }
        }
        
        /// <summary>
        /// Cleans up the card UI object pool
        /// </summary>
        private void CleanupCardPool()
        {
            // Destroy all pooled objects
            while (cardUIPool.Count > 0)
            {
                GameObject cardUI = cardUIPool.Dequeue();
                if (cardUI != null)
                {
                    Destroy(cardUI);
                }
            }
        }
        
        #endregion
        
        #region Buzz UI Methods
        /// <summary>
        /// Updates the buzz meter display
        /// </summary>
        /// <param name="currentBuzz">Current buzz value</param>
        /// <param name="maxBuzz">Maximum buzz value</param>
        private void UpdateBuzzMeter(float currentBuzz, float maxBuzz)
        {
            // Calculate buzz percentage
            currentBuzzPercentage = Mathf.Clamp01(currentBuzz / maxBuzz);
            
            // Update fill amount with smooth animation
            if (buzzMeterFill != null)
            {
                if (buzzMeterCoroutine != null)
                {
                    StopCoroutine(buzzMeterCoroutine);
                }
                
                buzzMeterCoroutine = StartCoroutine(AnimateBuzzMeter(currentBuzzPercentage));
            }
            
            // Update text
            if (buzzValueText != null)
            {
                buzzValueText.text = $"{Mathf.RoundToInt(currentBuzz)}/{Mathf.RoundToInt(maxBuzz)}";
            }
        }
        
        /// <summary>
        /// Handles buzz state changes and updates UI accordingly
        /// </summary>
        /// <param name="newState">New buzz state</param>
        private void HandleBuzzStateChanged(IBuzzSystem.BuzzState newState)
        {
            IBuzzSystem.BuzzState oldState = currentBuzzState;
            currentBuzzState = newState;
            if (buzzMeterFill != null)
            {
                if (buzzStateCoroutine != null)
                {
                    StopCoroutine(buzzStateCoroutine);
                }
                
                Color targetColor = GetBuzzStateColor(newState);
                buzzStateCoroutine = StartCoroutine(AnimateBuzzStateChange(targetColor));
            }
            
            // Update state icon
            UpdateBuzzStateIcon(newState);
            
            // Show notification
            ShowBuzzStateNotification(oldState, newState);
            
            // Add special effects for critical state
            if (newState == IBuzzSystem.BuzzState.Critical)
            {
                StartCoroutine(PulseCriticalEffect());
            }
        }
        
        /// <summary>
        /// Updates the buzz state icon
        /// </summary>
        /// <param name="state">Current buzz state</param>
        private void UpdateBuzzStateIcon(IBuzzSystem.BuzzState state)
        {
            if (buzzStateIcon == null) return;
            
            // Set the appropriate icon
            switch (state)
            {
                case IBuzzSystem.BuzzState.Normal:
                    buzzStateIcon.sprite = normalBuzzIcon;
                    break;
                case IBuzzSystem.BuzzState.Low:
                    buzzStateIcon.sprite = lowBuzzIcon;
                    break;
                case IBuzzSystem.BuzzState.Critical:
                    buzzStateIcon.sprite = criticalBuzzIcon;
                    break;
            }
            StartCoroutine(PulseIconOnChange());
        }
        
        /// <summary>
        /// Shows a notification about buzz state change
        /// </summary>
        /// <param name="oldState">Previous buzz state</param>
        /// <param name="newState">New buzz state</param>
        private void ShowBuzzStateNotification(IBuzzSystem.BuzzState oldState, IBuzzSystem.BuzzState newState)
        {
            if (buzzStateNotificationPanel == null || buzzStateNotificationText == null) return;
            
            // Don't show notification for initial state
            if (oldState == newState) return;
            
            // Set notification text and icon
            string notificationText = "";
            Sprite notificationIcon = null;
            
            buzzStateNotificationPanel.SetActive(true);
            
            switch (newState)
            {
                case IBuzzSystem.BuzzState.Normal:
                    notificationText = "Buzz Level Normal\nFull accuracy and defense";
                    notificationIcon = normalBuzzIcon;
                    break;
                case IBuzzSystem.BuzzState.Low:
                    notificationText = "Buzz Level Low\nReduced accuracy and defense";
                    notificationIcon = lowBuzzIcon;
                    break;
                case IBuzzSystem.BuzzState.Critical:
                    notificationText = "Buzz Level Critical!\nSeverely reduced accuracy and defense";
                    notificationIcon = criticalBuzzIcon;
                    break;
            }
            
            // Apply the icon
            if (buzzStateNotificationIcon != null && notificationIcon != null)
            {
                buzzStateNotificationIcon.sprite = notificationIcon;
            }
            
            // Update notification text
            if (buzzStateNotificationText != null)
            {
                buzzStateNotificationText.text = notificationText;
            }
            
            // Show notification with animation
            if (notificationCoroutine != null)
            {
                StopCoroutine(notificationCoroutine);
            }
            
            notificationCoroutine = StartCoroutine(ShowNotificationPanel(buzzStateNotificationPanel));
        }
        
        /// <summary>
        /// Returns the color associated with a buzz state
        /// </summary>
        private Color GetBuzzStateColor(IBuzzSystem.BuzzState state)
        {
            switch (state)
            {
                case IBuzzSystem.BuzzState.Normal:
                    return normalBuzzColor;
                case IBuzzSystem.BuzzState.Low:
                    return lowBuzzColor;
                case IBuzzSystem.BuzzState.Critical:
                    return criticalBuzzColor;
                default:
                    return normalBuzzColor;
            }
        }
        #endregion
        
        #region Card UI Methods
        /// <summary>
        /// Updates the card hand UI with the current hand
        /// </summary>
        /// <param name="hand">Current hand of cards</param>
        private void UpdateCardHandUI(List<ICardSystem.Card> hand)
        {
            // Clear existing card UI
            ClearCardUI();
            
            // Create new card UI elements
            if (cardHandContainer != null && cardUIPrefab != null)
            {
                for (int i = 0; i < hand.Count; i++)
                {
                    CreateCardUI(hand[i], i);
                }
            }
        }
        
        /// <summary>
        /// Creates a card UI element for a card
        /// </summary>
        /// <param name="card">Card data</param>
        /// <param name="index">Index in hand</param>
        private void CreateCardUI(ICardSystem.Card card, int index)
        {
            if (cardHandContainer == null || cardUIPrefab == null) return;
            
            GameObject cardUI = null;
            
            try
            {
                // Get a card UI from the pool or create a new one
                if (cardUIPool.Count > 0)
                {
                    cardUI = cardUIPool.Dequeue();
                    cardUI.SetActive(true);
                    cardUI.transform.SetParent(cardHandContainer);
                }
                else
                {
                    // If pool is empty, instantiate a new one
                    cardUI = Instantiate(cardUIPrefab, cardHandContainer);
                }
            
                // Position the card (can adjust based on hand layout)
                RectTransform rectTransform = cardUI.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    // Calculate position based on index
                    float xOffset = index * 120f; // Spacing between cards
                    rectTransform.anchoredPosition = new Vector2(xOffset, 0);
                }
                
                // Setup card data using CardPrefab component
                CardPrefab cardPrefabComponent = cardUI.GetComponent<CardPrefab>();
                if (cardPrefabComponent != null)
                {
                    cardPrefabComponent.SetupCard(card);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"BuzzUIController: Error creating card UI: {e.Message}");
                return;
            }
            
            try
            {
                // Add to list of card UI elements
                cardUIElements.Add(cardUI);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"BuzzUIController: Error adding card to UI elements: {e.Message}");
            }
        }

        /// <summary>
        /// Clears all card UI elements
        /// </summary>
        private void ClearCardUI()
        {
            try
            {
                // Return all card UI elements to the pool instead of destroying them
                foreach (GameObject cardUI in cardUIElements)
                {
                    if (cardUI != null)
                    {
                        cardUI.SetActive(false);
                        cardUIPool.Enqueue(cardUI);
                    }
                }
                
                // Clear the list
                cardUIElements.Clear();
                
                // Reset selection
                selectedCardIndex = -1;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"BuzzUIController: Error clearing card UI: {e.Message}");
            }
        }

        #endregion
        #region Environment UI Methods
        /// <summary>
        /// Handles environment changes and updates UI accordingly
        /// </summary>
        /// <param name="newType">New environment type</param>
        /// <param name="environmentName">Name of the new environment</param>
        private void HandleEnvironmentChanged(IEnvironmentSystem.EnvironmentType newType, string environmentName)
        {
            try
            {
                // Update environment type
                currentEnvironmentType = (int)newType;
                
                // Update environment overlays
                foreach (var overlay in environmentOverlays)
                {
                    if (overlay.Value != null)
                    {
                        overlay.Value.SetActive(overlay.Key == currentEnvironmentType);
                    }
                }
                
                // Show environment notification
                if (environmentNotificationPanel != null && environmentSystem != null)
                {
                    // Set notification text
                    if (environmentNotificationTitle != null)
                    {
                        environmentNotificationTitle.text = environmentName;
                    }
                    
                    if (environmentNotificationDescription != null)
                    {
                        environmentNotificationDescription.text = environmentSystem.GetEnvironmentDescription(currentEnvironmentType);
                    }
                    
                    // Set notification icon
                    if (environmentNotificationIcon != null)
                    {
                        environmentNotificationIcon.sprite = environmentSystem.GetEnvironmentIcon(currentEnvironmentType);
                    }
                    
                    // Show notification with animation
                    if (environmentCoroutine != null)
                    {
                        StopCoroutine(environmentCoroutine);
                    }
                    
                    environmentCoroutine = StartCoroutine(ShowNotificationPanel(environmentNotificationPanel));
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"BuzzUIController: Error handling environment change: {e.Message}");
            }
        }

        /// <summary>
        /// Animates the buzz meter fill amount
        /// </summary>
        /// <param name="targetFill">Target fill amount</param>
        /// <returns>Coroutine enumerator</returns>
        private IEnumerator AnimateBuzzMeter(float targetFill)
        {
            if (buzzMeterFill == null) yield break;
            
            float startFill = buzzMeterFill.fillAmount;
            float elapsed = 0f;
            
            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = transitionCurve.Evaluate(elapsed / transitionDuration);
                buzzMeterFill.fillAmount = Mathf.Lerp(startFill, targetFill, t);
                yield return null;
            }
            
            // Ensure we reach the target value
            buzzMeterFill.fillAmount = targetFill;
        }
        
        /// <summary>
        /// Animates the buzz meter color change
        /// </summary>
        /// <param name="targetColor">Target color</param>
        /// <returns>Coroutine enumerator</returns>
        private IEnumerator AnimateBuzzStateChange(Color targetColor)
        {
            if (buzzMeterFill == null) yield break;
            
            Color startColor = buzzMeterFill.color;
            float elapsed = 0f;
            
            while (elapsed < transitionDuration)
            {
                try
                {
                    elapsed += Time.deltaTime;
                    float t = transitionCurve.Evaluate(elapsed / transitionDuration);
                    buzzMeterFill.color = Color.Lerp(startColor, targetColor, t);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"BuzzUIController: Error animating buzz state change: {e.Message}");
                    break;
                }
                yield return null;
            }
            
            // Ensure final color is set even if there's an error
            buzzMeterFill.color = targetColor;
        }
        
        /// <summary>
        /// Creates a pulsing effect for critical buzz state
        /// </summary>
        /// <returns>Coroutine enumerator</returns>
        private IEnumerator PulseCriticalEffect()
        {
            if (buzzMeterFrame == null) yield break;
            
            // Original values
            Vector3 originalScale = buzzMeterFrame.transform.localScale;
            Color originalColor = buzzMeterFill ? buzzMeterFill.color : Color.white;
            
            // Number of pulses
            int pulseCount = 5;
            
            for (int i = 0; i < pulseCount; i++)
            {
                // Pulse outward
                float elapsed = 0f;
                while (elapsed < 0.2f)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Sin(elapsed * Mathf.PI * 2.5f);
                    float scaleFactor = 1f + t * 0.1f;
                    buzzMeterFrame.transform.localScale = originalScale * scaleFactor;
                    
                    // Flash color if we have a fill
                    if (buzzMeterFill != null)
                    {
                        buzzMeterFill.color = Color.Lerp(originalColor, Color.white, t * 0.5f);
                    }
                    
                    yield return null;
                }
                
                yield return new WaitForSeconds(0.1f);
            }
            
            // Restore original values
            buzzMeterFrame.transform.localScale = originalScale;
            if (buzzMeterFill != null)
            {
                buzzMeterFill.color = originalColor;
            }
        }
        
        /// <summary>
        /// Creates a pulsing effect for the buzz state icon when it changes
        /// </summary>
        /// <returns>Coroutine enumerator</returns>
        private IEnumerator PulseIconOnChange()
        {
            if (buzzStateIcon == null) yield break;
            
            // Original values
            Vector3 originalScale = buzzStateIcon.transform.localScale;
            
            // Pulse outward
            float elapsed = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                float t = transitionCurve.Evaluate(elapsed / 0.3f);
                // Scale up then back down
                float scaleFactor = 1f + Mathf.Sin(t * Mathf.PI) * 0.3f;
                buzzStateIcon.transform.localScale = originalScale * scaleFactor;
                
                // Add slight rotation for old-timey wobble effect
                float wobble = Mathf.Sin(elapsed * 20f) * 5f; // 5 degrees max wobble
                buzzStateIcon.transform.rotation = Quaternion.Euler(0, 0, wobble);
                
                yield return null;
            }
            
            // Restore original values
            buzzStateIcon.transform.localScale = originalScale;
            buzzStateIcon.transform.rotation = Quaternion.identity;
        }
        
        /// <summary>
        /// Shows a notification panel with animation
        /// </summary>
        /// <param name="panel">Panel to show</param>
        /// <returns>Coroutine enumerator</returns>
        private IEnumerator ShowNotificationPanel(GameObject panel)
        {
            if (panel == null) yield break;
            
            // Get panel components
            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = panel.AddComponent<CanvasGroup>();
            }
            
            RectTransform rectTransform = panel.GetComponent<RectTransform>();
            
            // Setup initial state
            panel.SetActive(true);
            canvasGroup.alpha = 0f;
            
            // Original position
            Vector2 originalPosition = rectTransform ? rectTransform.anchoredPosition : Vector2.zero;
            Vector2 startPosition = originalPosition + new Vector2(0, -50f); // Start below
            
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = startPosition;
            }
            
            // Fade in and move up
            float elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = transitionCurve.Evaluate(elapsed / transitionDuration);
                
                canvasGroup.alpha = t;
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = Vector2.Lerp(startPosition, originalPosition, t);
                }
                
                yield return null;
            }
            
            // Ensure final state
            canvasGroup.alpha = 1f;
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = originalPosition;
            }
            
            // Wait for display duration
            yield return new WaitForSeconds(notificationDuration);
            
            // Fade out
            elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = transitionCurve.Evaluate(elapsed / transitionDuration);
                
                canvasGroup.alpha = 1f - t;
                
                yield return null;
            }
            
            // Hide panel
            panel.SetActive(false);
        }
        #endregion
        
        #region Card Interaction Methods
        /// <summary>
        /// Selects a card in the hand
        /// </summary>
        /// <param name="index">Index of card to select</param>
        public void SelectCard(int index)
        {
            if (index < 0 || index >= cardUIElements.Count) return;
            
            // Update selected index
            selectedCardIndex = index;
            
            // Move selection indicator if available
            if (cardSelectionIndicator != null && index >= 0 && index < cardUIElements.Count)
            {
                cardSelectionIndicator.gameObject.SetActive(true);
                cardSelectionIndicator.transform.position = cardUIElements[index].transform.position;
            }
            else if (cardSelectionIndicator != null)
            {
                cardSelectionIndicator.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Handles when a card is used
        /// </summary>
        /// <param name="card">The card that was used</param>
        private void HandleCardUsed(ICardSystem.Card card)
        {
            // Play card use animation/effect if available
            if (cardEffectsController != null)
            {
                cardEffectsController.PlayCardEffect(card, transform.position);
            }
            // Deselect card
            SelectCard(-1);
        }
        
        /// <summary>
        /// Handles when a card is drawn
        /// </summary>
        /// <param name="card">The card that was drawn</param>
        private void HandleCardDrawn(ICardSystem.Card card)
        {
            // Play card draw animation/effect if available
            if (cardEffectsController != null)
            {
                cardEffectsController.PlayCardDrawEffect(transform.position);
            }
        }
        
        
        /// <summary>
        /// Handles when an ability is used
        /// </summary>
        /// <param name="abilityName">Name of the ability</param>
        /// <param name="cooldown">Cooldown duration</param>
        private void HandleAbilityUsed(string abilityName, float cooldown)
        {
            // Store cooldown
            abilityCooldowns[abilityName] = cooldown;
        }
        
        /// <summary>
        /// Updates cooldown indicators for abilities
        /// </summary>
        private void UpdateCooldownIndicators()
        {
            UpdateAbilityCooldown("Slice", sliceCooldownIndicator);
            UpdateAbilityCooldown("Flick", flickCooldownIndicator);
            UpdateAbilityCooldown("DrawCard", drawCardCooldownIndicator);
        }
        
        /// <summary>
        /// Updates a single ability cooldown indicator
        /// </summary>
        /// <param name="abilityName">Name of the ability</param>
        /// <param name="indicator">Cooldown indicator image</param>
        private void UpdateAbilityCooldown(string abilityName, Image indicator)
        {
            if (indicator == null) return;
            
            // Check if ability is on cooldown
            if (abilityCooldowns.TryGetValue(abilityName, out float cooldown) && cooldown > 0)
            {
                // Update cooldown
                abilityCooldowns[abilityName] = Mathf.Max(0, cooldown - Time.deltaTime);
                
                // Update cooldown indicator
                indicator.fillAmount = abilityCooldowns[abilityName] / cooldown;
                
                // Show indicator
                indicator.gameObject.SetActive(true);
            }
            else
            {
                // Hide indicator
                indicator.gameObject.SetActive(false);
            }
        }
        #endregion
        
    } // end class BuzzUIController
} // end namespace PDXUnderground.UI
