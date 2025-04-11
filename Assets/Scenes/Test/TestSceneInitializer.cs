using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using PDXUnderground.Core;

namespace PDXUnderground.Test
{
    /// <summary>
    /// Test-specific scene initializer that extends GameSceneSetup with additional
    /// testing functionality. This class configures the scene for testing with debug UI,
    /// specialized camera settings, and input handling for test controls.
    /// </summary>
    public class TestSceneInitializer : GameSceneSetup
    {
        [Header("Test Configuration")]
        [SerializeField] private bool enableTestControls = true;
        [SerializeField] private bool enableDebugUI = true;
        [SerializeField] private bool autoSetupTestEnvironment = true;
        
        [Header("Test UI")]
        [SerializeField] private GameObject testUIPrefab;
        [SerializeField] private Canvas debugCanvas;
        
        [Header("Debug Settings")]
        [SerializeField] private bool showFPS = true;
        [SerializeField] private bool showPlayerPosition = true;
        [SerializeField] private bool showBuzzLevel = true;
        
        // Instance references
        private GameObject testUI;
        private Dictionary<string, Button> testButtons = new Dictionary<string, Button>();
        private TextMeshProUGUI debugText;
        private float updateInterval = 0.5f;
        private float accum = 0;
        private int frames = 0;
        private float timeLeft;
        private float fps;
        
        // Debug info
        private Vector3 playerPosition;
        private float currentBuzzLevel;
        private string currentEnvironment;
        
        protected new void Awake()
        {
            // Call base class Awake
            base.Awake();
            
            // Initialize test-specific properties
            timeLeft = updateInterval;
        }
        
        protected new void Start()
        {
            // If auto setup is enabled, don't call base Start
            // because we'll handle initialization manually
            if (!autoSetupTestEnvironment)
            {
                base.Start();
            }
            else
            {
                // Force test mode to be enabled
                SetTestMode(true);
                
                // Initialize scene with test configuration
                InitializeScene();
                
                // Set up test UI and controls
                if (enableTestControls)
                {
                    SetupTestUI();
                }
                
                // Set up debug display
                if (enableDebugUI)
                {
                    SetupDebugUI();
                }
                
                Debug.Log("Test scene initialized with test configuration");
            }
        }
        
        protected new void Update()
        {
            // Process debug info updates
            if (enableDebugUI)
            {
                UpdateDebugInfo();
            }
            
            // Process test-specific input
            ProcessTestInput();
        }
        
        /// <summary>
        /// Sets up the UI elements specific to testing
        /// </summary>
        private void SetupTestUI()
        {
            // Create test UI if prefab is assigned, otherwise create a simple one
            if (testUIPrefab != null)
            {
                testUI = Instantiate(testUIPrefab);
                testUI.name = "TestUI";
            }
            else
            {
                // Create a simple test UI if no prefab is assigned
                CreateSimpleTestUI();
            }
        }
        
        /// <summary>
        /// Creates a simple test UI with buttons for common test actions
        /// </summary>
        private void CreateSimpleTestUI()
        {
            // Create canvas if needed
            if (debugCanvas == null)
            {
                GameObject canvasObj = new GameObject("TestCanvas");
                debugCanvas = canvasObj.AddComponent<Canvas>();
                debugCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            
            // Create panel
            GameObject panelObj = new GameObject("TestPanel");
            panelObj.transform.SetParent(debugCanvas.transform, false);
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(0, 1);
            panelRect.pivot = new Vector2(0, 1);
            panelRect.sizeDelta = new Vector2(200, 0);
            
            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.7f);
            
            // Create buttons for common test actions
            CreateTestButton("IncBuzz", "Increase Buzz", panelObj.transform, 0, () => {
                if (MainGameController.Instance != null)
                    MainGameController.Instance.ModifyPlayerBuzz(10f);
            });
            
            CreateTestButton("DecBuzz", "Decrease Buzz", panelObj.transform, 1, () => {
                if (MainGameController.Instance != null)
                    MainGameController.Instance.ModifyPlayerBuzz(-10f);
            });
            
            CreateTestButton("CritBuzz", "Critical Buzz", panelObj.transform, 2, () => {
                if (MainGameController.Instance != null)
                    MainGameController.Instance.SetPlayerBuzzToCritical();
            });
            
            CreateTestButton("DrawCard", "Draw Card", panelObj.transform, 3, () => {
                if (MainGameController.Instance != null)
                    MainGameController.Instance.PlayerDrawCard();
            });
            
            CreateTestButton("Street", "Street Env", panelObj.transform, 4, () => {
                ChangeSceneEnvironment(0);
            });
            
            CreateTestButton("Tunnel", "Tunnel Env", panelObj.transform, 5, () => {
                ChangeSceneEnvironment(1);
            });
            
            CreateTestButton("Speakeasy", "Speakeasy Env", panelObj.transform, 6, () => {
                ChangeSceneEnvironment(2);
            });
            
            CreateTestButton("Reset", "Reset Scene", panelObj.transform, 7, () => {
                ResetScene();
            });
            
            Debug.Log("Created simple test UI");
        }
        
