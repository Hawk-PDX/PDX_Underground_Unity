using UnityEngine;
using System;

namespace PDXUnderground.Core
{
    /// <summary>
    /// Controls the passage of time and related weather effects in the game world.
    /// Provides events for time changes and weather transitions.
    /// </summary>
    public class GameTimeController : MonoBehaviour
    {
        #region Inspector Properties
        [Header("Time Settings")]
        [Tooltip("Real seconds per in-game hour")]
        [SerializeField] private float secondsPerGameHour = 60f;
        
        [Tooltip("Starting time of day (0-1 where 0 is midnight, 0.5 is noon)")]
        [Range(0f, 1f)]
        [SerializeField] private float startTimeOfDay = 0.25f; // 6 AM
        
        [Tooltip("Whether time should flow automatically")]
        [SerializeField] private bool timeFlowEnabled = true;
        
        [Header("Weather Settings")]
        [Tooltip("Current wind strength (0-1)")]
        [Range(0f, 1f)]
        [SerializeField] private float windStrength = 0.2f;
        
        [Tooltip("Current rain intensity (0-1)")]
        [Range(0f, 1f)]
        [SerializeField] private float rainIntensity = 0f;
        
        [Tooltip("How often weather changes occur")]
        [SerializeField] private float weatherTransitionFrequency = 0.1f;
        
        [Tooltip("How long weather transitions take")]
        [SerializeField] private float weatherTransitionDuration = 60f;
        #endregion
        
        #region Events
        // Event triggered when time of day changes
        public event Action<float> OnTimeChanged;
        
        // Event triggered when weather conditions change
        public event Action<float, float> OnWeatherChanged;
        #endregion
        
        #region Private Variables
        private float currentTimeOfDay;
        private float targetWindStrength;
        private float targetRainIntensity;
        private float weatherTransitionProgress = 1f; // 1 means no active transition
        #endregion
        
        #region Unity Lifecycle
        private void Awake()
        {
            // Initialize time
            currentTimeOfDay = startTimeOfDay;
            
            // Set initial weather targets to current values
            targetWindStrength = windStrength;
            targetRainIntensity = rainIntensity;
        }
        
        private void Start()
        {
            // Trigger initial events
            TriggerTimeChangedEvent();
            TriggerWeatherChangedEvent();
        }
        
        private void Update()
        {
            if (timeFlowEnabled)
            {
                UpdateGameTime();
            }
            
            UpdateWeather();
        }
        #endregion
        
        #region Public Methods
        /// <summary>
        /// Gets the current time of day as a 0-1 value (0 = midnight, 0.5 = noon)
        /// </summary>
        public float GetTimeOfDay()
        {
            return currentTimeOfDay;
        }
        
        /// <summary>
        /// Sets the current time of day
        /// </summary>
        /// <param name="newTime">Time of day (0-1)</param>
        public void SetTimeOfDay(float newTime)
        {
            currentTimeOfDay = Mathf.Repeat(newTime, 1f);
            TriggerTimeChangedEvent();
        }
        
        /// <summary>
        /// Converts time of day to hour of day (0-23)
        /// </summary>
        public int GetHourOfDay()
        {
            return Mathf.FloorToInt(currentTimeOfDay * 24f);
        }
        
        /// <summary>
        /// Gets the current weather conditions
        /// </summary>
        /// <param name="wind">Output wind strength (0-1)</param>
        /// <param name="rain">Output rain intensity (0-1)</param>
        public void GetCurrentWeather(out float wind, out float rain)
        {
            wind = windStrength;
            rain = rainIntensity;
        }
        
        /// <summary>
        /// Sets weather conditions immediately
        /// </summary>
        /// <param name="wind">Wind strength (0-1)</param>
        /// <param name="rain">Rain intensity (0-1)</param>
        public void SetWeather(float wind, float rain)
        {
            windStrength = Mathf.Clamp01(wind);
            rainIntensity = Mathf.Clamp01(rain);
            
            targetWindStrength = windStrength;
            targetRainIntensity = rainIntensity;
            weatherTransitionProgress = 1f;
            
            TriggerWeatherChangedEvent();
        }
        
        /// <summary>
        /// Transitions to new weather conditions over time
        /// </summary>
        /// <param name="wind">Target wind strength (0-1)</param>
        /// <param name="rain">Target rain intensity (0-1)</param>
        public void TransitionWeather(float wind, float rain)
        {
            targetWindStrength = Mathf.Clamp01(wind);
            targetRainIntensity = Mathf.Clamp01(rain);
            weatherTransitionProgress = 0f;
        }
        
        /// <summary>
        /// Enable or disable automatic time flow
        /// </summary>
        public void SetTimeFlowEnabled(bool enabled)
        {
            timeFlowEnabled = enabled;
        }
        #endregion
        
        #region Private Methods
        /// <summary>
        /// Updates the game time based on real time
        /// </summary>
        private void UpdateGameTime()
        {
            // Calculate time increment
            float timeIncrement = Time.deltaTime / secondsPerGameHour / 24f;
            
            // Update time of day
            currentTimeOfDay = Mathf.Repeat(currentTimeOfDay + timeIncrement, 1f);
            
            // Trigger event
            TriggerTimeChangedEvent();
            
            // Randomly trigger weather transitions
            if (UnityEngine.Random.value < weatherTransitionFrequency * Time.deltaTime && weatherTransitionProgress >= 1f)
            {
                GenerateRandomWeatherTarget();
            }
        }
        
        /// <summary>
        /// Updates weather transition
        /// </summary>
        private void UpdateWeather()
        {
            // If we have an active transition
            if (weatherTransitionProgress < 1f)
            {
                // Increment progress
                weatherTransitionProgress += Time.deltaTime / weatherTransitionDuration;
                if (weatherTransitionProgress > 1f) weatherTransitionProgress = 1f;
                
                // Interpolate values
                float t = Mathf.SmoothStep(0f, 1f, weatherTransitionProgress);
                windStrength = Mathf.Lerp(windStrength, targetWindStrength, t);
                rainIntensity = Mathf.Lerp(rainIntensity, targetRainIntensity, t);
                
                // Trigger event
                TriggerWeatherChangedEvent();
            }
        }
        
        /// <summary>
        /// Generates random target weather conditions
        /// </summary>
        private void GenerateRandomWeatherTarget()
        {
            // Generate random targets within reasonable ranges
            targetWindStrength = UnityEngine.Random.Range(0f, 0.8f);
            
            // Higher chance of no rain
            if (UnityEngine.Random.value < 0.7f)
            {
                targetRainIntensity = 0f;
            }
            else
            {
                targetRainIntensity = UnityEngine.Random.Range(0.3f, 1f);
            }
            
            // Start transition
            weatherTransitionProgress = 0f;
        }
        
        /// <summary>
        /// Triggers the time changed event
        /// </summary>
        private void TriggerTimeChangedEvent()
        {
            OnTimeChanged?.Invoke(currentTimeOfDay);
        }
        
        /// <summary>
        /// Triggers the weather changed event
        /// </summary>
        private void TriggerWeatherChangedEvent()
        {
            OnWeatherChanged?.Invoke(windStrength, rainIntensity);
        }
        #endregion
    }
}

