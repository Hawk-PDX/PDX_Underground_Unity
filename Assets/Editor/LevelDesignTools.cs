using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using PDXUnderground.Interaction;

/// <summary>
/// Custom editor tools for designing levels with historically accurate elements
/// for the PDX Underground game set in 1800s Portland.
/// </summary>
public class LevelDesignTools : EditorWindow
{
    #region Private Variables

    // Tab system
    private int currentTab = 0;
    private readonly string[] tabNames = { 
        "Props", 
        "Lighting", 
        "NPCs", 
        "Collectibles", 
        "Pathways", 
        "Checkpoints", 
        "Visualization" 
    };

    // Props tab
    private GameObject selectedProp;
    private float propRotation = 0f;
    private float propScale = 1f;
    private bool randomizeRotation = false;
    private bool alignToSurface = true;
    private bool showPropPreview = true;
    private Vector2 propsScrollPosition;
    private Dictionary<string, List<GameObject>> propCategories = new Dictionary<string, List<GameObject>>();
    private string[] propCategoryNames = { 
        "Furniture", 
        "Decorative", 
        "Industrial", 
        "Historical", 
        "Tunnel", 
        "Speakeasy" 
    };
    private int selectedPropCategory = 0;
    private GameObject propParent;

    // Lighting tab
    private GameObject selectedLantern;
    private Color lanternColor = new Color(1f, 0.8f, 0.6f);
    private float lanternIntensity = 1.2f;
    private float lanternRange = 5f;
    private bool addFlickerEffect = true;
    private bool randomizeFlickerSpeed = true;
    private float flickerSpeed = 0.5f;
    private Vector2 lanternsScrollPosition;
    private GameObject[] lanternPrefabs;
    private GameObject lightParent;
    private Material glowMaterial;
    private LightmapSettings lightmapSettings;

    // NPCs tab
    private GameObject[] npcPrefabs;
    private GameObject selectedNPC;
    private Vector2 npcScrollPosition;
    private bool createWaypoints = true;
    private int waypointCount = 3;
    private float patrolRadius = 5f;
    private GameObject npcParent;
    private string[] npcTypes = { 
        "Dock Worker", 
        "Bartender", 
        "Gambler", 
        "Sailor", 
        "Shopkeeper", 
        "Policeman", 
        "Criminal", 
        "Businessman" 
    };
    private int selectedNpcType = 0;

    // Collectibles tab
    private Vector2 collectiblesScrollPosition;
    private GameObject[] collectiblePrefabs;
    private GameObject selectedCollectible;
    private bool addHighlightEffect = true;
    private Color collectibleGlowColor = Color.yellow;
    private string collectibleName = "Artifact";
    private string collectibleDescription = "A historical artifact from Portland's past.";
    private GameObject collectiblesParent;
    private ItemType selectedItemType = ItemType.Artifact;
    private bool isStoryRequired = false;
    private int itemValue = 10;

    // Pathways tab
    private GameObject[] pathwayPrefabs;
    private GameObject selectedPathway;
    private Vector2 pathwaysScrollPosition;
    private bool requireItemToUnlock = true;
    private string requiredItemID = "";
    private string pathwayName = "Hidden Passage";
    private GameObject pathwaysParent;
    private string unlockCondition = "Collect specific item";
    private string[] unlockConditions = { 
        "Always unlocked", 
        "Collect specific item", 
        "Complete event", 
        "Solve puzzle", 
        "Scripted unlock" 
    };
    private int selectedUnlockCondition = 0;

    // Checkpoints tab
    private Vector2 checkpointsScrollPosition;
    private GameObject checkpointPrefab;
    private GameObject selectedCheckpoint;
    private string checkpointName = "Checkpoint";
    private string checkpointDescription = "A safe haven in the Shanghai Tunnels.";
    private GameObject checkpointsParent;
    private bool autoConnect = true;
    private Color checkpointColor = Color.cyan;

    // Visualization tab
    private bool showCheckpointConnections = true;
    private bool showRequiredItems = true;
    private bool showCollectibleLocations = true;
    private bool showProgressionFlow = true;
    private bool showNPCPaths = true;
    private bool showLightingOverlay = true;
    private float visualizationScale = 1f;
    private Color connectionColor = Color.green;
    private Color flowColor = Color.blue;
    private Color blockedPathColor = Color.red;
    private LineType lineType = LineType.Solid;
    private enum LineType { Solid, Dashed, Dotted }

    // Prefabs location
    private string prefabsPath = "Assets/Prefabs/";
    private bool prefabsLoaded = false;
    private bool showLoadingError = false;
    private string errorMessage = "";

    #endregion

    #region Window Setup

    [MenuItem("PDX Underground/Level Design Tools")]
    public static void ShowWindow()
    {
        GetWindow<LevelDesignTools>("PDX Level Design");
    }

    private void OnEnable()
    {
        try
        {
            LoadPrefabs();
            InitializeParents();
        }
        catch (System.Exception e)
        {
            showLoadingError = true;
            errorMessage = "Error loading prefabs: " + e.Message;
            Debug.LogError(errorMessage);
        }
    }

