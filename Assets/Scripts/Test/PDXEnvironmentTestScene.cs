using UnityEngine;
using UnityEditor;
using PDXUnderground.Environment;

/// <summary>
/// Test scene for verifying PDX Underground environment generation
/// </summary>
public class PDXEnvironmentTestScene : MonoBehaviour
{
    [Header("Environment Components")]
    public PortlandEnvironmentSetup environmentSetup;
    public PDXProBuilderGenerator proBuilderGenerator;

    [Header("Test Configuration")]
    public bool generateOnStart = true;
    public bool showDebugVisuals = true;
    public Vector2Int testGridSize = new Vector2Int(3, 3);

    private void Start()
    {
        if (generateOnStart)
        {
            GenerateTestEnvironment();
        }
    }

    public void GenerateTestEnvironment()
    {
        if (environmentSetup == null || proBuilderGenerator == null)
        {
            Debug.LogError("Missing required components for test scene");
            return;
        }

        // Configure for test generation
        environmentSetup.gridSize = testGridSize;
        
        // Generate basic environment
        environmentSetup.GenerateEnvironment();

        // Add test visualization
        if (showDebugVisuals)
        {
            DrawDebugGrid();
        }

        Debug.Log("Test environment generated successfully");
    }

    private void DrawDebugGrid()
    {
        float blockSize = 61f; // Historical Portland block size
        Vector3 origin = transform.position;

        for (int x = 0; x <= testGridSize.x; x++)
        {
            Vector3 start = origin + new Vector3(x * blockSize, 0, 0);
            Vector3 end = start + new Vector3(0, 0, testGridSize.y * blockSize);
            Debug.DrawLine(start, end, Color.yellow, 100f);
        }

        for (int z = 0; z <= testGridSize.y; z++)
        {
            Vector3 start = origin + new Vector3(0, 0, z * blockSize);
            Vector3 end = start + new Vector3(testGridSize.x * blockSize, 0, 0);
            Debug.DrawLine(start, end, Color.yellow, 100f);
        }
    }

    private void OnDrawGizmos()
    {
        if (showDebugVisuals)
        {
            // Draw block size reference
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position + new Vector3(30.5f, 0, 30.5f), new Vector3(61f, 1f, 61f));
        }
    }
}
