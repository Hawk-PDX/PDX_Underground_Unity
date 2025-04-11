using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PDXUnderground.Environment
{
    /// <summary>
    /// GameEnvironmentController manages the environment settings for PDX Underground,
    /// including day/night cycle, weather systems, and environmental effects for
    /// historical Portland simulation.
    /// </summary>
    public class GameEnvironmentController : MonoBehaviour
    {
        #region Serialized Fields
        
        [Header("Time Settings")]
        [SerializeField] private bool enableDayNightCycle = true;
        [SerializeField] private float dayDurationMinutes = 20f; // real-time minutes for a full day cycle
        [SerializeField] private float startTimeOfDay = 0.3f; // 0-1 value (0 = midnight, 0.5 = noon)
        [SerializeField] private AnimationCurve daytimeBrightnessCurve;
        [SerializeField] private Light sunLight;
        [SerializeField] private Light moonLight;
        
        [Header("Weather Settings")]
        [SerializeField] private bool enableWeatherSystem = true;
        [SerializeField] private float weatherChangeProbability = 0.1f; // chance per hour to change weather
        [SerializeField] private float minWeatherDuration = 10f; // minimum minutes before weather can change
        [SerializeField] private GameObject[] weatherPrefabs; // rain, fog, clear prefabs
        [SerializeField] private AudioClip[] weatherAmbience; // ambient sounds for each weather type
        
        [Header("Environment References")]
        [SerializeField] private List<GameObject> gaslights = new List<GameObject>();
        [SerializeField] private Material daySkybox;
        [SerializeField] private Material nightSkybox;
        [SerializeField] private AudioSource ambientAudioSource;
        [SerializeField] private float gaslightDayIntensity = 0.3f;
        [SerializeField] private float gaslightNightIntensity = 0.8f;
        [SerializeField] private bool autoFindGaslights = true;
        
        [Header("NPC System")]
        [SerializeField] private bool enableNPCSystem = true;
        [SerializeField] private int maxActiveNPCs = 25;
        [SerializeField] private GameObject[] npcPrefabs;
        [SerializeField] private Transform[] npcSpawnPoints;
        [SerializeField] private float npcDespawnDistance = 50f;
        
        [Header("Port Activity")]
        [SerializeField] private bool enablePortActivity = true;
        [SerializeField] private GameObject[] shipPrefabs;
        [SerializeField] private Transform[] dockingPoints;
        [SerializeField] private float[] shipArrivalTimes; // time of day (0-1) when ships arrive
        [SerializeField] private float[] shipDepartureTimes; // time of day (0-1) when ships depart
        
        #endregion
        
        #region Private Variables
        
        private float currentTimeOfDay; // 0-1 value representing time of day
        private float timeRate; // calculated time rate based on day duration
        private Weather currentWeather = Weather.Clear;
        private GameObject activeWeatherEffect;
        private float lastWeatherChangeTime;
        private List<GameObject> activeNPCs = new List<GameObject>();
        private List<GameObject> activeShips = new List<GameObject>();
        private bool isNightTime = false;
        private Coroutine weatherCoroutine;
        
        public enum Weather
        {
            Clear,
            Rain,
            Fog,
            Storm
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Calculate the time progression rate
            timeRate = 1f / (dayDurationMinutes * 60f);
            
            // Find references if not set
            if (sunLight == null)
            {
                sunLight = FindMainDirectionalLight();
            }
            
            if (gaslights.Count == 0 && autoFindGaslights)
            {
                FindAllGaslights();
            }
            
            // Initialize curve if needed
            if (daytimeBrightnessCurve == null || daytimeBrightnessCurve.keys.Length == 0)
            {
                SetupDefaultBrightnessCurve();
            }
            
            // Initialize ambient audio source if needed
            if (ambientAudioSource == null)
            {
                ambientAudioSource = gameObject.AddComponent<AudioSource>();
                ambientAudioSource.loop = true;
                ambientAudioSource.spatialBlend = 0f; // 2D sound
                ambientAudioSource.volume = 0.5f;
            }
        }
        
        private void Start()
        {
            // Initialize time of day
            currentTimeOfDay = startTimeOfDay;
            
            // Apply initial environment settings
            UpdateEnvironmentForTime(currentTimeOfDay);
            
            // Start with clear weather
            SetWeather(Weather.Clear);
            
            // Start weather cycle if enabled
            if (enableWeatherSystem)
            {
                weatherCoroutine = StartCoroutine(WeatherCycle());
            }
        }
        
        private void Update()
        {
            if (enableDayNightCycle)
            {
                // Update time of day
                currentTimeOfDay += timeRate * Time.deltaTime;
                
                // Wrap around to keep within 0-1 range
                if (currentTimeOfDay >= 1f)
                {
                    currentTimeOfDay -= 1f;
                }
                
                // Update environment based on time
                UpdateEnvironmentForTime(currentTimeOfDay);
            }
            
            // Update NPC system
            if (enableNPCSystem)
            {
                UpdateNPCSystem();
            }
            
            // Update port activity
            if (enablePortActivity)
            {
                UpdatePortActivity();
            }
        }
        
        private void OnDestroy()
        {
            if (weatherCoroutine != null)
            {
                StopCoroutine(weatherCoroutine);
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Sets the current weather type
        /// </summary>
        public void SetWeather(Weather weatherType)
        {
            currentWeather = weatherType;
            lastWeatherChangeTime = Time.time;
            
            // Update weather effects
            UpdateWeatherEffects(weatherType);
        }
        
        /// <summary>
        /// Adds a gaslight to the managed list
        /// </summary>
        public void AddGaslight(GameObject gaslight)
        {
            if (!gaslights.Contains(gaslight))
            {
                gaslights.Add(gaslight);
                UpdateGaslightIntensity(gaslight, currentTimeOfDay);
            }
        }
        
        /// <summary>
        /// Removes a gaslight from the managed list
        /// </summary>
        public void RemoveGaslight(GameObject gaslight)
        {
            gaslights.Remove(gaslight);
        }
        
        /// <summary>
        /// Gets the current time of day (0-1 range)
        /// </summary>
        public float GetTimeOfDay()
        {
            return currentTimeOfDay;
        }
        
        /// <summary>
        /// Sets the time of day manually (0-1 range)
        /// </summary>
        public void SetTimeOfDay(float time)
        {
            currentTimeOfDay = Mathf.Clamp01(time);
            UpdateEnvironmentForTime(currentTimeOfDay);
        }
        
        /// <summary>
        /// Gets the current weather as a string
        /// </summary>
        public string GetWeatherString()
        {
            return currentWeather.ToString();
        }
        
        /// <summary>
        /// Cycles to the next weather type
        /// </summary>
        public void CycleWeather()
        {
            int nextWeather = ((int)currentWeather + 1) % System.Enum.GetValues(typeof(Weather)).Length;
            SetWeather((Weather)nextWeather);
        }
        
        /// <summary>
        /// Spawns an NPC at a specific location
        /// </summary>
        public GameObject SpawnNPC(Vector3 position)
        {
            if (npcPrefabs == null || npcPrefabs.Length == 0)
                return null;
                
            // Select a random NPC prefab
            GameObject npcPrefab = npcPrefabs[Random.Range(0, npcPrefabs.Length)];
            if (npcPrefab == null)
                return null;
                
            // Instantiate NPC
            GameObject npc = Instantiate(npcPrefab, position, Quaternion.identity);
            
            // Add to active NPCs list
            activeNPCs.Add(npc);
            
            return npc;
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Updates environment based on time of day
        /// </summary>
        private void UpdateEnvironmentForTime(float time)
        {
            // Check for day/night transition
            bool wasNight = isNightTime;
            isNightTime = time < 0.25f || time > 0.75f;
            
            if (wasNight != isNightTime)
            {
                if (isNightTime)
                {
                    OnNightfall();
                }
                else
                {
                    OnDaybreak();
                }
            }
            
            // Update directional lights
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
            
            // Update skybox
            if (daySkybox != null && nightSkybox != null)
            {
                RenderSettings.skybox = isNightTime ? nightSkybox : daySkybox;
            }
            
            // Update gaslights
            foreach (GameObject gaslight in gaslights.ToList())
            {
                if (gaslight != null)
                {
                    UpdateGaslightIntensity(gaslight, time);
                }
            }
        }
        
        /// <summary>
        /// Updates a single gaslight's intensity based on time of day
        /// </summary>
        private void UpdateGaslightIntensity(GameObject gaslight, float time)
        {
            Light light = gaslight.GetComponent<Light>();
            if (light != null)
            {
                float intensity = isNightTime ? gaslightNightIntensity : gaslightDayIntensity;
                
                // Smooth transition during dawn/dusk
                float transitionRange = 0.1f;
                if (time > 0.25f - transitionRange && time < 0.25f + transitionRange)
                {
                    // Dawn transition
                    float t = (time - (0.25f - transitionRange)) / (transitionRange * 2f);
                    intensity = Mathf.Lerp(gaslightNightIntensity, gaslightDayIntensity, t);
                }
                else if (time > 0.75f - transitionRange && time < 0.75f + transitionRange)
                {
                    // Dusk transition
                    float t = (time - (0.75f - transitionRange)) / (transitionRange * 2f);
                    intensity = Mathf.Lerp(gaslightDayIntensity, gaslightNightIntensity, t);
                }
                
                light.intensity = intensity;
                
                // Update flicker component if present
                GaslightFlicker flicker = gaslight.GetComponent<GaslightFlicker>();
                if (flicker != null)
                {
                    flicker.SetBaseIntensity(intensity);
                }
            }
        }
        
        /// <summary>
        /// Updates weather effects based on current weather type
        /// </summary>
        private void UpdateWeatherEffects(Weather weather)
        {
            // Disable current weather effect
            if (activeWeatherEffect != null)
            {
                Destroy(activeWeatherEffect);
                activeWeatherEffect = null;
            }
            
            // Enable new weather effect
            if (weatherPrefabs != null && weatherPrefabs.Length > (int)weather)
            {
                GameObject weatherPrefab = weatherPrefabs[(int)weather];
                if (weatherPrefab != null)
                {
                    activeWeatherEffect = Instantiate(weatherPrefab, transform);
                }
            }
            
            // Update ambient audio
            if (ambientAudioSource != null && weatherAmbience != null && weatherAmbience.Length > (int)weather)
            {
                AudioClip clip = weatherAmbience[(int)weather];
                if (clip != null)
                {
                    ambientAudioSource.clip = clip;
                    ambientAudioSource.Play();
                }
            }
        }
        
        /// <summary>
        /// Manages the weather cycle
        /// </summary>
        private IEnumerator WeatherCycle()
        {
            while (true)
            {
                // Wait for minimum duration
                yield return new WaitForSeconds(minWeatherDuration * 60f);
                
                // Check for weather change
                if (Random.value < weatherChangeProbability)
                {
                    // Get possible weather types excluding current
                    List<Weather> possibleWeather = System.Enum.GetValues(typeof(Weather))
                        .Cast<Weather>()
                        .Where(w => w != currentWeather)
                        .ToList();
                    
                    // Pick random new weather
                    if (possibleWeather.Count > 0)
                    {
                        Weather newWeather = possibleWeather[Random.Range(0, possibleWeather.Count)];
                        SetWeather(newWeather);
                    }
                }
                
                // Add variation to wait time
                yield return new WaitForSeconds(Random.Range(0f, minWeatherDuration * 30f));
            }
        }
        
        /// <summary>
        /// Updates NPC system based on time of day and player position
        /// </summary>
        private void UpdateNPCSystem()
        {
            // Clean up destroyed NPCs
            activeNPCs.RemoveAll(npc => npc == null);
            
            // Remove excess NPCs
            while (activeNPCs.Count > maxActiveNPCs)
            {
                RemoveFurthestNPC();
            }
            
            // Spawn new NPCs if needed
            if (activeNPCs.Count < maxActiveNPCs)
            {
                TrySpawnNewNPC();
            }
            
            // Update existing NPCs
            foreach (GameObject npc in activeNPCs.ToList())
            {
                if (ShouldDespawnNPC(npc))
                {
                    DespawnNPC(npc);
                }
            }
        }

        /// <summary>
        /// Updates port activity based on time of day
        /// </summary>
        private void UpdatePortActivity()
        {
            // Clean up destroyed ships
            activeShips.RemoveAll(ship => ship == null);
            
            // Check for ship arrivals
            for (int i = 0; i < shipArrivalTimes.Length; i++)
            {
                if (IsTimeForShipActivity(shipArrivalTimes[i]))
                {
                    Transform availableDock = FindAvailableDockingPoint();
                    if (availableDock != null && i < shipPrefabs.Length)
                    {
                        SpawnShipAtDock(shipPrefabs[i], availableDock);
                    }
                }
            }
            
            // Check for departures
            foreach (GameObject ship in activeShips.ToList())
            {
                float departureTime = GetShipDepartureTime(ship);
                if (IsTimeForShipActivity(departureTime))
                {
                    DepartShip(ship);
                }
            }
        }

        /// <summary>
        /// Event handler for nightfall
        /// </summary>
        private void OnNightfall()
        {
            // Update lighting and environment for night
            RenderSettings.ambientIntensity = 0.3f;
            RenderSettings.fogDensity *= 1.5f;
            
            // Activate night-specific systems
            foreach (GameObject gaslight in gaslights)
            {
                if (gaslight != null)
                {
                    Light light = gaslight.GetComponent<Light>();
                    if (light != null)
                    {
                        light.enabled = true;
                    }
                }
            }
        }

        /// <summary>
        /// Event handler for daybreak
        /// </summary>
        private void OnDaybreak()
        {
            // Update lighting and environment for day
            RenderSettings.ambientIntensity = 1f;
            RenderSettings.fogDensity /= 1.5f;
            
            // Update day-specific systems
            foreach (GameObject gaslight in gaslights)
            {
                if (gaslight != null)
                {
                    Light light = gaslight.GetComponent<Light>();
                    if (light != null)
                    {
                        light.enabled = false;
                    }
                }
            }
        }

        #region Utility Methods

        /// <summary>
        /// Finds all gaslights in the scene
        /// </summary>
        private void FindAllGaslights()
        {
            GameObject[] lights = GameObject.FindGameObjectsWithTag("Gaslight");
            gaslights = new List<GameObject>(lights);
        }

        /// <summary>
        /// Finds the main directional light in the scene
        /// </summary>
        private Light FindMainDirectionalLight()
        {
            Light[] lights = FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    return light;
                }
            }
            return null;
        }

        /// <summary>
        /// Sets up default brightness curve for day/night cycle
        /// </summary>
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

        /// <summary>
        /// Checks if it's time for ship activity
        /// </summary>
        private bool IsTimeForShipActivity(float targetTime)
        {
            float timeDiff = Mathf.Abs(currentTimeOfDay - targetTime);
            return timeDiff < timeRate * 10f || timeDiff > 1f - timeRate * 10f;
        }

        /// <summary>
        /// Finds an available docking point
        /// </summary>
        private Transform FindAvailableDockingPoint()
        {
            if (dockingPoints == null || dockingPoints.Length == 0)
                return null;

            foreach (Transform dock in dockingPoints)
            {
                if (dock != null && !IsDockedShipAtPoint(dock))
                {
                    return dock;
                }
            }
            return null;
        }

        /// <summary>
        /// Checks if a ship is docked at a specific point
        /// </summary>
        private bool IsDockedShipAtPoint(Transform dockPoint)
        {
            foreach (GameObject ship in activeShips)
            {
                if (ship != null && Vector3.Distance(ship.transform.position, dockPoint.position) < 5f)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the departure time for a ship
        /// </summary>
        private float GetShipDepartureTime(GameObject ship)
        {
            // In a full implementation, this would be stored with the ship
            // For now, return a random time
            return Random.Range(0f, 1f);
        }

        /// <summary>
        /// Spawns a ship at a dock
        /// </summary>
        private void SpawnShipAtDock(GameObject shipPrefab, Transform dock)
        {
            if (shipPrefab == null || dock == null)
                return;

            GameObject ship = Instantiate(shipPrefab, dock.position, dock.rotation);
            activeShips.Add(ship);
        }

        /// <summary>
        /// Handles ship departure
        /// </summary>
        private void DepartShip(GameObject ship)
        {
            if (ship != null)
            {
                activeShips.Remove(ship);
                Destroy(ship);
            }
        }

        /// <summary>
        /// Removes the NPC furthest from the player
        /// </summary>
        private void RemoveFurthestNPC()
        {
            if (activeNPCs.Count == 0)
                return;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
                return;

            GameObject furthestNPC = null;
            float maxDistance = 0f;

            foreach (GameObject npc in activeNPCs)
            {
                if (npc != null)
                {
                    float distance = Vector3.Distance(player.transform.position, npc.transform.position);
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        furthestNPC = npc;
                    }
                }
            }

            if (furthestNPC != null)
            {
                DespawnNPC(furthestNPC);
            }
        }

        /// <summary>
        /// Tries to spawn a new NPC at a valid spawn point
        /// </summary>
        private void TrySpawnNewNPC()
        {
            if (npcSpawnPoints == null || npcSpawnPoints.Length == 0)
                return;

            Transform spawnPoint = npcSpawnPoints[Random.Range(0, npcSpawnPoints.Length)];
            if (spawnPoint != null)
            {
                SpawnNPC(spawnPoint.position);
            }
        }

        /// <summary>
        /// Checks if an NPC should be despawned
        /// </summary>
        private bool ShouldDespawnNPC(GameObject npc)
        {
            if (npc == null)
                return true;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
                return false;

            return Vector3.Distance(player.transform.position, npc.transform.position) > npcDespawnDistance;
        }

        /// <summary>
        /// Despawns an NPC
        /// </summary>
        private void DespawnNPC(GameObject npc)
        {
            if (npc != null)
            {
                activeNPCs.Remove(npc);
                Destroy(npc);
            }
        }

        #endregion
    }
}
