using UnityEngine;
using PDXUnderground.Models;
using PDXUnderground.Core;  // For BuzzState and CardType

namespace PDXUnderground.Core.Data
{
    public class GameData
    {
        // Core game state
        public GameState currentState { get; private set; }
        public TimeOfDay currentTimeOfDay { get; private set; }
        public EnvironmentType currentEnvironment { get; private set; }

        // Environment Data
        public class EnvironmentData
        {
            public float timeOfDay;         // 0-1 range (0=midnight, 0.5=noon)
            public float weatherIntensity;  // 0-1 range
            public bool isNight;
            public Color ambientColor;
            public float fogDensity;
        }

        // Player State
        public class PlayerData
        {
            public float currentBuzzLevel;
            public float maxBuzzLevel;
            public BuzzState buzzState;
            public CardType[] currentHand;
        }

        // Runtime Data
        public class RuntimeData
        {
            public float currentFPS;
            public float memoryUsage;
            public float loadTime;
            public string lastError;
        }

        // Save Data
        public class SaveData
        {
            public Vector3 playerPosition;
            public float playTime;
            public int lastCheckpoint;
            public string currentScene;
        }

        // Instance data
        public EnvironmentData environmentData { get; private set; }
        public PlayerData playerData { get; private set; }
        public RuntimeData runtimeData { get; private set; }
        public SaveData saveData { get; private set; }

        // Constructor
        public GameData()
        {
            environmentData = new EnvironmentData
            {
                timeOfDay = 0.5f,
                weatherIntensity = 0,
                isNight = false,
                ambientColor = Color.white,
                fogDensity = 0
            };

            playerData = new PlayerData
            {
                currentBuzzLevel = 0,
                maxBuzzLevel = 100,
                buzzState = BuzzState.Sober,
                currentHand = new CardType[0]
            };

            runtimeData = new RuntimeData();
            saveData = new SaveData();
            currentState = GameState.Loading;
            currentTimeOfDay = TimeOfDay.Dawn;
            currentEnvironment = EnvironmentType.Streets;
        }
    }
}

