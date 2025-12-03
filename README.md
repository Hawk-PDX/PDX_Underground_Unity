# PDX Underground

A Unity game set in 1880s Portland featuring card-based combat and historically accurate lighting.

## Tech Stack

- Unity 2022.3 LTS
- C#
- Universal Render Pipeline (URP)

## Features

- **Card-Based Combat**: Strategic ability system powered by cards
- **Buzz Mechanic**: Character state affects movement and visual effects
- **Historical Lighting**: Period-accurate gas lamp simulation with synchronized flickering
- **Shanghai Tunnels**: Atmospheric underground environments

## Getting Started

### Prerequisites
- Unity 2022.3 LTS or newer
- Git with LFS

### Setup
1. Clone the repository
2. Open in Unity Hub
3. Scene hierarchy starts from `Assets/Scenes/Init.unity`

### Build
Use Unity's standard build process (`File > Build Settings`)

## Controls

- **WASD** - Movement
- **Mouse** - Camera control (hold right button)
- **1** - Slice (melee)
- **2** - Flick (ranged)
- **Space** - Jump

## Testing

Tests use Unity's Test Runner (NUnit framework):
- `Window > General > Test Runner`
- Tests located in `Assets/Scripts/Tests/`

## Project Structure

```
Assets/
├── Scenes/           # Unity scenes
├── Scripts/          # C# scripts
│   ├── Core/         # Game managers and systems
│   ├── Player/       # Player character
│   ├── Combat/       # Combat abilities
│   ├── Runtime/      # Runtime components
│   └── Tests/        # Unit tests
├── Prefabs/          # Reusable game objects
└── Materials/        # Material assets
```

## Contact

**Developer**: hawkpdx  
**Email**: hawkPdx@icloud.com
