using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PDXUnderground.Core;

namespace PDXUnderground.Test
{
    /// <summary>
    /// Test script to verify GameManager accessibility
    /// </summary>
    public class GameManagerTest : MonoBehaviour
    {
        // Reference to GameManager to test namespace accessibility
        private GameManager gameManagerRef;
        
        void Start()
        {
            Debug.Log("GameManagerTest: Attempting to access GameManager");
            
            // Try to find existing GameManager
            try
            {
                gameManagerRef = GameManager.Instance;
                if (gameManagerRef != null)
                {
                    Debug.Log("GameManagerTest: Successfully accessed GameManager instance!");
                }
                else
                {
                    Debug.LogWarning("GameManagerTest: GameManager.Instance returned null");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"GameManagerTest: Error accessing GameManager: {e.Message}");
            }
            
            // Try to create new instance - this is just a test, normally you'd use singleton
            try
            {
                GameObject testObj = new GameObject("TestGameManager");
                GameManager testManager = testObj.AddComponent<GameManager>();
                
                if (testManager != null)
                {
                    Debug.Log("GameManagerTest: Successfully created GameManager component");
                    Destroy(testObj); // Clean up
                }
                else
                {
                    Debug.LogError("GameManagerTest: Failed to create GameManager component");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"GameManagerTest: Error creating GameManager: {e.Message}");
            }
        }
        
        // Check namespace and assembly info
        void CheckNamespaces()
        {
            Type gameManagerType = typeof(GameManager);
            Debug.Log($"GameManager type: {gameManagerType.FullName}");
            Debug.Log($"GameManager assembly: {gameManagerType.Assembly.FullName}");
            
            Type thisType = this.GetType();
            Debug.Log($"This type: {thisType.FullName}");
            Debug.Log($"This assembly: {thisType.Assembly.FullName}");
        }
    }
}

