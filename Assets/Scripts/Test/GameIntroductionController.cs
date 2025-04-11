using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Controls the opening narrative scene that introduces the game's story and setting
/// Provides a cinematic introduction to 1800s Portland and the Shanghai Tunnels
/// </summary>
public class GameIntroductionController : MonoBehaviour
{
    [Header("UI References")]
    public Image backgroundImage;
    public Text narrativeText;
    public Text titleText;
    public Button continueButton;
    public GameObject textPanel;
    
    [Header("Camera Settings")]
    public Transform[] cameraWaypoints;
    public float waypointStayDuration = 5f;
    public float transitionDuration = 2f;
    
    [Header("Game References")]
    public GameObject playerObject;
    public SpeakeasyEntrance speakeasyEntrance;
    public ShanghaiTunnels shanghaiTunnels;
    
    [Header("Atmosphere Settings")]
    public bool useVolumeEffects = true;
    public bool usePeriodAudioEffects = true;
    public bool useParticleEffects = true;
    
    // Private variables
    private ThirdPersonCameraController playerCamera;
    private Camera mainCamera;
    private Transform originalCameraParent;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private bool isIntroRunning = false;
    private GameObject tempCameraObject;
    private List<string> narrativeSequences;
    private int currentSequenceIndex = 0;
    private bool isFadingIn = false;
    private bool isFadingOut = false;
    private Volume postProcessingVolume;
    private AudioSource audioSource;
    private AudioSource ambienceAudioSource;
    private int currentWaypointIndex = 0;
    
    private void Awake()
    {
        // Find main camera if not explicitly set
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No main camera found in the scene");
            return;
        }
        
        // Initialize audio sources
        InitializeAudio();
        
