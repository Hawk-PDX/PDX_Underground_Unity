# Minimal GamblerTest Scene Setup Guide

This guide will walk you through creating a minimal test scene to validate the Gambler's Buzz system and card mechanics using the provided scripts.

## Step 1: Create Basic Scene Structure

1. Open Unity and create a new scene (`File > New Scene`)
2. Save the scene as `Assets/Scenes/Test/GamblerTest.unity`
3. Create the following basic hierarchy:
   ```
   - Main Camera
   - Directional Light
   - GamblerCharacter (Empty GameObject)
   - Canvas
   - EventSystem
   ```

## Step 2: Configure Main Components

### Main Camera:
- Position: `(0, 1, -10)`
- Rotation: `(0, 0, 0)`
- Field of View: `60`

### Directional Light:
- Position: `(0, 3, 0)`
- Rotation: `(50, -30, 0)`
- Color: Set to a warm amber tone `#FFE8BA`
- Intensity: `0.7`

### GamblerCharacter:
1. Add a `GamblerCharacter` component to the GamblerCharacter GameObject
2. Configure with default values:
   - Max Buzz: `100`
   - Current Buzz: `100`
   - Buzz Depletion Rate: `5`
   - Buzz Regeneration Rate: `3`
   - Low Buzz Threshold: `30`
   - Critical Buzz Threshold: `10`
3. Add a visual representation (optional):
   - Add a child Capsule primitive
   - Position: `(0, 1, 0)`
   - Scale: `(1, 1, 1)`

## Step 3: Configure UI Canvas

1. Select the Canvas GameObject
2. In the Inspector, set Canvas properties:
   - Render Mode: `Screen Space - Overlay`
   - Canvas Scaler component:
     - UI Scale Mode: `Scale With Screen Size`
     - Reference Resolution: `1920 x 1080`
     - Match: `0.5` (both width and height)

## Step 4: Create UI Elements

### 1. Buzz Meter:
1. Create a UI Panel as child of Canvas
   - Name: `BuzzMeter`
   - Rect Transform:
     - Anchors: Top-left
     - Position: `(100, -50, 0)`
     - Size: `(200, 40)`
   - Image component:
     - Color: `#404040` (dark gray)

2. Add a Fill Bar as child of BuzzMeter
   - Name: `Fill`
   - Rect Transform:
     - Anchors: Stretch (all sides)
     - Offsets: `(2, 2, -2, -2)`
   - Image component:
     - Color: `#D4B86A` (gold/amber)
     - Image Type: `Filled`
     - Fill Method: `Horizontal`
     - Fill Origin: `Left`
     - Fill Amount: `1`

3. Add Text as child of BuzzMeter
   - Name: `Value`
   - Add TextMeshProUGUI component (create TMP Essentials if prompted)
   - Text: `100/100`
   - Font Size: `18`
   - Alignment: `Center`
   - Color: `White`
   - Rect Transform:
     - Anchors: Stretch (all sides)
     - Offsets: `(0, 0, 0, 0)`

4. Add an Icon as child of BuzzMeter
   - Name: `StateIcon`
   - Rect Transform:
     - Anchors: Middle Right
     - Position: `(-30, 0, 0)`
     - Size: `(30, 30)`
   - Image component:
     - Color: `White`

### 2. Control Panel:
1. Create a UI Panel as child of Canvas
   - Name: `ControlPanel`
   - Rect Transform:
     - Anchors: Top-right
     - Position: `(-150, -100, 0)`
     - Size: `(250, 200)`
   - Image component:
     - Color: `#1A1A1A80` (semi-transparent dark)

2. Add Buttons to ControlPanel:
   - Create 4 UI Buttons as children of ControlPanel
   - Configure each with proper labels:
     - Button 1: "Add Buzz (+10)"
       - Position: `(0, 70, 0)`
     - Button 2: "Remove Buzz (-10)"
       - Position: `(0, 20, 0)`
     - Button 3: "Critical Buzz"
       - Position: `(0, -30, 0)`
     - Button 4: "Reset Buzz"
       - Position: `(0, -80, 0)`
   - Size each button to `(180, 30)`

