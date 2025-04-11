using UnityEngine;
using PDXUnderground.Player;

namespace PDXUnderground.Test.Prefabs
{
    /// <summary>
    /// Component used to configure a game object as a player prefab for the PDX Underground game.
    /// This script helps set up all required components and default values.
    /// </summary>
    [ExecuteInEditMode]
    public class PlayerPrefab : MonoBehaviour
    {
        [Header("Character Configuration")]
        [Tooltip("Maximum buzz level for the character")]
        [SerializeField] private float maxBuzz = 100f;
        
        [Tooltip("Threshold at which buzz becomes critical (percentage of max)")]
        [Range(0.1f, 0.5f)]
        [SerializeField] private float criticalBuzzThreshold = 0.3f;
        
        [Tooltip("How quickly buzz decreases over time")]
        [SerializeField] private float buzzDecayRate = 0.5f;
        
        [Header("Movement Configuration")]
        [Tooltip("Character walking speed")]
        [SerializeField] private float walkSpeed = 5f;
        
        [Tooltip("Character running speed")]
        [SerializeField] private float runSpeed = 8f;
        
        [Tooltip("Character jump height")]
        [SerializeField] private float jumpForce = 8f;
        
        [Header("Visualization")]
        [Tooltip("Material to use for the player character")]
        [SerializeField] private Material playerMaterial;
        
        [Tooltip("Whether to use a capsule or custom model")]
        [SerializeField] private bool useCapsuleRepresentation = true;
        
        // Required components
        private CharacterController characterController;
        private GamblerCharacter gamblerCharacter;
        private PlayerController playerController;
        
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
            // Add and configure CharacterController if needed
            characterController = GetComponent<CharacterController>();
            if (characterController == null)
            {
                characterController = gameObject.AddComponent<CharacterController>();
                Debug.Log("Added CharacterController to player prefab");
                
                // Set default values
                characterController.height = 2f;
                characterController.radius = 0.5f;
                characterController.center = new Vector3(0, 1f, 0);
                characterController.slopeLimit = 45f;
                characterController.stepOffset = 0.3f;
            }
            
            // Add and configure PlayerController if needed
            playerController = GetComponent<PlayerController>();
            if (playerController == null)
            {
                playerController = gameObject.AddComponent<PlayerController>();
                Debug.Log("Added PlayerController to player prefab");
            }
            
            // Set PlayerController values
            if (playerController != null)
            {
                // Access via reflection or SerializedObject if needed to set private fields
                // This would be expanded in a real implementation to set properties
                Debug.Log("Configure PlayerController in the Inspector");
            }
            
            // Add and configure GamblerCharacter if needed
            gamblerCharacter = GetComponent<GamblerCharacter>();
            if (gamblerCharacter == null)
            {
                // Note: In a real implementation, this would be replaced with the actual GamblerCharacter component
                // For now, we just log a warning
                Debug.LogWarning("GamblerCharacter component is missing! Please add it manually.");
            }
            
            // Create visual representation if needed
            if (useCapsuleRepresentation && transform.childCount == 0)
            {
                GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                visual.transform.SetParent(transform);
                visual.transform.localPosition = new Vector3(0, 1f, 0);
                visual.transform.localScale = new Vector3(1f, 1f, 1f);
                visual.name = "PlayerVisual";
                
                // Apply material if available
                if (playerMaterial != null)
                {
                    visual.GetComponent<Renderer>().material = playerMaterial;
                }
                
                // Remove collider since we're using CharacterController
                Collider collider = visual.GetComponent<Collider>();
                if (collider != null)
                {
                    DestroyImmediate(collider);
                }
                
                Debug.Log("Created visual representation for player prefab");
            }
            
            // Set the tag for the player
            gameObject.tag = "Player";
        }
        
        /// <summary>
        /// Creates a new player prefab from scratch
        /// </summary>
        [ContextMenu("Create Player Prefab Structure")]
        public void CreatePrefabStructure()
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            
            SetupRequiredComponents();
            
            Debug.Log("Player prefab structure created. Save this as a prefab for use in GameSceneSetup.");
        }
    }
}

