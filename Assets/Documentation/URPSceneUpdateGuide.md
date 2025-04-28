# Scene Updates for URP

After the core URP settings are applied and Unity is restarted, each scene needs to be updated. Work through these steps for each scene:

## Scenes to Update
1. Assets/Scenes/SetupScene.unity
2. Assets/Scenes/Main/MainScene.unity
3. Assets/Init.unity
4. Assets/1.unity

## Per-Scene Updates

### 1. Camera Components
For each scene:
1. Open the scene
2. Find all Camera components in the Hierarchy
3. If missing, add "Universal Additional Camera Data" component:
   - Select the camera
   - In Inspector, click "Add Component"
   - Search for "Universal Additional Camera Data"
   - Or Unity may prompt you to upgrade the camera
4. Configure camera settings:
   - HDR: Enable for High quality only (main scenes)
   - Anti-aliasing method: 
     * Set to "MSAA" for High/Medium quality
     * "Post Processing" for Low quality
   - Post-processing layer mask: "Everything" for most cameras
   - Renderer: Set to "Forward Renderer"
   - Stack: If using overlay cameras, set up camera stacking

### 2. Post-Processing Setup
For each scene:
1. Create a new "Global Volume" GameObject if not present:
   - GameObject > Volume > Global Volume
   - Position it in a logical location in the scene
2. Configure the Volume component:
   - Ensure "Is Global" is checked
   - Set priority (higher numbers override lower)
   - Create new Volume Profile asset or use existing
3. Add post-processing effects to the profile:
   - Click "Add Override" button in the Volume component
   - Start with these basics:
     * Bloom (subtle for realism, stronger for stylized)
     * Tonemapping (ACES for cinematic, Neutral for accuracy)
     * Color Adjustments (adjust temperature, tint, etc.)
     * Vignette (subtle darkening at screen edges)
   - Consider these additional effects as needed:
     * Ambient Occlusion (for environmental depth)
     * Depth of Field (for focus effects)
     * Motion Blur (for action sequences)
     * Film Grain (for atmosphere)

### 3. Lighting Updates
1. Update all lights:
   - Check "Use Shadow Mask" for mixed lighting
   - Ensure intensity values look correct with Linear color space
   - You may need to reduce intensity values in Linear
2. Check shadow settings:
   - Update shadow resolution and distance to match URP asset
   - Adjust bias settings if shadows appear to "float"
3. Review reflection probes:
   - Regenerate reflection probes
   - Check box projection settings
   - Update probe intensity for Linear color space
4. Scene-specific lighting:
   - For indoor scenes: Check light bounces and indirect lighting
   - For outdoor scenes: Update skybox and ambient settings

### 4. Material Updates
1. Identify any Standard shader materials
2. Convert them to URP compatible shaders:
   - Select materials in Project view
   - Right-click > Rendering > Materials > Convert Selected Built-in Materials to URP
3. For particle effects:
   - Check for any custom particle materials
   - Update to URP-compatible particle shaders
4. For custom shaders:
   - These will need to be manually rewritten for URP
   - Look for "pink" materials that indicate missing shaders

### 5. Special Considerations for Speakeasy Scene
1. Volume components for indoor/outdoor transitions:
   - Create two separate Volume profiles (indoor and outdoor)
   - Set up volume triggers or code transitions between them
   - Update SpeakeasyEntrance.cs code to use URP Volume references
2. Check lighting effects:
   - Doorway light settings may need adjustment
   - Lantern lights should use URP light cookies if applicable
3. Verify particle effects:
   - Dust particles should use URP-compatible materials
   - Ensure particle lighting interactions work correctly
4. Post-processing profiles:
   - Create atmospheric indoor profile with stronger effects
   - Create cleaner outdoor profile with subtle effects
   - Test transitions between them

## Testing Each Scene
After updating:
1. Test each scene independently
2. Check for:
   - Proper lighting and shadows
   - Post-processing effects working correctly  
   - No pink/missing materials
   - No rendering artifacts
   - Proper camera settings and stacking
3. Test scene transitions to ensure consistent look

## Common Issues and Solutions
- **Pink Materials**: Missing shader, needs conversion to URP
- **Dark Scenes**: Lighting intensity values need adjustment for Linear color space
- **Missing Post-Processing**: Volume component not configured or profile missing effects
- **Flickering Shadows**: Shadow bias or cascade settings need adjustment
- **Performance Issues**: Lower quality URP asset may be needed for target platform

