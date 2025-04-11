# PDX Underground - Scene Setup Guide

This document explains how to set up the MainScene for PDX Underground using the provided SceneSetupUtility.

## Using the SceneSetupUtility

The SceneSetupUtility is an Editor script that creates the basic scene structure for PDX Underground. To use it:

1. Open your Unity project
2. In the top menu, navigate to **PDX Underground > Setup > Create Main Scene Structure**
3. The utility will automatically create or update the MainScene.unity file with the proper structure
4. After completion, you'll see a confirmation message in the console: "Main scene structure created successfully!"

If the MainScene.unity file already exists, the utility will open it and may overwrite existing GameObjects. Be cautious when using this on a scene that already has custom content.

## What Gets Created

The utility creates the following hierarchy and GameObjects:

```
MainScene
├── _GameSystems
│   ├── GameController
│   ├── EnvironmentController
│   └── UIController
├── _Level
│   ├── Ground
│   │   └── GroundPlane (basic floor)
│   ├── Streets
│   │   └── StreetEnv (placeholder)
│   ├── Tunnels
│   │   └── TunnelEnv (placeholder)
│   └── Speakeasy
│       └── SpeakeasyEnv (placeholder)
├── _Lighting
│   ├── DirectionalLight (main scene light)
│   ├── StreetLights
│   │   └── StreetGaslight (sample gaslight)
│   ├── TunnelLights
│   └── SpeakeasyLights
├── _Characters
│   └── PlayerSpawn (spawn point)
└── _Cameras
    └── MainCamera (main game camera)
```

### Created Components:

- **Basic ground plane**: Large flat surface for initial testing
- **Placeholder environments**: Simple objects marking the different environment areas
- **Camera**: Positioned to view the scene
- **Lighting**: Main directional light and a sample gaslight
- **Empty containers**: For GameController and other systems

## Next Steps After Scene Creation

After running the utility, you should:

1. **Add the Game Controller**:
   - Follow the GameController setup guide
   - Add MainGameController component to the GameController GameObject
   - Configure initial game state settings

2. **Add the Player**:
   - Place the Player prefab at the PlayerSpawn position
   - Configure player references
   - Ensure Character Controller settings are correct

3. **Complete the environments**:
   - Replace placeholder environments with actual level geometry
   - Add collision objects
   - Design each area (Streets, Tunnels, Speakeasy)

4. **Set up lighting**:
   - Add more gaslights to each environment
   - Configure GaslightFlicker components
   - Adjust environment-specific lighting

5. **Add UI elements**:
   - Create UI canvas
   - Add BuzzUI controller
   - Configure card UI elements

## Additional Manual Setup Requirements

These items need to be set up manually as they require specific configuration:

### 1. Scripts and Components

- Add `MainGameController` component to the GameController GameObject
- Add `GameEnvironmentController` to the EnvironmentController GameObject
- Create or assign a `BuzzUIController` for the UI system

### 2. Material Setup

- Create and assign materials to the ground and placeholder objects
- Set up lighting materials for gaslights

### 3. Environment Configuration

- Set up environment-specific settings in the GameEnvironmentController
- Configure transition points between environments
- Set up different gaslight settings for each environment

### 4. Camera Configuration

- Configure camera follow behavior (if implementing a follow camera)
- Adjust camera settings for different environments
- Set up any camera effects

### 5. Testing and Debugging

- Create simple UI elements for testing:
  - Button to change environments
  - Slider to modify buzz level
  - Button to trigger game state changes

## Troubleshooting

- **Missing scripts**: Ensure all required scripts are in your project before running the utility
- **Prefab issues**: Make sure to create prefabs following the prefab setup guides
- **Component references**: Some components may need manual reference assignment in the Inspector
- **Scene navigation**: Use Scene view gizmos to identify the different environment areas

## Additional Resources

For more detailed setup instructions, refer to:
- MainScene_Setup_Guide.txt in Setup_Guides folder
- Individual prefab setup guides for GameController, Player, and Gaslights

---

**Note**: This utility creates a basic starting point for your scene. You'll need to add your own creative elements, gameplay mechanics, and visual assets to complete the game environment.

