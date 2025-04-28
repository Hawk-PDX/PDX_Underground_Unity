using UnityEngine;
using TMPro;

namespace PDXUnderground.Utilities
{
    /// <summary>
    /// Simple performance monitor for the test scene
    /// </summary>
    public class PerformanceMonitor : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI fpsText;
        [SerializeField] private TextMeshProUGUI particlesText;
        [SerializeField] private TextMeshProUGUI drawCallsText;
        
        private float fpsUpdateInterval = 0.5f;
        private float fpsAccumulator = 0f;
        private int frameCount = 0;
        private float timeRemaining;
        
        private void Start()
        {
            timeRemaining = fpsUpdateInterval;
        }
        
        private void Update()
        {
            // Accumulate FPS
            timeRemaining -= Time.deltaTime;
            fpsAccumulator += Time.timeScale / Time.deltaTime;
            frameCount++;
            
            // Update UI at the specified interval
            if (timeRemaining <= 0f)
            {
                // Calculate average FPS
                float averageFPS = fpsAccumulator / frameCount;
                
                // Update UI
                if (fpsText != null)
                    fpsText.text = $"FPS: {Mathf.Round(averageFPS)}";
                
                // Count active particles
                ParticleSystem[] particleSystems = FindObjectsOfType<ParticleSystem>();
                int totalParticles = 0;
                foreach (ParticleSystem ps in particleSystems)
                {
                    totalParticles += ps.particleCount;
                }
                
                if (particlesText != null)
                    particlesText.text = $"Particles: {totalParticles}";
                
                // Estimate draw calls (note: not accurate in newer Unity versions)
                if (drawCallsText != null)
                    drawCallsText.text = $"Draw Calls: ~{UnityEngine.Rendering.BatchRendererGroup.BufferCount}";
                
                // Reset for next interval
                fpsAccumulator = 0f;
                frameCount = 0;
                timeRemaining = fpsUpdateInterval;
            }
        }
    }
}

