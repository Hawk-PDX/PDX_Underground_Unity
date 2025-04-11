using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

namespace PDXUnderground.Editor
{
    /// <summary>
    /// Editor utility for setting up the test scene for the Gambler character
    /// </summary>
    public class TestSceneSetup : MonoBehaviour
    {
        #region Scene Settings
        private static readonly string sceneName = "GamblerTestScene";
        private static readonly string scenePath = "Assets/Scenes/GamblerTestScene.unity";
        
        // Paths
        private static readonly string texturesDir = "Assets/Resources/Textures/UI";
        private static readonly string materialsDir = "Assets/Resources/Materials/UI";
        private static readonly string prefabsDir = "Assets/Resources/Prefabs/UI";
        #endregion
        
        #region Scene Creation Methods
        
        /// <summary>
        /// Creates a new test scene with all required setup
        /// </summary>
        public static void CreateTestScene()
        {
            // Check if we need to save current scene
            if (EditorSceneManager.GetActiveScene().isDirty)
            {
                bool save = EditorUtility.DisplayDialog(
                    "Save Current Scene?",
                    "The current scene has unsaved changes. Would you like to save them before creating the test scene?",
                    "Save", "Don't Save");
                
                if (save)
                {
                    EditorSceneManager.SaveOpenScenes();
                }
            }
            
            // Create new scene
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            // Set up the scene
            SetupSceneObjects();
            
            // Save the scene
            EditorSceneManager.SaveScene(scene, scenePath);
            
            Debug.Log($"Test scene created at: {scenePath}");
        }
        
        /// <summary>
        /// Sets up the scene with all required objects and components
        /// </summary>
        private static void SetupSceneObjects()
        {
            // Create SceneSetup object
            GameObject sceneSetupObj = new GameObject("SceneSetup");
            sceneSetupObj.AddComponent<UnityEditor.MonoScripts.SceneSetup>(); // this is a placeholder, replace with actual SceneSetup component
            
            // Create GameManager object
            GameObject gameManagerObj = new GameObject("GameManager");
            gameManagerObj.AddComponent<UnityEditor.MonoScripts.GameManager>(); // this is a placeholder, replace with actual GameManager component
            
            // Create basic environment
            CreateEnvironment();
            
            // Create player spawn point
            GameObject spawnPointObj = new GameObject("PlayerSpawnPoint");
            spawnPointObj.transform.position = new Vector3(0f, 1f, 0f);
            
            // Create test controller
            GameObject testControllerObj = new GameObject("TestController");
            TestSceneController testController = testControllerObj.AddComponent<PDXUnderground.Test.TestSceneController>();
            
            // Create debug canvas
            GameObject debugCanvasObj = new GameObject("DebugCanvas");
            Canvas debugCanvas = debugCanvasObj.AddComponent<Canvas>();
            debugCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            debugCanvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            debugCanvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // Create test player prefab
            GameObject playerPrefab = CreateTestPlayerPrefab();
            
            // Create UI canvas prefab
            GameObject uiCanvasPrefab = CreateUICanvasPrefab();
            
            // Create debug panel prefab
            GameObject debugPanelPrefab = CreateDebugPanelPrefab();
            
            // Set up references in test controller
            if (testController != null)
            {
                SerializedObject serializedTestController = new SerializedObject(testController);
                
                serializedTestController.FindProperty("playerSpawnPoint").objectReferenceValue = spawnPointObj.transform;
                serializedTestController.FindProperty("debugCanvas").objectReferenceValue = debugCanvas;
                serializedTestController.FindProperty("playerPrefab").objectReferenceValue = playerPrefab;
                serializedTestController.FindProperty("uiCanvasPrefab").objectReferenceValue = uiCanvasPrefab;
                serializedTestController.FindProperty("debugPanelPrefab").objectReferenceValue = debugPanelPrefab;
                
                serializedTestController.ApplyModifiedProperties();
            }
            
            // Set up test cards
            SetupTestCards(testController);
        }
        
