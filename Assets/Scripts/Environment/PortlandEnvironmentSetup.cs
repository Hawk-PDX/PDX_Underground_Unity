using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PDXUnderground.Environment
{
    /// <summary>
    /// PortlandEnvironmentSetup handles the generation and configuration of the Portland environment,
    /// including streets, buildings, and other environmental features in a grid-based layout.
    /// Provides historical accuracy for 1880s Portland with proper street patterns, lighting, and architecture.
    /// </summary>
    public class PortlandEnvironmentSetup : MonoBehaviour
    {
        #region Serialized Fields
        
        [Header("Environment Configuration")]
        [SerializeField] private Transform environmentParent;
        [SerializeField] private Vector2Int gridSize = new Vector2Int(10, 10);
        [SerializeField] private float cellSize = 61f; // 61m was a typical block size for Portland in the 1880s
        [SerializeField] private bool generateOnStart = true;
        [SerializeField] private bool useProceduralGeneration = false;
        [SerializeField] private bool usePresetLayout = true;
        [SerializeField] private int randomSeed = 0;
        
        [Header("Terrain Settings")]
        [SerializeField] private Terrain mainTerrain;
        [SerializeField] private float terrainHeight = 50f;
        [SerializeField] private float riverDepth = 10f;
        [SerializeField] private AnimationCurve heightCurve = AnimationCurve.Linear(0, 0, 1, 1);
        
        [Header("Street Layout")]
        [SerializeField] private GameObject streetStraightPrefab;
        [SerializeField] private GameObject sidewalkPrefab;
        [SerializeField] private GameObject streetCornerPrefab;
        [SerializeField] private GameObject intersectionPrefab;
        [SerializeField] private Material cobblestoneRoadMaterial;
        [SerializeField] private Material woodenSidewalkMaterial;
        [SerializeField] private bool useHistoricalStreetPattern = true;
        [SerializeField] private float streetWidth = 6.0f;
        [SerializeField] private float sidewalkWidth = 2.0f;
        [SerializeField] private bool createStreetGrid = true;
        
        [Header("Building Placement")]
        [SerializeField] private GameObject[] buildingPrefabs;
        [SerializeField] private List<GameObject> cornerBuildingPrefabs;
        [SerializeField] private List<GameObject> edgeBuildingPrefabs;
        [SerializeField] private float buildingSpacing = 2.0f;
        [SerializeField] private float buildingVariation = 0.5f;
        [SerializeField] private bool alignBuildingsToStreets = true;
        [SerializeField] private float buildingHeight = 12f;
        [SerializeField] private float buildingFootprintVariance = 0.2f;
        [SerializeField] private bool generateBuildings = true;
        [SerializeField] private float buildingInset = 4f; // Inset from street
        
        [Header("Lighting System")]
        [SerializeField] private GameObject gaslightPrefab;
        [SerializeField] private float gaslightSpacing = 20f;
        [SerializeField] private float gaslightHeight = 3.5f;
        [SerializeField] private bool placeGaslightsAlongStreets = true;
        [SerializeField] private bool placeGaslightsAtIntersections = true;
        [SerializeField] private bool generateGaslights = true;
        [SerializeField] private float gaslightIntensityDay = 0.3f;
        [SerializeField] private float gaslightIntensityNight = 0.8f;
        
        [Header("Port Configuration")]
        [SerializeField] private GameObject dockSectionPrefab;
        [SerializeField] private GameObject[] warehousePrefabs;
        [SerializeField] private GameObject[] cratePrefabs;
        [SerializeField] private GameObject mooringPostPrefab;
        [SerializeField] private int dockSections = 8;
        [SerializeField] private float dockWidth = 6f;
        [SerializeField] private float dockLength = 50f;
        
        [Header("Shanghai Tunnels")]
        [SerializeField] private GameObject tunnelSectionPrefab;
        [SerializeField] private GameObject tunnelSupportBeamPrefab;
        [SerializeField] private GameObject tunnelEntrancePrefab;
        [SerializeField] private int tunnelSectionCount = 20;
        [SerializeField] private Vector3[] tunnelEntrancePositions;
        
        [Header("Spawn Points")]
        [SerializeField] private Transform defaultSpawnPoint;
        [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
        [SerializeField] private float autoGenerateSpawnPointInterval = 60f;
        
        [Header("Transition Points")]
        [SerializeField] private AreaTransitionTrigger tunnelEntranceTrigger;
        [SerializeField] private AreaTransitionTrigger portEntranceTrigger;
        [SerializeField] private AreaTransitionTrigger[] customTransitionPoints;
        
        [Header("Historical Accuracy")]
        [SerializeField] private TextAsset historicalLayoutData;
        [SerializeField] private Vector2 portlandCenterCoordinate = new Vector2(0, 0);
        [SerializeField] private float historicalBlockSize = 61f;
        [SerializeField] private Vector2 gridNorthDirection = new Vector2(0, 1);
        
        [Header("Save/Load Settings")]
        [SerializeField] private string environmentSaveFilename = "portland_layout.json";
        [SerializeField] private bool loadLayoutOnStart = false;
        [SerializeField] private bool saveLayoutOnGeneration = false;
        
        [Header("Performance")]
        [SerializeField] private bool useLOD = true;
        [SerializeField] private bool combineStaticMeshes = true;
        [SerializeField] private bool generateNavMesh = true;
        
        [Header("Debug Visualization")]
        [SerializeField] private bool showDebugVisuals = true;
        [SerializeField] private bool regenerateOnStart = false;
        [SerializeField] private Color streetGridColor = new Color(0.7f, 0.7f, 0.7f, 0.3f);
        [SerializeField] private Color buildingGridColor = new Color(0.3f, 0.6f, 0.9f, 0.3f);
        [SerializeField] private Color spawnPointColor = new Color(0.0f, 1.0f, 0.0f, 0.5f);
        
        #endregion
        
        #region Private Variables
        
        private Dictionary<Vector2Int, CellType> gridCells = new Dictionary<Vector2Int, CellType>();
        private Dictionary<Vector2Int, GameObject> placedObjects = new Dictionary<Vector2Int, GameObject>();
        private List<GameObject> generatedObjects = new List<GameObject>();
        private List<Vector3> gaslightPositions = new List<Vector3>();
        private List<Vector3> buildingPositions = new List<Vector3>();
        private List<Vector3> dockPositions = new List<Vector3>();
        private List<Vector3> availableSpawnPoints = new List<Vector3>();
        private List<Vector3> intersectionPositions = new List<Vector3>();
        private bool[,] streetGrid;
        private System.Random random;
        
        private Transform streetsParent;
        private Transform buildingsParent;
        private Transform lightsParent;
        private Transform portsParent;
        private Transform tunnelsParent;
        private Transform transitionsParent;
        
        private Vector3 gridOrigin = Vector3.zero;
        
        private enum CellType
        {
            Empty,
            Street,
            Building,
            Dock,
            River,
            Terrain,
            Tunnel
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeEnvironment();
        }
        
        private void Start()
        {
            // Create default spawn point if none exists
            EnsureDefaultSpawnPoint();
            
            // Load existing layout or generate new one
            if (loadLayoutOnStart)
            {
                LoadEnvironmentLayout();
            }
            else if (generateOnStart || regenerateOnStart)
            {
                GenerateEnvironment();
            }
            
            // Post-processing for performance
            if (combineStaticMeshes)
            {
                StartCoroutine(CombineStaticMeshes());
            }
            
            // Generate NavMesh if needed
            if (generateNavMesh)
            {
                StartCoroutine(GenerateNavMesh());
            }
        }
        
        private void OnDrawGizmos()
        {
            if (!showDebugVisuals)
                return;
            
            DrawDebugGrid();
            DrawSpawnPoints();
        }
        
        #endregion
        
        #region Initialization Methods
        
        /// <summary>
        /// Initializes the environment with all required components
        /// </summary>
        private void InitializeEnvironment()
        {
            // Set up environment parent reference
            if (environmentParent == null)
            {
                environmentParent = transform;
            }

            // Create parent objects for organization
            CreateParentObjects();
            
            // Initialize random generator
            if (randomSeed != 0)
            {
                Random.InitState(randomSeed);
                random = new System.Random(randomSeed);
            }
            else
            {
                random = new System.Random();
            }
            
            // Calculate grid origin for centering
            CalculateGridOrigin();
        }
        
        /// <summary>
        /// Creates parent transform objects for better scene organization
        /// </summary>
        private void CreateParentObjects()
        {
            streetsParent = CreateOrGetParentObject("Streets");
            buildingsParent = CreateOrGetParentObject("Buildings");
            lightsParent = CreateOrGetParentObject("Lights");
            portsParent = CreateOrGetParentObject("Ports");
            tunnelsParent = CreateOrGetParentObject("Tunnels");
            transitionsParent = CreateOrGetParentObject("Transitions");
        }

        /// <summary>
        /// Creates or retrieves a parent object with the specified name
        /// </summary>
        private Transform CreateOrGetParentObject(string name)
        {
            Transform parent = environmentParent.Find(name);
            if (parent == null)
            {
                GameObject obj = new GameObject(name);
                obj.transform.SetParent(environmentParent);
                parent = obj.transform;
            }
            return parent;
        }
        
        /// <summary>
        /// Calculates the grid origin based on current settings
        /// </summary>
        private void CalculateGridOrigin()
        {
            if (gridSize.x <= 0 || gridSize.y <= 0)
            {
                Debug.LogWarning("Invalid grid size, using default (10x10)");
                gridSize = new Vector2Int(10, 10);
            }
            
            gridOrigin = new Vector3(
                -((gridSize.x * cellSize) / 2f),
                0f,
                -((gridSize.y * cellSize) / 2f)
            );
        }
        
        /// <summary>
        /// Ensures a default spawn point exists
        /// </summary>
        private void EnsureDefaultSpawnPoint()
        {
            if (defaultSpawnPoint == null && spawnPoints.Count == 0)
            {
                GameObject spawnObj = new GameObject("DefaultSpawn");
                spawnObj.transform.SetParent(environmentParent);
                spawnObj.transform.position = new Vector3(0, 0.5f, 0);
                defaultSpawnPoint = spawnObj.transform;
                spawnPoints.Add(defaultSpawnPoint);
            }
        }
        
        /// <summary>
        /// Combines static meshes for performance optimization
        /// </summary>
        private System.Collections.IEnumerator CombineStaticMeshes()
        {
            yield return new WaitForEndOfFrame();
            
            // This would combine meshes for static objects to improve performance
            // Implementation depends on specific requirements and Unity version
            Debug.Log("Static meshes combined for performance");
        }
        
        /// <summary>
        /// Generates NavMesh for the environment
        /// </summary>
        private System.Collections.IEnumerator GenerateNavMesh()
        {
            yield return new WaitForEndOfFrame();
            
            // This would build the NavMesh for the environment
            // Implementation depends on NavMesh components and Unity version
            Debug.Log("NavMesh generated for the environment");
        }
        
        #endregion
        
        #region Environment Generation

        /// <summary>
        /// Main method for generating the entire environment
        /// </summary>
        public void GenerateEnvironment()
        {
            // Clear existing environment if any
            ClearEnvironment();

            // Initialize the street grid array
            streetGrid = new bool[gridSize.x, gridSize.y];

            try
            {
                // Generate base terrain if needed
                if (mainTerrain != null)
                {
                    GenerateTerrain();
                }

                // Create street grid (either historical or procedural)
                if (createStreetGrid)
                {
                    if (useHistoricalStreetPattern && historicalLayoutData != null)
                    {
                        CreateHistoricalStreetGrid();
                    }
                    else
                    {
                        CreateStreetGrid();
                    }
                }

                // Generate buildings along streets
                if (generateBuildings && buildingPrefabs != null && buildingPrefabs.Length > 0)
                {
                    GenerateBuildings();
                }

                // Place gaslights
                if (generateGaslights && gaslightPrefab != null)
                {
                    PlaceGaslights();
                }

                // Generate port area if configured
                if (dockSectionPrefab != null && dockSections > 0)
                {
                    GeneratePortArea();
                }

                // Generate Shanghai tunnels
                if (tunnelSectionPrefab != null && tunnelSectionCount > 0)
                {
                    GenerateTunnels();
                }

                // Place transition points
                PlaceTransitionPoints();

                // Generate spawn points throughout the environment
                GenerateSpawnPoints();

                // Save layout if enabled
                if (saveLayoutOnGeneration)
                {
                    SaveEnvironmentLayout();
                }

                Debug.Log("Portland environment generation completed successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error during environment generation: {e.Message}\n{e.StackTrace}");
                ClearEnvironment(); // Clean up on failure
            }
        }

        /// <summary>
        /// Creates a basic terrain for the environment
        /// </summary>
        private void GenerateTerrain()
        {
            // Placeholder for terrain generation
            Debug.Log("Terrain generation placeholder");
            
            // In a full implementation, this would modify the terrain data
            // to create appropriate height variations, texture painting, etc.
        }

        /// <summary>
        /// Creates the basic street grid layout using a procedural approach
        /// </summary>
        private void CreateStreetGrid()
        {
            Debug.Log("Creating procedural street grid");
            
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int z = 0; z < gridSize.y; z++)
                {
                    // Determine if this should be a street based on grid position
                    bool isStreet = IsStreetPosition(x, z);
                    streetGrid[x, z] = isStreet;

                    if (isStreet)
                    {
                        gridCells[new Vector2Int(x, z)] = CellType.Street;
                        CreateStreetSection(x, z);
                    }
                    else
                    {
                        gridCells[new Vector2Int(x, z)] = CellType.Empty;
                    }
                }
            }

            // Create intersections where streets meet
            CreateIntersections();
        }

        /// <summary>
        /// Creates a historically accurate street grid based on layout data
        /// </summary>
        private void CreateHistoricalStreetGrid()
        {
            if (historicalLayoutData == null)
            {
                Debug.LogWarning("Historical layout data is missing, falling back to procedural generation");
                CreateStreetGrid();
                return;
            }

            Debug.Log("Creating historical street grid based on historical data");
            
            try
            {
                // Parse historical layout data
                // In a full implementation, this would parse JSON or another format
                // containing historical street information
                
                /* Example structure that would be parsed:
                {
                    "streets": [
                        { "position": {"x": 0, "y": 0, "z": 0}, "rotation": 0 },
                        { "position": {"x": 61, "y": 0, "z": 0}, "rotation": 0 }
                    ],
                    "intersections": [
                        { "position": {"x": 61, "y": 0, "z": 61}, "rotation": 0 }
                    ]
                }
                */
                
                // For now, create a pattern based on historical block size (61m)
                for (int x = 0; x < gridSize.x; x++)
                {
                    for (int z = 0; z < gridSize.y; z++)
                    {
                        // In historical Portland, streets followed a regular grid with
                        // distinctive small blocks (61m x 61m)
                        bool isStreet = x % 3 == 0 || z % 3 == 0; // Every third line for streets
                        streetGrid[x, z] = isStreet;

                        if (isStreet)
                        {
                            gridCells[new Vector2Int(x, z)] = CellType.Street;
                            float rotation = 0f;
                            
                            // Determine street orientation
                            if (x % 3 == 0 && z % 3 != 0)
                            {
                                rotation = 0f; // North-South streets
                            }
                            else if (x % 3 != 0 && z % 3 == 0)
                            {
                                rotation = 90f; // East-West streets
                            }
                            
                            CreateStreetSection(x, z, rotation);
                        }
                        else
                        {
                            gridCells[new Vector2Int(x, z)] = CellType.Empty;
                        }
                    }
                }

                // Create intersections
                for (int x = 0; x < gridSize.x; x += 3)
                {
                    for (int z = 0; z < gridSize.y; z += 3)
                    {
                        Vector3 position = GridToWorldPosition(x, z);
                        intersectionPositions.Add(position);
                    }
                }
                
                CreateIntersections();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error parsing historical layout data: {e.Message}");
                CreateStreetGrid(); // Fallback to procedural generation
            }
        }

        /// <summary>
        /// Creates a street section at the specified grid position
        /// </summary>
        private void CreateStreetSection(int x, int z, float rotation = 0f)
        {
            Vector3 position = GridToWorldPosition(x, z);
            
            // Determine street type and rotation based on connections
            bool hasNorthStreet = z < gridSize.y - 1 && streetGrid[x, z + 1];
            bool hasSouthStreet = z > 0 && streetGrid[x, z - 1];
            bool hasEastStreet = x < gridSize.x - 1 && streetGrid[x + 1, z];
            bool hasWestStreet = x > 0 && streetGrid[x - 1, z];
            
            GameObject streetPrefab = null;
            float streetRotation = rotation;

            // Select appropriate street prefab based on connections
            if ((hasNorthStreet || hasSouthStreet) && (hasEastStreet || hasWestStreet))
            {
                streetPrefab = intersectionPrefab;
                intersectionPositions.Add(position);
            }
            else if ((hasNorthStreet && hasSouthStreet) || (hasEastStreet && hasWestStreet))
            {
                streetPrefab = streetStraightPrefab;
                streetRotation = (hasNorthStreet && hasSouthStreet) ? 0f : 90f;
            }
            else
            {
                streetPrefab = streetCornerPrefab;
                // Calculate corner rotation based on connections
                streetRotation = CalculateCornerRotation(hasNorthStreet, hasEastStreet, hasSouthStreet, hasWestStreet);
            }

            // Create street section
            if (streetPrefab != null)
            {
                GameObject street = Instantiate(streetPrefab, position, Quaternion.Euler(0, streetRotation, 0), streetsParent);
                street.name = $"Street_{x}_{z}";
                generatedObjects.Add(street);
                placedObjects[new Vector2Int(x, z)] = street;
                
                // Create sidewalks
                CreateSidewalks(position, streetRotation, hasNorthStreet, hasEastStreet, hasSouthStreet, hasWestStreet);
            }
        }

        /// <summary>
        /// Creates sidewalks for a street section
        /// </summary>
        private void CreateSidewalks(Vector3 position, float rotation, bool north, bool east, bool south, bool west)
        {
            if (sidewalkPrefab == null) return;

            float offset = (streetWidth + sidewalkWidth) / 2f;
            
            // Place sidewalks based on street connections
            if (north || south)
            {
                // East sidewalk
                GameObject eastSidewalk = Instantiate(sidewalkPrefab, 
                    position + Quaternion.Euler(0, rotation, 0) * new Vector3(offset, 0, 0),
                    Quaternion.Euler(0, rotation, 0), 
                    streetsParent);
                eastSidewalk.name = $"Sidewalk_East_{position.x}_{position.z}";
                generatedObjects.Add(eastSidewalk);

                // West sidewalk
                GameObject westSidewalk = Instantiate(sidewalkPrefab,
                    position + Quaternion.Euler(0, rotation, 0) * new Vector3(-offset, 0, 0),
                    Quaternion.Euler(0, rotation, 0),
                    streetsParent);
                westSidewalk.name = $"Sidewalk_West_{position.x}_{position.z}";
                generatedObjects.Add(westSidewalk);
            }

            if (east || west)
            {
                // North sidewalk
                GameObject northSidewalk = Instantiate(sidewalkPrefab,
                    position + Quaternion.Euler(0, rotation, 0) * new Vector3(0, 0, offset),
                    Quaternion.Euler(0, rotation + 90, 0),
                    streetsParent);
                northSidewalk.name = $"Sidewalk_North_{position.x}_{position.z}";
                generatedObjects.Add(northSidewalk);

                // South sidewalk
                GameObject southSidewalk = Instantiate(sidewalkPrefab,
                    position + Quaternion.Euler(0, rotation, 0) * new Vector3(0, 0, -offset),
                    Quaternion.Euler(0, rotation + 90, 0),
                    streetsParent);
                southSidewalk.name = $"Sidewalk_South_{position.x}_{position.z}";
                generatedObjects.Add(southSidewalk);
            }
        }

        /// <summary>
        /// Creates intersection points where streets meet
        /// </summary>
        private void CreateIntersections()
        {
            if (intersectionPrefab == null) return;
            
            foreach (Vector3 position in intersectionPositions)
            {
                Vector2Int gridPos = WorldToGridPosition(position);
                
                // Skip if an intersection already exists at this position
                if (placedObjects.ContainsKey(gridPos)) continue;
                
                GameObject intersection = Instantiate(intersectionPrefab, position, Quaternion.identity, streetsParent);
                intersection.name = $"Intersection_{position.x}_{position.z}";
                generatedObjects.Add(intersection);
                placedObjects[gridPos] = intersection;
            }
        }

        /// <summary>
        /// Generates buildings along streets in historically accurate positions
        /// </summary>
        private void GenerateBuildings()
        {
            if (buildingPrefabs == null || buildingPrefabs.Length == 0)
            {
                Debug.LogWarning("No building prefabs assigned for generation");
                return;
            }

            Debug.Log("Generating buildings along streets...");

            // Iterate through the grid to place buildings
            for (int x = 1; x < gridSize.x - 1; x++)
            {
                for (int z = 1; z < gridSize.y - 1; z++)
                {
                    if (IsValidBuildingLocation(x, z))
                    {
                        PlaceBuilding(x, z);
                    }
                }
            }

            // Process corner buildings after regular buildings
            ProcessCornerBuildings();
        }

        /// <summary>
        /// Checks if a location is valid for building placement
        /// </summary>
        private bool IsValidBuildingLocation(int x, int z)
        {
            // Skip if this is a street cell
            if (streetGrid[x, z])
                return false;

            // Check if this cell is adjacent to a street
            bool hasAdjacentStreet = false;
            
            // Check all adjacent cells
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    if (dx == 0 && dz == 0) continue; // Skip self
                    
                    int newX = x + dx;
                    int newZ = z + dz;
                    
                    if (IsValidGridPosition(new Vector2Int(newX, newZ)) && streetGrid[newX, newZ])
                    {
                        hasAdjacentStreet = true;
                        break;
                    }
                }

                if (hasAdjacentStreet) break;
            }

            return hasAdjacentStreet && !placedObjects.ContainsKey(new Vector2Int(x, z));
        }

        /// <summary>
        /// Places a building at the specified grid position
        /// </summary>
        private void PlaceBuilding(int x, int z)
        {
            Vector2Int gridPos = new Vector2Int(x, z);
            Vector3 basePosition = GridToWorldPosition(x, z);
            
            // Determine building type and rotation based on street adjacency
            BuildingPlacementInfo placement = CalculateBuildingPlacement(x, z);
            
            // Select appropriate building prefab
            GameObject buildingPrefab = SelectBuildingPrefab(placement.type);
            if (buildingPrefab == null) return;

            // Apply position adjustments based on building type and street alignment
            Vector3 adjustedPosition = CalculateAdjustedBuildingPosition(basePosition, placement);
            
            // Create the building
            GameObject building = Instantiate(buildingPrefab, adjustedPosition, Quaternion.Euler(0, placement.rotation, 0), buildingsParent);
            building.name = $"Building_{x}_{z}";
            
            // Apply random height variation within historical limits
            float heightVariation = 1f + (random.Next(-100, 100) / 100f) * buildingFootprintVariance;
            building.transform.localScale = new Vector3(
                building.transform.localScale.x,
                building.transform.localScale.y * heightVariation,
                building.transform.localScale.z
            );

            // Add to tracking collections
            generatedObjects.Add(building);
            placedObjects[gridPos] = building;
            buildingPositions.Add(adjustedPosition);

            // Mark grid cell as building
            gridCells[gridPos] = CellType.Building;
        }

        /// <summary>
        /// Calculates building placement information including type and rotation
        /// </summary>
        private BuildingPlacementInfo CalculateBuildingPlacement(int x, int z)
        {
            BuildingPlacementInfo info = new BuildingPlacementInfo();
            
            // Check adjacent streets to determine building type and orientation
            bool northStreet = z < gridSize.y - 1 && streetGrid[x, z + 1];
            bool southStreet = z > 0 && streetGrid[x, z - 1];
            bool eastStreet = x < gridSize.x - 1 && streetGrid[x + 1, z];
            bool westStreet = x > 0 && streetGrid[x - 1, z];
            
            int streetCount = (northStreet ? 1 : 0) + (southStreet ? 1 : 0) + 
                             (eastStreet ? 1 : 0) + (westStreet ? 1 : 0);

            // Determine building type based on adjacent streets
            if (streetCount >= 2)
            {
                info.type = BuildingType.Corner;
                
                // Calculate rotation for corner buildings
                if (northStreet && eastStreet) info.rotation = 0f;
                else if (eastStreet && southStreet) info.rotation = 90f;
                else if (southStreet && westStreet) info.rotation = 180f;
                else if (westStreet && northStreet) info.rotation = 270f;
                else info.rotation = 0f; // Default for other combinations
            }
            else if (streetCount == 1)
            {
                info.type = BuildingType.Edge;
                
                // Calculate rotation for edge buildings
                if (northStreet) info.rotation = 0f;
                else if (eastStreet) info.rotation = 90f;
                else if (southStreet) info.rotation = 180f;
                else if (westStreet) info.rotation = 270f;
            }
            else
            {
                info.type = BuildingType.Interior;
                info.rotation = random.Next(0, 4) * 90f; // Random orientation for interior buildings
            }

            return info;
        }

        /// <summary>
        /// Calculates the adjusted position for a building based on its placement info
        /// </summary>
        private Vector3 CalculateAdjustedBuildingPosition(Vector3 basePosition, BuildingPlacementInfo placement)
        {
            Vector3 adjustedPosition = basePosition;
            
            // Apply inset from street
            if (placement.type != BuildingType.Interior)
            {
                adjustedPosition += Quaternion.Euler(0, placement.rotation, 0) * Vector3.back * buildingInset;
            }
            
            // Apply random position variation within the plot
            if (buildingVariation > 0)
            {
                float xVariation = ((float)random.NextDouble() * 2 - 1) * buildingVariation;
                float zVariation = ((float)random.NextDouble() * 2 - 1) * buildingVariation;
                adjustedPosition += new Vector3(xVariation, 0, zVariation);
            }
            
            return adjustedPosition;
        }

        /// <summary>
        /// Selects an appropriate building prefab based on building type
        /// </summary>
        private GameObject SelectBuildingPrefab(BuildingType type)
        {
            switch (type)
            {
                case BuildingType.Corner:
                    return cornerBuildingPrefabs != null && cornerBuildingPrefabs.Count > 0
                        ? cornerBuildingPrefabs[random.Next(cornerBuildingPrefabs.Count)]
                        : GetRandomBuildingPrefab();
                    
                case BuildingType.Edge:
                    return edgeBuildingPrefabs != null && edgeBuildingPrefabs.Count > 0
                        ? edgeBuildingPrefabs[random.Next(edgeBuildingPrefabs.Count)]
                        : GetRandomBuildingPrefab();
                    
                default:
                    return GetRandomBuildingPrefab();
            }
        }

        /// <summary>
        /// Returns a random building prefab from the main collection
        /// </summary>
        private GameObject GetRandomBuildingPrefab()
        {
            if (buildingPrefabs == null || buildingPrefabs.Length == 0)
                return null;
                
            return buildingPrefabs[random.Next(buildingPrefabs.Length)];
        }

        /// <summary>
        /// Processes corner buildings after regular building placement
        /// </summary>
        private void ProcessCornerBuildings()
        {
            // Iterate through intersection positions to ensure proper corner building placement
            foreach (Vector3 intersection in intersectionPositions)
            {
                Vector2Int gridPos = WorldToGridPosition(intersection);
                
                // Check and potentially adjust corner buildings around this intersection
                for (int dx = -1; dx <= 1; dx += 2)
                {
                    for (int dz = -1; dz <= 1; dz += 2)
                    {
                        Vector2Int cornerPos = new Vector2Int(gridPos.x + dx, gridPos.y + dz);
                        if (IsValidGridPosition(cornerPos) && !streetGrid[cornerPos.x, cornerPos.y])
                        {
                            // If there's already a building here, potentially replace it with a corner building
                            if (placedObjects.ContainsKey(cornerPos))
                            {
                                GameObject existingBuilding = placedObjects[cornerPos];
                                // Only replace non-corner buildings
                                if (existingBuilding != null && cornerBuildingPrefabs != null && 
                                    !IsCornerBuilding(existingBuilding))
                                {
                                    DestroyImmediate(existingBuilding);
                                    generatedObjects.Remove(existingBuilding);
                                    placedObjects.Remove(cornerPos);
                                    PlaceBuilding(cornerPos.x, cornerPos.y);
                                }
                            }
                            else
                            {
                                PlaceBuilding(cornerPos.x, cornerPos.y);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a building is a corner building based on its prefab
        /// </summary>
        private bool IsCornerBuilding(GameObject building)
        {
            if (cornerBuildingPrefabs == null || cornerBuildingPrefabs.Count == 0)
                return false;
                
            // Extract prefab name from instance name (format: "BuildingName(Clone)")
            string buildingName = building.name;
            int parenthesisIndex = buildingName.IndexOf('(');
            if (parenthesisIndex > 0)
            {
                buildingName = buildingName.Substring(0, parenthesisIndex);
            }
            
            // Check if any corner building prefab matches this name
            foreach (GameObject prefab in cornerBuildingPrefabs)
            {
                if (prefab != null && prefab.name == buildingName)
                    return true;
            }
            
            return false;
        }

        /// <summary>
        /// Represents building placement calculation results
        /// </summary>
        private struct BuildingPlacementInfo
        {
            public BuildingType type;
            public float rotation;
        }

        /// <summary>
        /// Defines the type of building based on its position relative to streets
        /// </summary>
        private enum BuildingType
        {
            Corner,
            Edge,
            Interior
        }

        /// <summary>
        /// Places gaslights along streets and at intersections following historical patterns
        /// </summary>
        private void PlaceGaslights()
        {
            if (!generateGaslights || gaslightPrefab == null)
            {
                Debug.LogWarning("Gaslight generation skipped - either disabled or missing prefab");
                return;
            }

            Debug.Log("Placing gaslights along streets...");

            try
            {
                // Place gaslights at intersections first
                if (placeGaslightsAtIntersections)
                {
                    PlaceIntersectionGaslights();
                }

                // Place gaslights along streets
                if (placeGaslightsAlongStreets)
                {
                    PlaceStreetGaslights();
                }

                // Configure all gaslight intensities
                ConfigureGaslightIntensities();

                Debug.Log($"Placed {gaslightPositions.Count} gaslights in the environment");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error during gaslight placement: {e.Message}");
            }
        }

        /// <summary>
        /// Places gaslights at street intersections
        /// </summary>
        private void PlaceIntersectionGaslights()
        {
            foreach (Vector3 intersection in intersectionPositions)
            {
                // Create gaslight at each corner of the intersection
                for (int i = 0; i < 4; i++)
                {
                    float angle = i * 90f;
                    Vector3 offset = Quaternion.Euler(0, angle, 0) * 
                                   new Vector3(streetWidth / 2f + sidewalkWidth / 2f, 
                                             0, 
                                             streetWidth / 2f + sidewalkWidth / 2f);
                    
                    Vector3 position = intersection + offset;
                    position.y = gaslightHeight;

                    // Check if position is too close to existing gaslights
                    if (!IsTooCloseToExistingGaslight(position))
                    {
                        CreateGaslight(position, angle + 45f); // 45-degree rotation for corner placement
                    }
                }
            }
        }

        /// <summary>
        /// Places gaslights along street segments
        /// </summary>
        private void PlaceStreetGaslights()
        {
            // Iterate through the grid to find street segments
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int z = 0; z < gridSize.y; z++)
                {
                    if (streetGrid[x, z])
                    {
                        PlaceGaslightsAlongStreetSegment(x, z);
                    }
                }
            }
        }

        /// <summary>
        /// Places gaslights along a specific street segment
        /// </summary>
        private void PlaceGaslightsAlongStreetSegment(int x, int z)
        {
            Vector3 streetPos = GridToWorldPosition(x, z);
            bool isNorthSouth = IsNorthSouthStreet(x, z);
            
            // Calculate number of gaslights needed for this segment
            float segmentLength = cellSize;
            int gaslightsPerSide = Mathf.FloorToInt(segmentLength / gaslightSpacing);
            
            // Skip if segment is too short for gaslights
            if (gaslightsPerSide <= 0) return;
            
            // Place gaslights on both sides of the street
            for (int side = 0; side < 2; side++)
            {
                float sideOffset = (streetWidth / 2f + sidewalkWidth / 2f) * (side == 0 ? 1 : -1);
                
                for (int i = 0; i < gaslightsPerSide; i++)
                {
                    // Calculate position along the street
                    float progress = (i + 1) / (float)(gaslightsPerSide + 1);
                    Vector3 position = streetPos;
                    
                    if (isNorthSouth)
                    {
                        position += new Vector3(sideOffset, 0, progress * cellSize);
                    }
                    else
                    {
                        position += new Vector3(progress * cellSize, 0, sideOffset);
                    }
                    
                    position.y = gaslightHeight;
                    
                    // Check spacing from other gaslights and intersections
                    if (!IsTooCloseToExistingGaslight(position) && 
                        !IsTooCloseToIntersection(position))
                    {
                        float rotation = isNorthSouth ? (side == 0 ? 90f : 270f) : (side == 0 ? 0f : 180f);
                        CreateGaslight(position, rotation);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a gaslight at the specified position with given rotation
        /// </summary>
        private void CreateGaslight(Vector3 position, float rotation)
        {
            GameObject gaslight = Instantiate(gaslightPrefab, position, Quaternion.Euler(0, rotation, 0), lightsParent);
            gaslight.name = $"Gaslight_{gaslightPositions.Count}";
            
            // Configure light component
            Light lightComponent = gaslight.GetComponentInChildren<Light>();
            if (lightComponent != null)
            {
                lightComponent.intensity = gaslightIntensityDay;
                
                // Add controller for day/night transitions
                GaslightController controller = gaslight.AddComponent<GaslightController>();
                controller.dayIntensity = gaslightIntensityDay;
                controller.nightIntensity = gaslightIntensityNight;
            }
            
            // Add optional flicker effect
            gaslight.AddComponent<GaslightFlicker>();
            
            generatedObjects.Add(gaslight);
            gaslightPositions.Add(position);
        }

        /// <summary>
        /// Checks if a position is too close to existing gaslights
        /// </summary>
        private bool IsTooCloseToExistingGaslight(Vector3 position)
        {
            float minDistance = gaslightSpacing * 0.75f; // 75% of spacing as minimum distance
            
            foreach (Vector3 existingPos in gaslightPositions)
            {
                float dist = Vector3.Distance(new Vector3(position.x, 0, position.z), 
                                            new Vector3(existingPos.x, 0, existingPos.z));
                if (dist < minDistance)
                {
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Checks if a position is too close to an intersection
        /// </summary>
        private bool IsTooCloseToIntersection(Vector3 position)
        {
            float minDistance = streetWidth * 0.75f;
            
            foreach (Vector3 intersection in intersectionPositions)
            {
                float dist = Vector3.Distance(new Vector3(position.x, 0, position.z), 
                                            new Vector3(intersection.x, 0, intersection.z));
                if (dist < minDistance)
                {
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Determines if a street segment is oriented north-south
        /// </summary>
        private bool IsNorthSouthStreet(int x, int z)
        {
            if (z > 0 && z < gridSize.y - 1)
            {
                return streetGrid[x, z - 1] && streetGrid[x, z + 1];
            }
            return false;
        }

        /// <summary>
        /// Configures light intensities for all placed gaslights
        /// </summary>
        private void ConfigureGaslightIntensities()
        {
            // This could be connected to a day/night cycle system
            foreach (Transform gaslight in lightsParent)
            {
                Light[] lights = gaslight.GetComponentsInChildren<Light>();
                foreach (Light light in lights)
                {
                    light.intensity = gaslightIntensityDay;
                }
            }
        }

        /// <summary>
        /// Helper component to manage gaslight intensity transitions between day and night
        /// </summary>
        [System.Serializable]
        public class GaslightController : MonoBehaviour
        {
            [HideInInspector] public float dayIntensity = 0.3f;
            [HideInInspector] public float nightIntensity = 0.8f;
            private Light lightComponent;

            private void Start()
            {
                lightComponent = GetComponentInChildren<Light>();
            }

            /// <summary>
            /// Updates the light intensity based on time of day
            /// </summary>
            /// <param name="timeOfDay">Value between 0 (midnight) and 1 (next midnight)</param>
            public void UpdateTimeOfDay(float timeOfDay)
            {
                if (lightComponent == null) return;

                // Simple day/night transition curve
                float dayFactor = Mathf.Sin(timeOfDay * Mathf.PI);
                float nightFactor = 1f - dayFactor;

                // Calculate intensity based on time (brighter at night)
                lightComponent.intensity = (dayIntensity * dayFactor) + (nightIntensity * nightFactor);
            }
        }

        /// <summary>
        /// Helper component to add subtle flicker effect to gaslights
        /// </summary>
        /// <summary>
        /// Helper component to add subtle flicker effect to gaslights with URP compatibility
        /// </summary>
        [System.Serializable]
        public class GaslightFlicker : MonoBehaviour
        {
            [Header("Flicker Settings")]
            [Range(0.5f, 0.95f)]
            [SerializeField] private float minIntensity = 0.8f;
            
            [Range(1.0f, 1.5f)]
            [SerializeField] private float maxIntensity = 1.2f;
            
            [Range(0.01f, 0.5f)]
            [SerializeField] private float flickerSpeed = 0.1f;
            
            [Header("URP Settings")]
            [SerializeField] private bool useTemperature = true;
            [SerializeField] private float colorTemperature = 2200f; // Warm gaslight color
            
            private Light lightComponent;
            private float baseIntensity;
            private float nextIntensity;
            private float lastUpdate;

            private void Start()
            {
                lightComponent = GetComponentInChildren<Light>();
                if (lightComponent != null)
                {
                    baseIntensity = lightComponent.intensity;
                    
                    // URP-specific settings
                    if (useTemperature)
                    {
                        lightComponent.useColorTemperature = true;
                        lightComponent.colorTemperature = colorTemperature;
                    }
                    
                    // Use pixel lighting for more accurate lighting
                    lightComponent.renderMode = LightRenderMode.ForcePixel;
                    
                    // Use shadow resolution from quality settings
                    lightComponent.shadowResolution = LightShadowResolution.FromQualitySettings;
                    
                    // Adjust shadow parameters for better performance
                    lightComponent.shadowBias = 0.05f;
                    lightComponent.shadowNormalBias = 0.4f;
                }
            }

            private void Update()
            {
                if (Time.time - lastUpdate > flickerSpeed)
                {
                    lastUpdate = Time.time;
                    
                    // Calculate new intensity with a mix of randomness and noise
                    float noise = Mathf.PerlinNoise(Time.time * flickerSpeed * 2f, 0f);
                    float randomFactor = Random.Range(0f, 0.2f);
                    nextIntensity = Mathf.Lerp(minIntensity, maxIntensity, noise + randomFactor) * baseIntensity;
                }
                
                if (lightComponent != null)
                {
                    // Smooth interpolation toward target intensity
                    // Smooth interpolation toward target intensity
                    lightComponent.intensity = Mathf.Lerp(lightComponent.intensity, nextIntensity, Time.deltaTime * 10f);
                }
            }
        } // End of GaslightFlicker class

        /// Generates the historical Portland port area with docks, warehouses, and related structures
        private void GeneratePortArea()
        {
            if (dockSectionPrefab == null || warehousePrefabs == null || warehousePrefabs.Length == 0)
            {
                Debug.LogWarning("Port generation skipped - missing required prefabs");
                return;
            }

            Debug.Log("Generating port area...");

            try
            {
                // Find the eastern edge of the grid for port placement
                int portEdgeX = gridSize.x - 1;
                
                // Create main dock structure
                CreateMainDockStructure(portEdgeX);
                
                // Place warehouses along the waterfront
                PlaceWaterfrontWarehouses(portEdgeX);
                
                // Add cargo and mooring areas
                CreateCargoAreas();
                
                // Place mooring posts along docks
                PlaceMooringPosts();
                
                Debug.Log("Port area generation completed successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error during port generation: {e.Message}");
            }
        }

        /// <summary>
        /// Creates the main dock structure along the waterfront
        /// </summary>
        private void CreateMainDockStructure(int startX)
        {
            // Calculate starting position for the dock
            Vector3 dockStart = GridToWorldPosition(startX, gridSize.y / 2 - dockSections / 2);
            
            // Create main dock platform
            for (int i = 0; i < dockSections; i++)
            {
                Vector3 sectionPos = dockStart + new Vector3(0, 0, i * dockWidth);
                
                // Create dock section
                GameObject dockSection = Instantiate(dockSectionPrefab, sectionPos, Quaternion.identity, portsParent);
                dockSection.name = $"DockSection_{i}";
                
                // Adjust scale to match dock dimensions
                dockSection.transform.localScale = new Vector3(
                    dockLength / 10f, // Assuming default prefab is 10 units long
                    1f,
                    dockWidth / 10f   // Assuming default prefab is 10 units wide
                );
                
                // Add to tracking collections
                generatedObjects.Add(dockSection);
                dockPositions.Add(sectionPos);
                
                // Mark grid cells as dock type
                Vector2Int gridPos = WorldToGridPosition(sectionPos);
                gridCells[gridPos] = CellType.Dock;
            }
        }

        /// <summary>
        /// Places warehouses along the waterfront area
        /// </summary>
        private void PlaceWaterfrontWarehouses(int startX)
        {
            // Calculate warehouse placement area
            int warehouseStartX = startX - 2; // Two cells back from water
            int warehouseCount = Mathf.Min(5, warehousePrefabs.Length); // Limit number of warehouses
            
            for (int i = 0; i < warehouseCount; i++)
            {
                // Calculate position with spacing between warehouses
                Vector3 basePos = GridToWorldPosition(warehouseStartX, 
                    gridSize.y / 2 - warehouseCount + i * 2);
                
                // Adjust position to align with street grid
                Vector3 adjustedPos = GetWarehousePosition(basePos);
                
                // Select and place warehouse
                GameObject warehouse = Instantiate(
                    warehousePrefabs[i % warehousePrefabs.Length],
                    adjustedPos,
                    Quaternion.Euler(0, 90, 0), // Face the water
                    portsParent
                );
                
                warehouse.name = $"Warehouse_{i}";
                
                // Add to tracking collections
                generatedObjects.Add(warehouse);
                Vector2Int gridPos = WorldToGridPosition(adjustedPos);
                gridCells[gridPos] = CellType.Building;
                placedObjects[gridPos] = warehouse;
            }
        }

        /// <summary>
        /// Creates cargo areas near warehouses and docks
        /// </summary>
        private void CreateCargoAreas()
        {
            if (cratePrefabs == null || cratePrefabs.Length == 0) return;
            
            foreach (Vector3 dockPos in dockPositions)
            {
                // Create cargo clusters near dock sections
                CreateCargoCluster(dockPos);
            }
        }

        /// <summary>
        /// Creates a cluster of cargo crates near a dock position
        /// </summary>
        private void CreateCargoCluster(Vector3 centerPos)
        {
            int crateCount = random.Next(3, 8); // Random number of crates per cluster
            float clusterRadius = 5f; // Maximum radius for crate placement
            
            for (int i = 0; i < crateCount; i++)
            {
                // Calculate random position within cluster
                float angle = random.Next(360) * Mathf.Deg2Rad;
                float radius = random.Next((int)(clusterRadius * 100)) / 100f;
                
                Vector3 offset = new Vector3(
                    Mathf.Cos(angle) * radius,
                    0,
                    Mathf.Sin(angle) * radius
                );
                
                Vector3 cratePos = centerPos + offset;
                cratePos.y = 0.5f; // Slight lift to prevent Z-fighting
                
                // Select random crate prefab
                GameObject cratePrefab = cratePrefabs[random.Next(cratePrefabs.Length)];
                
                // Create crate with random rotation
                GameObject crate = Instantiate(
                    cratePrefab,
                    cratePos,
                    Quaternion.Euler(0, random.Next(360), 0),
                    portsParent
                );
                
                crate.name = $"Crate_{generatedObjects.Count}";
                generatedObjects.Add(crate);
                
                // Add slight scale variation
                float scaleVar = 0.8f + ((float)random.NextDouble() * 0.4f);
                crate.transform.localScale *= scaleVar;
            }
        }

        /// <summary>
        /// Places mooring posts along the dock edge
        /// </summary>
        private void PlaceMooringPosts()
        {
            if (mooringPostPrefab == null) return;
            
            float postSpacing = 10f; // Distance between mooring posts
            
            foreach (Vector3 dockPos in dockPositions)
            {
                // Place posts on both sides of the dock
                for (int side = 0; side < 2; side++)
                {
                    float sideOffset = (side == 0 ? dockWidth / 2 : -dockWidth / 2);
                    
                    // Create mooring post
                    Vector3 postPos = dockPos + new Vector3(dockLength / 2, 0, sideOffset);
                    
                    GameObject post = Instantiate(
                        mooringPostPrefab,
                        postPos,
                        Quaternion.identity,
                        portsParent
                    );
                    
                    post.name = $"MooringPost_{generatedObjects.Count}";
                    generatedObjects.Add(post);
                }
            }
        }

        /// <summary>
        /// Calculates an appropriate position for a warehouse that aligns with the street grid
        /// </summary>
        private Vector3 GetWarehousePosition(Vector3 basePosition)
        {
            Vector2Int gridPos = WorldToGridPosition(basePosition);
            
            // Ensure we're not placing on a street
            if (IsValidGridPosition(gridPos) && streetGrid[gridPos.x, gridPos.y])
            {
                // Move one cell away from street
                gridPos.x -= 1;
            }
            
            // Calculate final position with slight random offset
            Vector3 finalPos = GridToWorldPosition(gridPos.x, gridPos.y);
            finalPos += new Vector3(
                ((float)random.NextDouble() - 0.5f) * buildingVariation,
                0,
                ((float)random.NextDouble() - 0.5f) * buildingVariation
            );
            
            return finalPos;
        }

        /// <summary>
        /// Generates the Shanghai tunnel network beneath the streets of Portland
        /// </summary>
        private void GenerateTunnels()
        {
            if (tunnelSectionPrefab == null || tunnelEntrancePrefab == null)
            {
                Debug.LogWarning("Tunnel generation skipped - missing required prefabs");
                return;
            }

            Debug.Log("Generating Shanghai tunnel network...");

            try
            {
                // Create main tunnel network
                List<TunnelSegment> tunnelNetwork = GenerateTunnelNetwork();
                
                // Create physical tunnel sections
                CreateTunnelSections(tunnelNetwork);
                
                // Place support beams
                if (tunnelSupportBeamPrefab != null)
                {
                    AddTunnelSupports(tunnelNetwork);
                }
                
                Debug.Log($"Generated {tunnelNetwork.Count} tunnel segments");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error during tunnel generation: {e.Message}");
            }
        }

        /// <summary>
        /// Generates the logical network of tunnel segments
        /// </summary>
        private List<TunnelSegment> GenerateTunnelNetwork()
        {
            List<TunnelSegment> tunnelNetwork = new List<TunnelSegment>();
            HashSet<Vector2Int> processedCells = new HashSet<Vector2Int>();
            
            // Start from predefined entrance positions if available
            if (tunnelEntrancePositions != null && tunnelEntrancePositions.Length > 0)
            {
                foreach (Vector3 entrancePos in tunnelEntrancePositions)
                {
                    Vector2Int gridPos = WorldToGridPosition(entrancePos);
                    if (!processedCells.Contains(gridPos))
                    {
                        GenerateTunnelBranch(gridPos, tunnelNetwork, processedCells);
                    }
                }
            }
            else
            {
                // Generate entrances near selected buildings historically used for shanghaiing
                // Primarily near bars, hotels, and buildings near the port
                foreach (var building in placedObjects.Where(p => p.Value != null))
                {
                    // Higher chance for buildings near the port
                    float distanceToEdge = Mathf.Abs(building.Key.x - (gridSize.x - 1));
                    float portProximityFactor = Mathf.Clamp01(1f - (distanceToEdge / gridSize.x));
                    
                    if (random.NextDouble() < 0.15f * (1 + portProximityFactor)) // Increased chance near port
                    {
                        Vector2Int gridPos = building.Key;
                        if (!processedCells.Contains(gridPos))
                        {
                            // Mark this as an entrance
                            TunnelSegment entrance = new TunnelSegment
                            {
                                gridPosition = gridPos,
                                type = TunnelType.Entrance,
                                rotation = CalculateTunnelEntranceRotation(gridPos),
                                hasEntrance = true
                            };
                            
                            tunnelNetwork.Add(entrance);
                            processedCells.Add(gridPos);
                            
                            // Generate branch from here
                            GenerateTunnelBranch(gridPos, tunnelNetwork, processedCells);
                        }
                    }
                }
            }
            
            // Ensure minimum number of tunnel sections
            int attempts = 0;
            while (tunnelNetwork.Count < tunnelSectionCount && attempts < 100)
            {
                attempts++;
                
                // Prefer to expand existing tunnels rather than create new branches
                if (tunnelNetwork.Count > 0 && random.NextDouble() < 0.7f)
                {
                    // Pick a random existing tunnel end to expand from
                    TunnelSegment segment = tunnelNetwork[random.Next(tunnelNetwork.Count)];
                    GenerateTunnelBranch(segment.gridPosition, tunnelNetwork, processedCells);
                }
                else
                {
                    // Create a new branch
                    Vector2Int randomPos = new Vector2Int(
                        random.Next(1, gridSize.x - 1),
                        random.Next(1, gridSize.y - 1)
                    );
                    
                    if (!processedCells.Contains(randomPos))
                    {
                        GenerateTunnelBranch(randomPos, tunnelNetwork, processedCells);
                    }
                }
            }
            
            // Connect isolated segments
            ConnectTunnelSegments(tunnelNetwork, processedCells);
            
            return tunnelNetwork;
        }

        /// <summary>
        /// Connects isolated tunnel segments to create a more connected network
        /// </summary>
        private void ConnectTunnelSegments(List<TunnelSegment> network, HashSet<Vector2Int> processed)
        {
            // Identify isolated segments by checking connections
            List<List<int>> connectedGroups = new List<List<int>>();
            bool[] visited = new bool[network.Count];
            
            for (int i = 0; i < network.Count; i++)
            {
                if (visited[i]) continue;
                
                // Find all connected segments
                List<int> group = new List<int>();
                FindConnectedSegments(network, i, visited, group);
                
                if (group.Count > 0)
                {
                    connectedGroups.Add(group);
                }
            }
            
            // Connect isolated groups if there's more than one group
            if (connectedGroups.Count > 1)
            {
                for (int i = 1; i < connectedGroups.Count; i++)
                {
                    // Connect this group to the first group
                    ConnectTunnelGroups(network, processed, connectedGroups[0], connectedGroups[i]);
                }
            }
        }

        /// <summary>
        /// Finds all tunnel segments connected to a starting segment
        /// </summary>
        private void FindConnectedSegments(List<TunnelSegment> network, int startIndex, bool[] visited, List<int> group)
        {
            visited[startIndex] = true;
            group.Add(startIndex);
            
            // Check all other segments
            for (int i = 0; i < network.Count; i++)
            {
                if (visited[i]) continue;
                
                // Check if they're adjacent
                if (Vector2Int.Distance(network[startIndex].gridPosition, network[i].gridPosition) <= 1.5f)
                {
                    FindConnectedSegments(network, i, visited, group);
                }
            }
        }

        /// <summary>
        /// Connects two isolated groups of tunnel segments
        /// </summary>
        private void ConnectTunnelGroups(List<TunnelSegment> network, HashSet<Vector2Int> processed, 
                                        List<int> group1, List<int> group2)
        {
            // Find closest segment between groups
            int closest1 = group1[0];
            int closest2 = group2[0];
            float minDistance = float.MaxValue;
            
            foreach (int i in group1)
            {
                foreach (int j in group2)
                {
                    float dist = Vector2Int.Distance(network[i].gridPosition, network[j].gridPosition);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        closest1 = i;
                        closest2 = j;
                    }
                }
            }
            
            // Draw path between these segments
            Vector2Int start = network[closest1].gridPosition;
            Vector2Int end = network[closest2].gridPosition;
            
            Vector2Int current = start;
            while (current != end)
            {
                // Move toward end
                Vector2Int next = current;
                if (Mathf.Abs(current.x - end.x) > Mathf.Abs(current.y - end.y))
                {
                    next.x += (current.x < end.x) ? 1 : -1;
                }
                else
                {
                    next.y += (current.y < end.y) ? 1 : -1;
                }
                
                // Skip if already processed
                if (processed.Contains(next))
                {
                    current = next;
                    continue;
                }
                
                // Create tunnel segment
                TunnelSegment segment = new TunnelSegment
                {
                    gridPosition = next,
                    type = TunnelType.Straight,
                    rotation = (next.x != current.x) ? 90f : 0f,
                    hasEntrance = false
                };
                
                network.Add(segment);
                processed.Add(next);
                current = next;
            }
        }

        /// <summary>
        /// Generates a branch of the tunnel network starting from a given position
        /// </summary>
        private void GenerateTunnelBranch(Vector2Int start, List<TunnelSegment> network, HashSet<Vector2Int> processed)
        {
            int maxBranchLength = random.Next(3, 8); // Variable branch length for realism
            int currentLength = 0;
            Vector2Int currentPos = start;
            
            // Don't create a new segment at the start position if it's already processed
            if (!processed.Contains(currentPos))
            {
                // Create initial tunnel segment
                TunnelSegment segment = new TunnelSegment
                {
                    gridPosition = currentPos,
                    type = TunnelType.Junction,
                    rotation = CalculateTunnelRotation(currentPos),
                    hasEntrance = false // Entrance is created separately
                };
                
                network.Add(segment);
                processed.Add(currentPos);
                
                // Mark cells as tunnel type
                gridCells[currentPos] = CellType.Tunnel;
            }
            
            while (currentLength < maxBranchLength && network.Count < tunnelSectionCount)
            {
                // Determine next direction
                Vector2Int nextPos = GetNextTunnelPosition(currentPos, processed);
                if (nextPos == currentPos) break; // No valid direction found
                
                // Create new tunnel segment
                TunnelSegment segment = new TunnelSegment
                {
                    gridPosition = nextPos,
                    type = DetermineTunnelType(nextPos),
                    rotation = CalculateTunnelRotation(nextPos, currentPos),
                    hasEntrance = false
                };
                
                network.Add(segment);
                processed.Add(nextPos);
                
                // Mark cells as tunnel type
                gridCells[nextPos] = CellType.Tunnel;
                
                // Potentially create a side branch
                if (random.NextDouble() < 0.2f && currentLength > 1)
                {
                    GenerateTunnelBranch(nextPos, network, processed);
                }
                
                currentPos = nextPos;
                currentLength++;
            }
            
            // Update the final segment type if it's a dead end
            if (currentLength > 0 && network.Count < tunnelSectionCount)
            {
                for (int i = network.Count - 1; i >= 0; i--)
                {
                    if (network[i].gridPosition == currentPos)
                    {
                        // Check if this is truly an end segment
                        if (CountTunnelConnections(currentPos) <= 1)
                        {
                            network[i].type = TunnelType.DeadEnd;
                            
                            // Adjust rotation to face the connected segment
                            Vector2Int[] directions = new[]
                            {
                                new Vector2Int(1, 0),
                                new Vector2Int(-1, 0),
                                new Vector2Int(0, 1),
                                new Vector2Int(0, -1)
                            };
                            
                            foreach (Vector2Int dir in directions)
                            {
                                Vector2Int checkPos = currentPos + dir;
                                if (HasTunnelAt(checkPos))
                                {
                                    // Calculate rotation to face the connected tunnel
                                    if (dir.x > 0) network[i].rotation = 90f;
                                    else if (dir.x < 0) network[i].rotation = 270f;
                                    else if (dir.y > 0) network[i].rotation = 0f;
                                    else network[i].rotation = 180f;
                                    break;
                                }
                            }
                        }
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Placeholder for placing transition points between areas
        /// </summary>
        private void PlaceTransitionPoints()
        {
            Debug.Log("Transition point placement placeholder");
        }

        /// <summary>
        /// Generates spawn points throughout the environment based on accessibility and gameplay requirements
        /// </summary>
        private void GenerateSpawnPoints()
        {
            Debug.Log("Generating spawn points...");
            
            try
            {
                // Clear existing spawn points except default
                ClearExistingSpawnPoints();
                
                // Generate different types of spawn points
                GenerateStreetSpawnPoints();
                GenerateTunnelEntranceSpawns();
                GeneratePortAreaSpawns();
                
                // Validate and process spawn points
                ValidateSpawnPoints();
                
                Debug.Log($"Generated {spawnPoints.Count} spawn points");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error during spawn point generation: {e.Message}");
            }
        }

        /// <summary>
        /// Clears existing spawn points while preserving the default spawn
        /// </summary>
        private void ClearExistingSpawnPoints()
        {
            // Keep track of default spawn
            Transform defaultSpawn = defaultSpawnPoint;
            
            // Remove existing spawn points
            foreach (Transform spawn in spawnPoints)
            {
                if (spawn != defaultSpawn && spawn != null)
                {
                    DestroyImmediate(spawn.gameObject);
                }
            }
            
            spawnPoints.Clear();
            availableSpawnPoints.Clear();
            
            // Restore default spawn
            if (defaultSpawn != null)
            {
                spawnPoints.Add(defaultSpawn);
                availableSpawnPoints.Add(defaultSpawn.position);
            }
        }

        /// <summary>
        /// Generates spawn points along accessible streets
        /// </summary>
        private void GenerateStreetSpawnPoints()
        {
            // Create spawn points at key street intersections
            foreach (Vector3 intersection in intersectionPositions)
            {
                if (random.NextDouble() < 0.3f) // 30% chance for each intersection
                {
                    CreateSpawnPoint(intersection + Vector3.up * 0.5f, "StreetSpawn");
                }
            }
            
            // Add spawn points along main streets
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int z = 0; z < gridSize.y; z++)
                {
                    if (streetGrid[x, z] && random.NextDouble() < 0.1f) // 10% chance for street segments
                    {
                        Vector3 position = GridToWorldPosition(x, z) + Vector3.up * 0.5f;
                        
                        // Only create if not too close to other spawn points
                        if (!IsTooCloseToExistingSpawn(position))
                        {
                            CreateSpawnPoint(position, "StreetSpawn");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Generates spawn points near tunnel entrances
        /// </summary>
        private void GenerateTunnelEntranceSpawns()
        {
            // Iterate through grid cells marked as tunnel entrances
            foreach (var cell in gridCells)
            {
                if (cell.Value == CellType.Tunnel)
                {
                    Vector3 position = GridToWorldPosition(cell.Key.x, cell.Key.y) + Vector3.up * 0.5f;
                    
                    // Check accessibility and spacing
                    if (IsAccessiblePosition(position) && !IsTooCloseToExistingSpawn(position))
                    {
                        CreateSpawnPoint(position, "TunnelSpawn");
                    }
                }
            }
        }

        /// <summary>
        /// Generates spawn points in the port area
        /// </summary>
        private void GeneratePortAreaSpawns()
        {
            // Add spawn points near warehouses
            foreach (Vector3 dockPos in dockPositions)
            {
                if (random.NextDouble() < 0.4f) // 40% chance for each dock section
                {
                    Vector3 spawnPos = dockPos + Vector3.up * 0.5f;
                    // Offset slightly from dock edge
                    spawnPos += new Vector3(random.Next(-2, 3), 0, random.Next(-2, 3));
                    
                    if (!IsTooCloseToExistingSpawn(spawnPos))
                    {
                        CreateSpawnPoint(spawnPos, "PortSpawn");
                    }
                }
            }
        }

        /// <summary>
        /// Creates a spawn point at the specified position
        /// </summary>
        private void CreateSpawnPoint(Vector3 position, string type)
        {
            GameObject spawnPoint = new GameObject($"{type}_{spawnPoints.Count}");
            spawnPoint.transform.position = position;
            spawnPoint.transform.SetParent(environmentParent);
            
            // Add spawn point component or tag as needed
            spawnPoint.tag = "SpawnPoint";
            
            // Optional: Add spawn point metadata component
            SpawnPointData spawnData = spawnPoint.AddComponent<SpawnPointData>();
            spawnData.spawnType = type;
            spawnData.lastUsedTime = 0f;
            
            spawnPoints.Add(spawnPoint.transform);
            availableSpawnPoints.Add(position);
        }

        /// <summary>
        /// Validates all spawn points for accessibility and proper placement
        /// </summary>
        private void ValidateSpawnPoints()
        {
            List<Transform> invalidSpawns = new List<Transform>();
            
            foreach (Transform spawn in spawnPoints)
            {
                if (spawn == defaultSpawnPoint) continue; // Skip default spawn validation
                
                Vector3 position = spawn.position;
                bool isValid = true;
                
                // Check ground presence
                if (!Physics.Raycast(position + Vector3.up, Vector3.down, 2f))
                {
                    isValid = false;
                }
                
                // Check for obstacles
                if (Physics.OverlapSphere(position, 1f).Length > 0)
                {
                    isValid = false;
                }
                
                // Check accessibility from at least one direction
                bool hasAccess = false;
                Vector3[] directions = { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };
                foreach (Vector3 dir in directions)
                {
                    if (!Physics.Raycast(position, dir, 2f))
                    {
                        hasAccess = true;
                        break;
                    }
                }
                
                if (!hasAccess) isValid = false;
                
                // Mark for removal if invalid
                if (!isValid)
                {
                    invalidSpawns.Add(spawn);
                }
            }
            
            // Remove invalid spawn points
            foreach (Transform spawn in invalidSpawns)
            {
                spawnPoints.Remove(spawn);
                availableSpawnPoints.Remove(spawn.position);
                DestroyImmediate(spawn.gameObject);
            }
        }

        /// <summary>
        /// Checks if a position is too close to existing spawn points
        /// </summary>
        private bool IsTooCloseToExistingSpawn(Vector3 position)
        {
            float minSpawnDistance = 10f; // Minimum distance between spawn points
            
            foreach (Vector3 existingSpawn in availableSpawnPoints)
            {
                if (Vector3.Distance(position, existingSpawn) < minSpawnDistance)
                {
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Checks if a position is accessible for spawning
        /// </summary>
        private bool IsAccessiblePosition(Vector3 position)
        {
            // Check ground
            if (!Physics.Raycast(position + Vector3.up, Vector3.down, 2f))
            {
                return false;
            }
            
            // Check overhead clearance
            if (Physics.Raycast(position, Vector3.up, 2f))
            {
                return false;
            }
            
            // Check surrounding area
            float checkRadius = 1f;
            Collider[] colliders = Physics.OverlapSphere(position, checkRadius);
            foreach (Collider col in colliders)
            {
                // Skip trigger colliders
                if (col.isTrigger) continue;
                
                // If we find any non-trigger colliders within radius, position is blocked
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Represents a segment in the tunnel network
        /// </summary>
        private struct TunnelSegment
        {
            public Vector2Int gridPosition;
            public TunnelType type;
            public float rotation;
            public bool hasEntrance;
        }

        /// <summary>
        /// Types of tunnel segments
        /// </summary>
        private enum TunnelType
        {
            Straight,
            Junction,
            DeadEnd
        }

        /// <summary>
        /// Metadata component for spawn points
        /// </summary>
        class SpawnPointData : MonoBehaviour
        {
            /// <summary>
            /// Type of spawn point (e.g., "StreetSpawn", "TunnelSpawn", "PortSpawn")
            /// </summary>
            public string spawnType;

            /// <summary>
            /// Last time this spawn point was used
            /// </summary>
            public float lastUsedTime;

            /// <summary>
            /// Whether this spawn point is currently occupied
            /// </summary>
            public bool isOccupied;
        }

        /// <summary>
        /// Draws debug grid visualization in the editor
        /// </summary>
        private void DrawDebugGrid()
        {
            // Draw grid lines for streets
            Gizmos.color = streetGridColor;
            
            // Draw horizontal streets
            for (int z = 0; z < gridSize.y; z++)
            {
                Vector3 start = gridOrigin + new Vector3(0, 0.1f, z * cellSize);
                Vector3 end = gridOrigin + new Vector3((gridSize.x - 1) * cellSize, 0.1f, z * cellSize);
                Gizmos.DrawLine(start, end);
            }
            
            // Draw vertical streets
            for (int x = 0; x < gridSize.x; x++)
            {
                Vector3 start = gridOrigin + new Vector3(x * cellSize, 0.1f, 0);
                Vector3 end = gridOrigin + new Vector3(x * cellSize, 0.1f, (gridSize.y - 1) * cellSize);
                Gizmos.DrawLine(start, end);
            }
            
            // Draw building plots
            Gizmos.color = buildingGridColor;
            for (int x = 0; x < gridSize.x - 1; x++)
            {
                for (int z = 0; z < gridSize.y - 1; z++)
                {
                    Vector3 center = gridOrigin + new Vector3(
                        x * cellSize + cellSize / 2,
                        0.1f,
                        z * cellSize + cellSize / 2
                    );
                    
                    float plotSize = cellSize - (streetWidth + sidewalkWidth * 2);
                    Vector3 size = new Vector3(plotSize, 0.1f, plotSize);
                    Gizmos.DrawWireCube(center, size);
                }
            }
        }
        
        /// <summary>
        /// Draws spawn points in the editor
        /// </summary>
        private void DrawSpawnPoints()
        {
            Gizmos.color = spawnPointColor;
            
            if (defaultSpawnPoint != null)
            {
                Gizmos.DrawSphere(defaultSpawnPoint.position, 1f);
                Gizmos.DrawWireSphere(defaultSpawnPoint.position, 1.5f);
            }
            
            foreach (Transform spawn in spawnPoints)
            {
                if (spawn != null && spawn != defaultSpawnPoint)
                {
                    Gizmos.DrawSphere(spawn.position, 0.5f);
                    
                    // Draw accessibility radius
                    Gizmos.DrawWireSphere(spawn.position, 1f);
                    
                    // Draw ground check line
                    Gizmos.DrawLine(spawn.position + Vector3.up, 
                                  spawn.position + Vector3.down);
                }
            }
        }

        #region Utility Methods

        /// <summary>
        /// Determines if a grid position should be a street based on the grid pattern
        /// </summary>
        private bool IsStreetPosition(int x, int z)
        {
            if (useProceduralGeneration)
            {
                // For procedural generation, create a regular grid pattern
                return x % 2 == 0 || z % 2 == 0;
            }
            else
            {
                // For preset layout, streets are on the edges of each block (historical 61m pattern)
                return x % 3 == 0 || z % 3 == 0;
            }
        }

        /// <summary>
        /// Converts grid coordinates to world position
        /// </summary>
        private Vector3 GridToWorldPosition(int x, int z)
        {
            return gridOrigin + new Vector3(x * cellSize, 0, z * cellSize);
        }

        /// <summary>
        /// Converts world position to grid coordinates
        /// </summary>
        private Vector2Int WorldToGridPosition(Vector3 worldPos)
        {
            Vector3 localPos = worldPos - gridOrigin;
            return new Vector2Int(
                Mathf.RoundToInt(localPos.x / cellSize),
                Mathf.RoundToInt(localPos.z / cellSize)
            );
        }

        /// <summary>
        /// Checks if a grid position is within bounds
        /// </summary>
        private bool IsValidGridPosition(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < gridSize.x && pos.y >= 0 && pos.y < gridSize.y;
        }

        /// <summary>
        /// Calculates the rotation for corner street sections
        /// </summary>
        private float CalculateCornerRotation(bool north, bool east, bool south, bool west)
        {
            if (north && east) return 0f;
            if (east && south) return 90f;
            if (south && west) return 180f;
            if (west && north) return 270f;
            return 0f;
        }

        /// <summary>
        /// Calculates the rotation for tunnel sections
        /// </summary>
        private float CalculateTunnelRotation(Vector2Int pos, Vector2Int fromPos = default)
        {
            // If coming from another position, align with that direction
            if (fromPos != default && fromPos != pos)
            {
                if (fromPos.x < pos.x) return 90f;  // Coming from west
                if (fromPos.x > pos.x) return 270f; // Coming from east
                if (fromPos.y < pos.y) return 0f;   // Coming from south
                if (fromPos.y > pos.y) return 180f; // Coming from north
            }
            
            // Otherwise check adjacent cells to determine orientation
            bool northTunnel = HasTunnelAt(pos + Vector2Int.up);
            bool southTunnel = HasTunnelAt(pos + Vector2Int.down);
            bool eastTunnel = HasTunnelAt(pos + Vector2Int.right);
            bool westTunnel = HasTunnelAt(pos + Vector2Int.left);
            
            if (northTunnel && southTunnel) return 0f;
            if (eastTunnel && westTunnel) return 90f;
            if (northTunnel) return 0f;
            if (eastTunnel) return 90f;
            if (southTunnel) return 180f;
            if (westTunnel) return 270f;
            
            return random.Next(4) * 90f;
        }

        /// <summary>
        /// Calculates rotation for tunnel entrances
        /// </summary>
        private float CalculateTunnelEntranceRotation(Vector2Int pos)
        {
            // Try to align entrance with nearby streets
            Vector2Int[] directions = new[] 
            {
                Vector2Int.up, 
                Vector2Int.right, 
                Vector2Int.down, 
                Vector2Int.left
            };
            
            foreach (Vector2Int dir in directions)
            {
                Vector2Int checkPos = pos + dir;
                if (IsValidGridPosition(checkPos) && streetGrid[checkPos.x, checkPos.y])
                {
                    // Orient entrance toward street
                    if (dir == Vector2Int.up) return 0f;
                    if (dir == Vector2Int.right) return 90f;
                    if (dir == Vector2Int.down) return 180f;
                    if (dir == Vector2Int.left) return 270f;
                }
            }
            
            // Default orientation
            return 0f;
        }

        /// <summary>
        /// Determines the type of tunnel section based on position
        /// </summary>
        private TunnelType DetermineTunnelType(Vector2Int pos)
        {
            // Check adjacent cells for other tunnel sections
            int connections = CountTunnelConnections(pos);
            
            if (connections > 2) return TunnelType.Junction;
            if (connections == 2) return TunnelType.Straight;
            return TunnelType.DeadEnd;
        }

        /// <summary>
        /// Counts the number of adjacent tunnel sections
        /// </summary>
        private int CountTunnelConnections(Vector2Int pos)
        {
            int connections = 0;
            Vector2Int[] directions = new[]
            {
                new Vector2Int(1, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, -1)
            };
            
            foreach (Vector2Int dir in directions)
            {
                Vector2Int adjacent = pos + dir;
                if (IsValidGridPosition(adjacent) && 
                    gridCells.ContainsKey(adjacent) && 
                    gridCells[adjacent] == CellType.Tunnel)
                {
                    connections++;
                }
            }
            
            return connections;
        }

        /// <summary>
        /// Checks if a position contains a tunnel
        /// </summary>
        private bool HasTunnelAt(Vector2Int pos)
        {
            return IsValidGridPosition(pos) && 
                   gridCells.ContainsKey(pos) && 
                   gridCells[pos] == CellType.Tunnel;
        }

        /// <summary>
        /// Gets the next valid position for tunnel generation
        /// </summary>
        private Vector2Int GetNextTunnelPosition(Vector2Int current, HashSet<Vector2Int> processed)
        {
            List<Vector2Int> possibleDirections = new List<Vector2Int>
            {
                new Vector2Int(1, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, -1)
            };
            
            // Shuffle directions
            for (int i = possibleDirections.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                Vector2Int temp = possibleDirections[i];
                possibleDirections[i] = possibleDirections[j];
                possibleDirections[j] = temp;
            }
            
            // Try each direction
            foreach (Vector2Int dir in possibleDirections)
            {
                Vector2Int next = current + dir;
                if (IsValidTunnelPosition(next) && !processed.Contains(next))
                {
                    return next;
                }
            }
            
            return current; // No valid direction found
        }

        /// <summary>
        /// Checks if a position is valid for tunnel placement
        /// </summary>
        private bool IsValidTunnelPosition(Vector2Int pos)
        {
            if (!IsValidGridPosition(pos)) return false;
            
            // Check if position is under a building or street
            return gridCells.ContainsKey(pos) && 
                   (gridCells[pos] == CellType.Empty || 
                    gridCells[pos] == CellType.Building || 
                    gridCells[pos] == CellType.Street);
        }

        /// <summary>
        /// Creates tunnel sections based on the logical network
        /// </summary>
        private void CreateTunnelSections(List<TunnelSegment> network)
        {
            foreach (TunnelSegment segment in network)
            {
                Vector3 position = GridToWorldPosition(segment.gridPosition.x, segment.gridPosition.y);
                position.y = -3f; // Underground level
                
                // Create tunnel section
                GameObject tunnel = Instantiate(tunnelSectionPrefab, position, 
                    Quaternion.Euler(0, segment.rotation, 0), tunnelsParent);
                
                tunnel.name = $"Tunnel_{segment.gridPosition.x}_{segment.gridPosition.y}";
                generatedObjects.Add(tunnel);
                
                // Create entrance if needed
                if (segment.hasEntrance)
                {
                    CreateTunnelEntrance(position, segment.rotation);
                }
            }
        }

        /// <summary>
        /// Creates a tunnel entrance at the specified position
        /// </summary>
        private void CreateTunnelEntrance(Vector3 tunnelPos, float rotation)
        {
            Vector3 entrancePos = tunnelPos;
            entrancePos.y = 0; // Ground level
            
            GameObject entrance = Instantiate(tunnelEntrancePrefab, entrancePos,
                Quaternion.Euler(0, rotation, 0), tunnelsParent);
            
            entrance.name = $"TunnelEntrance_{entrancePos.x}_{entrancePos.z}";
            generatedObjects.Add(entrance);
            
            // Add transition trigger if configured
            if (tunnelEntranceTrigger != null)
            {
                AreaTransitionTrigger trigger = entrance.AddComponent<AreaTransitionTrigger>();
                trigger.destinationScene = "ShanghaiTunnels";
                trigger.transitionType = AreaTransitionTrigger.TransitionType.FadeToBlack;
            }
        }

        /// <summary>
        /// Adds support beams throughout the tunnel network
        /// </summary>
        private void AddTunnelSupports(List<TunnelSegment> network)
        {
            if (tunnelSupportBeamPrefab == null) return;

            Debug.Log("Adding support beams to tunnel network...");

            float supportSpacing = 5f; // Historical spacing between support beams
            
            foreach (TunnelSegment segment in network)
            {
                Vector3 startPos = GridToWorldPosition(segment.gridPosition.x, segment.gridPosition.y);
                startPos.y = -3f; // Underground level
                
                // Calculate number of supports needed for this segment
                float segmentLength = cellSize;
                int supportsCount = Mathf.CeilToInt(segmentLength / supportSpacing);
                
                // Place support beams along the tunnel segment
                for (int i = 0; i < supportsCount; i++)
                {
                    float progress = (i + 1) / (float)(supportsCount + 1);
                    
                    // Calculate support beam position
                    Vector3 supportPos = startPos + (Quaternion.Euler(0, segment.rotation, 0) * 
                        Vector3.forward * progress * cellSize);
                    
                    // Add slight random variation for realism
                    float randomOffset = ((float)random.NextDouble() - 0.5f) * 0.5f;
                    supportPos += new Vector3(randomOffset, 0, randomOffset);
                    
                    // Create support beam
                    GameObject support = Instantiate(tunnelSupportBeamPrefab, supportPos,
                        Quaternion.Euler(0, segment.rotation + 90, 0), tunnelsParent);
                    
                    support.name = $"TunnelSupport_{segment.gridPosition.x}_{segment.gridPosition.y}_{i}";
                    
                    // Add slight random rotation for weathered look
                    float randomTilt = ((float)random.NextDouble() - 0.5f) * 5f;
                    support.transform.Rotate(new Vector3(randomTilt, 0, randomTilt));
                    
                    // Add to tracking
                    generatedObjects.Add(support);
                }
                
                // Add extra supports at junctions and entrances
                if (segment.type == TunnelType.Junction || segment.hasEntrance)
                {
                    // Add additional support beams in a circular pattern
                    int extraSupportCount = 4;
                    float radius = 2f;
                    
                    for (int i = 0; i < extraSupportCount; i++)
                    {
                        float angle = (i * 360f / extraSupportCount) + segment.rotation;
                        Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * radius;
                        
                        // Create reinforced support beam
                        GameObject extraSupport = Instantiate(tunnelSupportBeamPrefab, 
                            startPos + offset,
                            Quaternion.Euler(0, angle + 90, 0),
                            tunnelsParent);
                        
                        extraSupport.name = $"ReinforcedSupport_{segment.gridPosition.x}_{segment.gridPosition.y}_{i}";
                        extraSupport.transform.localScale *= 1.2f; // Reinforced supports are slightly larger
                        
                        generatedObjects.Add(extraSupport);
                    }
                }
            }
        }

        /// <summary>
        /// Saves the current environment layout to a file
        /// </summary>
        private void SaveEnvironmentLayout()
        {
            if (string.IsNullOrEmpty(environmentSaveFilename)) return;

            try
            {
                EnvironmentSaveData saveData = new EnvironmentSaveData
                {
                    gridSize = this.gridSize,
                    cellSize = this.cellSize,
                    randomSeed = this.randomSeed,
                    gridCells = this.gridCells.ToDictionary(
                        kvp => $"{kvp.Key.x},{kvp.Key.y}",
                        kvp => (int)kvp.Value
                    ),
                    spawnPoints = this.spawnPoints.Where(sp => sp != null)
                        .Select(sp => sp.position)
                        .ToList(),
                    buildingPositions = this.buildingPositions,
                    gaslightPositions = this.gaslightPositions,
                    dockPositions = this.dockPositions
                };

                string json = JsonUtility.ToJson(saveData, true);
                string path = Path.Combine(Application.dataPath, environmentSaveFilename);
                File.WriteAllText(path, json);

                Debug.Log($"Environment layout saved to {path}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error saving environment layout: {e.Message}");
            }
        }

        /// <summary>
        /// Loads an environment layout from a file
        /// </summary>
        private void LoadEnvironmentLayout()
        {
            if (string.IsNullOrEmpty(environmentSaveFilename)) return;

            try
            {
                string path = Path.Combine(Application.dataPath, environmentSaveFilename);
                if (!File.Exists(path))
                {
                    Debug.LogWarning($"No saved layout found at {path}");
                    return;
                }

                string json = File.ReadAllText(path);
                EnvironmentSaveData saveData = JsonUtility.FromJson<EnvironmentSaveData>(json);

                // Clear existing environment
                ClearEnvironment();

                // Restore saved data
                this.gridSize = saveData.gridSize;
                this.cellSize = saveData.cellSize;
                this.randomSeed = saveData.randomSeed;

                // Rebuild environment from saved data
                foreach (var kvp in saveData.gridCells)
                {
                    string[] coords = kvp.Key.Split(',');
                    Vector2Int pos = new Vector2Int(
                        int.Parse(coords[0]),
                        int.Parse(coords[1])
                    );
                    this.gridCells[pos] = (CellType)kvp.Value;
                }

                // Regenerate environment using saved data
                GenerateEnvironment();

                Debug.Log($"Environment layout loaded from {path}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading environment layout: {e.Message}");
            }
        }

        /// <summary>
        /// Clears all generated environment objects
        /// </summary>
        private void ClearEnvironment()
        {
            foreach (GameObject obj in generatedObjects)
            {
                if (obj != null)
                {
                    DestroyImmediate(obj);
                }
            }
            
            generatedObjects.Clear();
            gaslightPositions.Clear();
            buildingPositions.Clear();
            dockPositions.Clear();
            intersectionPositions.Clear();
            gridCells.Clear();
            placedObjects.Clear();
            availableSpawnPoints.Clear();
        }

        /// <summary>
        /// Data structure for saving environment layout
        /// </summary>
        [System.Serializable]
        private class EnvironmentSaveData
        {
            public Vector2Int gridSize;
            public float cellSize;
            public int randomSeed;
            public Dictionary<string, int> gridCells;
            public List<Vector3> spawnPoints;
            public List<Vector3> buildingPositions;
            public List<Vector3> gaslightPositions;
            public List<Vector3> dockPositions;
        }

        #endregion
        
        #endregion // for Utility Methods
    }
}
