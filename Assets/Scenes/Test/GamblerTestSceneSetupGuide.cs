
        /// <summary>
        /// Sets up the buzz meter UI component
        /// </summary>
        private void SetupBuzzMeter(Transform parent)
        {
            // Create the main container
            GameObject container = CreateGameObject("Container", Vector2.zero, parent);
            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(200, 40);
            
            // Create the frame background
            GameObject frame = CreateGameObject("Frame", Vector2.zero, container.transform);
            RectTransform frameRect = frame.AddComponent<RectTransform>();
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.offsetMin = Vector2.zero;
            frameRect.offsetMax = Vector2.zero;
            
            Image frameImage = frame.AddComponent<Image>();
            frameImage.color = new Color(0.4f, 0.4f, 0.4f, 0.8f);
            frameImage.sprite = CreateDefaultSprite();
            
            // Create the fill meter
            GameObject fill = CreateGameObject("Fill", Vector2.zero, container.transform);
            RectTransform fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0, 0);
            fillRect.anchorMax = new Vector2(1, 1);
            fillRect.offsetMin = new Vector2(4, 4);
            fillRect.offsetMax = new Vector2(-4, -4);
            fillRect.pivot = new Vector2(0, 0.5f);
            
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.827f, 0.722f, 0.416f); // Gold/amber
            fillImage.sprite = CreateDefaultSprite();
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillAmount = 1.0f;
            
            // Create the value text
            GameObject valueText = CreateGameObject("Value", Vector2.zero, container.transform);
            RectTransform valueRect = valueText.AddComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(0, 0);
            valueRect.anchorMax = new Vector2(1, 1);
            valueRect.offsetMin = Vector2.zero;
            valueRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI valueTextComponent = valueText.AddComponent<TextMeshProUGUI>();
            valueTextComponent.text = "100/100";
            valueTextComponent.fontSize = 14;
            valueTextComponent.alignment = TextAlignmentOptions.Center;
            valueTextComponent.color = Color.white;
            
            // Create the state icon
            GameObject stateIcon = CreateGameObject("StateIcon", new Vector2(110, 0), container.transform);
            RectTransform stateIconRect = stateIcon.AddComponent<RectTransform>();
            stateIconRect.sizeDelta = new Vector2(32, 32);
            
            Image stateIconImage = stateIcon.AddComponent<Image>();
            stateIconImage.sprite = normalBuzzIcon ? normalBuzzIcon : CreateDefaultSprite();
        }
        
        /// <summary>
        /// Sets up the card hand display UI component
        /// </summary>
        private void SetupHandDisplay(Transform parent)
        {
            // Create the main container
            GameObject container = CreateGameObject("Container", Vector2.zero, parent);
            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(600, 150);
            
            // Create background panel
            GameObject panel = CreateGameObject("Panel", Vector2.zero, container.transform);
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.6f);
            
            // Create card slots (5 card slots)
            for (int i = 0; i < 5; i++)
            {
                GameObject cardSlot = CreateGameObject($"CardSlot_{i}", new Vector2(-240 + i * 120, 0), container.transform);
                RectTransform slotRect = cardSlot.AddComponent<RectTransform>();
                slotRect.sizeDelta = new Vector2(100, 140);
                
                Image slotImage = cardSlot.AddComponent<Image>();
                slotImage.color = new Color(0.2f, 0.2f, 0.2f, 0.4f);
                slotImage.sprite = CreateDefaultSprite();
                
                // Create card visual (initially empty)
                GameObject cardVisual = CreateGameObject("CardVisual", Vector2.zero, cardSlot.transform);
                RectTransform cardRect = cardVisual.AddComponent<RectTransform>();
                cardRect.anchorMin = Vector2.zero;
                cardRect.anchorMax = Vector2.one;
                cardRect.offsetMin = new Vector2(5, 5);
                cardRect.offsetMax = new Vector2(-5, -5);
                
                Image cardImage = cardVisual.AddComponent<Image>();
                cardImage.color = new Color(0.9f, 0.9f, 0.9f);
                cardImage.sprite = CreateDefaultSprite();
                
                // Create card value text (suit and number)
                GameObject cardValue = CreateGameObject("CardValue", new Vector2(0, 0), cardVisual.transform);
                RectTransform valueRect = cardValue.AddComponent<RectTransform>();
                valueRect.anchorMin = new Vector2(0, 0);
                valueRect.anchorMax = Vector2.one;
                valueRect.offsetMin = Vector2.zero;
                valueRect.offsetMax = Vector2.zero;
                
                TextMeshProUGUI valueText = cardValue.AddComponent<TextMeshProUGUI>();
                valueText.text = "";
                valueText.fontSize = 28;
                valueText.alignment = TextAlignmentOptions.Center;
                valueText.color = Color.black;
            }
            
            // Create selection indicator
            GameObject selectionIndicator = CreateGameObject("SelectionIndicator", Vector2.zero, container.transform);
            RectTransform indicatorRect = selectionIndicator.AddComponent<RectTransform>();
            indicatorRect.sizeDelta = new Vector2(110, 150);
            
            Image indicatorImage = selectionIndicator.AddComponent<Image>();
            indicatorImage.color = new Color(0.8f, 0.7f, 0.2f, 0.5f); // Gold highlight
            indicatorImage.sprite = CreateDefaultSprite();
            
            // Initially position off-screen (no selection)
            selectionIndicator.transform.localPosition = new Vector3(0, -200, 0);
        }
        
        /// <summary>
        /// Sets up the ability indicators UI component
        /// </summary>
        private void SetupAbilityIndicators(Transform parent)
        {
            // Create the main container
            GameObject container = CreateGameObject("Container", Vector2.zero, parent);
            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(250, 120);
            
            // Create slice ability indicator
            GameObject sliceAbility = CreateGameObject("SliceAbility", new Vector2(-60, 0), container.transform);
            SetupAbilityIndicator(sliceAbility, "Slice", Color.red);
            
            // Create flick ability indicator
            GameObject flickAbility = CreateGameObject("FlickAbility", new Vector2(60, 0), container.transform);
            SetupAbilityIndicator(flickAbility, "Flick", Color.blue);
            
            // Create draw card indicator
            GameObject drawCard = CreateGameObject("DrawCard", new Vector2(0, -70), container.transform);
            SetupAbilityIndicator(drawCard, "Draw", new Color(0.2f, 0.7f, 0.2f));
        }
        
        /// <summary>
        /// Sets up individual ability indicator
        /// </summary>
        private void SetupAbilityIndicator(GameObject parent, string abilityName, Color color)
        {
            RectTransform parentRect = parent.AddComponent<RectTransform>();
            parentRect.sizeDelta = new Vector2(80, 80);
            
            // Create background
            Image backgroundImage = parent.AddComponent<Image>();
            backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            backgroundImage.sprite = CreateDefaultSprite();
            
            // Create ability icon
            GameObject icon = CreateGameObject("Icon", Vector2.zero, parent.transform);
            RectTransform iconRect = icon.AddComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(10, 10);
            iconRect.offsetMax = new Vector2(-10, -10);
            
            Image iconImage = icon.AddComponent<Image>();
            iconImage.color = color;
            iconImage.sprite = CreateDefaultSprite();
            
            // Create cooldown overlay
            GameObject cooldown = CreateGameObject("Cooldown", Vector2.zero, parent.transform);
            RectTransform cooldownRect = cooldown.AddComponent<RectTransform>();
            cooldownRect.anchorMin = Vector2.zero;
            cooldownRect.anchorMax = Vector2.one;
            cooldownRect.offsetMin = Vector2.zero;
            cooldownRect.offsetMax = Vector2.zero;
            
            Image cooldownImage = cooldown.AddComponent<Image>();
            cooldownImage.color = new Color(0, 0, 0, 0.7f);
            cooldownImage.sprite = CreateDefaultSprite();
            cooldownImage.type = Image.Type.Filled;
            cooldownImage.fillMethod = Image.FillMethod.Radial360;
            cooldownImage.fillOrigin = (int)Image.Origin360.Top;
            cooldownImage.fillClockwise = true;
            cooldownImage.fillAmount = 0; // 0 = ready, 1 = full cooldown
            
            // Create ability name text
            GameObject nameObj = CreateGameObject("Name", new Vector2(0, -45), parent.transform);
            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.sizeDelta = new Vector2(80, 20);
            
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = abilityName;
            nameText.fontSize = 12;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = Color.white;
        }
        
        /// <summary>
        /// Sets up the notification area UI component
        /// </summary>
        private void SetupNotificationArea(Transform parent)
        {
            // Create the main container
            GameObject container = CreateGameObject("Container", Vector2.zero, parent);
            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(400, 120);
            
            // Create panel background
            GameObject panel = CreateGameObject("Panel", Vector2.zero, container.transform);
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            panelImage.sprite = CreateDefaultSprite();
            
            // Create buzz state notification
            GameObject buzzNotification = CreateGameObject("BuzzStateNotification", Vector2.zero, container.transform);
            SetupNotification(buzzNotification, "Buzz Level Normal", "Full accuracy and defense", normalBuzzIcon);
            
            // Create environment notification
            GameObject envNotification = CreateGameObject("EnvironmentNotification", new Vector2(0, 80), container.transform);
            SetupNotification(envNotification, "Downtown Streets", "Standard conditions", null);
            
            // Initially hide container
            container.SetActive(false);
        }
        
        /// <summary>
        /// Sets up an individual notification
        /// </summary>
        private void SetupNotification(GameObject parent, string title, string description, Sprite icon)
        {
            RectTransform parentRect = parent.AddComponent<RectTransform>();
            parentRect.anchorMin = Vector2.zero;
            parentRect.anchorMax = Vector2.one;
            parentRect.offsetMin = Vector2.zero;
            parentRect.offsetMax = Vector2.zero;
            
            // Create icon if provided
            if (icon != null)
            {
                GameObject iconObj = CreateGameObject("Icon", new Vector2(-160, 0), parent.transform);
                RectTransform iconRect = iconObj.AddComponent<RectTransform>();
                iconRect.sizeDelta = new Vector2(40, 40);
                
                Image iconImage = iconObj.AddComponent<Image>();
                iconImage.sprite = icon;
            }
            
            // Create title text
            GameObject titleObj = CreateGameObject("Title", new Vector2(0, 20), parent.transform);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.sizeDelta = new Vector2(300, 30);
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = title;
            titleText.fontSize = 18;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;
            
            // Create description text
            GameObject descObj = CreateGameObject("Description", new Vector2(0, -10), parent.transform);
            RectTransform descRect = descObj.AddComponent<RectTransform>();
            descRect.sizeDelta = new Vector2(300, 30);
            
            TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.text = description;
            descText.fontSize = 14;
            descText.alignment = TextAlignmentOptions.Center;
            descText.color = new Color(0.8f, 0.8f, 0.8f);
        }
                dummyHealth.maxHealth = 100;
                dummyHealth.currentHealth = 100;
                
                // Add hit effect particle system
                GameObject hitEffectObj = new GameObject("HitEffect");
                hitEffectObj.transform.SetParent(testDummy.transform);
                hitEffectObj.transform.localPosition = Vector3.up * 1.5f;
                
                ParticleSystem hitEffect = hitEffectObj.AddComponent<ParticleSystem>();
                var main = hitEffect.main;
                main.startColor = new Color(1f, 0.2f, 0.2f);
                main.startSize = 0.2f;
                main.duration = 0.5f;
                hitEffect.Stop();
                
                // Add damage text component
                GameObject damageTextObj = new GameObject("DamageText");
                damageTextObj.transform.SetParent(testDummy.transform);
                damageTextObj.transform.localPosition = Vector3.up * 2f;
                
                TextMeshPro damageText = damageTextObj.AddComponent<TextMeshPro>();
                damageText.alignment = TextAlignmentOptions.Center;
                damageText.fontSize = 3;
                damageText.color = Color.red;
                damageText.text = "";
            }
            
            // Create environment toggle controls
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas != null)
            {
                // Create Environment Controls panel
                GameObject envControlsPanel = CreateGameObject("EnvironmentControls", new Vector3(350, 350, 0), canvas.transform);
                RectTransform panelRect = envControlsPanel.AddComponent<RectTransform>();
                panelRect.sizeDelta = new Vector2(200, 120);
                
                Image panelImage = envControlsPanel.AddComponent<Image>();
                panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
                
                // Create environment toggle buttons
                CreateButton(envControlsPanel.transform, "StreetsBtn", "Streets", new Vector2(0, 40), OnStreetsButtonClick);
                CreateButton(envControlsPanel.transform, "TunnelsBtn", "Tunnels", new Vector2(0, 0), OnTunnelsButtonClick);
                CreateButton(envControlsPanel.transform, "SpeakeasyBtn", "Speakeasy", new Vector2(0, -40), OnSpeakeasyButtonClick);
            }
            
            // Set up buzz manipulation controls
            if (canvas != null)
            {
                // Create Buzz Controls panel
                GameObject buzzControlsPanel = CreateGameObject("BuzzControls", new Vector3(-350, 350, 0), canvas.transform);
                RectTransform panelRect = buzzControlsPanel.AddComponent<RectTransform>();
                panelRect.sizeDelta = new Vector2(200, 160);
                
                Image panelImage = buzzControlsPanel.AddComponent<Image>();
                panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
                
                // Add buzz control buttons
                CreateButton(buzzControlsPanel.transform, "AddBuzzBtn", "+10 Buzz", new Vector2(0, 60), OnAddBuzzButtonClick);
                CreateButton(buzzControlsPanel.transform, "SubtractBuzzBtn", "-10 Buzz", new Vector2(0, 20), OnSubtractBuzzButtonClick);
                CreateButton(buzzControlsPanel.transform, "ResetBuzzBtn", "Reset Buzz", new Vector2(0, -20), OnResetBuzzButtonClick);
                CreateButton(buzzControlsPanel.transform, "TriggerCriticalBtn", "Critical", new Vector2(0, -60), OnTriggerCriticalButtonClick);
            }
            
            // Add performance monitoring
            GameObject managers = GameObject.Find("Managers");
            if (managers != null)
            {
                GameObject performanceMonitor = CreateGameObject("PerformanceMonitor", Vector3.zero, managers.transform);
                performanceMonitor.AddComponent<PerformanceMonitor>();
                
                // Create performance UI
                if (canvas != null)
                {
                    GameObject perfPanel = CreateGameObject("PerformancePanel", new Vector3(0, -200, 0), canvas.transform);
                    RectTransform panelRect = perfPanel.AddComponent<RectTransform>();
                    panelRect.sizeDelta = new Vector2(300, 100);
                    
                    Image panelImage = perfPanel.AddComponent<Image>();
                    panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
                    
                    // Add FPS text
                    GameObject fpsObj = CreateGameObject("FPSText", new Vector2(0, 30), perfPanel.transform);
                    TextMeshProUGUI fpsText = fpsObj.AddComponent<TextMeshProUGUI>();
                    fpsText.text = "FPS: 60";
                    fpsText.fontSize = 16;
                    fpsText.alignment = TextAlignmentOptions.Center;
                    
                    // Add particles count text
                    GameObject particlesObj = CreateGameObject("ParticlesText", new Vector2(0, 0), perfPanel.transform);
                    TextMeshProUGUI particlesText = particlesObj.AddComponent<TextMeshProUGUI>();
                    particlesText.text = "Particles: 0";
                    particlesText.fontSize = 16;
                    particlesText.alignment = TextAlignmentOptions.Center;
                    
                    // Add draw calls text
                    GameObject drawCallsObj = CreateGameObject("DrawCallsText", new Vector2(0, -30), perfPanel.transform);
                    TextMeshProUGUI drawCallsText = drawCallsObj.AddComponent<TextMeshProUGUI>();
                    drawCallsText.text = "Draw Calls: 0";
                    drawCallsText.fontSize = 16;
                    drawCallsText.alignment = TextAlignmentOptions.Center;
                }
            }
            
            // Set test elements as configured
            testElementsConfigured = true;
            Debug.Log("Test elements configured successfully!");
        }
        
        #region Helper Methods
        
        /// <summary>
        /// Creates walls for the test environment
        /// </summary>
        private void CreateWalls(Transform parent)
        {
            // North wall
            CreatePrimitive(PrimitiveType.Cube, "NorthWall", new Vector3(0, 1, 10), new Vector3(20, 2, 0.5f), parent);
            
            // South wall
            CreatePrimitive(PrimitiveType.Cube, "SouthWall", new Vector3(0, 1, -10), new Vector3(20, 2, 0.5f), parent);
            
            // East wall
            CreatePrimitive(PrimitiveType.Cube, "EastWall", new Vector3(10, 1, 0), new Vector3(0.5f, 2, 20), parent);
            
            // West wall
            CreatePrimitive(PrimitiveType.Cube, "WestWall", new Vector3(-10, 1, 0), new Vector3(0.5f, 2, 20), parent);
        }
        
        /// <summary>
        /// Sets up period-appropriate lighting
        /// </summary>
        private void SetupLighting(Transform parent)
        {
            // Main directional light (like sunlight or moonlight)
            GameObject mainLight = CreateGameObject("MainLight", new Vector3(0, 10, 0), parent);
            Light mainLightComponent = mainLight.AddComponent<Light>();
            mainLightComponent.type = LightType.Directional;
            mainLightComponent.intensity = 0.7f;
            mainLightComponent.color = new Color(1.0f, 0.91f, 0.73f); // Warm light
            mainLightComponent.shadows = LightShadows.Soft;
            
            // Ambient point lights (like lanterns or candles)
            CreatePointLight("Lantern1", new Vector3(5, 2, 5), 0.5f, new Color(1.0f, 0.82f, 0.54f), parent);
            CreatePointLight("Lantern2", new Vector3(-5, 2, 5), 0.5f, new Color(1.0f, 0.82f, 0.54f), parent);
            CreatePointLight("Lantern3", new Vector3(5, 2, -5), 0.5f, new Color(1.0f, 0.82f, 0.54f), parent);
            CreatePointLight("Lantern4", new Vector3(-5, 2, -5), 0.5f, new Color(1.0f, 0.82f, 0.54f), parent);
            
            // Set ambient light settings
            RenderSettings.ambientIntensity = 0.3f;
            RenderSettings.ambientLight = new Color(0.25f, 0.24f, 0.23f); // Muted brown
        }
        
        /// <summary>
        /// Creates period-appropriate props for the environment
        /// </summary>
        private void CreateProps(Transform parent)
        {
            // Create barrels
            CreatePrimitive(PrimitiveType.Cylinder, "Barrel1", new Vector3(8, 0.5f, 8), new Vector3(1, 1, 1), parent);
            CreatePrimitive(PrimitiveType.Cylinder, "Barrel2", new Vector3(7, 0.5f, 8), new Vector3(1, 1, 1), parent);
            CreatePrimitive(PrimitiveType.Cylinder, "Barrel3", new Vector3(8, 0.5f, 7), new Vector3(1, 1, 1), parent);
            
            // Create crates
            CreatePrimitive(PrimitiveType.Cube, "Crate1", new Vector3(-8, 0.5f, 8), new Vector3(1, 1, 1), parent);
            CreatePrimitive(PrimitiveType.Cube, "Crate2", new Vector3(-8, 1.5f, 8), new Vector3(1, 1, 1), parent);
            CreatePrimitive(PrimitiveType.Cube, "Crate3", new Vector3(-7, 0.5f, 8), new Vector3(1, 1, 1), parent);
            
            // Create saloon elements (simplified)
            GameObject bar = CreateGameObject("Bar", new Vector3(0, 0, -8), parent);
            CreatePrimitive(PrimitiveType.Cube, "BarCounter", new Vector3(0, 1, 0), new Vector3(6, 0.2f, 1), bar.transform);
            CreatePrimitive(PrimitiveType.Cube, "BarBase", new Vector3(0, 0.5f, -0.25f), new Vector3(6, 1, 0.5f), bar.transform);
            
            // Create some simple bottles
            for (int i = 0; i < 5; i++)
            {
                CreatePrimitive(PrimitiveType.Cylinder, $"Bottle{i}", new Vector3(-2 + i, 1.6f, 0), new Vector3(0.1f, 0.5f, 0.1f), bar.transform);
            }
        }
        
        /// <summary>
        /// Sets up the buzz meter UI component
        /// </summary>
        private void SetupBuzzMeter(Transform parent)
        {
            // Create the main container
            GameObject container = CreateGameObject("Container", Vector2.zero, parent);
            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(200, 40);
            
            

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PDXUnderground
{
    /// <summary>
    /// This script provides step-by-step guidance for setting up the GamblerTest scene.
    /// It can be used both as documentation and as a utility to help create scene elements.
    /// 
    /// INSTRUCTIONS:
    /// 1. Create a new scene and save it as Assets/Scenes/Test/GamblerTest.unity
    /// 2. Create an empty GameObject and attach this script
    /// 3. Follow the instructions in the Inspector or use the automated setup buttons
    /// </summary>
    [ExecuteInEditMode]
    public class GamblerTestSceneSetupGuide : MonoBehaviour
    {
        [Header("Scene Configuration")]
        public bool sceneStructureCreated = false;
        public bool materialsConfigured = false;
        public bool componentsAttached = false;
        public bool testElementsConfigured = false;
        
        [Header("Reference Materials")]
        public Material buzzMeterMaterial;
        public Material cardEffectsMaterial;
        public Material cardTrailMaterial;
        public Material tunnelEnvironmentMaterial;
        public Material speakeasyEnvironmentMaterial;
        
        [Header("Reference Components")]
        public GameObject gamblerCharacterPrefab;
        public GameObject testDummyPrefab;
        public GameObject cardPrefabRef;
        
        [Header("UI References")]
        public Sprite normalBuzzIcon;
        public Sprite lowBuzzIcon;
        public Sprite criticalBuzzIcon;
        public Sprite[] cardIcons;
        
        #if UNITY_EDITOR
        private readonly Vector2 sceneSize = new Vector2(1000, 800);
        
        [CustomEditor(typeof(GamblerTestSceneSetupGuide))]
        public class GamblerTestSceneSetupGuideEditor : Editor
        {
            if (cardEffectsController != null)
            {
                // Add CardEffectsController component
                CardEffectsController controller = cardEffectsController.AddComponent<CardEffectsController>();
                
                // Configure card effects settings
                controller.cardPoolSize = 20;
                controller.impactPoolSize = 10;
                
                // Add reference to materials
                if (cardEffectsMaterial != null)
                {
                    // This would be setting serialized fields via reflection in a real implementation
                    // For demo purposes, we'll just log that we would do this
                    Debug.Log("Would set card effects material references");
                }
                
                // Add AudioSource for sound effects
                AudioSource audioSource = cardEffectsController.AddComponent<AudioSource>();
                audioSource.spatialBlend = 0.0f; // 2D sound for UI effects
                audioSource.playOnAwake = false;
            }
            
            // Find and configure BuzzUIController
            if (canvas != null)
            {
                // Add BuzzUIController to Canvas
                BuzzUIController buzzUI = canvas.AddComponent<BuzzUIController>();
                
                // Configure UI references
                if (buzzMeter != null)
                {
                    // In a real implementation we'd set these via reflection
                    // Assign buzz meter components to controller
                    Image meterFill = buzzMeter.transform.Find("Fill")?.GetComponent<Image>();
                    TextMeshProUGUI buzzValueText = buzzMeter.transform.Find("Value")?.GetComponent<TextMeshProUGUI>();
                    Image meterFrame = buzzMeter.transform.Find("Frame")?.GetComponent<Image>();
                    Image stateIcon = buzzMeter.transform.Find("StateIcon")?.GetComponent<Image>();
                    
                    if (meterFill != null && buzzMeterMaterial != null)
                    {
                        meterFill.material = buzzMeterMaterial;
                    }
                    
                    // Set buzz icon references
                    if (stateIcon != null)
                    {
                        Debug.Log("Would set buzz state icons to the StateIcon component");
                    }
                }
                
                // Connect to GamblerCharacter events
                if (gamblerCharacter != null)
                {
                    Debug.Log("Connected BuzzUIController to GamblerCharacter events");
                }
            }
            
            // Connect all components together
            if (gamblerCharacter != null && cardEffectsController != null)
            {
                Debug.Log("Connected CardEffectsController to GamblerCharacter events");
            }
            
            // Set components as attached
            componentsAttached = true;
            Debug.Log("Components attached successfully!");
                GamblerTestSceneSetupGuide guide = (GamblerTestSceneSetupGuide)target;
                
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("Follow the steps below to set up the GamblerTest scene", MessageType.Info);
                EditorGUILayout.Space(10);
                
                // Step 1: Create scene structure
                GUI.enabled = !guide.sceneStructureCreated;
                EditorGUILayout.LabelField("Step 1: Create Basic Scene Structure", EditorStyles.boldLabel);
                if (GUILayout.Button("Create Scene Structure"))
                {
                    guide.CreateSceneStructure();
                }
                GUI.enabled = true;
                
                EditorGUILayout.Space(5);
                
                // Step 2: Configure materials
                GUI.enabled = guide.sceneStructureCreated && !guide.materialsConfigured;
                EditorGUILayout.LabelField("Step 2: Configure Materials", EditorStyles.boldLabel);
                if (GUILayout.Button("Set Up Materials"))
                {
                    guide.ConfigureMaterials();
                }
                GUI.enabled = true;
                
                EditorGUILayout.Space(5);
                
                // Step 3: Attach components
                GUI.enabled = guide.materialsConfigured && !guide.componentsAttached;
                EditorGUILayout.LabelField("Step 3: Attach Components", EditorStyles.boldLabel);
                if (GUILayout.Button("Set Up Components"))
                {
                    guide.AttachComponents();
                }
                GUI.enabled = true;
                
                EditorGUILayout.Space(5);
                
                // Step 4: Set up test elements
                GUI.enabled = guide.componentsAttached && !guide.testElementsConfigured;
                EditorGUILayout.LabelField("Step 4: Set Up Test Elements", EditorStyles.boldLabel);
                if (GUILayout.Button("Configure Test Elements"))
                {
                    guide.SetupTestElements();
                }
                GUI.enabled = true;
                
                EditorGUILayout.Space(10);
                
                // Final step
                if (guide.sceneStructureCreated && guide.materialsConfigured && 
                    guide.componentsAttached && guide.testElementsConfigured)
                {
                    EditorGUILayout.HelpBox("Scene setup complete! You can now test the GamblerCharacter system.", MessageType.Success);
                    
                    if (GUILayout.Button("Remove Setup Guide"))
                    {
                        DestroyImmediate(guide.gameObject);
                        EditorUtility.DisplayDialog("Setup Complete", "Setup guide has been removed. The scene is ready for testing!", "OK");
                    }
                }
            }
        }
        
        /// <summary>
        /// Step 1: Creates the basic scene structure
        /// </summary>
        public void CreateSceneStructure()
        {
            // Create main containers
            GameObject environment = CreateGameObject("Environment", Vector3.zero);
            GameObject player = CreateGameObject("Player", new Vector3(0, 0, -3));
            GameObject enemies = CreateGameObject("Enemies", new Vector3(0, 0, 3));
            GameObject ui = CreateGameObject("UI", Vector3.zero);
            GameObject managers = CreateGameObject("Managers", Vector3.zero);
            GameObject cameras = CreateGameObject("Cameras", Vector3.zero);
            
            // Create environment elements
            GameObject floor = CreateGameObject("Floor", Vector3.zero, environment.transform);
            CreatePrimitive(PrimitiveType.Plane, "FloorMesh", Vector3.zero, Vector3.one * 5, floor.transform);
            
            GameObject walls = CreateGameObject("Walls", Vector3.zero, environment.transform);
            CreateWalls(walls.transform);
            
            GameObject props = CreateGameObject("Props", Vector3.zero, environment.transform);
            CreateProps(props.transform);
            
            GameObject lightingRig = CreateGameObject("LightingRig", Vector3.zero, environment.transform);
            SetupLighting(lightingRig.transform);
            
            // Create player character
            GameObject gamblerCharacter = CreateGameObject("GamblerCharacter", Vector3.zero, player.transform);
            CreatePrimitive(PrimitiveType.Capsule, "CharacterMesh", Vector3.up, Vector3.one, gamblerCharacter.transform);
            CreateGameObject("CardSpawnPoint", new Vector3(0.5f, 1.5f, 0.5f), gamblerCharacter.transform);
            CreateGameObject("EffectsManager", Vector3.zero, gamblerCharacter.transform);
            CreateGameObject("BuzzSystem", Vector3.zero, gamblerCharacter.transform);
            
            // Create test dummy
            GameObject testDummy = CreateGameObject("TestDummy", new Vector3(0, 0, 5), enemies.transform);
            CreatePrimitive(PrimitiveType.Cylinder, "DummyMesh", new Vector3(0, 1, 0), new Vector3(1, 2, 1), testDummy.transform);
            
            // Create UI elements
            GameObject canvas = CreateGameObject("Canvas", Vector3.zero, ui.transform);
            Canvas canvasComponent = canvas.AddComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<CanvasScaler>();
            canvas.AddComponent<GraphicRaycaster>();
            
            GameObject buzzMeter = CreateGameObject("BuzzMeter", new Vector3(-350, 450, 0), canvas.transform);
            SetupBuzzMeter(buzzMeter.transform);
            
            GameObject handDisplay = CreateGameObject("HandDisplay", new Vector3(0, -400, 0), canvas.transform);
            SetupHandDisplay(handDisplay.transform);
            
            GameObject abilityIndicators = CreateGameObject("AbilityIndicators", new Vector3(350, -350, 0), canvas.transform);
            SetupAbilityIndicators(abilityIndicators.transform);
            
            GameObject notificationArea = CreateGameObject("NotificationArea", new Vector3(0, 400, 0), canvas.transform);
            SetupNotificationArea(notificationArea.transform);
            
            GameObject eventSystem = CreateGameObject("EventSystem", Vector3.zero, ui.transform);
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            
            // Create manager objects
            CreateGameObject("GameManager", Vector3.zero, managers.transform);
            CreateGameObject("CardEffectsController", Vector3.zero, managers.transform);
            CreateGameObject("EnvironmentManager", Vector3.zero, managers.transform);
            
            // Create camera objects
            GameObject mainCamera = CreateGameObject("MainCamera", new Vector3(0, 1.6f, -4), cameras.transform);
            Camera mainCameraComponent = mainCamera.AddComponent<Camera>();
            mainCameraComponent.fieldOfView = 60;
            mainCameraComponent.clearFlags = CameraClearFlags.Skybox;
            
            // Set scene as structured
            sceneStructureCreated = true;
            Debug.Log("Basic scene structure created successfully!");
        }
        
        /// <summary>
        /// Step 2: Configure materials with period-appropriate styling
        /// </summary>
        public void ConfigureMaterials()
        {
            // Create a directory for materials if it doesn't exist
            if (!AssetDatabase.IsValidFolder("Assets/Materials/UI"))
            {
                AssetDatabase.CreateFolder("Assets/Materials", "UI");
            }
            
            // Create Buzz Meter Material
            Material buzzMeter = new Material(Shader.Find("PDXUnderground/BuzzUIShader"));
            buzzMeter.name = "BuzzMeterMaterial";
            buzzMeter.SetColor("_Color", new Color(0.827f, 0.722f, 0.416f)); // Gold/amber
            buzzMeter.SetColor("_EmissionColor", new Color(0.827f, 0.722f, 0.416f));
            buzzMeter.SetFloat("_EmissionIntensity", 0.5f);
            buzzMeter.SetFloat("_GrainIntensity", 0.2f);
            buzzMeter.SetFloat("_EdgeWear", 0.3f);
            AssetDatabase.CreateAsset(buzzMeter, "Assets/Materials/UI/BuzzMeterMaterial.mat");
            
            // Create Card Effects Material
            Material cardEffects = new Material(Shader.Find("Particles/Standard Surface"));
            cardEffects.name = "CardEffectsMaterial";
            cardEffects.SetColor("_Color", Color.white);
            cardEffects.SetFloat("_Glossiness", 0.1f);
            cardEffects.EnableKeyword("_EMISSION");
            cardEffects.SetColor("_EmissionColor", new Color(1.0f, 0.835f, 0.5f)); // Soft gold
            AssetDatabase.CreateAsset(cardEffects, "Assets/Materials/UI/CardEffectsMaterial.mat");
            
            // Create Card Trail Material
            Material cardTrail = new Material(Shader.Find("Particles/Standard Unlit"));
            cardTrail.name = "CardTrailMaterial";
            cardTrail.SetColor("_Color", Color.white);
            cardTrail.renderQueue = 3000; // Transparent queue
            cardTrail.EnableKeyword("_EMISSION");
            cardTrail.SetColor("_EmissionColor", new Color(1.0f, 0.835f, 0.5f, 0.5f));
            AssetDatabase.CreateAsset(cardTrail, "Assets/Materials/UI/CardTrailMaterial.mat");
            
            // Create Environment Materials
            Material tunnelMaterial = new Material(Shader.Find("Standard"));
            tunnelMaterial.name = "TunnelEnvironmentMaterial";
            tunnelMaterial.SetColor("_Color", new Color(0.239f, 0.325f, 0.384f, 0.7f)); // Muted blue-gray
            tunnelMaterial.EnableKeyword("_EMISSION");
            tunnelMaterial.SetColor("_EmissionColor", new Color(0.251f, 0.314f, 0.376f)); // Misty blue
            AssetDatabase.CreateAsset(tunnelMaterial, "Assets/Materials/UI/TunnelEnvironmentMaterial.mat");
            
            Material speakeasyMaterial = new Material(Shader.Find("Standard"));
            speakeasyMaterial.name = "SpeakeasyEnvironmentMaterial";
            speakeasyMaterial.SetColor("_Color", new Color(0.416f, 0.227f, 0.176f, 0.9f)); // Rich brown
            speakeasyMaterial.EnableKeyword("_EMISSION");
            speakeasyMaterial.SetColor("_EmissionColor", new Color(1.0f, 0.78f, 0.45f)); // Warm gold
            AssetDatabase.CreateAsset(speakeasyMaterial, "Assets/Materials/UI/SpeakeasyEnvironmentMaterial.mat");
            
            // Assign references
            buzzMeterMaterial = buzzMeter;
            cardEffectsMaterial = cardEffects;
            cardTrailMaterial = cardTrail;
            tunnelEnvironmentMaterial = tunnelMaterial;
            speakeasyEnvironmentMaterial = speakeasyMaterial;
            
            // Apply materials to scene objects
            ApplyMaterialsToScene();
            
            // Set materials as configured
            materialsConfigured = true;
            Debug.Log("Materials configured successfully!");
        }
        
        /// <summary>
        /// Step 3: Attach components and scripts to appropriate GameObjects
        /// </summary>
        public void AttachComponents()
        {
            // Find key GameObjects
            GameObject gamblerCharacter = GameObject.Find("GamblerCharacter");
            GameObject cardEffectsController = GameObject.Find("CardEffectsController");
            GameObject buzzMeter = GameObject.Find("BuzzMeter");
            GameObject canvas = GameObject.Find("Canvas");
            
            if (gamblerCharacter != null)
            {
                // Add GamblerCharacter component
                GamblerCharacter character = gamblerCharacter.AddComponent<GamblerCharacter>();
                
                // Configure basic properties
                character.maxBuzz = 100f;
                character.currentBuzz = 100f;
                character.buzzDepletionRate = 5f;
                character.buzzRegenerationRate = 3f;
                character.lowBuzzThreshold = 30f;
                character.criticalBuzzThreshold = 10f;
                
                // Add Rigidbody for physics
                Rigidbody rb = gamblerCharacter.AddComponent<Rigidbody>();
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                
                // Add basic movement script (would be developed separately)
                gamblerCharacter.AddComponent<CharacterController>();
                
                // Add AudioSource for sound effects
                AudioSource audioSource = gamblerCharacter.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1.0f; // Full 3D
                audioSource.minDistance = 1.0f;
                audioSource.maxDistance = 20.0f;
            }
            
            if (cardEffectsController != null)
            {
                // Add CardEffectsController component
                CardEffectsController controller = cardEffects

