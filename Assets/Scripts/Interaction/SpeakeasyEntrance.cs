    
    /// <summary>
    /// Enum to differentiate between door sound types
    /// </summary>
    public enum DoorSoundType
    {
        Locked,
        Unlock,
        Open,
        Close,
        Knock
    }
    
    // Inspector variables for door sounds and effects
    [Header("Door Sounds and Effects")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private AudioClip lockedSound;
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private AudioClip knockSound;
    [SerializeField] private ParticleSystem doorDustParticles;
    [SerializeField] private Collider gamblerDetectionTrigger;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PDXUnderground.Interaction
{
    /// <summary>
    /// Manages a historical speakeasy entrance with hidden door mechanics, secret knock system,
    /// atmospheric elements, and connections to the Shanghai Tunnels.
    /// </summary>
    public class SpeakeasyEntrance : MonoBehaviour
    {
        #region Inspector Properties

        [Header("Entrance Properties")]
        [SerializeField] private GameObject entranceFacade;
        [SerializeField] private GameObject hiddenDoor;
        [SerializeField] private Transform doorPivot;
        [SerializeField] private float doorOpenAngle = 85f;
        [SerializeField] private float doorOpenSpeed = 2f;

        [Header("Secret Knock System")]
        [SerializeField] private bool requireSecretKnock = true;
        [SerializeField] private float knockTimeThreshold = 1.5f; // Max time between knocks
        [SerializeField] private List<float> knockPattern = new List<float>() { 0.5f, 0.3f, 0.8f }; // Timing pattern for knocks
        [SerializeField] private float knockTolerance = 0.2f; // Tolerance for timing accuracy
        [SerializeField] private GameObject knockTriggerArea;
        [SerializeField] private AudioClip knockSound;
        [SerializeField] private AudioClip wrongPatternSound;
        [SerializeField] private AudioClip correctPatternSound;

        [Header("Environment")]
        [SerializeField] private List<GameObject> lanterns = new List<GameObject>();
        [SerializeField] private List<Transform> npcSpawnPoints = new List<Transform>();
        [SerializeField] private GameObject secretPassagePrefab;
        [SerializeField] private Transform secretPassageSpawnPoint;
        
        [Header("Audio")]
        [SerializeField] private AudioClip ambientSound;
        [SerializeField] private AudioClip doorOpenSound;
        [SerializeField] private AudioClip doorCloseSound;
        [SerializeField] private float ambientVolume = 0.5f;
        
        [Header("Level Progression")]
        [SerializeField] private Transform tunnelEntrancePoint;
        [SerializeField] private GameObject tunnelEntrancePrefab;
        [SerializeField] private List<CollectibleItem> requiredItems = new List<CollectibleItem>();
        [SerializeField] private bool tunnelAccessRequiresItems = true;
        [SerializeField] private string nextLevelName;
        
        [Header("Events")]
        public UnityEvent OnDoorOpened;
        public UnityEvent OnDoorClosed;
        public UnityEvent OnSecretKnockCorrect;
        public UnityEvent OnSecretKnockIncorrect;
        public UnityEvent OnTunnelEntranceRevealed;
        public UnityEvent OnPlayerEnteredTunnel;

        #endregion

        #region Private Variables

        private bool doorOpen = false;
        private bool tunnelEntranceRevealed = false;
        private AudioSource audioSource;
        private List<float> playerKnockPattern = new List<float>();
        private float lastKnockTime;
        private bool isListeningForKnock = false;
        private GameObject doorJamb;
        private Light doorwayLight;
        private List<GameObject> spawnedNPCs = new List<GameObject>();
        private ParticleSystem dustParticles;
        private Coroutine knockEvaluationCoroutine;
        private bool playerInTriggerArea = false;
        private Quaternion doorClosedRotation;
        private Quaternion doorOpenRotation;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Initialize components
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // Cache door rotations
            if (doorPivot != null)
            {
                doorClosedRotation = doorPivot.rotation;
                doorOpenRotation = doorClosedRotation * Quaternion.Euler(0, doorOpenAngle, 0);
            }
            
            // Configure audio source
            audioSource.loop = true;
            audioSource.volume = ambientVolume;
            audioSource.spatialBlend = 1.0f; // Full 3D
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.maxDistance = 20f;
            
            // Create dust particle system
            CreateDustParticles();
            
            // Configure the knock trigger area
            SetupKnockTriggerArea();
        }

        private void Start()
        {
            // Set initial state
            if (hiddenDoor != null)
            {
                hiddenDoor.SetActive(true);
            }
            
            // Play ambient sound
            if (ambientSound != null)
            {
                audioSource.clip = ambientSound;
                audioSource.Play();
            }
            
            // Initialize lanterns
            ActivateLanterns();
            
            // Create doorjamb light
            CreateDoorwayLight();
            
            // Hide tunnel entrance initially
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
            StopListeningForKnock();
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
            OnDoorClosed?.Invoke();
        }

        /// <summary>
        /// Coroutine to animate the door opening
        /// </summary>
        private IEnumerator AnimateDoorOpen()
        {
            if (doorPivot != null)
            {
                float duration = 1f / doorOpenSpeed;
                float elapsed = 0f;
                
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    doorPivot.rotation = Quaternion.Slerp(doorClosedRotation, doorOpenRotation, t);
                    yield return null;
                }
                
                doorPivot.rotation = doorOpenRotation;
                
                // Enable player passage
                if (doorCollider != null)
                {
                    doorCollider.isTrigger = true;
                }
                
                // Play creaking sound
                PlayDoorSound(DoorSoundType.Open);
                
                // Emit dust particles for period-appropriate effect
                if (doorDustParticles != null)
                {
                    doorDustParticles.Play();
                }
                
                // Handle indoor/outdoor lighting transition
                StartCoroutine(AdjustLightingForDoorState(true, duration));
                
                // Update door state
                currentDoorState = DoorState.Open;
                
                // Notify any listeners
                OnDoorStateChanged?.Invoke(currentDoorState);
                
                yield return new WaitForSeconds(0.5f);
                
                // Allow Gambler to use special abilities through the door
                EnableGamblerPassage();
            }
        }
    }
    
    /// <summary>
    /// Animates the door closing over time
    /// </summary>
    private IEnumerator AnimateDoorClose(float duration = 1.0f)
    {
        if (doorPivot != null)
        {
            // Store original rotation
            Quaternion doorClosedRotation = doorOriginalRotation;
            Quaternion doorCurrentRotation = doorPivot.rotation;
            
            // Disable Gambler passage through door
            DisableGamblerPassage();
            
            // Play creaking sound when starting to close
            PlayDoorSound(DoorSoundType.Close);
            
            // Create dust effect
            if (doorDustParticles != null)
            {
                doorDustParticles.Play();
            }
            
            // Begin lighting transition back to exterior
            StartCoroutine(AdjustLightingForDoorState(false, duration));
            
            // Update door state
            currentDoorState = DoorState.Closing;
            OnDoorStateChanged?.Invoke(currentDoorState);
            
            float elapsed = 0;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // Use smooth step for more realistic door movement
                float smoothT = Mathf.SmoothStep(0, 1, t);
                
                // Rotate door
                doorPivot.rotation = Quaternion.Slerp(doorCurrentRotation, doorClosedRotation, smoothT);
                
                yield return null;
            }
            
            // Ensure door is fully closed
            doorPivot.rotation = doorClosedRotation;
            
            // Re-enable collider to block passage
            if (doorCollider != null)
            {
                doorCollider.isTrigger = false;
            }
            
            // Update door state
            currentDoorState = DoorState.Closed;
            OnDoorStateChanged?.Invoke(currentDoorState);
        }
    }
    
    /// <summary>
    /// Plays appropriate door sound effects based on the action
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
            audioSource.pitch = Random.Range(0.9f, 1.1f); // Slight variation for realism
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
        UnityEngine.Rendering.Volume postProcessVolume = Camera.main?.GetComponent<UnityEngine.Rendering.Volume>();
        
        if (speakeasyLights == null || speakeasyLights.Length == 0)
            yield break;
            
        // Store original light values
        Dictionary<Light, float> originalIntensities = new Dictionary<Light, float>();
        foreach (Light light in speakeasyLights)
        {
            originalIntensities[light] = light.intensity;
        }
        
        // Store original post-processing values
        float originalPostProcessWeight = postProcessVolume != null ? postProcessVolume.weight : 0f;
        
        // Target values based on door state
        float targetPostProcessWeight = isOpening ? 0.7f : 0.3f;  // Indoor vs outdoor post-processing
        
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
            
            // Adjust post-processing for indoor/outdoor transition
            if (postProcessVolume != null)
            {
                postProcessVolume.weight = Mathf.Lerp(originalPostProcessWeight, targetPostProcessWeight, smoothT);
            }
            
            yield return null;
        }
    }
    
    /// <summary>
    /// Enables special interactions for the Gambler character
    /// </summary>
    private void EnableGamblerPassage()
    {
        // Find Gambler character in scene
        GamblerCharacter gambler = FindObjectOfType<GamblerCharacter>();
        
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
        GamblerCharacter gambler = FindObjectOfType<GamblerCharacter>();
        
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
