# PDX Underground Unity Project Structure

## Project Overview

PDX Underground is a role-playing game being migrated from Python to Unity/C#. This document outlines the project structure, migration plan, and development workflow.

## Directory Structure

```
PDX_Underground_Unity/
│
├── Assets/
│   ├── Scripts/         # C# scripts for game logic
│   │   ├── Controllers/ # Input and game controllers
│   │   ├── Models/      # Data models matching database schema
│   │   ├── UI/          # User interface scripts
│   │   ├── Combat/      # Combat system scripts
│   │   └── Utilities/   # Helper functions and utilities
│   │
│   ├── Models/          # 3D models and assets
│   │   ├── Characters/
│   │   ├── Environment/
│   │   ├── Weapons/
│   │   └── Items/
│   │
│   ├── Database/        # Database-related files
│   │   ├── database_schema.sql  # Schema definition
│   │   ├── DatabaseManager.cs   # C# database connection manager
│   │   └── DataAccess/          # Data access objects
│   │
│   ├── Resources/       # Game resources
│   │   ├── Prefabs/
│   │   ├── Materials/
│   │   ├── Textures/
│   │   ├── Audio/
│   │   └── Animations/
│   │
│   ├── Scenes/          # Unity scenes
│   │   ├── MainMenu.unity
│   │   ├── CharacterCreation.unity
│   │   ├── GameWorld.unity
│   │   └── Combat.unity
│   │
│   └── Plugins/         # Third-party plugins
│
└── Documentation/       # Project documentation
    ├── ProjectStructure.md     # This file
    ├── MigrationPlan.md        # Details on Python to C# migration
    ├── DatabaseSchema.md       # Explanation of database design
    └── DeveloperGuide.md       # Guidelines for contributors
```

## Migration Plan from Python to Unity/C#

### Phase 1: Setup and Planning
- [x] Create Unity project structure
- [x] Define improved database schema
- [ ] Map Python classes to C# equivalents
- [ ] Identify core game mechanics to migrate first

### Phase 2: Core Systems Migration
- [ ] Implement database connectivity in C#
- [ ] Migrate character system from Python to C#
- [ ] Migrate inventory system from Python to C#
- [ ] Implement basic UI framework in Unity

### Phase 3: Game Logic Migration
- [ ] Migrate combat system from Python to C#
- [ ] Implement quest system in Unity
- [ ] Convert NPC interactions to Unity dialog system
- [ ] Migrate stat & ability calculations to C#

### Phase 4: Unity-Specific Enhancements
- [ ] Implement 3D movement and camera controls
- [ ] Add visual effects for abilities and combat
- [ ] Implement sound and music systems
- [ ] Design and implement UI/UX improvements

## Database Integration

The game uses a relational database structure defined in `Assets/Database/database_schema.sql`. For Unity integration, we have the following options:

1. **SQLite Integration**: Use the SQLite library for Unity to interact with a local database file.
   - Pro: Simple setup, works offline
   - Con: Limited concurrent access

2. **Server Backend**: Implement a server backend that Unity connects to for database operations.
   - Pro: Better for multiplayer, centralized data management
   - Con: More complex setup, requires server infrastructure

The current implementation will use SQLite for development and single-player modes, with architecture that allows for future server integration.

## C# Data Models

Each database table will have a corresponding C# class in the `Assets/Scripts/Models` directory. These classes will:

1. Match the database schema structure
2. Implement serialization for Unity's save/load system
3. Include validation logic for data integrity
4. Provide static methods for common queries (repository pattern)

Example model class structure:
```csharp
// Character.cs
using System;
using UnityEngine;

[Serializable]
public class Character
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public string Name { get; set; }
    public string Class { get; set; }
    public string WeaponType { get; set; }
    public string ArmorType { get; set; }
    public int Level { get; set; }
    public int Experience { get; set; }
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    public int CurrentMana { get; set; }
    public int MaxMana { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdated { get; set; }
    
    // Additional methods for character behavior
    public void LevelUp() { /* ... */ }
    public bool CanEquipItem(Item item) { /* ... */ }
    public void ApplyDamage(int amount) { /* ... */ }
    
    // Static methods for data access
    public static Character GetById(int id) { /* ... */ }
    public static Character[] GetAllForPlayer(int playerId) { /* ... */ }
}
```

## Development Workflow

1. **Version Control**: Use Git for version control with feature branches
2. **Unity Version**: Use Unity 2022.3 LTS or later
3. **Development Environment**: Visual Studio Code with Unity extension
4. **Coding Standard**: Follow [Unity C# Coding Guidelines](https://unity.com/how-to/naming-and-code-style-unity-csharp)
5. **Testing**: Implement unit tests for game logic using Unity Test Framework

## Migration Notes from main.py

Key elements to migrate from the Python implementation:

1. Player and character class hierarchy
2. Inventory and item management systems
3. Combat mechanics and formulas
4. Game state management
5. Save/load functionality
6. UI flow and interaction patterns

Each system should be implemented as a modular component in Unity, following the component-based architecture that Unity encourages.

## Next Steps

1. Review the database schema and make any necessary adjustments
2. Begin creating C# model classes that match the database structure
3. Set up SQLite integration with Unity
4. Implement basic character creation and management UI
5. Begin migrating core game mechanics from Python to C#

## Contacts

For questions about the project structure or migration plan, contact the project lead.

