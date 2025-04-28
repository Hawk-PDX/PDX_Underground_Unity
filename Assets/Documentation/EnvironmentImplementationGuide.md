# PDX Underground Environment Implementation Guide

This document provides step-by-step instructions for setting up the historically accurate 1880s Portland environment using the custom editor tools we've created.

## Overview of Implementation Steps

1. Generate textures using the EnvironmentTextureGenerator
2. Create materials using the EnvironmentMaterialGenerator
3. Create street environment prefabs using the StreetEnvironmentGenerator
4. Set up the complete scene using PDXUndergroundSceneSetup

## Step 1: Generate Environment Textures

First, we'll create the period-appropriate textures for our street environment:

1. Open the Unity Editor
2. Go to the top menu: **PDX Underground > Create > Environment Textures**
3. In the opened window, configure the following settings:
   - **Cobblestone Texture Size**: 2048 x 2048
   - **Sidewalk Texture Size**: 1024 x 1024
   - **Building Facade Size**: 2048 x 2048
   - **Cobblestone Color**: RGB(0.45, 0.45, 0.45)
   - **Sidewalk Color**: RGB(0.55, 0.45, 0.3)
   - **Building Facade Color**: RGB(0.6, 0.55, 0.5)
4. Ensure both "Create Street Textures" and "Create Building Textures" are checked
5. Click "Generate Textures"

This will create the following textures:
- `Assets/Textures/Environment/Streets/cobblestone_diffuse.png`
- `Assets/Textures/Environment/Streets/cobblestone_normal.png`
- `Assets/Textures/Environment/Streets/wooden_sidewalk_diffuse.png`
- `Assets/Textures/Environment/Streets/wooden_sidewalk_normal.png`
- `Assets/Textures/Environment/building_facade_diffuse.png`
- `Assets/Textures/Environment/building_facade_normal.png`

## Step 2: Create Environment Materials

Next, we'll create materials that use these textures and have period-appropriate properties:

1. Go to the top menu: **PDX Underground > Create > Environment Materials**
2. In the opened window, ensure all options are checked:
   - Create Street Materials
   - Create Tunnel Materials
   - Create Speakeasy Materials
3. Click "Generate Materials"

This will create the following materials:
- `Assets/Materials/Environment/Streets/CobblestoneStreet.mat`
- `Assets/Materials/Environment/Streets/WoodenSidewalk.mat`
- `Assets/Materials/Environment/Streets/Lamppost.mat`
- `Assets/Materials/Environment/Streets/GaslightGlass.mat`
- `Assets/Materials/Environment/BuildingFacade.mat`
- Additional materials for tunnels and speakeasy environments

## Step 3: Fine-Tune Materials (Optional)

For historically accurate results, you may want to fine-tune some material properties:

1. Select `Assets/Materials/Environment/Streets/CobblestoneStreet.mat`
2. In the Inspector, adjust the following properties:
   - **Smoothness**: 0.35 (slightly wet appearance)
   - **Metallic**: 0 (non-metallic stone)

3. Select `Assets/Materials/Environment/Streets/WoodenSidewalk.mat`
4. In the Inspector, adjust:
   - **Smoothness**: 0.2 (weathered wood)
   - **Metallic**: 0 (non-metallic wood)
   - **Color**: Slightly greyer to match weathered appearance

5. For Gaslight materials, ensure emission settings:
   - **Emission Color**: Warm yellow-orange (1.0, 0.75, 0.4)
   - **Emission Intensity**: 0.5 - 0.8 (historically accurate gas lamp brightness)

## Step 4: Create Street Environment Prefabs

Now we'll generate street prefabs using these materials:

1. Go to the top menu: **PDX Underground > Create > Street Environment**
2. If prompted about materials, click "Try Load Materials"
3. Configure the following:
   - **Block Size**: 61 (historically accurate for 1880s Portland)
   - **Street Width**: 6
   - **Sidewalk Width**: 2
   - **Building Height**: 12
4. Click "Generate Street Environment"

This will create street prefabs in `Assets/Prefabs/Environment/Streets/`

## Step 5: Create the Complete Street Scene

Finally, we'll create a complete scene with our street environment:

1. Go to the top menu: **PDX Underground > Setup > Create Street Environment Scene**
2. Configure the settings:
   - **Grid Size**: 3x3 (creates a 3x3 block area)
   - **Block Size**: 61
   - **Setup UI Elements**: Checked
   - **Generate NavMesh**: Checked
3. Click "Try Load Resources" to confirm all assets are available
4. Click "Create Street Environment Scene"

This will create a complete scene with:
- Historically accurate street grid with 61m blocks
- Building facades with proper 1880s Portland architecture
- Gaslights with flicker effects
- UI elements with period-appropriate styling
- NavMesh for NPC navigation

## Historical Accuracy Notes

The implementation follows these historical details:
- Portland's grid system was established in the 1850s with 200ft (61m) blocks
- Streets were typically cobblestone or packed dirt in the 1880s
- Sidewalks were primarily wooden planks that weathered gray from rain
- Gaslights provided dim, warm illumination with slight flicker
- Buildings were typically 2-3 stories with simple, functional facades

## Performance Considerations

The generated environment includes:
- LOD setup for distant buildings
- Static batching for street elements
- Optimized lighting with strategically placed light probes
- NavMesh built with proper surfaces marked as walkable