        /// <summary>
        /// Creates a basic environment for the test scene
        /// </summary>
        private static void CreateEnvironment()
        {
            // Create directional light
            GameObject lightObj = new GameObject("DirectionalLight");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.color = new Color(1.0f, 0.95f, 0.9f);
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            
            // Create ground plane
            GameObject groundObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
            groundObj.name = "Ground";
            groundObj.transform.position = Vector3.zero;
            groundObj.transform.localScale = new Vector3(5f, 1f, 5f);
            
            // Create basic material for ground
            Material groundMaterial = new Material(Shader.Find("Standard"));
            groundMaterial.color = new Color(0.3f, 0.3f, 0.3f);
            groundObj.GetComponent<Renderer>().material = groundMaterial;
            
            // Add a simple wall for reference
            GameObject wallObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallObj.name = "Wall";
            wallObj.transform.position = new Vector3(0f, 2.5f, -10f);
            wallObj.transform.localScale = new Vector3(20f, 5f, 0.5f);
            
            // Create basic material for wall
            Material wallMaterial = new Material(Shader.Find("Standard"));
            wallMaterial.color = new Color(0.4f, 0.4f, 0.4f);
            wallObj.GetComponent<Renderer>().material = wallMaterial;
            
            // Create main camera
            GameObject cameraObj = new GameObject("Main Camera");
            Camera camera = cameraObj.AddComponent<Camera>();
            cameraObj.tag = "MainCamera";
            cameraObj.transform.position = new Vector3(0f, 3f, -8f);
            cameraObj.transform.LookAt(new Vector3(0f, 1f, 0f));
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
        }
        
