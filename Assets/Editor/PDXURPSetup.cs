using UnityEngine;
using UnityEditor;

public class PDXURPSetup
{
    [MenuItem("Tools/Setup URP")]
    static void SetupURP()
    {
        var urpAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset>(
            "Assets/Settings/URP/UniversalRP-HighQuality.asset");
            
        if (urpAsset != null)
        {
            UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset = urpAsset;
            QualitySettings.renderPipeline = urpAsset;
            Debug.Log("URP setup complete!");
            EditorUtility.DisplayDialog("Success", "URP has been configured successfully!", "OK");
        }
        else
        {
            Debug.LogError("Could not find URP asset");
            EditorUtility.DisplayDialog("Error", "Could not find URP asset", "OK");
        }
    }
}
