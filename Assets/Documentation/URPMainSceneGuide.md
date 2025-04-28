# URP Implementation for MainScene

This guide provides specific instructions for implementing URP in the main game scene, focusing on the unique atmospheric requirements of PDX Underground.

## 1. Environment-Specific Volume Setup

### Streets Environment
1. Create Volume object: "_Level/Streets/StreetsPostProcess"
   - Add Volume component
   - Create new profile "StreetsVolumeProfile"
   - Add effects:
     * Bloom (subtle for street lights)
       - Threshold: 0.95
       - Intensity: 0.15
     * Color Adjustments (cool night tint)
       - Temperature: -15 (cooler blue tint)
       - Tint: +5 (slight purple)
     * Vignette (dark edges for atmosphere)
       - Intensity: 0.35
       - Smoothness: 0.4

### Tunnels Environment
1. Create Volume object: "_Level/Tunnels/TunnelsPostProcess"
   - Add Volume component
   - Create new profile "TunnelsVolumeProfile"
   - Add effects:
     * Bloom (stronger for moisture effects)
       - Threshold: 0.8
       - Intensity: 0.25
     * Color Adjustments (green/blue tint)
       - Temperature: -25 (colder)
       - Tint: -10 (green)
     * Fog
       - Color: Dark teal (#102030)
       - Mode: Exponential
       - Density: 0.03

### Speakeasy Environment
1. Create Volume object: "_Level/Speakeasy/SpeakeasyPostProcess"
   - Add Volume component
   - Create new profile "SpeakeasyVolumeProfile"
   - Add effects:
     * Bloom (warm glow for indoor lights)
       - Threshold: 0.7
       - Intensity: 0.4
     * Color Adjustments (warm tint)
       - Temperature: +20 (warm)
       - Tint: +10 (golden)
     * Vignette (stronger for intimate atmosphere)
       - Intensity: 0.45
       - Smoothness: 0.3

## 2. Environment Triggers and Transitions

Set up environment Volume triggers for seamless transitions:

1. Create trigger volumes for each environment zone
2. Use these colliders to activate the right post-processing:

```csharp
using UnityEngine;
using UnityEngine.Rendering;

public class EnvironmentTrigger : MonoBehaviour
{
    [SerializeField] private Volume environmentVolume;
    [SerializeField] private float transitionTime = 1.5f;
    
    private Volume globalVolume;
    
    private void Start()
    {
        // Find global volume
        globalVolume = GameObject.FindObjectOfType<Volume>();
        if (globalVolume == null || !globalVolume.isGlobal)
        {
            Debug.LogError("No global volume found! Create one first.");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && globalVolume != null)
        {
            // Start transition coroutine
            StartCoroutine(TransitionToProfie(environmentVolume.profile, transitionTime));
        }
    }
    
    private System.Collections.IEnumerator TransitionToProfie(VolumeProfile targetProfile, float duration)
    {
        float startTime = Time.time;
        float elapsedTime = 0;
        
        // Store initial weight
        float initialWeight = globalVolume.weight;
        
        // Fade out current profile
        while (elapsedTime < duration/2)
        {
            elapsedTime = Time.time - startTime;
            float t = elapsedTime / (duration/2);
            globalVolume.weight = Mathf.Lerp(initialWeight, 0, t);
            yield return null;
        }
        
        // Change profile
        globalVolume.profile = targetProfile;
        
        // Fade in new profile
        startTime = Time.time;
        elapsedTime = 0;
        
        while (elapsedTime < duration/2)
        {
            elapsedTime = Time.time - startTime;
            float t = elapsedTime / (duration/2);
            globalVolume.weight = Mathf.Lerp(0, 1, t);
            yield return null;
        }
        
        globalVolume.weight = 1;
    }
}
```

## 3. URP Lighting Setups

### Gaslight Prefab Updates
Update `GaslightFlicker.cs` for URP compatibility:

```csharp
[RequireComponent(typeof(Light))]
public class GaslightFlicker : MonoBehaviour
{
    [SerializeField] private float minIntensity = 0.8f;
    [SerializeField] private float maxIntensity = 1.2f;
    [SerializeField] private float flickerSpeed = 0.1f;
    
    private Light lightComponent;
    private float baseIntensity;
    private float nextIntensity;
    private float lastUpdate;

    void Start()
    {
        lightComponent = GetComponent<Light>();
        baseIntensity = lightComponent.intensity;
        
        // URP-specific settings
        lightComponent.useColorTemperature = true;
        lightComponent.shadowResolution = LightShadowResolution.FromQualitySettings;
    }
    
    void Update()
    {
        if (Time.time - lastUpdate > flickerSpeed)
        {
            lastUpdate = Time.time;
            nextIntensity = Random.Range(minIntensity, maxIntensity) * baseIntensity;
        }
        
        lightComponent.intensity = Mathf.Lerp(lightComponent.intensity, nextIntensity, Time.deltaTime * 10f);
    }
}
```

### Environment-Specific Light Settings

1. Streets:
   - Light Intensity: 1.5-2.0 (adjusted for Linear)
   - Temperature: 2200K (warm)
   - Shadow Resolution: Medium
   - Light Range: 8-10 units

2. Tunnels:
   - Light Intensity: 1.0-1.5
   - Temperature: 1800K (very warm)
   - Shadow Resolution: Low
   - Light Range: 5-7 units
   - Increased flicker (minIntensity: 0.7, maxIntensity: 1.3)

3. Speakeasy:
   - Light Intensity: 2.0-2.5
   - Temperature: 2400K
   - Shadow Resolution: High
   - Light Range: 6-8 units
   - Subtle flicker (minIntensity: 0.9, maxIntensity: 1.1)

## 4. Camera Setup for URP

### Main Camera Configuration
1. Select MainCamera object
2. Add Universal Additional Camera Data component
3. Configure settings:
   - Render Type: Base
   - Render Post Processing: Enabled
   - Anti-aliasing: FXAA
   - Priority: 0 (base camera)
   
### Environment Transitions
Update your GameEnvironmentController to handle camera transitions:

```csharp
// Add to environment transition code:
public void TransitionToEnvironment(EnvironmentType envType)
{
    // Update camera settings for URP
    var urpCam = Camera.main.GetComponent<UniversalAdditionalCameraData>();
    if (urpCam != null)
    {
        switch (envType)
        {
            case EnvironmentType.Streets:
                urpCam.renderPostProcessing = true;
                urpCam.volumeLayerMask = LayerMask.GetMask("Streets");
                break;
            case EnvironmentType.Tunnels:
                urpCam.renderPostProcessing = true;
                urpCam.volumeLayerMask = LayerMask.GetMask("Tunnels");
                break;
            case EnvironmentType.Speakeasy:
                urpCam.renderPostProcessing = true;
                urpCam.volumeLayerMask = LayerMask.GetMask("Speakeasy");
                break;
        }
    }
    
    // Existing environment transition code...
}
```

## 5. Material Updates for URP

### Required Materials by Environment
Create these materials using URP/Lit shader for consistent lighting:

1. Streets:
   - StreetCobblestone.mat (URP/Lit, normal map, roughness)
   - BuildingFacade.mat (URP/Lit, albedo texture)
   - Sidewalk.mat (URP/Lit, subtle normal map)

2. Tunnels:
   - TunnelWall.mat (URP/Lit, rough texture, moisture)
   - TunnelPipes.mat (URP/Lit with metallic property)
   - PuddleWater.mat (URP/Simple Lit with alpha transparency)

3. Speakeasy:
   - WoodFloor.mat (URP/Lit, wood texture)
   - BarCounter.mat (URP/Lit, high smoothness)
   - Furniture.mat (URP/Lit, various textures)

### Material Conversion Process
For existing materials:
1. Select all materials for a specific environment
2. Right-click > Rendering > Materials > Convert Selected Built-in Materials to URP
3. Check "Copy textures" option
4. Review results and adjust settings

## 6. Final Testing Checklist

After implementing all URP updates for MainScene:

1. **Visual Atmosphere**:
   - Verify each environment has distinctive mood/lighting
   - Check transitions between environments work smoothly
   - Ensure post-processing effects enhance rather than overwhelm

2. **Performance**:
   - Monitor frame rate in each environment
   - Test with real-time shadow quality settings at different levels
   - Optimize materials and light counts if needed

3. **Gameplay Impact**:
   - Ensure URP changes don't affect gameplay
   - Verify player abilities and mechanics work properly
   - Confirm UI elements render correctly

4. **Debugging**:
   - Look for any pink materials (missing shaders)
   - Fix any light artifacts or shadow issues
   - Resolve any console errors related to URP components

