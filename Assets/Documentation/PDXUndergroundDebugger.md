# PDX Underground Debugger

## Overview

The PDXUndergroundDebugger is a comprehensive runtime debugging and development tool for the PDX Underground game. It provides real-time monitoring of game systems, performance tracking, visual debugging, and development tools to help streamline the development process.

Key features:
- Real-time game state monitoring
- Visual debugging with UI element bounding boxes
- Performance metrics for critical operations
- Event logging system
- Development tools for testing game states
- Object pool statistics

![Debugger Preview](./Images/debugger_preview.png)

## Integration Instructions

### 1. Initial Setup

1. Create a new empty GameObject in your main scene:
   - Right-click in the Hierarchy panel
   - Select "Create Empty"
   - Rename it to "PDXUndergroundDebugger"

2. Add the PDXUndergroundDebugger component:
   - Select the new GameObject
   - In the Inspector panel, click "Add Component"
   - Search for "PDXUndergroundDebugger" and add it

3. Configure the inspectors fields (see Configuration section below)

4. Make it persist across scenes:
   - The debugger uses `DontDestroyOnLoad`, so it will automatically persist
   - No need to add it to every scene

### 2. Create Required UI Elements

The debugger requires several UI elements to display information. You can either:

**Option A: Create from scratch**

1. Create a Canvas for debug UI:
   - Right-click in Hierarchy, UI → Canvas
   - Rename to "DebugCanvas"
   - Set Canvas to "Screen Space - Overlay"
   - Add to the debugger GameObject
   
2. Create debug panel:
   - Create a UI Panel under the canvas
   - Set its RectTransform to cover a portion of the screen (recommended: right side)
   - Add a vertical layout group
   
3. Create text elements:
   - Add TextMeshPro - Text elements for stats and logs
   - Configure with monospace font
   
4. Create bounding box prefab:
   - Create a UI Image with border sprite
   - Add TextMeshPro child for labels
   - Save as prefab

**Option B: Import Provided Prefab**

1. Import the DebugUI prefab from the project repository
2. Drag it into your scene hierarchy
3. Assign references to the debugger component

### 3. Configure the Debugger Component

In the Inspector panel, configure the PDXUndergroundDebugger component:

1. Debug UI section:
   - Debug Panel: Assign your main debug panel GameObject
   - Stats Text: Assign the TextMeshPro element for stats
   - Event Log Text: Assign the TextMeshPro element for logs
   - Visual Debug Container: Create and assign an empty GameObject to hold bounding boxes
   - Bounding Box Prefab: Assign your bounding box prefab
   
2. Development Tools section:
   - Buzz Level Slider: Assign a UI Slider for controlling buzz level
   - Normal/Low/Critical Buzz Buttons: Assign UI Buttons for state changes
   - Environment Buttons: Assign buttons for switching environments (streets, tunnels, speakeasy)
   
3. Card Testing section:
   - Draw Card Button: Assign a button for drawing test cards
   - Use Card Button: Assign a button for using the top card
   - Card Type Dropdown: Assign a TMP_Dropdown for selecting card types
   
4. Settings section:
   - Max Log Entries: Set maximum number of log entries to display (default: 50)
   - Update Interval: Set how often stats update in seconds (default: 0.5)
   - Show Bounding Boxes: Toggle visual debugging
   - Track Performance: Toggle performance metrics
   - Toggle Key: Key to show/hide debugger (default: F12)

## Usage Guidelines

### Basic Usage

1. **Accessing the debugger**:
   - Press F12 (or your configured toggle key) to show/hide the debug panel
   - Debug UI will only be visible in the Unity Editor or Development Builds

2. **Using the stats display**:
   - The top section shows system information (FPS, memory usage)
   - The middle section shows game state (buzz levels, cards, environment)
   - The bottom section shows performance metrics

3. **Reading the event log**:
   - Color-coded events (red for errors, yellow for warnings, cyan for debug)
   - Timestamped log entries
   - Most recent events at the top

### Development Tools

1. **Testing buzz states**:
   - Use the slider to set specific buzz levels
   - Use buttons to jump directly to Normal/Low/Critical states
   - Watch the UI update in real-time

2. **Testing card mechanics**:
   - Select a card type from the dropdown
   - Click "Draw Card" to add a test card to the hand
   - Click "Use Card" to use the top card in hand

