# PDX Underground - URP Implementation Plan

## Priority 1: Core Settings Updates

1. **Switch to Linear Color Space**
   - Open Project Settings > Player
   - Set Color Space to Linear (currently using Gamma)
   - This will provide more accurate lighting and better visual quality
   - Note: This change requires restarting Unity and recompiling shaders

2. **Verify Graphics Settings**
   - Ensure the URP asset is set as the Scriptable Render Pipeline Asset in Graphics settings
   - Check Quality Settings to verify the URP asset is assigned to each quality level
   - Add Low Quality URP asset if not present (you already have Medium and High)

3. **Update Scripting Define Symbols**
   - Update Project Settings > Player > Scripting Define Symbols
   - Add `USING_URP` define to all platforms
   - This will eliminate conditional compilation issues in scripts

## Priority 2: Scene Updates

1. **Camera & Volume Setup for each scene**
   - Update all cameras with Universal Additional Camera Data component
   - Add Global Volume to each scene with appropriate profile
   - Check existing camera stacking (overlay cameras)

2. **Scene-specific Post-Processing**
   - Create a Volume Profile per scene if needed
   - Configure post-processing effects:
     * Bloom
     * Tonemapping
     * Color adjustments
     * Depth of field
   - Create postprocess volumes for special areas (tunnel entrances, speakeasy)

## Priority 3: Material & Shader Conversion

1. **Audit material shaders**
   - Scan project for non-URP materials/shaders
   - Convert Standard materials to Universal Lit
   - Check particle materials and update to URP shaders

2. **Fix any material render issues**
   - Transparent materials may need adjustments
   - Check for missing shader variants
   - Update texture import settings if needed (normal maps, etc.)

## Priority 4: Lighting Optimizations

1. **Update Light components**
   - Check shadow types for all lights
   - Set main directional light shadow resolution appropriately
   - Configure light layers if used

2. **Reflection Probes**
   - Update reflection probe settings for URP compatibility
   - Check box projection and blending settings

## Priority 5: Performance Tuning

1. **Fine-tune URP settings per platform**
   - Adjust shadow distance and cascade settings
   - Consider render scale adjustments for mobile
   - Configure appropriate MSAA level (2x for high quality, off for low)

2. **Enable and configure SRP Batcher**
   - Already enabled in asset, check compatibility with shaders
   - Review material count and batching

## Next Steps

Would you like me to start implementing these changes, beginning with:

1. Switching to Linear Color Space
2. Adding the `USING_URP` define symbol
3. Scanning for non-URP materials

Or do you prefer to focus on a specific area of this plan first?

