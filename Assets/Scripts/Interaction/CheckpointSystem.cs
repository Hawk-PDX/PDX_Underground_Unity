using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace PDXUnderground.Interaction
{
    /// <summary>
    /// Manages the checkpoint system for tracking player progress through the Shanghai Tunnels,
    /// handling save/load functionality and respawn points.
    /// </summary>
    public class CheckpointSystem : MonoBehaviour
    {
        #region Inspector Properties

        [Header("Checkpoint Settings")]
        [SerializeField] private List<Checkpoint> checkpoints = new List<Checkpoint>();
        [SerializeField] private bool autoSaveOnCheckpoint = true;
        [SerializeField] private bool loadLastCheckpointOnStart = true;
        [SerializeField] private string saveFileName = "pdx_underground_save.dat";
        [SerializeField] private Transform defaultRespawnPoint;

        [Header("Collectible Tracking")]
        [SerializeField] private bool trackCollectibles = true;

        [Header("Audio")]
        [SerializeField] private AudioClip checkpointReachedSound;
        [SerializeField] private AudioClip respawnSound;

        [Header("Effects")]
        [SerializeField] private GameObject checkpointActivationEffect;
        [SerializeField] private GameObject respawnEffect;

        [Header("Events")]
        public UnityEvent OnCheckpointReached;
        public UnityEvent OnPlayerRespawned;
        public UnityEvent<SaveGameData> OnGameSaved;
        public UnityEvent<SaveGameData> OnGameLoaded;

        #endregion

        #region Private Variables

        private Checkpoint currentCheckpoint;
        private List<string> collectedItemIDs = new List<string>();
        private Dictionary<string, bool> discoveredPathways = new Dictionary<string, bool>();
        private Dictionary<string, bool> unlockedEntrances = new Dictionary<string, bool>();
        private int deathCount = 0;
        private float gameStartTime;
        private float gameplayTime = 0f;
        private bool isLoading = false;

        #endregion

        #region Public Properties

        public Checkpoint CurrentCheckpoint => currentCheckpoint;
        public List<string> CollectedItemIDs => collectedItemIDs;
        public int DeathCount => deathCount;
        public float GameplayTime => gameplayTime + (Time.time - gameStartTime);

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Initialize checkpoints
            foreach (Checkpoint checkpoint in checkpoints)
            {
                if (checkpoint != null && checkpoint.checkpointTrigger != null)
                {
                    // Add trigger listeners
                    CheckpointTrigger trigger = checkpoint.checkpointTrigger.GetComponent<CheckpointTrigger>();
                    if (trigger == null)
                    {
                        trigger = checkpoint.checkpointTrigger.AddComponent<CheckpointTrigger>();
                    }
                    
                    trigger.SetCheckpoint(checkpoint);
                    trigger.OnTriggerActivated.AddListener(ActivateCheckpoint);
                    
                    // Mark all checkpoints as inactive initially
                    checkpoint.isActive = false;
                }
            }
            
            gameStartTime = Time.time;
        }

        private void Start()
        {
            // Load last checkpoint if enabled
            if (loadLastCheckpointOnStart)
            {
                LoadGame();
            }
        }

        private void Update()
        {
            // Debug controls in development
            #if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.F5))
            {
                SaveGame();
            }
            
            if (Input.GetKeyDown(KeyCode.F9))
            {
                LoadGame();
            }
            #endif
        }

        #endregion

        #region Checkpoint Management

        /// <summary>
        /// Activates a checkpoint when the player reaches it
        /// </summary>
        public void ActivateCheckpoint(Checkpoint checkpoint)
        {
            if (checkpoint == null || checkpoint == currentCheckpoint)
                return;
                
            // Deactivate the current checkpoint
            if (currentCheckpoint != null)
            {
                currentCheckpoint.isActive = false;
            }
            
            // Activate the new checkpoint
            checkpoint.isActive = true;
            checkpoint.activationTime = Time.time;
            currentCheckpoint = checkpoint;
            
            // Play effects
            if (checkpointActivationEffect != null && checkpoint.respawnPoint != null)
            {
                Instantiate(checkpointActivationEffect, checkpoint.respawnPoint.position, Quaternion.identity);
            }
            
            // Play sound
            if (checkpointReachedSound != null)
            {
                AudioSource.PlayClipAtPoint(checkpointReachedSound, checkpoint.respawnPoint != null ? 
                    checkpoint.respawnPoint.position : Camera.main.transform.position);
            }
            
            // Fire event
            OnCheckpointReached?.Invoke();
            
            // Auto-save if enabled
            if (autoSaveOnCheckpoint)
            {
                SaveGame();
            }
            
            Debug.Log($"Checkpoint activated: {checkpoint.checkpointName} at {checkpoint.respawnPoint.position}");
        }

        /// <summary>
        /// Respawns the player at the last checkpoint
        /// </summary>
        public void RespawnPlayer()
        {
            // Find respawn position
            Transform respawnPoint = null;
            
            if (currentCheckpoint != null && currentCheckpoint.respawnPoint != null)
            {
                respawnPoint = currentCheckpoint.respawnPoint;
            }
            else if (defaultRespawnPoint != null)
            {
                respawnPoint = defaultRespawnPoint;
            }
            
            if (respawnPoint == null)
            {
                Debug.LogWarning("No valid respawn point found!");
                return;
            }
            
            // Get player reference
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("Cannot respawn: Player not found!");
                return;
            }
            
            // Increment death counter
            deathCount++;
            
            // Teleport player to respawn point
            player.transform.position = respawnPoint.position;
            player.transform.rotation = respawnPoint.rotation;
            
            // Play effects
            if (respawnEffect != null)
            {
                Instantiate(respawnEffect, respawnPoint.position, Quaternion.identity);
            }
            
            // Play sound
            if (respawnSound != null)
            {
                AudioSource.PlayClipAtPoint(respawnSound, respawnPoint.position);
            }
            
            // Fire event
            OnPlayerRespawned?.Invoke();
            
            Debug.Log($"Player respawned at checkpoint: {currentCheckpoint?.checkpointName}. Death count: {deathCount}");
        }

        /// <summary>
        /// Registers a collected item in the checkpoint system
        /// </summary>
        public void RegisterCollectedItem(CollectibleItem item)
        {
            if (item == null || !trackCollectibles)
                return;
                
            string itemID = item.ItemID;
            
            if (!collectedItemIDs.Contains(itemID))
            {
                collectedItemIDs.Add(itemID);
                Debug.Log($"Item registered with checkpoint system: {item.ItemName} ({itemID})");
            }
        }

        /// <summary>
        /// Registers a discovered pathway
        /// </summary>
        public void RegisterDiscoveredPathway(string pathwayID)
        {
            if (string.IsNullOrEmpty(pathwayID))
                return;
                
            discoveredPathways[pathwayID] = true;
            Debug.Log($"Pathway registered with checkpoint system: {pathwayID}");
        }

        /// <summary>
        /// Registers an unlocked entrance
        /// </summary>
        public void RegisterUnlockedEntrance(string entranceID)
        {
            if (string.IsNullOrEmpty(entranceID))
                return;
                
            unlockedEntrances[entranceID] = true;
            Debug.Log($"Entrance registered with checkpoint system: {entranceID}");
        }

        /// <summary>
        /// Checks if an item has been collected
        /// </summary>
        public bool IsItemCollected(string itemID)
        {
            return collectedItemIDs.Contains(itemID);
        }

        /// <summary>
        /// Checks if a pathway has been discovered
        /// </summary>
        public bool IsPathwayDiscovered(string pathwayID)
        {
            return discoveredPathways.ContainsKey(pathwayID) && discoveredPathways[pathwayID];
        }

        /// <summary>
        /// Checks if an entrance has been unlocked
        /// </summary>
        public bool IsEntranceUnlocked(string entranceID)
        {
            return unlockedEntrances.ContainsKey(entranceID) && unlockedEntrances[entranceID];
        }

        #endregion

        #region Save/Load System

        /// <summary>
        /// Saves the current game state
        /// </summary>
        public void SaveGame()
        {
            SaveGameData saveData = new SaveGameData();
            
            // Save checkpoint data
            if (currentCheckpoint != null)
            {
                saveData.checkpointID = currentCheckpoint.checkpointID;
                saveData.checkpointName = currentCheckpoint.checkpointName;
                
                if (currentCheckpoint.respawnPoint != null)
                {
                    saveData.respawnPosition = currentCheckpoint.respawnPoint.position;
                    saveData.respawnRotation = currentCheckpoint.respawnPoint.rotation.eulerAngles;
                }
            }
            
            // Save collected items
            saveData.collectedItems = new List<string>(collectedItemIDs);
            
            // Save discovered pathways
            saveData.discoveredPathways = new Dictionary<string, bool>(discoveredPathways);
            
            // Save unlocked entrances
            saveData.unlockedEntrances = new Dictionary<string, bool>(unlockedEntrances);
            
            // Save player stats
            saveData.deathCount = deathCount;
            saveData.gameplayTime = GameplayTime;
            
            // Save to file
            try
            {
                string savePath = Path.Combine(Application.persistentDataPath, saveFileName);
                
                using (FileStream stream = new FileStream(savePath, FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, saveData);
                }
                
                Debug.Log($"Game saved successfully to {savePath}");
                OnGameSaved?.Invoke(saveData);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error saving game: {e.Message}");
            }
        }

        /// <summary>
        /// Loads the saved game state
        /// </summary>
        public void LoadGame()
        {
            string savePath = Path.Combine(Application.persistentDataPath, saveFileName);
            
            if (!File.Exists(savePath))
            {
                Debug.Log("No save file found. Starting a new game.");
                return;
            }
            
            try
            {
                isLoading = true;
                
                // Load from file
                SaveGameData saveData;
                using (FileStream stream = new FileStream(savePath, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    saveData = (SaveGameData)formatter.Deserialize(stream);
                }
                
                // Restore checkpoint
                if (!string.IsNullOrEmpty(saveData.checkpointID))
                {
                    foreach (Checkpoint checkpoint in checkpoints)
                    {
                        if (checkpoint.checkpointID == saveData.checkpointID)
                        {
                            currentCheckpoint = checkpoint;
                            checkpoint.isActive = true;
                            break;
                        }
                    }
                }
                
                // Restore collected items
                collectedItemIDs = new List<string>(saveData.collectedItems);
                
                // Restore discovered pathways
                discoveredPathways = new Dictionary<string, bool>(saveData.discoveredPathways);
                
                // Restore unlocked entrances
                unlockedEntrances = new Dictionary<string, bool>(saveData.unlockedEntrances);
                
                // Restore player stats
                deathCount = saveData.deathCount;
                gameplayTime = saveData.gameplayTime;
                gameStartTime = Time.time;
                
                // Respawn player at checkpoint
                if (currentCheckpoint != null)
                {
                    RespawnPlayer();
                }
                
                // Apply game state to relevant systems
                ApplySaveDataToGameSystems(saveData);
                
                Debug.Log($"Game loaded successfully from {savePath}");
                OnGameLoaded?.Invoke(saveData);
                
                isLoading = false;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading game: {e.Message}");
                isLoading = false;
            }
        }

        /// <summary>
        /// Applies the loaded save data to active game systems
        /// </summary>
        private void ApplySaveDataToGameSystems(SaveGameData saveData)
        {
            // Apply collected item state
            CollectibleItem[] collectibles = FindObjectsOfType<CollectibleItem>();
            foreach (CollectibleItem collectible in collectibles)
            {
                if (saveData.collectedItems.Contains(collectible.ItemID))
                {
                    // Mark as already collected - we need to disable it without triggering collection again
                    collectible.gameObject.SetActive(false);
                }
            }
            
            // Apply pathway discoveries
            ShanghaiTunnels tunnels = FindObjectOfType<ShanghaiTunnels>();
            if (tunnels != null)
            {
                // Unlock discovered pathways
                foreach (var pathway in saveData.discoveredPathways)
                {
                    if (pathway.Value)
                    {
                        tunnels.UnlockHiddenPathway(pathway.Key);
                    }
                }
                
                // Unlock entrances
                foreach (var entrance in saveData.unlockedEntrances)
                {
                    if (entrance.Value)
                    {
                        tunnels.UnlockTunnelEntrance(entrance.Key);
                    }
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents a checkpoint in the game world
    /// </summary>
    [System.Serializable]
    public class Checkpoint
    {
        public string checkpointID;
        public string checkpointName = "Checkpoint";
        [TextArea(1, 3)]
        public string description = "A safe haven in the Shanghai Tunnels.";
        public GameObject checkpointTrigger;
        public Transform respawnPoint;
        [HideInInspector] 
        public bool isActive = false;
        [HideInInspector] 
        public float activationTime = 0f;
    }

    /// <summary>
    /// Checkpoint trigger implementation for activating checkpoints
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CheckpointTrigger : MonoBehaviour
    {
        private Checkpoint checkpoint;
        private bool activated = false;
        
        // Event for when the checkpoint is triggered
        [HideInInspector]
        public UnityEvent<Checkpoint> OnTriggerActivated = new UnityEvent<Checkpoint>();
        
        private void Awake()
        {
            // Ensure the collider is a trigger
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }
        
        /// <summary>
        /// Set the checkpoint that this trigger activates
        /// </summary>
        public void SetCheckpoint(Checkpoint cp)
        {
            checkpoint = cp;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // Only activate for the player
            if (other.CompareTag("Player") && checkpoint != null)
            {
                // Trigger the checkpoint activation event
                OnTriggerActivated.Invoke(checkpoint);
                
                // Visual feedback that player has entered a checkpoint
                ActivateVisualFeedback();
            }
        }
        
        /// <summary>
        /// Provides visual feedback when player enters the checkpoint
        /// </summary>
        private void ActivateVisualFeedback()
        {
            // Add visual glow or particle effect to indicate checkpoint is active
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in renderers)
            {
                if (rend.material != null)
                {
                    // Create a glowing material effect
                    Material glowMaterial = new Material(rend.material);
                    glowMaterial.EnableKeyword("_EMISSION");
                    glowMaterial.SetColor("_EmissionColor", Color.cyan * 2f);
                    rend.material = glowMaterial;
                }
            }
            
            // Optionally add a light to the checkpoint
            Light checkpointLight = GetComponentInChildren<Light>();
            if (checkpointLight == null)
            {
                GameObject lightObj = new GameObject("CheckpointLight");
                lightObj.transform.SetParent(transform);
                lightObj.transform.localPosition = Vector3.up;
                
                checkpointLight = lightObj.AddComponent<Light>();
                checkpointLight.type = LightType.Point;
                checkpointLight.color = Color.cyan;
                checkpointLight.intensity = 1.5f;
                checkpointLight.range = 3f;
            }
            else
            {
                checkpointLight.enabled = true;
                checkpointLight.intensity = 1.5f;
            }
            
            // Add pulsing effect to checkpoint light
            StartCoroutine(PulseCheckpointLight(checkpointLight));
        }
        
        /// <summary>
        /// Creates a pulsing light effect for the checkpoint
        /// </summary>
        private IEnumerator PulseCheckpointLight(Light light)
        {
            if (light == null)
                yield break;
                
            float baseIntensity = light.intensity;
            float time = 0f;
            float duration = 2f;
            
            while (true)
            {
                // Pulse up
                while (time < duration/2)
                {
                    time += Time.deltaTime;
                    float t = time / (duration/2);
                    light.intensity = Mathf.Lerp(baseIntensity * 0.6f, baseIntensity * 1.2f, t);
                    yield return null;
                }
                
                // Pulse down
                while (time < duration)
                {
                    time += Time.deltaTime;
                    float t = (time - duration/2) / (duration/2);
                    light.intensity = Mathf.Lerp(baseIntensity * 1.2f, baseIntensity * 0.6f, t);
                    yield return null;
                }
                
                time = 0f;
            }
        }
    }
    
    /// <summary>
    /// Data structure for saving game state
    /// </summary>
    [System.Serializable]
    public class SaveGameData
    {
        // Checkpoint data
        public string checkpointID;
        public string checkpointName;
        public Vector3 respawnPosition;
        public Vector3 respawnRotation;
        
        // Collectibles and progression
        public List<string> collectedItems = new List<string>();
        public Dictionary<string, bool> discoveredPathways = new Dictionary<string, bool>();
        public Dictionary<string, bool> unlockedEntrances = new Dictionary<string, bool>();
        
        // Player stats
        public int deathCount = 0;
        public float gameplayTime = 0f;
        public int collectiblesFound = 0;
        public int totalSecrets = 0;
        public int secretsFound = 0;
        
        // Level progress
        public string currentLevel;
        public List<string> visitedAreas = new List<string>();
        public string lastActiveSegmentID;
        
        // Game progression flags
        public Dictionary<string, bool> gameFlags = new Dictionary<string, bool>();
        
        // Constructor
        public SaveGameData()
        {
            // Initialize with default values
            checkpointID = "";
            checkpointName = "Start";
            respawnPosition = Vector3.zero;
            respawnRotation = Vector3.zero;
            currentLevel = "MainScene";
            lastActiveSegmentID = "";
        }
        
        /// <summary>
        /// Sets a game flag value
        /// </summary>
        public void SetGameFlag(string flagName, bool value)
        {
            gameFlags[flagName] = value;
        }
        
        /// <summary>
        /// Gets a game flag value
        /// </summary>
        public bool GetGameFlag(string flagName, bool defaultValue = false)
        {
            if (gameFlags.ContainsKey(flagName))
            {
                return gameFlags[flagName];
            }
            return defaultValue;
        }
        
        /// <summary>
        /// Marks an area as visited
        /// </summary>
        public void MarkAreaVisited(string areaID)
        {
            if (!visitedAreas.Contains(areaID))
            {
                visitedAreas.Add(areaID);
            }
        }
        
        /// <summary>
        /// Checks if an area has been visited
        /// </summary>
        public bool HasVisitedArea(string areaID)
        {
            return visitedAreas.Contains(areaID);
        }
    }
}