        /// <summary>
        /// Helper method to create a test button
        /// </summary>
        private void CreateTestButton(string id, string label, Transform parent, int index, UnityEngine.Events.UnityAction action)
        {
            GameObject buttonObj = new GameObject($"Button_{id}");
            buttonObj.transform.SetParent(parent, false);
            
            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0, 1);
            buttonRect.anchorMax = new Vector2(1, 1);
            buttonRect.pivot = new Vector2(0.5f, 1);
            buttonRect.anchoredPosition = new Vector2(0, -50 - (index * 40));
            buttonRect.sizeDelta = new Vector2(-20, 30);
            
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.2f);
            
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.2f, 0.2f, 0.2f);
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f);
            colors.pressedColor = new Color(0.1f, 0.1f, 0.1f);
            button.colors = colors;
            
            // Create the button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = Vector2.zero;
            
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 14;
            
            // Add the button click event
            button.onClick.AddListener(action);
            
            // Store the button for later use
            testButtons[id] = button;
        }
        
        /// <summary>
        /// Sets up the debug UI display
        /// </summary>
        private void SetupDebugUI()
        {
            // Create debug text if it doesn't exist
            if (debugText == null)
            {
                // Create canvas if needed
                if (debugCanvas == null)
                {
                    GameObject canvasObj = new GameObject("DebugCanvas");
                    debugCanvas = canvasObj.AddComponent<Canvas>();
                    debugCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvasObj.AddComponent<CanvasScaler>();
                    canvasObj.AddComponent<GraphicRaycaster>();
                }
                
                // Create debug text object
                GameObject debugTextObj = new GameObject("DebugText");
                debugTextObj.transform.SetParent(debugCanvas.transform, false);
                
                // Set up rect transform
                RectTransform textRect = debugTextObj.AddComponent<RectTransform>();
                textRect.anchorMin = new Vector2(0, 1);
                textRect.anchorMax = new Vector2(1, 1);
                textRect.pivot = new Vector2(0, 1);
                textRect.anchoredPosition = new Vector2(210, 0);
                textRect.sizeDelta = new Vector2(-220, 200);
                
                // Add TextMeshProUGUI component
                debugText = debugTextObj.AddComponent<TextMeshProUGUI>();
                debugText.alignment = TextAlignmentOptions.TopLeft;
                debugText.fontSize = 16;
                debugText.color = Color.white;
                
                Debug.Log("Created debug UI display");
            }
        }
        
        /// <summary>
        /// Updates the debug information displayed on screen
        /// </summary>
        private void UpdateDebugInfo()
        {
            // Skip if debug text is not set up
            if (debugText == null)
                return;
            
            // Calculate FPS
            accum += Time.timeScale / Time.deltaTime;
            frames++;
            
            if (Time.time > timeLeft)
            {
                fps = accum / frames;
                timeLeft = Time.time + updateInterval;
                accum = 0;
                frames = 0;
            }
            
            // Get player position if available
            if (showPlayerPosition)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerPosition = player.transform.position;
                }
            }
            
            // Get buzz level if available
            if (showBuzzLevel && MainGameController.Instance != null)
            {
                GamblerCharacter character = FindObjectOfType<GamblerCharacter>();
                if (character != null)
                {
                    currentBuzzLevel = character.currentBuzz;
                }
            }
            
            // Get current environment if available
            if (MainGameController.Instance != null)
            {
                currentEnvironment = MainGameController.Instance.GetCurrentEnvironmentName();
            }
            
            // Build debug text
            string debugInfo = "DEBUG INFO\n";
            
            if (showFPS)
                debugInfo += $"FPS: {fps:F1}\n";
                
            if (showPlayerPosition)
                debugInfo += $"Position: {playerPosition:F2}\n";
                
            if (showBuzzLevel)
                debugInfo += $"Buzz Level: {currentBuzzLevel:F1}\n";
                
            debugInfo += $"Environment: {currentEnvironment}\n";
            debugInfo += $"Test Mode: {(isTestMode ? "Enabled" : "Disabled")}\n";
            debugInfo += $"Game State: {MainGameController.Instance?.GetCurrentGameState()}\n";
            
            // Display the debug info
            debugText.text = debugInfo;
        }
        
        /// <summary>
        /// Processes test-specific input commands
        /// </summary>
        private void ProcessTestInput()
        {
            // Toggle debug display with F1
            if (Input.GetKeyDown(KeyCode.F1))
            {
                enableDebugUI = !enableDebugUI;
                if (debugText != null)
                    debugText.gameObject.SetActive(enableDebugUI);
            }
            
            // Toggle test UI with F2
            if (Input.GetKeyDown(KeyCode.F2))
            {
                enableTestControls = !enableTestControls;
                if (testUI != null)
                    testUI.SetActive(enableTestControls);
            }
            
            // Reset scene with F5
            if (Input.GetKeyDown(KeyCode.F5))
            {
                ResetScene();
            }
            
            // Environment hotkeys (1,2,3)
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                ChangeSceneEnvironment(0); // Streets
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                ChangeSceneEnvironment(1); // Tunnels
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                ChangeSceneEnvironment(2); // Speakeasy
            }
            
            // Test ability hotkeys
            if (Input.GetKeyDown(KeyCode.T))
            {
                if (MainGameController.Instance != null)
                    MainGameController.Instance.PlayerDrawCard();
            }
        }
    }
}

