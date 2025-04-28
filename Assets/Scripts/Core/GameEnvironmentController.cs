using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PDXUnderground.Core
{
    /// <summary>
    /// Controls environmental aspects of the game world including time, weather,
    /// and atmospheric effects. Works with the GameTimeController to provide
    /// environmental data to other systems.
    /// </summary>
    public class GameEnvironmentController : MonoBehaviour
    {
        #region Inspector Properties
        [Header("Environmental References")]
        [SerializeField] private GameTimeController timeController;
        
        [Header("Environment Settings")]
        [Tooltip("Current environment type")]
        [SerializeField] private EnvironmentType currentEnvironment = EnvironmentType.Outdoor;
        
        [Tooltip("Ambient sound volume")]
        [Range(0f, 1f)]
        [SerializeField] private float ambientSoundVolume = 0.3f;
        
        [Tooltip("Fog density")]
        [Range(0f, 1f)]
        [SerializeField] private float fogDensity = 0.02f;
        
        [Header("Weather Effects")]
        [SerializeField] private GameObject rainEffect;
        [SerializeField] private GameObject fogEffect;
        [SerializeField] private GameObject windEffect;
        [SerializeField] private Light directionalLight;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;
        #endregion
        
        #region Enums
        /// <summary>
        /// Types of environments in the game
        /// </summary>
        public enum EnvironmentType
        {
            Outdoor,
            Indoor,
            Underground,
            Tunnels,
            Speakeasy
        }
        #endregion
        
        #region Private Variables
        private float currentWindStrength = 0f;
        private float currentRainIntensity = 0f;
        private float currentFogIntensity = 0f;
        
        private AudioSource ambientAudioSource;
        private Dictionary<EnvironmentType, float> environmentFogDensities = new Dictionary<EnvironmentType, float>();
        #endregion
        
        #region Unity Lifecycle
        private void Awake()
        {
            // Tag this GameObject so it can be found by other systems
            gameObject.tag = "EnvironmentController";
            
            // Find TimeController if not assigned
            if (timeController == null)
            {
                timeController = FindObjectOfType<GameTimeController>();
                if (timeController == null)
                {
                    Debug.LogWarning("No GameTimeController found. Creating one...");
                    GameObject timeObj = new GameObject("TimeController");
                    timeController = timeObj.AddComponent<GameTimeController>();
                    timeObj.transform.SetParent(transform);
                }
            }
            
            // Set up ambient audio
            ambientAudioSource = GetComponent<AudioSource>();
            if (ambientAudioSource == null)
            {
                ambientAudioSource = gameObject.AddComponent<AudioSource>();
                ambientAudioSource.loop = true;
                ambientAudioSource.volume = ambientSoundVolume;
                ambientAudioSource.spatialBlend = 0f; // 2D sound
            }
            
            // Initialize environment fog densities
            environmentFogDensities[EnvironmentType.Outdoor] = 0.01f;
            environmentFogDensities[EnvironmentType.Indoor] = 0.005f;
            environmentFogDensities[EnvironmentType.Underground] = 0.05f;
            environmentFogDensities[EnvironmentType.Tunnels] = 0.08f;
            environmentFogDensities[EnvironmentType.Speakeasy] = 0.03f;
        }
        
        private void Start()
        {
            // Subscribe to time controller events
            if (timeController != null)
            {
                timeController.OnTimeChanged += OnTimeChanged;
                timeController.OnWeatherChanged += OnWeatherChanged;
            }
            
            // Apply initial environment settings
            ApplyEnvironmentSettings(currentEnvironment);
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (timeController != null)
            {
                timeController.OnTimeChanged -= OnTimeChanged;
                timeController.OnWeatherChanged -= OnWeatherChanged;
            }
        }
        
        private void Update()
        {
            if (showDebugInfo)
            {
                DisplayDebugInfo();
            }
        }
        #endregion
        
        #region Public Methods
        /// <summary>
        /// Gets the current time of day (0-1 where 0 is midnight, 0.5 is noon)
        /// </summary>
        public float GetTimeOfDay()
        {
            if (timeController != null)
            {
                return timeController.GetTimeOfDay();
            }
            return 0.5f; // Default to noon if no time controller
        }
        
        /// <summary>
        /// Tries to get current weather data
        /// </summary>
        /// <param name="windStrength">Output wind strength (0-1)</param>
        /// <param name="rainIntensity">Output rain intensity (0-1)</param>
        /// <returns>True if weather data was available</returns>
        public bool TryGetWeatherData(out float windStrength, out float rainIntensity)
        {
            windStrength = currentWindStrength;
            rainIntensity = currentRainIntensity;
            
            if (timeController != null)
            {
                timeController.GetCurrentWeather(out windStrength, out rainIntensity);
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Changes the current environment type
        /// </summary>
        /// <param name="newEnvironment">The environment to change to</param>
        public void ChangeEnvironment(EnvironmentType newEnvironment)
        {
            currentEnvironment = newEnvironment;
            ApplyEnvironmentSettings(newEnvironment);
            
            Debug.Log($"Environment changed to {newEnvironment}");
        }
        
        /// <summary>
        /// Sets the fog density
        /// </summary>
        /// <param name="density">Fog density value (0-1)</param>
        public void SetFogDensity(float density)
        {
            fogDensity = Mathf.Clamp01(density);
            RenderSettings.fogDensity = fogDensity;
        }
        
        /// <summary>
        /// Sets the ambient sound volume
        /// </summary>
        /// <param name="volume">Volume level (0-1)</param>
        public void SetAmbientSoundVolume(float volume)
        {
            ambientSoundVolume = Mathf.Clamp01(volume);
            if (ambientAudioSource != null)
            {
                ambientAudioSource.volume = ambientSoundVolume;
            }
        }
        #endregion
        
        #region Private Methods
        /// <summary>
        /// Handles time of day changes
        /// </summary>
        private void OnTimeChanged(float timeOfDay)
        {
            // Update directional light angle based on time
            if (directionalLight != null)
            {
                float sunAngle = timeOfDay * 360f;
                directionalLight.transform.rotation = Quaternion.Euler(sunAngle, -30f, 0f);
                
                // Adjust intensity based on time of day
                // Full intensity at noon, dimmer at dawn/dusk, off at night
                float intensity = 0f;
                if (timeOfDay < 0.25f) // Midnight to 6am
                {
                    intensity = Mathf.Lerp(0f, 0.5f, timeOfDay * 4f);
                }
                else if (timeOfDay < 0.5f) // 6am to noon
                {
                    intensity = Mathf.Lerp(0.5f, 1f, (timeOfDay - 0.25f) * 4f);
                }
                else if (timeOfDay < 0.75f) // Noon to 6pm
                {
                    intensity = Mathf.Lerp(1f, 0.5f, (timeOfDay - 0.5f) * 4f);
                }
                else // 6pm to midnight
                {
                    intensity = Mathf.Lerp(0.5f, 0f, (timeOfDay - 0.75f) * 4f);
                }
                
                directionalLight.intensity = intensity;
            }
        }
        
        /// <summary>
        /// Handles weather changes
        /// </summary>
        private void OnWeatherChanged(float windStrength, float rainIntensity)
        {
            currentWindStrength = windStrength;
            currentRainIntensity = rainIntensity;
            
            // Update weather effects
            UpdateWeatherEffects();
        }
        
        /// <summary>
        /// Updates visual weather effects based on current conditions
        /// </summary>
        private void UpdateWeatherEffects()
        {
            // Update rain effect
            if (rainEffect != null)
            {
                rainEffect.SetActive(currentRainIntensity > 0.1f);
                
                ParticleSystem rainParticles = rainEffect.GetComponent<ParticleSystem>();
                if (rainParticles != null)
                {
                    var emission = rainParticles.emission;
                    emission.rateOverTime = 500f * currentRainIntensity;
                }
            }
            
            // Update fog effect
            if (fogEffect != null)
            {
                // More fog when raining
                currentFogIntensity = fogDensity + (currentRainIntensity * 0.05f);
                fogEffect.SetActive(currentFogIntensity > 0.01f);
                
                // Set global fog settings
                RenderSettings.fogDensity = currentFogIntensity;
            }
            
            // Update wind effect
            if (windEffect != null)
            {
                windEffect.SetActive(currentWindStrength > 0.2f);
                
                ParticleSystem windParticles = windEffect.GetComponent<ParticleSystem>();
                if (windParticles != null)
                {
                    var mainModule = windParticles.main;
                    mainModule.startSpeed = 5f * currentWindStrength;
                }
            }
        }
        
        /// <summary>
        /// Applies settings for the specified environment
        /// </summary>
        private void ApplyEnvironmentSettings(EnvironmentType environment)
        {
            // Apply fog density based on environment
            if (environmentFogDensities.TryGetValue(environment, out float density))
            {
                SetFogDensity(density);
            }
            
            // Disable outdoor weather effects for indoor environments
            bool isOutdoor = environment == EnvironmentType.Outdoor;
            
            if (rainEffect != null)
            {
                rainEffect.SetActive(isOutdoor && currentRainIntensity > 0.1f);
            }
            
            if (windEffect != null)
            {
                windEffect.SetActive(isOutdoor && currentWindStrength > 0.2f);
            }
            
            // Adjust ambient lighting based on environment
            switch (environment)
            {
                case EnvironmentType.Outdoor:
                    RenderSettings.ambientLight = new Color(0.6f, 0.6f, 0.6f);
                    break;
                    
                case EnvironmentType.Indoor:
                    RenderSettings.ambientLight = new Color(0.5f, 0.5f, 0.5f);
                    break;
                    
                case EnvironmentType.Underground:
                    RenderSettings.ambientLight = new Color(0.2f, 0.2f, 0.25f);
                    break;
                    
                case EnvironmentType.Tunnels:
                    RenderSettings.ambientLight = new Color(0.1f, 0.1f, 0.15f);
                    break;
                    
                case EnvironmentType.Speakeasy:
                    RenderSettings.ambientLight = new Color(0.4f, 0.3f, 0.2f);
                    break;
            }
        }
        
        /// <summary>
        /// Display debug information on screen
        /// </summary>
        private void DisplayDebugInfo()
        {
            string timeInfo = $"Time: {timeController?.GetHourOfDay():00}:00";
            string weatherInfo = $"Wind: {currentWindStrength:F2}, Rain: {currentRainIntensity:F2}";
            string envInfo = $"Environment: {currentEnvironment}, Fog: {currentFogIntensity:F3}";
            
            Debug.Log($"{timeInfo} | {weatherInfo} | {envInfo}");
        }
        #endregion
    }
}

