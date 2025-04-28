using UnityEngine;
using System.Collections;
using PDXUnderground.Core;
using PDXUnderground.Core.Interfaces;

namespace PDXUnderground.Effects
{
    /// <summary>
    /// Provides historically accurate lantern flickering effects for 1880s Portland tunnels.
    /// Simulates the subtle flicker and color variation of period oil lanterns.
    /// </summary>
    [RequireComponent(typeof(Light))]
    public class LanternFlicker : MonoBehaviour
    {
        #region Inspector Properties
        
        [Header("Flicker Settings")]
        [Tooltip("The intensity of the light flicker effect (0-1)")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float flickerIntensity = 0.2f;

        [Tooltip("How fast the light flickers")]
        [Range(0.1f, 10.0f)]
        [SerializeField] private float flickerSpeed = 1.5f;

        [Tooltip("Variation in the flicker's rhythm (0-1)")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float flickerVariation = 0.4f;

        [Header("Time of Day Settings")]
        [Tooltip("Base intensity during daytime")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float dayIntensity = 0.4f;

        [Tooltip("Base intensity during nighttime")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float nightIntensity = 0.9f;

        [Tooltip("Color during daytime")]
        [SerializeField] private Color dayColor = new Color(1.0f, 0.85f, 0.6f, 1.0f); // Warm yellow

        [Tooltip("Color during nighttime")]
        [SerializeField] private Color nightColor = new Color(1.0f, 0.7f, 0.3f, 1.0f); // Deep amber

        [Header("Environmental Effects")]
        [Tooltip("How much wind affects the flicker (0-1)")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float windInfluence = 0.7f;

        [Tooltip("How much dampness dampens the light (0-1)")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float dampnessEffect = 0.4f;

        [Header("Performance Settings")]
        [Tooltip("How often to update the flicker effect (in seconds)")]
        [Range(0.01f, 0.5f)]
        [SerializeField] private float updateInterval = 0.05f;

        [Tooltip("Maximum distance to activate full flicker effect")]
        [SerializeField] private float maxFlickerDistance = 25f;

        [Tooltip("Distance at which flicker effect is disabled")]
        [SerializeField] private float disableDistance = 50f;
        
        #endregion
        
        #region Private Variables
        
        private Light lanternLight;
        private float baseIntensity;
        private float baseLightRange;
        private Color baseColor;
        
        private float currentTimeOfDay = 0.5f; // 0 = midnight, 0.5 = noon, 1.0 = next midnight
        private float currentWindStrength = 0f;
        private float currentDampness = 0f;
        
        private float timeOffset;
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
            lanternLight = GetComponent<Light>();
            
            // Store initial values
            baseIntensity = lanternLight.intensity;
            baseLightRange = lanternLight.range;
            baseColor = lanternLight.color;
            
            // Create random time offset for variation between lanterns
            timeOffset = Random.value * 100f;
            
            // Create wait object for coroutine
            updateWait = new WaitForSeconds(updateInterval);
        }
        
        private void Start()
        {
            // Find player for distance check
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            
            // Find environment controller
            var timeController = FindObjectOfType<GameTimeController>();
            if (timeController != null)
            {
                timeController.OnTimeChanged += UpdateTimeOfDay;
                timeController.OnWeatherChanged += UpdateEnvironmentalEffects;
            }
            
            // Start the flicker coroutine
            flickerCoroutine = StartCoroutine(FlickerRoutine());
        }
        
        private void OnEnable()
        {
            // Start flicker coroutine if not running
            if (flickerCoroutine == null)
            {
                flickerCoroutine = StartCoroutine(FlickerRoutine());
            }
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
            lanternLight.intensity = baseIntensity;
            lanternLight.range = baseLightRange;
            lanternLight.color = baseColor;
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Updates the time of day for the lantern
        /// </summary>
        /// <param name="timeOfDay">0 = midnight, 0.5 = noon, 1.0 = next midnight</param>
        public void UpdateTimeOfDay(float timeOfDay)
        {
            currentTimeOfDay = Mathf.Clamp01(timeOfDay);
        }
        
        /// <summary>
        /// Updates environmental effects that influence the lantern
        /// </summary>
        /// <param name="windStrength">Wind strength (0-1)</param>
        /// <param name="dampness">Dampness level (0-1)</param>
        public void UpdateEnvironmentalEffects(float windStrength, float dampness)
        {
            currentWindStrength = Mathf.Clamp01(windStrength);
            currentDampness = Mathf.Clamp01(dampness);
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
                // Check player distance for performance
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
            
            // Scale flicker intensity based on distance for performance
            if (distanceToPlayer > 0)
            {
                float distanceFactor = Mathf.Clamp01(1f - ((distanceToPlayer - maxFlickerDistance) / (disableDistance - maxFlickerDistance)));
                effectiveFlickerIntensity *= distanceFactor;
            }
            
            // Apply final intensity with flicker
            lanternLight.intensity = targetIntensity * (1f + (flickerNoise * effectiveFlickerIntensity));
            
            // Apply color changes
            lanternLight.color = Color.Lerp(dayColor, nightColor, dayNightFactor);
            
            // Apply range changes
            lanternLight.range = baseLightRange * (1f + (flickerNoise * effectiveFlickerIntensity * 0.2f));
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
            
            // Apply dampness to dampen light
            float dampnessFactor = 1f - (currentDampness * dampnessEffect);
            
            return windFactor * dampnessFactor;
        }
        
        /// <summary>
        /// Calculates the flicker noise value using Perlin noise
        /// </summary>
        private float CalculateFlickerNoise()
        {
            // Combine multiple noise sources for natural flicker
            float primaryNoise = Mathf.PerlinNoise(timeOffset + Time.time * flickerSpeed, timeOffset * 0.5f);
            float secondaryNoise = Mathf.PerlinNoise(timeOffset * 2f + Time.time * flickerSpeed * 2.5f, timeOffset);
            float fastNoise = Mathf.PerlinNoise(timeOffset * 3f + Time.time * flickerSpeed * 5f, timeOffset * 2f) * flickerVariation;
            
            // Combine noise sources and convert to -1 to 1 range
            float noise = (primaryNoise * 0.6f + secondaryNoise * 0.3f + fastNoise * 0.1f) * 2f - 1f;
            
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
                    lanternLight.intensity = Mathf.Lerp(dayIntensity, nightIntensity, CalculateDayNightFactor());
                    lanternLight.color = Color.Lerp(dayColor, nightColor, CalculateDayNightFactor());
                    lanternLight.range = baseLightRange;
                }
                else if (distanceToPlayer <= disableDistance && !isFlickerActive)
                {
                    isFlickerActive = true;
                }
            }
        }
        
        #endregion
    }
}

