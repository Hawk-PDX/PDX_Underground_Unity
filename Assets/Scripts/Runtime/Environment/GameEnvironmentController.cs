using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PDXUnderground.Effects;

namespace PDXUnderground.Environment
{
    /// <summary>
    /// Controls the environmental aspects of PDX Underground including day/night cycle,
    /// weather systems, and gaslight effects for 1880s Portland simulation.
    /// </summary>
    public class GameEnvironmentController : MonoBehaviour
    {
        [Header("Time Settings")]
        [SerializeField] private bool enableDayNightCycle = true;
        [SerializeField] private float dayDurationMinutes = 20f;
        [SerializeField] private float startTimeOfDay = 0.3f;
        [SerializeField] private AnimationCurve daytimeBrightnessCurve;
        [SerializeField] private Light sunLight;
        [SerializeField] private Light moonLight;

        [Header("Weather Settings")]
        [SerializeField] private bool enableWeatherSystem = true;
        [SerializeField] private float weatherChangeChance = 0.2f;
        [SerializeField] private float minWeatherDuration = 2.0f;
        [SerializeField] private GameObject[] weatherPrefabs;
        [SerializeField] private AudioClip[] weatherAmbience;

        [Header("Environment References")]
        [SerializeField] private List<GameObject> gaslights = new List<GameObject>();
        [SerializeField] private Material daySkybox;
        [SerializeField] private Material nightSkybox;
        [SerializeField] private AudioSource ambientAudioSource;
        [SerializeField] private float gaslightDayIntensity = 0.3f;
        [SerializeField] private float gaslightNightIntensity = 0.8f;
        [SerializeField] private bool autoFindGaslights = true;

        public enum Weather
        {
            Clear,
            Cloudy,
            Windy,
            Rain,
            Storm
        }

        private float currentTimeOfDay;
        private float timeRate;
        private Weather currentWeather = Weather.Clear;
        private GameObject activeWeatherEffect;
        private float lastWeatherChangeTime;
        private bool isNightTime = false;
        private Coroutine weatherCoroutine;

        // Events
        public delegate void TimeChangedHandler(float time);
        public event TimeChangedHandler OnTimeChanged;
        
        public delegate void WeatherChangedHandler(Weather weather);
        public event WeatherChangedHandler OnWeatherChanged;

        private void Awake()
        {
            timeRate = 1f / (dayDurationMinutes * 60f);
            
            if (sunLight == null)
            {
                sunLight = FindMainDirectionalLight();
            }

            if (gaslights.Count == 0 && autoFindGaslights)
            {
                FindAllGaslights();
            }

            if (daytimeBrightnessCurve == null || daytimeBrightnessCurve.keys.Length == 0)
            {
                SetupDefaultBrightnessCurve();
            }

            SetupAudioSource();
        }

        private void Start()
        {
            currentTimeOfDay = startTimeOfDay;
            UpdateEnvironmentForTime(currentTimeOfDay);
            SetWeather(Weather.Clear);

            if (enableWeatherSystem)
            {
                weatherCoroutine = StartCoroutine(WeatherCycle());
            }
        }

        private void Update()
        {
            if (enableDayNightCycle)
            {
                float previousTime = currentTimeOfDay;
                currentTimeOfDay = (currentTimeOfDay + timeRate * Time.deltaTime) % 1f;

                if (previousTime != currentTimeOfDay)
                {
                    UpdateEnvironmentForTime(currentTimeOfDay);
                    OnTimeChanged?.Invoke(currentTimeOfDay);
                }
            }
        }

        private void OnDestroy()
        {
            if (weatherCoroutine != null)
            {
                StopCoroutine(weatherCoroutine);
            }
        }

        #region Public Methods
        
        /// <summary>
        /// Gets the current time of day (0-1 range)
        /// </summary>
        public float GetTimeOfDay()
        {
            return currentTimeOfDay;
        }

        /// <summary>
        /// Gets the current weather as a string
        /// </summary>
        public string GetWeatherString()
        {
            return currentWeather.ToString();
        }
        
        #endregion
        
        #region Private Methods

        private void SetWeather(Weather weatherType)
        {
            currentWeather = weatherType;
            lastWeatherChangeTime = Time.time;
            UpdateWeatherEffects();
            OnWeatherChanged?.Invoke(currentWeather);
        }
        private void UpdateEnvironmentForTime(float time)
        {
            bool wasNight = isNightTime;
            isNightTime = time < 0.25f || time > 0.75f;

            if (wasNight != isNightTime)
            {
                if (isNightTime) OnNightfall();
                else OnDaybreak();
            }

            UpdateLighting(time);
            UpdateGaslights();
        }

