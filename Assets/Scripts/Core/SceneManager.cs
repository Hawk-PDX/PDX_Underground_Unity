using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using PDXUnderground.Core.Interfaces;

namespace PDXUnderground.Core
{
    public class SceneManager : MonoBehaviour, ISceneTransitionManager
    {
        [Header("Scene References")]
        [SerializeField] private float transitionDuration = 1.5f;
        [SerializeField] private Transform defaultSpawnPoint;
        [SerializeField] private Transform[] streetSpawnPoints;
        [SerializeField] private Transform[] tunnelSpawnPoints;
        [SerializeField] private Transform[] portSpawnPoints;

        [Header("UI References")]
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private float crossFadeDuration = 1f;

        [Header("Events")]
        [SerializeField] private UnityEvent onSceneStart;
        [SerializeField] private UnityEvent onSceneEnd;

        private bool isTransitioning = false;
        private Dictionary<string, bool> loadedScenes = new Dictionary<string, bool>();
        private IUIManager uiManager;
        private IEnvironmentSetup environmentSetup;
        private ILoadingScreen loadingScreen;

        public bool IsTransitioning => isTransitioning;

        private void Awake()
        {
            InitializeSceneManager();
        }

        private void InitializeSceneManager()
        {
            DontDestroyOnLoad(gameObject);

            // Initialize scene dictionary
            loadedScenes[SceneTypes.STREETS_SCENE] = false;
            loadedScenes[SceneTypes.TUNNELS_SCENE] = false;
            loadedScenes[SceneTypes.PORT_SCENE] = false;

            // Find required interfaces
            FindRequiredInterfaces();

            // Subscribe to scene events
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void FindRequiredInterfaces()
        {
            uiManager = FindObjectOfType<MonoBehaviour>() as IUIManager;
            environmentSetup = FindObjectOfType<MonoBehaviour>() as IEnvironmentSetup;
            loadingScreen = FindObjectOfType<MonoBehaviour>() as ILoadingScreen;

            if (uiManager == null)
                Debug.LogWarning("SceneManager: No IUIManager implementation found");
            if (environmentSetup == null)
                Debug.LogWarning("SceneManager: No IEnvironmentSetup implementation found");
            if (loadingScreen == null)
                Debug.LogWarning("SceneManager: No ILoadingScreen implementation found");
        }

        private void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (loadedScenes.ContainsKey(scene.name))
                loadedScenes[scene.name] = true;

            FindRequiredInterfaces();
            onSceneStart?.Invoke();
        }

        // ISceneTransitionManager implementation
        public void LoadNextScene()
        {
            string current = CurrentlyActiveScene();
            if (current == SceneTypes.STREETS_SCENE)
                TransitionToTunnels(Vector3.zero);
            else if (current == SceneTypes.TUNNELS_SCENE)
                TransitionToPort(Vector3.zero);
            else
                TransitionToStreets(Vector3.zero);
        }

        public void LoadScene(string sceneName)
        {
            if (!loadedScenes.ContainsKey(sceneName) || isTransitioning)
                return;

            Vector3 spawnPoint = GetSpawnPointForScene(sceneName);
            StartCoroutine(HandleSceneTransition(CurrentlyActiveScene(), sceneName, spawnPoint, transitionDuration));
        }

        public void TransitionToTunnels(Vector3 playerPosition)
        {
            if (isTransitioning) return;
            StartCoroutine(HandleSceneTransition(
                CurrentlyActiveScene(),
                SceneTypes.TUNNELS_SCENE,
                playerPosition == Vector3.zero ? GetDefaultSpawnPoint(SceneTypes.TUNNELS_SCENE) : playerPosition,
                transitionDuration
            ));
        }

        public void TransitionToPort(Vector3 playerPosition)
        {
            if (isTransitioning) return;
            StartCoroutine(HandleSceneTransition(
                CurrentlyActiveScene(),
                SceneTypes.PORT_SCENE,
                playerPosition == Vector3.zero ? GetDefaultSpawnPoint(SceneTypes.PORT_SCENE) : playerPosition,
                transitionDuration
            ));
        }

        public void TransitionToStreets(Vector3 playerPosition)
        {
            if (isTransitioning) return;
            StartCoroutine(HandleSceneTransition(
                CurrentlyActiveScene(),
                SceneTypes.STREETS_SCENE,
                playerPosition == Vector3.zero ? GetDefaultSpawnPoint(SceneTypes.STREETS_SCENE) : playerPosition,
                transitionDuration
            ));
        }

