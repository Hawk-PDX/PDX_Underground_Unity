using UnityEngine;
using System;

namespace PDXUnderground.Systems
{
    public class WeatherSystem : MonoBehaviour
    {
        public event Action OnRainStart;
        
        public static bool IsRaining => false;
        public float RainIntensity = 0f;
        
        // Method to trigger rain start event for testing
        public void StartRain()
        {
            OnRainStart?.Invoke();
        }
        
        // Minimal implementation matching your TraumaSystem needs
        public bool CheckStormWarning() => false;
    }
}
