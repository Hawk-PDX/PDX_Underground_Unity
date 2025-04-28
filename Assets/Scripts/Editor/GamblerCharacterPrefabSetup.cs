using UnityEngine;
using UnityEditor;
using PDXUnderground.Player;
using PDXUnderground.Combat;

namespace PDXUnderground.Editor
{
    /// <summary>
    /// Editor utility for setting up a GamblerCharacter prefab with all required components and hierarchy.
    /// </summary>
    public class GamblerCharacterPrefabSetup : EditorWindow
    {
        private GameObject prefabRoot;
        private Material characterMaterial;
        private PhysicMaterial slipperyMaterial;
        
        // Default values
        private float characterHeight = 2f;
        private float characterRadius = 0.5f;
        private float moveSpeed = 5f;
        private float jumpForce = 8f;
        private float maxHealth = 100f;
        private float maxBuzz = 100f;
        private float criticalBuzzThreshold = 30f;
        private float lowBuzzThreshold = 20f;
        private int maxHandSize = 5;
        
        private bool createNewPrefab = true;
        private string prefabName = "GamblerCharacter";
        
        [MenuItem("PDX Underground/Create/Gambler Character Prefab")]
        public static void ShowWindow()
        {
            GetWindow<GamblerCharacterPrefabSetup>("Gambler Character Setup");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("GamblerCharacter Prefab Setup", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.LabelField("Prefab Settings", EditorStyles.boldLabel);
            createNewPrefab = EditorGUILayout.Toggle("Create New Prefab", createNewPrefab);
            
            if (!createNewPrefab)
            {
                prefabRoot = EditorGUILayout.ObjectField("Existing GameObject", prefabRoot, typeof(GameObject), true) as GameObject;
            }
            else
            {
                prefabName = EditorGUILayout.TextField("Prefab Name", prefabName);
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.LabelField("Character Settings", EditorStyles.boldLabel);
            characterHeight = EditorGUILayout.FloatField("Character Height", characterHeight);
            characterRadius = EditorGUILayout.FloatField("Character Radius", characterRadius);
            moveSpeed = EditorGUILayout.FloatField("Move Speed", moveSpeed);
            jumpForce = EditorGUILayout.FloatField("Jump Force", jumpForce);
            maxHealth = EditorGUILayout.FloatField("Max Health", maxHealth);
            maxBuzz = EditorGUILayout.FloatField("Max Buzz", maxBuzz);
            criticalBuzzThreshold = EditorGUILayout.FloatField("Critical Buzz Threshold", criticalBuzzThreshold);
            lowBuzzThreshold = EditorGUILayout.FloatField("Low Buzz Threshold", lowBuzzThreshold);
            maxHandSize = EditorGUILayout.IntField("Max Hand Size", maxHandSize);
            characterMaterial = EditorGUILayout.ObjectField("Character Material", characterMaterial, typeof(Material), false) as Material;
            slipperyMaterial = EditorGUILayout.ObjectField("Physics Material", slipperyMaterial, typeof(PhysicMaterial), false) as PhysicMaterial;
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Create GamblerCharacter Prefab"))
            {
                CreateGamblerCharacterPrefab();
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "This tool creates a complete GamblerCharacter prefab with all required components:\n" +
                "- Character Controller\n" +
                "- GamblerCharacter\n" +
                "- PlayerController\n" +
                "- Combat Abilities (Flick & Slash)\n" +
                "- Proper hierarchy and configuration\n\n" +
                "The prefab includes a visual placeholder and all necessary child objects.", 
                MessageType.Info);
        }
        
        private void CreateGamblerCharacterPrefab()
        {
            // Create or use existing GameObject
            GameObject root;
            if (createNewPrefab || prefabRoot == null)
            {
                root = new GameObject(prefabName);
                prefabRoot = root;
            }
            else
            {
                root = prefabRoot;
            }
            
            // Setup components on the root GameObject
            SetupRootComponents(root);
            
            // Create child GameObjects
            CreateChildGameObjects(root);
            
            // Configure layers
            SetupLayers(root);
            
            // Create a prefab
            if (createNewPrefab)
            {
                SaveAsPrefab(root);
            }
            else
            {
                Debug.Log($"GamblerCharacter setup completed on {root.name}");
            }
            
            // Select the new object
            Selection.activeGameObject = root;
        }
        
        private void SetupRootComponents(GameObject root)
        {
            // Add CharacterController
            CharacterController characterController = root.GetComponent<CharacterController>();
            if (characterController == null)
            {
                characterController = root.AddComponent<CharacterController>();
            }
            
            // Configure CharacterController
            characterController.height = characterHeight;
            characterController.radius = characterRadius;
            characterController.center = new Vector3(0, characterHeight / 2, 0);
            characterController.slopeLimit = 45f;
            characterController.stepOffset = 0.3f;
            
            // Add PlayerController
            PlayerController playerController = root.GetComponent<PlayerController>();
            if (playerController == null)
            {
                playerController = root.AddComponent<PlayerController>();
            }
            
            // Set PlayerController values through SerializedObject since some fields are private
            SerializedObject serializedPlayerController = new SerializedObject(playerController);
            serializedPlayerController.FindProperty("baseMovementSpeed").floatValue = moveSpeed;
            serializedPlayerController.FindProperty("jumpForce").floatValue = jumpForce;
            serializedPlayerController.ApplyModifiedProperties();
            
            // Add GamblerCharacter
            GamblerCharacter gamblerCharacter = root.GetComponent<GamblerCharacter>();
            if (gamblerCharacter == null)
            {
                gamblerCharacter = root.AddComponent<GamblerCharacter>();
            }
            
            // Configure GamblerCharacter
            SetupGamblerCharacterValues(gamblerCharacter);
            
            // Add combat abilities
            SetupCombatAbilities(root);
            
            // Add Animator component
            Animator animator = root.GetComponent<Animator>();
            if (animator == null)
            {
                animator = root.AddComponent<Animator>();
            }
            
            // Add Rigidbody for physics interactions if needed
            Rigidbody rb = root.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = root.AddComponent<Rigidbody>();
                rb.isKinematic = true; // Let CharacterController handle movement
                rb.useGravity = false;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }
            
            // Add Audio Source
            AudioSource audioSource = root.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = root.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1.0f; // Full 3D sound
            }
        }
        
