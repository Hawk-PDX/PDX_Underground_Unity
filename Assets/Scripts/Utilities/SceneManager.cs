using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

namespace PDXUnderground.Utilities
{
    public class SceneManager : MonoBehaviour
    {
        [SerializeField] private float transitionDuration = 1.5f;
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        
        public static PDXUnderground.Utilities.SceneManager Instance { get; private set; }
        
        // Event triggered when scene loading begins
        public event Action<string> OnSceneLoadStarted;
        
        // Event triggered when scene loading completes
        public event Action<string> OnSceneLoadCompleted;
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                // Initialize fadeCanvasGroup if not set
                if (fadeCanvasGroup == null)
                {
                    // Try to find it in the scene
                    fadeCanvasGroup = FindObjectOfType<CanvasGroup>();
                    
                    // If still not found, create one
                    if (fadeCanvasGroup == null)
                    {
                        GameObject fadeCanvas = new GameObject("FadeCanvas");
                        Canvas canvas = fadeCanvas.AddComponent<Canvas>();
                        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                        canvas.sortingOrder = 999; // Ensure it renders on top
                        
                        fadeCanvasGroup = fadeCanvas.AddComponent<CanvasGroup>();
                        fadeCanvasGroup.alpha = 0f;
                        
                        // Add a black background
                        GameObject bgImage = new GameObject("BlackBackground");
                        bgImage.transform.SetParent(fadeCanvas.transform);
                        RectTransform rectTransform = bgImage.AddComponent<RectTransform>();
                        rectTransform.anchorMin = Vector2.zero;
                        rectTransform.anchorMax = Vector2.one;
                        rectTransform.sizeDelta = Vector2.zero;
                        
                        UnityEngine.UI.Image image = bgImage.AddComponent<UnityEngine.UI.Image>();
                        image.color = Color.black;
                        
                        DontDestroyOnLoad(fadeCanvas);
                    }
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Load a scene by name with a fade transition
        /// </summary>
        /// <param name="sceneName">Name of the scene to load</param>
        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneRoutine(sceneName));
        }
        
        /// <summary>
        /// Coroutine to handle scene loading with fade transition
        /// </summary>
        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            // Notify that scene loading has started
            OnSceneLoadStarted?.Invoke(sceneName);
            
            // Fade out
            yield return StartCoroutine(FadeRoutine(0f, 1f, transitionDuration * 0.5f));
            
            // Load the scene
            AsyncOperation asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
            asyncOperation.allowSceneActivation = true;
            
            // Wait for scene to load
            while (!asyncOperation.isDone)
            {
                yield return null;
            }
            
            // Fade in
            yield return StartCoroutine(FadeRoutine(1f, 0f, transitionDuration * 0.5f));
            
            // Notify that scene loading has completed
            OnSceneLoadCompleted?.Invoke(sceneName);
        }
        
        /// <summary>
        /// Load a scene additively (without unloading the current scene)
        /// </summary>
        /// <param name="sceneName">Name of the scene to load</param>
        public void LoadSceneAdditive(string sceneName)
        {
            StartCoroutine(LoadSceneAdditiveRoutine(sceneName));
        }
        
        /// <summary>
        /// Coroutine to handle additive scene loading
        /// </summary>
        private IEnumerator LoadSceneAdditiveRoutine(string sceneName)
        {
            // Load the scene additively
            AsyncOperation asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(
                sceneName, LoadSceneMode.Additive);
            
            // Wait for scene to load
            while (!asyncOperation.isDone)
            {
                yield return null;
            }
        }
        
        /// <summary>
        /// Unload an additively loaded scene
        /// </summary>
        /// <param name="sceneName">Name of the scene to unload</param>
        public void UnloadScene(string sceneName)
        {
            StartCoroutine(UnloadSceneRoutine(sceneName));
        }
        
        /// <summary>
        /// Coroutine to handle scene unloading
        /// </summary>
        private IEnumerator UnloadSceneRoutine(string sceneName)
        {
            // Unload the scene
            AsyncOperation asyncOperation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);
            
            // Wait for scene to unload
            while (!asyncOperation.isDone)
            {
                yield return null;
            }
        }
        
        /// <summary>
        /// Fade canvas group from one alpha value to another
        /// </summary>
        private IEnumerator FadeRoutine(float startAlpha, float targetAlpha, float duration)
        {
            float elapsedTime = 0f;
            
            // Set initial alpha
            fadeCanvasGroup.alpha = startAlpha;
            
            // Enable raycast blocking during fade
            fadeCanvasGroup.blocksRaycasts = true;
            
            // Interpolate alpha over time
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsedTime / duration);
                fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, normalizedTime);
                yield return null;
            }
            
            // Ensure we reach the target alpha exactly
            fadeCanvasGroup.alpha = targetAlpha;
            
            // Disable raycast blocking if fully transparent
            if (targetAlpha <= 0f)
            {
                fadeCanvasGroup.blocksRaycasts = false;
            }
        }
        
        /// <summary>
        /// Get the name of the active scene
        /// </summary>
        public string GetActiveSceneName()
        {
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        }
    }
}
