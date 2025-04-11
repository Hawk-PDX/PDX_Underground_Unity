# PDX Underground Test Scene Setup Guide

## Overview
This guide will walk you through the complete process of setting up, configuring, and running the PDX Underground test scene in Unity. The test scene is designed to provide a controlled environment for testing gameplay mechanics, UI elements, and progression systems, while maintaining compatibility with the production environment.

## Table of Contents
1. [Scene Creation and Configuration](#1-scene-creation-and-configuration)
2. [Test Setup Instructions](#2-test-setup-instructions)
3. [Testing Procedures](#3-testing-procedures)
4. [Troubleshooting](#4-troubleshooting)

---

## 1. Scene Creation and Configuration

### Step-by-Step Unity Editor Instructions

#### 1.1 Create the Test Scene
1. Open Unity and navigate to the PDX Underground project
2. Go to File > New Scene
3. Save the scene as `PDXUndergroundTestScene.unity` in the `Assets/Scenes/Test` directory
4. The scene will start with just a Main Camera and a Directional Light

#### 1.2 Add the TestSceneLayout Component
1. Create an empty GameObject in the scene and name it `SceneSetupHelper`
2. With the `SceneSetupHelper` object selected, click "Add Component" in the Inspector
3. Search for and add the `TestSceneLayout` script
4. The Inspector will now show configuration options for the TestSceneLayout

#### 1.3 Generate the Test Scene Structure
1. With the `SceneSetupHelper` object selected, look for the gear icon (context menu) in the TestSceneLayout component
2. Click the gear icon and select "Create Complete Scene Hierarchy"
3. This will automatically create:
   - TestSceneSetup object with TestSceneInitializer component
   - Environment object with ground, walls, props, and spawn points
   - UI object with buzz UI and debug UI elements
   - Lighting object with directional light

#### 1.4 Save the scene after the hierarchy is created

### Component Setup and Prefab Configuration

#### 2.1 Configure the TestSceneInitializer
1. Select the `TestSceneSetup` object in the Hierarchy
2. In the Inspector, configure the TestSceneInitializer component:
   - Set "Enable Test Controls" to true
   - Set "Enable Debug UI" to true
   - Set "Auto Setup Test Environment" to true

#### 2.2 Create and Assign Required Prefabs
You need to create and assign these prefabs to the TestSceneInitializer:

1. **Player Prefab**:
   - Create an empty GameObject in the scene
   - Add the `PlayerPrefab` script to it
   - Use the context menu to select "Create Player Prefab Structure"
   - Save this as a prefab in the `Assets/Prefabs` directory
   - Assign this prefab to the "Player Prefab" field in TestSceneInitializer

2. **MainGameController Prefab**:
   - Create an empty GameObject in the scene
   - Add the `MainGameControllerPrefab` script to it
   - Use the context menu to select "Create MainGameController Prefab"
   - Save this as a prefab in the `Assets/Prefabs` directory
   - Assign this prefab to the "Main Game Controller Prefab" field in TestSceneInitializer

3. **BuzzUI Prefab**:
   - Create an empty GameObject in the scene
   - Add the `BuzzUIPrefab` script to it
   - Use the context menu to select component's functions to set up the UI
   - Save this as a prefab in the `Assets/Prefabs` directory
   - Assign this prefab to the "Buzz UI Controller Prefab" field in TestSceneInitializer

#### 2.3 Configure Environment Settings
1. Select the `Environment` object in the Hierarchy
2. Adjust materials and transforms as needed:
   - Assign materials to the ground and walls
   - Adjust the size of the environment if needed
   - Position props and obstacles as needed for testing

### Test Environment Initialization

#### 3.1 Set Up Initial Test State
1. Select the `SceneSetupHelper` object in the Hierarchy
2. Use the context menu to select "Setup For Test Run"
3. This will configure all components for testing, including:
   - Setting the TestSceneInitializer to test mode
   - Configuring test-specific settings
   - Setting up the debug UI and controls

#### 3.2 Configure Test Parameters
1. Select the `TestSceneSetup` object in the Hierarchy
2. In the Inspector, configure test-specific parameters:
   - Set starting buzz level if needed
   - Configure environment settings
   - Set up initial game state for testing

---

## 2. Test Setup Instructions

### Scene Hierarchy Organization

The test scene is organized into four main parent objects:

1. **TestSceneSetup**: Contains the TestSceneInitializer component that manages the entire test setup
   - Responsible for initializing the scene
   - Controls test mode settings
   - Manages references to other components

2. **Environment**: Contains all environment objects and structures
   - Ground: The main playing surface
   - Walls: Boundary walls that define the play area
   - Props: Interactive and decorative objects
   - SpawnPoints: Designated spawn locations for player and NPCs

3. **UI**: Contains all UI elements
   - BuzzUI: Interface elements for the buzz system
   - DebugUI: Test-specific UI components for debugging

4. **Lighting**: Contains all lighting elements
   - DirectionalLight: Main light source for the scene

#### Required Hierarchy Structure
```
PDXUndergroundTestScene
├── TestSceneSetup            (contains TestSceneInitializer)
├── Environment
│   ├── Ground                
│   ├── Walls
│   │   ├── NorthWall
│   │   ├── SouthWall
│   │   ├── EastWall
│   │   └── WestWall
│   ├── Props                 
│   └── SpawnPoints
│       └── PlayerSpawn
├── UI
│   ├── BuzzUI                (contains BuzzUIPrefab)
│   └── DebugUI
└── Lighting
    └── DirectionalLight
```

### Component Configuration

#### TestSceneInitializer Configuration
The TestSceneInitializer inherits from GameSceneSetup and extends it with testing functionality. Configure the following:

- **Test Configuration:**
  - Enable Test Controls: Whether to show test control buttons
  - Enable Debug UI: Whether to show debug information
  - Auto Setup Test Environment: Whether to automatically initialize test environment

- **Required Prefabs:**
  - Player Prefab: Reference to the player character prefab
  - Main Game Controller Prefab: Reference to the game controller prefab
  - Buzz UI Controller Prefab: Reference to the buzz UI prefab

- **Environment Settings:**
  - Ground Material: Material for the ground
  - Wall Material: Material for the walls
  - Player Material: Material for the player character
  - Ground Size: Size of the playing area
  - Wall Height: Height of the boundary walls

- **Debug Settings:**
  - Show FPS: Whether to display frames per second
  - Show Player Position: Whether to display player coordinates
  - Show Buzz Level: Whether to display the current buzz level

### Test Mode Enablement

#### Enabling Test Mode
1. Select the `TestSceneSetup` object in the Hierarchy
2. In the Inspector, there are two ways to enable test mode:
   - Check the "Is Test Mode" checkbox in the Scene Setup section
   - Or use the `SetTestMode(true)` function via the context menu

#### What Test Mode Enables
- Creates test control UI buttons
- Displays debug information
- Provides keyboard shortcuts for testing
- Enables test-specific game logic
- Adds additional debug visualization for buzz levels and game states

---

## 3. Testing Procedures

### How to Run Tests

#### Starting a Test Session
1. Make sure the scene is properly set up and configured
2. Press the Play button in Unity
3. The test environment will initialize automatically
4. The player character will spawn at the designated spawn point
5. The buzz UI and debug UI will be visible

#### Test Scenarios
You can test various aspects of the game:

1. **Player Movement:**
   - Use WASD keys to move the player
   - Use Space to jump
   - Test collision with walls and props

2. **Buzz System:**
   - Use test buttons to increase/decrease buzz level
   - Observe UI changes at different buzz levels
   - Test critical buzz state behavior

3. **Environment Changes:**
   - Use environment buttons to switch between different environments
   - Test how gameplay changes in different environments

4. **Game Progression:**
   - Test drawing cards
   - Test game state transitions
   - Test UI responses to game events

### Available Test Controls

#### UI Buttons
The test UI provides several buttons for controlling the test environment:

- **Buzz Controls:**
  - Increase Buzz: Increases buzz level by 10%
  - Decrease Buzz: Decreases buzz level by 10%
  - Critical Buzz: Sets buzz to critical level
  
- **Environment Controls:**
  - Street Env: Changes to street environment
  - Tunnel Env: Changes to tunnel environment
  - Speakeasy Env: Changes to speakeasy environment
  
- **Gameplay Controls:**
  - Draw Card: Simulates drawing a card
  - Reset Scene: Resets the scene to initial state

#### Keyboard Shortcuts
The test environment also provides keyboard shortcuts:

- **F1:** Toggle debug display
- **F2:** Toggle test UI
- **F5:** Reset scene
- **1:** Switch to Streets environment
- **2:** Switch to Tunnels environment
- **3:** Switch to Speakeasy environment
- **T:** Draw a card

### Debug UI Usage

#### Debug Information Display
The debug UI displays various information useful for testing:

- **FPS:** Current frames per second
- **Position:** Current player position coordinates
- **Buzz Level:** Current buzz level percentage
- **Environment:** Current active environment
- **Test Mode:** Whether test mode is enabled
- **Game State:** Current game state

#### Interpreting Debug Information
- **FPS:** Should remain above 30 for smooth gameplay
- **Buzz Level:** Ranges from 0-100, with critical threshold around 30
- **Environment:** Shows which environment assets and settings are active
- **Game State:** Shows current state of game progression

#### Logging
The test environment also outputs detailed logs to the Unity console:
- Scene initialization events
- Component creation and configuration
- Test actions and their results
- Error and warning messages

---

## 4. Troubleshooting

### Common Issues

#### Missing Prefabs
If you see errors about missing prefabs:
1. Make sure you've created all required prefabs (Player, MainGameController, BuzzUI)
2. Check that the prefabs are assigned to the TestSceneInitializer
3. Verify that the prefabs have all required components

#### Script Errors
If you encounter script errors:
1. Check the Unity console for specific error messages
2. Verify that all required namespaces are properly imported
3. Check for any missing component references

#### Scene Setup Problems
If the scene doesn't set up correctly:
1. Try using the "Create Complete Scene Hierarchy" function again
2. Check for any errors in the console during setup
3. Verify that the TestSceneLayout component is properly configured

### Getting Help
If you continue to experience issues:
1. Check the PDX Underground documentation for additional guidance
2. Contact the development team for support
3. Check the project's issue tracker for known issues and solutions

---

## Additional Resources

- [GameSceneSetup Documentation](../../Scripts/Runtime/Core/GameSceneSetup.cs)
- [TestSceneInitializer Documentation](./TestSceneInitializer.cs)
- [PlayerPrefab Documentation](./Prefabs/PlayerPrefab.cs)
- [MainGameControllerPrefab Documentation](./Prefabs/MainGameControllerPrefab.cs)
- [BuzzUIPrefab Documentation](./Prefabs/BuzzUIPrefab.cs)
