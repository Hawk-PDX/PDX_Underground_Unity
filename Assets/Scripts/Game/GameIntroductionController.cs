using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PDXUnderground.Core;
using PDXUnderground.Core.Models;  // For TimeOfDay and GameState enums
using PDXUnderground.Core.Data;
using PDXUnderground.Core.Interfaces; // For IGaslightFlicker interface
namespace PDXUnderground.Game
{
    /// <summary>
    /// GameIntroductionController manages the opening sequence for PDX Underground,
    /// handling narrative elements, camera movement, and scene transitions with
    /// period-appropriate styling for 1800s Portland.
    /// </summary>
    public class GameIntroductionController : MonoBehaviour
{
    #region Serialized Properties

    [SerializeField]
    private CanvasGroup introUI;

    [SerializeField]
    private TextMeshProUGUI narrativeText;

    [SerializeField]
    private Image fadeOverlay;

    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private Transform[] cameraPositions;

    [SerializeField]
    private Transform speakeasyEntrance;

    [SerializeField]
    private Transform stormDrain;

    [SerializeField]
    private Transform shanghaiTunnels;

    [SerializeField]
    private Transform gambler;

    [SerializeField]
    private float fadeInDuration = 2.0f;

    [SerializeField]
    private float fadeOutDuration = 1.5f;

    [SerializeField]
    private float textDisplayDuration = 4.0f;

    [SerializeField]
    private float cameraMoveSpeed = 2.0f;


    #endregion

    #region Private Fields

    private PDXUnderground.Core.GameManager gameManager; // Core GameManager reference
    private Coroutine currentCoroutine;
    private List<string> narrativeSequence;
    private int currentNarrativeIndex = 0;
    private bool introductionActive = false;
    private bool isNightTime = true; // Default to night for the introduction

    // Historical Portland atmosphere settings
    private Color nightSkyColor = new Color(0.05f, 0.05f, 0.1f);
    private Color daySkyColor = new Color(0.5f, 0.7f, 0.9f);
    private float gaslightIntensity = 0.8f;
    private float gaslightRange = 15.0f;
    #endregion

    #region Lifecycle Methods

    private void Start()
    {
        // Initialize references
        gameManager = FindObjectOfType<PDXUnderground.Core.GameManager>();

        // Prepare UI elements
        fadeOverlay.color = new Color(0, 0, 0, 1); // Start with black screen
        narrativeText.text = "";

        // Set up narrative text sequence
        InitializeNarrativeSequence();

        // Start in disabled state until explicitly started
        introUI.gameObject.SetActive(false);

        // Subscribe to game manager events if needed
        if (gameManager != null)
        {
            gameManager.GameStateChanged += OnGameStateChanged;
        }
    }

    private void Update()
    {
        if (introductionActive)
        {
            // Process any continuous updates for the intro sequence
            UpdateTimeOfDayEffects(Time.deltaTime);
        }
    }

