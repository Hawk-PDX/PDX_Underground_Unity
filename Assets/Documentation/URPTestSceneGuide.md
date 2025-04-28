# URP Test Scene Update Guide

## Overview
This guide focuses specifically on updating test scenes in the PDX_Underground_Unity project to properly work with Universal Render Pipeline (URP).

## Test Scene Specific Updates

### 1. Lighting Updates
Current setup in TestSceneLayout.cs needs these URP-specific changes:

```csharp
// Update DirectionalLight creation in CreateLighting() method:
light.type = LightType.Directional;
light.intensity = 1.5f; // Adjusted for Linear color space
light.shadows = LightShadows.Soft;
light.useColorTemperature = true; // Enable for better color control
light.shadowResolution = LightShadowResolution.FromQualitySettings;

// Add this for better shadow quality
light.shadowStrength = 0.8f;
light.shadowBias = 0.05f;
light.shadowNormalBias = 0.4f;
```

### 2. Material Updates
For test scene primitives (Ground, Walls, Props):

1. Create URP materials in Assets/Materials/Test:
   - TestGround.mat (URP/Lit with simple texture)
   - TestWall.mat (URP/Lit)
   - TestProp.mat (URP/Lit for cubes, spheres, etc.)

2. Update Material Application Code:

```csharp
// Add to the class
[Header("Materials")]
[SerializeField] private Material testGroundMaterial;
[SerializeField] private Material testWallMaterial;
[SerializeField] private Material testPropMaterial;

// Update CreateWall() method:
private GameObject CreateWall(string name, Transform parent, Vector3 position, Vector3 scale)
{
    GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
    wall.name = name;
    wall.transform.SetParent(parent);
    wall.transform.localPosition = position;
    wall.transform.localScale = scale;
    
    // Apply URP material
    var renderer = wall.GetComponent<MeshRenderer>();
    if (testWallMaterial != null)
    {
        renderer.material = testWallMaterial;
    }
    else
    {
        Debug.LogWarning("TestWall material is not assigned. Using default material.");
    }
    
    return wall;
}

// Update CreateProp() method similarly:
private GameObject CreateProp(string primitiveType, Transform parent, Vector3 position, Vector3 scale)
{
    // Existing code...
    
    // Apply URP material
    var renderer = prop.GetComponent<MeshRenderer>();
    if (testPropMaterial != null)
    {
        renderer.material = testPropMaterial;
    }
    
    return prop;
}
```

### 3. Camera Setup
Add a new method to TestSceneLayout.cs to configure the camera for URP:

```csharp
[ContextMenu("Setup Camera for URP")]
public void SetupMainCamera()
{
    // Find main camera if it exists
    var camera = Camera.main;
    if (camera == null)
    {
        Debug.LogError("No Main Camera found in scene. Create one first.");
        return;
    }
    
    // Add Universal Additional Camera Data if missing
    var urpData = camera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
    if (urpData == null)
    {
        urpData = camera.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        Debug.Log("Added Universal Additional Camera Data to main camera");
    }
    
    // Configure URP camera settings
    urpData.renderPostProcessing = true;
    urpData.volumeLayerMask = LayerMask.GetMask("Everything");
    urpData.renderShadows = true;
    urpData.requiresColorOption = UnityEngine.Rendering.Universal.CameraOverrideOption.Off;
    urpData.requiresDepthOption = UnityEngine.Rendering.Universal.CameraOverrideOption.Off;
    
    // Setup post-processing volume
    GameObject volumeGO = GameObject.Find("Global Volume");
    if (volumeGO == null)
    {
        volumeGO = CreateOrGetGameObject("Global Volume", null);
    }
    
    var volumeComponent = volumeGO.GetComponent<UnityEngine.Rendering.Volume>();
    if (volumeComponent == null)
    {
        volumeComponent = volumeGO.AddComponent<UnityEngine.Rendering.Volume>();
        volumeComponent.isGlobal = true;
        Debug.Log("Added Volume component to Global Volume GameObject");
        
        // Note: Profile creation needs to be done through Unity Editor
        // Add prompt for this
        Debug.Log("Important: Create a new Volume Profile asset and assign it to the Volume component");
    }
}
```

### 4. Create Test Scene Post-Processing Profile
To create a test scene post-processing profile:

1. In Unity Editor, right-click in Project window > Create > Volume Profile
2. Name it "TestSceneProfile"
3. Select the Global Volume in your scene
4. Assign the profile
5. Add these common post-processing effects:
   - Bloom
     * Threshold: 0.9
     * Intensity: 0.2
   - Tonemapping
     * Mode: ACES
   - Color Adjustments
     * Post Exposure: 0.1
     * Contrast: 10
     * Saturation: 10

### 5. UI Updates
The BuzzUI prefab might need material updates for any UI effects or shaders:

1. Check if BuzzUIPrefab uses any custom shaders
2. If so, replace with URP-compatible UI shaders
3. Ensure Text Mesh Pro uses compatible materials (should be automatic)

### 6. Complete Update Process

1. First apply the core URP settings changes (from main guide)
2. Create the required materials in Unity Editor
3. Open a test scene
4. Add the SetupMainCamera method to TestSceneLayout.cs
5. Select the TestSceneLayout GameObject and run the "Setup Camera for URP" context menu command
6. Create and assign Volume Profile
7. Update any material references in the inspector
8. Test render quality and lighting

