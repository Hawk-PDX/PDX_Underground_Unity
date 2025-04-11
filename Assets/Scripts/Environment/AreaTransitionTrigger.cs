using UnityEngine;
using System.Collections;

/// <summary>
/// AreaTransitionTrigger handles transitioning between different areas in PDX Underground,
/// such as from the Portland streets to the Shanghai tunnels or the port.
/// </summary>
public class AreaTransitionTrigger : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Transition Settings")]
    [SerializeField] private string destinationAreaName = "Portland Streets";
    [SerializeField] private float fadeOutDuration = 1.0f;
    [SerializeField] private float fadeInDuration = 1.0f;
    [SerializeField] private bool useLoadingScreen = true;
    
    [Header("Interaction Settings")]
    [SerializeField] private bool requirePlayerInteraction = true;
    [SerializeField] private string promptText = "Press E to enter";
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    
    [Header("Position Settings")]
    [SerializeField] private Vector3 destinationPosition = Vector3.zero;
    [SerializeField] private bool useSpawnPoint = false;
    [SerializeField] private string spawnPointName = "DefaultSpawn";
    
    [Header("Debug Settings")]
    [SerializeField] private bool showDebugVisuals = true;
    [SerializeField] private Color debugColor = new Color(1.0f, 0.5f, 0.0f, 0.5f);
    [SerializeField] private Vector3 debugSize = new Vector3(4f, 3f, 4f);
    
    #endregion
    
    #region Private Variables
    
    private BoxCollider triggerCollider;
    private bool playerInTrigger = false;
    private GameObject playerObject;
    private UIManager uiManager;
    private SceneManager sceneManager;
    private MeshRenderer debugVisualRenderer;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        // Get or create a box collider
        triggerCollider = GetComponent<BoxCollider>();
        if (triggerCollider == null)
        {
            triggerCollider = gameObject.AddComponent<BoxCollider>();
        }
        
        // Configure the collider
        triggerCollider.isTrigger = true;
        triggerCollider.size = debugSize;
        triggerCollider.center = new Vector3(0, debugSize.y / 2, 0);
        
        // Get references to managers
        uiManager = FindObjectOfType<UIManager>();
        sceneManager = FindObjectOfType<SceneManager>();
        
        // Create debug visuals if needed
        if (showDebugVisuals)
        {
            CreateDebugVisuals();
        }
    }
    
    private void Start()
    {
        // Ensure we have a SceneManager
        if (sceneManager == null)
        {
            Debug.LogWarning("No SceneManager found. Area transitions will not work correctly.");
        }
    }
    
    private void Update()
    {
        // Check for player interaction if required
        if (requirePlayerInteraction && playerInTrigger && playerObject != null)
        {
            if (Input.GetKeyDown(interactionKey))
            {
                InitiateTransition();
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if it's the player
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
            playerObject = other.gameObject;
            
            // Show interaction prompt if required
            if (requirePlayerInteraction && uiManager != null)
            {
                uiManager.ShowInteractionPrompt(promptText, transform.position + Vector3.up * 2);
            }
            // Otherwise transition immediately
            else if (!requirePlayerInteraction)
            {
                InitiateTransition();
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        // Check if it's the player
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
            playerObject = null;
            
            // Hide interaction prompt
            if (requirePlayerInteraction && uiManager != null)
            {
                uiManager.HideInteractionPrompt();
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        // Draw debug visuals in editor
        if (showDebugVisuals)
        {
            Gizmos.color = debugColor;
            
            // Draw a cube to represent trigger area
            Vector3 position = transform.position + new Vector3(0, debugSize.y / 2, 0);
            Gizmos.DrawCube(position, debugSize);
            
            // Draw a line to show transition direction
            Gizmos.DrawLine(position, position + transform.forward * 5f);
            
            // Draw label
            Gizmos.color = Color.white;
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(position + Vector3.up * 2, "To: " + destinationAreaName);
            #endif
        }
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Initiates the area transition process
    /// </summary>
    public void InitiateTransition()
    {
        if (sceneManager == null)
        {
            Debug.LogError("Cannot transition: No SceneManager found!");
            return;
        }
        
        // Hide interaction prompt if visible
        if (uiManager != null)
        {
            uiManager.HideInteractionPrompt();
        }
        
        // Determine which transition method to use based on destination
        switch (destinationAreaName.ToLower())
        {
            case "portland streets":
            case "streets":
                sceneManager.TransitionToStreets(destinationPosition);
                break;
                
            case "shanghai tunnels":
            case "tunnels":
                sceneManager.TransitionToTunnels(destinationPosition);
                break;
                
            case "port of portland":
            case "port":
                sceneManager.TransitionToPort(destinationPosition);
                break;
                
            default:
                // Generic transition using scene name
                sceneManager.LoadScene(destinationAreaName);
                break;
        }
    }
    
    /// <summary>
    /// Sets the destination area name
    /// </summary>
    public void SetDestinationArea(string areaName)
    {
        destinationAreaName = areaName;
    }
    
    /// <summary>
    /// Sets the destination position
    /// </summary>
    public void SetDestinationPosition(Vector3 position)
    {
        destinationPosition = position;
        useSpawnPoint = false;
    }
    
    /// <summary>
    /// Toggles debug visuals
    /// </summary>
    public void SetDebugVisualsVisible(bool visible)
    {
        showDebugVisuals = visible;
        
        if (debugVisualRenderer != null)
        {
            debugVisualRenderer.enabled = visible;
        }
        else if (visible)
        {
            CreateDebugVisuals();
        }
    }
    
    #endregion
    
    #region Private Methods
    
    /// <summary>
    /// Creates visual representation for debugging
    /// </summary>
    private void CreateDebugVisuals()
    {
        // Create a visual cube for the trigger area
        GameObject visualObj = new GameObject("DebugVisual");
        visualObj.transform.SetParent(transform);
        visualObj.transform.localPosition = new Vector3(0, debugSize.y / 2, 0);
        
        // Add mesh components
        MeshFilter meshFilter = visualObj.AddComponent<MeshFilter>();
        debugVisualRenderer = visualObj.AddComponent<MeshRenderer>();
        
        // Create cube mesh
        meshFilter.mesh = CreateCubeMesh();
        
        // Create and configure material
        Material debugMaterial = new Material(Shader.Find("Standard"));
        debugMaterial.color = debugColor;
        debugMaterial.SetFloat("_Mode", 3); // Transparent mode
        debugMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        debugMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        debugMaterial.SetInt("_ZWrite", 0);
        debugMaterial.DisableKeyword("_ALPHATEST_ON");
        debugMaterial.EnableKeyword("_ALPHABLEND_ON");
        debugMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        debugMaterial.renderQueue = 3000;
        
        // Assign material
        debugVisualRenderer.material = debugMaterial;
        
        // Make sure it doesn't interfere with gameplay
        visualObj.layer = LayerMask.NameToLayer("Ignore Raycast");
    }
    
    /// <summary>
    /// Creates a simple cube mesh for debug visualization
    /// </summary>
    private Mesh CreateCubeMesh()
    {
        Mesh mesh = new Mesh();
        
        // Scale vertices to match debug size
        Vector3 size = debugSize * 0.5f;
        
        // Define the 8 corners of a cube
        Vector3[] vertices = new Vector3[8]
        {
            new Vector3(-size.x, -size.y, -size.z),
            new Vector3(size.x, -size.y, -size.z),
            new Vector3(size.x, -size.y, size.z),
            new Vector3(-size.x, -size.y, size.z),
            new Vector3(-size.x, size.y, -size.z),
            new Vector3(size.x, size.y, -size.z),
            new Vector3(size.x, size.y, size.z),
            new Vector3(-size.x, size.y, size.z)
        };
        
        // Define the 12 triangles (2 per face, 6 faces)
        int[] triangles = new int[36]
        {
            // Bottom face
            0, 3, 1, 1, 3, 2,
            // Top face
            4, 5, 7, 7, 5, 6,
            // Front face
            3, 7, 2, 2, 7, 6,
            // Back face
            0, 1, 4, 4, 1, 5,
            // Left face
            0, 4, 3, 3, 4, 7,
            // Right face
            1, 2, 5, 5, 2, 6
        };
        
        // Set mesh data
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    #endregion
}

using UnityEngine;
using System.Collections;

/// <summary>
/// AreaTransitionTrigger handles transitions between different areas in PDX Underground,
/// such as streets, tunnels, and the port. Provides visual feedback and player interactions.
/// </summary>
public class AreaTransitionTrigger : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Transition Settings")]
    [SerializeField] private string destinationAreaName = "Shanghai Tunnels";
    [SerializeField] private Vector3 destinationPosition = Vector3.zero;
    [SerializeField] private float fadeOutDuration = 1.0f;
    [SerializeField] private float fadeInDuration = 1.0f;
    [SerializeField] private bool useLoadingScreen = true;
    
    [Header("Debug Visualization")]
    [SerializeField] private bool showDebugVisuals = true;
    [SerializeField] private Color debugColor = new Color(1.0f, 0.5f, 0.0f, 0.5f);
    [SerializeField] private float visualScale = 1.0f;
    [SerializeField] private Material debugMaterial;
    
    [Header("Interaction Settings")]
    [SerializeField] private bool requirePlayerInteraction = true;
    [SerializeField] private float interactionDistance = 2.0f;
    [SerializeField] private string promptText = "Press E to enter";
    [SerializeField] private AudioClip transitionSound;
    
    #endregion
    
    #region Private Variables
    
    private GameObject debugVisual;
    private SceneManager sceneManager;
    private UIManager uiManager;
    private bool playerInRange = false;
    private GameObject currentPlayer;
    private Transform debugTransform;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        // Create debug visualization if enabled
        if (showDebugVisuals)
        {
            CreateDebugVisual();
        }
    }
    
    private void Start()
    {
        // Find managers
        sceneManager = FindObjectOfType<SceneManager>();
        uiManager = FindObjectOfType<UIManager>();
        
        // If no collider, add a box collider as trigger
        if (GetComponent<Collider>() == null)
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector3(4f, 3f, 4f);
            boxCollider.center = new Vector3(0f, 1.5f, 0f);
        }
        else
        {
            // Ensure existing collider is a trigger
            Collider collider = GetComponent<Collider>();
            collider.isTrigger = true;
        }
    }
    
    private void Update()
    {
        // Handle interaction if player is in range and interaction is required
        if (playerInRange && requirePlayerInteraction && currentPlayer != null)
        {
            // Show interaction prompt
            if (uiManager != null)
            {
                Vector3 promptPosition = transform.position + Vector3.up * 2.0f;
                uiManager.ShowInteractionPrompt(promptText, promptPosition);
            }
            
            // Check for input
            if (Input.GetKeyDown(KeyCode.E))
            {
                InitiateTransition();
            }
        }
        
        // Update debug visual if needed
        if (debugVisual != null && debugTransform != null)
        {
            // Rotate the debug visual for visibility
            debugTransform.Rotate(0, 30 * Time.deltaTime, 0);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if this is the player
        if (other.CompareTag("Player"))
        {
            currentPlayer = other.gameObject;
            playerInRange = true;
            
            // If we don't require interaction, transition immediately
            if (!requirePlayerInteraction)
            {
                InitiateTransition();
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        // Check if this is the player leaving
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            currentPlayer = null;
            
            // Hide interaction prompt
            if (uiManager != null)
            {
                uiManager.HideInteractionPrompt();
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!showDebugVisuals)
            return;
            
        // Draw transition volume
        Gizmos.color = debugColor;
        
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            // Use the collider's bounds
            Gizmos.matrix = transform.localToWorldMatrix;
            
            if (collider is BoxCollider)
            {
                BoxCollider boxCollider = (BoxCollider)collider;
                Gizmos.DrawCube(boxCollider.center, boxCollider.size);
            }
            else if (collider is SphereCollider)
            {
                SphereCollider sphereCollider = (SphereCollider)collider;
                Gizmos.DrawSphere(sphereCollider.center, sphereCollider.radius);
            }
            else
            {
                // Default to a simple cube at the transform position
                Gizmos.DrawCube(Vector3.zero, Vector3.one * 2);
            }
        }
        else
        {
            // No collider, draw a default volume
            Gizmos.DrawCube(transform.position + Vector3.up * 1.5f, new Vector3(4f, 3f, 4f));
        }
        
        // Draw line to destination
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 5);
        
        // Draw text for area name
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 5, destinationAreaName);
        #endif
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Initiates the area transition
    /// </summary>
    public void InitiateTransition()
    {
        if (sceneManager == null)
        {
            sceneManager = FindObjectOfType<SceneManager>();
            if (sceneManager == null)
            {
                Debug.LogError("AreaTransitionTrigger: No SceneManager found in scene!");
                return;
            }
        }
        
        // Play transition sound if available
        if (transitionSound != null)
        {
            AudioSource.PlayClipAtPoint(transitionSound, transform.position);
        }
        
        // Determine which transition method to use based on area name
        switch (destinationAreaName.ToLower())
        {
            case "shanghai tunnels":
            case "tunnels":
                sceneManager.TransitionToTunnels(destinationPosition);
                break;
                
            case "port of portland":
            case "port":
                sceneManager.TransitionToPort(destinationPosition);
                break;
                
            case "portland streets":
            case "streets":
                sceneManager.TransitionToStreets(destinationPosition);
                break;
                
            default:
                // Generic scene load
                sceneManager.LoadScene(destinationAreaName);
                break;
        }
        
        // Hide the interaction prompt
        if (uiManager != null)
        {
            uiManager.HideInteractionPrompt();
        }
    }
    
    /// <summary>
    /// Sets the destination for this transition trigger
    /// </summary>
    public void SetDestination(string areaName, Vector3 position)
    {
        destinationAreaName = areaName;
        destinationPosition = position;
    }
    
    /// <summary>
    /// Toggles the debug visualization
    /// </summary>
    public void ToggleDebugVisual(bool visible)
    {
        showDebugVisuals = visible;
        
        if (debugVisual != null)
        {
            debugVisual.SetActive(visible);
        }
        else if (visible)
        {
            CreateDebugVisual();
        }
    }
    
    #endregion
    
    #region Private Methods
    
    /// <summary>
    /// Creates a debug visualization to make the trigger area visible
    /// </summary>
    private void CreateDebugVisual()
    {
        // Clean up any existing visual
        if (debugVisual != null)
        {
            Destroy(debugVisual);
        }
        
        // Create a new visual GameObject
        debugVisual = new GameObject("DebugVisual");
        debugVisual.transform.SetParent(transform);
        debugVisual.transform.localPosition = Vector3.up * 1.5f;
        debugTransform = debugVisual.transform;
        
        // Add a mesh filter and renderer
        MeshFilter meshFilter = debugVisual.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = debugVisual.AddComponent<MeshRenderer>();
        
        // Create primitive based on transition area
        GameObject visualPrimitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Mesh primitiveMesh = visualPrimitive.GetComponent<MeshFilter>().sharedMesh;
        meshFilter.sharedMesh = primitiveMesh;
        Destroy(visualPrimitive);
        
        // Set material
        if (debugMaterial == null)
        {
            // Create a simple colored material
            Material material = new Material(Shader.Find("Standard"));
            material.color = debugColor;
            material.SetFloat("_Mode", 3); // Transparent mode
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
            
            meshRenderer.material = material;
        }
        else
        {
            meshRenderer.material = debugMaterial;
        }
        
        // Scale the visual appropriately
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            if (collider is BoxCollider)
            {
                BoxCollider boxCollider = (BoxCollider)collider;
                debugVisual.transform.localPosition = boxCollider.center;
                debugVisual.transform.localScale = boxCollider.size * visualScale;
            }
            else if (collider is SphereCollider)
            {
                SphereCollider sphereCollider = (SphereCollider)collider;
                debugVisual.transform.localPosition = sphereCollider.center;
                debugVisual.transform.localScale = Vector3.one * sphereCollider.radius * 2 * visualScale;
                
                // Switch to sphere primitive
                GameObject spherePrimitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                meshFilter.sharedMesh = spherePrimitive.GetComponent<MeshFilter>().sharedMesh;
                Destroy(spherePrimitive);
            }
        }
        else
        {
            // Default size if no collider
            debugVisual.transform.localScale = new Vector3(4f, 3f, 4f) * visualScale;
        }
    }
    
    #endregion
}

using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// AreaTransitionTrigger handles transitions between different areas in the PDX Underground game.
/// Placed at key locations to enable moving between streets, tunnels, and the port.
/// </summary>
public class AreaTransitionTrigger : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Transition Settings")]
    [SerializeField] private string destinationAreaName = "Portland Streets";
    [SerializeField] private Vector3 destinationPosition = Vector3.zero;
    [SerializeField] private float fadeOutDuration = 1.0f;
    [SerializeField] private float fadeInDuration = 1.0f;
    [SerializeField] private bool useLoadingScreen = false;
    [SerializeField] private string transitionPrompt = "Press E to enter";
    
    [Header("Transition Types")]
    [SerializeField] private TransitionType transitionType = TransitionType.FadeTransition;
    [SerializeField] private SceneTransitionReference sceneReference;
    
    [Header("Visual Settings")]
    [SerializeField] private bool showDebugVisuals = true;
    [SerializeField] private Color debugColor = new Color(1, 0.5f, 0, 0.5f);
    [SerializeField] private GameObject transitionEffectPrefab;
    
    [Header("Events")]
    [SerializeField] private UnityEvent OnTransitionStart;
    [SerializeField] private UnityEvent OnTransitionComplete;
    
    #endregion
    
    #region Private Variables
    
    private bool playerInRange = false;
    private GameObject transitionEffect;
    private UIManager uiManager;
    private SceneManager sceneManager;
    private bool isTransitioning = false;
    
    public enum TransitionType
    {
        FadeTransition,
        SceneChange,
        PositionChange
    }
    
    [System.Serializable]
    public struct SceneTransitionReference
    {
        public string sceneName;
        public int sceneIndex;
        public string spawnPointName;
    }
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        // Find necessary managers
        uiManager = FindObjectOfType<UIManager>();
        sceneManager = FindObjectOfType<SceneManager>();
        
        // Create debug visuals if needed
        SetupDebugVisuals();
    }
    
    private void Start()
    {
        // Set up trigger collider if doesn't exist
        if (GetComponent<Collider>() == null)
        {
            BoxCollider trigger = gameObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(3, 2, 3);
            trigger.center = new Vector3(0, 1, 0);
        }
        
        // Create transition effect if specified
        if (transitionEffectPrefab != null)
        {
            transitionEffect = Instantiate(transitionEffectPrefab, transform.position, Quaternion.identity);
            transitionEffect.transform.SetParent(transform);
        }
    }
    
    private void Update()
    {
        // Show interaction prompt when player is in range
        if (playerInRange && !isTransitioning)
        {
            // Check for interaction input
            if (Input.GetKeyDown(KeyCode.E))
            {
                InitiateTransition();
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if player entered the trigger
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            
            // Show interaction prompt
            if (uiManager != null)
            {
                uiManager.ShowInteractionPrompt(transitionPrompt, transform.position + Vector3.up * 2);
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        // Check if player left the trigger
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            
            // Hide interaction prompt
            if (uiManager != null)
            {
                uiManager.HideInteractionPrompt();
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        if (showDebugVisuals)
        {
            // Draw a semitransparent cube to visualize the transition area
            Gizmos.color = debugColor;
            
            // Use collider size if available, otherwise use default size
            Collider col = GetComponent<Collider>();
            Vector3 size = col != null ? col.bounds.size : new Vector3(3, 2, 3);
            
            Gizmos.DrawCube(transform.position + new Vector3(0, size.y / 2, 0), size);
            
            // Draw arrow pointing to destination
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 3);
            Gizmos.DrawLine(transform.position + transform.forward * 3, 
                transform.position + transform.forward * 2.5f + transform.right * 0.5f);
            Gizmos.DrawLine(transform.position + transform.forward * 3, 
                transform.position + transform.forward * 2.5f - transform.right * 0.5f);
            
            // Draw area name
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2, destinationAreaName);
            #endif
        }
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Initiates the transition to the destination area
    /// </summary>
    public void InitiateTransition()
    {
        if (isTransitioning)
            return;
            
        isTransitioning = true;
        
        // Trigger start event
        OnTransitionStart.Invoke();
        
        // Start transition based on type
        switch (transitionType)
        {
            case TransitionType.FadeTransition:
                StartFadeTransition();
                break;
            case TransitionType.SceneChange:
                StartSceneTransition();
                break;
            case TransitionType.PositionChange:
                StartPositionTransition();
                break;
        }
    }
    
    /// <summary>
    /// Sets the destination for this transition
    /// </summary>
    public void SetDestination(string areaName, Vector3 position)
    {
        destinationAreaName = areaName;
        destinationPosition = position;
    }
    
    /// <summary>
    /// Sets whether debug visuals should be shown
    /// </summary>
    public void SetDebugVisuals(bool show)
    {
        showDebugVisuals = show;
        
        // Update debug visuals
        SetupDebugVisuals();
    }
    
    #endregion
    
    #region Private Methods
    
    /// <summary>
    /// Sets up visual debug elements
    /// </summary>
    private void SetupDebugVisuals()
    {
        // In development mode, add a visual indicator for the transition
        if (showDebugVisuals && Application.isEditor)
        {
            // Add a debug visual if not already present
            if (transform.Find("DebugVisual") == null)
            {
                GameObject debugVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                debugVisual.name = "DebugVisual";
                debugVisual.transform.SetParent(transform);
                debugVisual.transform.localPosition = Vector3.zero;
                debugVisual.transform.localScale = new Vector3(3, 2, 3);
                
                // Make it semitransparent
                Renderer renderer = debugVisual.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = debugColor;
                    renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    renderer.material.EnableKeyword("_ALPHABLEND_ON");
                    renderer.material.renderQueue = 3000;
                }
                
                // Remove collider from visual
                Collider collider = debugVisual.GetComponent<Collider>();
                if (collider != null)
                {
                    Destroy(collider);
                }
            }
        }
        else
        {
            // Remove debug visual if it exists
            Transform debugVisual = transform.Find("DebugVisual");
            if (debugVisual != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(debugVisual.gameObject);
                }
                else
                {
                    DestroyImmediate(debugVisual.gameObject);
                }
            }
        }
    }
    
    /// <summary>
    /// Starts a fade transition to another position in the same scene
    /// </summary>
    private void StartFadeTransition()
    {
        // Check if we have required managers
        if (uiManager == null)
        {
            Debug.LogError("UIManager not found for transition!");
            isTransitioning = false;
            return;
        }
        
        // Start transition coroutine on UIManager
        StartCoroutine(FadeTransitionSequence());
    }
    
    /// <summary>
    /// Performs a fade transition sequence
    /// </summary>
    private System.Collections.IEnumerator FadeTransitionSequence()
    {
        // Show area name
        if (uiManager != null)
        {
            uiManager.ShowAreaName(destinationAreaName);
        }
        
        // Fade to black
        if (uiManager != null)
        {
            yield return uiManager.FadeToBlack();
        }
        else
        {
            yield return new WaitForSeconds(fadeOutDuration);
        }
        
        // Teleport player to destination
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = destinationPosition;
        }
        
        // Small delay at black screen
        yield return new WaitForSeconds(0.5f);
        
        // Fade back in
        if (uiManager != null)
        {
            yield return uiManager.FadeFromBlack();
        }
        else
        {
            yield return new WaitForSeconds(fadeInDuration);
        }
        
        // Invoke completion event
        OnTransitionComplete.Invoke();
        
        // Reset transition state
        isTransitioning = false;
    }
    
    /// <summary>
    /// Starts a transition to another scene
    /// </summary>
    private void StartSceneTransition()
    {
        // Check if we have SceneManager
        if (sceneManager == null)
        {
            Debug.LogError("SceneManager not found for scene transition!");
            isTransitioning = false;
            return;
        }
        
        // Use loading screen if specified
        if (useLoadingScreen && uiManager != null)
        {
            uiManager.ShowLoadingScreen();
        }
        
        // Start scene transition
        sceneManager.LoadScene(sceneReference.sceneName);
        
        // Note: The SceneManager will handle fade transitions and completion events
    }
    
    /// <summary>
    /// Performs an instant position change within the same scene
    /// </summary>
    private void StartPositionTransition()
    {
        // Simply move the player to the destination position
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = destinationPosition;
        }
        
        // Show area name
        if (uiManager != null)
        {
            uiManager.ShowAreaName(destinationAreaName);
        }
        
        // Invoke completion event
        OnTransitionComplete.Invoke();
        
        // Reset transition state
        isTransitioning = false;
    }
    
    #endregion
}

