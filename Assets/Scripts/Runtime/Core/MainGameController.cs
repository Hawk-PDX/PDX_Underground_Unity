using UnityEngine;
using System;
using PDXUnderground.Player;
using PDXUnderground.UI;

namespace PDXUnderground.Core
{
    public class MainGameController : MonoBehaviour
    {
        #region Singleton
        private static MainGameController _instance;
        public static MainGameController Instance => _instance;
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeGameSystems();
        }
        #endregion
        
        #region References
        [Header("System References")]
        [SerializeField] private BuzzUIController buzzUIController;
        [SerializeField] private GamblerCharacter playerCharacter;
        #endregion
        
        #region Game State
        public enum GameState
        {
            MainMenu,
            Playing,
            Paused,
            GameOver
        }
        
        public enum Environment
        {
            Streets,
            Sewers,
            Club,
            Arena
        }
        
        private GameState _currentState = GameState.MainMenu;
        private Environment _currentEnvironment = Environment.Streets;
        
        public GameState CurrentState
        {
            get => _currentState;
            private set
            {
                if (_currentState != value)
                {
                    GameState oldState = _currentState;
                    _currentState = value;
                    OnGameStateChanged?.Invoke(oldState, _currentState);
                }
            }
        }
        
        public Environment CurrentEnvironment
        {
            get => _currentEnvironment;
            private set
            {
                if (_currentEnvironment != value)
                {
                    Environment oldEnv = _currentEnvironment;
                    _currentEnvironment = value;
                    OnEnvironmentChanged?.Invoke(oldEnv, _currentEnvironment);
                }
            }
        }
        
        // Events for state changes
        public event Action<GameState, GameState> OnGameStateChanged;
        public event Action<Environment, Environment> OnEnvironmentChanged;
        #endregion
        
        #region Buzz System
        private float _buzzLevel = 0f;
        private float _criticalBuzzLevel = 80f;
        
        public float BuzzLevel
        {
            get => _buzzLevel;
            private set
            {
                float oldValue = _buzzLevel;
                _buzzLevel = Mathf.Clamp(value, 0f, 100f);
                
                if (!Mathf.Approximately(oldValue, _buzzLevel))
                {
                    OnBuzzLevelChanged?.Invoke(oldValue, _buzzLevel);
                    
                    // Check for critical state changes
                    if (oldValue <= _criticalBuzzLevel && _buzzLevel > _criticalBuzzLevel)
                        OnEnterCriticalBuzz?.Invoke();
                    else if (oldValue > _criticalBuzzLevel && _buzzLevel <= _criticalBuzzLevel)
                        OnExitCriticalBuzz?.Invoke();
                }
            }
        }
        
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
