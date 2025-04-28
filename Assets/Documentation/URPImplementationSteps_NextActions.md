# Next URP Implementation Actions

After creating the Low Quality URP asset, please proceed with the following critical settings updates in Unity Editor:

## 1. Update Graphics Settings

1. Open Project Settings (Edit > Project Settings)
2. Select the Graphics category
3. Locate "Scriptable Render Pipeline Settings" 
4. Set it to your High Quality URP Asset (Assets/Settings/URP/UniversalRP-HighQuality.asset)
5. Enable "Lights Use Linear Intensity" checkbox (important for Linear color space)
6. Verify that under "SRP Default Settings" the URP asset is properly referenced

## 2. Update Color Space Setting

1. In Project Settings, select the Player category
2. In the "Other Settings" section, locate "Color Space"
3. Change it from "Gamma" to "Linear"
4. Note: This will trigger a shader recompilation. It might take a few minutes.
5. You may need to restart Unity after this change

## 3. Configure Quality Settings

1. In Project Settings, select the Quality category
2. For each quality level (Low, Medium, High):
   - Assign the corresponding URP asset:
     * High: UniversalRP-HighQuality.asset
     * Medium: UniversalRP-MediumQuality.asset
     * Low: UniversalRP-LowQuality.asset (newly created)

## 4. Add USING_URP Define Symbol

1. In Project Settings, select the Player category
2. In the "Other Settings" section, locate "Scripting Define Symbols"
3. Add "USING_URP" to the existing symbols
4. The final value should look similar to: 
   ```
   UNITY_POST_PROCESSING_STACK_V2;USING_URP
   ```
5. Click Apply

## 5. Restart Unity

After applying these changes, it's recommended to:
1. Save your project
2. Close Unity
3. Reopen the project
4. Allow shader compilation to complete

## Next Steps After Restart

After the restart, we'll proceed with:
1. Checking all materials for compatibility
2. Updating scene cameras
3. Setting up post-processing volumes

