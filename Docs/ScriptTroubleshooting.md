# Script Attachment Troubleshooting

Follow these steps to isolate and resolve the issue with the GameManager script.

## Step 1: Temporarily Rename Original Script

First, let's prevent the original script from causing compiler errors:

1. Navigate to your project folder in your file explorer:
   ```
   /Users/hawkpdx/Development/code/games/PDX_Underground_Unity/Assets/Scripts/Core/
   ```

2. Rename `GameManager.cs` to `GameManager.cs.bak`
   - Right-click on the file in Finder
   - Select "Rename"
   - Change the name to "GameManager.cs.bak"

3. Rename `GameManager.cs.meta` to `GameManager.cs.meta.bak` to prevent meta file errors
   - Right-click on this hidden file (if visible)
   - Select "Rename"
   - Change the name to "GameManager.cs.meta.bak"

## Step 2: Create Test GameObject

Now let's test if the simplified manager can be attached:

1. Open Unity if it's not already open
2. Create or open your Init scene
   - Go to File > New Scene (if needed)
   - Save it as "Init" in your Scenes folder

3. Create a new empty GameObject
   - Select GameObject > Create Empty from the menu
   - Name it "TestManager"

## Step 3: Attach the Simplified Script

1. Select the "TestManager" GameObject in the Hierarchy
2. In the Inspector panel, click the "Add Component" button
3. Type "Game Manager Simple" in the search box
4. The script should appear in the search results
5. Click on the script to attach it

## Step 4: Test the Script

1. Once attached, you should see the properties in the Inspector:
   - _timeOfDay (with a default value of 0.5)

2. Run the scene briefly to test:
   - Click the Play button
   - Check the Console for the initialization message
   - Stop the scene

## Step 5: Identify the Issue

If the simplified script attaches successfully, the issue is with your original GameManager.cs script. Possible problems include:

1. Syntax errors or missing braces
2. Missing references to other scripts
3. Circular dependencies with PDXUnderground.Effects
4. Incompatible using statements

## Step 6: Fix and Restore Original Script

Once you've identified the issue:

1. Rename `GameManager.cs.bak` back to `GameManager.cs`
2. Rename `GameManager.cs.meta.bak` back to `GameManager.cs.meta`
3. Open the script and fix the specific issues found
4. Save the changes

## Step 7: Clean Up

After fixing the original script:

1. Delete the temporary `GameManagerSimple.cs` script (or keep it as a backup)
2. Remove the test GameObject from your scene
3. Attach the fixed GameManager script to your main GameManager GameObject

## Potential Quick Fixes

Here are some common fixes for script attachment issues:

### Missing References
```csharp
// Comment out problematic references temporarily
// using SomeNamespace.ThatMightNotExist;
```

### Circular Dependencies
```csharp
// Use forward declarations or interfaces for circular dependencies
// Instead of direct references to other classes
```

### Namespace Issues
```csharp
// Make sure namespace exactly matches:
namespace PDXUnderground.Core
{
    public class GameManager : MonoBehaviour
    {
        // Implementation
    }
}
```

### Compilation Order
Unity sometimes has issues with script compilation order. Try:
1. Moving core scripts to special folders like 'Plugins'
2. Using assembly definition files to control compilation order

