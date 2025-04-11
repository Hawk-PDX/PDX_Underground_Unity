using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using PDXUnderground.Test.Prefabs;
using PDXUnderground.Core;

namespace PDXUnderground.Test
{
    /// <summary>
    /// Helper class to establish the initial scene hierarchy for the PDX Underground test scene.
    /// This script can be attached to any GameObject in the Unity Editor and will help set up
    /// the complete test scene structure with all required components and references.
    /// </summary>
    [ExecuteInEditMode]
    public class TestSceneLayout : MonoBehaviour
    {
        [Header("Scene Structure")]
        [SerializeField] private bool createTestSceneSetup = true;
        [SerializeField] private bool createEnvironment = true;
        [SerializeField] private bool createUI = true;
        [SerializeField] private bool createLighting = true;
        
        [Header("Component References")]
        [SerializeField] private TestSceneInitializer testSceneInitializer;
        [SerializeField] private PlayerPrefab playerPrefab;
        [SerializeField] private MainGameControllerPrefab mainGameControllerPrefab;
        [SerializeField] private BuzzUIPrefab buzzUIPrefab;
        
        [Header("Generated Objects")]
        [SerializeField] private GameObject testSceneSetupObject;
        [SerializeField] private GameObject environmentObject;
        [SerializeField] private GameObject uiObject;
        [SerializeField] private GameObject lightingObject;
        
        // Cached references for editor access
        private Dictionary<string, GameObject> sceneObjects = new Dictionary<string, GameObject>();
        
        /// <summary>
        /// Creates the complete scene hierarchy
        /// </summary>
        [ContextMenu("Create Complete Scene Hierarchy")]
        public void CreateSceneHierarchy()
        {
            // Clear existing references
            sceneObjects.Clear();
            
            // Create parent objects
            if (createTestSceneSetup)
                CreateTestSceneSetup();
                
            if (createEnvironment)
                CreateEnvironment();
                
            if (createUI)
                CreateUI();
                
            if (createLighting)
                CreateLighting();
                
            // Configure component references
            ConfigureComponentReferences();
            
            Debug.Log("Scene hierarchy created successfully. Save the scene now!");
        }
        
        /// <summary>
        /// Creates the TestSceneSetup object with required components
        /// </summary>
        [ContextMenu("Create Test Scene Setup")]
        public void CreateTestSceneSetup()
        {
            // Create parent object
            testSceneSetupObject = CreateOrGetGameObject("TestSceneSetup", null);
            
            // Add TestSceneInitializer component if it doesn't exist
            if (testSceneSetupObject.GetComponent<TestSceneInitializer>() == null)
            {
                testSceneInitializer = testSceneSetupObject.AddComponent<TestSceneInitializer>();
                Debug.Log("Added TestSceneInitializer to TestSceneSetup");
            }
            else
            {
                testSceneInitializer = testSceneSetupObject.GetComponent<TestSceneInitializer>();
            }
            
            // Configure test scene initializer
            if (testSceneInitializer != null)
            {
                // Basic configuration would happen here
                // Extended configuration is done in ConfigureComponentReferences()
                Debug.Log("TestSceneSetup created and configured");
            }
        }
        
        /// <summary>
        /// Creates the Environment object with child objects
        /// </summary>
        [ContextMenu("Create Environment")]
        public void CreateEnvironment()
        {
            // Create parent object
            environmentObject = CreateOrGetGameObject("Environment", null);
            
            // Create ground
            GameObject ground = CreateOrGetGameObject("Ground", environmentObject.transform);
            if (!ground.GetComponent<MeshRenderer>())
            {
                // Create a simple plane for the ground
                GameObject primitiveGround = GameObject.CreatePrimitive(PrimitiveType.Plane);
                primitiveGround.transform.SetParent(ground.transform);
                primitiveGround.transform.localPosition = Vector3.zero;
                primitiveGround.transform.localScale = new Vector3(5, 1, 5); // 50x50 units
                primitiveGround.name = "GroundMesh";
            }
            
            // Create walls
            GameObject walls = CreateOrGetGameObject("Walls", environmentObject.transform);
            
            // Create walls if they don't exist
            if (walls.transform.childCount == 0)
            {
                // North wall
                CreateWall("NorthWall", walls.transform, new Vector3(0, 2, 25), new Vector3(50, 4, 0.5f));
                
                // South wall
                CreateWall("SouthWall", walls.transform, new Vector3(0, 2, -25), new Vector3(50, 4, 0.5f));
                
                // East wall
                CreateWall("EastWall", walls.transform, new Vector3(25, 2, 0), new Vector3(0.5f, 4, 50f));
                
                // West wall
                CreateWall("WestWall", walls.transform, new Vector3(-25, 2, 0), new Vector3(0.5f, 4, 50f));
            }
            
            // Create props - sample objects for the environment
            GameObject props = CreateOrGetGameObject("Props", environmentObject.transform);
            if (props.transform.childCount == 0)
            {
                // Create some sample props
                CreateProp("Cube", props.transform, new Vector3(5, 0.5f, 5), Vector3.one);
                CreateProp("Sphere", props.transform, new Vector3(-5, 0.5f, -5), Vector3.one);
                CreateProp("Cylinder", props.transform, new Vector3(-5, 0.5f, 5), Vector3.one);
                CreateProp("Capsule", props.transform, new Vector3(5, 0.5f, -5), Vector3.one);
            }
            
            // Create spawn points
            GameObject spawnPoints = CreateOrGetGameObject("SpawnPoints", environmentObject.transform);
            if (spawnPoints.transform.childCount == 0)
            {
                // Create player spawn point
                GameObject playerSpawn = new GameObject("PlayerSpawn");
                playerSpawn.transform.SetParent(spawnPoints.transform);
                playerSpawn.transform.localPosition = new Vector3(0, 0, 0);
                playerSpawn.transform.localRotation = Quaternion.identity;
            }
            
            Debug.Log("Environment created");
        }
        
