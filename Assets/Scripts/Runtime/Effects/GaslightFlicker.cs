using UnityEngine;
using System.Collections;
using PDXUnderground.Core;
using PDXUnderground.Core.Interfaces;

namespace PDXUnderground.Effects
{
    /// <summary>
    /// Provides historically accurate gaslight flickering effects for 1880s Portland environment.
    /// Simulates the subtle flicker and color variation of period gas lighting.
    /// </summary>
    [RequireComponent(typeof(Light))]
    public class GaslightFlicker : MonoBehaviour
    {
        #region Inspector Properties
        
        [Header("Flicker Settings")]
        [Tooltip("The intensity of the light flicker effect (0-1)")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float flickerIntensity = 0.15f;

        [Tooltip("How fast the light flickers")]
        [Range(0.1f, 10.0f)]
        [SerializeField] private float flickerSpeed = 2.0f;

        [Tooltip("Variation in the flicker's rhythm (0-1)")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float flickerVariation = 0.3f;

        [Header("Time of Day Settings")]
        [Tooltip("Base intensity during daytime")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float dayIntensity = 0.3f;

        [Tooltip("Base intensity during nighttime")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float nightIntensity = 0.8f;

        [Tooltip("Color during daytime")]
        [SerializeField] private Color dayColor = new Color(1.0f, 0.9f, 0.7f, 1.0f); // Warm yellow-orange

        [Tooltip("Color during nighttime")]
        [SerializeField] private Color nightColor = new Color(1.0f, 0.75f, 0.4f, 1.0f); // Deeper orange-amber

        [Header("Environmental Effects")]
        [Tooltip("How much wind affects the flicker (0-1)")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float windInfluence = 0.5f;

        [Tooltip("How much rain dampens the light (0-1)")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float rainDampening = 0.3f;

        [Tooltip("If true, the light's range will also flicker")]
        [SerializeField] private bool flickerRange = false;

        [Tooltip("Noise texture for more natural flickering")]
        [SerializeField] private Texture2D noiseTexture;

        [Header("Performance Settings")]
        [Tooltip("How often to update the flicker effect (in seconds)")]
        [Range(0.01f, 0.5f)]
        [SerializeField] private float updateInterval = 0.05f;

        [Tooltip("Maximum distance to activate full flicker effect")]
        [SerializeField] private float maxFlickerDistance = 30f;

        [Tooltip("Distance at which flicker effect is disabled")]
        [SerializeField] private float disableDistance = 60f;
        
        #endregion
        
        #region Private Variables
        
        private Light gaslightComponent;
        private float baseIntensity;
        private float baseLightRange;
        private Color baseColor;
        
        private float currentTimeOfDay = 0.5f; // 0 = midnight, 0.5 = noon, 1.0 = next midnight
        private float currentWindStrength = 0f;
        private float currentRainIntensity = 0f;
        private BuzzState currentBuzzState = BuzzState.Sober;
        private float timeOffset;
        private float noiseScrollOffset = 0f;
        
        private Transform playerTransform;
        private bool isFlickerActive = true;
        private float distanceToPlayer = 0f;
        
        private WaitForSeconds updateWait;
        private Coroutine flickerCoroutine;
        
        #endregion
        
        #region Unity Lifecycle Methods
        
        private void Awake()
        {
            // Get components
            gaslightComponent = GetComponent<Light>();
            
            // Store initial values
            baseIntensity = gaslightComponent.intensity;
            baseLightRange = gaslightComponent.range;
            baseColor = gaslightComponent.color;
            
            // Create random time offset for variation between lights
            timeOffset = Random.value * 100f;
            
            // Create wait object for coroutine
            updateWait = new WaitForSeconds(updateInterval);
        }
        
        private void Start()
        {
            // Find player for distance check
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            
            // Try to find game time controller
            TryGetGameTimeController();
            
            // Find game time controller for event subscription
            var timeController = FindObjectOfType<GameTimeController>();
            if (timeController != null)
            {
                timeController.OnTimeChanged += UpdateTimeOfDay;
                timeController.OnWeatherChanged += UpdateEnvironmentalEffects;
            }
        }
        
        private void OnEnable()
        {
            // Start flicker coroutine
            if (flickerCoroutine != null)
                StopCoroutine(flickerCoroutine);
                
            flickerCoroutine = StartCoroutine(FlickerRoutine());
        }
        
        private void OnDisable()
        {
            // Stop flicker coroutine
            if (flickerCoroutine != null)
            {
                StopCoroutine(flickerCoroutine);
                flickerCoroutine = null;
            }

            // Reset light to original values
            gaslightComponent.intensity = baseIntensity;
            gaslightComponent.range = baseLightRange;
            gaslightComponent.color = baseColor;
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Updates the time of day for the gaslight
        /// </summary>
        /// <param name="timeOfDay">0 = midnight, 0.5 = noon, 1.0 = next midnight</param>
        public void UpdateTimeOfDay(float timeOfDay)
        {
            currentTimeOfDay = Mathf.Clamp01(timeOfDay);
        }
        
        /// <summary>
        /// Updates environmental effects that influence the gaslight
        /// </summary>
        /// <param name="windStrength">Wind strength (0-1)</param>
        /// <param name="rainIntensity">Rain intensity (0-1)</param>
        public void UpdateEnvironmentalEffects(float windStrength, float rainIntensity)
        {
            currentWindStrength = Mathf.Clamp01(windStrength);
            currentRainIntensity = Mathf.Clamp01(rainIntensity);
        }
        
        /// <summary>
        /// Updates the flicker effect based on the character's buzz state
        /// </summary>
        /// <param name="buzzState">Current buzz state of the character</param>
        public void UpdateBuzzEffects(BuzzState buzzState)
        {
            currentBuzzState = buzzState;
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// The main flicker coroutine that updates the light effect over time
        /// </summary>
        private IEnumerator FlickerRoutine()
        {
            while (true)
            {
                // Check if player is too far for performance
                CheckPlayerDistance();
                
                if (isFlickerActive)
                {
                    // Apply the flicker effect
                    ApplyFlickerEffect();
                }
                
                // Wait for next update
                yield return updateWait;
            }
        }
        
        /// <summary>
        /// Applies the flicker effect to the light
        /// </summary>
        private void ApplyFlickerEffect()
        {
            // Calculate time-of-day influence
            float dayNightFactor = CalculateDayNightFactor();
            
            // Calculate environmental factors
            float environmentalFactor = CalculateEnvironmentalFactor();
            
            // Calculate flicker noise
            float flickerNoise = CalculateFlickerNoise();
            
            // Apply intensity changes
            float targetIntensity = Mathf.Lerp(dayIntensity, nightIntensity, dayNightFactor);
            float effectiveFlickerIntensity = flickerIntensity * environmentalFactor;
            
            // Apply buzz state modifications
            ApplyBuzzStateEffects(ref effectiveFlickerIntensity, ref flickerNoise);
            
            // Scale flicker intensity based on distance for performance
            if (distanceToPlayer > 0)
            {
                float distanceFactor = Mathf.Clamp01(1f - ((distanceToPlayer - maxFlickerDistance) / (disableDistance - maxFlickerDistance)));
                effectiveFlickerIntensity *= distanceFactor;
            }
            
            // Apply final intensity with flicker
            gaslightComponent.intensity = targetIntensity * (1f + (flickerNoise * effectiveFlickerIntensity));
            
            // Apply color changes
            gaslightComponent.color = Color.Lerp(dayColor, nightColor, dayNightFactor);
            
            // Apply range changes if enabled
            if (flickerRange)
            {
                gaslightComponent.range = baseLightRange * (1f + (flickerNoise * effectiveFlickerIntensity * 0.3f));
            }
        }
        
        /// <summary>
        /// Calculates the day/night transition factor (0 = day, 1 = night)
        /// </summary>
        private float CalculateDayNightFactor()
        {
            // Convert time of day to day/night factor
            // Daytime is roughly 6am (0.25) to 6pm (0.75)
            float dayFactor;
            
            if (currentTimeOfDay < 0.25f) // Midnight to 6am
            {
                dayFactor = Mathf.SmoothStep(1f, 0f, currentTimeOfDay * 4f); // Gradually transition to day
            }
            else if (currentTimeOfDay < 0.75f) // 6am to 6pm
            {
                dayFactor = 0f; // Full day
            }
            else // 6pm to midnight
            {
                dayFactor = Mathf.SmoothStep(0f, 1f, (currentTimeOfDay - 0.75f) * 4f); // Gradually transition to night
            }
            
            return dayFactor;
        }
        
        /// <summary>
        /// Calculates the environmental effects factor
        /// </summary>
        private float CalculateEnvironmentalFactor()
        {
            // Apply wind strength to increase flicker
            float windFactor = 1f + (currentWindStrength * windInfluence);
            
            // Apply rain to dampen light
            float rainFactor = 1f - (currentRainIntensity * rainDampening);
            
            return windFactor * rainFactor;
        }
        
        /// <summary>
        /// Calculates the flicker noise value
        /// </summary>
        private float CalculateFlickerNoise()
        {
            float noise;
            
            // Use noise texture if available for more natural flicker
            if (noiseTexture != null)
            {
                // Scroll through noise texture for organic variation
                noiseScrollOffset += Time.deltaTime * flickerSpeed * 0.1f;
                float x = (timeOffset + Time.time * flickerSpeed) % 1.0f;
                float y = (noiseScrollOffset) % 1.0f;
                
                // Sample noise texture
                Color pixelColor = noiseTexture.GetPixelBilinear(x, y);
                noise = pixelColor.grayscale * 2f - 1f; // Convert to -1 to 1 range
            }
            else
            {
                // Fallback to Perlin noise with multiple octaves for natural flicker
                float primaryNoise = Mathf.PerlinNoise(timeOffset + Time.time * flickerSpeed, timeOffset * 0.5f);
                float secondaryNoise = Mathf.PerlinNoise(timeOffset * 2f + Time.time * flickerSpeed * 2.5f, timeOffset);
                float fastNoise = Mathf.PerlinNoise(timeOffset * 3f + Time.time * flickerSpeed * 5f, timeOffset * 2f) * flickerVariation;
                
                // Combine noise sources and convert to -1 to 1 range
                noise = (primaryNoise * 0.6f + secondaryNoise * 0.3f + fastNoise * 0.1f) * 2f - 1f;
            }
            
            return noise;
        }
        
        /// <summary>
        /// Checks distance to player for performance optimization
        /// </summary>
        private void CheckPlayerDistance()
        {
            if (playerTransform != null)
            {
                distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
                
                // Completely disable flicker at far distances
                if (distanceToPlayer > disableDistance && isFlickerActive)
                {
                    isFlickerActive = false;
                    gaslightComponent.intensity = Mathf.Lerp(dayIntensity, nightIntensity, CalculateDayNightFactor());
                    gaslightComponent.color = Color.Lerp(dayColor, nightColor, CalculateDayNightFactor());
                    gaslightComponent.range = baseLightRange;
                }
                else if (distanceToPlayer <= disableDistance && !isFlickerActive)
                {
                    isFlickerActive = true;
                }
            }
        }
        
        /// <summary>
        /// Attempts to find and connect to the game's time controller
        /// </summary>
        private void TryGetGameTimeController()
        {
            // Find environment controller to get time of day
            GameObject environmentController = GameObject.FindGameObjectWithTag("EnvironmentController");
            if (environmentController != null)
            {
                // If there's a method to subscribe to time changes, we could do it here
                var controller = environmentController.GetComponent<GameEnvironmentController>();
                if (controller != null)
                {
                    if (controller.GetTimeOfDay() > 0f)
                    {
                        UpdateTimeOfDay(controller.GetTimeOfDay());
                    }
                    
                    // Subscribe to weather changes if available
                    if (controller.TryGetWeatherData(out float wind, out float rain))
                    {
                        UpdateEnvironmentalEffects(wind, rain);
                    }
                }
            }
        }
        
        /// <summary>
        /// Applies the character's buzz state effects to the light flickering
        /// </summary>
        /// <param name="flickerIntensityMultiplier">Reference to the flicker intensity to modify</param>
        /// <param name="noiseValue">Reference to the noise value to modify</param>
        private void ApplyBuzzStateEffects(ref float flickerIntensityMultiplier, ref float noiseValue)
        {
            // Apply buzz effects
            switch (currentBuzzState)
            {
                case BuzzState.Sober:
                    // Normal state - standard flicker
                    break;
                    
                case BuzzState.Tipsy:
                    // Slight buzz - slower, more subtle flicker
                    flickerIntensityMultiplier *= 0.8f;
                    break;
                    
                case BuzzState.Buzzed:
                    // Medium buzz - slightly more intense flicker
                    flickerIntensityMultiplier *= 1.5f;
                    // Add some wobble to the noise
                    noiseValue += Mathf.Sin(Time.time * 3.0f) * 0.2f;
                    break;
                    
                case BuzzState.Drunk:
                    // Strong buzz - much more intense flicker
                    flickerIntensityMultiplier *= 2.5f;
                    // Add strong wobble to the noise
                    noiseValue += Mathf.Sin(Time.time * 8.0f) * 0.4f;
                    break;
                    
                case BuzzState.Wasted:
                    // Extreme buzz - extreme flickering
                    flickerIntensityMultiplier *= 4.0f;
                    // Add extreme wobble with multiple frequencies
                    noiseValue += Mathf.Sin(Time.time * 12.0f) * 0.5f;
                    noiseValue += Mathf.Cos(Time.time * 7.0f) * 0.3f;
                    break;
            }
        }
    #endregion // Private Methods
    }
}
