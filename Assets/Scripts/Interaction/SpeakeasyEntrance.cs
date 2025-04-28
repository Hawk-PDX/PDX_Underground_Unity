using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;  // For Volume and VolumeProfile
using UnityEngine.SceneManagement;
using PDXUnderground.Core.Interfaces;
using PDXUnderground.Core.Models;

// Force error if URP is not enabled
#if !USING_URP
#error "Universal Render Pipeline is not enabled. Please enable it in Project Settings > Graphics"
#endif
namespace PDXUnderground.Interaction
{
    /// <summary>
    /// Manages a historical speakeasy entrance with hidden door mechanics, secret knock system,
    /// atmospheric elements, and connections to the Shanghai Tunnels.
    /// </summary>
    public class SpeakeasyEntrance : MonoBehaviour
    {
        #region Lighting Configuration
        [Header("Lighting")]
        [SerializeField] private VolumeProfile indoorProfile;    // Using URP VolumeProfile
        [SerializeField] private VolumeProfile outdoorProfile;   // Using URP VolumeProfile
        private Volume cameraVolumeComponent;                    // Using URP Volume
        [SerializeField] private float outdoorVolumeWeight = 0.3f;
        [SerializeField] private float indoorVolumeWeight = 0.7f;
        [SerializeField] private Color indoorAmbientColor = new Color(0.4f, 0.4f, 0.6f);
        [SerializeField] private Color outdoorAmbientColor = new Color(0.6f, 0.6f, 0.7f);
        private Coroutine activeVolumeTransition;
        private PDXUnderground.Core.URPCameraSetup cameraSetup;
        #endregion

        #region Enums
        /// <summary>
        /// Enum for door sound types
        /// </summary>
        public enum DoorSoundType
        {
            Locked,
            Unlock,
            Open,
            Close,
            Knock
        }
        /// <summary>
        /// Enum representing the current state of the door
        /// </summary>
        public enum DoorState
        {
            Closed,
            Opening,
            Open,
            Closing
        }

        #endregion

        #region Events
        // Events
        public event System.Action<DoorState> OnDoorStateChanged;
        public UnityEvent OnDoorOpened;
        public UnityEvent OnDoorClosed;
        public UnityEvent OnSecretKnockCorrect;
        public UnityEvent OnSecretKnockIncorrect;
        public UnityEvent OnPlayerEnteredTunnel;
        #endregion

        #region State
        // Current door state
        private DoorState currentDoorState = DoorState.Closed;
        #endregion

        #region Fields and Properties
        // Door components
        [Header("Door Components")]
        [SerializeField] private GameObject hiddenDoor;
        [SerializeField] private Transform doorPivot;
        [SerializeField] private float doorOpenAngle = 90f;
        [SerializeField] private float doorOpenSpeed = 2f;
        [SerializeField] private Collider doorCollider;
        private Quaternion doorClosedRotation;
        private Quaternion doorOpenRotation;
        private bool doorOpen = false;

        // Audio
        [Header("Audio")]
        [SerializeField] private AudioClip ambientSound;
        [SerializeField] private float ambientVolume = 0.3f;
        [SerializeField] private AudioClip knockSound;
        [SerializeField] private AudioClip doorOpenSound;
        [SerializeField] private AudioClip doorCloseSound;
        [SerializeField] private AudioClip lockedSound;
        [SerializeField] private AudioClip unlockSound;
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;
        [SerializeField] private AudioClip correctPatternSound;
        [SerializeField] private AudioClip wrongPatternSound;
        private AudioSource audioSource;

        // Knock system
        [Header("Knock System")]
        [SerializeField] private bool requireSecretKnock = true;
        [SerializeField] private List<float> knockPattern = new List<float> { 0.5f, 0.3f, 0.8f };
        [SerializeField] private float knockTolerance = 0.2f;
        [SerializeField] private float knockTimeThreshold = 2f;
        private List<float> playerKnockPattern = new List<float>();
        private bool isListeningForKnock = false;
        private float lastKnockTime = 0f;
        private Coroutine knockEvaluationCoroutine;
        private GameObject knockTriggerArea;
        private bool playerInTriggerArea = false;

