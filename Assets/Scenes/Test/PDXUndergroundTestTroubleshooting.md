# PDX Underground Test Scene Troubleshooting Guide

This guide will help you set up, test, and troubleshoot the PDX Underground test scene. Follow these steps to ensure your game components are working correctly.

## 1. Quick Start Guide

### Setting Up the Test Scene
1. **Create a new scene**:
   - Open Unity and navigate to your project
   - Select `File > New Scene` (or use Ctrl+N)
   - Save the scene as `PDXUndergroundTestScene` in the `Assets/Scenes/Test` folder

2. **Add the Test Setup**:
   - Create an empty GameObject in the scene and name it `TestSceneSetup`
   - Add the `TestSceneLayout` component to this GameObject
   - In the Inspector, click the gear icon and select "Create Complete Scene Hierarchy"

3. **Configure References**:
   - Select the `TestSceneSetup` GameObject
   - In the Inspector, ensure all prefab references are assigned:
     - Player Prefab
     - Main Game Controller Prefab
     - Buzz UI Controller Prefab

4. **Run the Scene**:
   - Press the Play button in the Unity editor
   - You should see a character that you can control with WASD and Space to jump

## 2. Common Issues and Solutions

### Missing References
**Issue**: "Missing required prefab reference" errors in console  
**Solution**: 
- Check if all prefabs are assigned in the `TestSceneSetup` component
- Create the missing prefabs by right-clicking in Project window > Create > Prefab
- Add the appropriate components to each prefab

### Character Not Moving
**Issue**: Player character is not responding to input  
**Solution**:
- Verify the player has both `PlayerController` and `GamblerCharacter` components
- Check that the `CharacterController` component is attached
- Ensure Input Manager settings are default (Edit > Project Settings > Input)

### Buzz Level Not Working
**Issue**: Buzz level is not updating or displaying  
**Solution**:
- Check if `MainGameController` exists in the scene
- Verify `BuzzUIController` is properly set up
- Ensure `GamblerCharacter` has proper initial buzz settings

### Camera Issues
**Issue**: Camera not following player or positioned incorrectly  
**Solution**:
- Verify the Main Camera has a follow script attached
- Check camera offset and distance settings
- Reset camera position to be behind the player

### Compiler Errors
**Issue**: Scripts won't compile or have errors  
**Solution**:
- Check namespace declarations match folder structure
- Verify all required imports and references
- Make sure all script files are in the correct folders

## 3. Component Checklist

Verify you have all these components properly set up:

### Required Core Components
- [ ] `GameSceneSetup.cs` - Placed in `/Assets/Scripts/Runtime/Core/`
- [ ] `MainGameController.cs` - Placed in `/Assets/Scripts/Runtime/Core/`
- [ ] `GamblerCharacter.cs` - Placed in `/Assets/Scripts/Runtime/Player/`
- [ ] `PlayerController.cs` - Placed in `/Assets/Scripts/Runtime/Player/`
- [ ] `BuzzUIController.cs` - Placed in `/Assets/Scripts/Runtime/UI/`

### Required GameObject Hierarchy
- [ ] TestSceneSetup (GameObject with TestSceneLayout component)
- [ ] Player (GameObject with GamblerCharacter and PlayerController components)
- [ ] MainGameController (GameObject with MainGameController component)
- [ ] BuzzUI (Canvas with BuzzUIController component)
- [ ] Main Camera (Camera with proper following script)
- [ ] Directional Light

## 4. Testing Procedure

Follow this procedure to thoroughly test your implementation:

1. **Basic Movement**:
   - Move with WASD keys
   - Camera should follow the player
   - Character should rotate to face movement direction

2. **Jumping**:
   - Press Space to jump
   - Character should return to ground after jumping
   - Ground detection should work on all surfaces

3. **Buzz Mechanics**:
   - Buzz level should gradually decrease over time
   - UI should show the current buzz level
   - When buzz level gets low, movement should be affected

4. **Environment Testing**:
   - Try changing environments (if implemented)
   - Check for environment-specific effects on the character

5. **Game State Testing**:
   - Pause and resume the game (if implemented)
   - Test game over conditions (if implemented)
   - Check main menu transitions (if implemented)

## 5. Debug Commands and Shortcuts

Use these debug commands and shortcuts to help with testing:

### Keyboard Shortcuts
- **F1** - Toggle debug display
- **F2** - Toggle test controls
- **1/2/3** - Switch between environments (Streets/Tunnels/Speakeasy)
- **Shift+C** - Reset camera position
- **Shift+R** - Reload current scene
- **Shift+B** - Refill buzz to maximum
- **Shift+T** - Teleport to spawn point

### Debug Menu Options
- **Character Info** - Shows character stats and state
- **Buzz Controls** - Add/remove buzz with sliders
- **Environment Toggle** - Switch between available environments
- **Test Actions** - Buttons to test specific game mechanics

### Console Commands
If you've implemented console commands, you can use:
```
/buzz set 100     // Set buzz to maximum
/buzz set 30      // Set buzz to critical level
/teleport spawn   // Teleport to spawn point
/mode test        // Enable test mode
/debug on         // Enable debug visuals
```

## Final Notes

- If you encounter issues not covered in this guide, check the Unity Console for specific error messages
- Make sure all script modifications are saved before testing
- Some features might require additional implementation beyond the basic test scene
- Remember that this is a test environment - focus on functionality over aesthetics

Good luck with your PDX Underground development!

