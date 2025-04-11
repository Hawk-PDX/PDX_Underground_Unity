using UnityEngine;
using UnityEngine.Events;

namespace PDXUnderground.Utilities
{
    public class GameManager : MonoBehaviour
    {
        // Singleton instance
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GameManager");
                        _instance = go.AddComponent<GameManager>();
                    }
                }
                return _instance;
            }
        }

        // Game state enum
        public enum GameState
        {
            MainMenu,
            Playing,
            Paused,
            GameOver
        }

        // Current game state
        public GameState CurrentState { get; private set; }

        // Events
        public UnityEvent OnGameTimeUpdate { get; private set; }
        public UnityEvent<GameState> OnGameStateChanged { get; private set; }

        // Time management
        private float gameTime = 0f;
        public float GameTime => gameTime;

        private void Awake()
        {
            // Ensure singleton behavior
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize events
            OnGameTimeUpdate = new UnityEvent();
            OnGameStateChanged = new UnityEvent<GameState>();

            // Set initial state
            SetGameState(GameState.MainMenu);
        }

        private void Update()
        {
            if (CurrentState == GameState.Playing)
            {
                // Update game time
                gameTime += Time.deltaTime;
                OnGameTimeUpdate.Invoke();
            }
        }

        public void SetGameState(GameState newState)
        {
            if (CurrentState != newState)
            {
                CurrentState = newState;
                OnGameStateChanged.Invoke(newState);
                
                // Handle state-specific logic
                switch (newState)
                {
                    case GameState.Playing:
                        Time.timeScale = 1f;
                        break;
                    case GameState.Paused:
                        Time.timeScale = 0f;
                        break;
                    case GameState.GameOver:
                        Time.timeScale = 0f;
                        break;
                }
            }
        }

        public void StartGame()
        {
            SetGameState(GameState.Playing);
        }

        public void PauseGame()
        {
            SetGameState(GameState.Paused);
        }

        public void ResumeGame()
        {
            SetGameState(GameState.Playing);
        }

        public void GameOver()
        {
            SetGameState(GameState.GameOver);
        }
    }
}