        public Vector3 GetSpawnPointForScene(string sceneName, Vector3 nearPosition = default)
        {
            if (environmentSetup != null && nearPosition != default)
            {
                Vector3 spawnPoint = environmentSetup.GetNearestSpawnPoint(nearPosition);
                if (spawnPoint != Vector3.zero) return spawnPoint;
            }

            Transform[] spawnPoints = GetSpawnPointsForScene(sceneName);
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                if (nearPosition != default)
                    return GetNearestSpawnPoint(spawnPoints, nearPosition);
                return GetFirstValidSpawnPoint(spawnPoints);
            }

            return GetDefaultSpawnPoint(sceneName);
        }

        public void UpdateAreaName(string areaName)
        {
            if (uiManager != null)
                uiManager.ShowAreaName(areaName);
        }

        private string GetAreaNameForScene(string sceneName)
        {
            if (sceneName == SceneTypes.STREETS_SCENE) return SceneTypes.STREETS_AREA;
            if (sceneName == SceneTypes.TUNNELS_SCENE) return SceneTypes.TUNNELS_AREA;
            if (sceneName == SceneTypes.PORT_SCENE) return SceneTypes.PORT_AREA;
            return "Unknown Area";
        }

        private IEnumerator HandleSceneTransition(string fromScene, string toScene, Vector3 spawnPoint, float duration)
        {
            isTransitioning = true;
            onSceneEnd?.Invoke();

            if (fadeCanvasGroup != null)
                yield return StartCoroutine(FadeOut());

            if (loadingScreen != null)
            {
                loadingScreen.Show(true);
                loadingScreen.SetLoadingText($"Loading {GetAreaNameForScene(toScene)}...");
            }

            AsyncOperation operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(toScene);
            while (!operation.isDone)
            {
                if (loadingScreen != null)
                    loadingScreen.UpdateProgress(operation.progress);
                yield return null;
            }

            SpawnPlayerAtPoint(spawnPoint);

            if (loadingScreen != null)
                loadingScreen.Show(false);

            if (fadeCanvasGroup != null)
                yield return StartCoroutine(FadeIn());

            isTransitioning = false;
        }

        private void SpawnPlayerAtPoint(Vector3 position)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
                player.transform.position = position;
                controller.enabled = true;
            }
            else
            {
                player.transform.position = position;
            }
        }

        private Transform[] GetSpawnPointsForScene(string sceneName)
        {
            if (sceneName == SceneTypes.STREETS_SCENE) return streetSpawnPoints;
            if (sceneName == SceneTypes.TUNNELS_SCENE) return tunnelSpawnPoints;
            if (sceneName == SceneTypes.PORT_SCENE) return portSpawnPoints;
            return null;
        }

        private Vector3 GetDefaultSpawnPoint(string sceneName)
        {
            return defaultSpawnPoint != null ? defaultSpawnPoint.position : new Vector3(0, 0.5f, 0);
        }

        private string CurrentlyActiveScene()
        {
            foreach (var scene in loadedScenes)
                if (scene.Value) return scene.Key;
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        }

        private Vector3 GetNearestSpawnPoint(Transform[] spawnPoints, Vector3 position)
        {
            Transform nearest = null;
            float nearestDist = float.MaxValue;

            foreach (Transform spawn in spawnPoints)
            {
                if (spawn == null) continue;
                float dist = Vector3.Distance(position, spawn.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = spawn;
                }
            }

            return nearest != null ? nearest.position : GetDefaultSpawnPoint(CurrentlyActiveScene());
        }

        private Vector3 GetFirstValidSpawnPoint(Transform[] spawnPoints)
        {
            foreach (Transform spawn in spawnPoints)
                if (spawn != null) return spawn.position;
            return GetDefaultSpawnPoint(CurrentlyActiveScene());
        }

        private IEnumerator FadeOut()
        {
            float elapsed = 0;
            fadeCanvasGroup.blocksRaycasts = true;

            while (elapsed < crossFadeDuration)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = fadeCurve.Evaluate(elapsed / crossFadeDuration);
                yield return null;
            }

            fadeCanvasGroup.alpha = 1;
        }

        private IEnumerator FadeIn()
        {
            float elapsed = 0;

            while (elapsed < crossFadeDuration)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = 1 - fadeCurve.Evaluate(elapsed / crossFadeDuration);
                yield return null;
            }

            fadeCanvasGroup.alpha = 0;
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }
}