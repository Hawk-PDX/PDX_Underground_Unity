using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;
// ProBuilder namespaces
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
#if UNITY_EDITOR
using UnityEditor.ProBuilder;
using UnityEditor;
#endif

namespace PDXUnderground.Environment
{
    /// <summary>
    /// ProBuilder-based geometry generator for PDX Underground environment elements.
    /// Works in conjunction with PortlandEnvironmentSetup to create historically accurate structures.
    /// </summary>
    [ExecuteInEditMode]
    public class PDXProBuilderGenerator : MonoBehaviour
    {
        [System.Serializable]
        public class BuildingData
        {
            public Vector3 size = new Vector3(10f, 15f, 10f);
            public float windowHeight = 2f;
            public float windowWidth = 1.5f;
            public float windowSpacing = 3f;
            public float doorHeight = 2.5f;
            public float doorWidth = 1.2f;
            public Material wallMaterial;
            public Material roofMaterial;
            public bool addWindows = true;
            public bool addDoors = true;
            public int floorCount = 3;
        }
        #region Inspector Properties
        [Header("Historical Measurements")]
        [Tooltip("Portland's historical block size (61m)")]
        public float blockSize = 61f;
        [Tooltip("Standard street width in 1880s Portland")]
        public float streetWidth = 6f;
        [Tooltip("Historical sidewalk width")]
        public float sidewalkWidth = 2f;
        [Tooltip("Typical building height in 1880s Portland")]
        public float buildingHeight = 12f;

        [Header("References")]
        public PortlandEnvironmentSetup environmentSetup;
        
        [Header("Materials")]
        public Material cobblestoneStreetMaterial;
        public Material woodenSidewalkMaterial;
        public Material brickBuildingMaterial;
        public Material woodBuildingMaterial;
        public Material tunnelStoneMaterial;
        public Material tunnelSupportMaterial;
        #endregion
        #region Public Methods
        /// <summary>
        /// Creates a historically accurate street section with ProBuilder
        /// </summary>
        public GameObject GenerateStreetSection(Vector3 position)
        {
            #if UNITY_EDITOR
            // Create street base
            ProBuilderMesh street = CreateStreetBase(position);
            
            // Add sidewalks
            AddSidewalks(street);
            
            // Add historical details
            AddStreetDetails(street);

            // Tag and layer setup
            street.gameObject.tag = "Street";
            
            // Set materials
            if (cobblestoneStreetMaterial != null)
            {
                SetMaterial(street.gameObject, cobblestoneStreetMaterial);
                
                // Set sidewalk materials
                Transform leftSidewalk = street.transform.Find("Sidewalk_Left");
                Transform rightSidewalk = street.transform.Find("Sidewalk_Right");
                
                if (leftSidewalk != null && woodenSidewalkMaterial != null)
                {
                    SetMaterial(leftSidewalk.gameObject, woodenSidewalkMaterial);
                }
                
                if (rightSidewalk != null && woodenSidewalkMaterial != null)
                {
                    SetMaterial(rightSidewalk.gameObject, woodenSidewalkMaterial);
                }
            }
            
            return street.gameObject;
            #else
            return null;
            #endif
        }
        #endregion
        #region ProBuilder Generation Methods
        private void ExtrudeWalls(ProBuilderMesh mesh, float height)
        {
            if (mesh == null) return;

            // Get all edges for extrusion
            var edges = mesh.edges.ToArray();
            mesh.Extrude(edges, ExtrudeMethod.FaceNormal, height);

            // Important: Always call these after modifying the mesh
            mesh.ToMesh();
            mesh.Refresh();
        }

        private void AddRoof(ProBuilderMesh mesh, BuildingData data)
        {
            if (mesh == null) return;

            // Find top faces
            var topFaces = mesh.faces.Where(f => 
                Vector3.Dot(mesh.GetFaceNormal(f), Vector3.up) > 0.9f ||
                mesh.GetFaceCenter(f).y > data.size.y * 0.9f).ToArray();

            // Extrude roof upward
            mesh.Extrude(topFaces, 0, true, 2.0f);

            mesh.ToMesh();
            mesh.Refresh();
        }

