# PDX Underground Unity Game

A Unity-based game set in 1800s Portland with historically accurate gaslight effects.

## Setting Up Script Initialization Order

To ensure all scripts run in the correct order, follow these setup instructions:

### 1. Create an Init Scene

This scene will initialize the core game systems before loading any gameplay scenes.

1. In Unity, create a new scene (`File > New Scene` or `Ctrl+N`)
2. Save it as "Init" in your Scenes folder (`File > Save As` or `Ctrl+S`)
3. Create an empty GameObject and name it "GameManager"
   - Select `GameObject > Create Empty` from the menu
   - In the Inspector, rename it to "GameManager"
4. Add the GameManager script component:
   - With the GameManager object selected, click `Add Component` in the Inspector
   - Search for "GameManager" and select it
   - The GameManager should now appear in the Inspector

![Init Scene Setup](Docs/Images/init_scene_setup.png)

### 2. Set Up Build Settings

Configure your project's scene loading order:

1. Open the Build Settings window (`File > Build Settings` or `Ctrl+Shift+B`)
2. Add your scenes to the "Scenes in Build" list:
   - Drag the Init scene from your Project window to the first slot (index 0)
   - Add your MainMenu scene as the second scene
   - Add your gameplay scenes after that
3. Make sure "Init" is at index 0, as this will be the first scene loaded when the game starts

![Build Settings](Docs/Images/build_settings.png)

### 3. Configure GameManager Properties

With the GameManager GameObject selected in your Init scene:

1. In the Inspector, set the startup scene to match your menu scene name:
   - Find the "_startupScene" field and enter "MainMenu" (or your menu scene name)
2. Configure Time of Day settings:
   - Set "_timeOfDay" to a value between 0-1 (0=midnight, 0.5=noon)
   - For testing gas lamps, use 0.9 (evening) to see them at full brightness
3. Set your Weather Intensity as needed:
   - Higher values (0.5-1.0) create more dramatic flickering effects
   - Lower values (0-0.3) create more subtle, steady lighting

![GameManager Settings](Docs/Images/gamemanager_settings.png)

### 4. Create a Gas Lamp Prefab

Once you have the scene initialization set up, create a gas lamp prefab:

1. Create a new empty GameObject in any scene and name it "GasLamp"
2. Add a Light component:
   - With the GasLamp object selected, click `Add Component` and search for "Light"
   - Set the Light Type to "Point"
   - Set Range to around 5-10
   - Set Intensity to around 1.5
   - Set Color to a warm yellow/orange tone
3. Add the GaslightFlicker script:
   - Click `Add Component` and search for "GaslightFlicker"
   - Configure the properties in the Inspector:
     - Lantern Type: GasLamp
     - Color Temperature: ~1900K (historically accurate for gas lamps)
     - Min/Max Intensity: 0.8/1.2 (for subtle flickering)
     - Flicker Speed: 0.1 (moderate flickering)
     - Synchronization: Enable and set radius to 3-5 units
4. Save as a prefab by dragging the GameObject into your Project window

![Gas Lamp Prefab](Docs/Images/gaslamp_prefab.png)

### 5. Test Your Setup

To verify everything is working correctly:

1. Play the game starting from the Init scene
2. The GameManager should initialize all systems in order:
   - Core systems first
   - Scene systems second
   - Environment systems third
   - Effects (including gas lamps) last
3. Check the Console for initialization messages in the correct order
4. Your gas lamps should be flickering with historically accurate behavior
5. Try placing multiple gas lamps near each other to see the synchronization effect

## Initialization Order Details

This setup ensures your scripts initialize in the following order:

1. **Core Systems** (via GameManager):
   - Save data
   - Input system
   - Audio system
   - Other game-wide persistent systems
   
2. **Scene Systems**:
   - Level-specific managers
   - UI systems
   - Camera systems
   
3. **Environment Systems**:
   - Time of day
   - Weather effects
   - NPC systems
   
4. **Visual Effects**:
   - Gaslight flickering (GaslightFlicker.cs)
   - Other visual effects
   
This order ensures dependencies are resolved properly and prevents null reference errors.

## Troubleshooting

If you encounter issues:

1. **Script errors during initialization**:
   - Check the Console for error messages
   - Use GameManager.GetInitializationErrors() to get a list of all errors

2. **Gas lamps not flickering**:
   - Ensure the Time of Day value creates sufficient light intensity
   - Check that GaslightFlicker component is enabled
   - Verify the script has a reference to the Light component

