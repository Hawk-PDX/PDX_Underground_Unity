using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

namespace PDXUnderground.Editor
{
    /// <summary>
    /// Utility class to set up the main PDX Underground scene structure
    /// </summary>
    public class SceneSetupUtility : EditorWindow
    {
        [MenuItem("PDX Underground/Setup/Create Main Scene Structure")]
        public static void CreateMainSceneStructure()
        {
            // Create a new scene or open existing scene
            string scenePath = "Assets/Scenes/Main/MainScene.unity";
            Scene scene;

            if (File.Exists(scenePath))
            {
                // Open the scene if it exists
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }
            else
            {
                // Create a new scene
                scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, scenePath);
            }

            // Create main organizational GameObjects
            GameObject gameSystems = CreateEmptyWithName("_GameSystems");
            GameObject level = CreateEmptyWithName("_Level");
            GameObject lighting = CreateEmptyWithName("_Lighting");
            GameObject characters = CreateEmptyWithName("_Characters");
            GameObject cameras = CreateEmptyWithName("_Cameras");

            // Create GameSystems children
            GameObject gameController = CreateEmptyWithName("GameController", gameSystems.transform);
            GameObject environmentController = CreateEmptyWithName("EnvironmentController", gameSystems.transform);
            GameObject uiController = CreateEmptyWithName("UIController", gameSystems.transform);

            // Create Level children
            GameObject ground = CreateEmptyWithName("Ground", level.transform);
            GameObject streets = CreateEmptyWithName("Streets", level.transform);
            GameObject tunnels = CreateEmptyWithName("Tunnels", level.transform);
            GameObject speakeasy = CreateEmptyWithName("Speakeasy", level.transform);

            // Create basic ground plane
            GameObject groundPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            groundPlane.name = "GroundPlane";
            groundPlane.transform.position = new Vector3(0, 0, 0);
            groundPlane.transform.localScale = new Vector3(5, 1, 5);
            groundPlane.transform.SetParent(ground.transform);

            // Create Lighting children
            GameObject directionalLight = CreateEmptyWithName("DirectionalLight", lighting.transform);
            GameObject streetLights = CreateEmptyWithName("StreetLights", lighting.transform);
            GameObject tunnelLights = CreateEmptyWithName("TunnelLights", lighting.transform);
            GameObject speakeasyLights = CreateEmptyWithName("SpeakeasyLights", lighting.transform);

            // Set up main directional light
            Light mainLight = directionalLight.AddComponent<Light>();
            mainLight.type = LightType.Directional;
            mainLight.intensity = 0.4f;
            mainLight.color = new Color(1.0f, 0.95f, 0.9f);
            mainLight.shadows = LightShadows.Soft;
            directionalLight.transform.rotation = Quaternion.Euler(50, 30, 0);

            // Create Characters children
            GameObject playerSpawn = CreateEmptyWithName("PlayerSpawn", characters.transform);
            playerSpawn.transform.position = new Vector3(0, 1, 0);

            // Create main camera
            GameObject mainCamera = CreateEmptyWithName("MainCamera", cameras.transform);
            Camera cam = mainCamera.AddComponent<Camera>();
            mainCamera.AddComponent<AudioListener>();
            mainCamera.transform.position = new Vector3(0, 2, -10);
            mainCamera.transform.rotation = Quaternion.Euler(15, 0, 0);
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
            cam.clearFlags = CameraClearFlags.SkyBox;

            // Create sample placeholder objects for each environment
            CreatePlaceholderEnvironment(streets, "StreetEnv", new Vector3(0, 0, 0));
            CreatePlaceholderEnvironment(tunnels, "TunnelEnv", new Vector3(20, 0, 0));
            CreatePlaceholderEnvironment(speakeasy, "SpeakeasyEnv", new Vector3(-20, 0, 0));

            // Create sample gaslight
            CreateGaslight(streetLights.transform, new Vector3(0, 2.5f, 5), "StreetGaslight");

            // Save the scene
            EditorSceneManager.SaveScene(scene);

            Debug.Log("Main scene structure created successfully!");
        }

        /// <summary>
        /// Creates a placeholder environment with basic elements to visualize the area
        /// </summary>
        private static void CreatePlaceholderEnvironment(GameObject parent, string name, Vector3 position)
        {
            GameObject env = CreateEmptyWithName(name, parent.transform);
            env.transform.position = position;

            // Create a floor
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.position = position + new Vector3(0, -0.5f, 0);
            floor.transform.localScale = new Vector3(10, 1, 10);
            floor.transform.SetParent(env.transform);

            // Create a marker
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "Marker";
            marker.transform.position = position + new Vector3(0, 2, 0);
            marker.transform.localScale = new Vector3(0.5f, 4, 0.5f);
            marker.transform.SetParent(env.transform);

            // Add a text label (only visible in editor)
#if UNITY_EDITOR
            TextMesh label = new GameObject("Label").AddComponent<TextMesh>();
            label.text = name;
            label.fontSize = 50;
            label.alignment = TextAlignment.Center;
            label.anchor = TextAnchor.MiddleCenter;
            label.transform.position = position + new Vector3(0, 5, 0);
            label.transform.rotation = Quaternion.LookRotation(Vector3.forward);
            label.transform.SetParent(env.transform);
#endif
        }

        /// <summary>
        /// Creates a basic gaslight placeholder
        /// </summary>
        private static void CreateGaslight(Transform parent, Vector3 position, string name)
        {
            GameObject gaslight = CreateEmptyWithName(name, parent);
            gaslight.transform.position = position;

            // Create a pole
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "Pole";
            pole.transform.localScale = new Vector3(0.1f, 1f, 0.1f);
            pole.transform.SetParent(gaslight.transform);
            pole.transform.localPosition = new Vector3(0, -0.5f, 0);

            // Create a lamp globe
            GameObject globe = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            globe.name = "LampGlobe";
            globe.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            globe.transform.SetParent(gaslight.transform);
            globe.transform.localPosition = Vector3.zero;

            // Add a light
            GameObject lightObj = new GameObject("GaslightLight");
            lightObj.transform.SetParent(globe.transform);
            lightObj.transform.localPosition = Vector3.zero;
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 10f;
            light.intensity = 0.8f;
            light.color = new Color(1.0f, 0.8f, 0.6f);
        }

        /// <summary>
        /// Creates an empty GameObject with the specified name
        /// </summary>
        private static GameObject CreateEmptyWithName(string name, Transform parent = null)
        {
            GameObject obj = new GameObject(name);
            if (parent != null)
            {
                obj.transform.SetParent(parent);
                obj.transform.localPosition = Vector3.zero;
            }
            return obj;
        }
    }
}