        // Visual effects
        [Header("Visual Effects")]
        [SerializeField] private GameObject[] lanterns;
        [SerializeField] private GameObject speakeasyRoot;
        [SerializeField] private ParticleSystem doorDustParticles;
        private ParticleSystem dustParticles;
        private Light doorwayLight;

        // Level transition
        [Header("Level Transition")]
        [SerializeField] private GameObject tunnelEntrancePrefab;
        [SerializeField] private Transform tunnelEntrancePoint;
        [SerializeField] private string nextLevelName = "ShanghaiTunnels";
        [SerializeField] private bool tunnelEntranceRevealed = false;
        [SerializeField] private bool tunnelAccessRequiresItems = false;
        [SerializeField] private List<CollectibleItem> requiredItems;

        // Gambler character integration
        [Header("Gambler Integration")]
        [SerializeField] private Collider gamblerDetectionTrigger;

        #endregion

        #region Unity Lifecycle Methods

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            // Get camera setup for URP post-processing transitions
            cameraSetup = Camera.main?.GetComponent<PDXUnderground.Core.URPCameraSetup>();
            if (cameraSetup == null)
            {
                Debug.LogWarning("URPCameraSetup not found on main camera. Lighting transitions may not have proper post-processing.");
            }

            // Cache door rotations
            if (doorPivot != null)
            {
                doorClosedRotation = doorPivot.rotation;
                doorOpenRotation = doorClosedRotation * Quaternion.Euler(0, doorOpenAngle, 0);
            }

            if (audioSource != null)
            {
                // Configure audio source
                audioSource.loop = true;
                audioSource.volume = ambientVolume;
                audioSource.spatialBlend = 1.0f; // Full 3D
                audioSource.rolloffMode = AudioRolloffMode.Linear;
                audioSource.maxDistance = 20f;
            }

            // Initialize lighting system
            // Initialize lighting system
            InitializeVolume();
        }

        /// <summary>
        /// Initialize the URP Volume component
        /// </summary>
        private void InitializeVolume()
        {
            cameraVolumeComponent = Camera.main?.GetComponent<Volume>();
            if (cameraVolumeComponent == null)
            {
                Debug.LogWarning("Volume component not found on main camera. Searching for global volume...");

                // Try to find any Volume in the scene (might be on a global volume GameObject)
                Volume[] volumes = FindObjectsOfType<Volume>();
                if (volumes != null && volumes.Length > 0)
                {
                    cameraVolumeComponent = volumes[0];
                    Debug.Log($"Found alternative Volume component on '{cameraVolumeComponent.gameObject.name}'. Using this for transitions.");
                }
                else
                {
                    GameObject volumeObject = new GameObject("Global Volume");
                    cameraVolumeComponent = volumeObject.AddComponent<Volume>();
                    cameraVolumeComponent.isGlobal = true;

                    // Set default profile if available
                    if (outdoorProfile != null)
                    {
                        cameraVolumeComponent.profile = outdoorProfile;
                    }
                    else
                    {
                        Debug.LogWarning("Outdoor volume profile not assigned. Using an empty profile.");
                    }
                }
            }

            // Validate URP profiles
            if (indoorProfile == null)
            {
                Debug.LogWarning("Indoor volume profile not assigned. Lighting transitions may not work correctly.");
            }
        }
        private void Start()
        {
            // Create visual effects if not set in Inspector
            if (dustParticles == null)
            {
                CreateDustParticles();
            }

            if (doorwayLight == null)
            {
                CreateDoorwayLight();
            }

            // Initialize knock trigger area
            SetupKnockTriggerArea();

            // Initialize tunnel entrance
            InitializeTunnelEntrance();

            // Activate lanterns
            ActivateLanterns();
        }

