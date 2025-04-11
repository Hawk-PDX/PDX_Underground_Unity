# PDX Underground

A Unity game set in 1800s Portland featuring card-based combat mechanics and unique energy systems.
   
## Project Overview

PDX Underground is a character-based action game set in the historical Portland underground. The game features unique combat mechanics centered around cards and a special energy system called "Buzz". Players will take on the role of various characters, starting with the "Gambler" who uses cards as both melee and ranged weapons.

## Implemented Systems

### Gambler Character

The Gambler is a character who specializes in card-based combat:
- Uses "Buzz" energy to power abilities
- Has dual-wielding melee capabilities
- Can throw cards as ranged projectiles
- Features critical hit mechanics that restore Buzz on successful hits

### Combat Abilities

#### Base CombatAbility System
- Abstract base class for all character abilities
- Manages cooldowns, energy costs, and damage calculations
- Handles critical hit chances and effects
- Manages animation and VFX triggering

#### Slice Ability
- Close-range melee attack using dual-wielded cards
- Deals damage in a frontal arc
- Multiple hits in a combo sequence
- Higher damage than ranged attacks
- Visual feedback with slicing effects

#### Flick Ability
- Ranged attack that throws cards at targets
- Can throw multiple cards with spreading patterns
- Cards stick to hit surfaces
- Critical hits restore Buzz energy
- Customizable projectile physics

### User Interface

The game includes a comprehensive UI system:
- Buzz meter showing available energy
- Card hand display for available cards
- Ability cooldown indicators with visual feedback
- Debug panel showing character stats and combat log
- Health displays for target dummies

## Test Scene Setup

### Creating the Test Environment

1. Open Unity and load the PDX_Underground_Unity project
2. Go to the menu: **PDX Underground > Test > Create Test Scene**
3. Unity will create a complete test environment with:
   - Period-appropriate visuals (wooden floors, lanterns, barrels)
   - Target dummies for testing combat
   - Complete UI system
   - The Gambler character with abilities configured
   - Third-person camera with proper controls

### Individual Setup Options

You can also set up individual components:
- **PDX Underground > Test > Setup Gambler Character** - Only creates the player character
- **PDX Underground > Test > Setup UI Elements** - Only sets up the UI
- **PDX Underground > Test > Create Target Dummies** - Only creates targets for testing

## Historical Lighting System

PDX Underground features a historically accurate lighting system that recreates authentic 1800s Portland illumination:

### LanternFlicker System

The LanternFlicker component provides period-appropriate lighting effects:
- Realistic flickering behavior based on lamp type
- Historically accurate color temperatures
- Gas lamp synchronization across shared gas lines
- Variable intensity with occasional pressure drops for gas lamps

### Lamp Types and Historical Accuracy

#### Gas Lamps
- Color temperature range: 1800K-2000K
- Strong synchronization between lamps on shared gas lines
- Occasional pressure drops affecting connected lamps
- Moderate flickering with characteristic "pulse" effect

#### Oil Lamps
- Color temperature range: 1900K-2300K
- Independent behavior without synchronization
- Slower, more gentle flickering pattern
- Less pronounced intensity variation

#### Candle Lamps
- Color temperature range: 1700K-1900K
- Most variable flickering pattern
- Affected by air movement
- No synchronization between candle lamps

### Verification Utility

A comprehensive verification system ensures historical accuracy:
- **PDX Underground > Test > Verify Lantern Historical Accuracy** - Tests all lamps in scene
- **PDX Underground > Test > Final 1800s Gas Lamp Verification** - Specialized gas lamp testing
- **PDX Underground > Test > Verify Gas Lamp Propagation** - Tests shared gas line behavior

The verification utility tests:
- Color temperature ranges for historical accuracy
- Proper synchronization behavior between gas lamps
- Authentic intensity propagation through shared gas lines
- Combined noise calculation for realistic flickering effects

## Controls and Gameplay Mechanics

### Basic Controls
- **WASD** - Character movement
- **Mouse** - Look around (hold right mouse button)
- **1** - Use Slice ability (melee attack)
- **2** - Use Flick ability (ranged attack)
- **Space** - Jump
- **Mouse Scroll** - Zoom camera in/out
- **ESC** - Exit game/test

### Gameplay Mechanics

#### Buzz System
The Buzz meter represents the Gambler's energy. Using abilities consumes Buzz, which can be regained through critical hits.

#### Combat Flow
1. Approach enemies or keep distance based on preferred attack style
2. Use Slice for close combat or Flick for ranged attacks
3. Watch for cooldowns on abilities
4. Manage Buzz levels to ensure you have energy for important attacks
5. Aim for critical hits to regain Buzz and deal extra damage

## Development Roadmap

### Next Features to Implement

1. **Card Deck System**
   - Create a full deck management system
   - Allow collecting new cards through gameplay
   - Implement different card types with varied effects

2. **Enhanced Character Progression**
   - Add leveling system for the Gambler
   - Implement skill trees to customize abilities
   - Create upgrade paths for cards and abilities

3. **Additional Abilities**
   - "Fan of Cards" area attack
   - "Card Trick" ability for temporary buffs
   - Ultimate ability "Royal Flush"

4. **NPCs and Environment**
   - Add interactive NPCs with dialogue
   - Create quest system
   - Expand environment to include full Portland underground setting

5. **Card Crafting System**
   - Allow combining cards for stronger effects
   - Implement card enhancement mechanics
   - Create rare card collection mechanics

### Known Issues/Limitations

- Projectile collisions may need refinement
- Card visual assets need to be created and implemented
- Animation system needs to be integrated with ability usage
- UI needs proper styling to match the historical theme

## Contributing

When contributing to the project, please follow these guidelines:
- Use the existing architecture for new abilities and characters
- Keep the period-appropriate theme in mind for visuals and mechanics
- Document new systems in code with proper comments
- Test thoroughly in the test scene before committing changes

## License

[Include appropriate license information here]



one more thing... the concept of a regular, stage-based, rpg-type game while implementing the idea of strategy/logic/as well as, humanity based logic seems as if it might be an additional 'draw' for free-thinking individuals whom wish to seperate from the negativity of our current truths... 