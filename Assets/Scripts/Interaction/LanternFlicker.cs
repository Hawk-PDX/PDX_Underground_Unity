using UnityEngine;
using System.Collections;

namespace PDXUnderground.Effects
{
    /// <summary>
    /// Controls lantern flickering effects for atmosphere and visual fidelity.
    /// Handles day/night transitions and light intensity variations.
    /// </summary>
    [RequireComponent(typeof(Light))]
    public class LanternFlicker : MonoBehaviour
    {
        #region Inspector Settings
        
        [Header("Flicker Settings")]
        [Tooltip("Base intensity of the light")]
        [SerializeField] private float baseIntensity = 1.0f;
        
        [Tooltip("How intense the flickering effect should be (0-1)")]
        [Range(0f, 1f)]
        [SerializeField] private float _flickerIntensity = 0.2f;
        
        /// <summary>
        /// How intense the flickering effect should be (0-1)
        /// </summary>
        public float flickerIntensity
        {
            get { return _flickerIntensity; }
            set { _flickerIntensity = Mathf.Clamp01(value); }
        }
        
        [Tooltip("Speed of the flickering effect")]
        [Range(0.1f, 10f)]
        [SerializeField] private float _flickerSpeed = 2.0f;
        
        /// <summary>
        /// Speed of the flickering effect
        /// </summary>
        public float flickerSpeed
        {
            get { return _flickerSpeed; }
            set { _flickerSpeed = Mathf.Max(0.1f, value); }
        }
        
        [Tooltip("Randomness of the flickering (0-1)")]
        [Range(0f, 1f)]
        [SerializeField] private float _flickerVariation = 0.5f;
        
        /// <summary>
        /// Randomness of the flickering (0-1)
        /// </summary>
        public float flickerVariation
        {
            get { return _flickerVariation; }
            set { _flickerVariation = Mathf.Clamp01(value); }
        }
        
        [Header("Day/Night Settings")]
        [Tooltip("Light intensity during daytime")]
        [SerializeField] private float _dayIntensity = 0.5f;
        
        /// <summary>
        /// Light intensity during daytime
        /// </summary>
        public float dayIntensity
        {
            get { return _dayIntensity; }
            set { _dayIntensity = Mathf.Max(0f, value); }
        }
        
        [Tooltip("Light intensity during nighttime")]
        [SerializeField] private float _nightIntensity = 1.0f;
        
        /// <summary>
        /// Light intensity during nighttime
        /// </summary>
        public float nightIntensity
        {
            get { return _nightIntensity; }
            set { _nightIntensity = Mathf.Max(0f, value); }
        }
        
        [Tooltip("Color during daytime")]
        [SerializeField] private Color dayColor = new Color(1.0f, 0.9f, 0.7f);
        
        [Tooltip("Color during nighttime")]
        [SerializeField] private Color nightColor = new Color(1.0f, 0.6f, 0.2f);
        
        [Header("Performance")]
        [Tooltip("Distance from player to disable flickering")]
        [SerializeField] private float disableDistance = 20f;
        
        [Tooltip("Update interval for performance optimization")]
        [SerializeField] private float updateInterval = 0.05f;
        
        #endregion
        
        #region Private Variables
        
        private Light lanternLight;
        private float timeOffset;
        private float currentTimeOfDay = 0.5f; // 0.5 = noon
        private bool isFlickerActive = true;
        private float originalRange;
        private Color originalColor;
        private Transform playerTransform;
        private Coroutine flickerCoroutine;
        private WaitForSeconds flickerWait;
        
        #endregion
        
        #region Unity Lifecycle Methods
        