using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// AreaTransitionTrigger handles player transitions between different areas of Portland,
/// such as moving from streets to Shanghai tunnels or the port area.
/// </summary>
public class AreaTransitionTrigger : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Transition Settings")]
    [SerializeField] private string destinationAreaName;
    [SerializeField] private Vector3 destinationPosition;
    [SerializeField] private bool useCustomDestination = false;
    [SerializeField] private bool requiresPlayerInteraction = false;
    [SerializeField] private string interactionPrompt = "Press E to enter";
    [SerializeField] private float transitionDelay = 0.5f;
    
    [Header("Effects")]
    [SerializeField] private bool fadeOnTransition = true;
    [SerializeField] private AudioClip transitionSound;
    [SerializeField] private GameObject transitionVFX;
    
    [Header("Events")]
    [SerializeField] private UnityEvent onPlayerEnter;
    [SerializeField] private UnityEvent onTransitionStart;
    [SerializeField] private UnityEvent onTransitionComplete;
    
    #endregion
    
    #region Private Variables
    
    private bool playerInTrigger = false;
    private GameObject currentPlayer;
    private SceneManager sceneManager;
    private GameEnvironmentController environmentController;
    private bool transitioning = false;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Start()
    {
        // Get reference to scene manager
        sceneManager = FindObjectOfType<SceneManager>();
        if (sceneManager == null)
        {
            Debug.LogError("AreaTransitionTrigger requires a SceneManager in the scene!");
        }
        
        // Get reference to environment controller
        environmentController = FindObjectOfType<GameEnvironmentController>();
        
        // Disable transition VFX if it exists
        if (transitionVFX != null)
        {
            transitionVFX.SetActive(false);
        }
    }
    
    private void Update()
    {
        // Check if player is in trigger and wants to interact
        if (playerInTrigger && requiresPlayerInteraction && !transitioning)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                InitiateTransition();
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering is the player
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
            currentPlayer = other.gameObject;
            
            // Show interaction prompt if needed
            if (requiresPlayerInteraction)
            {
                ShowInteractionPrompt();
            }
            else
            {
                // Auto-transition if no interaction required
                InitiateTransition();
            }
            
            // Invoke enter event
            onPlayerEnter?.Invoke();
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        // Check if the object leaving is the player
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
            currentPlayer = null;
            
            // Hide interaction prompt
            if (requiresPlayerInteraction)
            {
                HideInteractionPrompt();
            }
        }
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Manually triggers the area transition
    /// </summary>
    public void InitiateTransition()
    {
        if (transitioning) return;
        
        transitioning = true;
        
        // Play transition effects
        if (transitionVFX != null)
        {
            transitionVFX.SetActive(true);
        }
        
        if (transitionSound != null)
        {
            AudioSource.PlayClipAtPoint(transitionSound, transform.position);
        }
        
        // Invoke transition start event
        onTransitionStart?.Invoke();
        
        // Start transition with delay
        Invoke("PerformTransition", transitionDelay);
    }
    
    /// <summary>
    /// Updates the destination area and position
    /// </summary>
    public void SetDestination(string areaName, Vector3 position)
    {
        destinationAreaName = areaName;
        destinationPosition = position;
    }
    
    #endregion
    
    #region Private Methods
    
    /// <summary>
    /// Performs the actual area transition
    /// </summary>
    private void PerformTransition()
    {
        if (sceneManager == null) return;
        
        // Determine which transition method to use based on the destination
        switch (destinationAreaName.ToLower())
        {
            case "tunnels":
            case "shanghaitunnels":
                sceneManager.TransitionToTunnels(destinationPosition);
                break;
                
            case "port":
            case "portofportland":
                sceneManager.TransitionToPort(destinationPosition);
                break;
                
            case "streets":
            case "portlandstreets":
                sceneManager.TransitionToStreets(destinationPosition);
                break;
                
            default:
                Debug.LogWarning("Unknown destination area: " + destinationAreaName);
                break;
        }
        
        // Update environment if needed
        if (environmentController != null)
        {
            if (destinationAreaName.ToLower().Contains("tunnel"))
            {
                // In tunnels, set appropriate lighting and weather
                environmentController.SetTimeOfDay(0.9f); // Night-like lighting in tunnels
            }
        }
        
        // Invoke completion event
        onTransitionComplete?.Invoke();
        
        // Reset state
        transitioning = false;
        if (transitionVFX != null)
        {
            transitionVFX.SetActive(false);
        }
    }
    
    /// <summary>
    /// Shows the interaction prompt UI
    /// </summary>
    private void ShowInteractionPrompt()
    {
        // This would integrate with your UI system
        // For example:
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.ShowInteractionPrompt(interactionPrompt, transform.position);
        }
        else
        {
            // Simple debug visualization if no UI manager
            Debug.Log("Interaction available: " + interactionPrompt);
        }
    }
    
    /// <summary>
    /// Hides the interaction prompt UI
    /// </summary>
    private void HideInteractionPrompt()
    {
        // This would integrate with your UI system
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.HideInteractionPrompt();
        }
    }
    
    #endregion
}

