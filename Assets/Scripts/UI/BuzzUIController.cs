using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PDXUnderground
{
    /// <summary>
    /// Controls the UI elements related to the Gambler's Buzz system.
    /// Provides visual feedback for buzz levels, card cooldowns, and ability states
    /// with period-appropriate styling for 1800s Portland.
    /// </summary>
    public class BuzzUIController : MonoBehaviour
    {
        #region Inspector Properties
        [Header("Buzz Meter Components")]
        [Tooltip("Image used for the buzz meter fill")]
        [SerializeField] private Image buzzMeterFill;
        [Tooltip("Text displaying current buzz value")]
        [SerializeField] private TextMeshProUGUI buzzValueText;
        [Tooltip("Background frame for buzz meter")]
        [SerializeField] private Image buzzMeterFrame;
        [Tooltip("Icon indicator for current buzz state")]
        [SerializeField] private Image buzzStateIcon;
        
        [Header("Buzz State Visuals")]
        [Tooltip("Color for normal buzz state")]
        [SerializeField] private Color normalBuzzColor = new Color(0.8f, 0.7f, 0.2f, 1f); // Gold
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
        // References
        private GamblerCharacter gamblerCharacter;
        private CardEffectsController cardEffectsController;
        
        // State tracking
        private GamblerCharacter.BuzzState currentBuzzState = GamblerCharacter.BuzzState.Normal;
        private int currentEnvironmentType = 0; // 0=streets, 1=tunnels, 2=speakeasy
        private float currentBuzzPercentage = 1.0f;
        private Dictionary<string, float> abilityCooldowns = new Dictionary<string, float>();
        
        // Card UI elements
        private List<GameObject> cardUIElements = new List<GameObject>();
        private int selectedCardIndex = -1;
        
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
            // Find and cache references
            gamblerCharacter = FindObjectOfType<GamblerCharacter>();
            cardEffectsController = FindObjectOfType<CardEffectsController>();
            
            // Initialize state
            buzzMeterStartScale = buzzMeterFrame ? buzzMeterFrame.rectTransform.localScale : Vector2.one;
            
            // Initialize environment overlays
            if (tunnelOverlayEffect != null) 
                environmentOverlays[1] = tunnelOverlayEffect;
            if (speakeasyOverlayEffect != null) 
                environmentOverlays[2] = speakeasyOverlayEffect;
            
            // Hide notifications initially
            if (buzzStateNotificationPanel) 
                buzzStateNotificationPanel.SetActive(false);
            if (environmentNotificationPanel) 
                environmentNotificationPanel.SetActive(false);
                
            // Initialize all environment effects to inactive
            foreach (var overlay in environmentOverlays.Values)
            {
                if (overlay != null)
                    overlay.SetActive(false);
            }
        }
        
        private void Start()
        {
            // Subscribe to GamblerCharacter events
            if (gamblerCharacter != null)
            {
                gamblerCharacter.OnBuzzChanged += UpdateBuzzMeter;
                gamblerCharacter.OnBuzzStateChanged += HandleBuzzStateChanged;
                gamblerCharacter.OnCardUsed += HandleCardUsed;
                gamblerCharacter.OnCardDrawn += HandleCardDrawn;
                gamblerCharacter.OnHandChanged += UpdateCardHandUI;
                gamblerCharacter.OnAbilityUsed += HandleAbilityUsed;
            }
            else
            {
                Debug.LogWarning("BuzzUIController couldn't find GamblerCharacter in scene");
            }
            
            // Initialize card UI
            ClearCardUI();
            if (gamblerCharacter != null)
            {
                UpdateCardHandUI(gamblerCharacter.GetCurrentHand());
            }
            
            // Initialize cooldown indicators
            UpdateCooldownIndicators();
        }
        
        private void Update()
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
            if (currentBuzzState == GamblerCharacter.BuzzState.Critical && buzzMeterFrame != null)
            {
                float shake = Mathf.Sin(Time.time * 12f) * criticalStateShakeIntensity * 0.01f;
                buzzMeterFrame.rectTransform.anchoredPosition = new Vector2(shake, shake);
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (gamblerCharacter != null)
            {
                gamblerCharacter.OnBuzzChanged -= UpdateBuzzMeter;
                gamblerCharacter.OnBuzzStateChanged -= HandleBuzzStateChanged;
                gamblerCharacter.OnCardUsed -= HandleCardUsed;
                gamblerCharacter.OnCardDrawn -= HandleCardDrawn;
                gamblerCharacter.OnHandChanged -= UpdateCardHandUI;
                gamblerCharacter.OnAbilityUsed -= HandleAbilityUsed;
            }
            
            // Stop all coroutines
            StopAllCoroutines();
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
        private void HandleBuzzStateChanged(GamblerCharacter.BuzzState newState)
        {
            // Update state tracking
            GamblerCharacter.BuzzState oldState = currentBuzzState;
            currentBuzzState = newState;
            
            // Update buzz meter color
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
            if (newState == GamblerCharacter.BuzzState.Critical)
            {
                StartCoroutine(PulseCriticalEffect());
            }
        }
        
        /// <summary>
        /// Updates the buzz state icon
        /// </summary>
        /// <param name="state">Current buzz state</param>
        private void UpdateBuzzStateIcon(GamblerCharacter.BuzzState state)
        {
            if (buzzStateIcon == null) return;
            
            // Set the appropriate icon
            switch (state)
            {
                case GamblerCharacter.BuzzState.Normal:
                    buzzStateIcon.sprite = normalBuzzIcon;
                    break;
                case GamblerCharacter.BuzzState.Low:
                    buzzStateIcon.sprite = lowBuzzIcon;
                    break;
                case GamblerCharacter.BuzzState.Critical:
                    buzzStateIcon.sprite = criticalBuzzIcon;
                    break;
            }
            
            // Animate the icon change
            StartCoroutine(PulseIconOnChange());
        }
        
        /// <summary>
        /// Shows a notification about buzz state change
        /// </summary>
        /// <param name="oldState">Previous buzz state</param>
        /// <param name="newState">New buzz state</param>
        private void ShowBuzzStateNotification(GamblerCharacter.BuzzState oldState, GamblerCharacter.BuzzState newState)
        {
            if (buzzStateNotificationPanel == null || buzzStateNotificationText == null) return;
            
            // Don't show notification for initial state
            if (oldState == newState) return;
            
            // Set notification text and icon
            string notificationText = "";
            Sprite notificationIcon = null;
            
            switch (newState)
            {
                case GamblerCharacter.BuzzState.Normal:
                    notificationText = "Buzz Level Normal\nFull accuracy and defense";
                    notificationIcon = normalBuzzIcon;
                    break;
                case GamblerCharacter.BuzzState.Low:
                    notificationText = "Buzz Level Low\nReduced accuracy and defense";
                    notificationIcon = lowBuzzIcon;
                    break;
                case GamblerCharacter.BuzzState.Critical:
                    notificationText = "Buzz Level Critical!\nSeverely reduced accuracy and defense";
                    notificationIcon = criticalBuzzIcon;
                    break;
            }
            
            buzzStateNotificationText.text = notificationText;
            if (buzzStateNotificationIcon != null && notificationIcon != null)
            {
                buzzStateNotificationIcon.sprite = notificationIcon;
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
        private Color GetBuzzStateColor(GamblerCharacter.BuzzState state)
        {
            switch (state)
            {
                case GamblerCharacter.BuzzState.Normal:
                    return normalBuzzColor;
                case GamblerCharacter.BuzzState.Low:
                    return lowBuzzColor;
                case GamblerCharacter.BuzzState.Critical:
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
        private void UpdateCardHandUI(List<Card> hand)
        {
            // Clear existing card UI
            ClearCardUI();
            
            // Create new card UI elements
            if (car

