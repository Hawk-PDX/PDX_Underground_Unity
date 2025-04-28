using UnityEngine;
using UnityEngine.UI;
using PDXUnderground.Player;
using PDXUnderground.UI;
using PDXUnderground.Core;

namespace PDXUnderground.Test
{
    /// <summary>
    /// Quick start test script for the Gambler character.
    /// Provides UI controls for testing basic functionality.
    /// </summary>
    public class GamblerTestQuickStartExample : MonoBehaviour
    {
        [Header("References")]
        public GamblerCharacter gamblerCharacter;
        public BuzzUIController buzzUIController;
        
        [Header("Test Controls")]
        [SerializeField] private bool enableTestUI = true;
        [SerializeField] private KeyCode toggleTestUIKey = KeyCode.BackQuote; // ` key
        
        // UI elements
        private Canvas testCanvas;
        private GameObject testPanel;
        
        private void Start()
        {
            if (enableTestUI)
            {
                CreateTestUI();
            }
            
            // Try to find references if not set
            if (gamblerCharacter == null)
            {
                gamblerCharacter = FindObjectOfType<GamblerCharacter>();
            }
            
            if (buzzUIController == null)
            {
                buzzUIController = FindObjectOfType<BuzzUIController>();
            }
        }
        
        private void Update()
        {
            // Toggle test UI
            if (Input.GetKeyDown(toggleTestUIKey))
            {
                ToggleTestUI();
            }
        }
        
        private void CreateTestUI()
        {
            // Create canvas
            GameObject canvasObj = new GameObject("TestCanvas");
            testCanvas = canvasObj.AddComponent<Canvas>();
            testCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            testCanvas.sortingOrder = 100; // Ensure it's on top
            
            // Add canvas scaler
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            // Add raycaster
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Create panel
            testPanel = CreatePanel();
            testPanel.transform.SetParent(testCanvas.transform, false);
            
            // Add test buttons
            CreateTestButton("Increase Buzz (+20)", () => { if (gamblerCharacter != null) gamblerCharacter.UpdateBuzz(20f); });
            CreateTestButton("Decrease Buzz (-20)", () => { if (gamblerCharacter != null) gamblerCharacter.UpdateBuzz(-20f); });
            CreateTestButton("Toggle Abilities", ToggleAbilities);
            CreateTestButton("Reset Character", ResetCharacter);
            
            Debug.Log("Test UI created");
        }
        
        private GameObject CreatePanel()
        {
            GameObject panel = new GameObject("TestPanel");
            RectTransform rectTransform = panel.AddComponent<RectTransform>();
            Image image = panel.AddComponent<Image>();
            
            // Set panel properties
            image.color = new Color(0, 0, 0, 0.8f);
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.sizeDelta = new Vector2(200, 300);
            rectTransform.anchoredPosition = new Vector2(10, -10);
            
            // Add layout group
            VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 5;
            
            return panel;
        }
        
        private void CreateTestButton(string text, UnityEngine.Events.UnityAction action)
        {
            GameObject buttonObj = new GameObject(text);
            RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
            buttonObj.transform.SetParent(testPanel.transform, false);
            
            // Add button component
            Button button = buttonObj.AddComponent<Button>();
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.2f);
            
            // Set up button appearance
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.2f, 0.2f, 0.2f);
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f);
            colors.pressedColor = new Color(0.1f, 0.1f, 0.1f);
            button.colors = colors;
            
            // Add text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            TMPro.TextMeshProUGUI tmp = textObj.AddComponent<TMPro.TextMeshProUGUI>();
            tmp.text = text;
            tmp.color = Color.white;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            
            // Set up layout
            rectTransform.sizeDelta = new Vector2(0, 30);
            RectTransform textRectTransform = textObj.GetComponent<RectTransform>();
            textRectTransform.anchorMin = Vector2.zero;
            textRectTransform.anchorMax = Vector2.one;
            textRectTransform.sizeDelta = Vector2.zero;
            
            // Add click handler
            button.onClick.AddListener(action);
        }
        
        private void ToggleTestUI()
        {
            if (testCanvas != null)
            {
                testCanvas.gameObject.SetActive(!testCanvas.gameObject.activeSelf);
            }
        }
        
        private void ToggleAbilities()
        {
            if (gamblerCharacter != null)
            {
                // Toggle abilities - implementation depends on your ability system
                Debug.Log("Toggle abilities - implement based on your ability system");
            }
        }
        
        private void ResetCharacter()
        {
            if (gamblerCharacter != null)
            {
                // Reset character state
                gamblerCharacter.ResetCharacter();
                Debug.Log("Character reset");
            }
        }
        
        private void OnDestroy()
        {
            if (testCanvas != null)
            {
                Destroy(testCanvas.gameObject);
            }
        }
    }
}

