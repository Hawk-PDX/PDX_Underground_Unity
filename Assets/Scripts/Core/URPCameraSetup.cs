using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PDXUnderground.Core
{
    /// <summary>
    /// Helper class to add and configure URP camera components for the game's camera system.
    /// This ensures all cameras have the proper URP settings for post-processing and rendering.
    /// </summary>
    [ExecuteInEditMode]
    public class URPCameraSetup : MonoBehaviour
    {
        [Header("URP Camera Settings")]
        [SerializeField] private bool renderPostProcessing = true;
        [SerializeField] private AntialiasingMode antiAliasingMode = AntialiasingMode.FastApproximateAntialiasing;
        [SerializeField] private LayerMask volumeLayerMask = -1; // Everything by default
        
        [Header("Volume References")]
        [SerializeField] private VolumeProfile globalProfile;
        [SerializeField] private VolumeProfile streetsProfile;
        [SerializeField] private VolumeProfile tunnelsProfile;
        [SerializeField] private VolumeProfile speakeasyProfile;
        
        [Header("Global Volume")]
        [SerializeField] private bool createGlobalVolume = true;
        [SerializeField] private float globalVolumePriority = 0;
        
        private Camera _camera;
        private UniversalAdditionalCameraData _cameraData;
        private Volume _globalVolume;
        
        private void OnEnable()
        {
            // Get camera and URP data component
            _camera = GetComponent<Camera>();
            
            if (_camera == null)
            {
                Debug.LogError("No Camera found on GameObject. URPCameraSetup requires a Camera component.");
                return;
            }
            
            // Get or add URP camera data
            _cameraData = _camera.GetComponent<UniversalAdditionalCameraData>();
            if (_cameraData == null)
            {
                _cameraData = _camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
                Debug.Log("Added Universal Additional Camera Data to camera.");
            }
            
            // Apply URP camera settings
            ConfigureCameraForURP();
            
            // Create global volume if needed
            if (createGlobalVolume && globalProfile != null)
            {
                SetupGlobalVolume();
            }
        }
        
        /// <summary>
        /// Configures the camera with URP settings
        /// </summary>
        private void ConfigureCameraForURP()
        {
            if (_cameraData != null)
            {
                _cameraData.renderPostProcessing = renderPostProcessing;
                _cameraData.antialiasing = antiAliasingMode;
                _cameraData.volumeLayerMask = volumeLayerMask;
                _cameraData.renderShadows = true;
                
                // These are the optimal settings for our game effects
                _cameraData.requiresColorOption = CameraOverrideOption.Off;
                _cameraData.requiresDepthOption = CameraOverrideOption.Off;
                
                Debug.Log($"URP Camera settings configured on {gameObject.name}");
            }
        }
        
        /// <summary>
        /// Sets up a global volume component if needed
        /// </summary>
        private void SetupGlobalVolume()
        {
            // Look for existing global volume first
            Volume[] volumes = FindObjectsOfType<Volume>();
            Volume globalVolume = null;
            
            foreach (Volume vol in volumes)
            {
                if (vol.isGlobal)
                {
                    globalVolume = vol;
                    break;
                }
            }
            
            // Create a new global volume if none exists
            if (globalVolume == null)
            {
                GameObject volumeObject = new GameObject("Global Volume");
                globalVolume = volumeObject.AddComponent<Volume>();
                globalVolume.isGlobal = true;
                globalVolume.priority = globalVolumePriority;
                globalVolume.profile = globalProfile;
                
                Debug.Log("Created Global Volume with assigned profile");
            }
            
            _globalVolume = globalVolume;
        }
        
        /// <summary>
        /// Switch the current volume profile based on environment
        /// </summary>
        public void SwitchToEnvironment(EnvironmentType type)
        {
            if (_globalVolume == null) return;
            
            // First switch the base profile
            switch (type)
            {
                case EnvironmentType.Streets:
                    if (streetsProfile != null)
                        _globalVolume.profile = streetsProfile;
                    break;
                    
                case EnvironmentType.Tunnels:
                    if (tunnelsProfile != null)
                        _globalVolume.profile = tunnelsProfile;
                    break;
                    
                case EnvironmentType.Speakeasy:
                    if (speakeasyProfile != null)
                        _globalVolume.profile = speakeasyProfile;
                    break;
                    
                default:
                    if (globalProfile != null)
                        _globalVolume.profile = globalProfile;
                    break;
            }
            
            // Then apply environment-specific post-processing settings
            UpdateEnvironmentSettings(type);
            
            Debug.Log($"Switched to {type} environment profile");
        }
        
        /// <summary>
        /// Apply environment-specific post-processing settings
        /// </summary>
        private void UpdateEnvironmentSettings(EnvironmentType type)
        {
            // Ensure we have a volume and profile
            if (_globalVolume == null || _globalVolume.profile == null)
                return;
            // Get components from profile
            if (_globalVolume.profile.TryGet<UnityEngine.Rendering.Universal.Bloom>(out var bloom) &&
                _globalVolume.profile.TryGet<UnityEngine.Rendering.Universal.ColorAdjustments>(out var colorAdjust) &&
                _globalVolume.profile.TryGet<UnityEngine.Rendering.Universal.WhiteBalance>(out var whiteBalance) &&
                _globalVolume.profile.TryGet<UnityEngine.Rendering.Universal.Vignette>(out var vignette))
            {
                switch (type)
                {
                    case EnvironmentType.Streets:
                        // Streets settings
                        bloom.intensity.value = 0.15f;
                        bloom.threshold.value = 0.95f;
                        whiteBalance.temperature.value = -15f; // Cool tint
                        vignette.intensity.value = 0.35f;
                        
                        // Disable fog using built-in settings
                        RenderSettings.fog = false;
                        break;

                    case EnvironmentType.Tunnels:
                        // Tunnel settings
                        bloom.intensity.value = 0.25f;
                        bloom.threshold.value = 0.8f;
                        whiteBalance.temperature.value = -25f; // Cold/teal tint
                        vignette.intensity.value = 0.45f;
                        
                        // Add fog for tunnels using built-in settings
                        RenderSettings.fog = true;
                        RenderSettings.fogMode = FogMode.Exponential;
                        RenderSettings.fogDensity = 0.03f;
                        break;

                    case EnvironmentType.Speakeasy:
                        // Speakeasy settings
                        bloom.intensity.value = 0.4f;
                        bloom.threshold.value = 0.7f;
                        whiteBalance.temperature.value = 20f; // Warm tint
                        vignette.intensity.value = 0.45f;
                        
                        // Disable fog using built-in settings
                        RenderSettings.fog = false;
                        break;
                }
            }
        }
        
        /// <summary>
        /// Available environment types for profile switching
        /// </summary>
        public enum EnvironmentType
        {
            Global,
            Streets,
            Tunnels,
            Speakeasy
        }
    }
}

