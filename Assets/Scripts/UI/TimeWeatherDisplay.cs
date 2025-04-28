using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PDXUnderground.Controllers;

namespace PDXUnderground.UI
{
    /// <summary>
    /// Provides a UI display for time and weather in the PDX Underground environment.
    /// Uses a proper UI canvas instead of OnGUI for better visuals.
    /// </summary>
    public class TimeWeatherDisplay : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI weatherText;
        [SerializeField] private Image timeIcon;
        [SerializeField] private Image weatherIcon;
        [SerializeField] private Slider timeSlider;
        [SerializeField] private Slider weatherSlider;
        
        [Header("Visual Settings")]
        [SerializeField] private Sprite dayIcon;
        [SerializeField] private Sprite nightIcon;
        [SerializeField] private Sprite clearIcon;
        [SerializeField] private Sprite windyIcon;
        [SerializeField] private Sprite stormyIcon;
        
        // References
        private PDXUndergroundTestController testController;
        
        private void Start()
        {
            // Find the test controller
            testController = FindObjectOfType<PDXUndergroundTestController>();
            
            if (testController == null)
            {
                Debug.LogWarning("TimeWeatherDisplay could not find PDXUndergroundTestController");
                enabled = false;
                return;
            }
            
            // Subscribe to events if the controller supports them
            var controller = testController as PDXUndergroundTestController;
            if (controller != null && controller.OnTimeChanged != null)
            {
                controller.OnTimeChanged.AddListener(UpdateTimeDisplay);
                controller.OnWeatherChanged.AddListener(UpdateWeatherDisplay);
            }
        }
        
        private void Update()
        {
            // If we couldn't get event subscriptions, poll the values
            if (testController != null)
            {
                UpdateTimeDisplay(testController.GetCurrentTime());
                UpdateWeatherDisplay(testController.GetCurrentWeather());
                
                // Update sliders if available
                if (timeSlider != null)
                {
                    timeSlider.value = testController.GetCurrentTime();
                }
                
                if (weatherSlider != null)
                {
                    weatherSlider.value = testController.GetCurrentWeather();
                }
            }
        }
        
        private void UpdateTimeDisplay(float normalizedTime)
        {
            if (timeText != null)
            {
                int hours = Mathf.FloorToInt(normalizedTime * 24f);
                int minutes = Mathf.FloorToInt((normalizedTime * 24f - hours) * 60f);
                timeText.text = $"Time: {hours:D2}:{minutes:D2}";
            }
            
            // Update icon if available
            if (timeIcon != null)
            {
                bool isNight = normalizedTime < 0.25f || normalizedTime > 0.75f;
                timeIcon.sprite = isNight ? nightIcon : dayIcon;
            }
        }
        
        private void UpdateWeatherDisplay(float intensity)
        {
            if (weatherText != null)
            {
                string weatherDesc = "Clear";
                if (intensity < 0.2f) weatherDesc = "Clear";
                else if (intensity < 0.4f) weatherDesc = "Light Breeze";
                else if (intensity < 0.6f) weatherDesc = "Breezy";
                else if (intensity < 0.8f) weatherDesc = "Windy";
                else weatherDesc = "Stormy";
                
                weatherText.text = $"Weather: {weatherDesc}";
            }
            
            // Update icon if available
            if (weatherIcon != null)
            {
                if (intensity < 0.3f) weatherIcon.sprite = clearIcon;
                else if (intensity < 0.7f) weatherIcon.sprite = windyIcon;
                else weatherIcon.sprite = stormyIcon;
            }
        }
    }
}

