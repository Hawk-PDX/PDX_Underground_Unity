using UnityEngine;
using UnityEditor;
using System.IO; // Add missing using directive for Directory operations
using PDXUnderground.Environment;
namespace PDXUnderground.Environment.Editor
{
    /// <summary>
    /// Editor tools for setting up the Portland environment in the PDX Underground game.
    /// </summary>
    [CustomEditor(typeof(SetupEnvironment))]
    public class EnvironmentSetupEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            SetupEnvironment setupEnvironment = (SetupEnvironment)target;
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "This component is responsible for setting up the environment directory structure. " +
                "Enter Play mode to execute the setup process.",
                MessageType.Info);
                
            if (GUILayout.Button("Setup Environment Now"))
            {
                if (EditorUtility.DisplayDialog(
                    "Execute Setup",
                    "This will create the required directory structure for the environment. Continue?",
                    "Yes", "Cancel"))
                {
                    // Create a log directory
                    if (!Directory.Exists("Logs"))
                    {
                        Directory.CreateDirectory("Logs");
                    }
                    
                    // Create the base directories
                    CreateDirectories();
                    
                    EditorUtility.DisplayDialog(
                        "Setup Complete",
                        "Environment directory structure has been created successfully.",
                        "OK");
                }
            }
        }
        
        private void CreateDirectories()
        {
            // Create base directories
            string[] baseDirs = new string[]
            {
                "Assets/Materials/Environment",
                "Assets/Textures/Environment",
                "Assets/Prefabs/Environment",
                "Assets/Models/Environment"
            };
            
            // Environment types
            string[] envTypes = new string[]
            {
                "Streets",
                "Tunnels",
                "Speakeasy"
            };
            
            // Create all directories
            // Create all directories
            foreach (string baseDir in baseDirs)
            {
                CreateDirectory(baseDir);
                
                // Create subdirectories for each environment type
                foreach (string envType in envTypes)
                {
                    CreateDirectory(Path.Combine(baseDir, envType));
                }
            }
        }

        private void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debug.Log($"Created directory: {path}");
            }
        }
    }
}
