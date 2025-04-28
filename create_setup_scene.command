#!/bin/bash
# Script to create the setup scene for PDX Underground using Unity's command line interface

# Change to script directory
cd "$(dirname "$0")"

# Set Unity path - adjust according to your Unity installation
UNITY_PATH="/Applications/Unity/Hub/Editor/2022.3.61f1/Unity.app/Contents/MacOS/Unity"

# Project path
PROJECT_PATH="$(pwd)"

# Create a temporary editor script that will be executed by Unity
TMP_SCRIPT="Assets/Editor/TempSceneCreator.cs"

# Create the directory if it doesn't exist
mkdir -p "$(dirname "$TMP_SCRIPT")"

# Create the temporary script
cat > "$TMP_SCRIPT" << 'EOL'
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

// This script will be executed automatically when Unity starts
[InitializeOnLoad]
public static class TempSceneCreator
{
    static TempSceneCreator()
    {
        EditorApplication.delayCall += CreateScene;
    }
    
    static void CreateScene()
    {
        const string ScenePath = "Assets/Scenes/SetupScene.unity";
        
        try
        {
            Debug.Log("Creating setup scene...");
            
            // Create Scenes directory if needed
            string directory = Path.GetDirectoryName(ScenePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }
            
            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            // Create EnvironmentSetup GameObject
            GameObject setupObj = new GameObject("EnvironmentSetup");
            
            // Add SetupEnvironment component if available
            System.Type setupType = System.Type.GetType("SetupEnvironment");
            if (setupType != null)
            {
                setupObj.AddComponent(setupType);
                Debug.Log("Added SetupEnvironment component");
            }
            else
            {
                Debug.LogWarning("SetupEnvironment component type not found");
            }
            
            // Save scene
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("Setup scene created at: " + ScenePath);
            
            // Clean up by deleting this temporary script
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(MonoScript.FromStaticMethodAsString("TempSceneCreator.CreateScene")));
            
            // Quit Unity
            EditorApplication.Exit(0);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error creating setup scene: " + ex.Message);
            EditorApplication.Exit(1);
        }
    }
}
EOL

# Run Unity in batchmode to create the scene
echo "Running Unity to create setup scene..."
"$UNITY_PATH" -batchmode -projectPath "$PROJECT_PATH" -logFile create_scene.log -quit

# Check exit code
if [ $? -eq 0 ]; then
    echo "Scene created successfully!"
    echo "To complete setup:"
    echo "1. Open Unity and load the project"
    echo "2. Open SetupScene.unity"
    echo "3. Enter Play mode to run the environment setup"
else
    echo "Failed to create scene. Check create_scene.log for details."
fi

# Clean up temporary script if it still exists
if [ -f "$TMP_SCRIPT" ]; then
    rm "$TMP_SCRIPT"
fi

echo "Script completed"

