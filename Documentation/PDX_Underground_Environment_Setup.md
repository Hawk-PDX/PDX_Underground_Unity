# PDX Underground - Historical Portland Environment Setup

This document provides setup instructions for implementing the historical Portland environment in the PDX Underground game. The environment recreates 1800s Portland with a focus on the Shanghai tunnels, busy port, and historical street layout.

## Environment System Overview

The environment system consists of several components working together:

1. **GameEnvironmentController**: Manages day/night cycle, weather, NPC spawning and environmental effects
2. **PortlandEnvironmentSetup**: Handles the generation and setup of the physical environment
3. **SceneManager**: Manages scene transitions between different areas (streets, tunnels, port)
4. **UIManager**: Handles UI elements for area transitions and interactions
5. **Supporting Components**: Gaslights, area transitions, and other interactive elements

## Scene Structure

The game environment is divided into three main scenes:

1. **PortlandStreets** (main scene): The above-ground historical Portland with streets, buildings and the port area
2. **ShanghaiTunnels** (sub-scene): The underground tunnel network used for shanghaiing sailors
3. **PortOfPortland** (sub-scene): The detailed port area with ships, docks and warehouses

## Setting Up the Environment

### Step 1: Create Basic Prefab Structure

Create the following prefab structure:

```
Assets/
├── Prefabs/
│   ├── Core/
│   │   └── GameManager.prefab (with SceneManager and GameEnvironmentController)
│   ├── UI/
│   │   └── MainCanvas.prefab (with UIManager component)
│   └── Environment/
│       ├── Streets/
│       │   ├── StreetSection.prefab
│       │   └── BuildingFacade_1800s.prefab
│       ├── Port/
│       │   ├── DockSection.prefab
│       │   └── Warehouse.prefab
│       ├── Tunnels/
│       │   ├── TunnelSection.prefab
│       │   └── TunnelEntrance.prefab
│       ├── Lighting/
│       │   └── GaslightPost.prefab
│       └── Transitions/
│           └── AreaTransitionPoint.prefab
```

### Step 2: Configure GameManager Prefab

1. Create an empty GameObject named "GameManager"
2. Add the SceneManager component
3. Add the GameEnvironmentController component
4. Configure scene references and environment settings
5. Create prefab in Assets/Prefabs/Core/

### Step 3: Configure UI Prefab (MainCanvas)

1. Create a Canvas (Screen Space - Overlay)
2. Add the UIManager component
3. Create child panels for:
   - Interaction prompts
   - Scene transition fades
   - Area name displays
   - Loading screen
   - Debug information panel
4. Configure references in the UIManager component
5. Create prefab in Assets/Prefabs/UI/

### Step 4: Create Environment Prefabs

#### Street Section:
1. Create modular street mesh (6m wide) with wooden sidewalks
2. Add Box Colliders for player collision
3. Add NavMeshSurface component for NPC navigation
4. Configure materials with period-appropriate textures

#### Gaslight Post:
1. Create a simple gaslight post model
2. Add Point Light component
   - Color: Warm yellow (RGB: 255, 200, 150)
   - Range: 15-20 meters
   - Intensity: 0.8
3. Add GaslightFlicker component
4. Add optional flame particle effect

#### Area Transition Point:
1. Create empty GameObject
2. Add Box Collider (set as trigger)
3. Add AreaTransitionTrigger component
4. Configure destination settings
5. Add visual indicator for development purposes

### Step 5: Create Test Scene

1. Create new scene "PortlandStreets_Test"
2. Add GameManager prefab
3. Add MainCanvas prefab
4. Add a Directional Light for the sun
5. Add Terrain for ground
6. Place test street sections and buildings
7. Add test area transition points
8. Configure NavMesh generation
9. Add placeholder player character with "Player" tag

## Environment Features

### Day/Night Cycle

The GameEnvironmentController manages a full day/night cycle:

1. **Time Scale**: 1 real minute = 3 game minutes (configurable)
2. **Night Effects**:
   - Increased gaslight intensity
   - Darker ambient lighting
   - Different NPC behaviors
   - Unique ambient sounds

### Shanghai Tunnels

The underground tunnels are accessed via specific transition points:

1. **Tunnel Entrances**: Configured in AreaTransitionTrigger
2. **Atmosphere**: Dimmer lighting, echo effects, and dripping water sounds
3. **Danger Areas**: Sections used for shanghaiing victims

### Port Activity

The port features dynamic activity based on time of day:

1. **Ship Movement**: Ships arrive and depart on schedules
2. **Worker NPCs**: Dock workers load/unload cargo
3. **Trading Activity**: Varies by time of day
4. **Period Ships**: Historically accurate vessel types

## Important Environment Settings

### Environmental Constants

The following settings provide period-accurate atmosphere:

1. **Gaslight Intensity**: 0.8-1.0 (night), 0.3-0.4 (day)
2. **Street Width**: 6-8 meters (varies by importance)
3. **Block Size**: 60-80 meters (typical for 1800s Portland)
4. **Building Height**: 2-3 stories (10-15 meters)
5. **Ambient Sounds**: Horse carriages, port activity, distant voices

### Historical Accuracy Notes

1. **Street Grid**: Portland's distinctive small block size (61m x 61m)
2. **Building Materials**: Primarily wood construction in the 1880s
3. **Lighting**: Gas lighting was prevalent (no electric street lights yet)
4. **Port Activity**: Ships were primarily sailing vessels and early steamships
5. **Shanghai Practices**: Active period for crimping (forced recruitment) via the tunnels

## Testing the Environment

To test the environment systems:

1. Enter play mode in the PortlandStreets_Test scene
2. Verify the day/night cycle functions correctly
3. Test area transitions between streets and tunnels
4. Verify that gaslights flicker realistically
5. Check that NavMesh allows proper NPC navigation
6. Test UI elements like interaction prompts and area names

## Next Development Steps

After basic environment setup:

1. Add detailed historical buildings based on research
2. Implement NPC behaviors and schedules
3. Create period-appropriate sound atmosphere
4. Add storyline trigger points in the environment
5. Implement weather effects (rain, fog)
6. Add detailed Shanghai tunnel system based on historical maps

