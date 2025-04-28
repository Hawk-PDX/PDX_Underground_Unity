using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
// Use Unity's SceneManagement explicitly to avoid ambiguity
using UnitySceneManagement = UnityEngine.SceneManagement;
// Forward declaration for GaslightFlicker to prevent circular dependency
using PDXUnderground.Core.Interfaces;
using PDXUnderground.Core.Models;
using PDXUnderground.Core.Utils;

namespace PDXUnderground.Core
{
    /// <summary>
    /// Core Game Manager for PDX Underground
    /// Handles initialization, system startup sequence and game state management.
    /// </summary>
    public class GameManager : MonoBehaviour, IEnvironmentSystem
    {
        #region Singleton Setup
        
        // Singleton instance
        private static GameManager _instance;
        
        // Public accessor with lazy initialization
        public static GameManager Instance 
        { 
            get 
            {
                // If instance doesn't exist and we're in play mode
                if (_instance == null && Application.isPlaying)
                {
                    // Look for existing instance in scene
                    _instance = FindObjectOfType<GameManager>();
                    
                    // If no instance exists, create one
                    if (_instance == null)
                    {
                        GameObject managerObject = new GameObject("GameManager");
                        _instance = managerObject.AddComponent<GameManager>();
                        DontDestroyOnLoad(managerObject);
                        Debug.Log("GameManager created as new GameObject");
                    }
                }
                return _instance;
            }
        }
        
        // Ensure only one instance exists
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("Multiple GameManager instances detected. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize core systems
            InitializeCoreSystems();
        }
        
        #endregion
        
        #region Public Properties
        
        // Environment system properties
        public EnvironmentType CurrentEnvironmentType { get; private set; } = EnvironmentType.Streets;
        public TimeOfDay CurrentTimeOfDay { get; private set; } = TimeOfDay.Night;

        // Current game state
        [SerializeField] private GameState _currentState = GameState.Initializing;
        public GameState CurrentState => _currentState;
        
        // Weather and time system references
        [SerializeField] private float _timeOfDay = 0.5f; // 0-1 range (0=midnight, 0.5=noon)
        [SerializeField] private float _weatherIntensity = 0.0f; // 0-1 range
        
        public float TimeOfDay
        {
            get => _timeOfDay;
            set
            {
                _timeOfDay = Mathf.Clamp01(value);
                OnTimeOfDayChanged?.Invoke(_timeOfDay);
            }
        }
        
        public float WeatherIntensity
        {
            get => _weatherIntensity;
            set
            {
                _weatherIntensity = Mathf.Clamp01(value);
                OnWeatherChanged?.Invoke(_weatherIntensity);
                UpdateAllGaslights();
            }
        }
        
        // Scene management
        [SerializeField] private string _startupScene = "MainMenu";
        [SerializeField] private string _currentSceneName;
        public string CurrentSceneName => _currentSceneName;
        
        #endregion
        
        #region Events
        
        // Game state events
        public event Action<GameState> GameStateChanged;
        
        // Environment events - IEnvironmentSystem
        public event Action<EnvironmentType> OnEnvironmentChanged;
        public event Action<string, int> OnEnvironmentTypeChanged;
        public event Action<TimeOfDay> OnTimeOfDayChanged;
        public event Action<float> OnWeatherChanged;
        // Scene management events
        public event Action<string> OnSceneLoadStarted;
        public event Action<string> OnSceneLoadCompleted;
        
        // System initialization events
        public event Action OnSystemsInitialized;
        public event Action OnGameReady;
        
        #endregion
        
        #region Private Fields
        
        // System initialization flags
        private bool _coreSystemsInitialized = false;
        private bool _sceneSystemsInitialized = false;
        private bool _effectsInitialized = false;
        private List<MonoBehaviour> _gaslightFlickers = new List<MonoBehaviour>();
        // Initialization error tracking
        private List<string> _initializationErrors = new List<string>();
        
        #endregion
        
        #region Unity Lifecycle Methods
        
        private void Start()
        {
            if (UnitySceneManagement.SceneManager.sceneCount == 1 && UnitySceneManagement.SceneManager.GetActiveScene().name == "Init")
            {
                LoadScene(_startupScene);
            }
            else
            {
                // Otherwise track current scene
                _currentSceneName = UnitySceneManagement.SceneManager.GetActiveScene().name;
                // Initialize scene-specific systems
                StartCoroutine(InitializeSceneSystems());
            }
        }
        
