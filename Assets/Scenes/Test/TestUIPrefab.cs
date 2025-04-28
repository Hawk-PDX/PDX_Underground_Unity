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
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.sizeDelta = new Vector2(300, 200);
            rectTransform.anchoredPosition = new Vector2(10, -10);
            
            // Add background image
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            // Create debug text
            GameObject textObj = new GameObject("DebugText");
            textObj.transform.SetParent(panel.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.sizeDelta = new Vector2(-20, -20);
            textRect.anchoredPosition = new Vector2(10, -10);
            
            TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.fontSize = 14;
            tmpText.color = Color.white;
            tmpText.alignment = TextAlignmentOptions.TopLeft;
            tmpText.text = "FPS: 60\nPlayer Position: (0, 0, 0)\nBuzz Level: 0%";
            
            // Store reference to the debug text
            debugText = tmpText;
            
            // Store reference to the debug panel
            debugPanel = panel;
            
            return panel;
        }
        
        private GameObject CreateControlPanel()
        {
            // Create control panel game object
            GameObject panel = new GameObject("ControlPanel");
            panel.transform.SetParent(transform, false);
            
            // Set up rect transform
            RectTransform rectTransform = panel.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(1, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(1, 1);
            rectTransform.sizeDelta = new Vector2(300, 400);
            rectTransform.anchoredPosition = new Vector2(-10, -10);
            
            // Add background image
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            // Create layout for button groups
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(panel.transform, false);
            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.sizeDelta = new Vector2(-20, -20);
            contentRect.anchoredPosition = new Vector2(10, -10);
            
            VerticalLayoutGroup verticalLayout = contentObj.AddComponent<VerticalLayoutGroup>();
            verticalLayout.spacing = 10;
            verticalLayout.padding = new RectOffset(5, 5, 5, 5);
            verticalLayout.childAlignment = TextAnchor.UpperCenter;
            verticalLayout.childControlWidth = true;
            verticalLayout.childControlHeight = false;
            verticalLayout.childForceExpandWidth = true;
            verticalLayout.childForceExpandHeight = false;
            
            // Add control sections based on enabled flags
            if (enableBuzzControls)
            {
                AddButtonSection(contentObj, "Buzz Controls", new string[] {
                    "Increase Buzz (+10)",
                    "Decrease Buzz (-10)",
                    "Set to Critical (90%)",
                    "Reset Buzz (0%)"
                });
            }
            
            if (enableEnvironmentControls)
            {
                AddButtonSection(contentObj, "Environment Controls", new string[] {
                    "Toggle Day/Night",
                    "Spawn Random NPC",
                    "Toggle Weather",
                    "Reset Environment"
                });
            }
            
            if (enableCardControls)
            {
                AddButtonSection(contentObj, "Card Controls", new string[] {
                    "Draw Card",
                    "Discard Hand",
                    "Add Random Card",
                    "Toggle Card UI"
                });
            }
            
            // Store reference to the control panel
            controlPanel = panel;
            
            return panel;
        }
        
        private void AddButtonSection(GameObject parent, string sectionTitle, string[] buttonLabels)
        {
            // Create section container
            GameObject sectionObj = new GameObject(sectionTitle);
            sectionObj.transform.SetParent(parent.transform, false);
            
            // Set up vertical layout
            VerticalLayoutGroup sectionLayout = sectionObj.AddComponent<VerticalLayoutGroup>();
            sectionLayout.spacing = 5;
            sectionLayout.padding = new RectOffset(0, 0, 0, 10);
            sectionLayout.childAlignment = TextAnchor.UpperCenter;
            sectionLayout.childControlWidth = true;
            sectionLayout.childControlHeight = false;
            sectionLayout.childForceExpandWidth = true;
            sectionLayout.childForceExpandHeight = false;
            
            // Add content size fitter to control height
            ContentSizeFitter sizeFitter = sectionObj.AddComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // Add section title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(sectionObj.transform, false);
            
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.sizeDelta = new Vector2(0, 25);
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = sectionTitle;
            titleText.fontSize = 16;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;
            
            // Create buttons
            for (int i = 0; i < buttonLabels.Length; i++)
            {
                string buttonLabel = buttonLabels[i];
                CreateControlButton(sectionObj, buttonLabel);
            }
        }
        
        private Button CreateControlButton(GameObject parent, string label)
        {
            GameObject buttonObj = new GameObject(label);
            buttonObj.transform.SetParent(parent.transform, false);
            
            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(0, 30);
            
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.2f);
            
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.2f, 0.2f, 0.2f);
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f);
            colors.pressedColor = new Color(0.15f, 0.15f, 0.15f);
            button.colors = colors;
            
            // Add button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = label;
            buttonText.fontSize = 14;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;
            
            // Store button reference for later use
            string buttonKey = label.Replace(" ", "");
            controlButtons[buttonKey] = button;
            
            return button;
        }
        
        private void Update()
        {
            // Only update in play mode
            if (Application.isPlaying && debugText != null)
            {
                UpdateDebugText();
            }
        }
        
        private void UpdateDebugText()
        {
            string debugInfo = "";
            
            // Add FPS info
            if (showFPS)
            {
                float fps = 1.0f / Time.deltaTime;
                debugInfo += $"FPS: {Mathf.Round(fps)}\n";
            }
            
            // Add player position
            if (showPlayerPosition)
            {
                Transform playerTransform = FindPlayerTransform();
                if (playerTransform != null)
                {
                    Vector3 pos = playerTransform.position;
                    debugInfo += $"Player Pos: ({pos.x:F1}, {pos.y:F1}, {pos.z:F1})\n";
                }
                else
                {
                    debugInfo += "Player: Not Found\n";
                }
            }
            
            // Add buzz level
            if (showBuzzLevel)
            {
                float buzzLevel = GetCurrentBuzzLevel();
                debugInfo += $"Buzz Level: {buzzLevel:F0}%";
            }
            
            // Update debug text
            debugText.text = debugInfo;
        }
        
        private Transform FindPlayerTransform()
        {
            // Find player by tag
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            return player?.transform;
        }
        
        private float GetCurrentBuzzLevel()
        {
            // This would normally access the actual buzz system
            // For now, return a default value or calculate one for testing
            // You can replace this with a reference to the actual buzz system later
            return Time.time % 100; // Just for demonstration
        }
        
        // Public API for accessing the UI controls
        
        /// <summary>
        /// Gets a button from the control panel by name
        /// </summary>
        /// <param name="buttonName">The name of the button without spaces (e.g., "IncreaseBuzz(+10)")</param>
        public Button GetControlButton(string buttonName)
        {
            if (controlButtons.TryGetValue(buttonName, out Button button))
            {
                return button;
            }
            
            Debug.LogWarning($"Button '{buttonName}' not found in control panel");
            return null;
        }
        
        /// <summary>
        /// Shows or hides the debug panel
        /// </summary>
        public void SetDebugPanelActive(bool active)
        {
            if (debugPanel != null)
            {
                debugPanel.SetActive(active);
            }
        }
        
        /// <summary>
        /// Shows or hides the control panel
        /// </summary>
        public void SetControlPanelActive(bool active)
        {
            if (controlPanel != null)
            {
                controlPanel.SetActive(active);
            }
        }
}
}