### 3. Card Ability Panel:
1. Create a UI Panel as child of Canvas
   - Name: `CardPanel`
   - Rect Transform:
     - Anchors: Bottom-center
     - Position: `(0, 100, 0)`
     - Size: `(400, 100)`
   - Image component:
     - Color: `#1A1A1A80` (semi-transparent dark)

2. Add Card Ability Buttons to CardPanel:
   - Create 3 UI Buttons as children of CardPanel
     - Button 1: "Draw Card"
       - Position: `(-120, 0, 0)`
     - Button 2: "Slice"
       - Position: `(0, 0, 0)`
     - Button 3: "Flick"
       - Position: `(120, 0, 0)`
   - Size each button to `(100, 60)`
   - Apply a gold tint to buttons: `#D4B86A` (with transparency)

### 4. Notification Panel:
1. Create a UI Panel as child of Canvas
   - Name: `NotificationPanel`
   - Rect Transform:
     - Anchors: Top-center
     - Position: `(0, -100, 0)`
     - Size: `(400, 100)`
   - Image component:
     - Color: `#1A1A1A80` (semi-transparent dark)
   - Set this GameObject to inactive initially

2. Add Text elements to NotificationPanel:
   - Title (TextMeshProUGUI):
     - Name: `Title`
     - Position: `(0, 30, 0)`
     - Font Size: `24`
     - Font Style: `Bold`
     - Alignment: `Center`
     - Color: `#D4B86A` (gold)
   - Message (TextMeshProUGUI):
     - Name: `Message`
     - Position: `(0, -10, 0)`
     - Font Size: `18`
     - Alignment: `Center`
     - Color: `White`

## Step 5: Add Controllers

### 1. Add BuzzUIController:
1. Select the Canvas GameObject
2. Add the `BuzzUIController` component
3. Assign references in the Inspector:
   - Buzz Meter Fill: Drag in the Fill Image
   - Buzz Value Text: Drag in the Value TextMeshProUGUI
   - State Icon: Drag in the StateIcon Image
   - Notification Panel: Drag in the NotificationPanel GameObject
   - Notification Title: Drag in the Title TextMeshProUGUI
   - Notification Text: Drag in the Message TextMeshProUGUI
   - Optional: Assign sprite references for the different buzz states

### 2. Add GamblerTestQuickStartExample:
1. Create an empty GameObject named `TestController`
2. Add the `GamblerTestQuickStartExample` component
3. Assign references in the Inspector:
   - Buzz UI Controller: Drag in the Canvas (with BuzzUIController)
   - Gambler Character: Drag in the GamblerCharacter GameObject
   - Increase Buzz Button: Drag in the "Add Buzz" button
   - Decrease Buzz Button: Drag in the "Remove Buzz" button
   - Critical Buzz Button: Drag in the "Critical Buzz" button
   - Normal Buzz Button: Drag in the "Reset Buzz" button
   - Draw Card Button: Drag in the "Draw Card" button
   - Slice Card Button: Drag in the "Slice" button
   - Flick Card Button: Drag in the "Flick" button

## Step 6: Test The Scene

1. Enter Play mode
2. Test the functionality:
   - Check the Console for debug messages
   - Press the "Add Buzz" and "Remove Buzz" buttons to verify the meter updates
   - Press "Critical Buzz" to test state changes and notifications
   - Press the card ability buttons to test notifications
3. Verify visual feedback:
   - The buzz meter fill should change smoothly
   - Notifications should appear and disappear automatically
   - The state icon should change based on buzz level

## Troubleshooting

### Missing references:
- Make sure all public variables in the BuzzUIController are properly assigned
- Check that all event connections between GamblerCharacter and BuzzUIController are working

### UI not displaying correctly:
- Verify Canvas Scaler is set to Scale With Screen Size with 1920x1080 reference
- Check that RectTransforms have proper anchoring and pivot points
- Ensure TextMeshPro components are correctly configured

### Console errors:
- Check for null references in the scripts
- Ensure the EventSystem is in the scene
- Verify button onClick events are properly connected