        private void AddWindows(ProBuilderMesh mesh, BuildingData data)
        {
            if (mesh == null) return;

            // Find side faces (vertical walls)
            var sideFaces = mesh.faces.Where(f => 
                Mathf.Abs(Vector3.Dot(mesh.GetFaceNormal(f), Vector3.up)) < 0.1f).ToArray();

            foreach (var face in sideFaces)
            {
                // Calculate number of windows for this face
                float faceWidth = mesh.GetFaceSize(face).x;
                int windowCount = Mathf.FloorToInt(faceWidth / data.windowSpacing);

                if (windowCount > 0)
                {
                    // Add windows by subtracting from the face
                    var faceVertices = mesh.faces[face.indexLocal].distinctIndexes;
                    var faceCenter = mesh.GetFaceCenter(face);

                    // Extrude inward for windows
                    mesh.Extrude(new Face[] { face }, ExtrudeMethod.IndividualFaces, -0.2f);
                }
            }

            mesh.ToMesh();
            mesh.Refresh();
        }

        private GameObject FinalizeMesh(ProBuilderMesh mesh, BuildingData data)
        {
            if (mesh == null) return null;

            // Ensure mesh and UV are properly set up
            mesh.ToMesh();
            mesh.Refresh();
            mesh.RefreshUV(mesh.faces);

            // Apply materials
            if (data.wallMaterial != null)
            {
                ApplyTexture(mesh, data.wallMaterial);
            }

            // Generate lightmap UVs and set up gameObject properties
            var meshFilter = mesh.GetComponent<MeshFilter>();
            var meshRenderer = mesh.GetComponent<MeshRenderer>();
            
            if (meshFilter != null && meshRenderer != null && meshFilter.sharedMesh != null)
            {
                UnityEngine.Unwrapping.GenerateSecondaryUVSet(meshFilter.sharedMesh);
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                meshRenderer.receiveShadows = true;
            }
            
            // Add collision
            if (!mesh.gameObject.GetComponent<MeshCollider>())
            {
                mesh.gameObject.AddComponent<MeshCollider>();
            }
            
            return mesh.gameObject;
        }
        
        private ProBuilderMesh CreateStreetBase(Vector3 position)
        {
            #if UNITY_EDITOR
            // Create base mesh
            ProBuilderMesh street = ShapeGenerator.GeneratePlane(
                PivotLocation.Center,
                blockSize,
                streetWidth,
                0, 0,
                Axis.Up
            );

            street.transform.position = position;
            street.gameObject.name = "StreetSection";

            // Add slight crown to street (historical detail)
            Vector3[] positions = street.positions.ToArray();
            for (int i = 0; i < positions.Length; i++)
            {
                float distanceFromCenter = Mathf.Abs(positions[i].x) / (streetWidth / 2f);
                positions[i].y = 0.1f * (1f - distanceFromCenter * distanceFromCenter);
            }
            street.positions = positions;
            street.Refresh();

            return street;
            #else
            return null;
            #endif
        }

        private ProBuilderMesh CreateSimpleBuilding(Vector3 size)
        {
            // Create a cube as base using ProBuilder API
            var meshGO = ShapeGenerator.CreateShape(ShapeType.Cube);
            var mesh = meshGO.GetComponent<ProBuilderMesh>();
            
            if (mesh == null)
            {
                Debug.LogError("Failed to create ProBuilderMesh. Check ProBuilder package installation.");
                return null;
            }

            // Scale to desired size
            var positions = mesh.positions.ToList();
            for (int i = 0; i < positions.Count; i++)
            {
                // Only scale X and Z, height will be extruded later
                positions[i] = new Vector3(
                    positions[i].x * size.x * 0.5f,
                    positions[i].y * 0.1f, // Keep it flat initially
                    positions[i].z * size.z * 0.5f
                );
            }
            mesh.positions = positions;

            // Important: Always call these after modifying the mesh
            mesh.ToMesh();
            mesh.Refresh();

            return mesh;
        }

