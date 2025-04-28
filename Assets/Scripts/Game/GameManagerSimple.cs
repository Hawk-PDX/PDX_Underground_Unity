using UnityEngine;

namespace PDXUnderground.Core
{
    /// <summary>
    /// Simplified GameManager for PDX Underground
    /// </summary>
    public class GameManagerSimple : MonoBehaviour
    {
        // Singleton instance
        private static GameManagerSimple _instance;
        
        // Public accessor with lazy initialization
        public static GameManagerSimple Instance 
        { 
            get 
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameManagerSimple>();
                    
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject("GameManagerSimple");
                        _instance = obj.AddComponent<GameManagerSimple>();
                        DontDestroyOnLoad(obj);
                        Debug.Log("GameManagerSimple created");
                    }
                }
                
                return _instance;
            }
        }
        
        // Test property
        [SerializeField] private float _timeOfDay = 0.5f;
        public float TimeOfDay => _timeOfDay;
        
        // Ensure only one instance exists
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("Multiple GameManagerSimple instances. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManagerSimple initialized");
        }
        
        // Simple test method
        public void TestMethod()
        {
            Debug.Log("GameManagerSimple test method called");
        }
    }
}

