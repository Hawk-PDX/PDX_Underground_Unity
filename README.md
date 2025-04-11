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