3. **Synchronization not working**:
   - Check that gas lamps are within each other's synchronization radius
   - Verify that synchronizeWithNearby is set to true
   - Try increasing the synchronization amount for a stronger effect

# PDX Underground Environment Setup

This project provides an environment setup wizard for the PDX Underground game, focusing on recreating 1880s Portland with streets, tunnels, and speakeasy environments.

## Setup Instructions

There are two ways to set up the environment:

### Method 1: Using the Setup Scene

1. Open the SetupScene located at `Assets/Scenes/SetupScene.unity`
2. The scene should automatically be checked and fixed if needed by the SetupSceneChecker
3. Press the Play button to run the setup
4. Check the Unity Console for progress
5. Once completed, you can exit Play mode
6. A detailed log will be saved to `Logs/setup_log.txt`

### Method 2: Using the Editor Menu

If Unity editor menus are working correctly, you can:

1. Use the `PDX Underground > Environment Setup Wizard` menu item
2. Follow the step-by-step process in the wizard
3. Alternatively, use `PDX Underground > Verify Setup Scene` to check and fix the setup scene

## Environment Components

The PDX Underground environment includes:

* Street environments with cobblestone streets and wooden sidewalks
* Shanghai Tunnels with wooden supports and brick walls
* Speakeasy interiors with period-appropriate details
* Gaslight street lighting for historical accuracy

## Folder Structure

The setup creates the following folder structure:

```
Assets/
├── Materials/
│   └── Environment/
│       ├── Streets/
│       ├── Tunnels/
│       └── Speakeasy/
├── Textures/
│   └── Environment/
│       ├── Streets/
│       ├── Tunnels/
│       └── Speakeasy/
├── Models/
│   └── Environment/
│       ├── Streets/
│       ├── Tunnels/
│       └── Lighting/
└── Prefabs/
    └── Environment/
        ├── Streets/
        │   ├── Compositions/
        │   └── Props/
        ├── Tunnels/
        └── Speakeasy/
```

## Troubleshooting

If you encounter issues:

1. Check the log file at `Logs/setup_log.txt`
2. Verify all scripts are compiled correctly
3. Try running the SetupEnvironment script directly in Play mode
4. Ensure you have proper write permissions in the project directory

For more detailed information, see the documentation in `Assets/Documentation/`.

# PDX Underground

A Unity game set in 1880s Portland, Oregon, featuring the mysterious Shanghai Tunnels and historical speakeasies.

## Project Description

PDX Underground is an atmospheric narrative-driven game set in the dark underbelly of late 19th century Portland. Players navigate the treacherous Shanghai Tunnels, interact with historical speakeasies, and unravel the mysteries of Portland's shadowy past.

The game features:
- Character-driven narrative set in historically accurate locations
- Atmospheric lighting using a custom gaslights system
- "Buzz" mechanic representing the character's awareness and state of mind
- Card-based ability system for interactions and challenges
- Multiple environments: Streets, Tunnels, and Speakeasy

## Gameplay Mechanics

### Combat System

The game uses a unique card-based combat system with the Gambler character:
- Uses "Buzz" energy to power abilities
- Features dual-wielding melee capabilities
- Can throw cards as ranged projectiles
- Critical hit mechanics restore Buzz on successful hits

### Key Abilities

#### Slice Ability
- Close-range melee attack using dual-wielded cards
- Deals damage in a frontal arc
- Multiple hits in a combo sequence
- Higher damage than ranged attacks

#### Flick Ability
- Ranged attack that throws cards at targets
- Can throw multiple cards with spreading patterns
- Cards stick to hit surfaces
- Critical hits restore Buzz energy

### Controls

- **WASD** - Character movement
- **Mouse** - Look around (hold right mouse button)
- **1** - Use Slice ability (melee attack)
- **2** - Use Flick ability (ranged attack)
- **Space** - Jump
- **Mouse Scroll** - Zoom camera in/out
- **ESC** - Exit game/test

## Historical Lighting System

PDX Underground features a historically accurate lighting system that recreates authentic 1800s Portland illumination:

### Lamp Types

#### Gas Lamps
- Color temperature range: 1800K-2000K
- Synchronized flickering between lamps on shared gas lines
- Occasional pressure drops affecting connected lamps

#### Oil Lamps
- Color temperature range: 1900K-2300K
- Independent behavior without synchronization
- Slower, more gentle flickering pattern