        // Create post-processing volume if needed
        InitializePostProcessing();
    }
    
    /// <summary>
    /// Initialize the introduction sequence
    /// </summary>
    public void StartIntroduction()
    {
        if (isIntroRunning)
            return;
            
        isIntroRunning = true;
        
        // Initialize narrative sequences if not already set
        if (narrativeSequences == null || narrativeSequences.Count == 0)
        {
            InitializeNarrativeSequences();
        }
        
        // Save original camera state
        SaveOriginalCameraState();
        
        // Begin the introduction sequence
        StartCoroutine(RunIntroSequence());
    }
    
    /// <summary>
    /// Initialize narrative text sequences
    /// </summary>
    private void InitializeNarrativeSequences()
    {
        narrativeSequences = new List<string>
        {
            "Welcome to Portland, 1800s. A city of opportunity and danger, where fortunes are made and lives are lost in the shadows.",
            
            "\"Welp, we've made it... time to dig in. Where to start? ... I'll give it some thought.. with a drink of course.\"",
            
            "Access to the tunnels is through storm drains out front between the speakeasy and the bank next door... there are others hidden in plain sight as this one is.",
            
            "Generally, these purposeful entry points are only accessed in the cloak of night's darkness. Chinese trading companies play a major part in the transaction of selling these unsuspecting patrons of the speakeasy.",
            
            "Alternative entry points are latched, false floors, strategically placed beneath certain chairs at certain tables... always one at the poker table.",
            
            "Once dropped through false floor, the unsuspecting patrons (high on liquid 'spirits', and tired from a hardworked day), would fall in to the tunnels, shoes removed and shattered glass peppering the tunnel floor in every direction.",
            
            "These victims had no choice but to pick a direction and walk over the glass, cutting their feet and leaving little doubt as to their whereabouts.",
            
            "Ultimately the only way out was through... and even if successfully reaching an exit, realizing they were trapped. Exhausted and destined to be deckhands on the sea to their demise... as slaves to east-asia.",
            
            "Along their journey, many died from scurvy and other 'sea-born' illnesses or sold along the way for their captures ship repairs and other materials that would be necessary to reach their intended destination.",
            
            "In the end they made it all this way to the west, through incredible loss and unbearable conditions, in hopes of making a better life for their families and their futures. They assumed they'd made it! ...circumstance barred the fruition of these individuals and their dreams.",
            
            "As 'The Gambler', you are on a mission to take down this villainous ring and restore hope amongst the region."
        };
    }
    
    /// <summary>
    /// Initialize audio components for the introduction
    /// </summary>
    private void InitializeAudio()
    {
        // Create audio source for narrative and effects
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.volume = 0.7f;
        }
        
        // Create separate audio source for ambient sounds
        if (ambienceAudioSource == null)
        {
            GameObject ambienceObj = new GameObject("AmbienceAudio");
            ambienceObj.transform.SetParent(transform);
            ambienceAudioSource = ambienceObj.AddComponent<AudioSource>();
            ambienceAudioSource.playOnAwake = false;
            ambienceAudioSource.loop = true;
            ambienceAudioSource.volume = 0.3f;
            ambienceAudioSource.spatialBlend = 0f; // 2D sound
        }
        
        // Load audio clips
        LoadAudioClips();
    }
    
    /// <summary>
    /// Load audio clips for the introduction
    /// </summary>
    private void LoadAudioClips()
    {
        if (!usePeriodAudioEffects)
            return;
            
        // Load period-appropriate ambient audio
        AudioClip ambienceClip = Resources.Load<AudioClip>("Audio/1800s_Portland_Ambience");
        if (ambienceClip != null && ambienceAudioSource != null)
        {
            ambienceAudioSource.clip = ambienceClip;
        }
        else
        {
            Debug.LogWarning("Ambience audio clip not found");
        }
        
        // Load narrative voice-over if available
        AudioClip narrativeClip = Resources.Load<AudioClip>("Audio/Introduction_Narration");
        if (narrativeClip != null && audioSource != null)
        {
            audioSource.clip = narrativeClip;
        }
    }
    
    /// <summary>
    /// Initialize post-processing for cinematic effects
    /// </summary>
    private void InitializePostProcessing()
    {
        if (!useVolumeEffects)
            return;
            
        // Check if we already have a post-processing volume
        postProcessingVolume = GetComponent<Volume>();
        if (postProcessingVolume == null)
        {
            // Create post-processing volume
            postProcessingVolume = gameObject.AddComponent<Volume>();
            postProcessingVolume.isGlobal = true;
            postProcessingVolume.priority = 100;
            
            // Load the introduction profile
            VolumeProfile introProfile = Resources.Load<VolumeProfile>("Profiles/IntroductionProfile");
            if (introProfile != null)
            {
                postProcessingVolume.profile = introProfile;
            }
            else
            {
                // Create a basic profile with period-appropriate effects
                CreatePeriodPostProcessingProfile();
            }
        }
        
        // Start with zero weight (will fade in)
        postProcessingVolume.weight = 0f;
    }
    
    /// <summary>
    /// Create a post-processing profile with period-appropriate effects
    /// </summary>
    private void CreatePeriodPostProcessingProfile()
    {
        // Create a new profile
        VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
        
        // Add vignette for period film look
        if (profile.TryGet<Vignette>(out var vignette))
        {
            vignette.intensity.Override(0.4f);
            vignette.smoothness.Override(0.3f);
            vignette.color.Override(Color.black);
        }
        else
        {
            Vignette newVignette = profile.Add<Vignette>(false);
            newVignette.intensity.Override(0.4f);
            newVignette.smoothness.Override(0.3f);
            newVignette.color.Override(Color.black);
        }
        
        // Add film grain for authentic 1800s photography look
        if (profile.TryGet<FilmGrain>(out var grain))
        {
            grain.intensity.Override(0.3f);
            grain.response.Override(0.8f);
        }
        else
        {
            FilmGrain newGrain = profile.Add<FilmGrain>(false);
            newGrain.intensity.Override(0.3f);
            newGrain.response.Override(0.8f);
        }
        
        // Add subtle color adjustments for period look
        if (profile.TryGet<ColorAdjustments>(out var colorAdjust))
        {
            colorAdjust.saturation.Override(-10f);
            colorAdjust.contrast.Override(10f);
        }
        else
        {
            ColorAdjustments newColorAdjust = profile.Add<ColorAdjustments>(false);
            newColorAdjust.saturation.Override(-10f);
            newColorAdjust.contrast.Override(10f);
        }
        
        // Assign profile to volume
        postProcessingVolume.profile = profile;
    }
    
    /// <summary>
    /// Save the original camera state before takeover
    /// </summary>
    private void SaveOriginalCameraState()
    {
        if (mainCamera != null)
        {
            originalCameraParent = mainCamera.transform.parent;
            originalCameraPosition = mainCamera.transform.localPosition;
            originalCameraRotation = mainCamera.transform.localRotation;
            
            // Find player camera controller
            playerCamera = FindObjectOfType<ThirdPersonCameraController>();
        }
    }
    
    /// <summary>
    /// Restore camera to original state
    /// </summary>
    private void RestoreOriginalCameraState()
    {
        if (mainCamera != null)
        {
            // Return camera to original parent
            mainCamera.transform.SetParent(originalCameraParent);
            mainCamera.transform.localPosition = originalCameraPosition;
            mainCamera.transform.localRotation = originalCameraRotation;
            
            // Re-enable player camera
            if (playerCamera != null)
            {
                playerCamera.enabled = true;
            }
        }
        
        // Cleanup temp camera object
        if (tempCameraObject != null)
        {
            Destroy(tempCameraObject);
        }
    }
    
    /// <summary>
    /// Run the complete introduction sequence
    /// </summary>
    private IEnumerator RunIntroSequence()
    {
        // Set up initial UI state
        SetupInitialUIState();
        
        // Initial black screen
        yield return StartCoroutine(FadeIn(1.5f));
        
        // Start period-appropriate ambience
        if (usePeriodAudioEffects && ambienceAudioSource != null && ambienceAudioSource.clip != null)
        {
            ambienceAudioSource.Play();
        }
        
        // Show title
        yield return StartCoroutine(ShowTitle(2.0f));
        
        // Fade in post-processing effects
        if (useVolumeEffects && postProcessingVolume != null)
        {
            float elapsed = 0f;
            float duration = 1.5f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                postProcessingVolume.weight = Mathf.Lerp(0f, 1f, elapsed / duration);
                yield return null;
            }
            postProcessingVolume.weight = 1f;
        }
        
        // Begin camera tour
        yield return StartCoroutine(CameraWaypointTour());
        
        // Conclude introduction
        yield return StartCoroutine(ConcludeIntroduction());
        
        // Reset controller state
        isIntroRunning = false;
    }
    
    /// <summary>
    /// Setup the initial UI state
    /// </summary>
    private void SetupInitialUIState()
    {
        if (backgroundImage != null)
        {
            // Start with full black
            backgroundImage.color = Color.black;
            backgroundImage.gameObject.SetActive(true);
        }
        
        if (textPanel != null)
        {
            textPanel.SetActive(false);
        }
        
        if (titleText != null)
        {
            // Hide title initially
            Color titleColor = titleText.color;
            titleColor.a = 0f;
            titleText.color = titleColor;
            titleText.gameObject.SetActive(true);
        }
        
        // Update the narrative text
        UpdateNarrativeText(0);
        
        // Hide continue button initially
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinueButtonClicked);
        }
    }
    
    /// <summary>
    /// Fade the background from black
    /// </summary>
    private IEnumerator FadeIn(float duration)
    {
        if (backgroundImage == null)
            yield break;
            
        isFadingIn = true;
        
        // Fade from black to transparent
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            Color color = backgroundImage.color;
            color.a = 1f - t;
            backgroundImage.color = color;
            
            yield return null;
        }
        
        // Ensure fully transparent
        Color finalColor = backgroundImage.color;
        finalColor.a = 0f;
        backgroundImage.color = finalColor;
        
        isFadingIn = false;
    }
    
    /// <summary>
    /// Fade to black
    /// </summary>
    private IEnumerator FadeOut(float duration)
    {
        if (backgroundImage == null)
            yield break;
            
        isFadingOut = true;
        
        // Fade from transparent to black
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            Color color = backgroundImage.color;
            color.a = t;
            backgroundImage.color = color;
            
            yield return null;
        }
        
        // Ensure fully black
        Color finalColor = backgroundImage.color;
        finalColor.a = 1f;
        backgroundImage.color = finalColor;
        
        isFadingOut = false;
    }
    
    /// <summary>
    /// Show the game title with period-appropriate fade in
    /// </summary>
    private IEnumerator ShowTitle(float duration)
    {
        if (titleText == null)
            yield break;
            
        // Fade in title text
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            // Use a slight ease-in function for authentic historical title reveal
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            Color color = titleText.color;
            color.a = smoothT;
            titleText.color = color;
            
            yield return null;
        }
        
        // Ensure full opacity
        Color finalColor = titleText.color;
        finalColor.a = 1f;
        titleText.color = finalColor;
        
        // Wait for title to be read
        yield return new WaitForSeconds(3.0f);
        
        // Fade out title
        elapsed = 0f;
        duration = 1.5f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            Color color = titleText.color;
            color.a = 1f - t;
            titleText.color = color;
            
            yield return null;
        }
        
        // Ensure title is invisible
        Color finalInvisibleColor = titleText.color;
        finalInvisibleColor.a = 0f;
        titleText.color = finalInvisibleColor;
    }
    
    /// <summary>
    /// Tour through camera waypoints showing key locations
    /// </summary>
    private IEnumerator CameraWaypointTour()
    {
        if (cameraWaypoints == null || cameraWaypoints.Length == 0)
        {
            Debug.LogWarning("No camera waypoints specified for introduction tour");
            yield break;
        }
        
        // Take control of camera
        TakeControlOfCamera();
        
        // Show first narrative text
        if (textPanel != null)
        {
            textPanel.SetActive(true);
        }
        
        // Tour each waypoint
        for (int i = 0; i < cameraWaypoints.Length; i++)
        {
            currentWaypointIndex = i;
            
            if (i > 0)
            {
                // Transition between waypoints
                yield return StartCoroutine(TransitionToWaypoint(
                    cameraWaypoints[i-1], cameraWaypoints[i], transitionDuration));
            }
            else
            {
                // First waypoint, just set position
                tempCameraObject.transform.position = cameraWaypoints[0].position;
                tempCameraObject.transform.rotation = cameraWaypoints[0].rotation;
            }
            
            // Update narrative text for this waypoint
            UpdateNarrativeText(i);
            
            // Apply atmospheric effects for each location
            ApplyLocationEffects(i);
            
            // User can press continue to advance, or we wait for the duration
            continueButton.gameObject.SetActive(true);
            
            // Wait for user input or timeout
            float elapsed = 0f;
            bool waitingForInput = true;
            
            while (waitingForInput && elapsed < waypointStayDuration)
            {
                elapsed += Time.deltaTime;
                
                if (!waitingForInput || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    waitingForInput = false;
                    break;
                }
                
                yield return null;
            }
            
            // Hide continue button
            continueButton.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Takes control of the main camera for the introduction sequence
    /// </summary>
    private void TakeControlOfCamera()
    {
        if (mainCamera == null)
            return;
            
        // Disable player camera controller if it exists
        if (playerCamera != null)
        {
            playerCamera.enabled = false;
        }
        
        // Create a temporary object to control camera movement
        tempCameraObject = new GameObject("IntroCameraControl");
        tempCameraObject.transform.position = mainCamera.transform.position;
        tempCameraObject.transform.rotation = mainCamera.transform.rotation;
        
        // Parent the camera to this object
        mainCamera.transform.SetParent(tempCameraObject.transform);
        mainCamera.transform.localPosition = Vector3.zero;
        mainCamera.transform.localRotation = Quaternion.identity;
    }
    
    /// <summary>
    /// Smoothly transitions camera between waypoints
    /// </summary>
    private IEnumerator TransitionToWaypoint(Transform from, Transform to, float duration)
    {
        if (tempCameraObject == null || from == null || to == null)
            yield break;
            
        Vector3 startPosition = from.position;
        Quaternion startRotation = from.rotation;
        Vector3 endPosition = to.position;
        Quaternion endRotation = to.rotation;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            // Use smooth step for more cinematic movement
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            // Update position and rotation
            tempCameraObject.transform.position = Vector3.Lerp(startPosition, endPosition, smoothT);
            tempCameraObject.transform.rotation = Quaternion.Slerp(startRotation, endRotation, smoothT);
            
            yield return null;
        }
        
        // Ensure we arrive exactly at destination
        tempCameraObject.transform.position = endPosition;
        tempCameraObject.transform.rotation = endRotation;
    }
    
    /// <summary>
    /// Apply atmospheric effects specific to each location
    /// </summary>
    private void ApplyLocationEffects(int waypointIndex)
    {
        if (!useParticleEffects || cameraWaypoints == null || waypointIndex >= cameraWaypoints.Length)
            return;
            
        // Different effects based on location
        switch (waypointIndex)
        {
            case 0: // City overview
                // Apply distant city effects - muted sounds, fog
                if (ambienceAudioSource != null)
                {
                    ambienceAudioSource.volume = 0.2f;
                    ambienceAudioSource.pitch = 0.9f;
                }
                break;
                
            case 1: // Street level
                // Apply street-level effects - busier sounds
                if (ambienceAudioSource != null)
                {
                    ambienceAudioSource.volume = 0.5f;
                    ambienceAudioSource.pitch = 1.0f;
                }
                break;
                
            case 2: // Speakeasy entrance
                // Apply speakeasy entrance effects - tense music, focused lighting
                if (ambienceAudioSource != null)
                {
                    ambienceAudioSource.volume = 0.4f;
                    ambienceAudioSource.pitch = 0.95f;
                }
                
                // Trigger door animation if available
                if (speakeasyEntrance != null)
                {
                    speakeasyEntrance.PreviewDoorAnimation();
                }
                break;
                
            case 3: // Storm drain
                // Apply storm drain effects - dripping sounds, eeriness
                if (ambienceAudioSource != null)
                {
                    ambienceAudioSource.volume = 0.3f;
                    ambienceAudioSource.pitch = 0.9f;
                }
                break;
                
            case 4: // Shanghai tunnels
                // Apply tunnel effects - echoing, darkness
                if (ambienceAudioSource != null)
                {
                    ambienceAudioSource.volume = 0.25f;
                    ambienceAudioSource.pitch = 0.85f;
                }
                
                // Trigger tunnel effects if available
                if (shanghaiTunnels != null)
                {
                    shanghaiTunnels.PreviewTunnelEffects();
                }
                break;
                
            case 5: // Player intro
                // Apply character intro effects - heroic subtle theme
                if (ambienceAudioSource != null)
                {
                    ambienceAudioSource.volume = 0.4f;
                    ambienceAudioSource.pitch = 1.0f;
                }
                break;
        }
        
        // Adjust post-processing effects for location
        AdjustPostProcessingForLocation(waypointIndex);
    }
    
    /// <summary>
    /// Adjust post-processing for specific locations
    /// </summary>
    private void AdjustPostProcessingForLocation(int waypointIndex)
    {
        if (!useVolumeEffects || postProcessingVolume == null || postProcessingVolume.profile == null)
            return;
            
        // Get volume components
        if (postProcessingVolume.profile.TryGet<Vignette>(out var vignette) &&
            postProcessingVolume.profile.TryGet<FilmGrain>(out var grain) &&
            postProcessingVolume.profile.TryGet<ColorAdjustments>(out var colorAdjust))
        {
            // Different post-processing settings based on location
            switch (waypointIndex)
            {
                case 0: // City overview - clearer, broader view
                    vignette.intensity.value = 0.3f;
                    grain.intensity.value = 0.2f;
                    colorAdjust.saturation.value = -5f;
                    colorAdjust.contrast.value = 5f;
                    break;
                    
                case 1: // Street level - normal period look
                    vignette.intensity.value = 0.4f;
                    grain.intensity.value = 0.3f;
                    colorAdjust.saturation.value = -10f;
                    colorAdjust.contrast.value = 10f;
                    break;
                    
                case 2: // Speakeasy - warm, intimate lighting
                    vignette.intensity.value = 0.5f;
                    grain.intensity.value = 0.4f;
                    colorAdjust.saturation.value = -5f;
                    colorAdjust.contrast.value = 15f;
                    break;
                    
                case 3: // Storm drain - cold, stark
                    vignette.intensity.value = 0.6f;
                    grain.intensity.value = 0.3f;
                    colorAdjust.saturation.value = -15f;
                    colorAdjust.contrast.value = 10f;
                    break;
                    
                case 4: // Shanghai tunnels - dark, oppressive
                    vignette.intensity
