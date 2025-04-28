# PDX Underground - Quick Start Guide

This quick start guide will walk you through the process of setting up the period-appropriate 1880s Portland environment for the PDX Underground game using the Environment Setup Wizard.

## Prerequisites

Before getting started, ensure that:
1. Unity is properly installed and the PDX Underground project is opened
2. All scripts have been compiled without errors
3. You have the necessary permissions to create files in the project directory

## Using the Environment Setup Wizard

### Step 1: Launch the Wizard

1. Open Unity with the PDX Underground project
2. From the top menu, navigate to **PDX Underground > Environment Setup Wizard**
3. The wizard will open showing the various stages of environment setup

### Step 2: Generate Textures

1. In the Environment Setup Wizard, find the "1. Generate Textures" section
2. Click the "Start" button
3. When the Texture Generator window opens, configure the following settings:
   - **Cobblestone Texture Size**: 2048 x 2048
   - **Sidewalk Texture Size**: 1024 x 1024
   - **Building Facade Size**: 2048 x 2048
   - **Cobblestone Color**: RGB(0.45, 0.45, 0.45)
   - **Sidewalk Color**: RGB(0.55, 0.45, 0.3)
   - **Building Facade Color**: RGB(0.6, 0.55, 0.5)
4. Ensure both "Create Street Textures" and "Create Building Textures" options are checked
5. Click the "Generate Textures" button
6. Wait for the texture generation process to complete (you'll see a confirmation in the console)
7. Return to the Environment Setup Wizard and click "Refresh Status" to verify completion

### Step 3: Create Materials

1. Once textures are generated, proceed to the "2. Create Materials" section
2. Click the "Start" button
3. In the Material Generator window, ensure all options are checked:
   - Create Street Materials
   - Create Tunnel Materials
   - Create Speakeasy Materials
4. Click the "Generate Materials" button
5. Wait for the material generation to complete
6. Return to the Environment Setup Wizard and refresh the status

### Step 4: Generate Street Environment Prefabs

1. After materials are created, continue to the "3. Generate Prefabs" section
2. Click the "Start" button
3. In the Street Environment Generator window, configure:
   - **Block Size**: 61 (historically accurate for 1880s Portland)
   - **Street Width**: 6
   - **Sidewalk Width**: 2
   - **Building Height**: 12
4. If prompted about materials, click "Try Load Materials"
5. Click "Generate Street Environment"
6. Wait for the prefab generation to complete
7. Return to the wizard and refresh the status

### Step 5: Create the Complete Scene

1. Finally, move to the "4. Setup Complete Scene" section
2. Click the "Start" button
3. In the Scene Generator window, configure:
   - **Grid Size**: 3x3 (creates a 3x3 block area)
   - **Block Size**: 61
   - **Setup UI Elements**: Checked
   - **Generate NavMesh**: Checked
4. Click "Try Load Resources" to confirm all assets are available
5. Click "Create Street Environment Scene"
6. The scene will be created and set up with all environment elements

## Troubleshooting

If you encounter any issues during the setup process:

1. **Missing textures or materials**: Ensure all previous steps have been completed successfully. Click "Refresh Status" in the wizard to verify.

2. **Script errors**: Check the console for any error messages. Make sure all required script components are properly implemented.

3. **Performance issues**: After scene generation, you can adjust LOD settings and light probe density in the generated scene to optimize performance.

## Next Steps

After completing the environment setup:

1. Play the scene to test navigation and visual appearance
2. Adjust lighting settings if needed for the desired mood
3. Fine-tune material properties for greater historical accuracy
4. Add gameplay elements and characters to the environment

For more detailed instructions, refer to the full [Implementation Guide](./EnvironmentImplementationGuide.md) included in the project.