        private void UpdateLighting(float time)
        {
            if (sunLight != null)
            {
                float sunRotation = time * 360f;
                sunLight.transform.rotation = Quaternion.Euler(sunRotation - 90f, 170f, 0f);
                sunLight.intensity = daytimeBrightnessCurve.Evaluate(time);
            }

            if (moonLight != null)
            {
                moonLight.gameObject.SetActive(isNightTime);
                if (isNightTime)
                {
                    float moonRotation = (time * 360f + 180f) % 360f;
                    moonLight.transform.rotation = Quaternion.Euler(moonRotation - 90f, 170f, 0f);
                }
            }

            RenderSettings.skybox = isNightTime ? nightSkybox : daySkybox;
        }

        private void UpdateGaslights()
        {
            foreach (GameObject gaslight in gaslights.ToList())
            {
                if (gaslight == null) continue;

                GaslightFlicker flicker = gaslight.GetComponent<GaslightFlicker>();
                if (flicker != null)
                {
                    flicker.SetTimeOfDay(currentTimeOfDay);
                    UpdateGaslightWeatherEffects(flicker);
                }
            }
        }

        private void UpdateGaslightWeatherEffects(GaslightFlicker flicker)
        {
            if (currentWeather != Weather.Clear)
            {
                float weatherIntensity = currentWeather == Weather.Storm ? 1.5f : 
                                       (currentWeather == Weather.Rain ? 1.3f : 1.0f);
                
                flicker.SetWeatherEffect(weatherIntensity);
                
                if (currentWeather == Weather.Windy || currentWeather == Weather.Storm)
                {
                    if (Random.value < 0.05f)
                    {
                        float dipAmount = currentWeather == Weather.Storm ? 0.6f : 0.3f;
                        flicker.TriggerDip(dipAmount);
                    }
                }
            }
            else
            {
                flicker.SetWeatherEffect(1.0f);
            }
        }

        private void UpdateWeatherEffects()
        {
            if (activeWeatherEffect != null)
            {
                Destroy(activeWeatherEffect);
            }

            if (weatherPrefabs != null && weatherPrefabs.Length > (int)currentWeather)
            {
                GameObject weatherPrefab = weatherPrefabs[(int)currentWeather];
                if (weatherPrefab != null)
                {
                    activeWeatherEffect = Instantiate(weatherPrefab, transform);
                }
            }

            UpdateAmbientAudio();
        }

        private void SetupAudioSource()
        {
            if (ambientAudioSource == null)
            {
                ambientAudioSource = gameObject.AddComponent<AudioSource>();
                ambientAudioSource.loop = true;
                ambientAudioSource.spatialBlend = 0f;
                ambientAudioSource.volume = 0.5f;
            }
        }

        private void UpdateAmbientAudio()
        {
            if (ambientAudioSource != null && weatherAmbience != null && 
                weatherAmbience.Length > (int)currentWeather)
            {
                AudioClip clip = weatherAmbience[(int)currentWeather];
                if (clip != null)
                {
                    ambientAudioSource.clip = clip;
                    ambientAudioSource.Play();
                }
            }
        }

        private void OnNightfall()
        {
            RenderSettings.ambientIntensity = 0.3f;
            foreach (GameObject gaslight in gaslights)
            {
                if (gaslight != null)
                {
                    var light = gaslight.GetComponent<Light>();
                    if (light != null) light.enabled = true;
                }
            }
        }

        private void OnDaybreak()
        {
            RenderSettings.ambientIntensity = 1f;
            foreach (GameObject gaslight in gaslights)
            {
                if (gaslight != null)
                {
                    var light = gaslight.GetComponent<Light>();
                    if (light != null) light.enabled = false;
                }
            }
        }

        private IEnumerator WeatherCycle()
        {
            while (true)
            {
                yield return new WaitForSeconds(minWeatherDuration * 60f);
                
                if (Random.value < weatherChangeChance)
                {
                    Weather newWeather = (Weather)Random.Range(0, System.Enum.GetValues(typeof(Weather)).Length);
                    if (newWeather == currentWeather)
                    {
                        newWeather = (Weather)(((int)newWeather + 1) % System.Enum.GetValues(typeof(Weather)).Length);
                    }
                    SetWeather(newWeather);
                }
                
                yield return new WaitForSeconds(Random.Range(0f, minWeatherDuration * 30f));
            }
        }

        private void FindAllGaslights()
        {
            GameObject[] lights = GameObject.FindGameObjectsWithTag("Gaslight");
            gaslights = new List<GameObject>(lights);
        }

        private Light FindMainDirectionalLight()
        {
            return FindObjectsOfType<Light>()
                .FirstOrDefault(light => light.type == LightType.Directional);
        }

        private void SetupDefaultBrightnessCurve()
        {
            daytimeBrightnessCurve = new AnimationCurve(
                new Keyframe(0.0f, 0.0f),   // Midnight
                new Keyframe(0.25f, 0.0f),  // Dawn start
                new Keyframe(0.35f, 1.0f),  // Morning
                new Keyframe(0.5f, 1.0f),   // Noon
                new Keyframe(0.65f, 1.0f),  // Afternoon
                new Keyframe(0.75f, 0.0f),  // Dusk end
                new Keyframe(1.0f, 0.0f)    // Midnight
            );
        }
        
        #endregion
    }
}