        private void AddSidewalks(ProBuilderMesh street)
        {
            #if UNITY_EDITOR
            if (street == null) return;

            // Add left and right sidewalks
            float[] sides = { -1, 1 }; // Left and right
            foreach (float side in sides)
            {
                float offset = (streetWidth / 2f + sidewalkWidth / 2f) * side;
                
                ProBuilderMesh sidewalk = ShapeGenerator.GeneratePlane(
                    PivotLocation.Center,
                    blockSize,
                    sidewalkWidth,
                    0, 0,
                    Axis.Up
                );

                sidewalk.transform.position = street.transform.position + 
                    new Vector3(offset, 0.15f, 0);
                sidewalk.transform.SetParent(street.transform);
                sidewalk.gameObject.name = side < 0 ? "Sidewalk_Left" : "Sidewalk_Right";

                // Update mesh
                sidewalk.ToMesh();
                sidewalk.Refresh();
            }
            #endif
        }

        private ProBuilderMesh CreateBuildingBase(Vector3 position, bool isCorner, Vector3 size)
        {
            #if UNITY_EDITOR
            ProBuilderMesh building = ShapeGenerator.GenerateCube(
                PivotLocation.Bottom,
                size
            );

            building.transform.position = position;
            building.gameObject.name = isCorner ? "CornerBuilding" : "Building";

            return building;
            #else
            return null;
            #endif
        }

        private void AddStreetDetails(ProBuilderMesh street)
        {
            #if UNITY_EDITOR
            if (street == null) return;

            // Add crown to street
            var positions = street.positions.ToList();
            for (int i = 0; i < positions.Count; i++)
            {
                float distanceFromCenter = Mathf.Abs(positions[i].x) / (streetWidth / 2f);
                positions[i].y += 0.05f * (1f - distanceFromCenter * distanceFromCenter);
            }
            street.positions = positions;
            street.Refresh();
            #endif
        }
        
        private void AddVictorianDetails(ProBuilderMesh building)
        {
            #if UNITY_EDITOR
            // Add window indents - Victorian buildings had recessed windows
            List<Face> facades = new List<Face>();
            foreach (Face face in building.faces)
            {
                if (face.normal == Vector3.forward || face.normal == Vector3.right)
                {
                    facades.Add(face);
                }
            }

            if (facades.Count > 0)
            {
                building.Extrude(facades.ToArray(), ExtrudeMethod.IndividualFaces, 0.3f);
            }

            // Add decorative cornices (common in 1880s architecture)
            List<Face> topFaces = new List<Face>();
            foreach (Face face in building.faces)
            {
                if (face.normal == Vector3.up)
                {
                    topFaces.Add(face);
                }
            }

            if (topFaces.Count > 0)
            {
                building.Extrude(topFaces.ToArray(), ExtrudeMethod.FaceNormal, 1f);
            }
            #endif
        }

        private void AddBuildingFeatures(ProBuilderMesh building, bool isCorner)
        {
            #if UNITY_EDITOR
            // Add doorways and storefronts (common in 1880s Portland)
            List<Face> groundFaces = new List<Face>();
            foreach (Face face in building.faces)
            {
                if ((face.normal == Vector3.forward || face.normal == Vector3.right) && 
                    face.Center().y < 1.5f)
                {
                    groundFaces.Add(face);
                }
            }

            if (groundFaces.Count > 0)
            {
                building.Extrude(groundFaces.ToArray(), ExtrudeMethod.IndividualFaces, 0.5f);
            }
            #endif
        }
        
        private ProBuilderMesh CreateTunnelBase(Vector3 position)
        {
            #if UNITY_EDITOR
            // Shanghai tunnels were typically about 2-3 meters high
            ProBuilderMesh tunnel = ShapeGenerator.GenerateCube(
                PivotLocation.Center,
                new Vector3(4f, 3f, 6f)
            );

            tunnel.transform.position = position;
            tunnel.gameObject.name = "TunnelSection";

            // Hollow out the tunnel
            List<Face> innerFaces = new List<Face>();
            foreach (Face face in tunnel.faces)
            {
                if (face.normal != Vector3.forward && face.normal != Vector3.back)
                {
                    innerFaces.Add(face);
                }
            }

            if (innerFaces.Count > 0)
            {
                tunnel.Extrude(innerFaces.ToArray(), ExtrudeMethod.IndividualFaces, -0.3f);
            }

            tunnel.ToMesh();
            tunnel.Refresh();
            return tunnel;
            #else
            return null;
            #endif
        }