        /// <summary>
        /// Creates the UI object with child objects
        /// </summary>
        [ContextMenu("Create UI")]
        public void CreateUI()
        {
            // Create parent object
            uiObject = CreateOrGetGameObject("UI", null);
            
            // Create BuzzUI
            GameObject buzzUI = CreateOrGetGameObject("BuzzUI", uiObject.transform);
            if (buzzUI.GetComponent<BuzzUIPrefab>() == null)
            {
                buzzUIPrefab = buzzUI.AddComponent<BuzzUIPrefab>();
                Debug.Log("Added BuzzUIPrefab to BuzzUI");
            }
            else
            {
                buzzUIPrefab = buzzUI.GetComponent<BuzzUIPrefab>();
            }
            
            // Create debug UI
            GameObject debugUI = CreateOrGetGameObject("DebugUI", uiObject.transform);
            
            Debug.Log("UI created");
        }
        
        /// <summary>
        /// Creates the Lighting object with child objects
        /// </summary>
        [ContextMenu("Create Lighting")]
        public void CreateLighting()
        {
            // Create parent object
            lightingObject = CreateOrGetGameObject("Lighting", null);
            
            // Create main directional light
            GameObject directionalLight = CreateOrGetGameObject("DirectionalLight", lightingObject.transform);
            Light light = directionalLight.GetComponent<Light>();
            if (light == null)
            {
                light = directionalLight.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1f;
                light.shadows = LightShadows.Soft;
                
                // Set rotation to classic directional light angle
                directionalLight.transform.rotation = Quaternion.Euler(50, -30, 0);
                
                Debug.Log("Added Light to DirectionalLight");
            }
            
            Debug.Log("Lighting created");
        }
        
        /// <summary>
        /// Configures all component references between objects
        /// </summary>
        private void ConfigureComponentReferences()
        {
            // Configure TestSceneInitializer
            if (testSceneInitializer != null)
            {
                // Set up default test configuration
                // This would be expanded in a full implementation to set all properties
                
                Debug.Log("Component references configured");
            }
        }
        
        /// <summary>
        /// Creates or gets a GameObject by name
        /// </summary>
        private GameObject CreateOrGetGameObject(string name, Transform parent)
        {
            // Check if already exists in the scene
            GameObject obj = GameObject.Find(name);
            
            // If it doesn't exist, create it
            if (obj == null)
            {
                obj = new GameObject(name);
                if (parent != null)
                {
                    obj.transform.SetParent(parent);
                }
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = Vector3.one;
                
                Debug.Log($"Created GameObject: {name}");
            }
            
            // Cache it
            sceneObjects[name] = obj;
            
            return obj;
        }
        
        /// <summary>
        /// Creates a wall object
        /// </summary>
        private GameObject CreateWall(string name, Transform parent, Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent);
            wall.transform.localPosition = position;
            wall.transform.localScale = scale;
            
            return wall;
        }
        
        /// <summary>
        /// Creates a prop object
        /// </summary>
        private GameObject CreateProp(string primitiveType, Transform parent, Vector3 position, Vector3 scale)
        {
            PrimitiveType type = PrimitiveType.Cube;
            
            // Determine the primitive type
            switch (primitiveType.ToLower())
            {
                case "sphere":
                    type = PrimitiveType.Sphere;
                    break;
                case "cylinder":
                    type = PrimitiveType.Cylinder;
                    break;
                case "capsule":
                    type = PrimitiveType.Capsule;
                    break;
                default:
                    type = PrimitiveType.Cube;
                    break;
            }
            
            // Create the primitive
            GameObject prop = GameObject.CreatePrimitive(type);
            prop.name = primitiveType;
            prop.transform.SetParent(parent);
            prop.transform.localPosition = position;
            prop.transform.localScale = scale;
            
            return prop;
        }
        
        /// <summary>
        /// Sets up the scene for a test run
        /// </summary>
        [ContextMenu("Setup For Test Run")]
        public void SetupForTestRun()
        {
            // Create the scene hierarchy if it doesn't exist
            if (testSceneSetupObject == null || environmentObject == null || uiObject == null || lightingObject == null)
            {
                CreateSceneHierarchy();
            }
            
            // Get the TestSceneInitializer
            TestSceneInitializer initializer = FindObjectOfType<TestSceneInitializer>();
            if (initializer != null)
            {
                // Set test mode
                initializer.SetTestMode(true);
                
                Debug.Log("Scene set up for test run. Press Play in Unity to begin testing.");
            }
            else
            {
                Debug.LogError("Failed to find TestSceneInitializer. Please create the scene hierarchy first.");
            }
        }
    }
}

