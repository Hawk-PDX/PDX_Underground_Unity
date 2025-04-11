using UnityEngine;

namespace PDXUnderground.Core
{
    /// <summary>
    /// Base class for setting up game scenes in PDX Underground.
    /// Provides common functionality for initializing game components and environments.
    /// </summary>
    public class GameSceneSetup : MonoBehaviour
    {
        [Header("Scene Configuration")]
        [Tooltip("Whether debug mode is enabled")]
        [SerializeField] protected bool isDebugMode = false;
        
        [Tooltip("Whether test mode is enabled")]
        [SerializeField] protected bool isTestMode = false;
        
        [Tooltip("Whether to automatically initialize on start")]
        [SerializeField] protected bool autoInitialize = true;
        
        [Header("Environment")]
        [Tooltip("Available environment names")]
        [SerializeField] protected string[] availableEnvironments = { "Streets", "Tunnels", "Speakeasy" };
        
        [Tooltip("Default environment index")]
        [SerializeField] protected int defaultEnvironmentIndex = 0;
        
        [Header("References")]
        [Tooltip("Reference to the player prefab")]
        [SerializeField] protected GameObject playerPrefab;
        
        [Tooltip("Reference to the main game controller prefab")]
        [SerializeField] protected GameObject mainGameControllerPrefab;
        
        protected GameObject playerInstance;
        protected GameObject mainGameControllerInstance;
        protected int currentEnvironmentIndex;
        
        /// <summary>
        /// Sets up the scene on start if auto-initialize is enabled
        /// </summary>
        protected virtual void Start()
        {
            if (autoInitialize)
            {
                InitializeScene();
            }
        }
        
        /// <summary>
        /// Initializes the scene with all required components
        /// </summary>
        public virtual void InitializeScene()
        {
            Debug.Log("Initializing game scene...");
            
            // Create main game controller
            CreateMainGameController();
            
            // Create player
            CreatePlayer();
            
            // Set initial environment
            SetEnvironment(defaultEnvironmentIndex);
            
            Debug.Log("Scene initialization complete.");
        }
        
        /// <summary>
        /// Creates the main game controller
        /// </summary>
        protected virtual void CreateMainGameController()
        {
            if (mainGameControllerPrefab != null)
            {
                mainGameControllerInstance = Instantiate(mainGameControllerPrefab);
                mainGameControllerInstance.name = "MainGameController";
                
                Debug.Log("Created MainGameController");
            }
            else
            {
                Debug.LogError("MainGameController prefab is not assigned.");
            }
        }
        
        /// <summary>
        /// Creates the player character
        /// </summary>
        protected virtual void CreatePlayer()
        {
            if (playerPrefab != null)
            {
                Vector3 spawnPosition = Vector3.zero;
                
                // Try to find a spawn point
                GameObject spawnPoint = GameObject.Find("PlayerSpawn");
                if (spawnPoint != null)
                {
                    spawnPosition = spawnPoint.transform.position;
                }
                
                playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
                playerInstance.name = "Player";
                
                Debug.Log("Created Player at " + spawnPosition);
            }
            else
            {
                Debug.LogError("Player prefab is not assigned.");
            }
        }
        
        /// <summary>
        /// Sets the active environment
        /// </summary>
        public virtual void SetEnvironment(int environmentIndex)
        {
            if (environmentIndex >= 0 && environmentIndex < availableEnvironments.Length)
            {
                currentEnvironmentIndex = environmentIndex;
                string environmentName = availableEnvironments[environmentIndex];
                
                Debug.Log("Setting environment to: " + environmentName);
                
                // Implementation would load the appropriate environment assets
                // This is a minimal implementation for testing
            }
            else
            {
                Debug.LogError("Invalid environment index: " + environmentIndex);
            }
        }
        
        /// <summary>
        /// Sets whether test mode is enabled
        /// </summary>
        public virtual void SetTestMode(bool enabled)
        {
            isTestMode = enabled;
            Debug.Log("Test mode " + (enabled ? "enabled" : "disabled"));
        }
        