#### Candle Lamps
- Color temperature range: 1700K-1900K
- Most variable flickering pattern
- Affected by air movement
- No synchronization between candle lamps

## Setup Instructions

### Prerequisites

- Unity 2022.3 LTS or newer
- Git with LFS support
- Basic understanding of C# and Unity development

### Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/hawkpdx/PDX_Underground_Unity.git
   cd PDX_Underground_Unity
   ```

2. Open the project in Unity:
   - Launch Unity Hub
   - Add the project from your local repository folder
   - Open the project

3. Set up the game scene:
   - In Unity's top menu, go to **PDX Underground > Setup > Create Main Scene Structure**
   - This will automatically set up the main scene hierarchy

4. Set up prefabs:
   - Follow the prefab setup guides in `Assets/Prefabs/Core/Setup_Guides/`
   - Create the GameController prefab
   - Create the Player prefab
   - Create the Gaslight prefabs for different environments

## Development Workflow

We use a Git Flow inspired workflow:

- `main` branch contains stable, production-ready code
- `develop` branch is the integration branch for ongoing development
- Feature branches should be created from `develop` for new features
- Use pull requests to merge changes back to `develop`

### Branching Pattern

```
feature/feature-name  → develop → main
bugfix/issue-name     → develop → main
```

### Commit Message Format

Please use descriptive commit messages with a clear prefix:

- `feat:` for new features
- `fix:` for bug fixes
- `docs:` for documentation changes
- `refactor:` for code refactoring
- `test:` for adding tests
- `chore:` for maintenance tasks

## Unity Version Requirements

- Unity 2022.3 LTS or newer
- Universal Render Pipeline (URP)
- Required packages:
  - TextMeshPro
  - Input System
  - 2D Sprite
  - Timeline

## Project Structure

```
PDX_Underground_Unity/
├── Assets/
│   ├── Prefabs/          # Reusable game objects
│   │   ├── Core/         # Core game systems
│   │   ├── Player/       # Player components
│   │   └── Environment/  # Environment elements
│   ├── Scenes/           # Unity scenes
│   │   └── Main/         # Main game scene and setup guides
│   ├── Scripts/          # C# scripts
│   │   ├── Runtime/      # Game runtime scripts
│   │   │   ├── Core/     # Core systems
│   │   │   ├── Player/   # Player behavior
│   │   │   └── Effects/  # Visual effects
│   │   ├── Editor/       # Unity editor tools
│   │   └── Tests/        # Test scripts
│   ├── Resources/        # Runtime-loaded assets
│   └── Materials/        # Material assets
└── Documentation/        # Project documentation
```

## Scene Setup Guides

The project includes several setup guides to help with scene creation:

- **Main Scene Setup**: See `Assets/Scenes/Main/Scene_Setup_README.md`
- **GameController Setup**: See `Assets/Prefabs/Core/Setup_Guides/GameController_Setup_Guide.txt`
- **Player Setup**: See `Assets/Prefabs/Core/Setup_Guides/Player_Setup_Guide.txt`
- **Gaslight Setup**: See `Assets/Prefabs/Core/Setup_Guides/Gaslight_Setup_Guide.txt`

## Key Scripts

- `MainGameController.cs`: Core game state and systems management
- `GamblerCharacter.cs`: Player character attributes and state
- `PlayerController.cs`: Player movement and input handling
- `GaslightFlicker.cs`: Atmospheric lighting system

## Contributing

We welcome contributions to PDX Underground! Please follow these guidelines:

- Use the existing architecture for new abilities and characters
- Keep the period-appropriate theme in mind for visuals and mechanics
- Document new systems in code with proper comments
- Test thoroughly in the test scene before committing changes
- Create pull requests against the `develop` branch
- Follow the commit message format described above

## Game Philosophy

PDX Underground aims to combine traditional gameplay with strategic thinking and historically-inspired settings. We believe that games can provide an immersive way to explore historical periods while offering engaging gameplay mechanics that challenge players both intellectually and technically.

The concept of a historically-based game with strategy/logic elements as well as humanity-based narrative choices creates an experience that appeals to free-thinking individuals looking for more than standard game formulas.

## Contact Information

- **Developer**: hawkpdx
- **GitHub**: [https://github.com/hawkpdx](https://github.com/hawkpdx)
- **Email**: hawkPdx@icloud.com

---

*Last updated: April 11, 2025*
