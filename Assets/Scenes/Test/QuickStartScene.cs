using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using PDXUnderground.Test.Prefabs;
using PDXUnderground.Core;

namespace PDXUnderground.Test
{
    /// <summary>
    /// QuickStartScene provides a single-click solution to set up and run the PDX Underground test scene.
    /// This script automatically creates and configures all necessary components for immediate testing.
    /// </summary>
    [ExecuteInEditMode]
    public class QuickStartScene : MonoBehaviour
    {
        [Header("QuickStart Settings")]
        [Tooltip("Enable to configure the scene in test mode automatically")]
        [SerializeField] private bool autoSetupTestMode = true;
        
        [Tooltip("Enable to automatically create required prefabs if they don't exist")]
        [SerializeField] private bool autoCreatePrefabs = true;
        
        [Tooltip("Enable to show debugging information")]
        [SerializeField] private bool enableDebugUI = true;
        
        [Tooltip("The starting environment to use (0=Streets, 1=Tunnels, 2=Speakeasy)")]
        [Range(0, 2)]
        [SerializeField] private int startingEnvironment = 0;
        
        [Header("Scene Configuration")]
        [Tooltip("Reference to the TestSceneLayout component (auto-assigned if not set)")]
        [SerializeField] private TestSceneLayout sceneLayout;
        
        [Tooltip("Reference to the TestSceneInitializer (auto-assigned if not set)")]
        [SerializeField] private TestSceneInitializer sceneInitializer;
        
        [Header("Prefab References")]
        [Tooltip("The player prefab (auto-created if not set)")]
        [SerializeField] private GameObject playerPrefab;
        
        [Tooltip("The main game controller prefab (auto-created if not set)")]
        [SerializeField] private GameObject mainGameControllerPrefab;
        
        [Tooltip("The buzz UI controller prefab (auto-created if not set)")]
        [SerializeField] private GameObject buzzUIPrefab;
        
        // Created GameObjects
        private GameObject playerObj;
        private GameObject mainGameControllerObj;
        private GameObject buzzUIObj;
        
        private bool setupComplete = false;
        
        private void OnEnable()
        {
            // Only run in edit mode for initial setup
            if (!Application.isPlaying)
            {
                if (sceneLayout == null)
                {
                    // Try to find an existing TestSceneLayout in the scene
                    sceneLayout = FindObjectOfType<TestSceneLayout>();
                    
                    // If not found, check if we need to add it to this GameObject
                    if (sceneLayout == null && gameObject.GetComponent<TestSceneLayout>() == null)
                    {
                        sceneLayout = gameObject.AddComponent<TestSceneLayout>();
                        Debug.Log("Added TestSceneLayout component automatically");
                    }
                }
            }
        }
        
        /// <summary>
        /// Main method to quickly set up the entire test scene
        /// </summary>
        [ContextMenu("Quick Start Test Scene")]
        public void QuickStartTestScene()
        {
            // Clear setup flag
            setupComplete = false;
            
            // Create scene hierarchy
            SetupSceneHierarchy();
            
            // Create and configure required prefabs
            if (autoCreatePrefabs)
            {
                CreateRequiredPrefabs();
            }
            
            // Configure the TestSceneInitializer
            ConfigureTestSceneInitializer();
            
            // Set up test mode
            if (autoSetupTestMode)
            {
                if (sceneInitializer != null)
                {
                    sceneInitializer.SetTestMode(true);
                }
            }
            
            // Mark setup as complete
            setupComplete = true;
            
            Debug.Log("Quick Start setup complete! Press Play in Unity to begin testing.");
            Debug.Log("Test Controls: F1=Toggle Debug UI, F2=Toggle Test Controls, 1/2/3=Switch Environments");
        }
        
        /// <summary>
        /// Sets up the complete scene hierarchy
        /// </summary>
        private void SetupSceneHierarchy()
        {
            if (sceneLayout != null)
            {
                // Create the complete scene hierarchy
                sceneLayout.CreateSceneHierarchy();
                
                // Try to find the TestSceneInitializer
                if (sceneInitializer == null)
                {
                    sceneInitializer = FindObjectOfType<TestSceneInitializer>();
                }
                
                Debug.Log("Scene hierarchy created successfully");
            }
            else
            {
                Debug.LogError("TestSceneLayout component is missing. Cannot create scene hierarchy.");
            }
        }
        