        private void InitializeTunnelEntrance()
        {
            if (tunnelEntrancePrefab != null && tunnelEntrancePoint != null)
            {
                GameObject tunnelEntrance = Instantiate(tunnelEntrancePrefab, tunnelEntrancePoint.position, tunnelEntrancePoint.rotation);
                tunnelEntrance.SetActive(false);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInTriggerArea = true;

                // Show interaction prompt if player is near door
                if (knockTriggerArea != null && knockTriggerArea.activeSelf)
                {
                    ShowInteractionPrompt(true, "Press E to knock");
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInTriggerArea = false;
                ShowInteractionPrompt(false);

                // Reset the knock system when player leaves
                StopListeningForKnock();
            }
        }

        private void Update()
        {
            // Check for player interaction
            if (playerInTriggerArea)
            {
                // Knock interaction
                if (Input.GetKeyDown(KeyCode.E) && !doorOpen)
                {
                    HandleKnock();
                }

                // Check for door interaction when open
                if (doorOpen && Input.GetKeyDown(KeyCode.E) && Vector3.Distance(hiddenDoor.transform.position, Camera.main.transform.position) < 2f)
                {
                    EnterSpeakeasy();
                }
            }
        }

        private void OnDestroy()
        {
            // Clean up any active coroutines
            if (activeVolumeTransition != null)
            {
                StopCoroutine(activeVolumeTransition);
                activeVolumeTransition = null;
            }

            // Clean up references to prevent memory leaks
            cameraVolumeComponent = null;
        }

        #endregion

        #region Knock System

        /// <summary>
        /// Setup the trigger area for knocking
        /// </summary>
        private void SetupKnockTriggerArea()
        {
            if (knockTriggerArea == null)
            {
                knockTriggerArea = new GameObject("KnockTriggerArea");
                knockTriggerArea.transform.SetParent(transform);
                knockTriggerArea.transform.localPosition = new Vector3(0, 1f, 0.5f);

                BoxCollider triggerCollider = knockTriggerArea.AddComponent<BoxCollider>();
                triggerCollider.isTrigger = true;
                triggerCollider.size = new Vector3(1.5f, 2f, 0.5f);
            }
        }

        /// <summary>
        /// Handle player knock interaction
        /// </summary>
        private void HandleKnock()
        {
            // Play knock sound
            if (knockSound != null)
            {
                AudioSource.PlayClipAtPoint(knockSound, hiddenDoor.transform.position);
            }

            // Visual feedback
            if (dustParticles != null)
            {
                dustParticles.Play();
            }

            // If we need a secret knock pattern
            if (requireSecretKnock)
            {
                // Record the knock timing
                float currentTime = Time.time;

                // If this is the first knock or we restarted the pattern
                if (!isListeningForKnock)
                {
                    StartListeningForKnock();
                }
                else
                {
                    // Calculate time since last knock
                    float timeSinceLastKnock = currentTime - lastKnockTime;

                    // Add to player pattern
                    playerKnockPattern.Add(timeSinceLastKnock);
                }

                // Update last knock time
                lastKnockTime = currentTime;

                // If we have enough knocks to evaluate, check if the pattern is correct
                if (playerKnockPattern.Count >= knockPattern.Count)
                {
                    EvaluateKnockPattern();
                }
            }
            else
            {
                // No secret knock required, just open door
                OpenDoor();
            }
        }

        /// <summary>
        /// Start listening for the knock pattern
        /// </summary>
        private void StartListeningForKnock()
        {
            isListeningForKnock = true;
            playerKnockPattern.Clear();
            lastKnockTime = Time.time;

            // Start the knock timeout coroutine
            if (knockEvaluationCoroutine != null)
            {
                StopCoroutine(knockEvaluationCoroutine);
            }
            knockEvaluationCoroutine = StartCoroutine(KnockTimeoutEvaluation());
        }

        /// <summary>
        /// Stop listening for knock pattern
        /// </summary>
        private void StopListeningForKnock()
        {
            isListeningForKnock = false;
            playerKnockPattern.Clear();

            if (knockEvaluationCoroutine != null)
            {
                StopCoroutine(knockEvaluationCoroutine);
                knockEvaluationCoroutine = null;
            }
        }

        /// <summary>
        /// Coroutine to evaluate the knock pattern after a timeout
        /// </summary>
        private IEnumerator KnockTimeoutEvaluation()
        {
            // Wait for the knock timeout
            yield return new WaitForSeconds(knockTimeThreshold);

            // If we're still listening, evaluate the pattern
            if (isListeningForKnock && playerKnockPattern.Count > 0)
            {
                EvaluateKnockPattern();
            }
        }

        /// <summary>
        /// Evaluate if the player's knock pattern matches the secret pattern
        /// </summary>
        private void EvaluateKnockPattern()
        {
            bool patternCorrect = true;

            // If patterns have different lengths, they're not equal
            if (playerKnockPattern.Count != knockPattern.Count)
            {
                patternCorrect = false;
            }
            else
            {
                // Skip first knock since it's the start time
                for (int i = 1; i < playerKnockPattern.Count; i++)
                {
                    // Check if the time between knocks matches within tolerance
                    if (Mathf.Abs(playerKnockPattern[i] - knockPattern[i]) > knockTolerance)
                    {
                        patternCorrect = false;
                        break;
                    }
                }
            }

            // Handle result
            if (patternCorrect)
            {
                OnCorrectKnockPattern();
            }
            else
            {
                OnIncorrectKnockPattern();
            }

            // Reset for next attempt
        }

        /// <summary>
        /// Handle correct knock pattern
        /// </summary>
        private void OnCorrectKnockPattern()
        {
            // Play success sound
            if (correctPatternSound != null)
            {
                AudioSource.PlayClipAtPoint(correctPatternSound, hiddenDoor.transform.position);
            }

            Debug.Log("Correct knock pattern!");
            OnSecretKnockCorrect?.Invoke();

            // Open the door
            OpenDoor();
        }

        /// <summary>
        /// Handle incorrect knock pattern
        /// </summary>
        private void OnIncorrectKnockPattern()
        {
            // Play failure sound
            if (wrongPatternSound != null)
            {
                AudioSource.PlayClipAtPoint(wrongPatternSound, hiddenDoor.transform.position);
            }

            Debug.Log("Incorrect knock pattern! Try again.");
            OnSecretKnockIncorrect?.Invoke();

            // Visual feedback - maybe shake the door slightly
            StartCoroutine(ShakeDoor());
        }

        #endregion

        #region Door Mechanics

        /// <summary>
        /// Open the hidden door
        /// </summary>
        public void OpenDoor()
        {
            if (doorOpen)
                return;

            doorOpen = true;

            // Animate door opening
            StartCoroutine(AnimateDoorOpen());

            // Play sound
            if (doorOpenSound != null)
            {
                AudioSource.PlayClipAtPoint(doorOpenSound, hiddenDoor.transform.position);
            }

            // Turn on doorway light
            // Turn on doorway light
            if (doorwayLight != null)
            {
                doorwayLight.enabled = true;
            }

            // Fire event
            OnDoorOpened?.Invoke();

            // Show interaction prompt
            if (playerInTriggerArea)
            {
                ShowInteractionPrompt(true, "Press E to enter");
            }
        }
        /// <summary>
        /// Shows an interaction prompt to the player
        /// </summary>
        private void ShowInteractionPrompt(bool show, string text = "")
        {
            // In a full implementation, this would display UI prompt
            // For now, we'll just log to console
            if (show)
            {
                Debug.Log("Interaction prompt: " + text);
            }
        }

        /// <summary>
        /// Close the hidden door
        /// </summary>
        public void CloseDoor()
        {
            if (!doorOpen)
                return;

            doorOpen = false;
            // Animate door closing
            StartCoroutine(AnimateDoorClose());

            // Play sound
            if (doorCloseSound != null)
            {
                AudioSource.PlayClipAtPoint(doorCloseSound, hiddenDoor.transform.position);
            }

            // Turn off doorway light
            if (doorwayLight != null)
            {
                doorwayLight.enabled = false;
            }

            // Fire event
            // Fire event
            OnDoorClosed?.Invoke();
        }
        /// <summary>
        /// Animates the door opening
        /// </summary>
        private IEnumerator AnimateDoorOpen()
        {
            if (doorPivot != null)
            {
                float duration = 1f / doorOpenSpeed;
                float elapsed = 0f;

                currentDoorState = DoorState.Opening;
                OnDoorStateChanged?.Invoke(currentDoorState);

                // Play door sound
                PlayDoorSound(DoorSoundType.Open);

                // Create dust particles for effect
                if (doorDustParticles != null)
                {
                    doorDustParticles.Play();
                }

                // Start interior lighting transition
                StartCoroutine(AdjustLightingForDoorState(true, duration));

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;

                    doorPivot.rotation = Quaternion.Lerp(doorClosedRotation, doorOpenRotation, t);

                    yield return null;
                }

                doorPivot.rotation = doorOpenRotation;

                // Enable player passage
                if (doorCollider != null)
                {
                    doorCollider.isTrigger = true;
                }

                currentDoorState = DoorState.Open;
                OnDoorStateChanged?.Invoke(currentDoorState);

                // Enable special Gambler interaction
                EnableGamblerPassage();
            }
        }