    private void LoadPrefabs()
    {
        // Load props by category
        foreach (string category in propCategoryNames)
        {
            string path = prefabsPath + "Props/" + category;
            List<GameObject> props = new List<GameObject>();
            
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { path });
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prop = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prop != null)
                {
                    props.Add(prop);
                }
            }
            
            propCategories[category] = props;
        }

        // Load lanterns
        string lanternsPath = prefabsPath + "Lights/Lanterns";
        string[] lanternGuids = AssetDatabase.FindAssets("t:Prefab", new[] { lanternsPath });
        lanternPrefabs = new GameObject[lanternGuids.Length];
        
        for (int i = 0; i < lanternGuids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(lanternGuids[i]);
            lanternPrefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        }

        // Load NPCs
        string npcsPath = prefabsPath + "Characters/NPCs";
        string[] npcGuids = AssetDatabase.FindAssets("t:Prefab", new[] { npcsPath });
        npcPrefabs = new GameObject[npcGuids.Length];
        
        for (int i = 0; i < npcGuids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(npcGuids[i]);
            npcPrefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        }

        // Load collectibles
        string collectiblesPath = prefabsPath + "Collectibles";
        string[] collectibleGuids = AssetDatabase.FindAssets("t:Prefab", new[] { collectiblesPath });
        collectiblePrefabs = new GameObject[collectibleGuids.Length];
        
        for (int i = 0; i < collectibleGuids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(collectibleGuids[i]);
            collectiblePrefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        }

        // Load pathways
        string pathwaysPath = prefabsPath + "Pathways";
        string[] pathwayGuids = AssetDatabase.FindAssets("t:Prefab", new[] { pathwaysPath });
        pathwayPrefabs = new GameObject[pathwayGuids.Length];
        
        for (int i = 0; i < pathwayGuids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(pathwayGuids[i]);
            pathwayPrefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        }

        // Load checkpoint prefab
        string checkpointsPath = prefabsPath + "Checkpoints";
        string[] checkpointGuids = AssetDatabase.FindAssets("t:Prefab", new[] { checkpointsPath });
        if (checkpointGuids.Length > 0)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(checkpointGuids[0]);
            checkpointPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        }

        prefabsLoaded = true;
    }

    private void InitializeParents()
    {
        // Create parent objects if they don't exist
        GameObject levelDesign = GameObject.Find("LevelDesign");
        if (levelDesign == null)
        {
            levelDesign = new GameObject("LevelDesign");
        }

        propParent = FindOrCreateChild(levelDesign, "Props");
        lightParent = FindOrCreateChild(levelDesign, "Lighting");
        npcParent = FindOrCreateChild(levelDesign, "NPCs");
        collectiblesParent = FindOrCreateChild(levelDesign, "Collectibles");
        pathwaysParent = FindOrCreateChild(levelDesign, "Pathways");
        checkpointsParent = FindOrCreateChild(levelDesign, "Checkpoints");
    }

    private GameObject FindOrCreateChild(GameObject parent, string name)
    {
        Transform child = parent.transform.Find(name);
        if (child == null)
        {
            GameObject newChild = new GameObject(name);
            newChild.transform.parent = parent.transform;
            return newChild;
        }
        return child.gameObject;
    }

    #endregion

    #region Main GUI

    private void OnGUI()
    {
        // Show error if prefabs didn't load correctly
        if (showLoadingError)
        {
            EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
            if (GUILayout.Button("Retry Loading Prefabs"))
            {
                showLoadingError = false;
                try
                {
                    LoadPrefabs();
                    InitializeParents();
                }
                catch (System.Exception e)
                {
                    showLoadingError = true;
                    errorMessage = "Error loading prefabs: " + e.Message;
                    Debug.LogError(errorMessage);
                }
            }
            return;
        }

        GUILayout.Space(10);
        GUILayout.Label("PDX Underground Level Design Tools", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Design historically accurate environments set in 1800s Portland", MessageType.Info);
        GUILayout.Space(5);

        // Tabs
        currentTab = GUILayout.Toolbar(currentTab, tabNames);
        
        GUILayout.Space(15);
        
        switch (currentTab)
        {
            case 0: // Props
                DrawPropsTab();
                break;
            case 1: // Lighting
                DrawLightingTab();
                break;
            case 2: // NPCs
                DrawNPCsTab();
                break;
            case 3: // Collectibles
                DrawCollectiblesTab();
                break;
            case 4: // Pathways
                DrawPathwaysTab();
                break;
            case 5: // Checkpoints
                DrawCheckpointsTab();
                break;
            case 6: // Visualization
                DrawVisualizationTab();
                break;
        }
    }

    #endregion

    #region Props Tab

    private void DrawPropsTab()
    {
        EditorGUILayout.LabelField("Period-Appropriate Props", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Place historically accurate props from 1800s Portland", MessageType.Info);
        
        GUILayout.Space(10);
        
        // Category selector
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Category:", GUILayout.Width(70));
        selectedPropCategory = EditorGUILayout.Popup(selectedPropCategory, propCategoryNames);
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(5);
        
        // Props grid
        if (propCategories.Count > 0 && selectedPropCategory < propCategoryNames.Length)
        {
            string categoryName = propCategoryNames[selectedPropCategory];
            List<GameObject> props = propCategories.ContainsKey(categoryName) ? propCategories[categoryName] : new List<GameObject>();
            
            if (props.Count > 0)
            {
                propsScrollPosition = EditorGUILayout.BeginScrollView(propsScrollPosition, GUILayout.Height(150));
                
                int columns = Mathf.FloorToInt(position.width / 80);
                int rows = Mathf.CeilToInt(props.Count / (float)columns);
                
                for (int y = 0; y < rows; y++)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    for (int x = 0; x < columns; x++)
                    {
                        int index = y * columns + x;
                        if (index < props.Count)
                        {
                            if (DrawPropButton(props[index]))
                            {
                                selectedProp = props[index];
                            }
                        }
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.HelpBox("No props found in this category", MessageType.Warning);
            }
        }
        
        GUILayout.Space(10);
        
        // Prop settings
        if (selectedProp != null)
            EditorGUILayout.LabelField("Selected Prop: " + selectedProp.name, EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Placement settings
            propRotation = EditorGUILayout.Slider("Rotation (Y)", propRotation, 0f, 360f);
            propScale = EditorGUILayout.Slider("Scale", propScale, 0.5f, 2f);
            randomizeRotation = EditorGUILayout.Toggle("Randomize Rotation", randomizeRotation);
            alignToSurface = EditorGUILayout.Toggle("Align to Surface", alignToSurface);
            showPropPreview = EditorGUILayout.Toggle("Show Preview", showPropPreview);
            
            GUILayout.Space(10);
            
            // Place button
            if (GUILayout.Button("Place Prop", GUILayout.Height(30)))
            {
                PlaceProp();
            }
            
            EditorGUILayout.EndVertical();
            
            // Show preview in scene view if enabled
            if (showPropPreview)
            {
                SceneView.duringSceneGui -= OnSceneGUI;
                SceneView.duringSceneGui += OnSceneGUI;
            }
            else
            {
                SceneView.duringSceneGui -= OnSceneGUI;
            }
        }
        else
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            EditorGUILayout.HelpBox("Select a prop to place", MessageType.Info);
        }
        
        GUILayout.Space(10);
        
        // Historical context section
        EditorGUILayout.LabelField("Historical Context", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("1800s Portland featured a mix of wooden structures, industrial equipment, and maritime decor reflecting its position as a key port city during westward expansion.", MessageType.Info);
    }
    
    private bool DrawPropButton(GameObject prop)
    {
        // Create a button with preview image
        GUIContent content = new GUIContent();
        content.text = prop.name;
        
        // Try to get a preview image
        Texture2D previewImage = AssetPreview.GetAssetPreview(prop);
        if (previewImage != null)
        {
            content.image = previewImage;
            content.text = ""; // Remove text when we have an image
        }
        
        // Selected state
        bool isSelected = (selectedProp == prop);
        GUIStyle style = new GUIStyle(GUI.skin.button);
        if (isSelected)
        {
            style.normal.background = MakeTex(1, 1, new Color(0.5f, 0.8f, 1f, 0.5f));
        }
        
        // Draw button with tooltip
        content.tooltip = prop.name;
        return GUILayout.Button(content, style, GUILayout.Width(75), GUILayout.Height(75));
    }
    
    private void PlaceProp()
    {
        if (selectedProp == null)
            return;
            
        // Get mouse position in scene
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            // Instantiate the prop
            GameObject prop = PrefabUtility.InstantiatePrefab(selectedProp) as GameObject;
            prop.transform.position = hit.point;
            
            // Apply rotation
            float yRotation = randomizeRotation ? Random.Range(0f, 360f) : propRotation;
            prop.transform.rotation = Quaternion.Euler(0, yRotation, 0);
            
            // Apply scale
            prop.transform.localScale = Vector3.one * propScale;
            
            // Apply alignment to surface if enabled
            if (alignToSurface)
            {
                prop.transform.up = hit.normal;
            }
            
            // Set parent
            prop.transform.SetParent(propParent.transform);
            
            // Select the placed object
            Selection.activeGameObject = prop;
            
            // Register undo
            Undo.RegisterCreatedObjectUndo(prop, "Place Prop");
        }
    }
    
    private void OnSceneGUI(SceneView sceneView)
    {
        if (selectedProp == null || !showPropPreview)
            return;
            
        // Draw preview at mouse position
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            // Draw position handle
            Quaternion rotation = Quaternion.Euler(0, propRotation, 0);
            if (alignToSurface)
            {
                rotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * rotation;
            }
            
            // Draw mesh preview
            MeshFilter[] meshFilters = selectedProp.GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter mf in meshFilters)
            {
                if (mf.sharedMesh != null)
                {
                    Matrix4x4 matrix = Matrix4x4.TRS(
                        hit.point,
                        rotation,
                        Vector3.one * propScale
                    );
                    
                    // Draw wireframe preview
                    Handles.color = new Color(0.5f, 0.8f, 1f, 0.5f);
                    Handles.DrawWireMesh(mf.sharedMesh, mf.transform.localPosition, mf.transform.localRotation, mf.transform.localScale, matrix);
                }
            }
            
            // Draw label
            Handles.BeginGUI();
            Vector3 screenPoint = Camera.current.WorldToScreenPoint(hit.point);
            GUI.Label(new Rect(screenPoint.x - 50, Camera.current.pixelHeight - screenPoint.y - 30, 100, 20), selectedProp.name);
            Handles.EndGUI();
            
            // Allow placement with left click
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                PlaceProp();
                Event.current.Use();
            }
            
            sceneView.Repaint();
        }
    }
    
    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    #endregion

    #region Lighting Tab

    private void DrawLightingTab()
    {
        EditorGUILayout.LabelField("Historical Lighting", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Place authentic 1800s lighting sources like lanterns, candles, and oil lamps", MessageType.Info);
        
        GUILayout.Space(10);
        
        // Lantern selection
        EditorGUILayout.LabelField("Lantern Selection", EditorStyles.boldLabel);
        if (lanternPrefabs != null && lanternPrefabs.Length > 0)
        {
            lanternsScrollPosition = EditorGUILayout.BeginScrollView(lanternsScrollPosition, GUILayout.Height(120));
            
            int columns = Mathf.FloorToInt(position.width / 90);
            int rows = Mathf.CeilToInt(lanternPrefabs.Length / (float)columns);
            
            for (int y = 0; y < rows; y++)
            {
                EditorGUILayout.BeginHorizontal();
                
                for (int x = 0; x < columns; x++)
                {
                    int index = y * columns + x;
                    if (index < lanternPrefabs.Length && lanternPrefabs[index] != null)
                    {
                        if (GUILayout.Button(
                            new GUIContent(
                                AssetPreview.GetAssetPreview(lanternPrefabs[index]), 
                                lanternPrefabs[index].name
                            ), 
                            GUILayout.Width(85), 
                            GUILayout.Height(85)
                        ))
                        {
                            selectedLantern = lanternPrefabs[index];
                        }
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.HelpBox("No lantern prefabs found. Please create lantern prefabs in the Prefabs/Lights/Lanterns folder.", MessageType.Warning);
        }
        
        GUILayout.Space(10);
        
        // Lantern settings
        if (selectedLantern != null)
        {
            EditorGUILayout.LabelField("Selected Lantern: " + selectedLantern.name, EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            lanternColor = EditorGUILayout.ColorField("Light Color", lanternColor);
            lanternIntensity = EditorGUILayout.Slider("Light Intensity", lanternIntensity, 0.1f, 3f);
            lanternRange = EditorGUILayout.Slider("Light Range", lanternRange, 1f, 10f);
            addFlickerEffect = EditorGUILayout.Toggle("Add Flicker Effect", addFlickerEffect);
            
            if (addFlickerEffect)
            {
                EditorGUI.indentLevel++;
                randomizeFlickerSpeed = EditorGUILayout.Toggle("Randomize Flicker", randomizeFlickerSpeed);
                if (!randomizeFlickerSpeed)
                {
                    flickerSpeed = EditorGUILayout.Slider("Flicker Speed", flickerSpeed, 0.1f, 2f);
                }
                EditorGUI.indentLevel--;
            }
            
            GUILayout.Space(10);
            
            EditorGUILayout.LabelField("Historical Note:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("In the 1800s, lanterns used whale oil or kerosene with a warm, yellowish glow, much dimmer than modern lighting.", MessageType.Info);
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Place Lantern", GUILayout.Height(30)))
            {
                PlaceLantern();
            }
            
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("Select a lantern to place", MessageType.Info);
        }
    }
    
    private void PlaceLantern()
    {
        if (selectedLantern == null)
            return;
            
        // Get mouse position in scene
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            // Instantiate the lantern
            GameObject lantern = PrefabUtility.InstantiatePrefab(selectedLantern) as GameObject;
            lantern.transform.position = hit.point;
            lantern.transform.SetParent(lightParent.transform);
            
            // Add or configure light component
            Light light = lantern.GetComponentInChildren<Light>();
            if (light == null)
            {
                GameObject lightObj = new GameObject("LanternLight");
                lightObj.transform.SetParent(lantern.transform);
                lightObj.transform.localPosition = Vector3.up * 0.2f;
                light = lightObj.AddComponent<Light>();
            }
            
            // Configure light properties
            light.type = LightType.Point;
            light.color = lanternColor;
            light.intensity = lanternIntensity;
            light.range = lanternRange;
            light.shadows = LightShadows.Soft;
            
            // Add flicker effect component if needed
            if (addFlickerEffect)
            {
                LanternFlicker flicker = lantern.GetComponent<LanternFlicker>();
                if (flicker == null)
                {
                    flicker = lantern.AddComponent<LanternFlicker>();
                }
                
                // Configure flicker
                flicker.minIntensity = lanternIntensity * 0.8f;
                flicker.maxIntensity = lanternIntensity * 1.2f;
                flicker.flickerSpeed = randomizeFlickerSpeed ? Random.Range(0.3f, 1.0f) : flickerSpeed;
                flicker.colorVariation = 0.1f;
            }
            
            // Register undo
            Undo.RegisterCreatedObjectUndo(lantern, "Place Lantern");
            
            // Select the placed object
            Selection.activeGameObject = lantern;
        }
    }
    
    #endregion
    
    #region NPCs Tab
    
    private void DrawNPCsTab()
    {
        EditorGUILayout.LabelField("Historical Characters", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Place period-appropriate NPCs from 1800s Portland", MessageType.Info);
        
        GUILayout.Space(10);
        
        // NPC Type Selector
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("NPC Type:", GUILayout.Width(70));
        selectedNpcType = EditorGUILayout.Popup(selectedNpcType, npcTypes);
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(5);
        
        // NPCs selection grid
        if (npcPrefabs != null && npcPrefabs.Length > 0)
        {
            npcScrollPosition = EditorGUILayout.BeginScrollView(npcScrollPosition, GUILayout.Height(120));
            
            int columns = Mathf.FloorToInt(position.width / 90);
            int rows = Mathf.CeilToInt(npcPrefabs.Length / (float)columns);
            
            for (int y = 0; y < rows; y++)
            {
                EditorGUILayout.BeginHorizontal();
                
                for (int x = 0; x < columns; x++)
                {
                    int index = y * columns + x;
                    if (index < npcPrefabs.Length && npcPrefabs[index] != null)
                    {
                        // Only show NPCs that match the selected type
                        // This assumes NPCs have a component or tag that identifies their type
                        if (npcPrefabs[index].name.Contains(npcTypes[selectedNpcType]) || 
                            selectedNpcType == 0) // Show all if "Dock Worker" (first option) is selected
                        {
                            if (GUILayout.Button(
                                new GUIContent(
                                    AssetPreview.GetAssetPreview(npcPrefabs[index]), 
                                    npcPrefabs[index].name
                                ), 
                                GUILayout.Width(85), 
                                GUILayout.Height(85)
                            ))
                            {
                                selectedNPC = npcPrefabs[index];
                            }
                        }
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.HelpBox("No NPC prefabs found. Please create NPC prefabs in the Prefabs/Characters/NPCs folder.", MessageType.Warning);
        }
        
        GUILayout.Space(10);
        
        // NPC settings
        if (selectedNPC != null)
        {
            EditorGUILayout.LabelField("Selected NPC: " + selectedNPC.name, EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Waypoint settings
            createWaypoints = EditorGUILayout.Toggle("Create Waypoints", createWaypoints);
            
            if (createWaypoints)
            {
                EditorGUI.indentLevel++;
                waypointCount = EditorGUILayout.IntSlider("Waypoint Count", waypointCount, 2, 8);
                patrolRadius = EditorGUILayout.Slider("Patrol Radius", patrolRadius, 1f, 15f);
                EditorGUI.indentLevel--;
            }
            
            GUILayout.Space(10);
            
            EditorGUILayout.LabelField("Historical Note:", EditorStyles.boldLabel);
            string historicalNote = "";
            
            // Show historical context based on NPC type
            switch (npcTypes[selectedNpcType])
            {
                case "Dock Worker":
                    historicalNote = "Dock workers in 1800s Portland handled cargo from ships involved in lumber, wheat, and fisheries trade.";
                    break;
                case "Bartender":
                    historicalNote = "Bartenders often had connections to Shanghai Tunnels, sometimes facilitating the 'crimping' of sailors.";
                    break;
                case "Gambler":
                    historicalNote = "Gambling was prevalent in Portland's saloons and opium dens during the gold rush era.";
                    break;
                case "Sailor":
                    historicalNote = "Sailors were frequently targets of crimping (kidnapping for forced labor aboard ships).";
                    break;
                case "Shopkeeper":
                    historicalNote = "Shopkeepers sold supplies to miners heading to gold fields and pioneers traveling the Oregon Trail.";
                    break;
                default:
                    historicalNote = "Various characters populated 1800s Portland, from respectable merchants to shady underground figures.";
                    break;
            }
            
            EditorGUILayout.HelpBox(historicalNote, MessageType.Info);
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Place NPC", GUILayout.Height(30)))
            {
                PlaceNPC();
            }
            
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("Select an NPC to place", MessageType.Info);
        }
    }
    
    private void PlaceNPC()
    {
        if (selectedNPC == null)
            return;
            
        // Get mouse position in scene
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            // Instantiate the NPC
            GameObject npc = PrefabUtility.InstantiatePrefab(selectedNPC) as GameObject;
            npc.transform.position = hit.point;
            npc.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            npc.transform.SetParent(npcParent.transform);
            
            // Create waypoints if enabled
            if (createWaypoints)
            {
                GameObject waypointsHolder = new GameObject("Waypoints_" + npc.name);
                waypointsHolder.transform.SetParent(npc.transform);
                
                // Create waypoints in a circular pattern
                for (int i = 0; i < waypointCount; i++)
                {
                    float angle = i * (360f / waypointCount);
                    float radians = angle * Mathf.Deg2Rad;
                    
                    Vector3 waypointPos = new Vector3(
                        hit.point.x + Mathf.Sin(radians) * patrolRadius,
                        hit.point.y,
                        hit.point.z + Mathf.Cos(radians) * patrolRadius
                    );
                    
                    GameObject waypoint = new GameObject("Waypoint_" + i);
                    waypoint.transform.position = waypointPos;
                    waypoint.transform.SetParent(waypointsHolder.transform);
                    
                    // Add visual representation in editor
                    if (!Application.isPlaying)
                    {
                        GameObject visualMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        visualMarker.transform.position = waypointPos;
                        visualMarker.transform.localScale = Vector3.one * 0.5f;
                        visualMarker.transform.SetParent(waypoint.transform);
                        visualMarker.name = "EditorVisual";
                        visualMarker.hideFlags = HideFlags.HideInHierarchy;
                        
                        // Destroy collider, it's just for visualization
                        DestroyImmediate(visualMarker.GetComponent<Collider>());
                    }
                }
                
                // Try to find and configure AI script
                var aiNavigation = npc.GetComponent<MonoBehaviour>();
                if (aiNavigation != null && aiNavigation.GetType().Name.Contains("AI"))
                {
                    // Use reflection to find patrol-related fields/properties
                    var patrolTargetsField = aiNavigation.GetType().GetField("patrolTargets");
                    if (patrolTargetsField != null)
                    {
                        // Collect transforms of waypoints
                        Transform[] waypoints = new Transform[waypointCount];
                        for (int i = 0; i < waypointCount; i++)
                        {
                            waypoints[i] = waypointsHolder.transform.GetChild(i).transform;
                        }
                        
                        // Assign waypoints to the AI component
                        patrolTargetsField.SetValue(aiNavigation, waypoints);
                    }
                }
            }
            
            // Register undo
            Undo.RegisterCreatedObjectUndo(npc, "Place NPC");
            
            // Select the placed NPC
            Selection.activeGameObject = npc;
        }
    }
    
    #endregion
    
    #region Collectibles Tab
    
    private void DrawCollectiblesTab()
    {
        EditorGUILayout.LabelField("Historical Collectibles", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Place period-appropriate collectible items from 1800s Portland", MessageType.Info);
        
        GUILayout.Space(10);
        
        // Collectible Type selector
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Item Type:", GUILayout.Width(70));
        selectedItemType = (ItemType)EditorGUILayout.EnumPopup(selectedItemType);
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(5);
        
        // Collectibles grid
        if (collectiblePrefabs != null && collectiblePrefabs.Length > 0)
        {
            collectiblesScrollPosition = EditorGUILayout.BeginScrollView(collectiblesScrollPosition, GUILayout.Height(120));
            
            int columns = Mathf.FloorToInt(position.width / 90);
            int rows = Mathf.CeilToInt(collectiblePrefabs.Length / (float)columns);
            
            for (int y = 0; y < rows; y++)
            {
                EditorGUILayout.BeginHorizontal();
                
                for (int x = 0; x < columns; x++)
                {
                    int index = y * columns + x;
                    if (index < collectiblePrefabs.Length && collectiblePrefabs[index] != null)
                    {
                        CollectibleItem item = collectiblePrefabs[index].GetComponent<CollectibleItem>();
                        // Only show items that match the selected type or show all if we can't determine type
                        if (item == null || item.Type == selectedItemType)
                        {
                            if (GUILayout.Button(
                                new GUIContent(
                                    AssetPreview.GetAssetPreview(collectiblePrefabs[index]), 
                                    collectiblePrefabs[index].name
                                ), 
                                GUILayout.Width(85), 
                                GUILayout.Height(85)
                            ))
                            {
                                selectedCollectible = collectiblePrefabs[index];
                            }
                        }
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.HelpBox("No collectible prefabs found. Please create collectible prefabs in the Prefabs/Collectibles folder.", MessageType.Warning);
        }
        
        GUILayout.Space(10);
        
        // Collectible settings
        if (selectedCollectible != null)
        {
            EditorGUILayout.LabelField("Selected Collectible: " + selectedCollectible.name, EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Item properties
            collectibleName = EditorGUILayout.TextField("Item Name", collectibleName);
            collectibleDescription = EditorGUILayout.TextArea(collectibleDescription, GUILayout.Height(60));
            itemValue = EditorGUILayout.IntSlider("Item Value", itemValue, 1, 100);
            isStoryRequired = EditorGUILayout.Toggle("Required for Story", isStoryRequired);
            
            // Visual effects
            addHighlightEffect = EditorGUILayout.Toggle("Add Highlight Effect", addHighlightEffect);
            if (addHighlightEffect)
            {
                EditorGUI.indentLevel++;
                collectibleGlowColor = EditorGUILayout.ColorField("Glow Color", collectibleGlowColor);
                EditorGUI.indentLevel--;
            }
            
            GUILayout.Space(10);
            
            // Historical context based on item type
            EditorGUILayout.LabelField("Historical Context:", EditorStyles.boldLabel);
            string historicalContext = GetHistoricalContext(selectedItemType);
            EditorGUILayout.HelpBox(historicalContext, MessageType.Info);
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Place Collectible", GUILayout.Height(30)))
            {
                PlaceCollectible();
            }
            
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("Select a collectible to place", MessageType.Info);
        }
    }
    
    private string GetHistoricalContext(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Artifact:
                return "Portland's gold rush era artifacts often include mining tools, pocket watches, and personal items left behind during the rapid growth of the city.";
            case ItemType.Document:
                return "Historical documents include ship manifests, maps, and business ledgers that detail the commerce and 'crimping' activities of the era.";
            case ItemType.Photograph:
                return "Early photographs of Portland show the bustling port, wooden buildings, and the contrast between respectable society and the rough frontier elements.";
            case ItemType.Currency:
                return "Currency from the 1800s includes gold nuggets, foreign coins, and early American dollars, reflecting Portland's role as a trading hub.";
            case ItemType.Tool:
                return "Period tools include shipbuilding implements, carpentry tools, and specialized items for loading cargo and constructing the tunnels.";
            case ItemType.Clothing:
                return "Fashion of the 1800s ranged from practical work clothes for laborers to fine attire for the wealthy merchants who controlled the port.";
            case ItemType.Jewelry:
                return "Jewelry pieces often combined gold from Oregon and California mines with imported gemstones, showing the wealth generated during the gold rush.";
            case ItemType.Contraband:
                return "Opium, smuggled alcohol, and forbidden goods flowed through Portland's underground tunnels, evading authorities and taxes.";
            case ItemType.KeyItem:
                return "Key items like master keys, secret maps, and membership tokens were essential for navigating Portland's underground society.";
            default:
                return "Various historical items can be found throughout Shanghai Tunnels, each telling a part of Portland's colorful past.";
        }
    }
    
    private void PlaceCollectible()
    {
        if (selectedCollectible == null)
            return;
            
        // Get mouse position in scene
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            // Instantiate the collectible
            GameObject collectible = PrefabUtility.InstantiatePrefab(selectedCollectible) as GameObject;
            collectible.transform.position = hit.point + Vector3.up * 0.1f; // Slightly above surface
            collectible.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            collectible.transform.SetParent(collectiblesParent.transform);
            
            // Configure collectible properties
            CollectibleItem item = collectible.GetComponent<CollectibleItem>();
            if (item == null)
            {
                item = collectible.AddComponent<CollectibleItem>();
            }
            
            // Set properties based on editor values
            var itemName = item.GetType().GetField("itemName", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (itemName != null) itemName.SetValue(item, collectibleName);
            
            var itemDescription = item.GetType().GetField("itemDescription", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (itemDescription != null) itemDescription.SetValue(item, collectibleDescription);
            
            var itemTypeField = item.GetType().GetField("itemType", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (itemTypeField != null) itemTypeField.SetValue(item, selectedItemType);
            
            var valueField = item.GetType().GetField("value", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (valueField != null) valueField.SetValue(item, itemValue);
            
            var storyRequiredField = item.GetType().GetField("isStoryRequired", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (storyRequiredField != null) storyRequiredField.SetValue(item, isStoryRequired);
            
            // Add highlight effect if needed
            if (addHighlightEffect)
            {
                var lightField = item.GetType().GetField("highlightLight", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                var colorField = item.GetType().GetField("highlightColor", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                
                if (colorField != null) colorField.SetValue(item, collectibleGlowColor);
                
                // Create light if needed
                GameObject lightObj = new GameObject("HighlightLight");
                lightObj.transform.SetParent(collectible.transform);
                lightObj.transform.localPosition = Vector3.up * 0.2f;
                
                Light light = lightObj.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = collectibleGlowColor;
                light.intensity = 1.2f;
                light.range = 2f;
                
                if (lightField != null) lightField.SetValue(item, light);
            }
            
            // Register undo
            Undo.RegisterCreatedObjectUndo(collectible, "Place Collectible");
            
            // Select the placed collectible
            Selection.activeGameObject = collectible;
        }
    }
    
    #endregion
    
    #region Pathways Tab
    
    private void DrawPathwaysTab()
    {
        EditorGUILayout.LabelField("Shanghai Tunnel Pathways", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Create and configure secret pathways and tunnel entrances in the Shanghai Tunnel network", MessageType.Info);
        
        GUILayout.Space(10);
        
        // Pathway selection
        EditorGUILayout.LabelField("Pathway Selection", EditorStyles.boldLabel);
        if (pathwayPrefabs != null && pathwayPrefabs.Length > 0)
        {
            pathwaysScrollPosition = EditorGUILayout.BeginScrollView(pathwaysScrollPosition, GUILayout.Height(120));
            
            int columns = Mathf.FloorToInt(position.width / 90);
            int rows = Mathf.CeilToInt(pathwayPrefabs.Length / (float)columns);
            
            for (int y = 0; y < rows; y++)
            {
                EditorGUILayout.BeginHorizontal();
                
                for (int x = 0; x < columns; x++)
                {
                    int index = y * columns + x;
                    if (index < pathwayPrefabs.Length && pathwayPrefabs[index] != null)
                    {
                        if (GUILayout.Button(
                            new GUIContent(
                                AssetPreview.GetAssetPreview(pathwayPrefabs[index]), 
                                pathwayPrefabs[index].name
                            ), 
                            GUILayout.Width(85), 
                            GUILayout.Height(85)
                        ))
                        {
                            selectedPathway = pathwayPrefabs[index];
                        }
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.HelpBox("No pathway prefabs found. Please create pathway prefabs in the Prefabs/Pathways folder.", MessageType.Warning);
        }
        
        GUILayout.Space(10);
        
        // Pathway settings
        if (selectedPathway != null)
        {
            EditorGUILayout.LabelField("Selected Pathway: " + selectedPathway.name, EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Basic properties
            pathwayName = EditorGUILayout.TextField("Pathway Name", pathwayName);
            
            // Unlock conditions
            EditorGUILayout.LabelField("Unlock Condition:", GUILayout.Width(120));
            selectedUnlockCondition = EditorGUILayout.Popup(selectedUnlockCondition, unlockConditions);
            
            // Required item selection if applicable
            if (unlockConditions[selectedUnlockCondition] == "Collect specific item")
            {
                EditorGUI.indentLevel++;
                requireItemToUnlock = true;
                requiredItemID = EditorGUILayout.TextField("Required Item ID", requiredItemID);
                EditorGUI.indentLevel--;
            }
            else
            {
                requireItemToUnlock = false;
            }
            
            GUILayout.Space(10);
            
            // Historical context
            EditorGUILayout.LabelField("Historical Note:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Shanghai Tunnels connected Portland's Old Town businesses to the docks. " +
                "These passageways facilitated the transportation of goods, smuggling operations, and the infamous practice of shanghaiing (kidnapping men to serve as sailors).", MessageType.Info);
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Place Pathway", GUILayout.Height(30)))
            {
                PlacePathway();
            }
            
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("Select a pathway to place", MessageType.Info);
        }
    }
    private void PlacePathway()
    {
        if (selectedPathway == null)
            return;
            
        // Get mouse position in scene
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            // Instantiate the pathway
            GameObject pathway = PrefabUtility.InstantiatePrefab(selectedPathway) as GameObject;
            pathway.transform.position = hit.point;
            pathway.transform.SetParent(pathwaysParent.transform);
            pathway.name = pathwayName;
            
            // Try to find pathway component (could be HiddenPathway, TunnelEntrance, etc.)
            MonoBehaviour pathwayComponent = null;
            
            // Find any component that might be our pathway controller
            MonoBehaviour[] components = pathway.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour comp in components)
            {
                if (comp.GetType().Name.Contains("Pathway") || 
                    comp.GetType().Name.Contains("Tunnel") || 
                    comp.GetType().Name.Contains("Entrance") ||
                    comp.GetType().Name.Contains("Passage"))
                {
                    pathwayComponent = comp;
                    break;
                }
            }
            
            // If no specific component was found, try to add one
            if (pathwayComponent == null)
            {
                // Try to add HiddenPathway component using reflection
                System.Type pathwayType = System.Type.GetType("PDXUnderground.Interaction.HiddenPathway");
                if (pathwayType != null)
                {
                    pathwayComponent = pathway.AddComponent(pathwayType) as MonoBehaviour;
                }
            }
            
            // Configure unlock conditions
            if (pathwayComponent != null)
            {
                // Set properties using reflection
                System.Type compType = pathwayComponent.GetType();
                
                // Set pathway ID if available
                var pathwayIDField = compType.GetField("pathwayID");
                if (pathwayIDField != null)
                {
                    string pathwayID = System.Guid.NewGuid().ToString().Substring(0, 8);
                    pathwayIDField.SetValue(pathwayComponent, pathwayID);
                }
                
                // Set name if available
                var nameField = compType.GetField("pathwayName") ?? compType.GetField("entranceID") ?? compType.GetField("name");
                if (nameField != null)
                {
                    nameField.SetValue(pathwayComponent, pathwayName);
                }
                
                // Set unlock condition
                var unlockTypeField = compType.GetField("unlockType") ?? compType.GetField("unlockCondition");
                if (unlockTypeField != null)
                {
                    unlockTypeField.SetValue(pathwayComponent, selectedUnlockCondition);
                }
                
                // Set required item
                if (requireItemToUnlock)
                {
                    var requiredItemField = compType.GetField("requiredItemID");
                    if (requiredItemField != null)
                    {
                        requiredItemField.SetValue(pathwayComponent, requiredItemID);
                    }
                }
                
                // Set locked state
                var lockedField = compType.GetField("isLocked") ?? compType.GetField("startLocked");
                if (lockedField != null)
                {
                    lockedField.SetValue(pathwayComponent, selectedUnlockCondition > 0);
                }
            }
            
            // Register undo
            Undo.RegisterCreatedObjectUndo(pathway, "Place Pathway");
            
            // Select the placed pathway
            Selection.activeGameObject = pathway;
        }
    }
    
    #endregion
    
    #region Checkpoints Tab
    
    private void DrawCheckpointsTab()
    {
        EditorGUILayout.LabelField("Game Progression Checkpoints", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Create checkpoint system for saving progress through the Shanghai Tunnels", MessageType.Info);
        
        GUILayout.Space(10);
        
        if (checkpointPrefab != null)
        {
            EditorGUILayout.BeginHorizontal();
            
            // Preview of checkpoint
            GUILayout.Box(
                AssetPreview.GetAssetPreview(checkpointPrefab),
                GUILayout.Width(100),
                GUILayout.Height(100)
            );
            
            // Checkpoint settings
            EditorGUILayout.BeginVertical();
            
            checkpointName = EditorGUILayout.TextField("Checkpoint Name", checkpointName);
            checkpointDescription = EditorGUILayout.TextArea(checkpointDescription, GUILayout.Height(60));
            checkpointColor = EditorGUILayout.ColorField("Indicator Color", checkpointColor);
            autoConnect = EditorGUILayout.Toggle("Auto-Connect to Previous", autoConnect);
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            EditorGUILayout.LabelField("Historical Note:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Safe havens in the Shanghai Tunnels were rare. Certain areas like storehouse alcoves, hidden chambers, " +
                "and lookout points served as places where people could momentarily escape danger.", MessageType.Info);
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Place Checkpoint", GUILayout.Height(30)))
            {
                PlaceCheckpoint();
            }
            
            GUILayout.Space(15);
            
            // Existing checkpoints
            EditorGUILayout.LabelField("Existing Checkpoints:", EditorStyles.boldLabel);
            
            if (checkpointsParent != null && checkpointsParent.transform.childCount > 0)
            {
                checkpointsScrollPosition = EditorGUILayout.BeginScrollView(checkpointsScrollPosition, GUILayout.Height(120));
                
                for (int i = 0; i < checkpointsParent.transform.childCount; i++)
                {
                    GameObject checkpoint = checkpointsParent.transform.GetChild(i).gameObject;
                    EditorGUILayout.BeginHorizontal();
                    
                    if (GUILayout.Button(checkpoint.name, GUILayout.Height(25)))
                    {
                        Selection.activeGameObject = checkpoint;
                        SceneView.lastActiveSceneView.FrameSelected();
                    }
                    
                    if (GUILayout.Button("Delete", GUILayout.Width(60), GUILayout.Height(25)))
                    {
                        if (EditorUtility.DisplayDialog("Delete Checkpoint", 
                            $"Are you sure you want to delete checkpoint '{checkpoint.name}'?", 
                            "Yes", "No"))
                        {
                            Undo.DestroyObjectImmediate(checkpoint);
                        }
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.HelpBox("No checkpoints have been placed in the scene.", MessageType.Info);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No checkpoint prefab found. Please create a checkpoint prefab in the Prefabs/Checkpoints folder.", MessageType.Warning);
        }
    }
    
    private void PlaceCheckpoint()
    {
        if (checkpointPrefab == null)
            return;
            
        // Get mouse position in scene
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            // Instantiate the checkpoint
            GameObject checkpoint = PrefabUtility.InstantiatePrefab(checkpointPrefab) as GameObject;
            checkpoint.transform.position = hit.point + Vector3.up * 0.1f;
            checkpoint.transform.SetParent(checkpointsParent.transform);
            checkpoint.name = checkpointName;
            
            // Get or add Checkpoint component
            var checkpointScript = checkpoint.GetComponent<Checkpoint>();
            if (checkpointScript == null)
            {
                // Try to get Checkpoint type through reflection
                System.Type checkpointType = System.Type.GetType("PDXUnderground.Interaction.Checkpoint");
                if (checkpointType != null)
                {
                    checkpointScript = checkpoint.AddComponent(checkpointType) as MonoBehaviour;
                }
            }
            
            // Configure checkpoint properties
            if (checkpointScript != null)
            {
                // Set properties using reflection
                System.Type compType = checkpointScript.GetType();
                
                // Set checkpoint ID
                var idField = compType.GetField("checkpointID");
                if (idField != null)
                {
                    string checkpointID = System.Guid.NewGuid().ToString().Substring(0, 8);
                    idField.SetValue(checkpointScript, checkpointID);
                }
                
                // Set name
                var nameField = compType.GetField("checkpointName");
                if (nameField != null)
                {
                    nameField.SetValue(checkpointScript, checkpointName);
                }
                
                // Set description
                var descField = compType.GetField("description");
                if (descField != null)
                {
                    descField.SetValue(checkpointScript, checkpointDescription);
                }
                
                // Set respawn point
                var respawnField = compType.GetField("respawnPoint");
                if (respawnField != null)
                {
                    // Create a respawn point marker
                    GameObject respawnPoint = new GameObject("RespawnPoint");
                    respawnPoint.transform.position = hit.point + Vector3.up * 1f;
                    respawnPoint.transform.parent = checkpoint.transform;
                    
                    respawnField.SetValue(checkpointScript, respawnPoint.transform);
                }
            }
            
            // Add visual elements
            Light checkpointLight = checkpoint.GetComponentInChildren<Light>();
            if (checkpointLight == null)
            {
                GameObject lightObj = new GameObject("CheckpointLight");
                lightObj.transform.SetParent(checkpoint.transform);
                lightObj.transform.localPosition = Vector3.up * 1.5f;
                
                checkpointLight = lightObj.AddComponent<Light>();
                checkpointLight.type = LightType.Point;
                checkpointLight.color = checkpointColor;
                checkpointLight.intensity = 1.5f;
                checkpointLight.range = 3f;
            }
            
            // Auto-connect to previous checkpoint if enabled
            if (autoConnect && checkpointsParent.transform.childCount > 1)
            {
                // Get the previous checkpoint (childCount - 2 because we just added one)
                
                // Draw a development-time connection line
                GameObject connectionLine = new GameObject("Connection_" + prevCheckpoint.name + "_to_" + checkpoint.name);
                connectionLine.transform.SetParent(checkpointsParent.transform);
                
                // Add line renderer
                LineRenderer lineRenderer = connectionLine.AddComponent<LineRenderer>();
                lineRenderer.startWidth = 0.1f;
                lineRenderer.endWidth = 0.1f;
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.startColor = checkpointColor;
                lineRenderer.endColor = checkpointColor;
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, prevCheckpoint.transform.position + Vector3.up * 0.5f);
                lineRenderer.SetPosition(1, checkpoint.transform.position + Vector3.up * 0.5f);
                
                // Add arrow to indicate direction
                GameObject arrow = GameObject.CreatePrimitive(PrimitiveType.Cone);
                arrow.name = "DirectionArrow";
                arrow.transform.SetParent(connectionLine.transform);
                
                // Calculate position and rotation
                Vector3 direction = (checkpoint.transform.position - prevCheckpoint.transform.position).normalized;
                Vector3 midpoint = Vector3.Lerp(prevCheckpoint.transform.position, checkpoint.transform.position, 0.7f);
                midpoint.y += 0.5f;
                
                arrow.transform.position = midpoint;
                arrow.transform.rotation = Quaternion.LookRotation(direction);
                arrow.transform.Rotate(90, 0, 0);
                arrow.transform.localScale = new Vector3(0.2f, 0.3f, 0.2f);
                
                // Set arrow material
                Renderer arrowRenderer = arrow.GetComponent<Renderer>();
                if (arrowRenderer != null)
                {
                    arrowRenderer.material.color = checkpointColor;
                }
                
                // Remove collider
                DestroyImmediate(arrow.GetComponent<Collider>());
                
                // Track connection in metadata (could be used by checkpoint system)
                var checkpointScript = checkpoint.GetComponent<MonoBehaviour>();
                var prevCheckpointScript = prevCheckpoint.GetComponent<MonoBehaviour>();
                
                if (checkpointScript != null && prevCheckpointScript != null)
                {
                    // Try to set previous checkpoint reference
                    var prevCheckField = checkpointScript.GetType().GetField("previousCheckpoint");
                    if (prevCheckField != null)
                    {
                        prevCheckField.SetValue(checkpointScript, prevCheckpointScript);
                    }
                    
                    // Try to set next checkpoint reference
                    var nextCheckField = prevCheckpointScript.GetType().GetField("nextCheckpoint");
                    if (nextCheckField != null)
                    {
                        nextCheckField.SetValue(prevCheckpointScript, checkpointScript);
                    }
                }
            }
            
            // Register undo
            Undo.RegisterCreatedObjectUndo(checkpoint, "Place Checkpoint");
            
            // Select the placed checkpoint
            Selection.activeGameObject = checkpoint;
        }
    }
    
    #endregion
    
    #region Visualization Tab
    
    private void DrawVisualizationTab()
    {
        EditorGUILayout.LabelField("Level Flow Visualization", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Visualize the progression flow, pathways, and connections in your level", MessageType.Info);
        
        GUILayout.Space(10);
        
        // Visualization options
        EditorGUILayout.LabelField("Visualization Options", EditorStyles.boldLabel);
        
        showCheckpointConnections = EditorGUILayout.Toggle("Show Checkpoint Connections", showCheckpointConnections);
        showRequiredItems = EditorGUILayout.Toggle("Show Required Items", showRequiredItems);
        showCollectibleLocations = EditorGUILayout.Toggle("Show Collectible Locations", showCollectibleLocations);
        showProgressionFlow = EditorGUILayout.Toggle("Show Progression Flow", showProgressionFlow);
        showNPCPaths = EditorGUILayout.Toggle("Show NPC Paths", showNPCPaths);
        showLightingOverlay = EditorGUILayout.Toggle("Show Lighting Overlay", showLightingOverlay);
        
        GUILayout.Space(10);
        
        // Visualization settings
        EditorGUILayout.LabelField("Visualization Settings", EditorStyles.boldLabel);
        
        visualizationScale = EditorGUILayout.Slider("Scale", visualizationScale, 0.5f, 2f);
        connectionColor = EditorGUILayout.ColorField("Connection Color", connectionColor);
        flowColor = EditorGUILayout.ColorField("Flow Path Color", flowColor);
        blockedPathColor = EditorGUILayout.ColorField("Blocked Path Color", blockedPathColor);
        
        lineType = (LineType)EditorGUILayout.EnumPopup("Line Type", lineType);
        
        GUILayout.Space(15);
        
        // Apply/Remove visualization
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Apply Visualization", GUILayout.Height(30)))
        {
            ApplyVisualization();
            SceneView.RepaintAll();
        }
        
        if (GUILayout.Button("Remove Visualization", GUILayout.Height(30)))
        {
            RemoveVisualization();
            SceneView.RepaintAll();
        }
        
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        // Historical context explanation
        EditorGUILayout.LabelField("Historical Context", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "The Shanghai Tunnels formed a complex underground network beneath Portland's streets during the 1800s. " +
            "These tunnels connected businesses, saloons, and hotels to the waterfront, facilitating illicit " +
            "activities such as smuggling and 'shanghaiing' (kidnapping men to serve as sailors).\n\n" +
            "The visualization tools help recreate this historically significant network with accurate flow patterns " +
            "and connections that mirror the actual tunnel systems used during Portland's early days.", 
            MessageType.Info);
        
        // Register scene callback
        if (showCheckpointConnections || showRequiredItems || 
            showCollectibleLocations || showProgressionFlow || 
            showNPCPaths || showLightingOverlay)
        {
            SceneView.duringSceneGui -= OnVisualizationSceneGUI;
            SceneView.duringSceneGui += OnVisualizationSceneGUI;
        }
        else
        {
            SceneView.duringSceneGui -= OnVisualizationSceneGUI;
        }
    }
    
    private void ApplyVisualization()
    {
        // Remove any existing visualization first
        RemoveVisualization();
        
        GameObject visualizationRoot = new GameObject("Visualization");
        visualizationRoot.hideFlags = HideFlags.DontSave;
        
        // Apply all requested visualizations
        if (showCheckpointConnections)
        {
            DrawCheckpointConnections(visualizationRoot);
        }
        
        if (showRequiredItems)
        {
            DrawRequiredItemConnections(visualizationRoot);
        }
        
        if (showProgressionFlow)
        {
            DrawProgressionFlow(visualizationRoot);
        }
        
        if (showNPCPaths)
        {
            DrawNPCPatrolPaths(visualizationRoot);
        }
        
        if (showLightingOverlay)
        {
            DrawLightingOverlay(visualizationRoot);
        }
    }
    
    private void RemoveVisualization()
    {
        GameObject visualizationRoot = GameObject.Find("Visualization");
        if (visualizationRoot != null)
        {
            DestroyImmediate(visualizationRoot);
        }
    }
    
    private void OnVisualizationSceneGUI(SceneView sceneView)
    {
        // Draw any runtime visualizations (handles, gizmos, etc.)
        if (showCollectibleLocations)
        {
            DrawCollectibleLocations();
        }
    }
    
    private void DrawCheckpointConnections(GameObject parent)
    {
        // Create a container for the connections
        GameObject connectionsContainer = new GameObject("CheckpointConnections");
        connectionsContainer.transform.SetParent(parent.transform);
        
        // Find all checkpoints
        if (checkpointsParent != null)
        {
            for (int i = 0; i < checkpointsParent.transform.childCount; i++)
            {
                GameObject checkpoint = checkpointsParent.transform.GetChild(i).gameObject;
                MonoBehaviour checkpointScript = checkpoint.GetComponent<MonoBehaviour>();
                
                if (checkpointScript == null)
                    continue;
                    
                // Find the next checkpoint using reflection
                System.Type compType = checkpointScript.GetType();
                var nextCheckField = compType.GetField("nextCheckpoint");
                
                if (nextCheckField != null)
                {
                    MonoBehaviour nextCP = nextCheckField.GetValue(checkpointScript) as MonoBehaviour;
                    if (nextCP != null)
                    {
                        // Create a connection line
                        CreateConnectionLine(connectionsContainer, checkpoint, nextCP.gameObject, connectionColor);
                    }
                }
            }
        }
    }
    
    private void DrawRequiredItemConnections(GameObject parent)
    {
        // Create a container for the item connections
        GameObject itemConnectionsContainer = new GameObject("RequiredItemConnections");
        itemConnectionsContainer.transform.SetParent(parent.transform);
        
        // Find all pathways and check for required items
        if (pathwaysParent != null)
        {
            for (int i = 0; i < pathwaysParent.transform.childCount; i++)
            {
                GameObject pathway = pathwaysParent.transform.GetChild(i).gameObject;
                MonoBehaviour pathwayScript = pathway.GetComponent<MonoBehaviour>();
                
                if (pathwayScript == null)
                    continue;
                    
                // Find the required item ID using reflection
                System.Type compType = pathwayScript.GetType();
                var requiredItemField = compType.GetField("requiredItemID");
                
                if (requiredItemField != null)
                {
                    string itemID = requiredItemField.GetValue(pathwayScript) as string;
                    
                    if (!string.IsNullOrEmpty(itemID))
                    {
                        // Find the collectible with this ID
                        CollectibleItem[] collectibles = FindObjectsOfType<CollectibleItem>();
                        foreach (CollectibleItem collectible in collectibles)
                        {
                            if (collectible.ItemID == itemID)
                            {
                                // Create a connection line
                                CreateConnectionLine(
                                    itemConnectionsContainer, 
                                    collectible.gameObject, 
                                    pathway, 
                                    Color.yellow, 
                                    true);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
    
    private void DrawProgressionFlow(GameObject parent)
        // Create a container for the progression flow
        GameObject flowContainer = new GameObject("ProgressionFlow");
        flowContainer.transform.SetParent(parent.transform);
        
        // Find all checkpoints ordered by progression sequence
        if (checkpointsParent != null && checkpointsParent.transform.childCount > 0)
        {
            // Collect all checkpoint scripts
            List<MonoBehaviour> checkpoints = new List<MonoBehaviour>();
            Dictionary<MonoBehaviour, GameObject> checkpointObjects = new Dictionary<MonoBehaviour, GameObject>();
            
            for (int i = 0; i < checkpointsParent.transform.childCount; i++)
            {
                GameObject checkpointObj = checkpointsParent.transform.GetChild(i).gameObject;
                MonoBehaviour checkpoint = checkpointObj.GetComponent<MonoBehaviour>();
                
                if (checkpoint != null && checkpoint.GetType().Name.Contains("Checkpoint"))
                {
                    checkpoints.Add(checkpoint);
                    checkpointObjects[checkpoint] = checkpointObj;
                }
            }
            
            // Try to find the first checkpoint (no previous checkpoint)
            MonoBehaviour firstCheckpoint = null;
            foreach (MonoBehaviour cp in checkpoints)
            {
                var prevField = cp.GetType().GetField("previousCheckpoint");
                if (prevField != null)
                {
                    object prevCheckpoint = prevField.GetValue(cp);
                    if (prevCheckpoint == null)
                    {
                        firstCheckpoint = cp;
                        break;
                    }
                }
            }
            
            // If we found a first checkpoint, follow the chain
            if (firstCheckpoint != null)
            {
                List<MonoBehaviour> orderedCheckpoints = new List<MonoBehaviour>();
                MonoBehaviour currentCheckpoint = firstCheckpoint;
                
                while (currentCheckpoint != null)
                {
                    orderedCheckpoints.Add(currentCheckpoint);
                    
                    // Get next checkpoint
                    var nextField = currentCheckpoint.GetType().GetField("nextCheckpoint");
                    if (nextField != null)
                    {
                        currentCheckpoint = nextField.GetValue(currentCheckpoint) as MonoBehaviour;
                    }
                    else
                    {
                        currentCheckpoint = null;
                    }
                    
                    // Prevent infinite loops by checking if we've already processed this checkpoint
                    if (currentCheckpoint != null && orderedCheckpoints.Contains(currentCheckpoint))
                    {
                        break;
                    }
                }
                
                // Create the flow visualization
                for (int i = 0; i < orderedCheckpoints.Count - 1; i++)
                {
                    GameObject current = checkpointObjects[orderedCheckpoints[i]];
                    GameObject next = checkpointObjects[orderedCheckpoints[i + 1]];
                    
                    // Create a thicker line for the main progression path
                    GameObject flowLine = new GameObject("FlowLine_" + i);
                    flowLine.transform.SetParent(flowContainer.transform);
                    
                    LineRenderer lineRenderer = flowLine.AddComponent<LineRenderer>();
                    lineRenderer.startWidth = 0.25f * visualizationScale;
                    lineRenderer.endWidth = 0.25f * visualizationScale;
                    
                    // Make a curved line for better visibility
                    lineRenderer.positionCount = 10;
                    Vector3 start = current.transform.position + Vector3.up * 0.5f;
                    Vector3 end = next.transform.position + Vector3.up * 0.5f;
                    Vector3 middleOffset = Vector3.up * 2f * visualizationScale;
                    
                    for (int j = 0; j < 10; j++)
                    {
                        float t = j / 9f;
                        Vector3 mid = Vector3.Lerp(start, end, 0.5f) + middleOffset;
                        Vector3 point = BezierPoint(start, mid, end, t);
                        lineRenderer.SetPosition(j, point);
                    }
                    
                    // Set material and color
                    lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                    lineRenderer.startColor = flowColor;
                    lineRenderer.endColor = flowColor;
                    
                    // Add arrow indicator at the middle point
                    GameObject arrow = GameObject.CreatePrimitive(PrimitiveType.Cone);
                    arrow.name = "FlowArrow_" + i;
                    arrow.transform.SetParent(flowLine.transform);
                    
                    Vector3 direction = (next.transform.position - current.transform.position).normalized;
                    Vector3 midpoint = BezierPoint(start, Vector3.Lerp(start, end, 0.5f) + middleOffset, end, 0.6f);
                    
                    arrow.transform.position = midpoint;
                    arrow.transform.rotation = Quaternion.LookRotation(direction);
                    arrow.transform.Rotate(90, 0, 0);
                    arrow.transform.localScale = new Vector3(0.3f, 0.4f, 0.3f) * visualizationScale;
                    
                    // Set arrow material
                    Renderer arrowRenderer = arrow.GetComponent<Renderer>();
                    if (arrowRenderer != null)
                    {
                        arrowRenderer.material.color = flowColor;
                    }
                    
                    // Remove collider
                    DestroyImmediate(arrow.GetComponent<Collider>());
                }
                
                // Add a special marker for the first and last checkpoint
                if (orderedCheckpoints.Count > 0)
                {
                    // First checkpoint (start)
                    GameObject startMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    startMarker.name = "StartMarker";
                    startMarker.transform.SetParent(flowContainer.transform);
                    startMarker.transform.position = checkpointObjects[orderedCheckpoints[0]].transform.position + Vector3.up * 1f;
                    startMarker.transform.localScale = Vector3.one * 0.5f * visualizationScale;
                    
                    Renderer startRenderer = startMarker.GetComponent<Renderer>();
                    if (startRenderer != null)
                    {
                        startRenderer.material.color = Color.green;
                    }
                    
                    DestroyImmediate(startMarker.GetComponent<Collider>());
                    
                    // Last checkpoint (end)
                    if (orderedCheckpoints.Count > 1)
                    {
                        GameObject endMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        endMarker.name = "EndMarker";
                        endMarker.transform.SetParent(flowContainer.transform);
                        endMarker.transform.position = checkpointObjects[orderedCheckpoints[orderedCheckpoints.Count - 1]].transform.position + Vector3.up * 1f;
                        endMarker.transform.localScale = Vector3.one * 0.5f * visualizationScale;
                        
                        Renderer endRenderer = endMarker.GetComponent<Renderer>();
                        if (endRenderer != null)
                        {
                            endRenderer.material.color = Color.red;
                        }
                        
                        DestroyImmediate(endMarker.GetComponent<Collider>());
                    }
                }
            }
        }
    }
    
    private void DrawNPCPatrolPaths(GameObject parent)
    {
        // Create a container for NPC patrol paths
        GameObject npcPathsContainer = new GameObject("NPCPatrolPaths");
        npcPathsContainer.transform.SetParent(parent.transform);
        
        // Find all NPCs with patrol paths
        if (npcParent != null)
        {
            for (int i = 0; i < npcParent.transform.childCount; i++)
            {
                GameObject npc = npcParent.transform.GetChild(i).gameObject;
                
                // Look for a waypoints container
                Transform waypointsHolder = npc.transform.Find("Waypoints_" + npc.name);
                if (waypointsHolder != null && waypointsHolder.childCount > 1)
                {
                    // Create a path line for this NPC
                    GameObject pathLine = new GameObject("PatrolPath_" + npc.name);
                    pathLine.transform.SetParent(npcPathsContainer.transform);
                    
                    LineRenderer lineRenderer = pathLine.AddComponent<LineRenderer>();
                    lineRenderer.startWidth = 0.1f * visualizationScale;
                    lineRenderer.endWidth = 0.1f * visualizationScale;
                    
                    // Set up line style based on selected type
                    Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
                    switch (lineType)
                    {
                        case LineType.Dotted:
                            lineMaterial.mainTextureScale = new Vector2(10, 1);
                            lineMaterial.mainTextureOffset = new Vector2(0, 0);
                            break;
                        case LineType.Dashed:
                            lineMaterial.mainTextureScale = new Vector2(4, 1);
                            lineMaterial.mainTextureOffset = new Vector2(0, 0);
                            break;
                    }
                    lineRenderer.material = lineMaterial;
                    
                    // Set color
                    Color pathColor = new Color(0.3f, 0.7f, 0.9f);
                    lineRenderer.startColor = pathColor;
                    lineRenderer.endColor = pathColor;
                    
                    // Connect waypoints - include return to first waypoint for a loop
                    List<Vector3> positions = new List<Vector3>();
                    for (int j = 0; j < waypointsHolder.childCount; j++)
                    {
                        positions.Add(waypointsHolder.GetChild(j).position + Vector3.up * 0.2f);
                    }
                    
                    // Complete the loop
                    positions.Add(waypointsHolder.GetChild(0).position + Vector3.up * 0.2f);
                    
                    // Set line positions
                    lineRenderer.positionCount = positions.Count;
                    lineRenderer.SetPositions(positions.ToArray());
                    
                    // Add a marker at the NPC start position
                    GameObject npcMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    npcMarker.name = "NPCMarker_" + npc.name;
                    npcMarker.transform.SetParent(pathLine.transform);
                    npcMarker.transform.position = npc.transform.position + Vector3.up * 1f;
                    npcMarker.transform.localScale = Vector3.one * 0.2f * visualizationScale;
                    
                    Renderer markerRenderer = npcMarker.GetComponent<Renderer>();
                    if (markerRenderer != null)
                    {
                        markerRenderer.material.color = pathColor;
                    }
                    
                    DestroyImmediate(npcMarker.GetComponent<Collider>());
                }
            }
        }
    }
    
    private void DrawLightingOverlay(GameObject parent)
    {
        // Create a container for lighting visualization
        GameObject lightingContainer = new GameObject("LightingOverlay");
        lightingContainer.transform.SetParent(parent.transform);
        
        // Find all light sources in the scene (including lanterns)
        Light[] lights = FindObjectsOfType<Light>();
        foreach (Light light in lights)
            // Only visualize point and spot lights (not directional)
            if (light.type == LightType.Point || light.type == LightType.Spot)
            {
                // Create a sphere to show light range
                GameObject lightSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                lightSphere.name = "LightRange_" + light.gameObject.name;
                lightSphere.transform.SetParent(lightingContainer.transform);
                lightSphere.transform.position = light.transform.position;
                lightSphere.transform.localScale = Vector3.one * light.range * 2f * visualizationScale;
                
                // Configure the sphere's material
                Renderer sphereRenderer = lightSphere.GetComponent<Renderer>();
                if (sphereRenderer != null)
                {
                    Material mat = new Material(Shader.Find("Transparent/Diffuse"));
                    mat.color = new Color(light.color.r, light.color.g, light.color.b, 0.15f);
                    sphereRenderer.material = mat;
                }
                
                // Remove collider
                DestroyImmediate(lightSphere.GetComponent<Collider>());
                
                // Add a small sphere at the light source
                GameObject lightSourceSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                lightSourceSphere.name = "LightSource_" + light.gameObject.name;
                lightSourceSphere.transform.SetParent(lightSphere.transform);
                lightSourceSphere.transform.position = light.transform.position;
                lightSourceSphere.transform.localScale = Vector3.one * 0.2f * visualizationScale;
                
                // Configure the source sphere's material
                Renderer sourceRenderer = lightSourceSphere.GetComponent<Renderer>();
                if (sourceRenderer != null)
                {
                    Material mat = new Material(Shader.Find("Standard"));
                    mat.color = light.color;
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", light.color * light.intensity);
                    sourceRenderer.material = mat;
                }
                
                // Remove collider
                DestroyImmediate(lightSourceSphere.GetComponent<Collider>());
                
                // Add historical context label based on light color and intensity
                string lightType = "";
                if (light.color.r > 0.8f && light.color.g > 0.6f && light.color.b < 0.5f)
                {
                    lightType = "Gas Lamp";
                }
                else if (light.color.r > 0.8f && light.color.g > 0.7f && light.color.b > 0.2f && light.color.b < 0.5f)
                {
                    lightType = "Oil Lantern";
                }
                else if (light.color.r > 0.9f && light.color.g > 0.8f && light.color.b > 0.7f)
                {
                    lightType = "Modern Light (Anachronistic)";
                }
                else
                {
                    lightType = "Candle";
                }
                
                // Create a label for the light source
                GameObject label = new GameObject("Label_" + light.gameObject.name);
                label.transform.SetParent(lightSphere.transform);
                label.transform.position = light.transform.position + Vector3.up * 0.5f;
                
                // Add TextMesh component
                TextMesh textMesh = label.AddComponent<TextMesh>();
                textMesh.text = lightType;
                textMesh.fontSize = 24;
                textMesh.alignment = TextAlignment.Center;
                textMesh.anchor = TextAnchor.MiddleCenter;
                textMesh.color = Color.white;
                
                // Make text face the camera
                label.AddComponent<FaceCamera>();
            }
        }
        
        // Find shadow areas (areas with no light)
        // This is a simplified version just for visualization
        if (lights.Length > 0)
        {
            GameObject shadowAreaContainer = new GameObject("ShadowAreas");
            shadowAreaContainer.transform.SetParent(lightingContainer.transform);
            
            // Create a grid covering the level
            Bounds levelBounds = GetLevelBounds();
            float gridSize = 2.0f;
            int xCount = Mathf.CeilToInt(levelBounds.size.x / gridSize);
            int zCount = Mathf.CeilToInt(levelBounds.size.z / gridSize);
            
            for (int x = 0; x < xCount; x++)
            {
                for (int z = 0; z < zCount; z++)
                {
                    Vector3 position = new Vector3(
                        levelBounds.min.x + x * gridSize + gridSize / 2,
                        levelBounds.min.y,
                        levelBounds.min.z + z * gridSize + gridSize / 2
                    );
                    
                    // Check if this point is in shadow
                    bool isInShadow = true;
                    foreach (Light light in lights)
                    {
                        if (light.type != LightType.Point && light.type != LightType.Spot)
                            continue;
                            
                        float distance = Vector3.Distance(position, light.transform.position);
                        if (distance < light.range)
                        {
                            // Simple line of sight check
                            RaycastHit hit;
                            Vector3 direction = (light.transform.position - position).normalized;
                            if (!Physics.Raycast(position, direction, out hit, distance))
                            {
                                isInShadow = false;
                                break;
                            }
                        }
                    }
                    
                    // Mark shadow points
                    if (isInShadow)
                    {
                        RaycastHit hit;
                        if (Physics.Raycast(position + Vector3.up * 10, Vector3.down, out hit, 20))
                        {
                            GameObject shadowMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            shadowMarker.name = "ShadowPoint_" + x + "_" + z;
                            shadowMarker.transform.SetParent(shadowAreaContainer.transform);
                            shadowMarker.transform.position = hit.point + Vector3.up * 0.1f;
                            shadowMarker.transform.localScale = new Vector3(0.2f, 0.01f, 0.2f);
                            
                            Renderer markerRenderer = shadowMarker.GetComponent<Renderer>();
                            if (markerRenderer != null)
                            {
                                Material mat = new Material(Shader.Find("Standard"));
                                mat.color = new Color(0.1f, 0.1f, 0.2f, 0.7f);
                                markerRenderer.material = mat;
                            }
                            
                            DestroyImmediate(shadowMarker.GetComponent<Collider>());
                        }
                    }
                }
            }
        }
    }
    
    private Bounds GetLevelBounds()
    {
        // Find bounds of all static objects in the level
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        bool boundsInitialized = false;
        
        // Get all renderers
        Renderer[] renderers = FindObjectsOfType<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer.gameObject.isStatic)
            {
                if (!boundsInitialized)
                {
                    bounds = renderer.bounds;
                    boundsInitialized = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }
        }
        
        // If no static objects found, use a default area
        if (!boundsInitialized)
        {
            bounds = new Bounds(Vector3.zero, new Vector3(50, 10, 50));
        }
        
        return bounds;
    }
    
    private void DrawCollectibleLocations()
    {
        if (collectiblesParent == null)
            return;
            
        // Draw gizmos for all collectibles
        for (int i = 0; i < collectiblesParent.transform.childCount; i++)
        {
            GameObject collectible = collectiblesParent.transform.GetChild(i).gameObject;
            CollectibleItem item = collectible.GetComponent<CollectibleItem>();
            
            if (item != null)
            {
                // Determine color based on item type
                Color itemColor = Color.white;
                switch (item.Type)
                {
                    case ItemType.Artifact:
                        itemColor = Color.yellow;
                        break;
                    case ItemType.Document:
                        itemColor = Color.white;
                        break;
                    case ItemType.KeyItem:
                        itemColor = Color.red;
                        break;
                    case ItemType.Currency:
                        itemColor = new Color(0.8f, 0.8f, 0.0f);
                        break;
                    case ItemType.Contraband:
                        itemColor = new Color(0.5f, 0.0f, 0.5f);
                        break;
                    default:
                        itemColor = Color.cyan;
                        break;
                }
                
                // Draw item marker
                Handles.color = itemColor;
                Handles.DrawWireDisc(collectible.transform.position + Vector3.up * 0.5f, Vector3.up, 0.5f * visualizationScale);
                Handles.DrawLine(
                    collectible.transform.position, 
                    collectible.transform.position + Vector3.up * 2f * visualizationScale
                );
                
                // Draw item name
                Handles.Label(
                    collectible.transform.position + Vector3.up * 2f * visualizationScale,
                    item.Name + (item.IsStoryRequired ? " (Required)" : "")
                );
                
                // If it's a required item for a pathway, draw connection
                if (showRequiredItems)
                {
                    FindAndDrawRequiredItemConnections(collectible, item);
                }
            }
        }
    }
    
    private void FindAndDrawRequiredItemConnections(GameObject collectible, CollectibleItem item)
    {
        if (pathwaysParent == null)
            return;
            
        // Find pathways that require this item
        for (int i = 0; i < pathwaysParent.transform.childCount; i++)
        {
            GameObject pathway = pathwaysParent.transform.GetChild(i).gameObject;
            MonoBehaviour pathwayScript = pathway.GetComponent<MonoBehaviour>();
            
            if (pathwayScript != null)
            {
                // Find the required item ID using reflection
                System.Type compType = pathwayScript.GetType();
                var requiredItemField = compType.GetField("requiredItemID");
                
                if (requiredItemField != null)
                {
                    string requiredID = requiredItemField.GetValue(pathwayScript) as string;
                    if (!string.IsNullOrEmpty(requiredID) && requiredID == item.ItemID)
                    {
                        // Draw connection line
                        Handles.color = Color.yellow;
                        Handles.DrawDottedLine(
                            collectible.transform.position + Vector3.up * 1f,
                            pathway.transform.position + Vector3.up * 1f,
                            4f
                        );
                        
                        // Draw arrow in the middle
                        Vector3 direction = (pathway.transform.position - collectible.transform
