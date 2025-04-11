using UnityEngine;

public class UnityVSCodeTest : MonoBehaviour
{
    [SerializeField]
    private string testMessage = "Hello from Unity and VS Code!";
    
    private float timer = 0f;
    private int updateCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        // This will appear in Unity's console when the script starts
        Debug.Log("Test Script Started!");
        
        // Good place to set a breakpoint for testing the debugger
        InitializeTest();
    }

    // Update is called once per frame
    void Update()
    {
        // Update timer every second
        timer += Time.deltaTime;
        if (timer >= 1f)
        {
            updateCount++;
            Debug.Log($"Update count: {updateCount} - {testMessage}");
            timer = 0f;
            
            // Test different debug message types
            if (updateCount % 3 == 0)
            {
                Debug.LogWarning("This is a warning message!");
            }
            else if (updateCount % 5 == 0)
            {
                Debug.LogError("This is an error message!");
            }
        }
    }

    private void InitializeTest()
    {
        // This method is just for setting a breakpoint
        Debug.Log("Initialization complete!");
    }

    private void OnEnable()
    {
        Debug.Log("Script was enabled!");
    }

    private void OnDisable()
    {
        Debug.Log("Script was disabled!");
    }
}

