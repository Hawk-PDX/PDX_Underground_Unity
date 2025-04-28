using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Use Unity's SceneManagement explicitly to avoid ambiguity
using UnitySceneManagement = UnityEngine.SceneManagement;

namespace PDXUnderground.Core
{
    /// <summary>
    /// Core Game Manager for PDX Underground
    /// Handles initialization, system startup sequence and game state management.
    /// </summary>
    public class GameManager : MonoBehaviour
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
        
        // Game state enum
        public enum GameState
        {
            Initializing,
            MainMenu,
            Loading,
            Playing,
            Paused,
            GameOver
        }
        
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
            }
        }
        
        // Scene management
        [SerializeField] private string _startupScene = "MainMenu";
        [SerializeField] private string _currentSceneName;
        public string CurrentSceneName => _currentSceneName;
        
        #endregion
        
        #region Events
        
        // Game state events
        public event Action<GameState> OnGameStateChanged;
        
        // Environment events
        public event Action<float> OnTimeOfDayChanged;
        public event Action<float> OnWeatherChanged;
        
        // Scene management events
        public event Action<string> OnSceneLoadStarted;
        public event Action<string> OnSceneLoadCompleted;
        
        // System initialization events
        public event Action OnSystemsInitialized;
        public event Action OnGameReady;
        
        // Time management event
        public UnityEngine.Events.UnityEvent OnGameTimeUpdate = new UnityEngine.Events.UnityEvent();
        
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
            
            // Start time update coroutine
            StartCoroutine(GameTimeUpdateCoroutine());
        }
        
        /// <summary>
        /// Coroutine to regularly update game time and trigger time-related events
        /// </summary>
        private IEnumerator GameTimeUpdateCoroutine()
        {
            while (true)
            {
                // Update time of day
                UpdateGameTime();
                
                // Trigger the time update event
                OnGameTimeUpdate?.Invoke();
                
                // Wait for next update
                yield return new WaitForSeconds(1.0f);
            }
        }
        
        /// <summary>
        /// Updates the game time (called regularly by the coroutine)
        /// </summary>
        private void UpdateGameTime()
        {
            // Update time of day (cycles from 0 to 1)
            float timeIncrement = Time.deltaTime / (24f * 60f); // One full day cycle takes 24 real-time minutes
            TimeOfDay = (TimeOfDay + timeIncrement) % 1.0f;
        }
        
        #endregion

        #region Core System Methods

        /// <summary>
        /// Initializes core game systems that persist across scenes
        /// </summary>
        private void InitializeCoreSystems()
        {
            Debug.Log("Initializing core systems...");
            
            try
            {
                // Initialize core systems
                
                // Set up time management
                // Note: GameTimeUpdateCoroutine is already started in Start()
                
                Debug.Log("Core systems initialized successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to initialize core systems: {e.Message}");
            }
        }

        /// <summary>
        /// Initialize scene systems after a scene is loaded
        /// </summary>
        private IEnumerator InitializeSceneSystems()
        {
            yield return null; // Wait one frame to ensure scene is loaded
            
            // Initialize systems specific to this scene
            // TODO: Add scene-specific initialization
            
            // Change state based on scene
            if (_currentSceneName == "MainMenu")
            {
                ChangeGameState(GameState.MainMenu);
            }
            else
            {
                ChangeGameState(GameState.Playing);
            }
            
            // Notify that systems are initialized
            OnSystemsInitialized?.Invoke();
            
            // Notify that game is ready
            OnGameReady?.Invoke();
        }

        #endregion

        #region Game State Methods

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
            OnGameStateChanged?.Invoke(newState);
        }

        #endregion

        #region Scene Management Methods
        
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
            
            // Start async loading using explicit UnitySceneManagement
            AsyncOperation asyncLoad = UnitySceneManagement.SceneManager.LoadSceneAsync(sceneName);
            
            // Wait until the scene is fully loaded
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            
            // Update current scene name
            _currentSceneName = sceneName;
            
            // Notify listeners that loading has completed
            OnSceneLoadCompleted?.Invoke(sceneName);
        }
        
        #endregion
    }
}