    private void OnDestroy()
    {
        // Clean up event subscriptions
        if (gameManager != null)
        {
            gameManager.GameStateChanged -= OnGameStateChanged;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Begins the game introduction sequence.
    /// </summary>
    public void StartIntroduction()
    {
        if (introductionActive)
            return;

        introductionActive = true;
        introUI.gameObject.SetActive(true);

        // Set appropriate time of day for the introduction
        SetTimeOfDay(isNightTime);

        // Start the introduction sequence
        StartCoroutine(IntroductionSequence());
    }

    /// <summary>
    /// Stops the introduction sequence and cleans up.
    /// </summary>
    public void StopIntroduction()
    {
        if (!introductionActive)
            return;

        // Fade out and clean up
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        StartCoroutine(FadeOutAndCleanup());
    }

    private IEnumerator FadeOutAndCleanup()
    {
        yield return StartCoroutine(FadeOut());

        introUI.gameObject.SetActive(false);
        introductionActive = false;

        // Notify game manager that intro is complete
        if (gameManager != null)
        {
            gameManager.OnIntroductionComplete();
        }
    }

    /// <summary>
    /// Skips the introduction sequence.
    /// </summary>
    public void SkipIntroduction()
    {
        if (!introductionActive)
            return;

        // Cancel any active coroutines
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        StopIntroduction();
    }

    private IEnumerator IntroductionSequence()
    {
        yield return StartCoroutine(FadeIn());
        yield return StartCoroutine(StartNarrativeSequence());
    }

    #endregion
    
    #region Private Methods

    /// <summary>
    /// Initializes the narrative sequence with historically accurate text.
    /// </summary>
    private void InitializeNarrativeSequence()
    {
        narrativeSequence = new List<string>
        {
            "Portland, Oregon - 1880s",
            "The city's rapid growth brought opportunity... and darkness.",
            "The Shanghai Tunnels beneath the streets hide terrible secrets.",
            "Unsuspecting patrons of speakeasies disappear nightly...",
            "Taken through stormins and false floors, their freedom stolen.",
            "Destined to become slaves aboard ships bound for the Far East.",
            "\"Welp, we've made it... time to dig in. Where to start?\"",
            "\"I'll give it some thought... with a drink of course.\""
        };
    }

    /// <summary>
    /// Handles the narrative sequence display and camera movements.
    /// </summary>

    private IEnumerator StartNarrativeSequence()
    {
        for (int i = 0; i < narrativeSequence.Count; i++)
        {
            currentNarrativeIndex = i;

            // Update camera position based on narrative progress
            yield return StartCoroutine(MoveCameraToKeyLocation(i));

            // Display the narrative text
            yield return StartCoroutine(DisplayNarrativeText(narrativeSequence[i]));

            // Wait before showing the next text
            yield return new WaitForSeconds(textDisplayDuration);

            // Fade out text before next one
            yield return StartCoroutine(FadeOutText());

            // Check if introduction was canceled
            if (!introductionActive)
                break;
        }

        // Introduction sequence complete, transition to gameplay
        yield return StartCoroutine(TransitionToGameplay());
    }




    /// <summary>
    /// Fades in the screen from black.
    /// </summary>
    private IEnumerator FadeIn()
    {
        float elapsedTime = 0;
        Color startColor = fadeOverlay.color;
        Color endColor = new Color(0, 0, 0, 0);

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeOverlay.color = Color.Lerp(startColor, endColor, elapsedTime / fadeInDuration);
            yield return null;
        }

        fadeOverlay.color = endColor;
    }
    /// <summary>
    /// Fades out the screen to black.
    /// </summary>
    private IEnumerator FadeOut()
    {
        float elapsedTime = 0;
        Color startColor = fadeOverlay.color;
        Color endColor = new Color(0, 0, 0, 1);

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeOverlay.color = Color.Lerp(startColor, endColor, elapsedTime / fadeOutDuration);
            yield return null;
        }

        fadeOverlay.color = endColor;
    }
    /// <summary>
    /// Displays narrative text with a fade-in effect.
    /// </summary>
    private IEnumerator DisplayNarrativeText(string text)
    {
        narrativeText.text = text;
        narrativeText.color = new Color(1, 1, 1, 0);

        float elapsedTime = 0;
        Color startColor = narrativeText.color;
        Color endColor = new Color(1, 1, 1, 1);

        while (elapsedTime < 1.0f)
        {
            elapsedTime += Time.deltaTime;
            narrativeText.color = Color.Lerp(startColor, endColor, elapsedTime);
            yield return null;
        }

        narrativeText.color = endColor;
    }
    /// <summary>
    /// Fades out the narrative text.
    /// </summary>
    private IEnumerator FadeOutText()
    {
        float elapsedTime = 0;
        Color startColor = narrativeText.color;
        Color endColor = new Color(1, 1, 1, 0);

        while (elapsedTime < 1.0f)
        {
            elapsedTime += Time.deltaTime;
            narrativeText.color = Color.Lerp(startColor, endColor, elapsedTime);
            yield return null;
        }

        narrativeText.color = endColor;
    }
    /// <summary>
    /// Moves the camera to a key location based on narrative progress.
    /// </summary>
    private IEnumerator MoveCameraToKeyLocation(int narrativeIndex)
    {
        if (cameraPositions == null || narrativeIndex >= cameraPositions.Length)
            yield break;

        Transform targetPosition = cameraPositions[narrativeIndex];
        if (targetPosition == null)
            yield break;

        float elapsedTime = 0;
        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;

        float distance = Vector3.Distance(startPosition, targetPosition.position);
        float duration = distance / cameraMoveSpeed;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition.position, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startRotation, targetPosition.rotation, t);

