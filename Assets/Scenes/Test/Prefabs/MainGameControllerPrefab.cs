using UnityEngine;
using PDXUnderground.Core;

namespace PDXUnderground.Test.Prefabs
{
    /// <summary>
    /// Component used to configure a game object as a MainGameController prefab for the PDX Underground game.
    /// This script helps set up all required components and default values.
    /// </summary>
    [ExecuteInEditMode]
    public class MainGameControllerPrefab : MonoBehaviour
    {
        [Header("Environment Configuration")]
        [Tooltip("List of environment names available in the game")]
        [SerializeField] private string[] environmentNames = { "Streets", "Tunnels", "Speakeasy" };
        
        [Tooltip("Default environment index to start with")]
        [SerializeField] private int defaultEnvironmentIndex = 0;
        
        [Header("Starting State")]
        [Tooltip("The game state to start with")]
        [SerializeField] private string defaultGameState = "MainMenu";
        
        [Header("Debug Options")]
        [Tooltip("Whether to log state changes and other events")]
        [SerializeField] private bool enableDebugLogging = true;
        
        // Required components
        private MainGameController mainGameController;
        
        private void OnEnable()
        {
            // Only run in edit mode to help set up the prefab
            if (!Application.isPlaying)
            {
                SetupRequiredComponents();
            }
        }
        
        private void SetupRequiredComponents()
        {
            // Add MainGameController if needed
            mainGameController = GetComponent<MainGameController>();
            if (mainGameController == null)
            {
                mainGameController = gameObject.AddComponent<MainGameController>();
                Debug.Log("Added MainGameController to prefab");
            }
            
            // Configure MainGameController
            // In a real implementation, we would set properties via reflection
            // or SerializedObject for private fields
            
            Debug.Log("MainGameController prefab has been set up. Configure additional settings in the Inspector.");
        }
        
        /// <summary>
        /// Creates a new MainGameController prefab from scratch
        /// </summary>
        [ContextMenu("Create MainGameController Prefab")]
        public void CreatePrefabStructure()
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            
            gameObject.name = "MainGameController";
            
            SetupRequiredComponents();
            
            Debug.Log("MainGameController prefab structure created. Save this as a prefab for use in GameSceneSetup.");
        }
        
        /// <summary>
        /// Inspector helper text displayed at the top
        /// </summary>
        [Header("Setup Instructions")]
        [TextArea(3, 10)]
        [SerializeField] private string setupInstructions = 
            "MAIN GAME CONTROLLER SETUP:\n" +
            "1. Create an empty GameObject\n" +
            "2. Add this script to it\n" +
            "3. Click 'Create MainGameController Prefab' in the context menu\n" +
            "4. Configure the settings above\n" +
            "5. Save as a prefab in your project\n" +
            "6. Assign to GameSceneSetup or TestSceneInitializer";
    }
}

