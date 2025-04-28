# URP Optimization Guide for PDX Underground

This guide provides specific URP settings optimized for PDX Underground's atmospheric and lighting requirements, focusing on gaslight effects, environment transitions, and historical atmosphere.

## High Quality Profile Settings

### Core Settings
1. In UniversalRP-HighQuality.asset:
   - HDR: Enabled (required for gaslight bloom)
   - MSAA: 2x (good balance for performance)
   - Render Scale: 1 (full resolution)
   - Color Grading Mode: High Quality (for better atmosphere)

### Lighting Optimizations
1. Main Light:
   - Shadow Resolution: 2048 (for sharp shadows)
   - Soft Shadows: Enabled
   - Shadow Cascades: 2 (balance of quality/performance)

2. Additional Lights (Gaslights):
   - Per Object Limit: 8
   - Shadows: Enabled
   - Cookie Resolution: 2048
   - Shadow Resolution: 1024
   - Shadow Bias: 1.0
   - Normal Bias: 1.0

### Post-Processing Settings
1. Enable required buffers:
   - Depth Texture: Required
   - Opaque Texture: Required
   - HDR: Required for bloom

2. Volume Framework:
   - Update Mode: Every Frame
   - Use SRP Batcher: Enabled
   - Mixed Lighting: Enabled

## Environment-Specific Optimizations

### Streets Environment
1. Lighting:
   - Main Light: Low intensity (0.3-0.4)
   - Ambient Light: Cool blue tint
   - Gaslights: Warm color (2200K)

2. Post-Processing:
   - Bloom: Subtle (0.15 intensity)
   - Color Grading: Cool tint
   - Vignette: Medium (0.35)

### Tunnels Environment
1. Lighting:
   - Main Light: Disabled or very low
   - Ambient Light: Dark teal
   - Gaslights: Warmer (1800K), higher flicker rate

2. Post-Processing:
   - Bloom: Moderate (0.25 intensity)
   - Color Grading: Green-blue tint
   - Fog: Exponential density 0.03
   - Vignette: Strong (0.45)

### Speakeasy Environment
1. Lighting:
   - Main Light: Disabled
   - Ambient Light: Warm amber
   - Gaslights: Warmest (2400K), low flicker rate

2. Post-Processing:
   - Bloom: Strong (0.4 intensity)
   - Color Grading: Warm golden tint
   - Vignette: Strong (0.45)

## Performance Considerations

### CPU Optimizations
1. Limit active lights per scene:
   - Streets: 15-20 gaslights maximum
   - Tunnels: 8-12 flickering lights
   - Speakeasy: 10-15 stable lights

2. Batch similar materials:
   - Group buildings with same materials
   - Reuse gaslight materials

### GPU Optimizations
1. Shadow optimizations:
   - Lower resolution shadows for distant lights
   - Disable shadows for smallest lights

2. Limit overdraw:
   - Avoid transparent materials when possible
   - Use alpha cutout for simple transparency

## Console Commands for Testing

Test your settings with these console commands (requires adding to a debug script):

```csharp
// Switch environments
void TestStreets() {
    var cameraSetup = Camera.main.GetComponent<URPCameraSetup>();
    cameraSetup.SwitchToEnvironment(URPCameraSetup.EnvironmentType.Streets);
}

void TestTunnels() {
    var cameraSetup = Camera.main.GetComponent<URPCameraSetup>();
    cameraSetup.SwitchToEnvironment(URPCameraSetup.EnvironmentType.Tunnels);
}

void TestSpeakeasy() {
    var cameraSetup = Camera.main.GetComponent<URPCameraSetup>();
    cameraSetup.SwitchToEnvironment(URPCameraSetup.EnvironmentType.Speakeasy);
}
```

