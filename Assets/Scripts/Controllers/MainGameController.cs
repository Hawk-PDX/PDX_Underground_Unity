using UnityEngine;
using PDXUnderground.Core.Interfaces;

namespace PDXUnderground.Controllers
{
    public class MainGameController : MonoBehaviour, IEnvironmentSystem
    {
        [Header("Environment Assets")]
        [SerializeField] private Sprite[] environmentIcons;
        [SerializeField] private string[] environmentDescriptions;
        
        [Header("Time Settings")]
        [SerializeField] private float dayNightCycleDuration = 24f;
        [SerializeField] private float timeScale = 1f;
        
        [Header("Weather Settings")]
        [SerializeField] private string[] weatherStates = { "Clear", "Rainy", "Foggy" };
        [SerializeField] private float weatherChangeInterval = 300f;
        
        private IEnvironmentSystem.EnvironmentType currentEnvironment;
        private float currentDayTime;
        private int currentWeatherIndex;
        private float weatherChangeTimer;
        
        // Interface Events
        public event System.Action<IEnvironmentSystem.EnvironmentType, string> OnEnvironmentChanged;
        public event System.Action<string, int> OnEnvironmentTypeChanged;

        private void Start()
        {
            // Initialize with default values
            currentDayTime = 12f; // Start at noon
            currentWeatherIndex = 0; // Start with clear weather
            weatherChangeTimer = weatherChangeInterval;
            
            // Start in the streets environment
            ChangeEnvironment(IEnvironmentSystem.EnvironmentType.Streets);
        }
        
        private void Update()
        {
            // Update time of day
            currentDayTime += Time.deltaTime * timeScale;
            if (currentDayTime >= dayNightCycleDuration)
            {
                currentDayTime = 0f;
            }
            
            // Update weather
            weatherChangeTimer -= Time.deltaTime;
            if (weatherChangeTimer <= 0)
            {
                UpdateWeather();
                weatherChangeTimer = weatherChangeInterval;
            }
        }

        #region IEnvironmentSystem Implementation
        public void ChangeEnvironment(IEnvironmentSystem.EnvironmentType environmentType)
        {
            currentEnvironment = environmentType;
            string envName = GetEnvironmentName(environmentType);
            OnEnvironmentChanged?.Invoke(environmentType, envName);
            OnEnvironmentTypeChanged?.Invoke(envName, (int)environmentType);
        }

        public void ChangeEnvironment(int environmentIndex)
        {
            if (System.Enum.IsDefined(typeof(IEnvironmentSystem.EnvironmentType), environmentIndex))
            {
                ChangeEnvironment((IEnvironmentSystem.EnvironmentType)environmentIndex);
            }
        }

        public string GetCurrentEnvironmentName()
        {
            return GetEnvironmentName(currentEnvironment);
        }

        public string GetEnvironmentDescription(int environmentType)
        {
            if (environmentDescriptions != null && environmentType < environmentDescriptions.Length)
            {
                return environmentDescriptions[environmentType];
            }
            return "No description available";
        }

        public Sprite GetEnvironmentIcon(int environmentType)
        {
            if (environmentIcons != null && environmentType < environmentIcons.Length)
            {
                return environmentIcons[environmentType];
            }
            return null;
        }

        public string GetTimeOfDay()
        {
            int hours = Mathf.FloorToInt((currentDayTime / dayNightCycleDuration) * 24f);
            int minutes = Mathf.FloorToInt(((currentDayTime / dayNightCycleDuration) * 24f - hours) * 60f);
            return $"{hours:D2}:{minutes:D2}";
        }

        public string GetWeatherString()
        {
            return weatherStates[currentWeatherIndex];
        }
        #endregion

        #region Private Helper Methods
        private void UpdateWeather()
        {
            // Randomly select new weather, avoiding current weather
            int newWeather;
            do
            {
                newWeather = Random.Range(0, weatherStates.Length);
            } while (newWeather == currentWeatherIndex);
            
            currentWeatherIndex = newWeather;
            Debug.Log($"Weather changed to: {GetWeatherString()}");
        }

        private string GetEnvironmentName(IEnvironmentSystem.EnvironmentType type)
        {
            switch (type)
            {
                case IEnvironmentSystem.EnvironmentType.Streets:
                    return "Streets";
                case IEnvironmentSystem.EnvironmentType.Tunnels:
                    return "Shanghai Tunnels";
                case IEnvironmentSystem.EnvironmentType.Speakeasy:
                    return "Speakeasy";
                default:
                    return "Unknown";
            }
        }
        #endregion
    }
}
