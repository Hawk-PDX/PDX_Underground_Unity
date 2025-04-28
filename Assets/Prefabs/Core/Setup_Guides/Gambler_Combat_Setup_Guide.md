# Gambler Character Combat Setup Guide

This guide walks through setting up the Gambler character with card-based combat abilities in the Unity Editor.

## 1. Creating the Gambler GameObject

1. In the Unity Editor, right-click in the Hierarchy window and select **Create Empty**
2. Rename the new GameObject to "Gambler"
3. With the Gambler GameObject selected:
   - Add a Character Controller component (Component > Physics > Character Controller)
   - Add an Animator component (Component > Miscellaneous > Animator)

## 2. Add Required Scripts

Add the following scripts to the Gambler GameObject:

1. **GamblerCharacter Script**
   - Component > Scripts > Runtime > Player > GamblerCharacter
   - Configure basic properties:
     - Set Max Health to 100
     - Set Max Energy to 100
     - Set Max Buzz to 100
     - Set Current Buzz to 50 (half full)

2. **GamblerAnimatorController Script**
   - Component > Scripts > Runtime > Player > GamblerAnimatorController
   - Assign the Animator reference to the Animator component on the Gambler
   - Assign the GamblerCharacter reference to the GamblerCharacter component on the Gambler

3. **RangedFlickAbility Script**
   - Component > Scripts > Combat > RangedFlickAbility
   - Configure the basic properties:
     - Set Base Damage to 8
     - Set Range to 20
     - Set Cooldown Time to 1.5
     - Set Buzz Cost to 10

4. **MeleeSlashAbility Script**
   - Component > Scripts > Combat > MeleeSlashAbility
   - Configure the basic properties:
     - Set Base Damage to 12
     - Set Slash Attack Arc to 120
     - Set Slash Attack Range to 2.5
     - Set Slash Combo Count to 3
     - Set Buzz Cost to 15

## 3. Create Required Transforms

1. **Create Card Spawn Point**
   - Right-click on the Gambler GameObject and select **Create Empty**
   - Rename it to "CardSpawnPoint"
   - Position it slightly in front of and to the right of the Gambler (e.g., X: 0.5, Y: 0, Z: 0.3)
   
2. **Create Attack Origin**
   - Right-click on the Gambler GameObject and select **Create Empty**
   - Rename it to "AttackOrigin"
   - Position it slightly in front of the Gambler (e.g., X: 0, Y: 0, Z: 0.5)

## 4. Set Up Card Projectile Prefab

1. Create a new GameObject in the Scene
   - Right-click in the Hierarchy window and select **Create Empty**
   - Rename it to "CardProjectile"
   
2. Add components to the CardProjectile:
   - Add a Box Collider (Component > Physics > Box Collider)
     - Check the "Is Trigger" checkbox
     - Set the size to a card shape (e.g., X: 0.1, Y: 0.15, Z: 0.01)
   - Add a Rigidbody (Component > Physics > Rigidbody)
     - Check "Use Gravity" off
     - Set "Collision Detection" to Continuous
   - Add the CardProjectile script (Component > Scripts > Combat > CardProjectile)
   
3. Add a Visual for the Card:
   - Right-click on CardProjectile and select **3D Object > Quad**
   - Rename it to "CardVisual"
   - Adjust the scale to match a playing card (e.g., X: 0.1, Y: 0.15, Z: 1)
   - Add a material with a card texture to the Quad

4. Add a Trail Renderer:
   - Add a Trail Renderer component to CardProjectile (Component > Effects > Trail Renderer)
   - Configure it with appropriate settings:
     - Time: 0.5
     - Min Vertex Distance: 0.1
     - Width: Start 0.1, End 0
     - Color: Start white, End transparent
   - Assign this as the cardTrail reference in the CardProjectile script

5. Create the Prefab:
   - Drag the CardProjectile from the Hierarchy into the Prefabs folder
   - Delete the CardProjectile from the scene

## 5. Set Up Visual Effects

1. **Create Slash VFX Prefab**
   - Create a new GameObject in the Scene
   - Rename it to "SlashVFX"
   - Add a Particle System component (Component > Effects > Particle System)
   - Configure the particle system for a slashing effect:
     - Duration: 0.5
     - Shape: Cone with a 120-degree angle
     - Start Color: Red with some alpha
     - Start Size: 0.5
     - Start Speed: 5
   - Drag it into the Prefabs folder to create a prefab
   - Delete it from the scene

2. **Create Card Impact VFX Prefab**
   - Create a new GameObject in the Scene
   - Rename it to "CardImpactVFX"
   - Add a Particle System component
   - Configure for an impact effect:
     - Duration: 0.3
     - Shape: Sphere
     - Emission: Burst of 20 particles
     - Start Color: White fading to transparent
     - Start Size: 0.1
     - Start Speed: 3
   - Drag it into the Prefabs folder
   - Delete it from the scene

## 6. Configure Ability References

1. **Configure RangedFlickAbility**:
   - With the Gambler selected, find the RangedFlickAbility component in the Inspector
   - Assign the Card Projectile Prefab to the "Card Projectile Prefab" field
   - Assign the CardSpawnPoint transform to the "Card Spawn Point" field
   - Assign the CardImpactVFX prefab to the "Ability VFX Prefab" field

2. **Configure MeleeSlashAbility**:
   - With the Gambler selected, find the MeleeSlashAbility component
   - Assign the AttackOrigin transform to the "Attack Origin" field
   - Set the Target Layers to include enemies
   - Assign the SlashVFX prefab to the "Slash VFX Prefab" field

## 7. Set Up Card Effects Controller

1. Add CardEffectsController to the Gambler:
   - Component > Scripts > Effects > CardEffectsController
   
2. Configure the CardEffectsController:
   - Assign the SlashVFX prefab to the "Slice Effect" field
   - Assign the CardImpactVFX prefab to the "Standard Impact Effect" field
   - If you have critical hit versions, assign those as well

## 8. Set Up Input Bindings

1. Create or update your Input Action asset to include:
   - "Flick" action bound to keyboard key 2
   - "Slash" action bound to keyboard key 1

2. Connect the input actions to the GamblerCharacter in your PlayerController or InputHandler script.

## 9. Testing Your Setup

1. Create a simple test environment with:
   - A floor plane
   - Some target objects with colliders
   - Tag the targets as "Enemy" or add them to the appropriate layer

2. Add a simple test script to the targets that implements the IDamageable interface.

3. Run the scene and test both abilities:
   - Press 1 to use the Slash ability
   - Press 2 to use the Flick ability
   - Verify that hits are registered, damage is applied, and visual effects play
   - Check that buzz costs are properly deducted

## Troubleshooting Common Issues

- **Abilities not firing**: Check that the buzz level is sufficient and the abilities aren't on cooldown
- **No visual effects**: Ensure all prefab references are properly assigned
- **Cards not hitting targets**: Verify that the layers are set correctly in the ability components
- **Animation not playing**: Check that all animation triggers are properly set up in the Animator Controller

## Next Steps

Once the basic setup is working:

1. Fine-tune the ability parameters for game balance
2. Add sound effects for card throws, impacts, and slashes
3. Implement UI elements to show cooldowns and buzz costs
4. Create visual feedback for critical hits
5. Add more advanced card effects and abilities

