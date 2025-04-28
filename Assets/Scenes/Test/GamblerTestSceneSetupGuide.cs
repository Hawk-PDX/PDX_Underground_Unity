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
        #region Fields

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

        #endregion

        #region Public Methods

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
            
            // Create UI canvas
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
                CardEffectsController controller = cardEffectsController.AddComponent<CardEffectsController>();
                
                // Configure card effects properties
                controller.cardPoolSize = 20;
                controller.impactPoolSize = 10;
                
                // Set materials
                if (cardEffectsMaterial != null)
                {
                    controller.effectsMaterial = cardEffectsMaterial;
                    controller.trailMaterial = cardTrailMaterial;
                }
                
                // Add AudioSource for card effects
                AudioSource audioSource = cardEffectsController.AddComponent<AudioSource>();
                audioSource.spatialBlend = 0.0f; // 2D sound
                audioSource.playOnAwake = false;
            }
            
            if (buzzMeter != null && canvas != null)
            {
                // Add BuzzUIController to canvas
                BuzzUIController buzzUI = canvas.AddComponent<BuzzUIController>();
                
                // Configure UI elements
                Image meterFill = buzzMeter.transform.Find("Fill")?.GetComponent<Image>();
                TextMeshProUGUI buzzValueText = buzzMeter.transform.Find("Value")?.GetComponent<TextMeshProUGUI>();
                Image meterFrame = buzzMeter.transform.Find("Frame")?.GetComponent<Image>();
                Image stateIcon = buzzMeter.transform.Find("StateIcon")?.GetComponent<Image>();
                
                // Apply buzz meter material
                if (meterFill != null && buzzMeterMaterial != null)
                {
                    meterFill.material = buzzMeterMaterial;
                }
                
                // Set up state icons
                if (stateIcon != null)
                {
                    buzzUI.normalStateIcon = normalBuzzIcon;
                    buzzUI.lowStateIcon = lowBuzzIcon;
                    buzzUI.criticalStateIcon = criticalBuzzIcon;
                }
            }
            
            // Set up test dummy
            SetupTestDummy();
            
            componentsAttached = true;
            Debug.Log("Components attached successfully!");
        }
        
        /// <summary>
        /// Step 4: Set up test elements and configure game settings
        /// </summary>
        public void SetupTestElements()
        {
            if (!componentsAttached)
            {
                Debug.LogError("Please attach components before setting up test elements");
                return;
            }
            
            // Set up test dummy if not already done
            SetupTestDummy();
            
            // Set up test environment
            GameObject environment = GameObject.Find("Environment");
            if (environment != null)
            {
                GameEnvironmentController envController = environment.AddComponent<GameEnvironmentController>();
                // Configure environment settings as needed
            }
            
            // Set up game manager
            GameObject manager = GameObject.Find("GameManager");
            if (manager != null)
            {
                GameManager gameManager = manager.AddComponent<GameManager>();
                // Configure game manager settings as needed
            }
            
            testElementsConfigured = true;
            Debug.Log("Test elements configured successfully!");
        }
        
        #endregion
        
        #region Private Helper Methods
        
        private void SetupTestDummy()
        {
            GameObject dummyObj = GameObject.Find("TestDummy");
            if (dummyObj != null)
            {
                TestDummyHealth dummy = dummyObj.AddComponent<TestDummyHealth>();
                dummy.maxHealth = 100f;
                dummy.currentHealth = 100f;
            }
        }
        
        private GameObject CreateGameObject(string name, Vector3 position, Transform parent = null)
        {
            GameObject obj = new GameObject(name);
            obj.transform.position = position;
            if (parent != null)
            {
                obj.transform.parent = parent;
            }
            return obj;
        }
        
        private void CreateWalls(Transform parent)
        {
            // Create basic wall structure
            float wallHeight = 3f;
            float roomWidth = 10f;
            float roomLength = 10f;
            
            // North wall
            CreatePrimitive(PrimitiveType.Cube, "NorthWall", new Vector3(0, wallHeight/2, roomLength/2), 
                new Vector3(roomWidth, wallHeight, 0.2f), parent);
            
            // South wall
            CreatePrimitive(PrimitiveType.Cube, "SouthWall", new Vector3(0, wallHeight/2, -roomLength/2), 
                new Vector3(roomWidth, wallHeight, 0.2f), parent);
            
            // East wall
            CreatePrimitive(PrimitiveType.Cube, "EastWall", new Vector3(roomWidth/2, wallHeight/2, 0), 
                new Vector3(0.2f, wallHeight, roomLength), parent);
            
            // West wall
            CreatePrimitive(PrimitiveType.Cube, "WestWall", new Vector3(-roomWidth/2, wallHeight/2, 0), 
                new Vector3(0.2f, wallHeight, roomLength), parent);
        }
        
        private void CreateProps(Transform parent)
        {
            // Add basic props for testing
            CreatePrimitive(PrimitiveType.Cube, "Table", new Vector3(2, 0.5f, 2), 
                new Vector3(1.5f, 1, 1), parent);
            
            CreatePrimitive(PrimitiveType.Cylinder, "Pillar1", new Vector3(-3, 1.5f, -3), 
                new Vector3(0.5f, 3, 0.5f), parent);
            
            CreatePrimitive(PrimitiveType.Cylinder, "Pillar2", new Vector3(3, 1.5f, -3), 
                new Vector3(0.5f, 3, 0.5f), parent);
        }
        
        private void SetupLighting(Transform parent)
        {
            // Create main light
            GameObject mainLight = CreateGameObject("MainLight", new Vector3(0, 5, 0), parent);
            Light lightComponent = mainLight.AddComponent<Light>();
            lightComponent.type = LightType.Point;
            lightComponent.intensity = 1.5f;
            lightComponent.range = 15f;
            lightComponent.color = new Color(1f, 0.95f, 0.8f); // Warm light
            
            // Create ambient light
            GameObject ambientLight = CreateGameObject("AmbientLight", new Vector3(0, 3, 0), parent);
            Light ambientComponent = ambientLight.AddComponent<Light>();
            ambientComponent.type = LightType.Point;
            ambientComponent.intensity = 0.5f;
            ambientComponent.range = 20f;
            ambientComponent.color = new Color(0.7f, 0.7f, 1f); // Cool ambient
        }
        
        private void CreatePrimitive(PrimitiveType type, string name, Vector3 position, Vector3 scale, Transform parent)
        {
            GameObject primitive = GameObject.CreatePrimitive(type);
            primitive.name = name;
            primitive.transform.parent = parent;
            primitive.transform.localPosition = position;
            primitive.transform.localScale = scale;
        }
        
        private void ApplyMaterialsToScene()
        {
            // Apply environment materials
            foreach (MeshRenderer renderer in GameObject.Find("Walls")?.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.material = tunnelEnvironmentMaterial;
            }
            
            foreach (MeshRenderer renderer in GameObject.Find("Props")?.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.material = speakeasyEnvironmentMaterial;
            }
        }
        
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
        private void SetupHandDisplay(Transform parent)
        {
            // Create main container
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
            panelImage.sprite = CreateDefaultSprite();
            
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
                
                // Create card value text
                GameObject cardValue = CreateGameObject("CardValue", new Vector2(0, 0), cardVisual.transform);
                RectTransform valueRect = cardValue.AddComponent<RectTransform>();
                valueRect.anchorMin = Vector2.zero;
                valueRect.anchorMax = Vector2.one;
                valueRect.offsetMin = Vector2.zero;
                valueRect.offsetMax = Vector2.zero;
                
                TextMeshProUGUI valueText = cardValue.AddComponent<TextMeshProUGUI>();
                valueText.text = "";
                valueText.fontSize = 28;
                valueText.alignment = TextAlignmentOptions.Center;
                valueText.color = Color.black;
                
                // Create energy cost indicator
                GameObject costObj = CreateGameObject("EnergyCost", new Vector2(40, -57), cardVisual.transform);
                RectTransform costRect = costObj.AddComponent<RectTransform>();
                costRect.sizeDelta = new Vector2(25, 25);
                
                Image costBg = costObj.AddComponent<Image>();
                costBg.color = new Color(0.1f, 0.6f, 1f, 0.9f);
                costBg.sprite = CreateDefaultSprite();
                
                GameObject costText = CreateGameObject("CostValue", Vector2.zero, costObj.transform);
                RectTransform costTextRect = costText.AddComponent<RectTransform>();
                costTextRect.anchorMin = Vector2.zero;
                costTextRect.anchorMax = Vector2.one;
                costTextRect.offsetMin = Vector2.zero;
                costTextRect.offsetMax = Vector2.zero;
                
                TextMeshProUGUI costValue = costText.AddComponent<TextMeshProUGUI>();
                costValue.text = (i + 1).ToString();
                costValue.fontSize = 16;
                costValue.alignment = TextAlignmentOptions.Center;
                costValue.color = Color.white;
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
            
            // Create notification title
            GameObject titleObj = CreateGameObject("Title", new Vector2(0, 40), container.transform);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.sizeDelta = new Vector2(380, 30);
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Notification Title";
            titleText.fontSize = 18;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;
            
            // Create notification description
            GameObject descObj = CreateGameObject("Description", new Vector2(0, 0), container.transform);
            RectTransform descRect = descObj.AddComponent<RectTransform>();
            descRect.sizeDelta = new Vector2(380, 60);
            
            TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.text = "Notification description with details about the event or status change.";
            descText.fontSize = 14;
            descText.alignment = TextAlignmentOptions.Center;
            descText.color = new Color(0.8f, 0.8f, 0.8f);
            
            // Add animation component
            CanvasGroup canvasGroup = container.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0; // Start hidden
            
            // Initially hide container
            container.SetActive(false);
        }
        
        private Sprite CreateDefaultSprite()
        {
            // Create a default white sprite for UI elements
            Texture2D texture = new Texture2D(2, 2);
            Color[] colors = new Color[4];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.white;
            }
            texture.SetPixels(colors);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
        }
        
        #endregion
        
        #if UNITY_EDITOR
        [CustomEditor(typeof(GamblerTestSceneSetupGuide))]
        public class GamblerTestSceneSetupGuideEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                GamblerTestSceneSetupGuide guide = (GamblerTestSceneSetupGuide)target;
                
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("Follow the steps below to set up the GamblerTest scene", MessageType.Info);
                EditorGUILayout.Space(10);
                
                // Step 1: Create scene structure
                GUI.enabled = !guide.sceneStructureCreated;
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
                
                // Step 4: Configure test elements
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
                        EditorUtility.DisplayDialog("Setup Complete", 
                            "Setup guide has been removed. The scene is ready for testing!", "OK");
                    }
                }
            }
        }
        #endif
    }
}