        /// <summary>
        /// Creates the test player prefab
        /// </summary>
        private static GameObject CreateTestPlayerPrefab()
        {
            // Create player object
            GameObject playerObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerObj.name = "PlayerPrefab";
            
            // Add a simple head to the player
            GameObject headObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            headObj.name = "Head";
            headObj.transform.SetParent(playerObj.transform);
            headObj.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            headObj.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            
            // Add GamblerCharacter component
            playerObj.AddComponent<PDXUnderground.GamblerCharacter>();
            
            // Create prefab
            string prefabPath = "Assets/Resources/Prefabs/PlayerPrefab.prefab";
            
            // Create directory if it doesn't exist
            string directory = Path.GetDirectoryName(prefabPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
#if UNITY_2018_3_OR_NEWER
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(playerObj, prefabPath);
#else
            GameObject prefab = PrefabUtility.CreatePrefab(prefabPath, playerObj);
#endif
            
            // Clean up scene object
            Object.DestroyImmediate(playerObj);
            
            return prefab;
        }
        
        /// <summary>
        /// Creates the UI canvas prefab
        /// </summary>
        private static GameObject CreateUICanvasPrefab()
        {
            // Create UI canvas
            GameObject canvasObj = new GameObject("GamblerUICanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // Add GamblerUI component
            GamblerUI gamblerUI = canvasObj.AddComponent<PDXUnderground.UI.GamblerUI>();
            
            // Set up UI components
            SetupUIComponents(canvasObj, gamblerUI);
            
            // Create prefab
            string prefabPath = $"{prefabsDir}/GamblerUICanvas.prefab";
            
            // Create directory if it doesn't exist
            string directory = Path.GetDirectoryName(prefabPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
#if UNITY_2018_3_OR_NEWER
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(canvasObj, prefabPath);
#else
            GameObject prefab = PrefabUtility.CreatePrefab(prefabPath, canvasObj);
#endif
            
            // Clean up scene object
            Object.DestroyImmediate(canvasObj);
            
            return prefab;
        }
        
        /// <summary>
        /// Sets up UI components on the canvas
        /// </summary>
        private static void SetupUIComponents(GameObject canvasObj, GamblerUI gamblerUI)
        {
            // Check if UIResourceGenerator has been run, if not generate resources
            if (!File.Exists($"{texturesDir}/CardFrame.png"))
            {
                UIResourceGenerator.GenerateAll();
            }
            
            // Load sprites
            Sprite cardFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{texturesDir}/CardFrame.png");
            Sprite cardBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{texturesDir}/CardBackground.png");
            Sprite meterFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{texturesDir}/MeterFrame.png");
            Sprite meterFillSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{texturesDir}/MeterFill.png");
            
            // Create buzz meter
            GameObject buzzMeterObj = new GameObject("BuzzMeter");
            buzzMeterObj.transform.SetParent(canvasObj.transform);
            RectTransform meterRect = buzzMeterObj.AddComponent<RectTransform>();
            meterRect.anchorMin = new Vector2(0.5f, 0.5f);
            meterRect.anchorMax = new Vector2(0.5f, 0.5f);
            meterRect.pivot = new Vector2(0.5f, 0.5f);
            meterRect.sizeDelta = new Vector2(300f, 30f);
            meterRect.anchoredPosition = new Vector2(0f, 400f);
            
            // Add frame image
            GameObject frameObj = new GameObject("Frame");
            frameObj.transform.SetParent(buzzMeterObj.transform);
            RectTransform frameRect = frameObj.AddComponent<RectTransform>();
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.offsetMin = Vector2.zero;
            frameRect.offsetMax = Vector2.zero;
            
            UnityEngine.UI.Image frameImage = frameObj.AddComponent<UnityEngine.UI.Image>();
            frameImage.sprite = meterFrameSprite;
            
            // Add fill image
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(buzzMeterObj.transform);
            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(2f, 2f);
            fillRect.offsetMax = new Vector2(-2f, -2f);
            
            UnityEngine.UI.Image fillImage = fillObj.AddComponent<UnityEngine.UI.Image>();
            fillImage.sprite = meterFillSprite;
            fillImage.type = UnityEngine.UI.Image.Type.Filled;
            fillImage.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            fillImage.fillOrigin = 0;
            fillImage.fillAmount = 0.75f;
            
            // Create card hand container
            GameObject cardHandObj = new GameObject("CardHandContainer");
            cardHandObj.transform.SetParent(canvasObj.transform);
            RectTransform cardHandRect = cardHandObj.AddComponent<RectTransform>();
            cardHandRect.anchorMin = new Vector2(0.5f, 0);
            cardHandRect.anchorMax = new Vector2(0.5f, 0);
            cardHandRect.pivot = new Vector2(0.5f, 0);
            cardHandRect.sizeDelta = new Vector2(950f, 250f);
            cardHandRect.anchoredPosition = new Vector2(0f, 50f);
            
            // Add horizontal layout group
            UnityEngine.UI.HorizontalLayoutGroup layoutGroup = cardHandObj.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            layoutGroup.spacing = 20f;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            
            // Create health text
            GameObject healthTextObj = new GameObject("HealthText");
            healthTextObj.transform.SetParent(canvasObj.transform);
            RectTransform healthTextRect = healthTextObj.AddComponent<RectTransform>();
            healthTextRect.anchorMin = new Vector2(0, 1);
            healthTextRect.anchorMax = new Vector2(0, 1);
            healthTextRect.pivot = new Vector2(0, 1);
            healthTextRect.sizeDelta = new Vector2(300f, 50f);
            healthTextRect.anchoredPosition = new Vector2(20f, -20f);
            
            TMPro.TextMeshProUGUI healthText = healthTextObj.AddComponent<TMPro.TextMeshProUGUI>();
            healthText.text = "Health: 100/100";
            healthText.fontSize = 24;
            healthText.color = Color.white;
            
            // Create buzz state text
            GameObject buzzStateObj = new GameObject("BuzzStateText");
            buzzStateObj.transform.SetParent(canvasObj.transform);
            RectTransform buzzStateRect = buzzStateObj.AddComponent<RectTransform>();
            buzzStateRect.anchorMin = new Vector2(0.5f, 0.5f);
            buzzStateRect.anchorMax = new Vector2(0.5f, 0.5f);
            buzzStateRect.pivot = new Vector2(0.5f, 0.5f);
            buzzStateRect.sizeDelta = new Vector2(200f, 40f);
            buzzStateRect.anchoredPosition = new Vector2(0f, 430f);
            
            TMPro.TextMeshProUGUI buzzStateText = buzzStateObj.AddComponent<TMPro.TextMeshProUGUI>();
            buzzStateText.text = "Normal";
            buzzStateText.fontSize = 20;
            buzzStateText.color = new Color(0.8f, 0.6f, 0.2f); // Golden
            buzzStateText.alignment = TMPro.TextAlignmentOptions.Center;
            
            // Create sample card
            GameObject cardPrefab = CreateCardPrefab();
            
            // Set references in GamblerUI component
            SerializedObject serializedUI = new SerializedObject(gamblerUI);
            serializedUI.FindProperty("buzzMeterFill").objectReferenceValue = fillImage;
            serializedUI.FindProperty("buzzMeterFrame").objectReferenceValue = frameImage;
            serializedUI.FindProperty("cardHandContainer").objectReferenceValue = cardHandObj.transform;
            serializedUI.FindProperty("cardPrefab").objectReferenceValue = cardPrefab;
            serializedUI.FindProperty("healthText").objectReferenceValue = healthText;
            serializedUI.FindProperty("buzzStateText").objectReferenceValue = buzzStateText;
            
            // Set color properties
            serializedUI.FindProperty("buzzNormalColor").colorValue = new Color(0.8f, 0.6f, 0.2f); // Gold
            serializedUI.FindProperty("buzzLowColor").colorValue = new Color(0.7f, 0.3f, 0.1f); // Orange
            serializedUI.FindProperty("buzzCriticalColor").colorValue = new Color(0.8f, 0.1f, 0.1f); // Red
            
            serializedUI.FindProperty("attackCardColor").colorValue = new Color(0.8f, 0.2f, 0.2f); // Red
            serializedUI.FindProperty("defenseCardColor").colorValue = new Color(0.2f, 0.4f, 0.8f); // Blue
            serializedUI.FindProperty("recoveryCardColor").colorValue = new Color(0.2f, 0.8f, 0.4f); // Green
            serializedUI.FindProperty("specialCardColor").colorValue = new Color(0.8f, 0.6f, 0.0f); // Gold
            
            serializedUI.FindProperty("maxCardsVisible").intValue = 5;
            serializedUI.FindProperty("cardSpacing").floatValue = 120f;
            
            serializedUI.ApplyModifiedProperties();
        }
        
        /// <summary>
        /// Creates a card prefab for the UI
        /// </summary>
        private static GameObject CreateCardPrefab()
        {
            // Check if UIResourceGenerator has been run, if not generate resources
            if (!File.Exists($"{texturesDir}/CardFrame.png"))
            {
                UIResourceGenerator.GenerateAll();
            }
            
            // Load sprites
            Sprite cardFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{texturesDir}/CardFrame.png");
            Sprite cardBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{texturesDir}/CardBackground.png");
            Sprite attackIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{texturesDir}/AttackIcon.png");
            Sprite cooldownOverlaySprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{texturesDir}/CooldownOverlay.png");
            
            // Create card object
            GameObject cardObj = new GameObject("CardPrefab");
            RectTransform rectTransform = cardObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(180f, 250f);
            
            // Add button component
            UnityEngine.UI.Button button = cardObj.AddComponent<UnityEngine.UI.Button>();
            
            // Add background
            GameObject bgObj = new GameObject("Background");
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.SetParent(rectTransform);
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            
            UnityEngine.UI.Image bgImage = bgObj.AddComponent<UnityEngine.UI.Image>();
            bgImage.sprite = cardBgSprite;
            
            // Add frame
            GameObject frameObj = new GameObject("Frame");
            RectTransform frameRect = frameObj.AddComponent<RectTransform>();
            frameRect.SetParent(rectTransform);
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.offsetMin = Vector2.zero;
            frameRect.offsetMax = Vector2.zero;
            
            UnityEngine.UI.Image frameImage = frameObj.AddComponent<UnityEngine.UI.Image>();
            frameImage.sprite = cardFrameSprite;
            
            button.targetGraphic = frameImage;
            
            // Add name text
            GameObject nameObj = new GameObject("Name");
            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.SetParent(rectTransform);
            nameRect.anchorMin = new Vector2(0.5f, 1f);
            nameRect.anchorMax = new Vector2(0.5f, 1f);
            nameRect.pivot = new Vector2(0.5f, 1f);
            nameRect.sizeDelta = new Vector2(160f, 30f);
            nameRect.anchoredPosition = new Vector2(0f, -15f);
            
            TMPro.TextMeshProUGUI nameText = nameObj.AddComponent<TMPro.TextMeshProUGUI>();
            nameText.text = "Card Name";
            nameText.fontSize = 16;
            nameText.alignment = TMPro.TextAlignmentOptions.Center;
            
            // Add description text
            GameObject descObj = new GameObject("Description");
            RectTransform descRect = descObj.AddComponent<RectTransform>();
            descRect.SetParent(rectTransform);
            descRect.anchorMin = new Vector2(0f, 0f);
            descRect.anchorMax = new Vector2(1f, 1f);
            descRect.pivot = new Vector2(0.5f, 0.5f);
            descRect.offsetMin = new Vector2(10f, 50f);
            descRect.offsetMax = new Vector2(-10f, -50f);
            
            TMPro.TextMeshProUGUI descText = descObj.AddComponent<TMPro.TextMeshProUGUI>();
            descText.text = "Card description goes here";
            descText.fontSize = 12;
            descText.alignment = TMPro.TextAlignmentOptions.Center;
            
            // Add cost text
            GameObject costObj = new GameObject("EnergyCost");
            RectTransform costRect = costObj.AddComponent<RectTransform>();
            costRect.SetParent(rectTransform);
            costRect.anchorMin = new Vector2(0f, 1f);
            costRect.anchorMax = new Vector2(0f, 1f);
            costRect.pivot = new Vector2(0f, 1f);
            costRect.sizeDelta = new Vector2(40f, 40f);
            costRect.anchoredPosition = new Vector2(10f, -10f);
            
            TMPro.TextMeshProUGUI costText = costObj.AddComponent<TMPro.TextMeshProUGUI>();
            costText.text = "5";
            costText.fontSize = 24;
            costText.alignment = TMPro.TextAlignmentOptions.Center;
            
            // Add type icon
            GameObject iconObj = new GameObject("TypeIcon");
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.SetParent(rectTransform);
            iconRect.anchorMin = new Vector2(1f, 1f);
            iconRect.anchorMax = new Vector2(1f, 1f);
            iconRect.pivot = new Vector2(1f, 1f);
            iconRect.sizeDelta = new Vector2(40f, 40f);
            iconRect.anchoredPosition = new Vector2(-10f, -10f);
            
            UnityEngine.UI.Image iconImage = iconObj.AddComponent<UnityEngine.UI.Image>();
            iconImage.sprite = attackIconSprite;
            
            // Add cooldown overlay
            GameObject cooldownObj = new GameObject("CooldownOverlay");
            RectTransform cooldownRect = cooldownObj.AddComponent<RectTransform>();
            cooldownRect.SetParent(rectTransform);
            cooldownRect.anchorMin = Vector2.zero;
            cooldownRect.anchorMax = Vector2.one;
            cooldownRect.offsetMin = Vector2.zero;
            cooldownRect.offsetMax = Vector2.zero;
            
            UnityEngine.UI.Image cooldownImage = cooldownObj.AddComponent<UnityEngine.UI.Image>();
            cooldownImage.sprite = cooldownOverlaySprite;
            cooldownImage.type = UnityEngine.UI.Image.Type.Filled;
            cooldownImage.fillMethod = UnityEngine.UI.Image.FillMethod.Radial360;
            cooldownImage.fillOrigin = 2; // Bottom
            cooldownImage.fillAmount = 0.5f;
            cooldownObj.SetActive(false);
            
            // Save prefab
            string prefabPath = $"{prefabsDir}/CardPrefab.prefab";
            
            // Create directory if it doesn't exist
            string directory = Path.GetDirectoryName(prefabPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
#if UNITY_2018_3_OR_NEWER
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(cardObj, prefabPath);
#else
            GameObject prefab = PrefabUtility.CreatePrefab(prefabPath, cardObj);
#endif
            
            // Clean up scene object
            Object.DestroyImmediate(cardObj);
            
            return prefab;
        }
        
        /// <summary>
        /// Creates the debug panel prefab
        /// </summary>
        private static GameObject CreateDebugPanelPrefab()
        {
            // Check if UIResourceGenerator has been run, if not generate resources
            if (!File.Exists($"{texturesDir}/DebugBackground.png"))
            {
                UIResourceGenerator.GenerateAll();
            }
            
            // Load sprite
            Sprite debugBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{texturesDir}/DebugBackground.png");
            
            // Create debug panel object
            GameObject panelObj = new GameObject("DebugPanel");
            RectTransform rectTransform = panelObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(400f, 600f);
            
            // Add background image
            UnityEngine.UI.Image bgImage = panelObj.AddComponent<UnityEngine.UI.Image>();
            bgImage.sprite = debugBgSprite;
            bgImage.color = new Color(1f, 1f, 1f, 0.8f);
            
            // Add debug panel component
            PDXUnderground.Test.DebugPanel debugPanel = panelObj.AddComponent<PDXUnderground.Test.DebugPanel>();
            
            // Create stats text
            GameObject statsObj = new GameObject("StatsText");
            statsObj.transform.SetParent(panelObj.transform);
            RectTransform statsRect = statsObj.AddComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0, 1);
            statsRect.anchorMax = new Vector2(1, 1);
            statsRect.pivot = new Vector2(0.5f, 1f);
            statsRect.offsetMin = new Vector2(10f, -200f);
            statsRect.offsetMax = new Vector2(-10f, -10f);
            
            TMPro.TextMeshProUGUI statsText = statsObj.AddComponent<TMPro.TextMeshProUGUI>();
            statsText.fontSize = 16;
            statsText.color = Color.white;
            statsText.text = "GAMBLER STATS\nHealth: 100/100\nBuzz: 100%\nBuzz State: Normal\nDefense: 50";
            
            // Create cards text
            GameObject cardsObj = new GameObject("CardsText");
            cardsObj.transform.SetParent(panelObj.transform);
            RectTransform cardsRect = cardsObj.AddComponent<RectTransform>();
            cardsRect.anchorMin = new Vector2(0, 0.33f);
