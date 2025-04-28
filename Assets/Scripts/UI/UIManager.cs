using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using PDXUnderground.Controllers;
using PDXUnderground.Core.Interfaces;
namespace PDXUnderground.UI
{
    public class UIManager : MonoBehaviour, IUIManager, ILoadingScreen
    {
        #region Inspector Fields
        [Header("Interaction UI")]
        [SerializeField] private GameObject interactionPromptPanel;
        [SerializeField] private TextMeshProUGUI interactionPromptText;
        [SerializeField] private float promptFadeInDuration = 0.25f;
        [SerializeField] private float promptFadeOutDuration = 0.25f;

        [Header("Area Name UI")]
        [SerializeField] private GameObject areaNamePanel;
        [SerializeField] private TextMeshProUGUI areaNameText;
        [SerializeField] private float areaNameFadeDuration = 0.5f;
        [SerializeField] private float areaNameDisplayDuration = 3f;

        [Header("Loading Screen")]
        [SerializeField] private GameObject loadingScreenPanel;
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private Slider loadingBar;

        [Header("Screen Fade")]
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private AnimationCurve fadeCurve;
        [SerializeField] private float fadeInDuration = 1f;
        [SerializeField] private float fadeOutDuration = 1f;

        [Header("Debug UI")]
        [SerializeField] private GameObject debugInfoPanel;
        [SerializeField] private TextMeshProUGUI fpsText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI weatherText;
        [SerializeField] private TextMeshProUGUI locationText;
        #endregion

        #region Private Fields
        // Singleton instance
        public static UIManager Instance { get; private set; }
        
        // References
        private Camera mainCamera;
        private Canvas mainCanvas;
        private IEnvironmentSystem environmentController;
        
        // Runtime variables
        private float fps;
        
        // Coroutine references
        private Coroutine areaNameCoroutine;
        private Coroutine fadeCoroutine;
        private Coroutine promptCoroutine;
        #endregion

        #region MonoBehaviour Methods
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            // Get required references
            mainCanvas = GetComponent<Canvas>();
            if (mainCanvas == null)
            {
                Debug.LogError("UIManager should be attached to a Canvas GameObject!");
            }
            
            environmentController = FindObjectOfType<MonoBehaviour>() as IEnvironmentSystem;
            InitializeUIState();
        }

        private void Start()
        {
            mainCamera = Camera.main;
            CheckReferences();
        }