3. **Switching environments**:
   - Click environment buttons to change the current environment
   - Environment overlays and effects will update accordingly

### Visual Debugging

1. **Understanding bounding boxes**:
   - Green boxes: Card UI elements
   - Yellow boxes: Important UI components (meters, icons, buttons)
   - Each box displays the name of the GameObject

2. **Performance warnings**:
   - Red log entries indicate performance issues
   - Operations taking >100ms will trigger warnings
   - Use these to identify bottlenecks

### Logging From Your Code

You can use the debugger's logging system from anywhere in your code:

```csharp
// Using the static method (recommended)
PDXUndergroundDebugger.Log("Something happened", LogType.Info);

// Debug-only events
PDXUndergroundDebugger.Log("Debug details", LogType.Debug);

// Warnings
PDXUndergroundDebugger.Log("Warning: something might be wrong", LogType.Warning);

// Errors
PDXUndergroundDebugger.Log("Error: something is definitely wrong", LogType.Error);
```

## Performance Tracking

### Measuring Custom Operations

You can measure the performance of any custom operation:

```csharp
// Measure a simple operation
StartCoroutine(PDXUndergroundDebugger.Instance.MeasurePerformance("OperationName", () => {
    // Your operation code here
    DoSomethingExpensive();
}));
```

### Understanding Metrics

The performance display shows:
- Average execution time in milliseconds
- Minimum time recorded
- Maximum time recorded
- Number of times measured

Operations taking more than 100ms are automatically flagged as warnings.

## Troubleshooting

### Debugger Not Appearing

1. Check console for errors related to PDXUndergroundDebugger
2. Verify the toggle key is set correctly (default: F12)
3. Confirm you're running in the Editor or a Development Build
4. Check if UI Canvas is properly configured
5. Verify references in the debugger component are assigned

### Interface Components Not Found

If you see warnings about interfaces not found:
1. Make sure GamblerCharacter is in your scene
2. Verify MainGameController exists
3. Check that BuzzUIController is present
4. Use manual GameObject.Find in FindGameSystems() if needed

### Visual Debug Issues

If bounding boxes don't appear or look incorrect:
1. Check that visualDebugContainer is assigned
2. Verify boundingBoxPrefab has correct components
3. Make sure showBoundingBoxes is enabled
4. Check console for errors in DrawBoundingBox method

### Custom Logging Not Working

If your custom log messages don't appear:
1. Verify that PDXUndergroundDebugger exists in the scene
2. Check if you're using the static Log method correctly
3. Make sure debug UI is visible (toggle with F12)
4. Check if eventLogText is properly assigned

## Advanced Configuration

### Customizing the Debugger

The PDXUndergroundDebugger is designed to be extendable. Some ways to customize it:

1. **Add custom stat displays**:
   - Modify UpdateDebugInfo() to add your own system info
   - Create new UI elements for specialized metrics

2. **Add custom test tools**:
   - Extend the SetupUIControls() method
   - Add your own test buttons and controls to the debug panel

3. **Custom visual debugging**:
   - Add new visualization types in UpdateVisualDebugging()
   - Use different colors for different types of objects

### Build Configurations

The debugger automatically disables itself in non-development builds:

```csharp
#if !DEVELOPMENT_BUILD && !UNITY_EDITOR
gameObject.SetActive(false);
return;
#endif
```

To include the debugger in releases:
1. Remove or modify the conditional compiler directive above
2. Set a harder-to-trigger toggle key
3. Consider removing sensitive development tools

## Best Practices

1. **Keep debugger updated**:
   - When adding new systems, update the debugger to recognize them
   - Add new test tools for new gameplay mechanics

2. **Use performance metrics wisely**:
   - Focus on measuring operations that might be bottlenecks
   - Don't measure trivial operations (adds overhead)

3. **Custom logging guidelines**:
   - Use Debug type for developer-only information
   - Use Info for general system state changes
   - Use Warning for potential issues
   - Use Error for critical problems

4. **Visual debugging etiquette**:
   - Only draw bounding boxes around important elements
   - Use consistent color coding for element types

---

## Version History

- 1.0.0 (2025-04-11) - Initial implementation
  - Basic monitoring and debugging features
  - Buzz system testing tools
  - Card system testing tools
  - Environment switching tools

## Contacts

- For issues or improvements, contact the development team
- Repository: https://github.com/hawkpdx/PDX_Underground

