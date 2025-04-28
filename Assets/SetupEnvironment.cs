using UnityEngine;
using System.IO;

/// <summary>
/// Direct environment setup script for PDX Underground.
/// This script will run in Play mode to set up the environment.
/// </summary>
public class SetupEnvironment : MonoBehaviour
{
    // Start is called on the first frame update
    void Start()
    {
        Debug.Log("PDX Underground: Starting environment setup...");
        
        // Create logs directory to store detailed logs
        Directory.CreateDirectory("Logs");
        string logPath = Path.Combine("Logs", "setup_log.txt");
        
        using (StreamWriter writer = new StreamWriter(logPath, false))
        {
            writer.WriteLine("PDX Underground Environment Setup");
            writer.WriteLine($"Started: {System.DateTime.Now}");
            writer.WriteLine("----------------------------------------");
            
            try
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
                foreach (string baseDir in baseDirs)
                {
                    CreateDirectoryAndLog(baseDir, writer);
                    
                    // Create environment type subdirectories
                    foreach (string envType in envTypes)
                    {
                        string subDir = Path.Combine(baseDir, envType);
                        CreateDirectoryAndLog(subDir, writer);
                    }
                }
                
                // Create additional directories
                CreateDirectoryAndLog("Assets/Prefabs/Environment/Streets/Compositions", writer);
                CreateDirectoryAndLog("Assets/Prefabs/Environment/Streets/Props", writer);
                
                // Log completion
                Debug.Log("PDX Underground: Environment setup completed successfully.");
                writer.WriteLine("Setup completed successfully!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"PDX Underground: Setup failed - {ex.Message}");
                writer.WriteLine($"ERROR: {ex.Message}");
                writer.WriteLine(ex.StackTrace);
            }
            
            writer.WriteLine("----------------------------------------");
            writer.WriteLine($"Finished: {System.DateTime.Now}");
        }
        
        Debug.Log($"PDX Underground: Setup log written to {Path.GetFullPath(logPath)}");
    }
    
    private void CreateDirectoryAndLog(string path, StreamWriter writer)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            string message = $"Created directory: {path}";
            Debug.Log(message);
            writer.WriteLine(message);
        }
        else
        {
            string message = $"Directory already exists: {path}";
            Debug.Log(message);
            writer.WriteLine(message);
        }
    }
}