        /// <summary>
        /// Animates the door closing
        /// </summary>
        private IEnumerator AnimateDoorClose()
        {
            if (doorPivot != null)
            {
                float duration = 1f / doorOpenSpeed;
                float elapsed = 0f;

                // Play door sound
                PlayDoorSound(DoorSoundType.Close);

                // Create dust particles for effect
                if (doorDustParticles != null)
                {
                    doorDustParticles.Play();
                }

                // Disable Gambler passage
                DisableGamblerPassage();

                // Start exterior lighting transition
                StartCoroutine(AdjustLightingForDoorState(false, duration));

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;

                    doorPivot.rotation = Quaternion.Lerp(doorOpenRotation, doorClosedRotation, t);

                    yield return null;
                }

                doorPivot.rotation = doorClosedRotation;

                // Disable player passage
                if (doorCollider != null)
                {
                    doorCollider.isTrigger = false;
                }

                currentDoorState = DoorState.Closed;
                OnDoorStateChanged?.Invoke(currentDoorState);
            }
        }

        /// <summary>
        /// Shakes the door as visual feedback
        /// </summary>
        private IEnumerator ShakeDoor()
        {

            Quaternion originalRotation = doorPivot.rotation;
            float shakeTime = 0.5f;
            float elapsed = 0f;

            while (elapsed < shakeTime)
            {
                elapsed += Time.deltaTime;
                float shake = Mathf.Sin(elapsed * 30f) * 0.5f * (1f - elapsed / shakeTime);
                doorPivot.rotation = originalRotation * Quaternion.Euler(0, shake * 2f, 0);

                yield return null;
            }

            // Reset to original rotation
            doorPivot.rotation = originalRotation;
        }

        /// <summary>
        /// Play a door-related sound
        /// </summary>
        private void PlayDoorSound(DoorSoundType soundType)
        {
            if (audioSource == null)
                return;

            AudioClip clipToPlay = null;

            switch (soundType)
            {
                case DoorSoundType.Locked:
                    clipToPlay = lockedSound;
                    break;

                case DoorSoundType.Unlock:
                    clipToPlay = unlockSound;
                    break;

                case DoorSoundType.Open:
                    clipToPlay = openSound;
                    break;

                case DoorSoundType.Close:
                    clipToPlay = closeSound;
                    break;

                case DoorSoundType.Knock:
                    clipToPlay = knockSound;
                    break;
            }

            if (clipToPlay != null)
            {
                audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f); // Slight variation for realism
                audioSource.PlayOneShot(clipToPlay);
            }
        }

        /// <summary>
        /// Handles lighting transition between indoor/outdoor when door opens/closes
        /// </summary>
        private IEnumerator AdjustLightingForDoorState(bool isOpening, float duration)
        {
            // Get references to relevant lighting components
            Light[] speakeasyLights = speakeasyRoot?.GetComponentsInChildren<Light>();

            // Check for speakeasy lights
            if (speakeasyLights == null || speakeasyLights.Length == 0)
                yield break;

            // Store original light values
            Dictionary<Light, float> originalIntensities = new Dictionary<Light, float>();
            foreach (Light light in speakeasyLights)
            {
                originalIntensities[light] = light.intensity;
            }
            // Store original lighting values
            // Store original volume weight
            float originalVolumeWeight = 0f;

            // Switch URP camera profile first for quick effect
            if (cameraSetup != null)
            {
                cameraSetup.SwitchToEnvironment(isOpening ?
                    PDXUnderground.Core.URPCameraSetup.EnvironmentType.Speakeasy :
                    PDXUnderground.Core.URPCameraSetup.EnvironmentType.Streets);
            }

            // Use cached Volume component for gradual transition
            if (cameraVolumeComponent != null)
            {
                originalVolumeWeight = cameraVolumeComponent.weight;

                if (cameraVolumeComponent.profile == null)
                {
                    Debug.LogWarning("Volume component on main camera has no profile assigned. Lighting transition will only affect lights, not post-processing.");
                }
            }
            else
            {
                // Try to get the component again in case it was added after initialization
                // Try to get the component again in case it was added after initialization
                cameraVolumeComponent = Camera.main?.GetComponent<Volume>();
                if (cameraVolumeComponent != null)
                {
                    originalVolumeWeight = cameraVolumeComponent.weight;

                    if (cameraVolumeComponent.profile == null)
                    {
                        Debug.LogWarning("Volume component on main camera has no profile assigned. Lighting transition will only affect lights, not post-processing.");
                    }
                }
                else
                {
                    Debug.LogWarning("Volume component not found on main camera. Lighting transition will only affect lights, not post-processing.");
                }
            }

            // Target values based on door state
            // For indoor scenes (when door is opening), we use a higher weight (0.7) to enhance atmospheric effects
            // For outdoor scenes (when door is closing), we use a lower weight (0.3) for a more natural look
            float targetVolumeWeight = isOpening ? indoorVolumeWeight : outdoorVolumeWeight;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float smoothT = Mathf.SmoothStep(0, 1, t);

                // Adjust speakeasy lights based on door state
                foreach (Light light in speakeasyLights)
                {
                    if (isOpening)
                    {
                        // When opening, lights gradually come on
                        light.intensity = Mathf.Lerp(0.2f * originalIntensities[light], originalIntensities[light], smoothT);
                    }
                    else
                    {
                        // When closing, lights gradually dim
                        light.intensity = Mathf.Lerp(originalIntensities[light], 0.2f * originalIntensities[light], smoothT);
                    }
                }

                // Adjust Volume weight for indoor/outdoor transition
                if (cameraVolumeComponent != null)
                {
                    // URP Volume weight controls the influence of the post-processing effects
                    // Higher weight (0.7) for indoor scene to enhance indoor atmospheric effects
                    // Lower weight (0.3) for outdoor scene for a more natural outdoor look
                    cameraVolumeComponent.weight = Mathf.Lerp(originalVolumeWeight, targetVolumeWeight, smoothT);
                }
                yield return null;
            }

            // Ensure final state is set correctly
            if (cameraVolumeComponent != null)
            {
                cameraVolumeComponent.weight = targetVolumeWeight;
            }
        }

        #endregion

        #region Character Interactions
        /// <summary>
        /// Enables special interactions for the Gambler character
        /// </summary>
        private void EnableGamblerPassage()
        {
            // Find Gambler character in scene
            IGamblerCharacter gambler = FindObjectOfType<MonoBehaviour>() as IGamblerCharacter;
            if (gambler != null)
            {
                // Notify Gambler that this entrance is now available
                SendMessage("OnSpeakeasyEntranceAvailable", this, SendMessageOptions.DontRequireReceiver);

                // Optional: Apply any Gambler-specific effects
                if (gamblerDetectionTrigger != null)
                {
                    gamblerDetectionTrigger.enabled = true;
                }
            }
        }

        /// <summary>
        /// Disables special interactions for the Gambler character
        /// </summary>
        private void DisableGamblerPassage()
        {
            // Find Gambler character in scene
            IGamblerCharacter gambler = FindObjectOfType<MonoBehaviour>() as IGamblerCharacter;
            if (gambler != null)
            {
                // Notify Gambler that this entrance is now unavailable
                SendMessage("OnSpeakeasyEntranceClosed", this, SendMessageOptions.DontRequireReceiver);

                // Optional: Remove any Gambler-specific effects
                if (gamblerDetectionTrigger != null)
                {
                    gamblerDetectionTrigger.enabled = false;
                }
            }
        }
        
        #endregion // Character Interactions
        
        #region Visual Effects
        
        /// <summary>
        /// Creates dust particles for door effects
        /// </summary>
        private void CreateDustParticles()
        {
            GameObject particleObj = new GameObject("DoorDustParticles");
            particleObj.transform.SetParent(doorPivot);
            particleObj.transform.localPosition = Vector3.zero;

            dustParticles = particleObj.AddComponent<ParticleSystem>();
            var main = dustParticles.main;
            main.duration = 1f;
            main.loop = false;

            // Configure particle system for dust effect
            var emission = dustParticles.emission;
            emission.rateOverTime = 20f;

            var shape = dustParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(0.5f, 2f, 0.1f);
        }

        /// <summary>
        /// Creates a light source for the doorway
        /// </summary>
        private void CreateDoorwayLight()
        {
            if (doorwayLight == null)
            {
                GameObject lightObj = new GameObject("DoorwayLight");
                lightObj.transform.SetParent(doorPivot);
                lightObj.transform.localPosition = new Vector3(0, 2f, 0);

                doorwayLight = lightObj.AddComponent<Light>();
                doorwayLight.type = LightType.Point;
                doorwayLight.color = new Color(1f, 0.95f, 0.8f); // Warm light
                doorwayLight.intensity = 1.5f;
                doorwayLight.range = 5f;
                doorwayLight.enabled = false;
            }
        }

        /// <summary>
        /// Activates lanterns in the environment
        /// </summary>
        private void ActivateLanterns()
        {
            foreach (GameObject lantern in lanterns)
            {
                if (lantern != null)
                {
                    lantern.SetActive(true);
                    Light lanternLight = lantern.GetComponentInChildren<Light>();
                    if (lanternLight != null)
                    {
                        lanternLight.enabled = true;
                    }
                }
            }
        }
        
        #endregion // Visual Effects

        #region Interaction Methods
        
        /// <summary>
        /// Handles player entering the speakeasy
        /// </summary>
        private void EnterSpeakeasy()
        {
            // Handle player entering the speakeasy
            if (tunnelEntranceRevealed)
            {
                // If tunnel entrance is revealed, allow transition
                if (tunnelAccessRequiresItems)
                {
                    bool hasAllItems = true;
                    foreach (CollectibleItem item in requiredItems)
                    {
                        // Replace with your actual inventory system
                        // if (!PlayerInventory.Instance.HasItem(item.itemId))
                        // {
                        //     hasAllItems = false;
                        //     break;
                        // }
                    }

                    if (!hasAllItems)
                    {
                        // Play locked sound and show message
                        PlayDoorSound(DoorSoundType.Locked);
                        Debug.Log("You need all required items to enter the tunnels.");
                        return;
                    }
                }

                // Trigger level transition
                OnPlayerEnteredTunnel?.Invoke();
                SceneManager.LoadScene(nextLevelName);
            }
        }
        
        #endregion // Interaction Methods
    }
} // end of namespace