        private void OnEnable()
        {
            // Subscribe to scene loading events using explicit UnitySceneManagement
            UnitySceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void OnDisable()
        {
            // Unsubscribe from scene loading events using explicit UnitySceneManagement
            UnitySceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        #endregion
        
        #region Initialization Methods
        
        /// <summary>
        /// Initializes core game systems that persist across scenes
        /// </summary>
        private void InitializeCoreSystems()
        {
            Debug.Log("Initializing core systems...");
            
            try
            {
                // TODO: Add initialization for core systems like:
                // - Player profile/save system
                // - Audio system
                // - Input system
                // - Achievement system
                
                _coreSystemsInitialized = true;
                Debug.Log("Core systems initialized successfully");
            }
            catch (Exception e)
            {
                LogError("Failed to initialize core systems", e);
                _coreSystemsInitialized = false;
            }
        }
        
        /// <summary>
        /// Initializes systems specific to the current scene
        /// </summary>
        private IEnumerator InitializeSceneSystems()
        {
            try
            {
                // Initialize scene-specific systems
                yield return StartCoroutine(InitializeEnvironmentSystems());
                yield return StartCoroutine(InitializeEffectsSystems());
                
                _sceneSystemsInitialized = true;
                Debug.Log("Scene systems initialized successfully");
                
                // Change game state to appropriate state based on scene
                if (_currentSceneName == "MainMenu")
                {
                    ChangeGameState(GameState.MainMenu);
                }
                else
                {
                    ChangeGameState(GameState.Playing);
                }
                
                // Notify systems initialization complete
                OnSystemsInitialized?.Invoke();
                
                // Final setup and notification that game is ready
                if (_coreSystemsInitialized && _sceneSystemsInitialized)
                {
                    Debug.Log("Game initialization complete - game is ready");
                    OnGameReady?.Invoke();
                }
                else
                {
                    Debug.LogError("Game initialization failed - see previous errors");
                }
            }
            catch (Exception e)
            {
                LogError($"Failed to initialize scene systems for {_currentSceneName}", e);
                _sceneSystemsInitialized = false;
            }
        }
        
        /// <summary>
        /// Initializes environment systems (time, weather, etc.)
        /// </summary>
        private IEnumerator InitializeEnvironmentSystems()
        {
            Debug.Log("Initializing environment systems...");
            yield return null;
            
            try
            {
                // TODO: Initialize environment-specific systems
                // - Weather system
                // - Time system
                // - NPC systems
            }
            catch (Exception e)
            {
                LogError("Failed to initialize environment systems", e);
            }
            
            yield return null;
        }
        
        /// <summary>
        /// Initializes visual effects and lighting systems
        /// </summary>
        private IEnumerator InitializeEffectsSystems()
        {
            Debug.Log("Initializing effects systems...");
            _effectsInitialized = false;
            
            try
            {
                // Find all gaslight flickers in the scene
                _gaslightFlickers.Clear();
                var flickers = FindObjectsOfType<MonoBehaviour>()
                    .Where(mb => mb.GetType().Name == "GaslightFlicker")
                    .ToArray();
                if (flickers != null && flickers.Length > 0)
                {
                    foreach (var flicker in flickers)
                    {
                        _gaslightFlickers.Add(flicker as MonoBehaviour);
                    }
                    
                    Debug.Log($"Found {_gaslightFlickers.Count} GaslightFlicker components");
                    
                    // Allow all flickers to find neighbors first
                    yield return new WaitForSeconds(0.2f);
                    
                    // Set initial time of day on all lights
                    UpdateAllGaslights();
                }
                
                _effectsInitialized = true;
            }
            catch (Exception e)
            {
                LogError("Failed to initialize effects systems", e);
                _effectsInitialized = false;
            }
            
            yield return null;
        }
        
        #endregion
        
        #region Game State Management
        
        /// <summary>
        /// Changes the game state and broadcasts the change
        /// </summary>
        public void ChangeGameState(GameState newState)
        {
            if (_currentState == newState)
                return;
                
            GameState previousState = _currentState;
            _currentState = newState;
            
            Debug.Log($"Game state changed: {previousState} -> {newState}");
            
            // Perform state transition logic
            switch (newState)
            {
                case GameState.MainMenu:
                    // Handle transition to main menu
                    break;
                    
                case GameState.Loading:
                    // Handle loading state
                    break;
                    
                case GameState.Playing:
                    // Handle transition to gameplay
                    break;
                    
                case GameState.Paused:
                    // Handle game pause
                    Time.timeScale = 0f;
                    break;
                    
                case GameState.GameOver:
                    // Handle game over state
                    break;
            }
            // Notify listeners of state change
            GameStateChanged?.Invoke(newState);
        }
        /// <summary>
        /// Pauses or unpauses the game
        /// </summary>
        public void TogglePause()
        {
            if (_currentState == GameState.Playing)
            {
                ChangeGameState(GameState.Paused);
            }
            else if (_currentState == GameState.Paused)
            {
                ChangeGameState(GameState.Playing);
                Time.timeScale = 1f;
            }
        }
        
        #endregion
        
        #region Scene Management
        
        /// <summary>
        /// Handles scene loaded events
        /// </summary>
        private void OnSceneLoaded(UnitySceneManagement.Scene scene, UnitySceneManagement.LoadSceneMode mode)
        {
            _currentSceneName = scene.name;
            Debug.Log($"Scene loaded: {_currentSceneName}");
            
            // Initialize scene systems
            StartCoroutine(InitializeSceneSystems());
            
            // Notify listeners
            OnSceneLoadCompleted?.Invoke(_currentSceneName);
        }
        
        /// <summary>
        /// Loads a new scene
        /// </summary>
        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }
        
        /// <summary>
        /// Loads a scene asynchronously with optional loading screen
        /// </summary>
        private IEnumerator LoadSceneAsync(string sceneName)
        {
            // Change state to loading
            ChangeGameState(GameState.Loading);
            
            // Notify listeners that loading has started
            OnSceneLoadStarted?.Invoke(sceneName);
            // TODO: Show loading screen if needed
            
            
            // Start async loading using explicit UnitySceneManagement
            AsyncOperation asyncLoad = UnitySceneManagement.SceneManager.LoadSceneAsync(sceneName);
            // Wait until the scene is almost ready
            while (asyncLoad.progress < 0.9f)
            {
                // Update loading progress
                // TODO: Update loading UI
                
                yield return null;
            }
            
            // Short delay for better user experience
            yield return new WaitForSeconds(0.5f);
            
            // Activate the scene
            asyncLoad.allowSceneActivation = true;
            
            // Wait for scene to fully load
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }
        
        #endregion
        
        #region Environment Management
        
        /// <summary>
        /// Updates the time of day and triggers related events
        /// </summary>
        /// <param name="newTime">Time value (0-1 range, 0=midnight, 0.5=noon)</param>
        // TimeOfDay methods from IEnvironmentSystem
        public void SetTimeOfDay(TimeOfDay time)
        {
            if (CurrentTimeOfDay != time)
            {
                CurrentTimeOfDay = time;
                OnTimeOfDayChanged?.Invoke(time);
                
                // Convert enum to float time (0-1) for backwards compatibility
                float timeValue = TimeUtils.TimeOfDayToFloat(time);
                TimeOfDay = timeValue;
                UpdateAllGaslights();
            }
        }
        
        // Original SetTimeOfDay, kept for backward compatibility
        public void SetTimeOfDay(float newTime)
        {
            TimeOfDay = newTime;
            
            // Update the enum version too
            CurrentTimeOfDay = TimeUtils.FloatToTimeOfDay(newTime);
            OnTimeOfDayChanged?.Invoke(CurrentTimeOfDay);
            UpdateAllGaslights();
        }
        
        // Conversion methods moved to TimeUtils class
        
        /// <summary>
        /// Updates the weather intensity and triggers related events
        /// </summary>
        /// <param name="intensity">Weather intensity (0-1 range)</param>
        // Weather intensity is now handled directly through the WeatherIntensity property
        // which calls UpdateAllGaslights() when set
        
        /// <summary>
        /// Updates all gaslights with current time and weather settings
        /// </summary>
        private void UpdateAllGaslights()
        {
            if (_gaslightFlickers == null || _gaslightFlickers.Count == 0)
                return;
                
            // Convert time of day to gaslight intensity based on day/night cycle
            // In gas lamps, time factor should be higher at night, lower during day
            float lightTimeFactor = CalculateLightTimeFactor(_timeOfDay);
            
            // Apply to all gas lamps
            foreach (var gaslight in _gaslightFlickers)
            {
                if (gaslight != null)
                {
                    // Use SendMessage approach instead of direct method calls
                    gaslight.SendMessage("SetTimeOfDay", lightTimeFactor, SendMessageOptions.DontRequireReceiver);
                    gaslight.SendMessage("SetWeatherEffect", 1.0f + _weatherIntensity, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
        
        /// <summary>
        /// Calculates light intensity factor based on time of day
        /// </summary>
        private float CalculateLightTimeFactor(float timeOfDay)
        {
            // This is simplified - in a full implementation you might use a curve
            
            // Determine how far from noon (0.5) we are
            float distanceFromNoon = Mathf.Abs(timeOfDay - 0.5f) * 2.0f; // 0 at noon, 1 at midnight
            
            // Late evening through early morning (higher light intensity)
            if (timeOfDay < 0.25f || timeOfDay > 0.75f)
            {
                return Mathf.Lerp(0.8f, 1.0f, distanceFromNoon);
            }
            // During daytime (lower light intensity)
            else
            {
                return Mathf.Lerp(0.2f, 0.5f, 1.0f - distanceFromNoon);
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Logs an error with optional exception details
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="exception">Optional exception</param>
        private void LogError(string message, Exception exception = null)
        {
            string errorMsg = message;
            
            if (exception != null)
            {
                errorMsg += $" Exception: {exception.Message}\nStack Trace: {exception.StackTrace}";
            }
            
            Debug.LogError(errorMsg);
            _initializationErrors.Add(errorMsg);
        }
        
        /// <summary>
        /// Returns a list of all initialization errors
        /// </summary>
        public List<string> GetInitializationErrors()
        {
            return new List<string>(_initializationErrors);
        }
        
        /// <summary>
        /// Finds new GaslightFlicker components that might have been added dynamically
        /// </summary>
        public void RefreshGaslightComponents()
        {
            StartCoroutine(RefreshGaslightComponentsAsync());
        }
        
        /// <summary>
        /// Asynchronously finds and refreshes all GaslightFlicker components in the scene
        /// </summary>
        private IEnumerator RefreshGaslightComponentsAsync()
        {
            var flickers = FindObjectsOfType<MonoBehaviour>()
                .Where(mb => mb.GetType().Name == "GaslightFlicker")
                .ToArray();
            // Build lookup of existing references to avoid duplicates
            HashSet<MonoBehaviour> existingFlickers = new HashSet<MonoBehaviour>();
            foreach (var flicker in _gaslightFlickers)
            {
                if (flicker != null)
                {
                    existingFlickers.Add(flicker);
                }
            }
            
            // Remove any null references from original list
            _gaslightFlickers.RemoveAll(f => f == null);
            
            // Add any new flickers
            int newCount = 0;
            foreach (var flicker in flickers)
            {
                if (!existingFlickers.Contains(flicker))
                {
                    _gaslightFlickers.Add(flicker);
                    newCount++;
                }
            }
            
            Debug.Log($"Found {newCount} new GaslightFlicker components");
            
            // Allow new flickers to find neighbors
            yield return new WaitForSeconds(0.2f);
            
            // Update all gaslights with current settings
            UpdateAllGaslights();
        }
        
        #endregion
        #region IEnvironmentSystem Implementation

        /// <summary>
        /// Sets the current environment type and notifies listeners of the change
        /// </summary>
        /// <param name="type">The new environment type to set</param>
        public void SetEnvironment(EnvironmentType type)
        {
            if (CurrentEnvironmentType != type)
            {
                CurrentEnvironmentType = type;
                OnEnvironmentChanged?.Invoke(type);
                OnEnvironmentTypeChanged?.Invoke(GetCurrentEnvironmentName(), (int)type);
            }
        }

        /// <summary>
        /// Changes the environment based on its index in the EnvironmentType enum
        /// </summary>
        /// <param name="environmentIndex">The index corresponding to the desired EnvironmentType</param>
        public void ChangeEnvironment(int environmentIndex)
        {
            if (System.Enum.IsDefined(typeof(EnvironmentType), environmentIndex))
            {
                SetEnvironment((EnvironmentType)environmentIndex);
            }
        }

        /// <summary>
        /// Gets the name of the current environment
        /// </summary>
        /// <returns>String representation of the current environment</returns>
        public string GetCurrentEnvironmentName()
        {
            return CurrentEnvironmentType.ToString();
        }

        /// <summary>
        /// Gets a description for the specified environment type
        /// </summary>
        /// <param name="environmentType">The environment type index</param>
        /// <returns>A descriptive string for the environment type</returns>
        public string GetEnvironmentDescription(int environmentType)
        {
            if (System.Enum.IsDefined(typeof(EnvironmentType), environmentType))
            {
                switch ((EnvironmentType)environmentType)
                {
                    case EnvironmentType.Streets:
                        return "Historical Portland streets";
                    case EnvironmentType.Tunnels:
                        return "Underground Shanghai Tunnels";
                    case EnvironmentType.Speakeasy:
                        return "Hidden Speakeasy";
                    default:
                        return "Unknown area";
                }
            }
            return "Unknown";
        }

        /// <summary>
        /// Gets an icon representing the specified environment type
        /// </summary>
        /// <param name="environmentType">The environment type index</param>
        /// <returns>A sprite representing the environment or null if not found</returns>
        public Sprite GetEnvironmentIcon(int environmentType)
        {
            // Implementation depends on your asset system
            return null;
        }

        /// <summary>
        /// Gets a string representation of the current time of day
        /// </summary>
        /// <returns>String representation of the current time of day</returns>
        public string GetTimeOfDay()
        {
            return CurrentTimeOfDay.ToString();
        }

        /// <summary>
        /// Gets a string description of the current weather conditions
        /// </summary>
        /// <returns>A descriptive weather condition string</returns>
        public string GetWeatherString()
            if (_weatherIntensity > 0.8f) return "Stormy";
            if (_weatherIntensity > 0.5f) return "Rainy";
            if (_weatherIntensity > 0.2f) return "Cloudy";
            return "Clear";
        }
        }
        
        #endregion // End of IEnvironmentSystem Implementation
        
        #region Cleanup Methods
        /// <summary>
        /// Handles cleanup of all game systems when quitting or changing scenes
        /// </summary>
        public void CleanupSystems()
        {
            CleanupEffectsSystems();
            CleanupEnvironmentSystems();
            CleanupSceneSystems();
        }
        
        /// <summary>
        /// Cleanup for scene-specific systems
        /// </summary>
        private void CleanupSceneSystems()
        {
            Debug.Log("Cleaning up scene-specific systems...");
            
            try
            {
                // TODO: Add cleanup code for scene-specific systems
                // - Release references to scene-specific objects
                // - Stop any scene-specific coroutines
                // - Unsubscribe from scene-specific events
            }
            catch (Exception e)
            {
                LogError("Error during scene cleanup", e);
            }
        }
        
        /// <summary>
        /// Cleanup for environment systems
        /// </summary>
        private void CleanupEnvironmentSystems()
        {
            Debug.Log("Cleaning up environment systems...");
            
            try
            {
                // TODO: Add cleanup code for environment systems
                // - Stop any active weather effects
                // - Save time/weather state if needed
            }
            catch (Exception e)
            {
                LogError("Error during environment systems cleanup", e);
            }
        }
        
        /// <summary>
        /// Cleanup for effects systems
        /// </summary>
        private void CleanupEffectsSystems()
        {
            Debug.Log("Cleaning up effects systems...");
            
            try
            {
                // Reset gaslights if needed
                foreach (var gaslight in _gaslightFlickers)
                {
                    if (gaslight != null)
                    {
                        // Optional: Reset to default state
                        // gaslight.SetTimeOfDay(0.5f);
                        // gaslight.SetWeatherEffect(1.0f);
                    }
                }
                
                // Clear references without destroying objects
                _gaslightFlickers.Clear();
                _effectsInitialized = false;
            }
            catch (Exception e)
            {
                LogError("Error during effects systems cleanup", e);
            }
        }
        
        /// <summary>
        /// Cleanup when the application is quitting
        /// </summary>
        private void OnApplicationQuit()
        {
            Debug.Log("Application is quitting - cleaning up systems");
            CleanupSystems();
        }
        
        #endregion
    }
}
