using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;  // For UI components
using TMPro;          // For TextMeshProUGUI
using PDXUnderground.UI;  // For BuzzUIController

namespace PDXUnderground.Test.Prefabs
{
    /// <summary>
    /// Component used to configure a game object as a BuzzUIController prefab for the PDX Underground game.
    /// This script helps set up all required UI components for the buzz system.
    /// </summary>
    [ExecuteInEditMode]
    public class BuzzUIPrefab : MonoBehaviour
    {
        [Header("UI Components")]
        [Tooltip("Reference to the buzz meter Slider component")]
        [SerializeField] private Slider buzzMeter;
        
        [Tooltip("Reference to the buzz level text display")]
        [SerializeField] private TextMeshProUGUI buzzLevelText;
        
        [Tooltip("Reference to the critical buzz warning panel")]
        [SerializeField] private GameObject criticalWarningPanel;
        
        [Header("UI Settings")]
        [Tooltip("Color for normal buzz levels")]
        [SerializeField] private Color normalColor = new Color(0.2f, 0.8f, 1f);
        
        [Tooltip("Color for critical buzz levels")]
        [SerializeField] private Color criticalColor = new Color(1f, 0.2f, 0.2f);
        
        [Tooltip("Whether to show numerical buzz level")]
        [SerializeField] private bool showNumericalValue = true;
        
        // Required components
        private Canvas canvas;
        private CanvasScaler canvasScaler;
        private GraphicRaycaster graphicRaycaster;
        private BuzzUIController buzzUIController; // This would be your actual UI controller
        
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
            canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                Debug.Log("Added Canvas to BuzzUI prefab");
            }
            
