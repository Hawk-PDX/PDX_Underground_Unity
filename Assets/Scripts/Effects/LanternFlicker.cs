using UnityEngine;
using System.Collections;

namespace PDXUnderground.Effects
{
    /// <summary>
    /// Controls lantern flickering effects for atmosphere and visual fidelity
    /// </summary>
    [RequireComponent(typeof(Light))]
    public class LanternFlicker : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Flicker Settings")]
        [SerializeField] private float baseIntensity = 1.0f;
        [SerializeField, Range(0f, 1f)] private float flickerIntensity = 0.2f;
        [SerializeField, Range(0.1f, 10f)] private float flickerSpeed = 2.0f;
        
        [Header("Performance")]
        [SerializeField] private float updateInterval = 0.05f;
        #endregion

        #region Private Variables
        private Light lanternLight;
        private float timeOffset;
        private bool isFlickerActive = true;
        private Coroutine flickerCoroutine;
        #endregion

        #region Unity Lifecycle Methods
        private void Awake()
        {
            lanternLight = GetComponent<Light>();
            timeOffset = Random.value * 100f;
        }
        
        private void Start()
        {
            flickerCoroutine = StartCoroutine(FlickerRoutine());
        }
        
        private void OnEnable()
        {
            if (flickerCoroutine == null)
            {
                flickerCoroutine = StartCoroutine(FlickerRoutine());
            }
        }
        
        private void OnDisable()
        {
            if (flickerCoroutine != null)
            {
                StopCoroutine(flickerCoroutine);
                flickerCoroutine = null;
            }
        }
        #endregion

        #region Private Methods
        private IEnumerator FlickerRoutine()
        {
            WaitForSeconds wait = new WaitForSeconds(updateInterval);
            
            while (true)
            {
                if (isFlickerActive && lanternLight != null)
                {
                    // Calculate flicker using Perlin noise
                    float noise = Mathf.PerlinNoise(Time.time * flickerSpeed + timeOffset, 0f);
                    float normalizedNoise = (noise * 2f) - 1f; // Convert to -1 to 1 range
                    
                    // Apply to light intensity
                    lanternLight.intensity = baseIntensity * (1f + (normalizedNoise * flickerIntensity));
                }
                
                yield return wait;
            }
        }
        
        /// <summary>
        /// Apply a temporary boost to light intensity, like a gust of wind affecting the flame
        /// </summary>
        public void ApplyIntensityBoost(float boostAmount, float duration)
        {
            StartCoroutine(IntensityBoostRoutine(boostAmount, duration));
        }
        
        private IEnumerator IntensityBoostRoutine(float boostAmount, float duration)
        {
            float originalFlickerIntensity = flickerIntensity;
            float originalFlickerSpeed = flickerSpeed;
            
            flickerIntensity = Mathf.Clamp01(flickerIntensity + boostAmount);
            flickerSpeed = flickerSpeed * 1.5f;
            
            yield return new WaitForSeconds(duration);
            
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
            
            flickerIntensity = originalFlickerIntensity;
            flickerSpeed = originalFlickerSpeed;
        }
        #endregion
    }
}