        private void Update()
        {
            UpdateFPS();
            if (debugInfoPanel != null && debugInfoPanel.activeSelf)
            {
                UpdateDebugPanel();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
        #endregion

#region Interface Implementations - IUIManager
        public void ShowTransitionScreen(string areaName, float duration)
        {
            if (fadeCanvasGroup == null)
                return;
                
            // Stop any existing fade coroutine
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
                
            // Start fade in
            fadeCoroutine = StartCoroutine(FadeCanvas(fadeCanvasGroup, 0, 1, duration));
            fadeCanvasGroup.blocksRaycasts = true;
            
            // Show area name if provided
            if (!string.IsNullOrEmpty(areaName))
            {
                ShowAreaName(areaName);
            }
        }

        public void HideTransitionScreen()
        {
            if (fadeCanvasGroup == null)
                return;
                
            // Stop any existing fade coroutine
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
                
            // Start fade out
            fadeCoroutine = StartCoroutine(FadeCanvas(fadeCanvasGroup, 1, 0, fadeOutDuration));
            fadeCanvasGroup.blocksRaycasts = false;
        }

        public void UpdateUIState(GameState state)
        {
            // Update UI elements based on game state
            switch (state)
            {
                case GameState.MainMenu:
                    // Configure UI for main menu
                    break;
                    
                case GameState.Playing:
                    // Configure UI for gameplay
                    break;
                    
                case GameState.Paused:
                    // Configure UI for pause state
                    break;
                    
                case GameState.Loading:
                    // Configure UI for loading state
                    break;
                    
                case GameState.GameOver:
                    // Configure UI for game over state
                    break;
            }
        }

        public void ShowLoadingScreen(float progress)
        {
            if (loadingScreenPanel == null || loadingBar == null)
                return;

            loadingScreenPanel.SetActive(true);
            loadingBar.value = Mathf.Clamp01(progress);
            
            if (loadingText != null)
            {
                loadingText.text = $"Loading... {(progress * 100):F0}%";
            }
        }
        
        /// <summary>
        /// Shows the loading screen without a progress value
        /// </summary>
        public void ShowLoadingScreen()
        {
            ShowLoadingScreen(0f);
        }

        public void HideLoadingScreen()
        {
            if (loadingScreenPanel != null)
            {
                loadingScreenPanel.SetActive(false);
            }
        }
        public void ShowAreaName(string areaName)
        {
            if (areaNamePanel == null || areaNameText == null)
                return;

            areaNameText.text = areaName;

            if (areaNameCoroutine != null)
                StopCoroutine(areaNameCoroutine);

            areaNameCoroutine = StartCoroutine(DisplayAreaName());
        }
        
        public void ShowInteractionPrompt(string promptText, bool show)
        {
            if (interactionPromptPanel == null || interactionPromptText == null)
                return;
                
            interactionPromptText.text = promptText;
            
            if (promptCoroutine != null)
                StopCoroutine(promptCoroutine);
                
            promptCoroutine = StartCoroutine(FadeInteractionPrompt(show));
        }
        
        /// <summary>
        /// Shows an interaction prompt at a specific world position
        /// </summary>
        public void ShowInteractionPrompt(string promptText, Vector3 worldPosition)
        {
            if (interactionPromptPanel == null || interactionPromptText == null)
                return;

            // Update prompt text
            interactionPromptText.text = promptText;
            
            // Update position if the prompt is in world space
            if (mainCanvas.renderMode == RenderMode.WorldSpace)
            {
                interactionPromptPanel.transform.position = worldPosition;
            }
            
            ShowInteractionPrompt(true, promptText);
        }

        /// <summary>
        /// Hides the interaction prompt
        /// </summary>
        public void HideInteractionPrompt()
        {
            ShowInteractionPrompt(string.Empty, false);
        }
        
        
        public void FadeScreen(bool fadeIn)
        {
            if (fadeCanvasGroup == null)
                return;
                
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
                
            float duration = fadeIn ? fadeInDuration : fadeOutDuration;
            fadeCoroutine = StartCoroutine(FadeCanvas(fadeCanvasGroup, fadeIn ? 0 : 1, fadeIn ? 1 : 0, duration));
            fadeCanvasGroup.blocksRaycasts = fadeIn;
        }
        
        /// <summary>
        /// Fades the screen to black
        /// </summary>
        public IEnumerator FadeToBlack()
        {
            if (fadeCanvasGroup == null)
                yield break;

            yield return StartCoroutine(FadeCanvas(fadeCanvasGroup, 0, 1, fadeInDuration));
        }

        /// <summary>
        /// Fades the screen from black
        /// </summary>
        public IEnumerator FadeFromBlack()
        {
            if (fadeCanvasGroup == null)
                yield break;

            yield return StartCoroutine(FadeCanvas(fadeCanvasGroup, 1, 0, fadeOutDuration));
        }
        public void ToggleDebugInfo(bool show)
        {
            if (debugInfoPanel != null)
                debugInfoPanel.SetActive(show);
        }
        #endregion
        
        #region Interface Implementations - ILoadingScreen
        public void Show(bool visible)
        {
            if (loadingScreenPanel != null)
                loadingScreenPanel.SetActive(visible);
        }

        public void UpdateProgress(float progress)
        {
            if (loadingBar != null)
                loadingBar.value = Mathf.Clamp01(progress);
        }

        public void SetLoadingText(string text)
        {
            if (loadingText != null)
                loadingText.text = text;
        }
        #endregion

        #region Private Helper Methods
        private void InitializeUIState()
        {
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
            if (debugInfoPanel != null)
                debugInfoPanel.SetActive(false);
        }

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
            if (loadingBar == null)
                Debug.LogWarning("Loading bar not assigned in UIManager!");
            if (fadeCanvasGroup == null)
                Debug.LogWarning("Fade canvas group not assigned in UIManager!");
        }

        private void UpdateFPS()
        {
            fps = Mathf.Lerp(fps, 1.0f / Time.unscaledDeltaTime, 0.1f);
            if (fpsText != null)
            {
                fpsText.text = $"FPS: {Mathf.Round(fps)}";
            }
        }

        private void UpdateDebugPanel()
        {
            IEnvironmentSystem environmentSystem = environmentController as IEnvironmentSystem;
            if (environmentSystem != null)
            {
                if (timeText != null)
                {
                    string timeOfDay = environmentSystem.GetTimeOfDay();
                    if (timeOfDay.Contains(":"))
                    {
                        timeText.text = $"Time: {timeOfDay}";
                    }
                }

                if (weatherText != null)
                {
                    weatherText.text = $"Weather: {environmentSystem.GetWeatherString()}";
                }

                if (locationText != null)
                {
                    locationText.text = $"Location: {environmentSystem.GetCurrentEnvironmentName()}";
                }
            }
        }
        #endregion

        #region Coroutines
        private IEnumerator DisplayAreaName()
        {
            if (areaNamePanel == null)
                yield break;

            areaNamePanel.SetActive(true);
            var canvasGroup = areaNamePanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = areaNamePanel.AddComponent<CanvasGroup>();

            yield return StartCoroutine(FadeCanvas(canvasGroup, 0, 1, areaNameFadeDuration));
            yield return new WaitForSeconds(areaNameDisplayDuration);
            yield return StartCoroutine(FadeCanvas(canvasGroup, 1, 0, areaNameFadeDuration));

            areaNamePanel.SetActive(false);
        }

        private IEnumerator FadeInteractionPrompt(bool fadeIn)
        {
            if (interactionPromptPanel == null)
                yield break;

            interactionPromptPanel.SetActive(true);
            var canvasGroup = interactionPromptPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = interactionPromptPanel.AddComponent<CanvasGroup>();

            float duration = fadeIn ? promptFadeInDuration : promptFadeOutDuration;
            yield return StartCoroutine(FadeCanvas(canvasGroup, fadeIn ? 0 : 1, fadeIn ? 1 : 0, duration));

            if (!fadeIn)
                interactionPromptPanel.SetActive(false);
        }

        private IEnumerator FadeCanvas(CanvasGroup group, float startAlpha, float targetAlpha, float duration)
        {
            if (group == null)
                yield break;

            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                if (fadeCurve != null)
                    t = fadeCurve.Evaluate(t);
                group.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }
            group.alpha = targetAlpha;
        }
        #endregion
    }
}
