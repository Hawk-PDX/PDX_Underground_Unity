using UnityEngine;
using PDXUnderground.Effects;

namespace PDXUnderground.Controllers
{
    /// <summary>
    /// Test controller for PDX Underground environment.
    /// Provides keyboard controls to test various environment features.
    /// </summary>
    public class PDXUndergroundTestController : MonoBehaviour
    {
        [Header("Time Simulation")]
        [Tooltip("Simulated time of day (0=midnight, 0.5=noon, 1=midnight)")]
        [Range(0f, 1f)]
        [SerializeField] private float timeOfDay = 0.5f;
        
        [Tooltip("How fast time passes in the simulation")]
        [Range(0f, 0.1f)]
        [SerializeField] private float timePassageSpeed = 0.01f;
        
        [Tooltip("Whether time automatically passes")]
        [SerializeField] private bool autoAdvanceTime = true;
        
        [Header("Weather Simulation")]
        [Tooltip("Simulated weather intensity (wind/rain)")]
        [Range(0f, 1f)]
        [SerializeField] private float weatherIntensity = 0f;
        
        [Tooltip("How quickly weather can change")]
        [Range(0f, 0.1f)]
        [SerializeField] private float weatherChangeSpeed = 1f;
        
        [SerializeField] private float baseWeatherDuration = 60f;
        private float nextWeatherChange = 60f;
        
        [Header("Events")]
        public UnityEngine.Events.UnityEvent<float> OnTimeChanged;
        public UnityEngine.Events.UnityEvent<float> OnWeatherChanged;
        
        private GaslightFlicker[] gaslights;
        private bool timeKeyPressed = false;
        private bool weatherKeyPressed = false;
        private float lastTimeValue = -1f;
        private float lastWeatherValue = -1f;
        
        private void Start()
        {
            // Find all gaslights in the scene
            gaslights = FindObjectsOfType<GaslightFlicker>();
            Debug.Log($"Found {gaslights.Length} gaslights in the scene");
            
            // Initial update of all gaslights
            UpdateAllGaslights();
        }

        private void Update()
        {
            // Handle time passage
            if (autoAdvanceTime)
            {
                timeOfDay = (timeOfDay + timePassageSpeed * Time.deltaTime) % 1f;
            }

            // Check for weather change
            if (Time.time > nextWeatherChange)
            {
                nextWeatherChange = Time.time + (baseWeatherDuration / weatherChangeSpeed);
                // Change weather randomly
                ChangeWeather();
            }
            
            // Time controls (T/Y keys)
            if (Input.GetKeyDown(KeyCode.T))
            {
                timeOfDay = Mathf.Max(0f, timeOfDay - 0.1f);
                timeKeyPressed = true;
            }
            else if (Input.GetKeyDown(KeyCode.Y))
            {
                timeOfDay = Mathf.Min(1f, timeOfDay + 0.1f);
                timeKeyPressed = true;
            }
            
            // Weather controls (W/S keys)
            if (Input.GetKeyDown(KeyCode.W))
            {
                weatherIntensity = Mathf.Min(1f, weatherIntensity + 0.2f);
                weatherKeyPressed = true;
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                weatherIntensity = Mathf.Max(0f, weatherIntensity - 0.2f);
                weatherKeyPressed = true;
            }
            
            // Toggle auto time advance
            if (Input.GetKeyDown(KeyCode.P))
            {
                autoAdvanceTime = !autoAdvanceTime;
                Debug.Log($"Auto time advance: {(autoAdvanceTime ? "ON" : "OFF")}");
            }
            
            // Update gaslights if values changed
            if (timeKeyPressed || weatherKeyPressed || autoAdvanceTime)
            {
                UpdateAllGaslights();
                timeKeyPressed = false;
                weatherKeyPressed = false;
            }
            
            // Trigger events if values changed
            if (Mathf.Abs(lastTimeValue - timeOfDay) > 0.001f)
            {
                lastTimeValue = timeOfDay;
                OnTimeChanged?.Invoke(timeOfDay);
            }
            
            if (Mathf.Abs(lastWeatherValue - weatherIntensity) > 0.001f)
            {
                lastWeatherValue = weatherIntensity;
                OnWeatherChanged?.Invoke(weatherIntensity);
            }
        }
        
        private void ChangeWeather()
        {
            // Randomly change weather intensity
            weatherIntensity = Random.Range(0f, 1f);
            weatherKeyPressed = true;
        }
        
        private void UpdateAllGaslights()
        {
            if (gaslights != null)
            {
                foreach (var gaslight in gaslights)
                {
                    if (gaslight != null)
                    {
                        gaslight.UpdateTimeOfDay(timeOfDay);
                        gaslight.UpdateEnvironmentalEffects(weatherIntensity, 0f); // Pass wind strength and no rain
                    }
                }
            }
        }
        
        private void OnGUI()
        {
            // Display current time and weather info
            GUILayout.BeginArea(new Rect(10, 10, 300, 100));
            GUILayout.Label($"Time: {GetTimeString(timeOfDay)} ({timeOfDay:F2})");
            GUILayout.Label($"Weather: {GetWeatherString(weatherIntensity)} ({weatherIntensity:F2})");
            GUILayout.Label("Controls: T/Y - Time, W/S - Weather, P - Toggle auto time");
            GUILayout.EndArea();
        }
        
        private string GetTimeString(float normalizedTime)
        {
            int hours = Mathf.FloorToInt(normalizedTime * 24f);
            int minutes = Mathf.FloorToInt((normalizedTime * 24f - hours) * 60f);
            return $"{hours:D2}:{minutes:D2}";
        }
        
        private string GetWeatherString(float intensity)
        {
            if (intensity < 0.2f) return "Clear";
            if (intensity < 0.4f) return "Light Breeze";
            if (intensity < 0.6f) return "Breezy";
            if (intensity < 0.8f) return "Windy";
            return "Stormy";
        }
        
        /// <summary>
        /// Gets the current time of day value (0-1)
        /// </summary>
        public float GetCurrentTime()
        {
            return timeOfDay;
        }
        
        /// <summary>
        /// Gets the current weather intensity value (0-1)
        /// </summary>
        public float GetCurrentWeather()
        {
            return weatherIntensity;
        }
        
        /// <summary>
        /// Initializes the events if they haven't been created yet
        /// </summary>
        private void OnEnable()
        {
            if (OnTimeChanged == null)
            {
                OnTimeChanged = new UnityEngine.Events.UnityEvent<float>();
            }
            
            if (OnWeatherChanged == null)
            {
                OnWeatherChanged = new UnityEngine.Events.UnityEvent<float>();
            }
        }
    }
}

