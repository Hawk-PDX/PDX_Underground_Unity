using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using PDXUnderground.Core;
using PDXUnderground.Effects;
using PDXUnderground.AI.Enemies;

namespace PDXUnderground.Interaction
{
    /// <summary>
    /// Manages the Shanghai Tunnels system with secret entrances/exits, hidden pathways,
    /// period-appropriate environmental features, and level progression mechanics.
    /// Implements historically accurate elements from Portland's underground tunnels used 
    /// during the gold rush / westward expansion era.
    /// </summary>
    public class ShanghaiTunnels : MonoBehaviour
    {
        #region Inspector Properties

        [Header("Tunnel Structure")]
        [SerializeField] private GameObject tunnelRoot;
        [SerializeField] private List<GameObject> tunnelSegments = new List<GameObject>();
        [SerializeField] private List<TunnelEntrance> entrances = new List<TunnelEntrance>();
        [SerializeField] private List<TunnelExit> exits = new List<TunnelExit>();
        [SerializeField] private List<HiddenPathway> hiddenPathways = new List<HiddenPathway>();
        [SerializeField] private GameObject speakeasyConnectionPoint;

        [Header("Environmental Features")]
        [SerializeField] private List<GameObject> lanterns = new List<GameObject>();
        [SerializeField] private List<GameObject> interactiveObjects = new List<GameObject>();
        [SerializeField] private GameObject waterPuddlePrefab;
        [SerializeField] private GameObject ratPrefab;
        [SerializeField] private int maxRats = 8;
        [SerializeField] private float ratSpawnInterval = 45f;
        [SerializeField] private GameObject cobwebPrefab;
        [SerializeField] private GameObject debrisPrefab;
        [SerializeField] private Material dampWallMaterial;
        [SerializeField] private Material woodenBeamMaterial;

        [Header("Lighting")]
        [SerializeField] private Color lanternColor = new Color(1.0f, 0.7f, 0.3f);
        [SerializeField] private float lanternIntensity = 1.2f;
        [SerializeField] private float lanternRange = 8f;
        [SerializeField] private Light mainAmbientLight;
        [SerializeField] private float mainLightIntensity = 0.2f;

        [Header("Audio")]
        [SerializeField] private AudioClip ambientSound;
        [SerializeField] private AudioClip waterDripSound;
        [SerializeField] private AudioClip woodCreakSound;
        [SerializeField] private AudioClip ratSquealSound;
        [SerializeField] private float ambientVolume = 0.5f;
        [SerializeField] private float randomSoundInterval = 20f;

        [Header("Collectibles")]
        [SerializeField] private List<CollectibleItem> availableCollectibles = new List<CollectibleItem>();
        [SerializeField] private int requiredCollectiblesCount = 3;
        [SerializeField] private Transform[] collectibleSpawnPoints;

        [Header("Level Progression")]
        [SerializeField] private List<Checkpoint> checkpoints = new List<Checkpoint>();
        [SerializeField] private string nextLevelName;
        
        /// <summary>
        /// When true, all collectibles must be found to complete the level.
        /// Used in CheckLevelCompletion to determine completion criteria.
        /// </summary>
        [SerializeField] private bool requireAllCollectibles = true;
        [SerializeField] private Transform levelEndPoint;
        
        /// <summary>
        /// Delay in seconds before transitioning to the next level after completion.
        /// Used in LoadNextLevelAfterDelay when player completes the level.
        /// </summary>
        [SerializeField] private float levelTransitionDelay = 1.5f;
        [Header("Events")]
        public UnityEvent OnTunnelEntered;
        public UnityEvent OnTunnelExited;
        public UnityEvent OnHiddenPathwayDiscovered;
        public UnityEvent OnCheckpointReached;
        public UnityEvent OnCollectibleFound;
        public UnityEvent OnAllCollectiblesFound;
        public UnityEvent OnLevelComplete;

        #endregion

        #region Private Variables

        private List<GameObject> spawnedRats = new List<GameObject>();
        private List<CollectibleItem> collectedItems = new List<CollectibleItem>();
        private AudioSource ambientAudioSource;
        private AudioSource effectsAudioSource;
        private Checkpoint currentCheckpoint;
        private int currentSegmentIndex = 0;
        private bool playerInTunnels = false;
        