        private void AddTunnelDetails(ProBuilderMesh tunnel, bool isEntrance)
        {
            #if UNITY_EDITOR
            // Add support beam slots (historically accurate for Shanghai tunnels)
            float tunnelLength = 6f;
            int supportCount = Mathf.FloorToInt(tunnelLength / 2f);

            for (int i = 0; i < supportCount; i++)
            {
                float zPos = (i * 2f) - (tunnelLength / 2f);
                
                ProBuilderMesh support = ShapeGenerator.GenerateCube(
                    PivotLocation.Center,
                    new Vector3(4.2f, 0.2f, 0.2f)
                );

                support.transform.position = tunnel.transform.position + 
                    new Vector3(0f, 1.5f, zPos);
                support.transform.SetParent(tunnel.transform);
                support.gameObject.name = $"Support_Beam_{i}";
                
                // Add slightly weathered appearance by varying scale
                float variation = 0.9f + Random.Range(0f, 0.2f);
                support.transform.localScale = new Vector3(
                    support.transform.localScale.x,
                    support.transform.localScale.y * variation,
                    support.transform.localScale.z
                );
            }

            if (isEntrance)
            {
                // Add entrance-specific details
                AddEntranceDetails(tunnel);
            }
            #endif
        }

        private void AddEntranceDetails(ProBuilderMesh tunnel)
        {
            #if UNITY_EDITOR
            // Add entrance archway (common in historical tunnels)
            List<Face> entranceFaces = new List<Face>();
            foreach (Face face in tunnel.faces)
            {
                if (face.normal == Vector3.forward)
                {
                    entranceFaces.Add(face);
                }
            }

            if (entranceFaces.Count > 0)
            {
                tunnel.Extrude(entranceFaces.ToArray(), ExtrudeMethod.FaceNormal, 0.5f);
            }
            #endif
        }

        #endregion

        #region Helper Methods
        private void SetMaterial(GameObject obj, Material material)
        {
            #if UNITY_EDITOR
            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
            #endif
        }

        private void ApplyTexture(ProBuilderMesh mesh, Material material)
        {
            if (mesh == null || material == null) return;

            // Apply material to all faces
            var faces = mesh.faces.ToArray();
            mesh.SetMaterial(faces, material);

            // Ensure UVs are properly set
            mesh.RefreshUV(faces);
            mesh.ToMesh();
            mesh.Refresh();
        }
        #endregion

        #region Editor Methods

        /// <summary>
        /// Creates all necessary materials for the environment
        /// </summary>
        public void CreateDefaultMaterials()
        {
            #if UNITY_EDITOR
            // Create street materials if they don't exist
            cobblestoneStreetMaterial = CreateMaterial("Street_Cobblestone", new Color(0.3f, 0.3f, 0.3f));
            woodenSidewalkMaterial = CreateMaterial("Sidewalk_Wood", new Color(0.6f, 0.4f, 0.2f));
            
            // Create building materials
            brickBuildingMaterial = CreateMaterial("Building_Brick", new Color(0.6f, 0.3f, 0.2f));
            woodBuildingMaterial = CreateMaterial("Building_Wood", new Color(0.7f, 0.5f, 0.3f));
            
            // Create tunnel materials
            tunnelStoneMaterial = CreateMaterial("Tunnel_Stone", new Color(0.3f, 0.3f, 0.3f));
            tunnelSupportMaterial = CreateMaterial("Tunnel_Wood", new Color(0.5f, 0.3f, 0.2f));
            #endif
        }

        private Material CreateMaterial(string name, Color color)
        {
            #if UNITY_EDITOR
            string path = $"Assets/Materials/Environment/{name}.mat";
            
            // Check if material already exists
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
                return material;
            
            // Ensure directory exists
            string directory = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            // Create new material
            material = new Material(Shader.Find("Standard"));
            material.color = color;

            // Save the material asset
            AssetDatabase.CreateAsset(material, path);
            AssetDatabase.SaveAssets();

            return material;
            #else
            return null;
            #endif
        }

        #endregion
    }
}
