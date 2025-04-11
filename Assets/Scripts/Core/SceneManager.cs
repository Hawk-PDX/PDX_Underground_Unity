using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// SceneManager handles all scene loading and transitions in PDX Underground,
/// managing movement between the Portland streets, Shanghai tunnels, and port areas.
/// </summary>
public class SceneManager : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Scene References")]
    [SerializeField] private string streetSceneName = "PortlandStreets_Test";
    [SerializeField] private string tunnelsSceneName = "ShanghaiTunnels";
    [SerializeField] private string portSceneName = "PortOfPortland";
    
    [Header("Transition Settings")]
    [SerializeField] private float fadeOutDuration = 1.0f;
    [SerializeField] private float fadeInDuration = 1.0f;
    [SerializeField] private bool useLoadingScreen = true;
    [SerializeField] private float minimumLoadingScreenTime = 0.5f;
    
    [Header("Player Settings")]
    [SerializeField] private string playerTag = "Player";
    
    [Header("Default Spawn Points")]
    [SerializeField] private Vector3 defaultStreetsSpawnPoint = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 defaultTunnelsSpawnPoint = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 defaultPortSpawnPoint = new Vector3(0, 0.5f, 0);
    
    [Header("Area Names")]
    [SerializeField] private string streetsAreaName = "Portland Streets";
    [SerializeField] private string tunnelsAreaName = "Shanghai Tunnels";
    [SerializeField] private string portAreaName = "Port of Portland";
    
    #endregion
    
    #region Private Variables
    
    private UIManager uiManager;
    private string currentSceneName;
    private bool isTransitioning = false;
    private Vector3 targetSpawnPosition = Vector3.zero;
    private GameObject playerInstance;
    private PortlandEnvironmentSetup environmentSetup;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        // Get references
        uiManager = FindObjectOfType<UIManager>();
        
        // Set initial scene name
        currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }
    
    private void Start()
    {
        // Display initial area name
        if (uiManager != null)
        {
            string areaName = GetAreaNameForScene(currentSceneName);
            uiManager.ShowAreaName(areaName);
        }
        
        // Find player instance
        FindPlayerInstance();
        
        // Find environment setup
        FindEnvironmentSetup();
    }
    
    private void OnEnable()
    {
        // Register for scene loaded event
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        // Unregister from scene loaded event
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Transitions to the Portland streets scene
    /// </summary>
    public void TransitionToStreets(Vector3 spawnPosition = default)
    {
        if (isTransitioning)
            return;
            
        targetSpawnPosition = spawnPosition != default ? spawnPosition : defaultStreetsSpawnPoint;
        StartCoroutine(TransitionToScene(streetSceneName, streetsAreaName));
    }
    
    /// <summary>
    /// Transitions to the Shanghai tunnels scene
    /// </summary>
    public void TransitionToTunnels(Vector3 spawnPosition = default)
    {
        if (isTransitioning)
            return;
            
        targetSpawnPosition = spawnPosition != default ? spawnPosition : defaultTunnelsSpawnPoint;
        StartCoroutine(TransitionToScene(tunnelsSceneName, tunnelsAreaName));
    }
    
    /// <summary>
    /// Transitions to the Port of Portland scene
    /// </summary>
    public void TransitionToPort(Vector3 spawnPosition = default)
    {
        if (isTransitioning)
            return;
            
        targetSpawnPosition = spawnPosition != default ? spawnPosition : defaultPortSpawnPoint;
        StartCoroutine(TransitionToScene(portSceneName, portAreaName));
    }
    
    /// <summary>
    /// Loads a scene by name
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (isTransitioning)
            return;
            
        StartCoroutine(TransitionToScene(sceneName, GetAreaNameForScene(sceneName)));
    }
    
    #endregion
    
    #region Private Methods
    
    /// <summary>
    /// Transitions to a new scene with fade effects
    /// </summary>
    private IEnumerator TransitionToScene(string sceneName, string areaName)
    {
        isTransitioning = true;
        
        // Preserve player state if needed (inventory, health, etc.)
        SavePlayerState();
        
        // Fade out
        if (uiManager != null)
        {
            yield return StartCoroutine(uiManager.FadeToBlack());
        }
        else
        {
            yield return new WaitForSeconds(fadeOutDuration);
        }
        
        // Show loading screen if enabled
        float loadStartTime = Time.time;
        if (useLoadingScreen && uiManager != null)
        {
            uiManager.ShowLoadingScreen();
        }
        
        // Load the new scene asynchronously
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = true;
        
        // Wait for the scene to load
        while (!asyncLoad.isDone)
        {
            // Update loading progress
            if (useLoadingScreen && uiManager != null)
            {
                uiManager.UpdateLoadingProgress(asyncLoad.progress);
            }
            
            yield return null;
        }
        
        // Update current scene name
        currentSceneName = sceneName;
        
        // Ensure minimum loading screen time
        float loadingElapsed = Time.time - loadStartTime;
        if (useLoadingScreen && loadingElapsed < minimumLoadingScreenTime)
        {
            yield return new WaitForSeconds(minimumLoadingScreenTime - loadingElapsed);
        }
        
        // Scene is loaded, find references in new scene
        FindPlayerInstance();
        FindEnvironmentSetup();
        
        // Position player at spawn location
        PositionPlayerAtSpawn();
        
        // Hide loading screen if it was shown
        if (useLoadingScreen && uiManager != null)
        {
            uiManager.HideLoadingScreen();
        }
        
        // Show area name
        if (uiManager != null)
        {
            uiManager.ShowAreaName(areaName);
        }
        
        // Fade in
        if (uiManager != null)
        {
            yield return StartCoroutine(uiManager.FadeFromBlack());
        }
        else
        {
            yield return new WaitForSeconds(fadeInDuration);
        }
        
        isTransitioning = false;
    }
    
    /// <summary>
    /// Called when a scene has finished loading
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Update current scene name
        currentSceneName = scene.name;
        
        // Find references in the new scene
        FindPlayerInstance();
        FindEnvironmentSetup();
    }
    
    /// <summary>
    /// Finds the player instance in the scene
    /// </summary>
    private void FindPlayerInstance()
    {
        playerInstance = GameObject.FindGameObjectWithTag(playerTag);
        
        if (playerInstance == null)
        {
            Debug.LogWarning("No player found in scene with tag: " + playerTag);
        }
    }
    
    /// <summary>
    /// Finds the environment setup in the scene
    /// </summary>
    private void FindEnvironmentSetup()
    {
        environmentSetup = FindObjectOfType<PortlandEnvironmentSetup>();
    }
    
    /// <summary>
    /// Positions the player at the spawn location
    /// </summary>
    private void PositionPlayerAtSpawn()
    {
        if (playerInstance == null)
        {
            Debug.LogWarning("Cannot position player: No player instance found.");
            return;
        }
        
        // Check if we have a valid spawn position
        if (targetSpawnPosition != Vector3.zero)
        {
            // Use direct positioning
            PlayerController playerController = playerInstance.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TeleportTo(targetSpawnPosition);
            }
            else
            {
                // Fall back to transform position
                playerInstance.transform.position = targetSpawnPosition;
            }
        }
        else if (environmentSetup != null)
        {
            // Use environment setup to find a spawn point
            Vector3 spawnPos = environmentSetup.GetNearestSpawnPoint(playerInstance.transform.position);
            
            PlayerController playerController = playerInstance.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TeleportTo(spawnPos);
            }
            else
            {
                // Fall back to transform position
                playerInstance.transform.position = spawnPos;
            }
        }
        else
        {
            // Use default spawn points based on scene
            Vector3 defaultSpawn = GetDefaultSpawnForScene(currentSceneName);
            
            PlayerController playerController = playerInstance.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TeleportTo(defaultSpawn);
            }
            else
            {
                // Fall back to transform position
                playerInstance.transform.position = defaultSpawn;
            }
        }
        
        // Reset target spawn position
        targetSpawnPosition = Vector3.zero;
    }
    
    /// <summary>
    /// Gets the default spawn point for a scene
    /// </summary>
    private Vector3 GetDefaultSpawnForScene(string sceneName)
    {
        if (sceneName == streetSceneName)
        {
            return defaultStreetsSpawnPoint;
        }
        else if (sceneName == tunnelsSceneName)
        {
            return defaultTunnelsSpawnPoint;
        }
        else if (sceneName == portSceneName)
        {
            return defaultPortSpawnPoint;
        }
        
        // Default fallback
        return new Vector3(0, 0.5f, 0);
    }
    
    /// <summary>
    /// Gets the area name for a scene
    /// </summary>
    private string GetAreaNameForScene(string sceneName)
    {
        if (sceneName == streetSceneName)
        {
            return streetsAreaName;
        }
        else if (sceneName == tunnelsSceneName)
        {
            return tunnelsAreaName;
        }
        else if (sceneName == portSceneName)
        {
            return portAreaName;
        }
        
        // Default fallback
        return sceneName;
    }
    
    /// <summary>
    /// Saves the player's state before scene transition
    /// </summary>
    private void SavePlayerState()
    {
        // In a full implementation, this would save player inventory, health, etc.
        // For now, this is just a placeholder
        
        if (playerInstance == null)
        {
            return;
        }
        
        // Example of saving player state (would be expanded in a full implementation)
        PlayerController playerController = playerInstance.GetComponent<PlayerController>();
        if (playerController != null)
        {
            // TODO: Save player data to a persistent store
        }
    }
    
    #endregion
}

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// SceneManager handles scene transitions and area loading for PDX Underground,
/// including fading, loading screens, and player teleportation between areas.
/// </summary>
public class SceneManager : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Scene References")]
    [SerializeField] private string streetSceneName = "PortlandStreets_Test";
    [SerializeField] private string tunnelsSceneName = "ShanghaiTunnels";
    [SerializeField] private string portSceneName = "PortOfPortland";
    
    [Header("Transition Settings")]
    [SerializeField] private float fadeOutDuration = 1.0f;
    [SerializeField] private float fadeInDuration = 1.0f;
    [SerializeField] private float minimumLoadingScreenTime = 0.5f;
    [SerializeField] private bool useLoadingScreen = true;
    
    [Header("Area Names")]
    [SerializeField] private string streetsAreaName = "Portland Streets";
    [SerializeField] private string tunnelsAreaName = "Shanghai Tunnels";
    [SerializeField] private string portAreaName = "Port of Portland";
    
    [Header("Spawn Points")]
    [SerializeField] private Vector3 defaultStreetSpawnPoint = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 defaultTunnelSpawnPoint = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 defaultPortSpawnPoint = new Vector3(0, 0.5f, 0);
    
    #endregion
    
    #region Private Variables
    
    private bool isTransitioning = false;
    private UIManager uiManager;
    private string currentSceneName;
    private Vector3 savedPlayerPosition;
    private Quaternion savedPlayerRotation;
    private Dictionary<string, string> sceneToAreaName = new Dictionary<string, string>();
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        // Get references
        uiManager = FindObjectOfType<UIManager>();
        
        // Set up scene to area name mapping
        sceneToAreaName.Add(streetSceneName, streetsAreaName);
        sceneToAreaName.Add(tunnelsSceneName, tunnelsAreaName);
        sceneToAreaName.Add(portSceneName, portAreaName);
        
        // Get current scene name
        currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        // Register scene loaded callback
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDestroy()
    {
        // Unregister scene loaded callback
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Transitions to the streets scene
    /// </summary>
    public void TransitionToStreets(Vector3 targetPosition = default)
    {
        if (isTransitioning)
            return;
            
        // If we're already in the streets scene, just teleport the player
        if (currentSceneName == streetSceneName)
        {
            TeleportPlayer(targetPosition.Equals(default(Vector3)) ? defaultStreetSpawnPoint : targetPosition);
            return;
        }
        
        // Start transition to streets scene
        StartCoroutine(TransitionToScene(streetSceneName, targetPosition.Equals(default(Vector3)) ? defaultStreetSpawnPoint : targetPosition));
    }
    
    /// <summary>
    /// Transitions to the tunnels scene
    /// </summary>
    public void TransitionToTunnels(Vector3 targetPosition = default)
    {
        if (isTransitioning)
            return;
            
        // If we're already in the tunnels scene, just teleport the player
        if (currentSceneName == tunnelsSceneName)
        {
            TeleportPlayer(targetPosition.Equals(default(Vector3)) ? defaultTunnelSpawnPoint : targetPosition);
            return;
        }
        
        // Start transition to tunnels scene
        StartCoroutine(TransitionToScene(tunnelsSceneName, targetPosition.Equals(default(Vector3)) ? defaultTunnelSpawnPoint : targetPosition));
    }
    
    /// <summary>
    /// Transitions to the port scene
    /// </summary>
    public void TransitionToPort(Vector3 targetPosition = default)
    {
        if (isTransitioning)
            return;
            
        // If we're already in the port scene, just teleport the player
        if (currentSceneName == portSceneName)
        {
            TeleportPlayer(targetPosition.Equals(default(Vector3)) ? defaultPortSpawnPoint : targetPosition);
            return;
        }
        
        // Start transition to port scene
        StartCoroutine(TransitionToScene(portSceneName, targetPosition.Equals(default(Vector3)) ? defaultPortSpawnPoint : targetPosition));
    }
    
    /// <summary>
    /// Loads a scene by name
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (isTransitioning)
            return;
            
        // Start transition to the specified scene
        StartCoroutine(TransitionToScene(sceneName, Vector3.zero));
    }
    
    /// <summary>
    /// Teleports the player to a specific position in the current scene
    /// </summary>
    public void TeleportPlayer(Vector3 position)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Check if player has a character controller
            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null)
            {
                // Disable controller before teleporting to avoid issues
                controller.enabled = false;
                player.transform.position = position;
                controller.enabled = true;
            }
            else
            {
                // Just set position directly if no character controller
                player.transform.position = position;
            }
            
            Debug.Log("Player teleported to " + position);
            
            // Update UI with current area name
            if (uiManager != null && sceneToAreaName.ContainsKey(currentSceneName))
            {
                uiManager.ShowAreaName(sceneToAreaName[currentSceneName]);
            }
        }
        else
        {
            Debug.LogWarning("Player not found for teleportation!");
        }
    }
    
    #endregion
    
    #region Private Methods
    
    /// <summary>
    /// Handler for when a scene is loaded
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Update current scene name
        currentSceneName = scene.name;
        
        // Find or create player if needed
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Player not found in loaded scene. Player should be pre-placed or dynamically spawned.");
        }
        
        // Apply saved player state if needed
        if (player != null && !savedPlayerPosition.Equals(default(Vector3)))
        {
            TeleportPlayer(savedPlayerPosition);
            player.transform.rotation = savedPlayerRotation;
            
            // Reset saved state
            savedPlayerPosition = default(Vector3);
            savedPlayerRotation = Quaternion.identity;
        }
        
        // Show area name if UI manager is available
        if (uiManager != null && sceneToAreaName.ContainsKey(currentSceneName))
        {
            uiManager.ShowAreaName(sceneToAreaName[currentSceneName]);
        }
        
        // Hide loading screen
        if (useLoadingScreen && uiManager != null)
        {
            uiManager.HideLoadingScreen();
        }
        
        // Start fade in
        if (uiManager != null)
        {
            StartCoroutine(uiManager.FadeFromBlack());
        }
        
        // Reset transitioning flag
        isTransitioning = false;
        
        Debug.Log("Scene loaded: " + scene.name);
    }
    
    /// <summary>
    /// Coroutine to handle scene transitions
    /// </summary>
    private IEnumerator TransitionToScene(string sceneName, Vector3 targetPosition)
    {
        isTransitioning = true;
        
        // Save player state if needed
        SavePlayerState();
        
        // Show area name
        string areaName = "Loading...";
        if (sceneToAreaName.ContainsKey(sceneName))
        {
            areaName = sceneToAreaName[sceneName];
        }
        
        if (uiManager != null)
        {
            uiManager.ShowAreaName(areaName);
        }
        
        // Start fade out
        if (uiManager != null)
        {
            yield return StartCoroutine(uiManager.FadeToBlack());
        }
        else
        {
            yield return new WaitForSeconds(fadeOutDuration);
        }
        
        // Show loading screen if needed
        float loadStartTime = Time.time;
        if (useLoadingScreen && uiManager != null)
        {
            uiManager.ShowLoadingScreen();
        }
        
        // Load the scene asynchronously
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        
        // Don't allow scene activation until fade is complete
        asyncLoad.allowSceneActivation = false;
        
        // Wait for load to complete
        while (!asyncLoad.isDone)
        {
            // Update loading progress
            if (uiManager != null)
            {
                uiManager.UpdateLoadingProgress(asyncLoad.progress / 0.9f); // AsyncOperation goes to 0.9 when loading is done
            }
            
            // Check if load is nearly finished
            if (asyncLoad.progress >= 0.9f)
            {
                // Ensure minimum loading time for visual consistency
                float elapsedLoadTime = Time.time - loadStartTime;
                if (elapsedLoadTime < minimumLoadingScreenTime)
                {
                    yield return new WaitForSeconds(minimumLoadingScreenTime - elapsedLoadTime);
                }
                
                // Allow scene activation
                asyncLoad.allowSceneActivation = true;
            }
            
            yield return null;
        }
        
        // The OnSceneLoaded callback will handle the rest of the transition
    }
    
    /// <summary>
    /// Saves the current player state before a scene transition
    /// </summary>
    private void SavePlayerState()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            savedPlayerPosition = player.transform.position;
            savedPlayerRotation = player.transform.rotation;
        }
    }
    
    #endregion
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// SceneManager handles scene transitions and area changes for PDX Underground.
/// Coordinates with UIManager for smooth transitions between different parts of historical Portland.
/// </summary>
public class SceneManager : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Scene References")]
    [SerializeField] private string streetSceneName = "PortlandStreets_Test";
    [SerializeField] private string tunnelsSceneName = "ShanhaiTunnels";
    [SerializeField] private string portSceneName = "PortOfPortland";
    
    [Header("Transition Settings")]
    [SerializeField] private float fadeOutDuration = 1.0f;
    [SerializeField] private float fadeInDuration = 1.0f;
    [SerializeField] private bool useLoadingScreen = true;
    [SerializeField] private float minLoadingScreenTime = 1.5f;
    
    [Header("Spawn Points")]
    [SerializeField] private Transform defaultSpawnPoint;
    [SerializeField] private Transform[] streetSpawnPoints;
    [SerializeField] private Transform[] tunnelSpawnPoints;
    [SerializeField] private Transform[] portSpawnPoints;
    
    [Header("Area Names")]
    [SerializeField] private string streetsAreaName = "Portland Streets";
    [SerializeField] private string tunnelsAreaName = "Shanghai Tunnels";
    [SerializeField] private string portAreaName = "Port of Portland";
    
    #endregion
    
    #region Private Variables
    
    private UIManager uiManager;
    private PortlandEnvironmentSetup environmentSetup;
    private string currentScene;
    private string currentAreaName;
    private Coroutine sceneLoadCoroutine;
    private Dictionary<string, Dictionary<string, Vector3>> transitionPositions;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        // Find necessary components
        uiManager = FindObjectOfType<UIManager>();
        environmentSetup = FindObjectOfType<PortlandEnvironmentSetup>();
        
        // Initialize transition positions
        InitializeTransitionPositions();
        
        // Store current scene name
        currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        // Set current area name based on scene
        SetCurrentAreaNameFromScene();
    }
    
    private void Start()
    {
        // If no spawn points are set, create a default one
        if (defaultSpawnPoint == null)
        {
            GameObject spawnPointObj = new GameObject("DefaultSpawnPoint");
            spawnPointObj.transform.position = new Vector3(0, 0.5f, 0);
            defaultSpawnPoint = spawnPointObj.transform;
        }
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Loads a new scene with fading transition
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (sceneLoadCoroutine != null)
        {
            StopCoroutine(sceneLoadCoroutine);
        }
        
        // Start scene loading coroutine
        sceneLoadCoroutine = StartCoroutine(LoadSceneCoroutine(sceneName));
    }
    
    /// <summary>
    /// Transitions player to the tunnels area
    /// </summary>
    public void TransitionToTunnels(Vector3 targetPosition)
    {
        if (currentScene == tunnelsSceneName)
        {
            // Already in tunnels, just move player
            MovePlayerToPosition(targetPosition);
            ShowAreaName(tunnelsAreaName);
        }
        else
        {
            // Save target position for tunnels scene
            StoreDestinationPosition("tunnels", targetPosition);
            
            // Load tunnels scene
            LoadScene(tunnelsSceneName);
        }
    }
    
    /// <summary>
    /// Transitions player to the port area
    /// </summary>
    public void TransitionToPort(Vector3 targetPosition)
    {
        if (currentScene == portSceneName)
        {
            // Already in port, just move player
            MovePlayerToPosition(targetPosition);
            ShowAreaName(portAreaName);
        }
        else
        {
            // Save target position for port scene
            StoreDestinationPosition("port", targetPosition);
            
            // Load port scene
            LoadScene(portSceneName);
        }
    }
    
    /// <summary>
    /// Transitions player to the streets area
    /// </summary>
    public void TransitionToStreets(Vector3 targetPosition)
    {
        if (currentScene == streetSceneName)
        {
            // Already in streets, just move player
            MovePlayerToPosition(targetPosition);
            ShowAreaName(streetsAreaName);
        }
        else
        {
            // Save target position for streets scene
            StoreDestinationPosition("streets", targetPosition);
            
            // Load streets scene
            LoadScene(streetSceneName);
        }
    }
    
    /// <summary>
    /// Gets the nearest spawn point to a position
    /// </summary>
    public Vector3 GetNearestSpawnPoint(Vector3 position)
    {
        // If we have an environment setup, use its spawn point system
        if (environmentSetup != null)
        {
            return environmentSetup.GetNearestSpawnPoint(position);
        }
        
        // Otherwise use our spawn points based on current scene
        Transform[] relevantSpawnPoints = GetCurrentSceneSpawnPoints();
        
        if (relevantSpawnPoints.Length == 0)
        {
            // If no spawn points, use default
            if (defaultSpawnPoint != null)
            {
                return defaultSpawnPoint.position;
            }
            else
            {
                return new Vector3(0, 0.5f, 0);
            }
        }
        
        // Find closest spawn point
        Transform closestSpawn = relevantSpawnPoints[0];
        float closestDistance = Vector3.Distance(position, closestSpawn.position);
        
        foreach (Transform spawn in relevantSpawnPoints)
        {
            float distance = Vector3.Distance(position, spawn.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestSpawn = spawn;
            }
        }
        
        return closestSpawn.position;
    }
    
    /// <summary>
    /// Shows the area name in the UI
    /// </summary>
    public void ShowAreaName(string areaName)
    {
        if (uiManager != null)
        {
            uiManager.ShowAreaName(areaName);
        }
    }
    
    #endregion
    
    #region Private Methods
    
    /// <summary>
    /// Coroutine to load a scene with transitions
    /// </summary>
    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        // Get destination position if we have one
        Vector3 destinationPosition = GetDestinationPosition(sceneName);
        
        // Fade out current scene
        if (uiManager != null)
        {
            yield return uiManager.FadeToBlack();
            
            // Show loading screen if enabled
            if (useLoadingScreen)
            {
                uiManager.ShowLoadingScreen();
            }
        }
        else
        {
            yield return new WaitForSeconds(fadeOutDuration);
        }
        
        // Start loading the scene asynchronously
        AsyncOperation loadOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        loadOperation.allowSceneActivation = true;
        
        // Wait for scene to load
        float loadStartTime = Time.time;
        
        while (!loadOperation.isDone)
        {
            // Update loading progress
            if (uiManager != null && useLoadingScreen)
            {
                uiManager.UpdateLoadingProgress(loadOperation.progress);
            }
            
            yield return null;
        }
        
        // Ensure loading screen shows for minimum time
        float loadTime = Time.time - loadStartTime;
        if (loadTime < minLoadingScreenTime && useLoadingScreen)
        {
            yield return new WaitForSeconds(minLoadingScreenTime - loadTime);
        }
        
        // Update current scene reference
        currentScene = sceneName;
        
        // Set area name based on new scene
        SetCurrentAreaNameFromScene();
        
        // If we have a destination position, move player there
        if (destinationPosition != Vector3.zero)
        {
            MovePlayerToPosition(destinationPosition);
        }
        else
        {
            // Otherwise spawn at default location for this scene
            SpawnPlayerAtDefaultLocation();
        }
        
        // Hide loading screen
        if (uiManager != null && useLoadingScreen)
        {
            uiManager.HideLoadingScreen();
        }
        
        // Show area name
        if (uiManager != null)
        {
            uiManager.ShowAreaName(currentAreaName);
        }
        
        // Fade in new scene
        if (uiManager != null)
        {
            yield return uiManager.FadeFromBlack();
        }
        else
        {
            yield return new WaitForSeconds(fadeInDuration);
        }
        
        sceneLoadCoroutine = null;
    }
    
    /// <summary>
    /// Initializes transition positions dictionary
    /// </summary>
    private void InitializeTransitionPositions()
    {
        transitionPositions = new Dictionary<string, Dictionary<string, Vector3>>();
        
        // Initialize dictionaries for each scene
        transitionPositions["streets"] = new Dictionary<string, Vector3>();
        transitionPositions["tunnels"] = new Dictionary<string, Vector3>();
        transitionPositions["port"] = new Dictionary<string, Vector3>();
    }
    
    /// <summary>
    /// Stores a destination position for scene transitions
    /// </summary>
    private void StoreDestinationPosition(string sceneKey, Vector3 position)
    {
        // Get key for current scene
        string sourceSceneKey = GetKeyForScene(currentScene);
        
        // Store the position
        if (transitionPositions.ContainsKey(sceneKey))
        {
            transitionPositions[sceneKey][sourceSceneKey] = position;
        }
    }
    
    /// <summary>
    /// Gets a destination position for the target scene
    /// </summary>
    private Vector3 GetDestinationPosition(string targetScene)
    {
        // Get key for target scene
        string targetSceneKey = GetKeyForScene(targetScene);
        
        // Get key for source scene
        string sourceSceneKey = GetKeyForScene(currentScene);
        
        // Look up the position
        if (transitionPositions.ContainsKey(targetSceneKey) && 
            transitionPositions[targetSceneKey].ContainsKey(sourceSceneKey))
        {
            return transitionPositions[targetSceneKey][sourceSceneKey];
        }
        
        return Vector3.zero;
    }
    
    /// <summary>
    /// Gets the key for a scene name
    /// </summary>
    private string GetKeyForScene(string scene)
    {
        if (scene == streetSceneName)
            return "streets";
        else if (scene == tunnelsSceneName)
            return "tunnels";
        else if (scene == portSceneName)
            return "port";
            
        return "unknown";
    }
    
    /// <summary>
    /// Sets the current area name based on scene
    /// </summary>
    private void SetCurrentAreaNameFromScene()
    {
        if (currentScene == streetSceneName)
            currentAreaName = streetsAreaName;
        else if (currentScene == tunnelsSceneName)
            currentAreaName = tunnelsAreaName;
        else if (currentScene == portSceneName)
            currentAreaName = portAreaName;
        else
            currentAreaName = "Unknown Area";
    }
    
    /// <summary>
    /// Gets the spawn points for current scene
    /// </summary>
    private Transform[] GetCurrentSceneSpawnPoints()
    {
        if (currentScene == streetSceneName)
            return streetSpawnPoints;
        else if (currentScene == tunnelsSceneName)
            return tunnelSpawnPoints;
        else if (currentScene == portSceneName)
            return portSpawnPoints;
            
        return new Transform[0];
    }
    
    /// <summary>
    /// Moves the player to a specific position
    /// </summary>
    private void MovePlayerToPosition(Vector3 position)
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = position;
        }
        else
        {
            Debug.LogWarning("Player not found for repositioning!");
        }
    }
    
    /// <summary>
    /// Spawns the player at the default location for the current scene
    /// </summary>
    private void SpawnPlayerAtDefaultLocation()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Player not found for spawning!");
            return;
        }
        
        // Get spawn points for current scene
        Transform[] spawnPoints = GetCurrentSceneSpawnPoints();
        
        // Use first spawn point if available, otherwise use default
        if (spawnPoints.Length > 0 && spawnPoints[0] != null)
        {
            player.transform.position = spawnPoints[0].position;
        }
        else if (defaultSpawnPoint != null)
        {
            player.transform.position = defaultSpawnPoint.position;
        }
    }
    
    #endregion
}

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// SceneManager handles loading, unloading, and transitioning between scenes in PDX Underground.
/// Manages the multi-scene approach for the Portland historical environment.
/// </summary>
public class SceneManager : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Scene References")]
    [SerializeField] private string portlandStreetsSceneName = "PortlandStreets";
    [SerializeField] private string shanghaiTunnelsSceneName = "ShanghaiTunnels";
    [SerializeField] private string portOfPortlandSceneName = "PortOfPortland";
    
    [Header("Transition Settings")]
    [SerializeField] private float crossFadeDuration = 1.5f;
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Loading UI")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private UnityEngine.UI.Slider progressBar;
    [SerializeField] private TMPro.TextMeshProUGUI loadingText;
    
    #endregion
    
    #region Private Variables
    
    private bool isTransitioning = false;
    private List<AsyncOperation> scenesLoading = new List<AsyncOperation>();
    private Dictionary<string, bool> loadedScenes = new Dictionary<string, bool>();
    
    #endregion
    
    #region Singleton Pattern
    
    public static SceneManager Instance { get; private set; }
    
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
        
        // Initialize the loaded scenes dictionary
        loadedScenes.Add(portlandStreetsSceneName, false);
        loadedScenes.Add(shanghaiTunnelsSceneName, false);
        loadedScenes.Add(portOfPortlandSceneName, false);
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Loads the initial Portland Streets scene as the main scene
    /// </summary>
    public void LoadInitialScene()
    {
        StartCoroutine(LoadScene(portlandStreetsSceneName, true));
    }
    
    /// <summary>
    /// Transitions the player to the Shanghai Tunnels
    /// </summary>
    public void TransitionToTunnels(Vector3 playerPosition)
    {
        // If already in tunnels, just reposition the player
        if (loadedScenes[shanghaiTunnelsSceneName])
        {
            RepositionPlayer(playerPosition);
            return;
        }
        
        StartCoroutine(TransitionBetweenAreas(portlandStreetsSceneName, shanghaiTunnelsSceneName, playerPosition));
    }
    
    /// <summary>
    /// Transitions the player to the Port of Portland
    /// </summary>
    public void TransitionToPort(Vector3 playerPosition)
    {
        // If already at port, just reposition the player
        if (loadedScenes[portOfPortlandSceneName])
        {
            RepositionPlayer(playerPosition);
            return;
        }
        
        StartCoroutine(TransitionBetweenAreas(portlandStreetsSceneName, portOfPortlandSceneName, playerPosition));
    }
    
    /// <summary>
    /// Transitions the player back to the streets from either tunnels or port
    /// </summary>
    public void TransitionToStreets(Vector3 playerPosition)
    {
        // Determine which scene to transition from based on what's currently loaded
        string fromScene = loadedScenes[shanghaiTunnelsSceneName] ? shanghaiTunnelsSceneName : portOfPortlandSceneName;
        
        StartCoroutine(TransitionBetweenAreas(fromScene, portlandStreetsSceneName, playerPosition));
    }
    
    #endregion
    
    #region Private Methods
    
    /// <summary>
    /// Handles the transition between two areas with proper visual effects
    /// </summary>
    private IEnumerator TransitionBetweenAreas(string fromScene, string toScene, Vector3 newPlayerPosition)
    {
        if (isTransitioning)
            yield break;
            
        isTransitioning = true;
        
        // Fade out
        yield return StartCoroutine(FadeOut());
        
        // Unload the current scene
        if (loadedScenes[fromScene])
        {
            AsyncOperation unloadOperation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(fromScene);
            yield return unloadOperation;
            loadedScenes[fromScene] = false;
        }
        
        // Load the new scene
        yield return StartCoroutine(LoadScene(toScene, false));
        
        // Reposition the player at the destination
        RepositionPlayer(newPlayerPosition);
        
        // Fade back in
        yield return StartCoroutine(FadeIn());
        
        isTransitioning = false;
    }
    
    /// <summary>
    /// Loads a scene additively or as a single scene
    /// </summary>
    private IEnumerator LoadScene(string sceneName, bool isSingleScene)
    {
        // Show loading screen
        if (loadingScreen != null)
            loadingScreen.SetActive(true);
            
        // Start loading the scene
        AsyncOperation loadOperation;
        
        if (isSingleScene)
        {
            loadOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        }
        else
        {
            loadOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        }
        
        loadOperation.allowSceneActivation = false;
        
        // Update progress bar
        while (loadOperation.progress < 0.9f)
        {
            if (progressBar != null)
                progressBar.value = loadOperation.progress;
                
            if (loadingText != null)
                loadingText.text = $"Loading {sceneName}... {(loadOperation.progress * 100):0}%";
                
            yield return null;
        }
        
        // When loading is complete
        loadOperation.allowSceneActivation = true;
        loadedScenes[sceneName] = true;
        
        // Hide loading screen
        if (loadingScreen != null)
            loadingScreen.SetActive(false);
            
        yield return loadOperation;
    }
    
    /// <summary>
    /// Repositions the player to a new location
    /// </summary>
    private void RepositionPlayer(Vector3 newPosition)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = newPosition;
        }
        else
        {
            Debug.LogError("Player not found! Make sure the player has the 'Player' tag.");
        }
    }
    
    /// <summary>
    /// Fades the screen to black
    /// </summary>
    private IEnumerator FadeOut()
    {
        float elapsedTime = 0;
        
        while (elapsedTime < crossFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = fadeCurve.Evaluate(elapsedTime / crossFadeDuration);
            fadeCanvasGroup.alpha = alpha;
            yield return null;
        }
        
        fadeCanvasGroup.alpha = 1;
    }
    
    /// <summary>
    /// Fades the screen from black to clear
    /// </summary>
    private IEnumerator FadeIn()
    {
        float elapsedTime = 0;
        
        while (elapsedTime < crossFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = 1 - fadeCurve.Evaluate(elapsedTime / crossFadeDuration);
            fadeCanvasGroup.alpha = alpha;
            yield return null;
        }
        
        fadeCanvasGroup.alpha = 0;
    }
    
    #endregion
}

