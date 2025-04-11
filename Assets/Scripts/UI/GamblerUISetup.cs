using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

namespace PDXUnderground.UI
{
    /// <summary>
    /// Editor utility script to help set up the Gambler UI prefabs.
    /// This script contains methods to create UI prefabs with proper structure and components.
    /// </summary>
    public class GamblerUISetup : MonoBehaviour
    {
        #region Canvas Settings
        [Header("Canvas Settings")]
        [SerializeField] private Vector2 referenceResolution = new Vector2(1920, 1080);
        [SerializeField] private CanvasScaler.ScaleMode scaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        [SerializeField] private CanvasScaler.ScreenMatchMode matchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        [SerializeField] private float matchWidthOrHeight = 0.5f; // 0 = width, 1 = height
        #endregion

        #region UI Element References
        [Header("UI Sprites")]
        [SerializeField] private Sprite cardFrameSprite;
        [SerializeField] private Sprite cardBackgroundSprite;
        [SerializeField] private Sprite buzzMeterFrameSprite;
        [SerializeField] private Sprite buzzMeterFillSprite;
        [SerializeField] private Sprite cooldownOverlaySprite;
        
        [Header("Card Type Icons")]
        [SerializeField] private Sprite attackIconSprite;
        [SerializeField] private Sprite defenseIconSprite;
        [SerializeField] private Sprite recoveryIconSprite;
        [SerializeField] private Sprite specialIconSprite;
        
        [Header("Fonts")]
        [SerializeField] private TMP_FontAsset mainFont;
        [SerializeField] private TMP_FontAsset titleFont;
        #endregion

        #region Styling
        [Header("Colors")]
        [SerializeField] private Color cardBackgroundColor = new Color(0.15f, 0.15f, 0.15f);
        [SerializeField] private Color cardFrameDefaultColor = Color.white;
        [SerializeField] private Color textDefaultColor = Color.white;
        [SerializeField] private Color buzzMeterFrameColor = new Color(0.2f, 0.2f, 0.2f);
        [SerializeField] private Color buzzMeterFillColor = new Color(0.8f, 0.6f, 0.2f);
        
        [Header("Card Type Colors")]
        [SerializeField] private Color attackCardColor = new Color(0.8f, 0.2f, 0.2f);
        [SerializeField] private Color defenseCardColor = new Color(0.2f, 0.4f, 0.8f);
        [SerializeField] private Color recoveryCardColor = new Color(0.2f, 0.8f, 0.4f);
        [SerializeField] private Color specialCardColor = new Color(0.8f, 0.6f, 0.0f);
        
        [Header("Text Sizes")]
        [SerializeField] private int cardNameFontSize = 16;
        [SerializeField] private int cardDescriptionFontSize = 12;
        [SerializeField] private int cardCostFontSize = 24;
        [SerializeField] private int healthTextFontSize = 24;
        [SerializeField] private int buzzStateFontSize = 20;
        #endregion

        #region Layout Settings
        [Header("Card Layout")]
        [SerializeField] private Vector2 cardSize = new Vector2(180f, 250f);
        [SerializeField] private float cardSpacing = 20f;
        [SerializeField] private int maxCardsVisible = 5;
        
        [Header("UI Positioning")]
        [SerializeField] private Vector2 buzzMeterSize = new Vector2(300f, 30f);
        [SerializeField] private Vector2 buzzMeterPosition = new Vector2(0f, 400f);
        [SerializeField] private Vector2 healthTextPosition = new Vector2(-750f, 450f);
        [SerializeField] private Vector2 cardHandPosition = new Vector2(0f, -400f);
        [SerializeField] private Vector2 buzzStateTextPosition = new Vector2(0f, 430f);
        #endregion