        private void SetupGamblerCharacterValues(GamblerCharacter gamblerCharacter)
        {
            // Use SerializedObject to access serialized properties
            SerializedObject serializedCharacter = new SerializedObject(gamblerCharacter);
            
            // Health settings
            serializedCharacter.FindProperty("maxHealth").floatValue = maxHealth;
            
            // Buzz system settings
            serializedCharacter.FindProperty("maxBuzz").floatValue = maxBuzz;
            serializedCharacter.FindProperty("criticalBuzzThreshold").floatValue = criticalBuzzThreshold;
            serializedCharacter.FindProperty("lowBuzzThreshold").floatValue = lowBuzzThreshold;
            
            // Card system settings
            serializedCharacter.FindProperty("maxHandSize").intValue = maxHandSize;
            serializedCharacter.FindProperty("initialHandSize").intValue = 3;
            serializedCharacter.FindProperty("autoAddAbilities").boolValue = true;
            
            // Apply modified properties
            serializedCharacter.ApplyModifiedProperties();
        }
        
        private void SetupCombatAbilities(GameObject root)
        {
            // Add MeleeSlashAbility
            MeleeSlashAbility slashAbility = root.GetComponent<MeleeSlashAbility>();
            if (slashAbility == null)
            {
                slashAbility = root.AddComponent<MeleeSlashAbility>();
            }
            
            // Configure SlashAbility
            SerializedObject serializedSlashAbility = new SerializedObject(slashAbility);
            serializedSlashAbility.FindProperty("abilityName").stringValue = "Slash";
            serializedSlashAbility.FindProperty("cooldownTime").floatValue = 1.5f;
            serializedSlashAbility.FindProperty("baseDamage").floatValue = 25f;
            serializedSlashAbility.FindProperty("buzzCost").floatValue = 10f;
            serializedSlashAbility.FindProperty("animationTriggerName").stringValue = "Slash";
            serializedSlashAbility.ApplyModifiedProperties();
            
            // Add RangedFlickAbility
            RangedFlickAbility flickAbility = root.GetComponent<RangedFlickAbility>();
            if (flickAbility == null)
            {
                flickAbility = root.AddComponent<RangedFlickAbility>();
            }
            
            // Configure FlickAbility
            SerializedObject serializedFlickAbility = new SerializedObject(flickAbility);
            serializedFlickAbility.FindProperty("abilityName").stringValue = "Flick";
            serializedFlickAbility.FindProperty("cooldownTime").floatValue = 2f;
            serializedFlickAbility.FindProperty("baseDamage").floatValue = 15f;
            serializedFlickAbility.FindProperty("buzzCost").floatValue = 15f;
            serializedFlickAbility.FindProperty("animationTriggerName").stringValue = "Flick";
            serializedFlickAbility.ApplyModifiedProperties();
        }
        
