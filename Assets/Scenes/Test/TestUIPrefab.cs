using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace PDXUnderground.Test.Prefabs
{
    /// <summary>
    /// Component used to configure a game object as a TestUI prefab for the PDX Underground game.
    /// This script helps set up the debug UI elements for testing.
    /// </summary>
    [ExecuteInEditMode]
    public class TestUIPrefab : MonoBehaviour
    {
        [Header("Debug Panel")]
        [SerializeField] private bool showFPS = true;
        [SerializeField] private bool showPlayerPosition = true;
        [SerializeField] private bool showBuzzLevel = true;
        
        [Header("Test Controls")]
        [SerializeField] private bool enableBuzzControls = true;
        [SerializeField] private bool enableEnvironmentControls = true;
        [SerializeField] private bool enableCardControls = true;
        
        [Header("UI References")]
        [SerializeField] private Canvas debugCanvas;
        [SerializeField] private GameObject debugPanel;
        [SerializeField] private GameObject controlPanel;
        [SerializeField] private TextMeshProUGUI debugText;
        
        private Dictionary<string, Button> controlButtons = new Dictionary<string, Button>();
        
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
            // Add Canvas if needed
            if (debugCanvas == null)
            {
                // Check if we already have a canvas
                debugCanvas = GetComponent<Canvas>();
                if (debugCanvas == null)
                {
                    debugCanvas = gameObject.AddComponent<Canvas>();
                    debugCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    gameObject.AddComponent<CanvasScaler>();
                    gameObject.AddComponent<GraphicRaycaster>();
                    Debug.Log("Added Canvas components to TestUI prefab");
                }
            }
            
            // Create debug panel if it doesn't exist
            if (debugPanel == null)
            {
                debugPanel = CreateDebugPanel();
            }
            
            // Create control panel if it doesn't exist
            if (controlPanel == null)
            {
                controlPanel = CreateControlPanel();
            }
        }
        
        private GameObject CreateDebugPanel()
        {
            // Create debug panel game object
            GameObject panel = new GameObject("DebugPanel");
            panel.transform.SetParent(transform, false);
            
            // Set up rect transform
            RectTransform rectTransform = panel.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anc

