using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.SceneManagement;  // For SceneManager
using PDXUnderground.Core.Interfaces;
using PDXUnderground.UI;
namespace PDXUnderground.Environment
{
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
    private bool isTransitioning = false;
    private PDXUnderground.Core.URPCameraSetup cameraSetup;
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
        
        // Get reference to URP camera setup
        cameraSetup = Camera.main?.GetComponent<PDXUnderground.Core.URPCameraSetup>();
        if (cameraSetup == null)
        {
            Debug.LogWarning("URPCameraSetup not found on main camera. Environment transitions may not have proper post-processing.");
        }
        
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
        
        // Update camera profile based on destination environment
        if (cameraSetup != null)
        {
            // Map destination name to environment type
            if (destinationAreaName.Contains("Street") || destinationAreaName.Contains("Portland"))
            {
                cameraSetup.SwitchToEnvironment(PDXUnderground.Core.URPCameraSetup.EnvironmentType.Streets);
            }
            else if (destinationAreaName.Contains("Tunnel") || destinationAreaName.Contains("Shanghai"))
            {
                cameraSetup.SwitchToEnvironment(PDXUnderground.Core.URPCameraSetup.EnvironmentType.Tunnels);
            }
            else if (destinationAreaName.Contains("Speakeasy"))
            {
                cameraSetup.SwitchToEnvironment(PDXUnderground.Core.URPCameraSetup.EnvironmentType.Speakeasy);
            }
        }
        
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
        // Use loading screen if specified
        if (useLoadingScreen && uiManager != null)
        {
            uiManager.ShowLoadingScreen();
        }
        
        // Start scene transition
        SceneManager.LoadScene(sceneReference.sceneName);
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
}