        /// <summary>
        /// Sets whether debug mode is enabled
        /// </summary>
        public virtual void SetDebugMode(bool enabled)
        {
            isDebugMode = enabled;
            Debug.Log("Debug mode " + (enabled ? "enabled" : "disabled"));
        }
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace PDXUnderground.Core
{
    /// <summary>
    /// Handles the setup and initialization of game scenes, including the creation
    /// of required game objects, setting up environments, and ensuring compatibility
    /// with the test framework.
    /// </summary>
    public class GameSceneSetup : MonoBehaviour
    {
        #region Scene Configuration
        [Header("Required Prefabs")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject mainGameControllerPrefab;
        [SerializeField] private GameObject buzzUIControllerPrefab;
        
        [Header("Environment")]
        [SerializeField] private Material groundMaterial;
        [SerializeField] private Material wallMaterial;
        [SerializeField] private Material playerMaterial;
        [SerializeField] private Vector2 groundSize = new Vector2(20f, 20f);
        [SerializeField] private float wallHeight = 4f;
        [SerializeField] private Vector3 playerStartPosition = new Vector3(0f, 1f, 0f);
        
        [Header("Lighting")]
        [SerializeField] private Light mainDirectionalLight;
        [SerializeField] private Color ambientLightColor = new Color(0.2f, 0.2f, 0.2f);
        [SerializeField] private bool useRealtimeLighting = true;
        
        [Header("Scene Setup")]
        [SerializeField] private bool autoInitializeOnStart = true;
        [SerializeField] private bool createEnvironmentBoundaries = true;
        [SerializeField] private bool isTestMode = false;
        
        // References to created objects
        private GameObject groundPlane;
        private GameObject[] walls;
        private GameObject playerInstance;
        private GameObject mainGameControllerInstance;
        private GameObject buzzUIControllerInstance;
        #endregion
        
        #region Unity Lifecycle
        private void Awake()
        {
            // Register with SceneManager to be notified when scene is loaded
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void Start()
        {
            if (autoInitializeOnStart)
            {
                InitializeScene();
            }
        }
        
        private void OnDestroy()
        {
            // Unregister from SceneManager
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        #endregion
        
        #region Scene Initialization
        /// <summary>
        /// Called when a scene is loaded, checks if this is the active scene setup
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // If this is the active scene, initialize it
            if (scene == gameObject.scene && !autoInitializeOnStart)
            {
                InitializeScene();
            }
        }
        
        /// <summary>
        /// Initialize the scene with all required game objects and components
        /// </summary>
        public void InitializeScene()
        {
            Debug.Log("Initializing game scene...");
            
            // Create the core game elements
            CreateCoreGameSystems();
            
            // Create the environment
            if (createEnvironmentBoundaries)
            {
                CreateEnvironment();
            }
            
            // Create the player
            CreatePlayer();
            
            // Set up camera
            SetupCamera();
            
            // Set up lighting
            SetupLighting();
            
            // Set up UI
            SetupUI();
            
            // Configure test mode if needed
            if (isTestMode)
            {
                SetupTestEnvironment();
            }
            
            Debug.Log("Scene initialization complete!");
        }
        
        /// <summary>
        /// Creates the core game systems needed for the game to function
        /// </summary>
        private void CreateCoreGameSystems()
        {
            // Create MainGameController if it doesn't exist
            if (MainGameController.Instance == null && mainGameControllerPrefab != null)
            {
                mainGameControllerInstance = Instantiate(mainGameControllerPrefab);
                mainGameControllerInstance.name = "MainGameController";
                DontDestroyOnLoad(mainGameControllerInstance);
                Debug.Log("Created MainGameController");
            }
            else if (MainGameController.Instance != null)
            {
                Debug.Log("MainGameController already exists");
            }
            else
            {
                Debug.LogWarning("MainGameController prefab not assigned!");
            }
        }
        
        /// <summary>
        /// Creates the basic environment (ground and walls)
        /// </summary>
        private void CreateEnvironment()
        {
            // Create ground plane
            groundPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            groundPlane.name = "Ground";
            groundPlane.transform.localScale = new Vector3(groundSize.x / 10, 1, groundSize.y / 10);
            
            if (groundMaterial != null)
            {
                groundPlane.GetComponent<Renderer>().material = groundMaterial;
            }
            
            // Create walls
            walls = new GameObject[4];
            
            // Calculate wall positions and sizes
            float halfWidth = groundSize.x / 2;
            float halfDepth = groundSize.y / 2;
            
            // Create walls
            walls[0] = CreateWall("Wall_North", new Vector3(0, wallHeight / 2, halfDepth), new Vector3(groundSize.x, wallHeight, 0.1f));
            walls[1] = CreateWall("Wall_South", new Vector3(0, wallHeight / 2, -halfDepth), new Vector3(groundSize.x, wallHeight, 0.1f));
            walls[2] = CreateWall("Wall_East", new Vector3(halfWidth, wallHeight / 2, 0), new Vector3(0.1f, wallHeight, groundSize.y));
            walls[3] = CreateWall("Wall_West", new Vector3(-halfWidth, wallHeight / 2, 0), new Vector3(0.1f, wallHeight, groundSize.y));
            
            Debug.Log("Created environment boundaries");
        }
        
        /// <summary>
        /// Helper method to create a wall with the given name, position and size
        /// </summary>
        private GameObject CreateWall(string name, Vector3 position, Vector3 size)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.position = position;
            wall.transform.localScale = size;
            
            if (wallMaterial != null)
            {
                wall.GetComponent<Renderer>().material = wallMaterial;
            }
            
            return wall;
        }
        
        /// <summary>
        /// Creates the player character in the scene
        /// </summary>
        private void CreatePlayer()
        {
            if (playerPrefab != null)
            {
                playerInstance = Instantiate(playerPrefab, playerStartPosition, Quaternion.identity);
                playerInstance.name = "Player";
                
                // Check for required components
                if (!playerInstance.GetComponent<PDXUnderground.Player.PlayerController>())
                {
                    Debug.LogWarning("Player prefab does not have a PlayerController component");
                }
                
                if (!playerInstance.GetComponent<GamblerCharacter>())
                {
                    Debug.LogWarning("Player prefab does not have a GamblerCharacter component");
                }
                
                // Apply player material if available
                if (playerMaterial != null)
                {
                    Renderer[] renderers = playerInstance.GetComponentsInChildren<Renderer>();
                    foreach (Renderer renderer in renderers)
                    {
                        renderer.material = playerMaterial;
                    }
                }
                
                Debug.Log("Created player character");
            }
            else
            {
                Debug.LogError("Player prefab not assigned!");
            }
        }
        
        /// <summary>
        /// Sets up the main camera for the scene
        /// </summary>
        private void SetupCamera()
        {
            // Find or create main camera
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraObj = new GameObject("Main Camera");
                cameraObj.tag = "MainCamera";
                mainCamera = cameraObj.AddComponent<Camera>();
                cameraObj.AddComponent<AudioListener>();
                Debug.Log("Created main camera");
            }
            
            // Position the camera
            if (playerInstance != null)
            {
                // Set initial camera position behind player
                Vector3 cameraPosition = playerInstance.transform.position - playerInstance.transform.forward * 5f + Vector3.up * 3f;
                mainCamera.transform.position = cameraPosition;
                mainCamera.transform.LookAt(playerInstance.transform.position + Vector3.up);
                
                // Add simple follow camera script if needed
                if (!mainCamera.GetComponent<SimpleCameraFollow>())
                {
                    SimpleCameraFollow cameraFollow = mainCamera.gameObject.AddComponent<SimpleCameraFollow>();
                    cameraFollow.target = playerInstance.transform;
                    cameraFollow.offset = new Vector3(0, 3f, -5f);
                    cameraFollow.smoothSpeed = 0.125f;
                }
            }
        }
        
        /// <summary>
        /// Sets up lighting for the scene
        /// </summary>
        private void SetupLighting()
        {
            // Set ambient lighting
            RenderSettings.ambientLight = ambientLightColor;
            
            // Create or configure directional light
            if (mainDirectionalLight == null)
            {
                GameObject lightObj = new GameObject("Directional Light");
                mainDirectionalLight = lightObj.AddComponent<Light>();
                mainDirectionalLight.type = LightType.Directional;
                mainDirectionalLight.intensity = 1.0f;
                mainDirectionalLight.color = Color.white;
                
                // Position the light
                lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
                
                Debug.Log("Created directional light");
            }
            
            // Configure lighting settings
            if (useRealtimeLighting)
            {
                mainDirectionalLight.shadows = LightShadows.Soft;
            }
            else
            {
                mainDirectionalLight.shadows = LightShadows.None;
            }
        }
        
        /// <summary>
        /// Sets up the UI elements for the scene
        /// </summary>
        private void SetupUI()
        {
            // Create BuzzUIController if it doesn't exist
            if (FindObjectOfType<BuzzUIController>() == null && buzzUIControllerPrefab != null)
            {
                buzzUIControllerInstance = Instantiate(buzzUIControllerPrefab);
                buzzUIControllerInstance.name = "BuzzUIController";
                
                // Since this is likely a Canvas, we don't want to DontDestroyOnLoad
                Debug.Log("Created BuzzUIController");
            }
            else if (FindObjectOfType<BuzzUIController>() != null)
            {
                Debug.Log("BuzzUIController already exists");
            }
            else
            {
                Debug.LogWarning("BuzzUIController prefab not assigned but is needed for gameplay!");
            }
        }
        
        /// <summary>
        /// Configures the scene for test mode if needed
        /// </summary>
        private void SetupTestEnvironment()
        {
            // Create test buttons for controlling buzz and environments
            if (FindObjectOfType<BuzzUIController>() != null)
            {
                // Create test controller if needed
                GameObject testControllerObj = new GameObject("TestController");
                GamblerTestQuickStartExample testController = testControllerObj.AddComponent<GamblerTestQuickStartExample>();
                
                // Reference the BuzzUIController
                testController.buzzUIController = FindObjectOfType<BuzzUIController>();
                
                // Reference the GamblerCharacter
                if (playerInstance != null && playerInstance.GetComponent<GamblerCharacter>() != null)
                {
                    testController.gamblerCharacter = playerInstance.GetComponent<GamblerCharacter>();
                }
                
                Debug.Log("Created test environment with GamblerTestQuickStartExample");
            }
        }
        #endregion
        
        #region Public Methods
        /// <summary>
        /// Helper method to create a simple camera follow script
        /// </summary>
        public class SimpleCameraFollow : MonoBehaviour
        {
            public Transform target;
            public Vector3 offset = new Vector3(0, 2, -5);
            public float smoothSpeed = 0.125f;
            
            private void LateUpdate()
            {
                if (target == null)
                    return;
                    
                Vector3 desiredPosition = target.position + offset;
                Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
                transform.position = smoothedPosition;
                
                transform.LookAt(target.position + Vector3.up);
            }
        }
        
        /// <summary>
        /// Switches between test and production mode
        /// </summary>
        /// <param name="testMode">Whether to enable test mode</param>
        public void SetTestMode(bool testMode)
        {
            isTestMode = testMode;
            
            // Find and enable/disable test controls
            GamblerTestQuickStartExample testController = FindObjectOfType<GamblerTestQuickStartExample>();
            if (testController != null)
            {
                testController.gameObject.SetActive(testMode);
            }
            else if (testMode)
            {
                // Create test environment if it doesn't exist
                SetupTestEnvironment();
            }
            
            Debug.Log($"Test mode {(testMode ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Resets the scene to its initial state
        /// </summary>
        public void ResetScene()
        {
            // Destroy existing objects
            if (groundPlane != null) Destroy(groundPlane);
            if (walls != null)
            {
                foreach (GameObject wall in walls)
                {
                    if (wall != null) Destroy(wall);
                }
            }
            if (playerInstance != null) Destroy(playerInstance);
            
            // Reinitialize the scene
            InitializeScene();
            
            Debug.Log("Scene has been reset");
        }
        
        /// <summary>
        /// Changes the environment materials and lighting to match the specified environment type
        /// </summary>
        /// <param name="environmentIndex">Index of the environment to switch to</param>
        public void ChangeSceneEnvironment(int environmentIndex)
        {
            // Notify the MainGameController about the environment change
            if (MainGameController.Instance != null)
            {
                MainGameController.Instance.ChangeEnvironment(environmentIndex);
            }
            
            // This would change materials, lighting, etc. based on the environment type
            switch (environmentIndex)
            {
                case 0: // Streets
                    // Brighter lighting
                    if (mainDirectionalLight != null)
                    {
                        mainDirectionalLight.intensity = 1.0f;
                        mainDirectionalLight.color = new Color(1f, 0.95f, 0.9f);
                    }
                