        /// <summary>
        /// Creates all required prefabs if they don't exist
        /// </summary>
        private void CreateRequiredPrefabs()
        {
            // Create player prefab
            if (playerPrefab == null)
            {
                playerObj = new GameObject("PlayerPrefab");
                PlayerPrefab playerComponent = playerObj.AddComponent<PlayerPrefab>();
                playerComponent.CreatePrefabStructure();
                playerPrefab = playerObj;
                Debug.Log("Created player prefab");
            }
            
            // Create main game controller prefab
            if (mainGameControllerPrefab == null)
            {
                mainGameControllerObj = new GameObject("MainGameControllerPrefab");
                MainGameControllerPrefab controllerComponent = mainGameControllerObj.AddComponent<MainGameControllerPrefab>();
                controllerComponent.CreatePrefabStructure();
                mainGameControllerPrefab = mainGameControllerObj;
                Debug.Log("Created main game controller prefab");
            }
            
            // Create buzz UI prefab
            if (buzzUIPrefab == null)
            {
                buzzUIObj = new GameObject("BuzzUIPrefab");
                BuzzUIPrefab uiComponent = buzzUIObj.AddComponent<BuzzUIPrefab>();
                buzzUIPrefab = buzzUIObj;
                Debug.Log("Created buzz UI prefab");
            }
        }
        
        /// <summary>
        /// Configures the TestSceneInitializer with all necessary settings
        /// </summary>
        private void ConfigureTestSceneInitializer()
        {
            if (sceneInitializer != null)
            {
                // Set test configuration through reflection or direct assignment
                // In a real implementation, we would set all properties of TestSceneInitializer
                
                // Set the debug UI
                var debugUIField = sceneInitializer.GetType().GetField("enableDebugUI", 
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (debugUIField != null)
                {
                    debugUIField.SetValue(sceneInitializer, enableDebugUI);
                }
                
                // Set test controls
                var testControlsField = sceneInitializer.GetType().GetField("enableTestControls", 
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (testControlsField != null)
                {
                    testControlsField.SetValue(sceneInitializer, true);
                }
                
                // Set auto setup
                var autoSetupField = sceneInitializer.GetType().GetField("autoSetupTestEnvironment", 
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (autoSetupField != null)
                {
                    autoSetupField.SetValue(sceneInitializer, true);
                }
                
                Debug.Log("TestSceneInitializer configured for quick testing");
            }
            else
            {
                Debug.LogError("TestSceneInitializer not found. Scene may not function correctly.");
            }
        }
        
        /// <summary>
        /// Called when Play mode is entered, ensures environment is ready for testing
        /// </summary>
        private void Start()
        {
            if (Application.isPlaying)
            {
                if (!setupComplete)
                {
                    Debug.LogWarning("Test scene may not be fully set up. Consider running Quick Start before playing.");
                }
                
                // Set the starting environment
                StartCoroutine(DelayedEnvironmentChange());
            }
        }
        
        private IEnumerator DelayedEnvironmentChange()
        {
            // Wait for one frame to ensure all components are initialized
            yield return null;
            
            // Set the environment if needed
            if (startingEnvironment != 0)
            {
                // Find the TestSceneInitializer if not already set
                if (sceneInitializer == null)
                {
                    sceneInitializer = FindObjectOfType<TestSceneInitializer>();
                }
                
                if (sceneInitializer != null)
                {
                    // Call the change environment method
                    sceneInitializer.SendMessage("ChangeSceneEnvironment", startingEnvironment, 
                        SendMessageOptions.DontRequireReceiver);
                    
                    Debug.Log("Set starting environment to " + GetEnvironmentName(startingEnvironment));
                }
            }
        }
        
        private string GetEnvironmentName(int index)
        {
            switch (index)
            {
                case 0:
                    return "Streets";
                case 1:
                    return "Tunnels";
                case 2:
                    return "Speakeasy";
                default:
                    return "Unknown";
            }
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// Menu item to create a new test scene
        /// </summary>
        [MenuItem("PDX Underground/Create Test Scene")]
        public static void CreateTestScene()
        {
            // Create a new scene
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // Create the QuickStartScene GameObject
            GameObject quickStartObj = new GameObject("QuickStartSetup");
            QuickStartScene quickStart = quickStartObj.AddComponent<QuickStartScene>();
            
            // Run the quick start setup
            quickStart.QuickStartTestScene();
            
            // Focus on the QuickStartScene GameObject
            Selection.activeGameObject = quickStartObj;
            
            // Suggest saving
            EditorUtility.DisplayDialog("Test Scene Created", 
                "Test scene created successfully. Press Play to start testing.\n\nDon't forget to save your scene!", "OK");
        }
#endif
        
        /// <summary>
        /// Instructions displayed in the inspector
        /// </summary>
        [Header("How To Use")]
        [TextArea(5, 10)]
        public string instructions = 
            "QUICK START INSTRUCTIONS:\n\n" +
            "1. Click 'Quick Start Test Scene' in the context menu (gear icon)\n" +
            "2. Wait for automatic setup to complete\n" +
            "3. Press Play to begin testing\n\n" +
            "SHORTCUTS DURING PLAY:\n" +
            "F1: Toggle debug display\n" +
            "F2: Toggle test controls\n" +
            "1/2/3: Switch environments\n" +
            "T: Draw card";
    }
}