            // Add CanvasScaler if needed
            canvasScaler = GetComponent<CanvasScaler>();
            if (canvasScaler == null)
            {
                canvasScaler = gameObject.AddComponent<CanvasScaler>();
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1920, 1080);
                canvasScaler.matchWidthOrHeight = 0.5f;
                Debug.Log("Added CanvasScaler to BuzzUI prefab");
            }
            
            // Add GraphicRaycaster if needed
            graphicRaycaster = GetComponent<GraphicRaycaster>();
            if (graphicRaycaster == null)
            {
                graphicRaycaster = gameObject.AddComponent<GraphicRaycaster>();
                Debug.Log("Added GraphicRaycaster to BuzzUI prefab");
            }
            
            // Add BuzzUIController if needed
            // Note: In a real implementation, this would be replaced with the actual component
            buzzUIController = GetComponent<BuzzUIController>();
            if (buzzUIController == null)
            {
                // For now, just log a warning
                Debug.LogWarning("BuzzUIController component is missing! Please add it manually.");
            }
            
            // Create UI elements if they don't exist
            if (transform.childCount == 0)
            {
                CreateUIElements();
            }
        }
        
        private void CreateUIElements()
        {
            // Create main panel
            GameObject panel = new GameObject("BuzzPanel");
            panel.transform.SetParent(transform, false);
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 1);
            panelRect.anchorMax = new Vector2(1, 1);
            panelRect.pivot = new Vector2(0.5f, 1);
            panelRect.sizeDelta = new Vector2(0, 80);
            panelRect.anchoredPosition = new Vector2(0, 0);
            
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            // Create buzz meter slider
            GameObject sliderObj = new GameObject("BuzzMeter");
            sliderObj.transform.SetParent(panel.transform, false);
            RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0, 0.5f);
            sliderRect.anchorMax = new Vector2(1, 0.5f);
            sliderRect.pivot = new Vector2(0.5f, 0.5f);
            sliderRect.sizeDelta = new Vector2(-100, 30);
            sliderRect.anchoredPosition = new Vector2(0, 0);
            
            Slider slider = sliderObj.AddComponent<Slider>();
            
            // Create slider background
            GameObject background = new GameObject("Background");
            background.transform.SetParent(sliderObj.transform, false);
            RectTransform bgRect = background.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f);
            
            slider.targetGraphic = bgImage;
            
            // Create slider fill area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform fillRect = fillArea.AddComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0, 0);
            fillRect.anchorMax = new Vector2(1, 1);
            fillRect.sizeDelta = new Vector2(-10, -6);
            fillRect.anchoredPosition = new Vector2(-5, 0);
            
            // Create slider fill
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillImageRect = fill.AddComponent<RectTransform>();
            fillImageRect.anchorMin = Vector2.zero;
            fillImageRect.anchorMax = new Vector2(1, 1);
            fillImageRect.sizeDelta = Vector2.zero;
            
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = normalColor;
            
            // Configure the slider
            slider.fillRect = fillImageRect;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 100f;
            slider.value = 0f;
            slider.wholeNumbers = false;
            
            // Set the reference to our class variable
            buzzMeter = slider;
            
            // Create buzz level text
            GameObject textObj = new GameObject("BuzzLevelText");
            textObj.transform.SetParent(panel.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(1, 0.5f);
            textRect.anchorMax = new Vector2(1, 0.5f);
            textRect.pivot = new Vector2(1, 0.5f);
            textRect.sizeDelta = new Vector2(80, 30);
            textRect.anchoredPosition = new Vector2(-10, 0);
            
            TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = "0%";
            tmpText.fontSize = 18;
            tmpText.alignment = TextAlignmentOptions.Right;
            tmpText.color = Color.white;
            
            // Set the reference to our class variable
            buzzLevelText = tmpText;
            
            // Create critical warning panel
            GameObject warningObj = new GameObject("CriticalWarningPanel");
            warningObj.transform.SetParent(panel.transform, false);
            RectTransform warningRect = warningObj.AddComponent<RectTransform>();
            warningRect.anchorMin = new Vector2(0, 0);
            warningRect.anchorMax = new Vector2(1, 1);
            warningRect.sizeDelta = Vector2.zero;
            warningRect.anchoredPosition = Vector2.zero;
            
            Image warningImage = warningObj.AddComponent<Image>();
            warningImage.color = new Color(1f, 0f, 0f, 0.2f);
            
            // Create warning text
            GameObject warningTextObj = new GameObject("WarningText");
            warningTextObj.transform.SetParent(warningObj.transform, false);
            RectTransform warningTextRect = warningTextObj.AddComponent<RectTransform>();
            warningTextRect.anchorMin = new Vector2(0, 0.5f);
            warningTextRect.anchorMax = new Vector2(1, 0.5f);
            warningTextRect.pivot = new Vector2(0.5f, 0.5f);
            warningTextRect.sizeDelta = new Vector2(0, 40);
            warningTextRect.anchoredPosition = Vector2.zero;
            
            TextMeshProUGUI warningTmpText = warningTextObj.AddComponent<TextMeshProUGUI>();
            warningTmpText.text = "CRITICAL BUZZ LEVEL!";
            warningTmpText.fontSize = 24;
            warningTmpText.fontStyle = FontStyles.Bold;
            warningTmpText.alignment = TextAlignmentOptions.Center;
            warningTmpText.color = Color.white;
            
            // Set the reference to our class variable and hide by default
            criticalWarningPanel = warningObj;
            criticalWarningPanel.SetActive(false);
        }
        
        /// <summary>
        /// Updates the buzz UI display with the provided buzz level
        /// </summary>
        /// <param name="buzzLevel">Current buzz level (0-100)</param>
        public void UpdateBuzzDisplay(float buzzLevel)
        {
            if (buzzMeter != null)
            {
                buzzMeter.value = buzzLevel;
                
                // Update fill color based on level
                Image fillImage = buzzMeter.fillRect.GetComponent<Image>();
                if (fillImage != null)
                {
                    fillImage.color = buzzLevel >= 80 ? criticalColor : normalColor;
                }
                
                // Update text
                if (buzzLevelText != null && showNumericalValue)
                {
                    buzzLevelText.text = Mathf.RoundToInt(buzzLevel) + "%";
                }
                
                // Show critical warning if needed
                if (criticalWarningPanel != null)
                {
                    criticalWarningPanel.SetActive(buzzLevel >= 90);
                }
            }
        }
    }
}