        #region Creation Methods
        /// <summary>
        /// Creates the main Gambler UI canvas with all necessary elements.
        /// </summary>
        public void CreateGamblerUICanvas()
        {
            // Create main canvas
            GameObject canvasObj = new GameObject("GamblerUICanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            // Add canvas scaler
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = scaleMode;
            scaler.referenceResolution = referenceResolution;
            scaler.screenMatchMode = matchMode;
            scaler.matchWidthOrHeight = matchWidthOrHeight;
            
            // Add graphic raycaster
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Create main UI elements
            CreateBuzzMeter(canvasObj);
            CreateHealthDisplay(canvasObj);
            CreateBuzzStateText(canvasObj);
            CreateCardHandContainer(canvasObj);
            
            // Add GamblerUI component
            GamblerUI gamblerUI = canvasObj.AddComponent<GamblerUI>();
            
            // Set up references in the GamblerUI component
            SetupGamblerUIReferences(gamblerUI);
            
            Debug.Log("GamblerUI canvas created successfully!");
        }
        
        /// <summary>
        /// Creates a card prefab with all necessary elements.
        /// </summary>
        public void CreateCardPrefab()
        {
            // Create card game object
            GameObject cardObj = new GameObject("CardPrefab");
            RectTransform rectTransform = cardObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = cardSize;
            
            // Add card background
            GameObject bgObj = new GameObject("Background");
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.SetParent(rectTransform);
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.sprite = cardBackgroundSprite;
            bgImage.color = cardBackgroundColor;
            
            // Add card frame (this will be the main button)
            GameObject frameObj = new GameObject("Frame");
            RectTransform frameRect = frameObj.AddComponent<RectTransform>();
            frameRect.SetParent(rectTransform);
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.offsetMin = Vector2.zero;
            frameRect.offsetMax = Vector2.zero;
            
            Image frameImage = frameObj.AddComponent<Image>();
            frameImage.sprite = cardFrameSprite;
            frameImage.color = cardFrameDefaultColor;
            
            Button cardButton = frameObj.AddComponent<Button>();
            cardButton.targetGraphic = frameImage;
            
            // Add card name text
            GameObject nameObj = new GameObject("Name");
            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.SetParent(rectTransform);
            nameRect.anchorMin = new Vector2(0.5f, 1f);
            nameRect.anchorMax = new Vector2(0.5f, 1f);
            nameRect.pivot = new Vector2(0.5f, 1f);
            nameRect.sizeDelta = new Vector2(cardSize.x - 20f, 30f);
            nameRect.anchoredPosition = new Vector2(0f, -10f);
            
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.font = titleFont;
            nameText.fontSize = cardNameFontSize;
            nameText.color = textDefaultColor;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.text = "Card Name";
            
            // Add card description text
            GameObject descObj = new GameObject("Description");
            RectTransform descRect = descObj.AddComponent<RectTransform>();
            descRect.SetParent(rectTransform);
            descRect.anchorMin = new Vector2(0f, 0f);
            descRect.anchorMax = new Vector2(1f, 1f);
            descRect.pivot = new Vector2(0.5f, 0.5f);
            descRect.offsetMin = new Vector2(10f, 60f);
            descRect.offsetMax = new Vector2(-10f, -50f);
            
            TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.font = mainFont;
            descText.fontSize = cardDescriptionFontSize;
            descText.color = textDefaultColor;
            descText.alignment = TextAlignmentOptions.Center;
            descText.text = "Card description goes here. This explains what the card does when played.";
            
            // Add energy cost text
            GameObject costObj = new GameObject("EnergyCost");
            RectTransform costRect = costObj.AddComponent<RectTransform>();
            costRect.SetParent(rectTransform);
            costRect.anchorMin = new Vector2(0f, 1f);
            costRect.anchorMax = new Vector2(0f, 1f);
            costRect.pivot = new Vector2(0f, 1f);
            costRect.sizeDelta = new Vector2(40f, 40f);
            costRect.anchoredPosition = new Vector2(10f, -10f);
            
            TextMeshProUGUI costText = costObj.AddComponent<TextMeshProUGUI>();
            costText.font = titleFont;
            costText.fontSize = cardCostFontSize;
            costText.color = textDefaultColor;
            costText.alignment = TextAlignmentOptions.Center;
            costText.text = "5";
            
            // Add card type icon
            GameObject iconObj = new GameObject("TypeIcon");
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.SetParent(rectTransform);
            iconRect.anchorMin = new Vector2(1f, 1f);
            iconRect.anchorMax = new Vector2(1f, 1f);
            iconRect.pivot = new Vector2(1f, 1f);
            iconRect.sizeDelta = new Vector2(40f, 40f);
            iconRect.anchoredPosition = new Vector2(-10f, -10f);
            
            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.sprite = attackIconSprite;
            
            // Add cooldown overlay
            GameObject cooldownObj = new GameObject("CooldownOverlay");
            RectTransform cooldownRect = cooldownObj.AddComponent<RectTransform>();
            cooldownRect.SetParent(rectTransform);
            cooldownRect.anchorMin = Vector2.zero;
            cooldownRect.anchorMax = Vector2.one;
            cooldownRect.offsetMin = Vector2.zero;
            cooldownRect.offsetMax = Vector2.zero;
            
            Image cooldownImage = cooldownObj.AddComponent<Image>();
            cooldownImage.sprite = cooldownOverlaySprite;
            cooldownImage.color = new Color(0f, 0f, 0f, 0.6f);
            cooldownImage.fillMethod = Image.FillMethod.Radial360;
            cooldownImage.fillOrigin = 2; // Bottom
            cooldownImage.fillAmount = 0.5f; // Half filled
            cooldownObj.SetActive(false); // Hide by default
            
            Debug.Log("Card prefab created successfully!");
            
            // Save prefab
#if UNITY_EDITOR
            string prefabPath = "Assets/Resources/Prefabs/UI";
            if (!System.IO.Directory.Exists(prefabPath))
            {
                System.IO.Directory.CreateDirectory(prefabPath);
            }
            
            string assetPath = prefabPath + "/CardPrefab.prefab";
            PrefabUtility.SaveAsPrefabAsset(cardObj, assetPath);
            DestroyImmediate(cardObj);
            Debug.Log("Card prefab saved at: " + assetPath);
#endif
        }
        
        private void CreateBuzzMeter(GameObject parent)
        {
            // Create buzz meter container
            GameObject meterObj = new GameObject("BuzzMeter");
            RectTransform meterRect = meterObj.AddComponent<RectTransform>();
            meterRect.SetParent(parent.transform);
            meterRect.anchorMin = new Vector2(0.5f, 0.5f);
            meterRect.anchorMax = new Vector2(0.5f, 0.5f);
            meterRect.pivot = new Vector2(0.5f, 0.5f);
            meterRect.sizeDelta = buzzMeterSize;
            meterRect.anchoredPosition = buzzMeterPosition;
            
            // Add meter frame
            GameObject frameObj = new GameObject("Frame");
            RectTransform frameRect = frameObj.AddComponent<RectTransform>();
            frameRect.SetParent(meterRect);
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.offsetMin = Vector2.zero;
            frameRect.offsetMax = Vector2.zero;
            
            Image frameImage = frameObj.AddComponent<Image>();
            frameImage.sprite = buzzMeterFrameSprite;
            frameImage.color = buzzMeterFrameColor;
            
            // Add meter fill
            GameObject fillObj = new GameObject("Fill");
            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.SetParent(meterRect);
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(2f, 2f);
            fillRect.offsetMax = new Vector2(-2f, -2f);
            
            Image fillImage = fillObj.AddComponent<Image>();
            fillImage.sprite = buzzMeterFillSprite;
            fillImage.color = buzzMeterFillColor;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = 0; // Left
            fillImage.fillAmount = 0.75f; // 75% filled
        }
        
        private void CreateHealthDisplay(GameObject parent)
        {
            // Create health text
            GameObject healthObj = new GameObject("HealthText");
            RectTransform healthRect = healthObj.AddComponent<RectTransform>();
            healthRect.SetParent(parent.transform);
            healthRect.anchorMin = new Vector2(0.5f, 0.5f);
            healthRect.anchorMax = new Vector2(0.5f, 0.5f);
            healthRect.pivot = new Vector2(0.5f, 0.5f);
            healthRect.sizeDelta = new Vector2(200f, 40f);
            healthRect.anchoredPosition = healthTextPosition;
            
            TextMeshProUGUI healthText = healthObj.AddComponent<TextMeshProUGUI>();
            healthText.font = mainFont;
            healthText.fontSize = healthTextFontSize;
            healthText.color = textDefaultColor;
            healthText.alignment = TextAlignmentOptions.Left;
            healthText.text = "Health: 100/100";
        }
        
        private void CreateBuzzStateText(GameObject parent)
        {
            // Create buzz state text
            GameObject stateObj = new GameObject("BuzzStateText");
            RectTransform stateRect = stateObj.AddComponent<RectTransform>();
            stateRect.SetParent(parent.transform);
            stateRect.anchorMin = new Vector2(0.5f, 0.5f);
            stateRect.anchorMax = new Vector2(0.5f, 0.5f);
            stateRect.pivot = new Vector2(0.5f, 0.5f);
            stateRect.sizeDelta = new Vector2(200f, 30f);
            stateRect.anchoredPosition = buzzStateTextPosition;
            
            TextMeshProUGUI stateText = stateObj.AddComponent<TextMeshProUGUI>();
            stateText.font = mainFont;
            stateText.fontSize = buzzStateFontSize;
            stateText.color = buzzMeterFillColor;
            stateText.alignment = TextAlignmentOptions.Center;
            stateText.text = "Normal";
        }
        
        private void CreateCardHandContainer(GameObject parent)
        {
            // Create card hand container
            GameObject containerObj = new GameObject("CardHandContainer");
            RectTransform containerRect = containerObj.AddComponent<RectTransform>();
            containerRect.SetParent(parent.transform);
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            containerRect.sizeDelta = new Vector2(cardSize.x * maxCardsVisible + cardSpacing * (maxCardsVisible - 1), cardSize.y);
            containerRect.anchoredPosition = cardHandPosition;
            
            // Add a horizontal layout group for automatic card positioning
            HorizontalLayoutGroup layoutGroup = containerObj.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = cardSpacing;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
        }
        
        private void SetupGamblerUIReferences(GamblerUI gamblerUI)
        {
            // Find components and set up references
            Canvas canvas = gamblerUI.GetComponent<Canvas>();
            
            // Set buzz meter references
            Transform buzzMeter = canvas.transform.Find("BuzzMeter");
            if (buzzMeter != null)
            {
                Image buzzMeterFill = buzzMeter.Find("Fill").GetComponent<Image>();
                Image buzzMeterFrame = buzzMeter.Find("Frame").GetComponent<Image>();
                
                // Use reflection to set private serialized fields
                SetPrivateField(gamblerUI, "buzzMeterFill", buzzMeterFill);
                SetPrivateField(gamblerUI, "buzzMeterFrame", buzzMeterFrame);
            }
            
            // Set health text reference
            Transform healthText = canvas.transform.Find("HealthText");
            if (healthText != null)
            {
                TextMeshProUGUI healthTextComponent = healthText.GetComponent<TextMeshProUGUI>();
                SetPrivateField(gamblerUI, "healthText", healthTextComponent);
            }
            
            // Set buzz state text reference
            Transform buzzStateText = canvas.transform.Find("BuzzStateText");
            if (buzzStateText != null)
            {
                TextMeshProUGUI buzzStateTextComponent = buzzStateText.GetComponent<TextMeshProUGUI>();
                SetPrivateField(gamblerUI, "buzzStateText", buzzStateTextComponent);
            }
            
            // Set card hand container reference
            Transform cardHand = canvas.transform.Find("CardHandContainer");
            if (cardHand != null)
            {
                SetPrivateField(gamblerUI, "cardHandContainer", cardHand);
            }
            
            // Set card prefab reference
            // Note: We'll need to assign the actual prefab in the editor, 
            // as we don't have a reference to it here
            
            // Set color references
            SetPrivateField(gamblerUI, "buzzNormalColor", buzzMeterFillColor);
            SetPrivateField(gamblerUI, "buzzLowColor", new Color(0.7f, 0.3f, 0.1f));
            SetPrivateField(gamblerUI, "buzzCriticalColor", new Color(0.8f, 0.1f, 0.1f));
            
            SetPrivateField(gamblerUI, "attackCardColor", attackCardColor);
            SetPrivateField(gamblerUI, "defenseCardColor", defenseCardColor);
            SetPrivateField(gamblerUI, "recoveryCardColor", recoveryCardColor);
            SetPrivateField(gamblerUI, "specialCardColor", specialCardColor);
            
            // Set layout parameters
            SetPrivateField(gamblerUI, "maxCardsVisible", maxCardsVisible);
            SetPrivateField(gamblerUI, "cardSpacing", cardSpacing);
        }
        
        private void SetPrivateField(object instance, string fieldName, object value)
        {
            var field = instance.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
                
            if (field != null)
            {
                field.SetValue(instance, value);
            }
            else
            {
                Debug.LogWarning($"Field {fieldName} not found on {instance.GetType().Name}");
            }
        }
        
        /// <summary>
        /// Creates all necessary UI prefabs for the Gambler character.
        /// </summary>
        public void CreateAllPrefabs()
        {
            CreateCardPrefab();
            CreateGamblerUICanvas();
        }
        #endregion
        
        #if UNITY_EDITOR
        /// <summary>
        /// Static class containing editor menu items for easy prefab creation.
        /// </summary>
        [UnityEditor.MenuItem("PDX Underground/Create UI Prefabs/Card Prefab")]
        private static void CreateCardPrefabMenuItem()
        {
            // Create a temporary GameObject with the GamblerUISetup component
            GameObject tempObj = new GameObject("TempGamblerUISetup");
            GamblerUISetup setup = tempObj.AddComponent<GamblerUISetup>();
            
            // Create the card prefab
            setup.CreateCardPrefab();
            
            // Clean up
            DestroyImmediate(tempObj);
        }
        
        [UnityEditor.MenuItem("PDX Underground/Create UI Prefabs/Gambler UI Canvas")]
        private static void CreateGamblerUICanvasMenuItem()
        {
            // Create a temporary GameObject with the GamblerUISetup component
            GameObject tempObj = new GameObject("TempGamblerUISetup");
            GamblerUISetup setup = tempObj.AddComponent<GamblerUISetup>();
            
            // Create the UI canvas
            setup.CreateGamblerUICanvas();
            
            // Clean up
            DestroyImmediate(tempObj);
        }
        
        [UnityEditor.MenuItem("PDX Underground/Create UI Prefabs/All Prefabs")]
        private static void CreateAllPrefabsMenuItem()
        {
            // Create a temporary GameObject with the GamblerUISetup component
            GameObject tempObj = new GameObject("TempGamblerUISetup");
            GamblerUISetup setup = tempObj.AddComponent<GamblerUISetup>();
            
            // Create all prefabs
            setup.CreateAllPrefabs();
            
            // Clean up
            DestroyImmediate(tempObj);
        }
        #endif
    }
}
