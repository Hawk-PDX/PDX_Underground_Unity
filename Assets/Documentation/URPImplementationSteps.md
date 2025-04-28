# URP Implementation Steps

## Step 1: Core Graphics Settings
1. In Unity Editor, open Project Settings > Graphics
2. Set 'Scriptable Render Pipeline Settings' to High Quality URP Asset
   - Asset Path: Assets/Settings/URP/UniversalRP-HighQuality.asset
3. Verify SRP Default Settings references the correct URP asset
   - This is already correctly set in your project
4. Enable "Lights Use Linear Intensity" setting in the Graphics settings panel

## Step 2: Color Space Update
1. Open Project Settings > Player
2. Under "Other Settings":
   - Set "Color Space" to "Linear" (currently set to Gamma)
   - This will trigger a shader recompilation
   - Wait for Unity to finish recompiling (this might take several minutes)
   - You may need to restart Unity after this change

## Step 3: Quality Settings

### Create Low Quality URP Asset
1. In Unity Editor, go to the Project window
2. Navigate to Assets/Settings/URP folder
3. Right-click in the folder > Create > Rendering > Universal Render Pipeline > Pipeline Asset (Low Quality)
4. Name it "UniversalRP-LowQuality"
5. Select the created asset and in the Inspector set these key properties:
   * Renderer: Use the same Forward Renderer as other URP assets
   * Depth Texture: Disabled
   * Opaque Texture: Disabled
   * HDR: Disabled
   * MSAA: 1x (disabled)
   * Main Light: Directional
   * Cast Shadows: Enabled
   * Shadow Resolution: 1024
   * Additional Lights: Per Vertex
   * Additional Light Shadows: Disabled
   * Shadow Distance: 30
   * Shadow Cascades: 1
   * Soft Shadows: Disabled
   * SRP Batcher: Enabled

### Apply Quality Settings
1. Open Project Settings > Quality
2. For each quality level:
   - Set the Rendering Pipeline Asset
   - High: UniversalRP-HighQuality
   - Medium: UniversalRP-MediumQuality  
   - Low: UniversalRP-LowQuality (newly created)

## Step 4: Scripting Define Symbols
1. Open Project Settings > Player
2. Under "Other Settings" > "Scripting Define Symbols"
3. Add: USING_URP
4. This should be added to the existing symbols, not replacing:
   ```
   UNITY_POST_PROCESSING_STACK_V2;USING_URP
   ```

## Step 5: Scene Camera Updates
1. For each scene in your project:
   - Ensure all cameras have the Universal Additional Camera Data component
   - Add a global Volume with a profile if missing
   - Check for camera stacking settings if using overlay cameras

## Step 6: Material Conversion
1. In the Project window, locate materials using Standard shader
2. Select all these materials and:
   - Right-click > Rendering > Materials > Convert Selected Built-in Materials to URP
   - Check "copy textures" option

## Step 7: Verify Post-Processing
1. Check if you're using Post-Processing Stack V2
2. Begin migration to URP's built-in post-processing:
   - Create Volume Profiles with effects needed
   - Remove any legacy Post Process Layer components
   - Add Volume components to cameras or scenes

## Step 8: Testing
1. After making these changes, check for:
   - Missing shaders (pink materials)
   - Incorrect lighting
   - Rendering artifacts
   - Performance issues

## Important Notes
- The color space change will require a full shader recompilation
- Some materials may need manual adjustments after conversion
- Third-party assets might need updates or URP-compatible versions
- Custom shaders will need to be rewritten for URP

- **Missing shadows or lighting artifacts**:
   - Check shadow distance and cascade settings in URP asset
   - Adjust shadow bias values to reduce shadow acne
   - Enable/adjust Normal Bias to fix shadow alignment issues

## Verify Implementation

After setting up URP, check that:

1. Camera transitions work:
   - Streets environment shows cool night tint
   - Tunnels show proper fog and moisture effects
   - Speakeasy shows warm indoor lighting

2. Lighting effects:
   - Gaslights show proper bloom
   - Shadows are correctly rendered
   - Post-processing effects transition smoothly

3. In the Console window, look for:
   - "URPCameraSetup not found" warnings are resolved
   - No shader compilation errors
   - No missing post-processing components

If you see any issues, verify that:
- USING_URP define symbol is set in Project Settings > Player > Scripting Define Symbols
- URP package is properly installed
- All materials are converted to URP
- Camera references are correctly set up with Universal Additional Camera Data

