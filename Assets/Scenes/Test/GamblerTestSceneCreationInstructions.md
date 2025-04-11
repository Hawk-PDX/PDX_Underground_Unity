# GamblerTest Scene Creation Instructions

This document provides step-by-step instructions for creating the GamblerTest scene in Unity using the GamblerTestSceneSetupGuide.cs script.

## Prerequisites

Before you begin, ensure you have:
- Unity 2021.3 or newer installed
- PDX_Underground_Unity project open
- BuzzUIController.cs and related scripts properly imported
- Materials and shaders configured as per our previous setup

## Step 1: Create a New Scene

1. In Unity, go to **File > New Scene**
2. Select the **Basic (Built-in)** template
3. Immediately save the scene as `Assets/Scenes/Test/GamblerTest.unity`

## Step 2: Add the Setup Guide

1. Create an empty GameObject in the scene
   - In the Hierarchy panel, right-click and select **Create Empty**
   - Rename it to "SceneSetupGuide"
   
2. Add the GamblerTestSceneSetupGuide component
   - Select the "SceneSetupGuide" GameObject
   - In the Inspector panel, click **Add Component**
   - Search for "GamblerTestSceneSetupGuide" and add it

3. Assign reference assets (if available)
   - If you have icon sprites for buzz states, assign them in the Inspector
   - If you have prefabs for the character, assign them in the Inspector

## Step 3: Use the Automated Setup

The guide provides an automated setup process through buttons in the Inspector:

1. Click the **Create Scene Structure** button
   - This will create the basic hierarchy (Environment, Player, UI, etc.)
   - Verify in the Hierarchy that all objects were created properly

2. Click the **Set Up Materials** button
   - This will create and assign all required materials
   - Check the Project window to confirm materials were created

3. Click the **Set Up Components** button
   - This will add all necessary components and configure them
   - Verify in the Inspector that components were added to objects

4. Click the **Configure Test Elements** button
   - This will add testing elements like control panels
   - Verify the testing elements appear in the scene

## Step 4: Manual Adjustments

Some elements may need manual adjustment:

### Fixing Missing References

If you see console errors about missing references:

1. Find objects with missing scripts in the Hierarchy
2. Check which components should be there (based on GamblerTestSceneSetupGuide.cs)
3. Add those components manually and configure their properties

### Adjusting UI Elements

1. Select the Canvas object in the Hierarchy
2. Make sure its Scale Mode is set to "Scale With Screen Size"
3. Set Reference Resolution to 1920x1080
4. Set Match to 0.5 (width and height)

### Period-Appropriate Adjustments

1. Environment Styling:
   - Adjust lighting to have a warm amber glow (like gas lamps)
   - Apply wooden textures to props and floors
   - Add subtle fog for atmosphere

2. UI Styling:
   - Ensure UI elements have the weathered, period-appropriate look
   - Apply the BuzzUIShader to UI elements
   - Verify the card designs match 1850s-1870s playing card styles

## Step 5: Testing the Scene

1. Enter Play Mode to test the functioning scene
2. Use the provided test controls:
   - Buzz controls to adjust the character's buzz level
   - Environment toggle buttons to switch environments
   - Card mechanics testing buttons

3. Verify visual feedback is working:
   - Buzz meter updates properly
   - Card cooldown indicators function
   - Notifications appear for state changes

## Step 6: Performance Optimization

1. Check the Performance Monitor panel in-game
2. If FPS is below target:
   - Reduce particle effect counts
   - Simplify shader complexity
   - Optimize draw calls

## Troubleshooting

### Missing Shader
If you see pink materials, the custom shader is missing:
1. Check that BuzzUIShader.shader is imported correctly
2. Verify shader compilation has no errors
3. Reassign the shader to materials

### Script Errors
If script errors occur:
1. Check for missing namespaces or dependencies 
2. Verify GamblerCharacter, CardEffectsController, and BuzzUIController are properly implemented
3. Check for null reference exceptions in component connections

### UI Scaling Issues
If UI elements don't appear correctly:
1. Check Canvas Scaler settings
2. Verify RectTransform settings for each UI element
3. Ensure the canvas is set to Screen Space - Overlay

## Final Checklist

Before considering the scene complete, verify:

- [x] Scene has complete hierarchy structure
- [x] All materials are properly assigned
- [x] All required components are attached and configured
- [x] UI elements are properly positioned and scaled
- [x] Test controls are functional
- [x] Period-appropriate styling is consistent
- [x] Performance is acceptable (60+ FPS)
- [x] No errors or warnings in the console

