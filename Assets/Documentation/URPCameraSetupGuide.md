# URP Camera Setup Guide for PDX Underground

This guide provides instructions for configuring your camera system to work with Universal Render Pipeline (URP) in PDX Underground, focusing on environment-specific post-processing effects.

## Camera Setup for URP

### Main Camera Configuration

1. Under _Cameras/MainCamera:
   - Add URPCameraSetup component
   - Configure settings:
     ```
     URP Camera Settings:
     - Render Post Processing: Enabled
     - Anti Aliasing Mode: FXAA
     - Volume Layer Mask: Everything
     
     Volume References:
     - Global Profile: GlobalPostProcessing
     - Streets Profile: StreetsProfile
     - Tunnels Profile: TunnelsProfile
     - Speakeasy Profile: SpeakeasyProfile
     
     Global Volume:
     - Create Global Volume: True
     - Global Volume Priority: 0
     ```

2. Camera Position and Settings:
   - Position: Slightly above and behind player spawn point
   - Rotation: Looking slightly downward
   - Field of View: 60 (default)
   - Near Clip Plane: 0.3
   - Far Clip Plane: 1000

3. Required Components:
   - Camera (main camera tag)
   - URPCameraSetup
   - Universal Additional Camera Data (added automatically)
   - Audio Listener

### Environment Transition Implementation

To trigger environment profile changes in your scripts, add this code:

```csharp
// Get reference to the URPCameraSetup
URPCameraSetup cameraSetup = Camera.main.GetComponent<URPCameraSetup>();

// Change to appropriate environment
if (cameraSetup != null)
{
    // When entering streets
    cameraSetup.SwitchToEnvironment(URPCameraSetup.EnvironmentType.Streets);
    
    // When entering tunnels
    // cameraSetup.SwitchToEnvironment(URPCameraSetup.EnvironmentType.Tunnels);
    
    // When entering speakeasy
    // cameraSetup.SwitchToEnvironment(URPCameraSetup.EnvironmentType.Speakeasy);
}
```

Add this to:
- AreaTransitionTrigger script
- SpeakeasyEntrance script
- GameEnvironmentController script

## Verifying URP Camera Setup

After setting up the camera:

1. Enter Play mode 
2. Look for URP-specific effects:
   - Bloom around gaslights
   - Proper ambient light
   - Color adjustments changing with environment
   - Fog effects in tunnels

3. Check Console for any error messages:
   - "Added Universal Additional Camera Data to camera" is a normal message
   - "URP Camera settings configured" confirms proper setup
   - "Switched to [Environment] environment profile" when transitioning

## Troubleshooting

If post-processing effects aren't visible:

1. **Missing profile references**:
   - Create Volume profiles if missing (follow URPRendererSetup.md guide)
   - Assign correct paths in URPCameraSetup component

2. **Camera component issues**:
   - Check that Universal Additional Camera Data component exists
   - Verify "Render Post Processing" is enabled
   - Make sure correct Layer Mask is set (typically "Everything")

3. **Global volume problems**:
   - Ensure a global volume exists in the scene
   - Check that volume weight is > 0
   - Verify profile is assigned to volume

## Special Camera Effects

### Tunnel Vision Effect
For advanced speakeasy atmosphere, add these effects:

1. Vignette intensity transitioning:
   ```csharp
   // Get Vignette component from volume profile
   VolumeProfile profile = _globalVolume.profile;
   if (profile.TryGet<Vignette>(out var vignette))
   {
       // Gradually increase vignette as buzz level decreases
       float vignetteFactor = 1 - (currentBuzzLevel / maxBuzzLevel);
       vignette.intensity.value = Mathf.Lerp(0.3f, 0.7f, vignetteFactor);
   }
   ```

2. Color shift with buzz level:
   ```csharp
   // Get Color Adjustments from volume profile
   if (profile.TryGet<ColorAdjustments>(out var colorAdjustments))
   {
       // Color shifts to red/blue as buzz decreases
       float buzzFactor = 1 - (currentBuzzLevel / maxBuzzLevel);
       colorAdjustments.saturation.value = Mathf.Lerp(0, -50, buzzFactor);
       colorAdjustments.colorFilter.value = Color.Lerp(Color.white, 
           new Color(1.1f, 0.9f, 0.9f), buzzFactor);
   }
   ```

### Day/Night Cycle
If implementing time progression:

1. Add Color Curves component to global volume
2. Create day/night cycle manager:
   ```csharp
   void UpdateTimeOfDay(float timeOfDay) // 0-24 hour format
   {
       // Get light
       Light mainLight = GetComponent<Light>();
       
       // Update light intensity and color
       float dayFactor = GetDaylightFactor(timeOfDay);
       mainLight.intensity = Mathf.Lerp(0.1f, 1.5f, dayFactor);
       
       // Update post-processing
       URPCameraSetup cameraSetup = Camera.main.GetComponent<URPCameraSetup>();
       if (cameraSetup != null)
       {
           // Switch between day/night profiles
           // Or modify parameters directly
       }
   }
   ```