        /// <summary>
        /// Tracks whether all required collectibles have been found.
        /// Set to true in RegisterCollectedItem when requiredCollectiblesFound reaches requiredCollectiblesCount.
        /// </summary>
        private bool allCollectiblesFound = false;
        
        /// <summary>
        /// Tracks whether the level has been completed.
        /// Set to true in CompleteLevel method and prevents multiple completion triggers.
        /// </summary>
        private bool levelCompleted = false;
        private Coroutine ratSpawningCoroutine;
        private Coroutine ambientSoundCoroutine;
        private int requiredCollectiblesFound = 0;
        private Dictionary<HiddenPathway, bool> discoveredPathways = new Dictionary<HiddenPathway, bool>();
        private Dictionary<TunnelEntrance, bool> unlockedEntrances = new Dictionary<TunnelEntrance, bool>();
        private Dictionary<TunnelExit, bool> unlockedExits = new Dictionary<TunnelExit, bool>();

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Initialize audio sources
            ambientAudioSource = gameObject.AddComponent<AudioSource>();
            ambientAudioSource.loop = true;
            ambientAudioSource.volume = ambientVolume;
            ambientAudioSource.spatialBlend = 1.0f;
            ambientAudioSource.priority = 0;
            
            effectsAudioSource = gameObject.AddComponent<AudioSource>();
            effectsAudioSource.loop = false;
            effectsAudioSource.spatialBlend = 1.0f;
            
            // Initialize pathways and entrances dictionaries
            foreach (HiddenPathway pathway in hiddenPathways)
            {
                discoveredPathways[pathway] = false;
                if (pathway.pathwayObject != null)
                {
                    pathway.pathwayObject.SetActive(false);
                }
            }
            
            foreach (TunnelEntrance entrance in entrances)
            {
                unlockedEntrances[entrance] = entrance.startUnlocked;
                if (entrance.entranceObject != null && !entrance.startUnlocked)
                {
                    entrance.entranceObject.SetActive(false);
                }
            }
            
            foreach (TunnelExit exit in exits)
            {
                unlockedExits[exit] = exit.startUnlocked;
                if (exit.exitObject != null && !exit.startUnlocked)
                {
                    exit.exitObject.SetActive(false);
                }
            }
            
            // Set tunnel to be inactive by default until player enters
            if (tunnelRoot != null)
            {
                tunnelRoot.SetActive(false);
            }
        }

        private void Start()
        {
            SetupEnvironment();
            
            // Set up speakeasy connection
            if (speakeasyConnectionPoint != null)
            {
                SpeakeasyEntrance speakeasy = FindObjectOfType<SpeakeasyEntrance>();
                if (speakeasy != null)
                {
                    // Connect events between speakeasy and tunnels
                    speakeasy.OnPlayerEnteredTunnel.AddListener(OnPlayerEnteredFromSpeakeasy);
                }
            }
            
            // Place collectibles at random spawn points
            SpawnCollectibles();
            
            // Play ambient sounds
            if (ambientSound != null)
            {
                ambientAudioSource.clip = ambientSound;
                ambientAudioSource.Play();
            }
            
            // Start ambient sound effects coroutine
            ambientSoundCoroutine = StartCoroutine(PlayRandomAmbientSounds());
        }

        private void OnDestroy()
        {
            // Clean up coroutines when destroyed
            if (ratSpawningCoroutine != null)
            {
                StopCoroutine(ratSpawningCoroutine);
                ratSpawningCoroutine = null;
            }
            
            if (ambientSoundCoroutine != null)
            {
                StopCoroutine(ambientSoundCoroutine);
                ambientSoundCoroutine = null;
            }
            
            // Clean up spawned rats
            foreach (GameObject rat in spawnedRats)
            {
                if (rat != null)
                {
                    Destroy(rat);
                }
            }
            spawnedRats.Clear();
            
            // Clean up audio
            if (ambientAudioSource != null)
            {
                ambientAudioSource.Stop();
                ambientAudioSource = null;
            }
            
            if (effectsAudioSource != null)
            {
                effectsAudioSource.Stop();
                effectsAudioSource = null;
            }
        }

        #endregion

        #region Tunnel Structure Methods

