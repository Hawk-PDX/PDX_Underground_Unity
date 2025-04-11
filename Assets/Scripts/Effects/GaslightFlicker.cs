using UnityEngine;
using System.Collections;

/// <summary>
/// GaslightFlicker controls the flickering behavior of gaslights in PDX Underground,
/// adjusting for time of day, weather conditions, and adding randomized variation.
/// </summary>
public class GaslightFlicker : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Light Settings")]
    [SerializeField] private Light gaslightLight;
    [SerializeField] private float baseIntensity = 0.8f;
    [SerializeField] private Color baseColor = new Color(1.0f, 0.8f, 0.6f);
    [SerializeField] private float lightRange = 10f;
    [SerializeField] private bool startOn = true;
    
    [Header("Flicker Settings")]
    [SerializeField] private bool enableFlicker = true;
    [SerializeField] private float flickerSpeed = 0.1f;
    [SerializeField] private float minIntensityMultiplier = 0.8f;
    [SerializeField] private float maxIntensityMultiplier = 1.2f;
    [SerializeField] private float colorVariation = 0.1f;
    
    [Header("Weather Effects")]
    [SerializeField] private bool respondToWeather = true;
    [SerializeField] private float calmWindMultiplier = 1.0f;
    [SerializeField] private float stormWindMultiplier = 3.0f;
    
    #endregion
    
    #region Private Variables
    
    private float currentIntensity;
    private Color currentColor;
    private float targetIntensity;
    private Color targetColor;
    private float intensityNoiseOffset;
    private float colorNoiseOffset;
    private float weatherMultiplier = 1.0f;
    private Coroutine flickerCoroutine;
    private GameEnvironmentController environmentController;
    private bool isLightOn = true;
    private float timeMultiplier = 1.0f;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        // Find the light component if not assigned
        if (gaslightLight == null)
        {
            gaslightLight = GetComponent<Light>();
            
            // Try to find in children if not on this GameObject
            if (gaslightLight == null)
            {
                gaslightLight = GetComponentInChildren<Light>();
            }
            
            // Create a new light component if still not found
            if (gaslightLight == null)
            {
                GameObject lightObj = new GameObject("GaslightLight");
                lightObj.transform.SetParent(transform);
                lightObj.transform.localPosition = Vector3.zero;
                gaslightLight = lightObj.AddComponent<Light>();
                gaslightLight.type = LightType.Point;
                gaslightLight.range = lightRange;
            }
        }
        
        // Initialize light
        if (gaslightLight != null)
        {
            gaslightLight.intensity = baseIntensity;
            gaslightLight.color = baseColor;
            gaslightLight.range = lightRange;
            
            // Set the light's initial state
            gaslightLight.enabled = startOn;
            isLightOn = startOn;
        }
        
        // Find environment controller
        environmentController = FindObjectOfType<GameEnvironmentController>();
        
        // Initialize noise offsets with random values for variation
        intensityNoiseOffset = Random.Range(0f, 1000f);
        colorNoiseOffset = Random.Range(0f, 1000f);
        
        // Initialize current values
        currentIntensity = baseIntensity;
        currentColor = baseColor;
        targetIntensity = baseIntensity;
        targetColor = baseColor;
        weatherMultiplier = calmWindMultiplier;
    }
    
    private void Start()
    {
        // Register with environment controller if available
        if (environmentController != null)
        {
            environmentController.AddGaslight(this);
        }
        
        // Start flicker effect if enabled
        if (enableFlicker && isLightOn)
        {
            StartFlickering();
        }
    }
    
    private void OnDestroy()
    {
        // Unregister from environment controller if available
        if (environmentController != null)
        {
            environmentController.RemoveGaslight(this);
        }
        
        // Stop coroutine if running
        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
        }
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Sets the light intensity directly
    /// </summary>
    public void SetIntensity(float intensity)
    {
        baseIntensity = intensity;
        currentIntensity = intensity;
        targetIntensity = intensity;
        
        if (gaslightLight != null)
        {
            gaslightLight.intensity = intensity;
        }
    }
    
    /// <summary>
    /// Sets the light color directly
    /// </summary>
    public void SetColor(Color color)
    {
        baseColor = color;
        currentColor = color;
        targetColor = color;
        
        if (gaslightLight != null)
        {
            gaslightLight.color = color;
        }
    }
    
    /// <summary>
    /// Turns the light on or off
    /// </summary>
    public void SetLightState(bool on)
    {
        isLightOn = on;
        
        if (gaslightLight != null)
        {
            gaslightLight.enabled = on;
        }
        
        // Start or stop flickering based on state
        if (on && enableFlicker)
        {
            StartFlickering();
        }
        else
        {
            StopFlickering();
        }
    }
    
    /// <summary>
    /// Sets the weather multiplier for flicker effects
    /// </summary>
    public void SetWeatherMultiplier(float multiplier)
    {
        if (respondToWeather)
        {
            weatherMultiplier = Mathf.Clamp(multiplier, calmWindMultiplier, stormWindMultiplier);
        }
    }
    
    /// <summary>
    /// Gets the current intensity of the light
    /// </summary>
    public float GetCurrentIntensity()
    {
        return currentIntensity;
    }
    
    /// <summary>
    /// Toggles the light on or off
    /// </summary>
    public void ToggleLight()
    {
        SetLightState(!isLightOn);
    }
    
    #endregion
    
    #region Private Methods
    
    /// <summary>
    /// Starts the flicker effect coroutine
    /// </summary>
    private void StartFlickering()
    {
        // Stop existing coroutine if already running
        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
        }
        
        // Start new flicker coroutine
        flickerCoroutine = StartCoroutine(FlickerRoutine());
    }
    
    /// <summary>
    /// Stops the flicker effect coroutine
    /// </summary>
    private void StopFlickering()
    {
        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
            flickerCoroutine = null;
        }
    }
    
    /// <summary>
    /// Coroutine that handles the flickering effect
    /// </summary>
    private IEnumerator FlickerRoutine()
    {
        while (true)
        {
            // Calculate new target values with Perlin noise for smooth transitions
            float noiseTime = Time.time * flickerSpeed * weatherMultiplier;
            
            // Calculate intensity variation
            float intensityNoise = Mathf.PerlinNoise(intensityNoiseOffset, noiseTime);
            float intensityRange = maxIntensityMultiplier - minIntensityMultiplier;
            float intensityMultiplier = minIntensityMultiplier + (intensityNoise * intensityRange);
            targetIntensity = baseIntensity * intensityMultiplier;
            
            // Calculate color variation
            float colorNoise = Mathf.PerlinNoise(colorNoiseOffset, noiseTime);
            float redVariation = (colorNoise - 0.5f) * colorVariation;
            float greenVariation = (Mathf.PerlinNoise(colorNoiseOffset + 100, noiseTime) - 0.5f) * colorVariation;
            float blueVariation = (Mathf.PerlinNoise(colorNoiseOffset + 200, noiseTime) - 0.5f) * colorVariation;
            
            // Apply color variation
            targetColor = new Color(
                Mathf.Clamp01(baseColor.r + redVariation),
                Mathf.Clamp01(baseColor.g + greenVariation),
                Mathf.Clamp01(baseColor.b + blueVariation),
                baseColor.a
            );
            
            // Smoothly interpolate to the target values
            currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, 0.2f);
            currentColor = Color.Lerp(currentColor, targetColor, 0.2f);
            
            // Apply to the light
            if (gaslightLight != null)
            {
                gaslightLight.intensity = currentIntensity * timeMultiplier;
                gaslightLight.color = currentColor;
            }
            
            // Wait for next update
            yield return null;
        }
    }
    
    #endregion
}

