using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PDXUnderground.Core
{
    /// <summary>
    /// Main controller for the PDX Underground game. Manages game state, environment, and buzz level.
    /// Provides comprehensive game flow control and cross-cutting concerns management.
    /// Implemented as a singleton to allow easy access from other scripts.
    /// </summary>
    public class MainGameController : MonoBehaviour
    {
        #region Singleton Pattern
        // Singleton instance
        private static MainGameController _instance;
        
        // Public accessor for the singleton instance
        public static MainGameController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<MainGameController>();
                    
                    if (_instance == null)
                    {
                        Debug.LogWarning("No MainGameController found in scene. Creating a default instance.");
                        GameObject controllerObject = new GameObject("MainGameController");
                        _instance = controllerObject.AddComponent<MainGameController>();
                    }
                }
                
                return _instance;
            }
        }
        
        // Make sure we don't create duplicate controllers
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("Duplicate MainGameController found. Destroying the newer one.");
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize the controller
            InitializeGameSystems();
        }
        #endregion
        
        #region Properties and References
        [Header("UI References")]
        [SerializeField] private BuzzUIController buzzUIController;
        
        [Header("Character References")]
        [SerializeField] private GamblerCharacter playerCharacter;
        #endregion

        #region Game State Management
        // Combined game state enum with all states from both versions
        public enum GameState
        {
            MainMenu,
            Loading,
            Playing,
            Paused,
            CardSelection,
            Dialogue,
            GameOver,
            Victory
        }
        
        // Current game state
        [SerializeField] private GameState _currentState = GameState.MainMenu;
        
        // Property to get/set the current game state
        public GameState CurrentState
        {
            get { return _currentState; }
            set
            {
                if (_currentState != value)
                {
                    GameState prevState = _currentState;
                    _currentState = value;
                    HandleStateTransition(prevState, _currentState);
                    OnGameStateChanged?.Invoke(prevState, _currentState);
                }
            }
        }
        
        // Enhanced game state change event
        public delegate void GameStateChangedHandler(GameState previousState, GameState newState);
        public event GameStateChangedHandler OnGameStateChanged;
        
        private void HandleStateTransition(GameState previousState, GameState newState)
        {
            switch (newState)
            {
                case GameState.MainMenu:
                    Time.timeScale = 1f;
                    break;
                    
                case GameState.Loading:
                    // Handle loading state
                    break;
                    
                case GameState.Playing:
                    Time.timeScale = 1f;
                    if (previousState == GameState.Paused)
                    {
                        // Resuming from pause
                    }
                    else if (previousState == GameState.MainMenu)
                    {
                        // Starting new game
                        ResetGameState();
                    }
                    break;
                    
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                    
                case GameState.CardSelection:
                    // Handle card selection state
                    break;
                    
                case GameState.Dialogue:
                    // Handle dialogue state
                    break;
                    
                case GameState.GameOver:
                    // Handle game over state
                    break;
                    
                case GameState.Victory:
                    // Handle victory state
                    break;
            }
            
            Debug.Log($"Game State changed from {previousState} to {newState}");
        }
        #endregion
        
        #region Environment Management
        [Header("Environment")]
        [SerializeField] private int currentEnvironmentIndex = 0;
        [SerializeField] private string[] environmentTypes = { "Streets", "Tunnels", "Speakeasy" };
        
        public enum Environment
        {
            Streets,
            Tunnels,
            Speakeasy
        }
        
        // Current environment
        [SerializeField] private Environment _currentEnvironment = Environment.Streets;
        
        // Property to get/set the current environment
        public Environment CurrentEnvironment
        {
            get { return _currentEnvironment; }
            private set
            {
                if (_currentEnvironment != value)
                {
                    Environment prevEnvironment = _currentEnvironment;
                    _currentEnvironment = value;
                    OnEnvironmentChanged?.Invoke(prevEnvironment, _currentEnvironment);
                    
                    // Notify with additional environment details
                    string environmentName = environmentTypes[currentEnvironmentIndex];
                    OnEnvironmentTypeChanged?.Invoke(environmentName, (int)value);
                }
            }
        }
        
        // Events for environment changes
        public delegate void EnvironmentChangedHandler(Environment previousEnvironment, Environment newEnvironment);
        public event EnvironmentChangedHandler OnEnvironmentChanged;
        
        public delegate void EnvironmentTypeChangedHandler(string environmentName, int environmentIndex);
        public event EnvironmentTypeChangedHandler OnEnvironmentTypeChanged;
        
        /// <summary>
        /// Changes the current environment and notifies all relevant systems
        /// </summary>
        public void ChangeEnvironment(int environmentIndex)
        {
            if (environmentIndex < 0 || environmentIndex >= environmentTypes.Length)
            {
                Debug.LogError($"Invalid environment index: {environmentIndex}");
                return;
            }
            
            currentEnvironmentIndex = environmentIndex;
            CurrentEnvironment = (Environment)environmentIndex;
            
            // Update UI
            if (buzzUIController != null)
            {
                Debug.Log($"Updated UI visuals to {environmentTypes[environmentIndex]} theme");
            }
            
            // Apply environment effects to player
            if (playerCharacter != null)
            {
                ApplyEnvironmentEffects();
            }
        }
        
        private void ApplyEnvironmentEffects()
        {
            switch (CurrentEnvironment)
            {
                case Environment.Streets:
                    // Normal gameplay
                    break;
                case Environment.Tunnels:
                    // Buzz drains more slowly
                    break;
                case Environment.Speakeasy:
                    // Card abilities are enhanced
                    break;
            }
        }
        #endregion
        
        #region Buzz Management
        // Buzz level (0-100)
        [SerializeField] [Range(0f, 100f)] private float _buzzLevel = 0f;
        
        // Critical buzz level threshold
        [SerializeField] [Range(0f, 100f)] private float _criticalBuzzLevel = 30f;
        
        // Property to get/set the buzz level
        public float BuzzLevel
        {
            get { return _buzzLevel; }
            set
            {
                float prevValue = _buzzLevel;
                _buzzLevel = Mathf.Clamp(value, 0f, 100f);
                
                if (_buzzLevel != prevValue)
                {
                    OnBuzzLevelChanged?.Invoke(prevValue, _buzzLevel);
                    
                    // Check for critical buzz level
                    if (prevValue > _criticalBuzzLevel && _buzzLevel <= _criticalBuzzLevel)
                    {
                        OnExitCriticalBuzz?.Invoke();
                    }
                    else if (prevValue <= _criticalBuzzLevel && _buzzLevel > _criticalBuzzLevel)
                    {
                        OnEnterCriticalBuzz?.Invoke();
                    }
                }
            }
        }
        
        // Property to check if buzz level is critical
        public bool IsBuzzCritical => _buzzLevel > _criticalBuzzLevel;
        
        // Events for buzz level changes
        public event System.Action<float, float> OnBuzzLevelChanged;
        public event System.Action OnEnterCriticalBuzz;
        public event System.Action OnExitCriticalBuzz;
        
        // Methods to modify buzz level
        public void AdjustBuzzLevel(float delta)
        {
            BuzzLevel += delta;
        }
        
        public void SetBuzzLevel(float value)
        {
            BuzzLevel = value;
        }
        #endregion
        
        #region Initialization and Updates
        private void InitializeGameSystems()
        {
            // Find references if not assigned in inspector
            if (buzzUIController == null)
                buzzUIController = FindObjectOfType<BuzzUIController>();
                
            if (playerCharacter == null)
                playerCharacter = FindObjectOfType<GamblerCharacter>();
            
            // Set up default event handlers
            SetupEventHandlers();
            
            Debug.Log("Main Game Controller initialized!");
        }
        
        private void SetupEventHandlers()
        {
            OnGameStateChanged += (prev, current) => 
                Debug.Log($"Game state changed from {prev} to {current}");
                
            OnEnvironmentChanged += (prev, current) => 
                Debug.Log($"Environment changed from {prev} to {current}");
                
            OnBuzzLevelChanged += (prev, current) => 
                Debug.Log($"Buzz level changed from {prev:F1} to {current:F1}");
                
            OnEnterCriticalBuzz += () => 
                Debug.Log("Entered critical buzz state!");
                
            OnExitCriticalBuzz += () => 
                Debug.Log("Exited critical buzz state");
        }
        
        private void Update()
        {
            if (CurrentState == GameState.Playing)
            {
                UpdateGameplay();
            }
        }
        
        private void UpdateGameplay()
        {
            // Time-based buzz level decay
            if (BuzzLevel > 0)
            {
                // Reduce buzz by a small amount over time
                BuzzLevel -= Time.deltaTime * 0.5f;
            }
        }
        #endregion
        
        #region Game Control Methods
        public void StartGame()
        {
            BuzzLevel = 0f;
            CurrentState = GameState.Playing;
            CurrentEnvironment = Environment.Streets;
            Debug.Log("Game started");
        }
        
        public void PauseGame()
        {
            if (CurrentState == GameState.Playing)
            {
                CurrentState = GameState.Paused;
                Debug.Log("Game paused");
            }
        }
        
        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
            {
                CurrentState = GameState.Playing;
                Debug.Log("Game resumed");
            }
        }
        
        public void EndGame()
        {
            CurrentState = GameState.GameOver;
            Debug.Log("Game over");
        }
        
        public void ReturnToMainMenu()
        {
            CurrentState = GameState.MainMenu;
            Debug.Log("Returned to main menu");
        }
        
        private void ResetGameState()
        {
            // Reset player character
            if (playerCharacter != null)
            {
                SetBuzzLevel(0f);
            }
            
            // Reset environment
            ChangeEnvironment(0);
            
            Debug.Log("Game state has been reset for a new game");
        }
        #endregion
        
        #region Testing API
        /// <summary>
        /// Modifies the player's buzz level by the specified amount
        /// </summary>
        public void ModifyPlayerBuzz(float amount)
        {
            float newBuzz = Mathf.Clamp(BuzzLevel + amount, 0f, 100f);
            SetBuzzLevel(newBuzz);
            Debug.Log($"Modified Buzz to {newBuzz}");
        }
        
        /// <summary>
        /// Set player buzz to critical threshold
        /// </summary>
        public void SetPlayerBuzzToCritical()
        {
            float criticalLevel = _criticalBuzzLevel + 0.1f;
            SetBuzzLevel(criticalLevel);
            Debug.Log($"Set Critical Buzz level: {criticalLevel}");
        }
        
        /// <summary>
        /// Triggers the player to draw a card
        /// </summary>
        public void PlayerDrawCard()
        {
            if (CurrentState == GameState.Playing)
            {
                Debug.Log("Player drew a card");
            }
        }
        
        /// <summary>
        /// Triggers the player to use a specific card ability
        /// </summary>
        public void UseCardAbility(string abilityName)
        {
            if (CurrentState == GameState.Playing)
            {
                Debug.Log($"Player used {abilityName} ability");
            }
        }
        #endregion
    }
}