        /// <summary>
        /// Activates the tunnel system when player enters
        /// </summary>
        public void EnterTunnels()
        {
            if (tunnelRoot != null && !playerInTunnels)
            {
                tunnelRoot.SetActive(true);
                playerInTunnels = true;
                OnTunnelEntered?.Invoke();
                
                // Start spawning rats
                if (ratSpawningCoroutine == null)
                {
                    ratSpawningCoroutine = StartCoroutine(SpawnRatsRandomly());
                }
                
                // Activate the first segment
                ActivateTunnelSegment(0);
                
                Debug.Log("Player has entered the Shanghai Tunnels");
            }
        }

        /// <summary>
        /// Deactivates the tunnel system when player exits
        /// </summary>
        public void ExitTunnels()
        {
            if (tunnelRoot != null && playerInTunnels)
            {
                playerInTunnels = false;
                OnTunnelExited?.Invoke();
                
                // Stop spawning rats when player leaves
                if (ratSpawningCoroutine != null)
                {
                    StopCoroutine(ratSpawningCoroutine);
                    ratSpawningCoroutine = null;
                }
                
                Debug.Log("Player has exited the Shanghai Tunnels");
            }
        }

        /// <summary>
        /// Called when player enters from the speakeasy
        /// </summary>
        public void OnPlayerEnteredFromSpeakeasy()
        {
            EnterTunnels();
            
            // Move player to first segment start position
            if (tunnelSegments.Count > 0 && tunnelSegments[0] != null)
            {
                Transform startPoint = tunnelSegments[0].transform.Find("StartPoint");
                if (startPoint != null && Camera.main != null)
                {
                    // Get player controller
                    GameObject player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        player.transform.position = startPoint.position;
                        player.transform.rotation = startPoint.rotation;
                    }
                }
            }
        }

        /// <summary>
        /// Activates a specific tunnel segment and deactivates others
        /// </summary>
        public void ActivateTunnelSegment(int segmentIndex)
        {
            if (segmentIndex >= 0 && segmentIndex < tunnelSegments.Count)
            {
                for (int i = 0; i < tunnelSegments.Count; i++)
                {
                    if (tunnelSegments[i] != null)
                    {
                        // Only activate the current segment and adjacent ones for seamless transitions
                        bool shouldActivate = (i == segmentIndex || i == segmentIndex - 1 || i == segmentIndex + 1);
                        tunnelSegments[i].SetActive(shouldActivate);
                    }
                }
                
                currentSegmentIndex = segmentIndex;
                
                Debug.Log($"Activated tunnel segment {segmentIndex}");
            }
        }

        /// <summary>
        /// Unlocks a hidden pathway for the player to discover
        /// </summary>
        public void UnlockHiddenPathway(string pathwayID)
        {
            foreach (HiddenPathway pathway in hiddenPathways)
            {
                if (pathway.pathwayID == pathwayID && !discoveredPathways[pathway])
                {
                    discoveredPathways[pathway] = true;
                    
                    if (pathway.pathwayObject != null)
                    {
                        pathway.pathwayObject.SetActive(true);
                        
                        // Play discovery effects
                        if (pathway.discoveryEffect != null)
                        {
                            Instantiate(pathway.discoveryEffect, pathway.pathwayObject.transform.position, Quaternion.identity);
                        }
                        
                        if (pathway.discoverySound != null)
                        {
                            AudioSource.PlayClipAtPoint(pathway.discoverySound, pathway.pathwayObject.transform.position);
                        }
                    }
                    
                    OnHiddenPathwayDiscovered?.Invoke();
                    Debug.Log($"Unlocked hidden pathway: {pathwayID}");
                    break;
                }
            }
        }

        /// <summary>
        /// Unlocks a tunnel entrance
        /// </summary>
        public void UnlockTunnelEntrance(string entranceID)
        {
            foreach (TunnelEntrance entrance in entrances)
            {
                if (entrance.entranceID == entranceID && !unlockedEntrances[entrance])
                {
                    unlockedEntrances[entrance] = true;
                    
                    if (entrance.entranceObject != null)
                    {
                        entrance.entranceObject.SetActive(true);
                        
                        // Play unlock effects
                        if (entrance.unlockEffect != null)
                        {
                            Instantiate(entrance.unlockEffect, entrance.entranceObject.transform.position, Quaternion.identity);
                        }
                        
                        if (entrance.unlockSound != null)
                        {
                            AudioSource.PlayClipAtPoint(entrance.unlockSound, entrance.entranceObject.transform.position);
                        }
                    }
                    
                    Debug.Log($"Unlocked tunnel entrance: {entranceID}");
                    break;
                }
            }
        }

        /// <summary>
        /// Unlocks a tunnel exit
        /// </summary>
        public void UnlockTunnelExit(string exitID)
        {
            foreach (TunnelExit exit in exits)
            {
                if (exit.exitID == exitID && !unlockedExits[exit])
                {
                    unlockedExits[exit] = true;
                    
                    if (exit.exitObject != null)
                    {
                        exit.exitObject.SetActive(true);
                        
                        // Play unlock effects
                        if (exit.unlockEffect != null)
                        {
                            Instantiate(exit.unlockEffect, exit.exitObject.transform.position, Quaternion.identity);
                        }
                        
                        if (exit.unlockSound != null)
                        {
                            AudioSource.PlayClipAtPoint(exit.unlockSound, exit.exitObject.transform.position);
                        }
                    }
                    
                    Debug.Log($"Unlocked tunnel exit: {exitID}");
                    break;
                }
            }
        }

        #endregion

        #region Environmental Features

        /// <summary>
        /// Set up the environmental features of the tunnel system
        /// </summary>
        private void SetupEnvironment()
        {
            // Setup lanterns
            foreach (GameObject lantern in lanterns)
            {
                if (lantern != null)
                {
                    // Add light component to lantern
                    Light lanternLight = lantern.GetComponent<Light>();
                    if (lanternLight == null)
                    {
                        lanternLight = lantern.AddComponent<Light>();
                    }
                    
                    // Configure the light
                    lanternLight.type = LightType.Point;
                    lanternLight.color = lanternColor;
                    lanternLight.intensity = lanternIntensity;
                    lanternLight.range = lanternRange;
                    lanternLight.shadows = LightShadows.Soft;
                    
                    // Add flickering effect
                    PDXUnderground.Effects.LanternFlicker flicker = lantern.GetComponent<PDXUnderground.Effects.LanternFlicker>() ?? 
                        lantern.AddComponent<PDXUnderground.Effects.LanternFlicker>();
                    // Set properties using the proper names from our implementation
                    flicker.flickerIntensity = 0.2f;  // Controls the intensity of the flicker
                    flicker.flickerSpeed = 1.5f;     // Controls how fast the light flickers
                    flicker.flickerVariation = 0.4f; // Controls variation in flicker rhythm
                    flicker.dayIntensity = 0.4f;     // Base intensity during daytime
                    flicker.nightIntensity = 0.9f;   // Base intensity during nighttime
                }
            }
            
            // Set ambient light
            if (mainAmbientLight != null)
            {
                mainAmbientLight.intensity = mainLightIntensity;
                mainAmbientLight.color = new Color(0.15f, 0.15f, 0.2f);
            }
            
            // Place water puddles
            if (waterPuddlePrefab != null)
            {
                PlaceEnvironmentalObjects(waterPuddlePrefab, 15, 0.05f, 0.2f);
            }
            
            // Place cobwebs
            if (cobwebPrefab != null)
            {
                PlaceEnvironmentalObjects(cobwebPrefab, 25, 1.5f, 2.5f);
            }
            
            // Place debris
            if (debrisPrefab != null)
            {
                PlaceEnvironmentalObjects(debrisPrefab, 20, 0.1f, 0.3f);
            }
            
            Debug.Log("Shanghai Tunnels environment setup complete");
        }
        
        /// <summary>
        /// Place environmental objects randomly in the tunnel segments
        /// </summary>
        private void PlaceEnvironmentalObjects(GameObject prefab, int count, float minHeight, float maxHeight)
        {
            if (tunnelSegments.Count == 0 || prefab == null)
                return;
                
            for (int i = 0; i < count; i++)
            {
                // Choose a random segment
                GameObject segment = tunnelSegments[Random.Range(0, tunnelSegments.Count)];
                if (segment == null)
                    continue;
                    
                // Get the bounds of the segment
                Renderer[] renderers = segment.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0)
                    continue;
                    
                // Calculate combined bounds
                Bounds bounds = renderers[0].bounds;
                foreach (Renderer renderer in renderers)
                {
                    bounds.Encapsulate(renderer.bounds);
                }
                
                // Get a random position within the bounds
                Vector3 randomPosition = new Vector3(
                    Random.Range(bounds.min.x + 1f, bounds.max.x - 1f),
                    Random.Range(bounds.min.y + minHeight, bounds.min.y + maxHeight),
                    Random.Range(bounds.min.z + 1f, bounds.max.z - 1f)
                );
                
                // Check if position is near a wall
                Ray ray = new Ray(randomPosition, Random.onUnitSphere);
                RaycastHit hit;
                
                if (Physics.Raycast(ray, out hit, 1.5f))
                {
                    // Instantiate the object at the hit point
                    GameObject obj = Instantiate(prefab, hit.point, Quaternion.LookRotation(hit.normal));
                    obj.transform.SetParent(segment.transform);
                    
                    // Add slight rotation variation
                    obj.transform.Rotate(new Vector3(0, Random.Range(0, 360), 0));
                    
                    // Scale variation
                    float scale = Random.Range(0.8f, 1.2f);
                    obj.transform.localScale *= scale;
                }
            }
        }

        /// <summary>
        /// Spawn collectible items at random spawn points
        /// </summary>
        private void SpawnCollectibles()
        {
            if (collectibleSpawnPoints == null || collectibleSpawnPoints.Length == 0 || availableCollectibles.Count == 0)
                return;
                
            // Determine how many collectibles to spawn
            int spawnCount = Mathf.Min(requiredCollectiblesCount, collectibleSpawnPoints.Length);
            
            // Create a list of spawn point indices and shuffle them
            List<int> spawnIndices = new List<int>();
            for (int i = 0; i < collectibleSpawnPoints.Length; i++)
            {
                spawnIndices.Add(i);
            }
            
            // Fisher-Yates shuffle
            for (int i = spawnIndices.Count - 1; i > 0; i--)
            {
                int swapIndex = Random.Range(0, i + 1);
                int temp = spawnIndices[i];
                spawnIndices[i] = spawnIndices[swapIndex];
                spawnIndices[swapIndex] = temp;
            }
            
            // Spawn collectibles at selected points
            for (int i = 0; i < spawnCount; i++)
            {
                if (i < availableCollectibles.Count && collectibleSpawnPoints[spawnIndices[i]] != null)
                {
                    // Get a collectible and instantiate it
                    CollectibleItem collectiblePrefab = availableCollectibles[Random.Range(0, availableCollectibles.Count)];
                    if (collectiblePrefab != null)
                    {
                        CollectibleItem collectible = Instantiate(collectiblePrefab, collectibleSpawnPoints[spawnIndices[i]].position, collectibleSpawnPoints[spawnIndices[i]].rotation);
                        collectible.tunnels = this;
                        
                        // Add slight idle animation
                        if (collectible.transform.Find("Visual") != null)
                        {
                            StartCoroutine(AnimateCollectible(collectible.transform.Find("Visual")));
                        }
                    }
                }
            }
            
            Debug.Log($"Spawned {spawnCount} collectibles in the tunnels");
        }
        
        /// <summary>
        /// Animate collectible with floating/rotation effect
        /// </summary>
        private IEnumerator AnimateCollectible(Transform collectibleVisual)
        {
            Vector3 startPos = collectibleVisual.localPosition;
            float rotationSpeed = Random.Range(10f, 30f);
            float bobSpeed = Random.Range(0.5f, 1.5f);
            float bobHeight = Random.Range(0.05f, 0.15f);
            
            while (collectibleVisual != null)
            {
                // Rotate the object
                collectibleVisual.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
                
                // Bob up and down
                float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
                collectibleVisual.localPosition = new Vector3(startPos.x, newY, startPos.z);
                
                yield return null;
            }
        }
        
        /// <summary>
        /// Spawn rats randomly throughout the tunnels
        /// </summary>
        private IEnumerator SpawnRatsRandomly()
        {
            if (ratPrefab == null)
                yield break;
                
            while (playerInTunnels)
            {
                // Keep spawning rats until we reach the max
                if (spawnedRats.Count < maxRats)
                {
                    // Choose a random segment
                    if (tunnelSegments.Count > 0)
                    {
                        GameObject segment = tunnelSegments[Random.Range(0, tunnelSegments.Count)];
                        if (segment != null && segment.activeInHierarchy)
                        {
                            // Get a random position within the segment
                            Renderer[] renderers = segment.GetComponentsInChildren<Renderer>();
                            if (renderers.Length > 0)
                            {
                                // Get a random renderer
                                Renderer randomRenderer = renderers[Random.Range(0, renderers.Length)];
                                Bounds bounds = randomRenderer.bounds;
                                
                                // Get a random position along the floor
                                Vector3 spawnPos = new Vector3(
                                    Random.Range(bounds.min.x + 0.5f, bounds.max.x - 0.5f),
                                    bounds.min.y + 0.1f,
                                    Random.Range(bounds.min.z + 0.5f, bounds.max.z - 0.5f)
                                );
                                
                                // Spawn a rat
                                GameObject rat = Instantiate(ratPrefab, spawnPos, Quaternion.Euler(0, Random.Range(0, 360), 0));
                                spawnedRats.Add(rat);
                                
                                // Add AI navigation to run away from player
                                // Add AI navigation and behavior
                                RatBehavior ratBehavior = rat.GetComponent<RatBehavior>() ?? rat.AddComponent<RatBehavior>();
                                
                                // Set properties using the proper names from our implementation
                                ratBehavior.patrolSpeed = Random.Range(1.0f, 2.0f);
                                ratBehavior.chaseSpeed = Random.Range(2.5f, 3.5f);
                                ratBehavior.retreatSpeed = Random.Range(3.0f, 4.0f);
                                ratBehavior.detectionRange = Random.Range(6.0f, 10.0f);
                                
                                // Add sound effects if available
                                if (ratSquealSound != null)
                                {
                                    // Add AudioSource directly instead of using reflection
                                    AudioSource ratAudio = rat.GetComponent<AudioSource>() ?? rat.AddComponent<AudioSource>();
                                    ratAudio.clip = ratSquealSound;
                                    ratAudio.spatialBlend = 1.0f;  // Full 3D sound
                                    ratAudio.minDistance = 1.0f;
                                    ratAudio.maxDistance = 15.0f;
                                    ratAudio.playOnAwake = false;
                                    ratAudio.volume = 0.6f;  // Set reasonable volume
                                }
                                // Destroy rat after random lifetime
                                StartCoroutine(DestroyRatAfterTime(rat, Random.Range(20f, 40f)));
                            }
                        }
                    }
                }
                
                // Wait before spawning the next rat
                yield return new WaitForSeconds(ratSpawnInterval);
            }
        }
        
        /// <summary>
        /// Destroy a rat after a specified time
        /// </summary>
        private IEnumerator DestroyRatAfterTime(GameObject rat, float time)
        {
            yield return new WaitForSeconds(time);
            
            if (rat != null)
            {
                // Remove from the list
                spawnedRats.Remove(rat);
                
                // Destroy the rat
                Destroy(rat);
            }
        }
        
        /// <summary>
        /// Play random ambient sounds at intervals
        /// </summary>
        private IEnumerator PlayRandomAmbientSounds()
        {
            // List of available ambient sounds
            List<AudioClip> ambientSounds = new List<AudioClip>();
            if (waterDripSound != null) ambientSounds.Add(waterDripSound);
            if (woodCreakSound != null) ambientSounds.Add(woodCreakSound);
            
            while (true)
            {
                // Wait random interval
                yield return new WaitForSeconds(Random.Range(randomSoundInterval * 0.5f, randomSoundInterval * 1.5f));
                
                if (playerInTunnels && ambientSounds.Count > 0)
                {
                    // Choose a random sound
                    AudioClip randomSound = ambientSounds[Random.Range(0, ambientSounds.Count)];
                    
                    // Find a random position to play from
                    Vector3 soundPosition;
                    if (tunnelSegments.Count > 0 && tunnelSegments[currentSegmentIndex] != null)
                    {
                        Renderer[] renderers = tunnelSegments[currentSegmentIndex].GetComponentsInChildren<Renderer>();
                        if (renderers.Length > 0)
                        {
                            Bounds bounds = renderers[0].bounds;
                            soundPosition = new Vector3(
                                Random.Range(bounds.min.x, bounds.max.x),
                                Random.Range(bounds.min.y, bounds.max.y),
                                Random.Range(bounds.min.z, bounds.max.z)
                            );
                            
                            // Play the sound at that position
                            AudioSource.PlayClipAtPoint(randomSound, soundPosition, 0.6f);
                        }
                    }
                }
            }
        }
        #endregion // Environmental Features

        #region Collectible Management

        /// <summary>
        /// Register a collected item when player picks it up.
        /// This method is called by CollectibleItem instances when they are collected.
        /// It increments the requiredCollectiblesFound counter and checks if all required
        /// collectibles have been found, updating allCollectiblesFound when the requirement is met.
        /// </summary>
        /// <param name="item">The collectible item that was picked up</param>
        public void RegisterCollectedItem(CollectibleItem item)
        {
            if (item == null || collectedItems.Contains(item))
                return;

            // Add the item to the collection
            collectedItems.Add(item);
            requiredCollectiblesFound++;

            // Trigger event
            OnCollectibleFound?.Invoke();

            Debug.Log($"Collected item: {item.ItemName}");

            // Check if all required collectibles are found
            if (requiredCollectiblesFound >= requiredCollectiblesCount && !allCollectiblesFound)
            {
                allCollectiblesFound = true;
                OnAllCollectiblesFound?.Invoke();
                Debug.Log("All required collectibles found!");

                // If level completion requires all collectibles, check for level completion
                if (requireAllCollectibles)
                {
                    CheckLevelCompletion();
                }
            }
        }

        /// <summary>
        /// Check if level completion criteria are met.
        /// This method evaluates whether the conditions for level completion are satisfied,
        /// which may depend on the requireAllCollectibles flag and allCollectiblesFound state.
        /// When conditions are met, the player simply needs to reach the level end point.
        /// </summary>
        private void CheckLevelCompletion()
        {
            // Basic implementation - can be expanded
            if ((requireAllCollectibles && allCollectiblesFound) || !requireAllCollectibles)
            {
                // Level is completed when player reaches the end point
                // This is typically triggered by a collider at the level end
                if (!levelCompleted)
                {
                    Debug.Log("Level completion criteria met - reach end point to finish level");
                }
            }
        }

        /// <summary>
        /// Complete the level and transition to next level.
        /// Sets levelCompleted flag to true, triggers OnLevelComplete event,
        /// and loads the next level after the specified delay when nextLevelName is set.
        /// </summary>
        public void CompleteLevel()
        {
            if (levelCompleted)
                return;

            levelCompleted = true;
            OnLevelComplete?.Invoke();

            Debug.Log("Level completed!");

            // Load next level after delay
            if (!string.IsNullOrEmpty(nextLevelName))
            {
                StartCoroutine(LoadNextLevelAfterDelay(levelTransitionDelay));
            }
        }

        /// <summary>
        /// Load the next level after delay
        /// </summary>
        private IEnumerator LoadNextLevelAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextLevelName);
        }

        #endregion

    }
    
    /// <summary>
    /// Represents an entrance to the Shanghai Tunnels
    /// </summary>
    [System.Serializable]
    public class TunnelEntrance
    {
        public string entranceID;
        public GameObject entranceObject;
        public bool startUnlocked = false;
        public GameObject unlockEffect;
        public AudioClip unlockSound;
    }
    
    /// <summary>
    /// Represents an exit from the Shanghai Tunnels
    /// </summary>
    [System.Serializable]
    public class TunnelExit
    {
        public string exitID;
        public GameObject exitObject;
        public bool startUnlocked = false;
        public GameObject unlockEffect;
        public AudioClip unlockSound;
    }
    /// <summary>
    /// Represents a hidden pathway in the tunnels
    /// </summary>
    [System.Serializable]
    public class HiddenPathway
    {
        public string pathwayID;
        public GameObject pathwayObject;
        public GameObject discoveryEffect;
        public AudioClip discoverySound;
    }
} // Close namespace PDXUnderground.Interaction