            yield return null;
        }

        mainCamera.transform.position = targetPosition.position;
        mainCamera.transform.rotation = targetPosition.rotation;
    }
    /// <summary>
    /// Transitions from introduction to gameplay.
    /// </summary>
    private IEnumerator TransitionToGameplay()
    {
        yield return StartCoroutine(FadeOut());

        if (gambler != null)
        {
            Vector3 offset = new Vector3(0, 1.5f, -3.0f);
            mainCamera.transform.position = gambler.position + offset;
            mainCamera.transform.LookAt(gambler.position + Vector3.up);
        }

        if (gameManager != null)
        {
            gameManager.SetGameState(PDXUnderground.Core.Models.GameState.Playing);
        }

        yield return StartCoroutine(FadeIn());

        StopIntroduction();
    }

    /// <summary>
    /// Sets the time of day for the introduction (day or night).
    /// </summary>
    private void SetTimeOfDay(bool isNight)
    {
        isNightTime = isNight;

        if (isNight)
        {
            // Night settings
            RenderSettings.ambientLight = new Color(0.1f, 0.1f, 0.2f);
            RenderSettings.ambientIntensity = 0.3f;
            AdjustGaslights(1.0f);
        }
        else
        {
            // Day settings
            RenderSettings.ambientLight = new Color(0.6f, 0.6f, 0.5f);
            RenderSettings.ambientIntensity = 1.0f;
            AdjustGaslights(0.4f);
        }

        // Notify game manager of time change
        if (gameManager != null)
        {
            gameManager.SetTimeOfDay(isNight ? 
                PDXUnderground.Core.Models.TimeOfDay.Night : 
                PDXUnderground.Core.Models.TimeOfDay.Day);
        }
    }

    /// <summary>
    /// Adjusts gaslight intensity based on time of day.
    /// </summary>
    private void AdjustGaslights(float intensityMultiplier)
    {
        // Find all gaslights using Unity's tag system
        Light[] gaslights = GameObject.FindGameObjectsWithTag("Gaslight")
            .Select(go => go.GetComponent<Light>())
            .Where(light => light != null)
            .ToArray();

        foreach (Light light in gaslights)
        {
            light.intensity = gaslightIntensity * intensityMultiplier;

            // Update flicker component if present, using interface
            IGaslightFlicker flickerComponent = light.GetComponent<IGaslightFlicker>();
            if (flickerComponent != null)
            {
                // Use the correct time of day method which handles intensity internally
                flickerComponent.SetTimeOfDay(isNightTime ? 0.0f : 0.5f);
                
                // If stormy weather, add some flicker effect
                flickerComponent.SetWeatherEffect(1.0f);
            }
        }
    }

    /// <summary>
    /// Updates time-of-day specific effects.
    /// </summary>
    private void UpdateTimeOfDayEffects(float deltaTime)
    {
        // Update any time-based effects using Unity's Time.deltaTime
        if (isNightTime)
        {
            // Night-specific updates
            RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, nightSkyColor, deltaTime * 0.5f);
            RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, nightSkyColor * 0.5f, deltaTime * 0.5f);
        }
        else
        {
            // Day-specific updates
            RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, daySkyColor, deltaTime * 0.5f);
            RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, daySkyColor * 0.8f, deltaTime * 0.5f);
        }
    }

    /// <summary>
    /// Handles game state changes from the game manager.
    /// </summary>
    private void OnGameStateChanged(PDXUnderground.Core.Models.GameState newState)
    {
        // React to game state changes if needed
        if (newState == PDXUnderground.Core.Models.GameState.MainMenu)
        {
            // Reset introduction if player returns to main menu
            if (introductionActive)
            {
                SkipIntroduction();
            }
        }
    }
    #endregion
    }
}
