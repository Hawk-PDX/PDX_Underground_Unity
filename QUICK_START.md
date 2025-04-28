# PDX Underground - Quick Start Guide

This guide provides step-by-step instructions for setting up the PDX Underground environment in Unity.

## Opening and Compiling the Project

1. **Launch Unity Hub**
   - Open Unity Hub on your computer
   - If Unity Hub isn't installed, download it from [unity.com](https://unity.com/download)

2. **Open the Project**
   - In Unity Hub, click the "Open" button
   - Browse to the PDX_Underground_Unity directory
   - Select the project folder and click "Open"

3. **Wait for Compilation**
   - Unity will open the project and begin compiling scripts
   - This may take a few minutes depending on your computer
   - Wait until the "Compiling..." message in the bottom right corner disappears
   - The Unity Editor should be fully loaded when compilation completes

## Verifying Script Compilation

1. **Check Console for Errors**
   - Open the Console window by going to **Window > General > Console**
   - Look for any red error messages
   - If errors exist, fix them before proceeding

2. **Check for Setup Script Initialization**
   - In the Console, look for messages starting with `[InitialSetupScript]`
   - You should see "InitialSetupScript initialized" if the setup script loaded properly
   - If you don't see these messages, try the manual setup steps below

## Running the Environment Setup

### Automatic Setup
The setup should run automatically when Unity loads. If it did:

1. **Check for Setup Scene**
   - In the Project window, navigate to **Assets > Scenes**
   - You should see a `SetupScene.unity` file
   - Double-click to open it

2. **Verify Scene Contents**
   - In the Hierarchy window, look for the `EnvironmentSetup` GameObject
   - If it exists, the setup script is working correctly

3. **Run the Setup**
   - With SetupScene open, press the Play button at the top of the Editor
   - Check the Console for setup progress messages
   - When complete, press Play again to exit Play mode
   - Look for the message: "PDX Underground: Environment setup completed successfully."

### Manual Setup (If Automatic Failed)

1. **Create Setup Scene Manually**
   - Go to **File > New Scene**
   - Save it as `SetupScene.unity` in the `Assets/Scenes` folder

2. **Add Setup GameObject**
   - In the Hierarchy, right-click and select **Create Empty**
   - Rename it to `EnvironmentSetup`

3. **Add Setup Component**
   - Select the `EnvironmentSetup` GameObject
   - In the Inspector, click **Add Component**
   - Type "SetupEnvironment" and select it
   - If you can't find it, see troubleshooting below

4. **Run the Setup**
   - Press the Play button
   - Check Console for progress messages
   - When complete, exit Play mode

## Verifying Results

After running the setup:

1. **Check Folder Structure**
   - In the Project window, navigate through the Assets folder
   - You should see:
     - Assets/Materials/Environment/[Streets, Tunnels, Speakeasy]
     - Assets/Textures/Environment/[Streets, Tunnels, Speakeasy]
     - Assets/Models/Environment/[Streets, Tunnels, Lighting]
     - Assets/Prefabs/Environment/[Streets/Compositions, Streets/Props, Tunnels, Speakeasy]

2. **Check Log File**
   - If available, check `Logs/setup_log.txt` for detailed setup information

## Troubleshooting

### Script Not Compiling

1. **Check Script Location**
   - Ensure `SetupEnvironment.cs` is in the `Assets` folder
   - Ensure `InitialSetupScript.cs` is in the `Assets/Editor` folder

2. **Force Recompilation**
   - Make a small change to any script (add/remove a space)
   - Save the file to trigger recompilation

3. **Check Namespaces**
   - If errors mention namespaces, ensure the SetupEnvironment script doesn't have a namespace or uses the same namespace as other scripts

### SetupEnvironment Component Not Found

1. **Try Reopening Unity**
   - Close Unity completely
   - Reopen the project through Unity Hub

2. **Create the Script Manually**
   - If the SetupEnvironment.cs file is missing, create it with the content found in the README or SETUP_INSTRUCTIONS.txt

### Folder Creation Fails

1. **Check Permissions**
   - Ensure Unity has write permissions to the project directory
   - Try running Unity as administrator (Windows) or with proper permissions (Mac/Linux)

2. **Check for File Locks**
   - Close any applications that might be accessing files in the project
   - Try restarting your computer if issues persist

## Next Steps

After successful environment setup:

1. Follow the Environment Setup Wizard to generate:
   - Textures for streets, tunnels and speakeasy environments
   - Materials using these textures
   - Prefabs for environment elements

2. Set up your game scene using these assets

## Additional Resources

- Full documentation in `README.md`
- Detailed setup instructions in `SETUP_INSTRUCTIONS.txt`
- Environment implementation guide in `Assets/Documentation/EnvironmentImplementationGuide.md`

If you continue to experience issues, please refer to the "Troubleshooting" section in the README or contact the development team.

