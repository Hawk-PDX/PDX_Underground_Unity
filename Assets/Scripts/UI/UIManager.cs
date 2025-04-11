using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// UIManager handles all UI-related functionality in PDX Underground,
/// including panels, fading, prompts, and debug information.
/// </summary>
public class UIManager : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Panel References")]
    [SerializeField] private CanvasGroup fadePanel;
    [SerializeField] private CanvasGroup interactionPromptPanel;
    [SerializeField] private CanvasGroup areaNamePanel;
    [SerializeField] private CanvasGroup loadingPanel;
    [SerializeField] private CanvasGroup debugPanel;
    
    [Header("Text References")]
    [SerializeField] private TextMeshProUGUI interactionPromptText;
    [SerializeField] private TextMeshProUGUI areaNameText;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI weatherText;
    [SerializeField] private TextMeshProUGUI locationText;
    [SerializeField] private TextMeshProUGUI fpsText;
    
    [Header("Loading Screen")]
    [SerializeField] private Slider loadingBar;
    [SerializeField] private TextMeshProUGUI loadingProgressText;
    
    [Header("Transition Settings")]
    [SerializeField] private float fadeSpeed = 1.5f;
    [SerializeField] private float promptFadeSpeed = 3.0f;
    [SerializeField] private float areaNameDisplayTime = 3.0f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Debug Settings")]
    [SerializeField] private bool showFpsCounter = true;
    [SerializeField] private bool showDebugInfo = false;
    [SerializeField] private KeyCode toggleDebugKey = KeyCode.F3;
    [SerializeField] private float debugUpdateInterval = 0.5f;
    
    #endregion
    
    #region Private Variables
    
    private float lastFpsUpdateTime;
    private int frameCount;
    private float totalFps;
    private Coroutine areaNameCoroutine;
    private Coroutine promptCoroutine;
    private Coroutine fadeCoroutine;
    private GameEnvironmentController environmentController;
    private Camera mainCamera;
    private GameObject promptWorldSpaceIndicator;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        // Initialize all panels
        InitializePanels();
        
        // Create world space prompt indicator if needed
        CreatePromptIndicator();
        
        // Get references
        environmentController = FindObjectOfType<GameEnvironmentController>();
        mainCamera = Camera.main;
    }
    
    private void Start()
    {
        // Set initial UI state
        fadePanel.alpha = 0f; // Start with no fade
        fadePanel.blocksRaycasts = false;
        
        HideInteractionPrompt();
        HideAreaName();
        HideLoadingScreen();
        
        // Set debug panel based on initial setting
        SetDebugInfoVisibility(showDebugInfo);
        
        // Initialize FPS counter
        lastFpsUpdateTime = Time.unscaledTime;
        frameCount = 0;
        totalFps = 0;
    }
    
    private void Update()
    {
        // Toggle debug panel with hotkey
        if (Input.GetKeyDown(toggleDebugKey))
        {
            showDebugInfo = !showDebugInfo;
            SetDebugInfoVisibility(showDebugInfo);
        }
        
        // Update FPS counter
        if (showFpsCounter)
        {
            UpdateFpsCounter();
        }
        
        // Update debug info at intervals
        if (showDebugInfo && Time.unscaledTime > lastFpsUpdateTime + debugUpdateInterval)
        {
            UpdateDebugInfo();
            lastFpsUpdateTime = Time.unscaledTime;
        }
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Fades the screen to black
    /// </summary>
    public IEnumerator FadeToBlack()
    {
        // Stop any existing fade
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        // New fade to black
        fadeCoroutine = StartCoroutine(FadeCanvasGroup(fadePanel, 0f, 1f, fadeSpeed));
        yield return fadeCoroutine;
        
        // Block raycasts when fully faded
        fadePanel.blocksRaycasts = true;
    }
    
    /// <summary>
    /// Fades the screen from black back to normal
    /// </summary>
    public IEnumerator FadeFromBlack()
    {
        // Stop any existing fade
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        // Don't block raycasts during fade-in
        fadePanel.blocksRaycasts = false;
        
        // New fade from black
        fadeCoroutine = StartCoroutine(FadeCanvasGroup(fadePanel, 1f, 0f, fadeSpeed));
        yield return fadeCoroutine;
    }
    
    /// <summary>
    /// Shows the interaction prompt
    /// </summary>
    public void ShowInteractionPrompt(string promptText, Vector3 worldPosition = default)
    {
        // Set the prompt text
        if (interactionPromptText != null)
        {
            interactionPromptText.text = promptText;
        }
        
        // Update prompt position if world position is provided
        if (worldPosition != default && promptWorldSpaceIndicator != null && mainCamera != null)
        {
            // Update position
            promptWorldSpaceIndicator.transform.position = worldPosition;
            promptWorldSpaceIndicator.SetActive(true);
            
            // Position the prompt panel relative to the world position
            if (interactionPromptPanel != null)
            {
                // Convert world position to screen position
                Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPosition);
                
                // Position prompt panel above the screen position
                RectTransform panelRect = interactionPromptPanel.GetComponent<RectTransform>();
                if (panelRect != null)
                {
                    panelRect.position = new Vector3(screenPos.x, screenPos.y + 50f, screenPos.z);
                }
            }
        }
        else
        {
            // Default to center position
            RectTransform panelRect = interactionPromptPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.anchoredPosition = Vector2.zero;
            }
            
            if (promptWorldSpaceIndicator != null)
            {
                promptWorldSpaceIndicator.SetActive(false);
            }
        }
        
        // Show the prompt panel
        if (promptCoroutine != null)
        {
            StopCoroutine(promptCoroutine);
        }
        
        promptCoroutine = StartCoroutine(FadeCanvasGroup(interactionPromptPanel, 0f, 1f, promptFadeSpeed));
    }
    
    /// <summary>
    /// Hides the interaction prompt
    /// </summary>
    public void HideInteractionPrompt()
    {
        if (interactionPromptPanel != null)
        {
            if (promptCoroutine != null)
            {
                StopCoroutine(promptCoroutine);
            }
            
            promptCoroutine = StartCoroutine(FadeCanvasGroup(interactionPromptPanel, interactionPromptPanel.alpha, 0f, promptFadeSpeed));
        }
        
        // Hide the world space indicator
        if (promptWorldSpaceIndicator != null)
        {
            promptWorldSpaceIndicator.SetActive(false);
        }
    }
    
    /// <summary>
    /// Shows the area name
    /// </summary>
    public void ShowAreaName(string areaName)
    {
        // Set the area name text
        if (areaNameText != null)
        {
            areaNameText.text = areaName;
        }
        
        // Stop any existing coroutine
        if (areaNameCoroutine != null)
        {
            StopCoroutine(areaNameCoroutine);
        }
        
        // Start new display sequence
        areaNameCoroutine = StartCoroutine(ShowAreaNameSequence());
    }
    
    /// <summary>
    /// Hides the area name
    /// </summary>
    public void HideAreaName()
    {
        if (areaNamePanel != null)
        {
            if (areaNameCoroutine != null)
            {
                StopCoroutine(areaNameCoroutine);
            }
            
            areaNameCoroutine = StartCoroutine(FadeCanvasGroup(areaNamePanel, areaNamePanel.alpha, 0f, fadeSpeed));
        }
    }
    
    /// <summary>
    /// Shows the loading screen
    /// </summary>
    public void ShowLoadingScreen()
    {
        if (loadingPanel != null)
        {
            loadingPanel.alpha = 1f;
            loadingPanel.blocksRaycasts = true;
            loadingPanel.gameObject.SetActive(true);
            
            // Reset loading bar
            if (loadingBar != null)
            {
                loadingBar.value = 0f;
            }
            
            // Set initial loading text
            if (loadingText != null)
            {
                loadingText.text = "Loading...";
            }
            
            if (loadingProgressText != null)
            {
                loadingProgressText.text = "0%";
            }
        }
    }
    
    /// <summary>
    /// Hides the loading screen
    /// </summary>
    public void HideLoadingScreen()
    {
        if (loadingPanel != null)
        {
            loadingPanel.alpha = 0f;
            loadingPanel.blocksRaycasts = false;
            loadingPanel.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Updates the loading progress
    /// </summary>
    public void UpdateLoadingProgress(float progress)
    {
        // Clamp progress between 0 and 1
        progress = Mathf.Clamp01(progress);
        
        // Update loading bar
        if (loadingBar != null)
        {
            loadingBar.value = progress;
        }
        
        // Update progress text
        if (loadingProgressText != null)
        {
            loadingProgressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
        }
    }
    
    /// <summary>
    /// Toggles the visibility of the debug panel
    /// </summary>
    public void SetDebugInfoVisibility(bool visible)
    {
        showDebugInfo = visible;
        
        if (debugPanel != null)
        {
            debugPanel.alpha = visible ? 1f : 0f;
            debugPanel.blocksRaycasts = visible;
            debugPanel.gameObject.SetActive(visible);
        }
        
        // Update debug info immediately if enabled
        if (visible)
        {
            UpdateDebugInfo();
        }
    }
    
    #endregion
    
    #region Private Methods
    
    /// <summary>
    /// Initializes UI panels
    /// </summary>
    private void InitializePanels()
    {
        // Make sure all panels are valid
        if (fadePanel == null)
        {
            Debug.LogError("Fade Panel not assigned in UIManager!");
        }
        
        if (interactionPromptPanel == null)
        {
            Debug.LogError("Interaction Prompt Panel not assigned in UIManager!");
        }
        
        if (areaNamePanel == null)
        {
            Debug.LogError("Area Name Panel not assigned in UIManager!");
        }
        
        if (loadingPanel == null)
        {
            Debug.LogError("Loading Panel not assigned in UIManager!");
        }
        
        if (debugPanel == null)
        {
            Debug.LogError("Debug Panel not assigned in UIManager!");
        }
    }
    
    /// <summary>
    /// Creates a world space indicator for prompts
    /// </summary>
    private void CreatePromptIndicator()
    {
        if (promptWorldSpaceIndicator == null)
        {
            promptWorldSpaceIndicator = new GameObject("PromptIndicator");
            promptWorldSpaceIndicator.SetActive(false);
        }
    }
    
    /// <summary>
    /// Fades a canvas group between alpha values
    /// </summary>
    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        if (canvasGroup == null)
            yield break;
            
        // Show the canvas group
        canvasGroup.gameObject.SetActive(true);
        
        // Set initial alpha
        canvasGroup.alpha = startAlpha;
        
        // Handle instant transitions
        if (duration <= 0f)
        {
            canvasGroup.alpha = endAlpha;
            
            // If fading out completely, deactivate the gameObject
            if (endAlpha <= 0f)
            {
                canvasGroup.gameObject.SetActive(false);
            }
            
            yield break;
        }
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / duration);
            
            // Get current alpha using animation curve
            float curveValue = fadeCurve.Evaluate(normalizedTime);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, curveValue);
            
            yield return null;
        }
        
        // Ensure final alpha is set
        canvasGroup.alpha = endAlpha;
        
        // If fading out completely, deactivate the gameObject
        if (endAlpha <= 0f)
        {
            canvasGroup.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Shows the area name with a fade in/out sequence
    /// </summary>
    private IEnumerator ShowAreaNameSequence()
    {
        if (areaNamePanel == null)
            yield break;
            
        // Show and fade in the area name
        areaNamePanel.gameObject.SetActive(true);
        yield return StartCoroutine(FadeCanvasGroup(areaNamePanel, 0f, 1f, fadeSpeed));
        
        // Wait for display duration
        yield return new WaitForSeconds(areaNameDisplayTime);
        
        // Fade out and hide the area name
        yield return StartCoroutine(FadeCanvasGroup(areaNamePanel, 1f, 0f, fadeSpeed));
        areaNamePanel.gameObject.SetActive

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UIManager handles all UI-related functionality for PDX Underground,
/// including transitions, prompts, debug info, and loading screens.
/// </summary>
public class UIManager : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Main Canvas References")]
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private CanvasGroup mainCanvasGroup;
    
    [Header("Fade Transition")]
    [SerializeField] private CanvasGroup fadePanel;
    [SerializeField] private float fadeSpeed = 1.5f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Interaction Prompts")]
    [SerializeField] private CanvasGroup interactionPromptPanel;
    [SerializeField] private TextMeshProUGUI interactionPromptText;
    [SerializeField] private float promptFadeSpeed = 3.0f;
    [SerializeField] private bool useWorldSpacePrompts = false;
    [SerializeField] private GameObject worldSpacePromptPrefab;
    
    [Header("Area Transitions")]
    [SerializeField] private CanvasGroup areaNamePanel;
    [SerializeField] private TextMeshProUGUI areaNameText;
    [SerializeField] private float areaNameDisplayTime = 3.0f;
    [SerializeField] private float areaNameFadeSpeed = 2.0f;
    
    [Header("Loading Screen")]
    [SerializeField] private CanvasGroup loadingPanel;
    [SerializeField] private Slider loadingProgressBar;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private string[] loadingTips;
    
    [Header("Debug Information")]
    [SerializeField] private CanvasGroup debugInfoPanel;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI weatherText;
    [SerializeField] private TextMeshProUGUI locationText;
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private bool showDebugInfoOnStart = false;
    [SerializeField] private float debugUpdateInterval = 0.5f;
    
    #endregion
    
    #region Private Variables
    
    private Coroutine fadeCoroutine;
    private Coroutine areaNameCoroutine;
    private Coroutine interactionPromptCoroutine;
    private Coroutine loadingScreenCoroutine;
    private Coroutine debugUpdateCoroutine;
    private GameObject currentWorldPrompt;
    private bool isTransitioning = false;
    private float deltaTime = 0f;
    private GameEnvironmentController environmentController;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        // Initialize canvas if not set
        if (mainCanvas == null)
        {
            mainCanvas = GetComponent<Canvas>();
        }
        
        if (mainCanvasGroup == null && mainCanvas != null)
        {
            mainCanvasGroup = mainCanvas.GetComponent<CanvasGroup>();
            if (mainCanvasGroup == null)
            {
                mainCanvasGroup = mainCanvas.gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        // Initialize panels if not already set
        InitializeCanvasElements();
        
        // Get references to other managers
        environmentController = FindObjectOfType<GameEnvironmentController>();
    }
    
    private void Start()
    {
        // Initialize UI state
        if (fadePanel != null)
        {
            fadePanel.alpha = 0f;
            fadePanel.blocksRaycasts = false;
        }
        
        if (interactionPromptPanel != null)
        {
            interactionPromptPanel.alpha = 0f;
            interactionPromptPanel.blocksRaycasts = false;
        }
        
        if (areaNamePanel != null)
        {
            areaNamePanel.alpha = 0f;
            areaNamePanel.blocksRaycasts = false;
        }
        
        if (loadingPanel != null)
        {
            loadingPanel.alpha = 0f;
            loadingPanel.blocksRaycasts = false;
        }
        
        // Set debug info visibility
        SetDebugInfoVisibility(showDebugInfoOnStart);
        
        if (showDebugInfoOnStart)
        {
            StartDebugInfoUpdates();
        }
    }
    
    private void Update()
    {
        // Update FPS counter for debug info
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Fades the screen to black
    /// </summary>
    public IEnumerator FadeToBlack()
    {
        if (fadePanel == null)
        {
            Debug.LogWarning("Fade panel not assigned!");
            yield break;
        }
        
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        isTransitioning = true;
        
        // Ensure fade panel is active
        fadePanel.gameObject.SetActive(true);
        fadePanel.blocksRaycasts = true;
        
        float elapsedTime = 0f;
        float startAlpha = fadePanel.alpha;
        
        while (elapsedTime < fadeSpeed)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / fadeSpeed);
            float curveValue = fadeCurve.Evaluate(normalizedTime);
            
            fadePanel.alpha = Mathf.Lerp(startAlpha, 1f, curveValue);
            
            yield return null;
        }
        
        fadePanel.alpha = 1f;
        
        fadeCoroutine = null;
    }
    
    /// <summary>
    /// Fades the screen from black to clear
    /// </summary>
    public IEnumerator FadeFromBlack()
    {
        if (fadePanel == null)
        {
            Debug.LogWarning("Fade panel not assigned!");
            yield break;
        }
        
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        // Ensure fade panel is active
        fadePanel.gameObject.SetActive(true);
        
        float elapsedTime = 0f;
        float startAlpha = fadePanel.alpha;
        
        while (elapsedTime < fadeSpeed)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / fadeSpeed);
            float curveValue = fadeCurve.Evaluate(normalizedTime);
            
            fadePanel.alpha = Mathf.Lerp(startAlpha, 0f, curveValue);
            
            yield return null;
        }
        
        fadePanel.alpha = 0f;
        fadePanel.blocksRaycasts = false;
        
        isTransitioning = false;
        fadeCoroutine = null;
    }
    
    /// <summary>
    /// Shows the loading screen
    /// </summary>
    public void ShowLoadingScreen()
    {
        if (loadingPanel == null)
        {
            Debug.LogWarning("Loading panel not assigned!");
            return;
        }
        
        loadingPanel.gameObject.SetActive(true);
        
        if (loadingScreenCoroutine != null)
        {
            StopCoroutine(loadingScreenCoroutine);
        }
        
        // Reset progress bar
        if (loadingProgressBar != null)
        {
            loadingProgressBar.value = 0f;
        }
        
        // Set random loading tip
        if (loadingText != null && loadingTips != null && loadingTips.Length > 0)
        {
            string randomTip = loadingTips[Random.Range(0, loadingTips.Length)];
            loadingText.text = randomTip;
        }
        
        // Fade in the loading panel
        loadingScreenCoroutine = StartCoroutine(FadeCanvasGroup(loadingPanel, 0f, 1f, fadeSpeed));
    }
    
    /// <summary>
    /// Updates the loading progress
    /// </summary>
    public void UpdateLoadingProgress(float progress)
    {
        if (loadingProgressBar != null)
        {
            loadingProgressBar.value = progress;
        }
        
        if (loadingText != null)
        {
            loadingText.text = $"Loading... {progress * 100:0}%";
        }
    }
    
    /// <summary>
    /// Hides the loading screen
    /// </summary>
    public void HideLoadingScreen()
    {
        if (loadingPanel == null)
        {
            Debug.LogWarning("Loading panel not assigned!");
            return;
        }
        
        if (loadingScreenCoroutine != null)
        {
            StopCoroutine(loadingScreenCoroutine);
        }
        
        // Fade out the loading panel
        loadingScreenCoroutine = StartCoroutine(FadeCanvasGroup(loadingPanel, loadingPanel.alpha, 0f, fadeSpeed, () => {
            loadingPanel.gameObject.SetActive(false);
        }));
    }
    
    /// <summary>
    /// Shows an interaction prompt at the specified position
    /// </summary>
    public void ShowInteractionPrompt(string promptText, Vector3 worldPosition = default)
    {
        if (interactionPromptPanel == null && !useWorldSpacePrompts)
        {
            Debug.LogWarning("Interaction prompt panel not assigned!");
            return;
        }
        
        // Clear any existing prompt coroutine
        if (interactionPromptCoroutine != null)
        {
            StopCoroutine(interactionPromptCoroutine);
        }
        
        if (useWorldSpacePrompts)
        {
            // Remove any existing world prompt
            if (currentWorldPrompt != null)
            {
                Destroy(currentWorldPrompt);
            }
            
            // Create new world space prompt
            if (worldSpacePromptPrefab != null)
            {
                currentWorldPrompt = Instantiate(worldSpacePromptPrefab, worldPosition, Quaternion.identity);
                
                // Set text if there's a TextMeshPro component
                TextMeshPro promptTMP = currentWorldPrompt.GetComponentInChildren<TextMeshPro>();
                if (promptTMP != null)
                {
                    promptTMP.text = promptText;
                }
                
                // Face prompt toward camera
                currentWorldPrompt.transform.forward = Camera.main.transform.forward * -1;
            }
        }
        else
        {
            // Screen space UI prompt
            interactionPromptPanel.gameObject.SetActive(true);
            
            // Set prompt text
            if (interactionPromptText != null)
            {
                interactionPromptText.text = promptText;
            }
            
            // Position the prompt if we have a world position
            if (worldPosition != Vector3.zero && Camera.main != null)
            {
                RectTransform promptRect = interactionPromptPanel.GetComponent<RectTransform>();
                if (promptRect != null)
                {
                    Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
                    promptRect.position = screenPos;
                }
            }
            
            // Fade in the prompt
            interactionPromptCoroutine = StartCoroutine(FadeCanvasGroup(interactionPromptPanel, 0f, 1f, promptFadeSpeed));
        }
    }
    
    /// <summary>
    /// Hides the interaction prompt
    /// </summary>
    public void HideInteractionPrompt()
    {
        if (useWorldSpacePrompts)
        {
            // Remove world prompt
            if (currentWorldPrompt != null)
            {
                Destroy(currentWorldPrompt);
                currentWorldPrompt = null;
            }
        }
        else if (interactionPromptPanel != null)
        {
            // Clear any existing prompt coroutine
            if (interactionPromptCoroutine != null)
            {
                StopCoroutine(interactionPromptCoroutine);
            }
            
            // Fade out the prompt
            interactionPromptCoroutine = StartCoroutine(FadeCanvasGroup(interactionPromptPanel, interactionPromptPanel.alpha, 0f, promptFadeSpeed, () => {
                interactionPromptPanel.gameObject.SetActive(false);
            }));
        }
    }
    
    /// <summary>
    /// Shows the area name during transitions
    /// </summary>
    public void ShowAreaName(string areaName)
    {
        if (areaNamePanel == null)
        {
            Debug.LogWarning("Area name panel not assigned!");
            return;
        }
        
        // Clear any existing area name coroutine
        if (areaNameCoroutine != null)
        {
            StopCoroutine(areaNameCoroutine);
        }
        
        // Set area name text
        if (areaNameText != null)
        {
            areaNameText.text = areaName;
        }
        
        // Show area name with fade
        areaNameCoroutine = StartCoroutine(ShowAreaNameSequence(areaName));
    }
    
    /// <summary>
    /// Sets the visibility of the debug info panel
    /// </summary>
    public void SetDebugInfoVisibility(bool visible)
    {
        if (debugInfoPanel == null)
        {
            Debug.LogWarning("Debug info panel not assigned!");
            return;
        }
        
        debugInfoPanel.gameObject.SetActive(visible);
        
        if (visible && debugUpdateCoroutine == null)
        {
            StartDebugInfoUpdates();
        }
        else if (!visible && debugUpdateCoroutine != null)
        {
            StopCoroutine(debugUpdateCoroutine);
            debugUpdateCoroutine = null;
        }
    }
    
    /// <summary>
    /// Updates debug information with the latest values
    /// </summary>
    public void UpdateDebugInfo()
    {
        if (debugInfoPanel == null || !debugInfoPanel.gameObject.activeSelf)
            return;
            
        // Update time info
        if (timeText != null && environmentController != null)
        {
            float timeOfDay = environmentController.GetTimeOfDay();
            int hours = Mathf.FloorToInt(timeOfDay * 24);
            int minutes = Mathf.FloorToInt((timeOfDay * 24 * 60) % 60);
            timeText.text = $"Time: {hours:00}:{minutes:00}";
        }
        
        // Update weather info
        if (weatherText != null && environmentController != null)
        {
            string

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// UIManager handles all UI elements in PDX Underground, including interaction prompts, 
/// area name displays, loading screens, and transitions between areas.
/// </summary>
public class UIManager : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Interaction UI")]
    [SerializeField] private GameObject interactionPromptPanel;
    [SerializeField] private TextMeshProUGUI interactionPromptText;
    [SerializeField] private float promptFadeInDuration = 0.25f;
    [SerializeField] private float promptFadeOutDuration = 0.25f;
    
    [Header("Area Display")]
    [SerializeField] private GameObject areaNamePanel;
    [SerializeField] private TextMeshProUGUI areaNameText;
    [SerializeField] private float areaNameDisplayDuration = 3f;
    [SerializeField] private float areaNameFadeDuration = 0.5f;
    
    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingScreenPanel;
    [SerializeField] private Slider loadingBar;
    [SerializeField] private TextMeshProUGUI loadingText;
    
    [Header("Transitions")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.Linear(0, 0, 1, 1);
    
    [Header("Debug Info")]
    [SerializeField] private GameObject debugInfoPanel;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI weatherText;
    [SerializeField] private TextMeshProUGUI locationText;
    [SerializeField] private TextMeshProUGUI fpsText;
    
    #endregion
    
    #region Private Variables
    
    private Coroutine fadeCoroutine;
    private Coroutine areaNameCoroutine;
    private Coroutine interactionPromptCoroutine;
    private GameEnvironmentController environmentController;
    private float fps;
    private Canvas mainCanvas;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        // Get reference to main canvas
        mainCanvas = GetComponent<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("UIManager should be attached to a Canvas GameObject!");
        }
        
        // Find environment controller
        environmentController = FindObjectOfType<GameEnvironmentController>();
        
        // Initialize UI state
        if (interactionPromptPanel != null)
            interactionPromptPanel.SetActive(false);
            
        if (areaNamePanel != null)
            areaNamePanel.SetActive(false);
            
        if (loadingScreenPanel != null)
            loadingScreenPanel.SetActive(false);
            
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0;
            fadeCanvasGroup.blocksRaycasts = false;
        }
        
        // Initialize debug panel
        UpdateDebugPanel(true);
    }
    
    private void Start()
    {
        // Check for missing references
        CheckReferences();
    }
    
    private void Update()
    {
        // Update FPS counter
        UpdateFPS();
        
        // Update debug panel if enabled
        if (debugInfoPanel != null && debugInfoPanel.activeSelf)
        {
            UpdateDebugPanel();
        }
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Shows an interaction prompt at the specified world position
    /// </summary>
    public void ShowInteractionPrompt(string promptText, Vector3 worldPosition)
    {
        if (interactionPromptPanel == null || interactionPromptText == null)
            return;
            
        // Set the prompt text
        interactionPromptText.text = promptText;
        
        // Stop any existing fade coroutine
        if (interactionPromptCoroutine != null)
            StopCoroutine(interactionPromptCoroutine);
            
        // Start new fade in coroutine
        interactionPromptCoroutine = StartCoroutine(FadeInteractionPrompt(true, worldPosition));
    }
    
    /// <summary>
    /// Hides the interaction prompt
    /// </summary>
    public void HideInteractionPrompt()
    {
        if (interactionPromptPanel == null)
            return;
            
        // Stop any existing fade coroutine
        if (interactionPromptCoroutine != null)
            StopCoroutine(interactionPromptCoroutine);
            
        // Start new fade out coroutine
        interactionPromptCoroutine = StartCoroutine(FadeInteractionPrompt(false, Vector3.zero));
    }
    
    /// <summary>
    /// Shows the area name briefly
    /// </summary>
    public void ShowAreaName(string areaName)
    {
        if (areaNamePanel == null || areaNameText == null)
            return;
            
        // Set the area name text
        areaNameText.text = areaName;
        
        // Stop any existing area name coroutine
        if (areaNameCoroutine != null)
            StopCoroutine(areaNameCoroutine);
            
        // Start new area name display coroutine
        areaNameCoroutine = StartCoroutine(DisplayAreaName());
    }
    
    /// <summary>
    /// Shows the loading screen
    /// </summary>
    public void ShowLoadingScreen(string loadingMessage = "Loading...")
    {
        if (loadingScreenPanel == null)
            return;
            
        // Set loading text
        if (loadingText != null)
            loadingText.text = loadingMessage;
            
        // Reset loading bar
        if (loadingBar != null)
            loadingBar.value = 0;
            
        // Show the panel
        loadingScreenPanel.SetActive(true);
    }
    
    /// <summary>
    /// Updates the loading progress
    /// </summary>
    public void UpdateLoadingProgress(float progress)
    {
        if (loadingBar != null)
            loadingBar.value = Mathf.Clamp01(progress);
    }
    
    /// <summary>
    /// Hides the loading screen
    /// </summary>
    public void HideLoadingScreen()
    {
        if (loadingScreenPanel != null)
            loadingScreenPanel.SetActive(false);
    }
    
    /// <summary>
    /// Fades to black
    /// </summary>
    public IEnumerator FadeToBlack()
    {
        if (fadeCanvasGroup == null)
            yield break;
            
        // Stop any existing fade coroutine
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
            
        // Start new fade coroutine
        fadeCoroutine = StartCoroutine(FadeCanvas(fadeCanvasGroup, 0, 1, fadeOutDuration));
        yield return fadeCoroutine;
        
        // Ensure we're fully faded at the end
        fadeCanvasGroup.alpha = 1;
        fadeCanvasGroup.blocksRaycasts = true;
    }
    
    /// <summary>
    /// Fades from black
    /// </summary>
    public IEnumerator FadeFromBlack()
    {
        if (fadeCanvasGroup == null)
            yield break;
            
        // Stop any existing fade coroutine
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
            
        // Start new fade coroutine
        fadeCoroutine = StartCoroutine(FadeCanvas(fadeCanvasGroup, 1, 0, fadeInDuration));
        yield return fadeCoroutine;
        
        // Ensure we're fully transparent at the end
        fadeCanvasGroup.alpha = 0;
        fadeCanvasGroup.blocksRaycasts = false;
    }
    
    /// <summary>
    /// Sets the visibility of the debug info panel
    /// </summary>
    public void SetDebugInfoVisibility(bool visible)
    {
        if (debugInfoPanel != null)
            debugInfoPanel.SetActive(visible);
    }
    
    /// <summary>
    /// Toggles the visibility of the debug info panel
    /// </summary>
    public void ToggleDebugInfo()
    {
        if (debugInfoPanel != null)
            debugInfoPanel.SetActive(!debugInfoPanel.activeSelf);
    }
    
    #endregion
    
    #region Private Methods
    
    /// <summary>
    /// Checks for missing references and logs warnings
    /// </summary>
    private void CheckReferences()
    {
        if (interactionPromptPanel == null)
            Debug.LogWarning("Interaction prompt panel not assigned in UIManager!");
            
        if (interactionPromptText == null)
            Debug.LogWarning("Interaction prompt text not assigned in UIManager!");
            
        if (areaNamePanel == null)
            Debug.LogWarning("Area name panel not assigned in UIManager!");
            
        if (areaNameText == null)
            Debug.LogWarning("Area name text not assigned in UIManager!");
            
        if (loadingScreenPanel == null)
            Debug.LogWarning("Loading screen panel not assigned in UIManager!");
            
        if (fadeCanvasGroup == null)
            Debug.LogWarning("Fade canvas group not assigned in UIManager!");
    }
    
    /// <summary>
    /// Updates FPS counter
    /// </summary>
    private void UpdateFPS()
    {
        // Calculate FPS
        fps = Mathf.Lerp(fps, 1.0f / Time.unscaledDeltaTime, 0.1f);
        
        // Update text if visible
        if (fpsText != null && debugInfoPanel != null && debugInfoPanel.activeSelf)
        {
            fpsText.text = "FPS: " + Mathf.Round(fps);
        }
    }
    
    /// <summary>
    /// Updates all debug information
    /// </summary>
    private void UpdateDebugPanel(bool forceUpdate = false)
    {
        if (debugInfoPanel == null || (!debugInfoPanel.activeSelf && !forceUpdate))
            return;
            
        // Update time display
        if (timeText != null && environmentController != null)
        {
            float timeOfDay = environmentController.GetTimeOfDay();
            int hours = Mathf.FloorToInt(timeOfDay * 24);
            int minutes = Mathf.FloorToInt((timeOfDay * 24 - hours) * 60);
            timeText.text = $"Time: {hours:00}:{minutes:00}";
        }
        
        // Update weather display
        if (weatherText != null && environmentController != null)
        {
            weatherText.text = "Weather: " + environmentController.GetWeatherString();
        }
        
        // Update location display (would require position tracking in a full implementation)
        if (locationText != null)
        {
            // For testing, just use a fixed value
            locationText.text = "Location: Portland Streets";
        }
    }
    
    /// <summary>
    /// Fades a canvas group between two alpha values
    /// </summary>
    private IEnumerator FadeCanvas(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        // Ensure the canvas group exists
        if (canvasGroup == null)
            yield break;
            
        // Enable the canvas group if we're fading in
        if (startAlpha < endAlpha)
            canvasGroup.blocksRaycasts = true;
            
        // Get start time
        float startTime = Time.time;
        float elapsedTime = 0;
        
        // Fade over time
        while (elapsedTime < duration)
        {
            elapsedTime = Time.time - startTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            
            // Apply curve if available
            if (fadeCurve != null)
                t = fadeCurve.Evaluate(t);
                
            // Set alpha
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            
            yield return null;
        }
        
        // Ensure we reach the final value
        canvasGroup.alpha = endAlpha;
        
        // Disable the canvas group if we're faded out
        if (endAlpha < 0.01f)
            canvasGroup.blocksRaycasts = false;
    }
    
    /// <summary>
    /// Fades the interaction prompt in or out
    /// </summary>
    private IEnumerator FadeInteractionPrompt(bool fadeIn, Vector3 worldPosition)
    {
        // Ensure the prompt exists
        if (interactionPromptPanel == null)
            yield break;
            
        // Activate the panel
        interactionPromptPanel.SetActive(true);
        
        // Position the prompt if fade in (convert world position to screen space)
        if (fadeIn && mainCanvas != null)
        {
            // Get position in screen space
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
            
            // Adjust Y position to be above the object
            screenPos.y += 50;
            
            // Set the position
            RectTransform promptTransform = interactionPromptPanel.GetComponent<RectTransform>();
            if (promptTransform != null)
            {
                promptTransform.position = screenPos;
            }
        }
        
        // Get canvasgroup or add one if missing
        CanvasGroup promptGroup = interactionPromptPanel.GetComponent<CanvasGroup>();
        if (promptGroup == null)
            promptGroup = interactionPromptPanel.AddComponent<CanvasGroup>();
            
        // Set fade parameters
        float startAlpha = fadeIn ? 0 : 1;
        float endAlpha = fadeIn ? 1 : 0;
        float duration = fadeIn ? promptFadeInDuration : promptFadeOutDuration;
        
        // Fade the prompt
        yield return FadeCanvas(promptGroup, startAlpha, endAlpha, duration);
        
        // Deactivate if faded out
        if (!fadeIn)
            interactionPromptPanel.SetActive(false);
    }
    
    /// <summary>
    /// Displays the area name with fade in/out
    /// </summary>
    private IEnumerator DisplayAreaName()
    {
        // Ensure the area name panel exists
        if (areaNamePanel == null)
            yield break;
            
        // Activate the panel
        areaNamePanel.SetActive(true);
        
        // Get canvasgroup or add one if missing
        CanvasGroup areaNameGroup = areaNamePanel.GetComponent<CanvasGroup>();
        if (areaNameGroup == null)
            areaNameGroup = areaNamePanel.AddComponent<CanvasGroup>();
            
        // Fade in
        yield return FadeCanvas(areaNameGroup, 0, 1, areaNameFadeDuration);
        
        // Wait for display duration
        yield return new WaitForSeconds(areaNameDisplayDuration);
        
        // Fade out
        yield return F

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// UIManager handles UI elements for PDX Underground, including interaction prompts,
/// scene transition effects, and environmental information displays.
/// </summary>
public class UIManager : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("UI References")]
    [SerializeField] private GameObject interactionPromptPanel;
    [SerializeField] private TextMeshProUGUI interactionPromptText;
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private TextMeshProUGUI areaNameText;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider loadingProgressBar;
    
    [Header("UI Settings")]
    [SerializeField] private float promptFadeSpeed = 3.0f;
    [SerializeField] private Vector2 promptOffset = new Vector2(0, 30);
    [SerializeField] private float crossFadeDuration = 1.0f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float areaTextDisplayDuration = 3.0f;
    
    [Header("Debug Options")]
    [SerializeField] private bool showDebugInfo = false;
    [SerializeField] private TextMeshProUGUI debugInfoText;
    
    #endregion
    
    #region Private Variables
    
    private Camera mainCamera;
    private Coroutine fadeCoroutine;
    private Coroutine promptCoroutine;
    private Coroutine areaTextCoroutine;
    
    #endregion
    
    #region Singleton Pattern
    
    public static UIManager Instance { get; private set; }
    
    private void Awake()
    {
        // Singleton pattern setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Initialize UI elements
        if (interactionPromptPanel != null)
            interactionPromptPanel.SetActive(false);
            
        if (fadeCanvasGroup != null)
            fadeCanvasGroup.alpha = 0;
            
        if (loadingScreen != null)
            loadingScreen.SetActive(false);
            
        if (debugInfoText != null)
            debugInfoText.gameObject.SetActive(showDebugInfo);
    }
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Start()
    {
        mainCamera = Camera.main;
    }
    
    private void Update()
    {
        // Update debug info if enabled
        if (showDebugInfo && debugInfoText != null)
        {
            UpdateDebugInfo();
        }
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Shows an interaction prompt at the specified world position
    /// </summary>
    public void ShowInteractionPrompt(string promptText, Vector3 worldPosition)
    {
        if (interactionPromptPanel == null || interactionPromptText == null)
            return;
            
        // Update the prompt text
        interactionPromptText.text = promptText;
        
        // Position the prompt panel
        PositionPromptAtWorldPoint(worldPosition);
        
        // Make sure the panel is visible
        interactionPromptPanel.SetActive(true);
        
        // Cancel existing fade coroutine if running
        if (promptCoroutine != null)
            StopCoroutine(promptCoroutine);
            
        // Fade in the prompt
        CanvasGroup promptCanvasGroup = interactionPromptPanel.GetComponent<CanvasGroup>();
        if (promptCanvasGroup != null)
        {
            promptCoroutine = StartCoroutine(FadeCanvasGroup(promptCanvasGroup, 0, 1, promptFadeSpeed));
        }
    }
    
    /// <summary>
    /// Hides the current interaction prompt
    /// </summary>
    public void HideInteractionPrompt()
    {
        if (interactionPromptPanel == null)
            return;
            
        // Get the canvas group component
        CanvasGroup promptCanvasGroup = interactionPromptPanel.GetComponent<CanvasGroup>();
        
        // If no canvas group, just deactivate
        if (promptCanvasGroup == null)
        {
            interactionPromptPanel.SetActive(false);
            return;
        }
        
        // Cancel existing fade coroutine if running
        if (promptCoroutine != null)
            StopCoroutine(promptCoroutine);
            
        // Fade out the prompt
        promptCoroutine = StartCoroutine(FadeCanvasGroupAndDeactivate(promptCanvasGroup, 1, 0, promptFadeSpeed));
    }
    
    /// <summary>
    /// Fades the screen to black
    /// </summary>
    public IEnumerator FadeToBlack()
    {
        if (fadeCanvasGroup == null)
            yield break;
            
        // Cancel existing fade coroutine if running
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
            
        // Fade to black
        fadeCoroutine = StartCoroutine(FadeCanvasGroup(fadeCanvasGroup, fadeCanvasGroup.alpha, 1, crossFadeDuration));
        
        yield return fadeCoroutine;
    }
    
    /// <summary>
    /// Fades the screen from black
    /// </summary>
    public IEnumerator FadeFromBlack()
    {
        if (fadeCanvasGroup == null)
            yield break;
            
        // Cancel existing fade coroutine if running
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
            
        // Fade from black
        fadeCoroutine = StartCoroutine(FadeCanvasGroup(fadeCanvasGroup, fadeCanvasGroup.alpha, 0, crossFadeDuration));
        
        yield return fadeCoroutine;
    }
    
    /// <summary>
    /// Shows the loading screen with an optional progress bar
    /// </summary>
    public void ShowLoadingScreen(float progress = 0)
    {
        if (loadingScreen == null)
            return;
            
        loadingScreen.SetActive(true);
        
        if (loadingProgressBar != null)
        {
            loadingProgressBar.value = progress;
        }
    }
    
    /// <summary>
    /// Updates the loading progress
    /// </summary>
    public void UpdateLoadingProgress(float progress)
    {
        if (loadingProgressBar != null)
        {
            loadingProgressBar.value = progress;
        }
    }
    
    /// <summary>
    /// Hides the loading screen
    /// </summary>
    public void HideLoadingScreen()
    {
        if (loadingScreen == null)
            return;
            
        loadingScreen.SetActive(false);
    }
    
    /// <summary>
    /// Displays the name of the area being entered
    /// </summary>
    public void ShowAreaName(string areaName)
    {
        if (areaNameText == null)
            return;
            
        // Update the area name text
        areaNameText.text = areaName;
        
        // Make sure the text is visible
        areaNameText.gameObject.SetActive(true);
        
        // Cancel existing coroutine if running
        if (areaTextCoroutine != null)
            StopCoroutine(areaTextCoroutine);
            
        // Start the display coroutine
        areaTextCoroutine = StartCoroutine(DisplayAreaNameTimed());
    }
    
    /// <summary>
    /// Sets whether debug info should be shown
    /// </summary>
    public void SetDebugInfoVisibility(bool isVisible)
    {
        showDebugInfo = isVisible;
        
        if (debugInfoText != null)
            debugInfoText.gameObject.SetActive(showDebugInfo);
    }
    
    #endregion
    
    #region Private Methods
    
    /// <summary>
    /// Positions the interaction prompt at the specified world position
    /// </summary>
    private void PositionPromptAtWorldPoint(Vector3 worldPosition)
    {
        if (mainCamera == null || interactionPromptPanel == null)
            return;
            
        // Convert world position to screen position
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPosition);
        
        // Add offset
        screenPos += new Vector3(promptOffset.x, promptOffset.y, 0);
        
        // Set position of prompt
        interactionPromptPanel.transform.position = screenPos;
    }
    
    /// <summary>
    /// Fades a canvas group from one alpha value to another
    /// </summary>
    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0;
        canvasGroup.alpha = startAlpha;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, fadeCurve.Evaluate(elapsedTime / duration));
            canvasGroup.alpha = newAlpha;
            yield return null;
        }
        
        canvasGroup.alpha = endAlpha;
    }
    
    /// <summary>
    /// Fades a canvas group and deactivates its GameObject when done
    /// </summary>
    private IEnumerator FadeCanvasGroupAndDeactivate(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        yield return StartCoroutine(FadeCanvasGroup(canvasGroup, startAlpha, endAlpha, duration));
        
        canvasGroup.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Displays the area name for a timed duration
    /// </summary>
    private IEnumerator DisplayAreaNameTimed()
    {
        // Get or add canvas group to area name text
        CanvasGroup textCanvasGroup = areaNameText.GetComponent<CanvasGroup>();
        if (textCanvasGroup == null)
        {
            textCanvasGroup = areaNameText.gameObject.AddComponent<CanvasGroup>();
        }
        
        // Fade in
        yield return StartCoroutine(FadeCanvasGroup(textCanvasGroup, 0, 1, 0.5f));
        
        // Wait for display duration
        yield return new WaitForSeconds(areaTextDisplayDuration);
        
        // Fade out
        yield return StartCoroutine(FadeCanvasGroup(textCanvasGroup, 1, 0, 0.5f));
        
        // Hide the game object
        areaNameText.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Updates the debug information display
    /// </summary>
    private void UpdateDebugInfo()
    {
        if (debugInfoText == null)
            return;
            
        // Get environment info
        GameEnvironmentController envController = FindObjectOfType<GameEnvironmentController>();
        
        string debugText = "PDX Underground Debug Info\n";
        debugText += $"FPS: {(int)(1.0f / Time.smoothDeltaTime)}\n";
        
        if (envController != null)
        {
            // Add environment information
            // Note: These properties would need to be exposed in GameEnvironmentController
            debugText += $"Weather: {GetFieldValueIfExists(envController, "currentWeather")}\n";
            debugText += $"Time: {GetFieldValueIfExists(envController, "currentTimeOfDay")}\n";
            debugText += $"Active NPCs: {GetFieldValueIfExists(envController, "activeNPCs.Count")}\n";
        }
        
        // Add player position if available
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 pos = player.transform.position;
            debugText += $"Player Position: ({pos.x:F1}, {pos.y:F1}, {pos.z:F1})\n";
        }
        
        // Update the text
        debugInfoText.text = debugText;
    }
    
    /// <summary>
    /// Helper method to get field values using reflection for debug purposes
    /// </summary>
    private string GetFieldValueIfExists(object target, string fieldPath)
    {
        try
        {
            // Split path for nested fields
            string[] parts = fieldPath.Split('.');
            object current = target;
            
            // Navigate through the path
            foreach (string part in parts)
            {
                System.Reflection.FieldInfo field = current.GetType().GetField(part, 
                    System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.NonPublic);
                
                if (field == null)
                    return "N/A";
                    
                current = field.GetValue(current);
                
                if (current == null)
                    return "null";
            }
            
            return current.ToString();
        }
        catch
        {
            return "Error";
        }
    }
    
    #endregion
}

