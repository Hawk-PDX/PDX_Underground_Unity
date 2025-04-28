
using UnityEngine;

namespace PDXUnderground.Systems
{
    public class TraumaSystem : MonoBehaviour
    {
        [Header("Weather Triggers")]
        public float rainAccuracyPenalty = -30f;
        public float rainDuration = 60f;
        
        [Header("Nightmare Settings")]
        public float nightmareChance = 0.3f;
        public float nightmareCooldown = 3600f; // 1 hour
        
        private WeatherSystem weather;
        private float lastNightmareTime;

        private void Start()
        {
            weather = FindObjectOfType<WeatherSystem>();
            if (weather != null)
            {
                weather.OnRainStart += HandleRainStart;
            }
        }

        private void HandleRainStart()
        {
            var gambler = FindObjectOfType<GamblerCharacter>();
            gambler.AddStatBoost("accuracy", (int)rainAccuracyPenalty, true, rainDuration, "RainTrauma");
            
            // Chance for nightmare sequence
            if (Time.time - lastNightmareTime > nightmareCooldown && 
                Random.value < nightmareChance)
            {
                TriggerNightmare();
            }
        }

        private void TriggerNightmare()
        {
            lastNightmareTime = Time.time;
            // Play nightmare animation/effects
            Debug.Log("FLASHBACK: Luca relives the night he lost his family");
        }
    }
}