        private void CreateChildGameObjects(GameObject root)
        {
            // Create visual model (capsule)
            GameObject modelObject = CreateChildIfMissing(root, "Model");
            
            // If the model doesn't have a renderer, create a capsule
            Renderer modelRenderer = modelObject.GetComponentInChildren<Renderer>();
            if (modelRenderer == null)
            {
                GameObject capsuleVisual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                capsuleVisual.name = "CapsuleVisual";
                capsuleVisual.transform.SetParent(modelObject.transform);
                capsuleVisual.transform.localPosition = new Vector3(0, characterHeight / 2, 0);
                capsuleVisual.transform.localScale = new Vector3(characterRadius * 2, characterHeight / 2, characterRadius * 2);
                
                // Remove collider since we're using CharacterController
                DestroyImmediate(capsuleVisual.GetComponent<Collider>());
                
                // Assign material if specified
                Renderer renderer = capsuleVisual.GetComponent<Renderer>();
                if (renderer != null && characterMaterial != null)
                {
                    renderer.sharedMaterial = characterMaterial;
                }
            }
            
            // Create ground check object
            GameObject groundCheck = CreateChildIfMissing(modelObject, "GroundCheck");
            groundCheck.transform.localPosition = new Vector3(0, -characterHeight / 2, 0);
            
            // Create camera target
            GameObject cameraTarget = CreateChildIfMissing(root, "CameraTarget");
            cameraTarget.transform.localPosition = new Vector3(0, characterHeight * 0.8f, 0);
            
            // Create card spawn point
            GameObject cardSpawnPoint = CreateChildIfMissing(root, "CardSpawnPoint");
            cardSpawnPoint.transform.localPosition = new Vector3(0, characterHeight * 0.6f, characterRadius + 0.1f);
            
            // Connect the groundCheck and cardSpawnPoint to their respective components
            // (This would be done by using serializedObject.FindProperty if these were exposed as serialized fields)
        }
        
        private GameObject CreateChildIfMissing(GameObject parent, string childName)
        {
            Transform existingChild = parent.transform.Find(childName);
            if (existingChild != null)
            {
                return existingChild.gameObject;
            }
            
            GameObject child = new GameObject(childName);
            child.transform.SetParent(parent.transform);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;
            
            return child;
        }
        
        private void SetupLayers(GameObject root)
        {
            // Get the Player layer
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer == -1)
            {
                Debug.LogWarning("Player layer not found. Character will use default layer.");
                return;
            }
            
            // Set the player object to the Player layer
            root.layer = playerLayer;
            
            // Set all child objects to the Player layer
            foreach (Transform child in root.GetComponentsInChildren<Transform>())
            {
                child.gameObject.layer = playerLayer;
            }
            
            // Set the player tag
            root.tag = "Player";
        }
        
        private void SaveAsPrefab(GameObject root)
        {
            // Create Prefabs directory if it doesn't exist
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }
            
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Characters"))
            {
                AssetDatabase.CreateFolder("Assets/Prefabs", "Characters");
            }
            
            string prefabPath = $"Assets/Prefabs/Characters/{prefabName}.prefab";
            
            // Check if the prefab already exists
            bool prefabExists = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null;
            
            // Create the prefab
            GameObject prefabAsset;
            if (prefabExists)
            {
                prefabAsset = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                Debug.Log($"Updated existing prefab at {prefabPath}");
            }
            else
            {
                prefabAsset = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                prefabAsset = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                Debug.Log($"Created new prefab at {prefabPath}");
            }
            
            // Cleanup the temporary object if we created a new one
            if (createNewPrefab)
            {
                DestroyImmediate(root);
            }
            
            // Select the created prefab in the Project window
            Selection.activeObject = prefabAsset;
        }
    }
}
