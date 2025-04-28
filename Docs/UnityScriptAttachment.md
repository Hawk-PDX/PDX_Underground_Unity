# How to Attach Scripts to GameObjects in Unity

This guide explains how to attach C# scripts to GameObjects in your Unity project.

## Method 1: Using the Inspector's Add Component Button

This is the quickest and most common way to add scripts to GameObjects:

1. **Select the GameObject** in the Hierarchy panel that you want to attach the script to
   
   ![Select GameObject](Images/select_gameobject.png)

2. **Click the "Add Component" button** at the bottom of the Inspector panel
   
   ![Add Component Button](Images/add_component_button.png)

3. **Search for your script name** in the search box that appears
   - Type the name of your script (e.g., "Game Manager" or "Gaslight Flicker")
   - Unity will filter the available scripts as you type
   
   ![Search for Script](Images/search_script.png)

4. **Click on your script** in the filtered results to attach it to the GameObject
   
   ![Select Script from List](Images/select_script.png)

5. The script should now appear in the Inspector panel with all its serialized fields visible

## Method 2: Drag and Drop from Project Window

You can also attach scripts using drag and drop:

1. **Find your script** in the Project window
   - Navigate to the folder containing your script (e.g., `Assets/Scripts/Core` for GameManager.cs)
   - Locate the script file (it will have a C# icon)
   
   ![Find Script in Project](Images/find_script_project.png)

2. **Select the target GameObject** in the Hierarchy panel

3. **Drag the script** from the Project window onto either:
   - The GameObject in the Hierarchy panel
   - Any empty area in the Inspector panel while the GameObject is selected
   
   ![Drag Script](Images/drag_script.png)

4. Unity will automatically attach the script to the GameObject

## Method 3: Using Script Code (Advanced)

For programmers who want to attach scripts through code:

```csharp
// Attach a script to the current GameObject
gameObject.AddComponent<GameManager>();

// Or to attach to another GameObject
GameObject targetObject = GameObject.Find("TargetName");
targetObject.AddComponent<GaslightFlicker>();
```

## Verifying Script Attachment

After attaching a script, you should check:

1. **The script appears in the Inspector** when the GameObject is selected
   
   ![Verify Script in Inspector](Images/verify_script.png)

2. **All serialized fields are visible** and can be edited
   - Public variables will appear in the Inspector
   - Private variables with [SerializeField] attribute will also appear
   
   ![Check Serialized Fields](Images/serialized_fields.png)

3. **No "Missing Script" errors** are shown
   - If you see "Missing Script" in red, the script may have been deleted, renamed, or moved
   
   ![Missing Script Error](Images/missing_script.png)

## Common Issues and Solutions

### Script Not Appearing in Search

If your script doesn't appear when searching in the Add Component menu:

1. **Check for compilation errors** in the Console window
2. **Make sure your script class name matches** the filename
3. **Verify the script is in a valid project folder** (must be in Assets folder or subfolder)

### Missing Script Reference

If you see "Missing Script" in the Inspector:

1. **Remove the missing component** by clicking the gear icon and selecting "Remove Component"
2. **Re-attach the script** using one of the methods above
3. If the script was renamed, you may need to update references in other scripts

### Script Cannot Be Added

If Unity won't let you add a script:

1. **Check if the script has a specific requirement** (like RequireComponent attribute)
2. **Verify the GameObject has all required components** first
3. **Check the Console for error messages**

## Example: Attaching the GameManager Script

Here's how to attach the GameManager script created for PDX Underground:

1. Create an empty GameObject named "GameManager"
2. With the GameManager object selected, click "Add Component" in the Inspector
3. Type "Game Manager" in the search field
4. Click on the GameManager script to attach it
5. Verify all fields appear in the Inspector:
   - _currentState
   - _timeOfDay
   - _weatherIntensity
   - _startupScene

## Example: Attaching the GaslightFlicker Script

To attach the GaslightFlicker script to a light object:

1. Create a GameObject with a Light component (or select an existing light)
2. With the light object selected, click "Add Component"
3. Type "Gaslight Flicker" in the search field
4. Click on the GaslightFlicker script
5. Configure the properties:
   - Set lanternType to GasLamp
   - Adjust flickerSpeed, minIntensity, and maxIntensity
   - Enable synchronization if desired