        private void Awake()
        {
            // Get the Light component
            lanternLight = GetComponent<Light>();
            
            // Store original values
            baseIntensity = lanternLight.intensity;
            baseIntensity = lanternLight.intensity;
            originalRange = lanternLight.range;
            originalColor = lanternLight.color;
            
            // Initialize properties from serialized fields
            flickerIntensity = _flickerIntensity;
            flickerSpeed = _flickerSpeed;
            flickerVariation = _flickerVariation;
            dayIntensity = _dayIntensity;
            nightIntensity = _nightIntensity;
            // Create random offset for variety between lanterns
            timeOffset = Random.value * 100f;
            
            // Create wait object for coroutine
            flickerWait = new WaitForSeconds(updateInterval);
        }
        
        private void Start()
        {
            // Find player
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            
            // Start flickering
            flickerCoroutine = StartCoroutine(FlickerRoutine());
        }
        
        private void OnEnable()
        {
            // Start flickering if not already running
            if (flickerCoroutine == null)
            {
                flickerCoroutine = StartCoroutine(FlickerRoutine());
            }
        }
        
        private void OnDisable()
        {
            // Stop flickering
            if (flickerCoroutine != null)
            {
                StopCoroutine(flickerCoroutine);
                flickerCoroutine = null;
            }
            
            // Reset light to original values
            ResetLightValues();
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Updates the time of day to affect light intensity
        /// </summary>
        /// <param name="timeOfDay">0 = midnight, 0.5 = noon, 1 = next midnight</param>
        public void SetTimeOfDay(float timeOfDay)
        {
            currentTimeOfDay = Mathf.Clamp01(timeOfDay);
        }
        
        /// <summary>
        /// Updates weather effect that can affect the flicker
        /// </summary>
        /// <param name="intensity">Weather effect intensity (0-1)</param>
        public void SetWeatherEffect(float intensity)
        {
            // Adjust flicker intensity based on weather
            // Higher values for storms, rain, etc.
            flickerIntensity = Mathf.Lerp(0.2f, 0.5f, intensity);
            flickerSpeed = Mathf.Lerp(2.0f, 4.0f, intensity);
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Main routine for light flickering
        /// </summary>
        private IEnumerator FlickerRoutine()
        {
            while (true)
            {
                // Check if player is too far away for performance
                if (playerTransform != null)
                {
                    float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
                    isFlickerActive = distanceToPlayer < disableDistance;
                }
                
                if (isFlickerActive)
                {
                    // Calculate day/night factor (0 = day, 1 = night)
                    float dayNightFactor = CalculateDayNightFactor();
                    
                    // Calculate flicker
                    float noise = CalculateFlickerNoise();
                    
                    // Apply to light
                    ApplyLightEffects(dayNightFactor, noise);
                }
                else
                {
                    // Just maintain steady day/night intensity without flickering
                    float dayNightFactor = CalculateDayNightFactor();
                    lanternLight.intensity = Mathf.Lerp(dayIntensity, nightIntensity, dayNightFactor) * baseIntensity;
                    lanternLight.color = Color.Lerp(dayColor, nightColor, dayNightFactor);
                }
                
                yield return flickerWait;
            }
        }
        
        /// <summary>
        /// Calculate the day/night transition factor
        /// </summary>
        private float CalculateDayNightFactor()
        {
            // Convert time of day to night factor
            // 0.5 is noon, so night influence is minimal
            // 0.0 and 1.0 are midnight, so night influence is maximal
            
            if (currentTimeOfDay < 0.25f) // Midnight to 6am
            {
                return Mathf.Lerp(1f, 0f, currentTimeOfDay * 4f); // Transition from night to day
            }
            else if (currentTimeOfDay < 0.75f) // 6am to 6pm
            {
                return 0f; // Full day
            }
            else // 6pm to midnight
            {
                return Mathf.Lerp(0f, 1f, (currentTimeOfDay - 0.75f) * 4f); // Transition from day to night
            }
        }
        
        /// <summary>
        /// Calculate the flicker noise value
        /// </summary>
        private float CalculateFlickerNoise()
        {
            // Use Perlin noise to create realistic flickering
            float mainNoise = Mathf.PerlinNoise(timeOffset + Time.time * flickerSpeed, 0f);
            float detailNoise = Mathf.PerlinNoise(timeOffset * 2f + Time.time * flickerSpeed * 3f, 1f);
            
            // Combine noises with variation
            float combinedNoise = mainNoise * (1f - flickerVariation) + detailNoise * flickerVariation;
            
            // Convert to -1 to 1 range for easy application
            return (combinedNoise * 2f) - 1f;
        }
        
        /// <summary>
        /// Apply calculated effects to the light
        /// </summary>
        private void ApplyLightEffects(float dayNightFactor, float noise)
        {
            // Calculate target intensity based on time of day
            float targetIntensity = Mathf.Lerp(dayIntensity, nightIntensity, dayNightFactor) * baseIntensity;
            
            // Apply flicker to intensity
            lanternLight.intensity = targetIntensity * (1f + (noise * flickerIntensity));
            
            // Apply color based on time of day
            lanternLight.color = Color.Lerp(dayColor, nightColor, dayNightFactor);
            
            // Slightly adjust range too
            lanternLight.range = originalRange * (1f + (noise * flickerIntensity * 0.15f));
            
            // Optional: add a slight color variation for more realistic fire/lantern effect
            float redShift = Mathf.Max(0, noise * 0.05f);
            Color flickerColorShift = new Color(redShift, -redShift * 0.5f, -redShift * 0.5f, 0);
            lanternLight.color += flickerColorShift;
        }
        
        /// <summary>
        /// Reset light to its original values
        /// </summary>
        private void ResetLightValues()
        {
            if (lanternLight == null)
                return;
                
            lanternLight.intensity = baseIntensity;
            lanternLight.range = originalRange;
            lanternLight.color = originalColor;
        }
        
        /// <summary>
        /// Optimize performance based on player distance
        /// </summary>
        private void OptimizePerformance()
        {
            if (playerTransform == null)
                return;
                
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            
            // Adjust update interval based on distance
            if (distanceToPlayer > disableDistance * 0.5f)
            {
                // Further away - use a longer update interval
                float newInterval = Mathf.Lerp(updateInterval, updateInterval * 3f, 
                    (distanceToPlayer - disableDistance * 0.5f) / (disableDistance * 0.5f));
                
                // Simply create a new WaitForSeconds with the new interval
                flickerWait = new WaitForSeconds(newInterval);
            }
            else
            {
                // Close to player - use the default update interval
                flickerWait = new WaitForSeconds(updateInterval);
            }
        }
        
        /// <summary>
        /// Apply a temporary boost to light intensity, like a gust of wind affecting the flame
        /// </summary>
        public void ApplyIntensityBoost(float boostAmount, float duration)
        {
            StartCoroutine(IntensityBoostRoutine(boostAmount, duration));
        }
        
        /// <summary>
        /// Temporarily boosts the light intensity
        /// </summary>
        private IEnumerator IntensityBoostRoutine(float boostAmount, float duration)
        {
            float originalFlickerIntensity = flickerIntensity;
            float originalFlickerSpeed = flickerSpeed;
            
            // Increase flicker intensity and speed for the duration
            flickerIntensity = Mathf.Clamp01(flickerIntensity + boostAmount);
            flickerSpeed = flickerSpeed * 1.5f;
            
            // Wait for the specified duration
            yield return new WaitForSeconds(duration);
            
            // Smoothly transition back to original values
            float transitionTime = 0.5f;
            float elapsed = 0f;
            
            while (elapsed < transitionTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / transitionTime;
                
                flickerIntensity = Mathf.Lerp(flickerIntensity, originalFlickerIntensity, t);
                flickerSpeed = Mathf.Lerp(flickerSpeed, originalFlickerSpeed, t);
                
                yield return null;
            }
            
            // Reset to original values
            flickerIntensity = originalFlickerIntensity;
            flickerSpeed = originalFlickerSpeed;
    }
    #endregion
}
}
