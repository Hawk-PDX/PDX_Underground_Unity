# URP Renderer Feature Setup

## Overview
These instructions explain how to add the necessary renderer features to create the atmospheric effects needed for PDX Underground, including gaslight glow, fog effects, and color grading for different environments.

## Setup Steps

### 1. Add Post-Processing Volume Profiles

First, create the required Volume Profiles for different environments:

1. In the Project window, right-click > Create > Volume Profile
2. Create the following profiles:
   - **GlobalPostProcessing**: Default post-processing for the entire game
   - **StreetsProfile**: For street environments
   - **TunnelsProfile**: For underground tunnel environments 
   - **SpeakeasyProfile**: For interior speakeasy environments

### 2. Configure Post-Processing Profiles

#### Global Profile Settings:
1. Select **GlobalPostProcessing** profile
2. Add these effects:
   - **Bloom**:
     * Threshold: 0.9
     * Intensity: 0.25
     * Scatter: 0.7
   - **Tonemapping**:
     * Mode: ACES
   - **Color Adjustments**:
     * Post Exposure: 0.1
     * Contrast: 10
     * Temperature: 0
     * Tint: 0
   - **Vignette**:
     * Intensity: 0.25
     * Smoothness: 0.3
     * Roundness: 1

#### Streets Profile Settings:
1. Select **StreetsProfile** profile
2. Add these effects:
   - **Bloom** (for gaslight glow):
     * Threshold: 0.95
     * Intensity: 0.15
   - **Color Adjustments** (cool night tint):
     * Temperature: -15
     * Tint: +5
   - **Vignette** (dark edges):
     * Intensity: 0.35
     * Smoothness: 0.4

#### Tunnels Profile Settings:
1. Select **TunnelsProfile** profile
2. Add these effects:
   - **Bloom** (moisture effects):
     * Threshold: 0.8
     * Intensity: 0.25
   - **Color Adjustments** (green/blue tint):
     * Temperature: -25
     * Tint: -10
   - **Fog**:
     * Color: Dark teal (#102030)
     * Mode: Exponential
     * Density: 0.03

#### Speakeasy Profile Settings:
1. Select **SpeakeasyProfile** profile
2. Add these effects:
   - **Bloom** (warm glow):
     * Threshold: 0.7
     * Intensity: 0.4
   - **Color Adjustments** (warm tint):
     * Temperature: +20
     * Tint: +10
   - **Vignette** (intimate atmosphere):
     * Intensity: 0.45
     * Smoothness: 0.3

### 3. Add Global Volume to Main Scene

1. In your main scene, create a GameObject named "Global Volume"
2. Add the Volume component
3. Check "Is Global"
4. Assign the GlobalPostProcessing profile
5. Set Priority to 0 (baseline)

### 4. Configure URP Renderer Settings

1. In the Project window, select Assets/Settings/URP/UniversalRP-PipelineAsset_Renderer.asset
2. In the Inspector, under Universal Renderer:
   - Enable **Depth Texture**: Required for fog and post-processing effects
   - Enable **Opaque Texture**: Required for glass and transparent effects
   - Set **Opaque Downsampling**: None (for quality)
   - Enable **HDR**: Required for bloom effects
   - Set **MSAA**: 2x (balance of quality and performance)
   - Set **Render Scale**: 1

### 5. Update Project Settings

1. Open Project Settings (Edit > Project Settings)
2. Graphics Settings:
   - **Color Space**: Set to Linear (for better lighting quality)
   - **HDRP/URP Default Settings**: Set to High Quality URP Asset
   - Enable **Lights Use Linear Intensity**

3. Player Settings:
   - Under "Other Settings", add `USING_URP` to Scripting Define Symbols
   - This should be added to existing symbols like: `UNITY_POST_PROCESSING_STACK_V2;USING_URP`

### 6. Check Camera Settings

Ensure your main camera has the necessary components:

1. Select the Main Camera in each scene
2. Add Universal Additional Camera Data if not present
3. Set the following:
   - **Rendering**: Base
   - **Render Post Processing**: Enabled
   - **Anti-aliasing**: FXAA
   - **Depth Texture**: Auto
   - **Opaque Texture**: Auto

### 7. Test the Lighting Effects

1. After completing setup, enter Play mode
2. Check gaslight effects:
   - Bloom should create a soft glow around lights
   - Shadows should appear natural and properly biased
   - Color grading should be noticeable between different environments

3. Verify lighting in each environment:
   - Streets: Cool ambient lighting with warm point lights
   - Tunnels: Dark with limited direct lighting, foggy atmosphere
   - Speakeasy: Warm ambient lighting with strong local lights

## Troubleshooting

If lighting effects don't appear as expected:

1. **No post-processing effects visible**:
   - Ensure Camera has "Render Post Processing" enabled
   - Check that Volume component is active and has a profile assigned
   - Verify Camera's Volume Mask includes the layer where the Volume is placed

2. **Bloom not visible on lights**:
   - Ensure HDR is enabled in URP settings
   - Check that light intensity is high enough to exceed bloom threshold
   - Verify that linear color space is being used

3. **Missing shadows or lighting artifacts**:
   - Check shadow distance and cascade settings in URP asset
   - Adjust shadow bias values to reduce shadow acne
   - Enable/adjust Normal Bias to fix shadow alignment issues

