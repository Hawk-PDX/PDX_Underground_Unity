using UnityEngine;
using UnityEditor;
using System.IO;

namespace PDXUnderground.Editor
{
    /// <summary>
    /// Editor utility for generating placeholder UI textures and materials
    /// </summary>
    public class UIResourceGenerator : MonoBehaviour
    {
        #region Texture Settings
        // Texture sizes
        private static readonly Vector2Int cardFrameSize = new Vector2Int(180, 250);
        private static readonly Vector2Int cardBgSize = new Vector2Int(160, 230);
        private static readonly Vector2Int iconSize = new Vector2Int(40, 40);
        private static readonly Vector2Int meterFrameSize = new Vector2Int(300, 30);
        private static readonly Vector2Int meterFillSize = new Vector2Int(296, 26);
        private static readonly Vector2Int cooldownOverlaySize = new Vector2Int(180, 250);
        private static readonly Vector2Int debugBgSize = new Vector2Int(400, 600);
        
        // Texture colors
        private static readonly Color cardFrameColor = new Color(0.8f, 0.8f, 0.8f);
        private static readonly Color cardBgColor = new Color(0.15f, 0.15f, 0.15f);
        private static readonly Color attackIconColor = new Color(0.8f, 0.2f, 0.2f);
        private static readonly Color defenseIconColor = new Color(0.2f, 0.4f, 0.8f);
        private static readonly Color recoveryIconColor = new Color(0.2f, 0.8f, 0.4f);
        private static readonly Color specialIconColor = new Color(0.8f, 0.6f, 0.0f);
        private static readonly Color meterFrameColor = new Color(0.2f, 0.2f, 0.2f);
        private static readonly Color meterFillColor = new Color(0.8f, 0.6f, 0.2f);
        private static readonly Color cooldownColor = new Color(0.0f, 0.0f, 0.0f, 0.6f);
        private static readonly Color debugBgColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        #endregion
        
        #region Directory Paths
        private static readonly string texturesDir = "Assets/Resources/Textures/UI";
        private static readonly string materialsDir = "Assets/Resources/Materials/UI";
        private static readonly string prefabsDir = "Assets/Resources/Prefabs/UI";
        #endregion
        
        #region Generation Methods
        
        /// <summary>
        /// Creates all texture directories if they don't exist
        /// </summary>
        private static void CreateDirectories()
        {
            if (!Directory.Exists(texturesDir))
            {
                Directory.CreateDirectory(texturesDir);
            }
            
            if (!Directory.Exists(materialsDir))
            {
                Directory.CreateDirectory(materialsDir);
            }
            
            if (!Directory.Exists(prefabsDir))
            {
                Directory.CreateDirectory(prefabsDir);
            }
        }
        
        /// <summary>
        /// Generates all UI textures
        /// </summary>
        public static void GenerateAllTextures()
        {
            CreateDirectories();
            
            // Generate card textures
            GenerateCardFrame();
            GenerateCardBackground();
            
            // Generate type icons
            GenerateTypeIcon("AttackIcon", attackIconColor);
            GenerateTypeIcon("DefenseIcon", defenseIconColor);
            GenerateTypeIcon("RecoveryIcon", recoveryIconColor);
            GenerateTypeIcon("SpecialIcon", specialIconColor);
            
            // Generate meter textures
            GenerateMeterFrame();
            GenerateMeterFill();
            
            // Generate cooldown overlay
            GenerateCooldownOverlay();
            
            // Generate debug panel background
            GenerateDebugBackground();
            
            AssetDatabase.Refresh();
        }
        
        /// <summary>
        /// Generates a card frame texture
        /// </summary>
        private static void GenerateCardFrame()
        {
            Texture2D texture = new Texture2D(cardFrameSize.x, cardFrameSize.y);
            
            // Fill texture with transparent pixels
            Color[] pixels = new Color[cardFrameSize.x * cardFrameSize.y];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }
            
            // Draw frame border (5px thick)
            int borderThickness = 5;
            
            // Draw horizontal borders
            for (int x = 0; x < cardFrameSize.x; x++)
            {
                for (int y = 0; y < borderThickness; y++)
                {
                    // Bottom border
                    pixels[y * cardFrameSize.x + x] = cardFrameColor;
                    
                    // Top border
                    pixels[(cardFrameSize.y - 1 - y) * cardFrameSize.x + x] = cardFrameColor;
                }
            }
            
            // Draw vertical borders
            for (int y = 0; y < cardFrameSize.y; y++)
            {
                for (int x = 0; x < borderThickness; x++)
                {
                    // Left border
                    pixels[y * cardFrameSize.x + x] = cardFrameColor;
                    
                    // Right border
                    pixels[y * cardFrameSize.x + (cardFrameSize.x - 1 - x)] = cardFrameColor;
                }
            }
            
            // Draw a header area
            for (int y = cardFrameSize.y - borderThickness - 30; y < cardFrameSize.y - borderThickness; y++)
            {
                for (int x = borderThickness; x < cardFrameSize.x - borderThickness; x++)
                {
                    pixels[y * cardFrameSize.x + x] = new Color(cardFrameColor.r, cardFrameColor.g, cardFrameColor.b, 0.5f);
                }
            }
            
            // Draw a footer area
            for (int y = borderThickness; y < borderThickness + 20; y++)
            {
                for (int x = borderThickness; x < cardFrameSize.x - borderThickness; x++)
                {
                    pixels[y * cardFrameSize.x + x] = new Color(cardFrameColor.r, cardFrameColor.g, cardFrameColor.b, 0.5f);
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            // Save texture to file
            SaveTextureToFile(texture, "CardFrame");
        }
        
        /// <summary>
        /// Generates a card background texture
        /// </summary>
        private static void GenerateCardBackground()
        {
            Texture2D texture = new Texture2D(cardBgSize.x, cardBgSize.y);
            
            // Fill with background color
            Color[] pixels = new Color[cardBgSize.x * cardBgSize.y];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = cardBgColor;
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            // Save texture to file
            SaveTextureToFile(texture, "CardBackground");
        }
        
        /// <summary>
        /// Generates a card type icon texture
        /// </summary>
        private static void GenerateTypeIcon(string name, Color color)
        {
            Texture2D texture = new Texture2D(iconSize.x, iconSize.y);
            
            // Fill with transparent
            Color[] pixels = new Color[iconSize.x * iconSize.y];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }
            
            // Draw the icon (a simple shape based on the type)
            int padding = 4;
            int centerX = iconSize.x / 2;
            int centerY = iconSize.y / 2;
            int radius = (iconSize.x / 2) - padding;
            
            for (int y = 0; y < iconSize.y; y++)
            {
                for (int x = 0; x < iconSize.x; x++)
                {
                    float distanceFromCenter = Mathf.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));
                    
                    if (distanceFromCenter <= radius)
                    {
                        pixels[y * iconSize.x + x] = color;
                    }
                }
            }
            
            // Add distinctive shapes inside the circle based on the type
            if (name == "AttackIcon")
            {
                // Draw a cross shape
                for (int y = centerY - radius/2; y <= centerY + radius/2; y++)
                {
                    for (int x = centerX - 2; x <= centerX + 2; x++)
                    {
                        if (y >= 0 && y < iconSize.y && x >= 0 && x < iconSize.x)
                        {
                            pixels[y * iconSize.x + x] = Color.white;
                        }
                    }
                }
                
                for (int x = centerX - radius/2; x <= centerX + radius/2; x++)
                {
                    for (int y = centerY - 2; y <= centerY + 2; y++)
                    {
                        if (y >= 0 && y < iconSize.y && x >= 0 && x < iconSize.x)
                        {
                            pixels[y * iconSize.x + x] = Color.white;
                        }
                    }
                }
            }
            else if (name == "DefenseIcon")
            {
                // Draw a shield shape
                for (int y = centerY - radius/2; y < centerY + radius/2; y++)
                {
                    for (int x = centerX - radius/2; x < centerX + radius/2; x++)
                    {
                        if (y >= 0 && y < iconSize.y && x >= 0 && x < iconSize.x)
                        {
                            if (y <= centerY + (x - centerX) && y <= centerY - (x - centerX))
                            {
                                pixels[y * iconSize.x + x] = Color.white;
                            }
                        }
                    }
                }
            }
            else if (name == "RecoveryIcon")
            {
                // Draw a plus shape
                for (int y = centerY - radius/2; y <= centerY + radius/2; y++)
                {
                    for (int x = centerX - 2; x <= centerX + 2; x++)
                    {
                        if (y >= 0 && y < iconSize.y && x >= 0 && x < iconSize.x)
                        {
                            pixels[y * iconSize.x + x] = Color.white;
                        }
                    }
                }
                
                for (int x = centerX - radius/2; x <= centerX + radius/2; x++)
                {
                    for (int y = centerY - 2; y <= centerY + 2; y++)
                    {
                        if (y >= 0 && y < iconSize.y && x >= 0 && x < iconSize.x)
                        {
                            pixels[y * iconSize.x + x] = Color.white;
                        }
                    }
                }
            }
            else if (name == "SpecialIcon")
            {
                // Draw a star shape (simplified)
                for (int i = 0; i < 360; i += 72)
                {
                    float rad = i * Mathf.Deg2Rad;
                    int x1 = centerX + Mathf.RoundToInt(radius * 0.4f * Mathf.Cos(rad));
                    int y1 = centerY + Mathf.RoundToInt(radius * 0.4f * Mathf.Sin(rad));
                    
                    float rad2 = (i + 36) * Mathf.Deg2Rad;
                    int x2 = centerX + Mathf.RoundToInt(radius * 0.8f * Mathf.Cos(rad2));
                    int y2 = centerY + Mathf.RoundToInt(radius * 0.8f * Mathf.Sin(rad2));
                    
                    // Draw line from center to point
                    DrawLine(texture, pixels, centerX, centerY, x1, y1, Color.white);
                    DrawLine(texture, pixels, x1, y1, x2, y2, Color.white);
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            // Save texture to file
            SaveTextureToFile(texture, name);
        }
        
        /// <summary>
        /// Generates a meter frame texture
        /// </summary>
        private static void GenerateMeterFrame()
        {
            Texture2D texture = new Texture2D(meterFrameSize.x, meterFrameSize.y);
            
            // Fill with frame color
            Color[] pixels = new Color[meterFrameSize.x * meterFrameSize.y];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = meterFrameColor;
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            // Save texture to file
            SaveTextureToFile(texture, "MeterFrame");
        }
        
        /// <summary>
        /// Generates a meter fill texture
        /// </summary>
        private static void GenerateMeterFill()
        {
            Texture2D texture = new Texture2D(meterFillSize.x, meterFillSize.y);
            
            // Fill with meter fill color
            Color[] pixels = new Color[meterFillSize.x * meterFillSize.y];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = meterFillColor;
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            // Save texture to file
            SaveTextureToFile(texture, "MeterFill");
        }
        
        /// <summary>
        /// Generates a cooldown overlay texture
        /// </summary>
        private static void GenerateCooldownOverlay()
        {
            Texture2D texture = new Texture2D(cooldownOverlaySize.x, cooldownOverlaySize.y);
            
            // Create a radial gradient
            Color[] pixels = new Color[cooldownOverlaySize.x * cooldownOverlaySize.y];
            int centerX = cooldownOverlaySize.x / 2;
            int centerY = cooldownOverlaySize.y / 2;
            
            for (int y = 0; y < cooldownOverlaySize.y; y++)
            {
                for (int x = 0; x < cooldownOverlaySize.x; x++)
                {
                    float distanceFromCenter = Mathf.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));
                    float maxDistance = Mathf.Sqrt(centerX * centerX + centerY * centerY);
                    
                    // Create a semi-transparent gradient that fades from center
                    float alpha = cooldownColor.a * (1f - (distanceFromCenter / maxDistance));
                    pixels[y * cooldownOverlaySize.x + x] = new Color(
                        cooldownColor.r, 
                        cooldownColor.g, 
                        cooldownColor.b, 
                        alpha);
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            // Save texture to file
            SaveTextureToFile(texture, "CooldownOverlay");
        }
        
        /// <summary>
        /// Generates a debug panel background texture
        /// </summary>
        private static void GenerateDebugBackground()
        {
            Texture2D texture = new Texture2D(debugBgSize.x, debugBgSize.y);
            
            // Fill with debug background color
            Color[] pixels = new Color[debugBgSize.x * debugBgSize.y];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = debugBgColor;
            }
            
            // Add a border
            int borderThickness = 2;
            
            // Draw horizontal borders
            for (int x = 0; x < debugBgSize.x; x++)
            {
                for (int y = 0; y < borderThickness; y++)
                {
                    // Bottom border
                    pixels[y * debugBgSize.x + x] = Color.white;
                    
                    // Top border
                    pixels[(debugBgSize.y - 1 - y) * debugBgSize.x + x] = Color.white;
                }
            }
            
            // Draw vertical borders
            for (int y = 0; y < debugBgSize.y; y++)
            {
                for (int x = 0; x < borderThickness; x++)
                {
                    // Left border
                    pixels[y * debugBgSize.x + x] = Color.white;
                    
                    // Right border
                    pixels[y * debugBgSize.x + (debugBgSize.x - 1 - x)] = Color.white;
                }
            }
            
            // Add section dividers
            int third = debugBgSize.y / 3;
            for (int x = 0; x < debugBgSize.x; x++)
            {
                for (int y = 0; y < borderThickness; y++)
                {
                    // Divider at 1/3
                    pixels[(third + y) * debugBgSize.x + x] = new Color(1f, 1f, 1f, 0.5f);
                    
                    // Divider at 2/3
                    pixels[(2 * third + y) * debugBgSize.x + x] = new Color(1f, 1f, 1f, 0.5f);
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            // Save texture to file
            SaveTextureToFile(texture, "DebugBackground");
        }
        
        /// <summary>
        /// Saves a texture to a file in the textures directory
        /// </summary>
        private static void SaveTextureToFile(Texture2D texture, string name)
        {
            // Create the textures directory if it doesn't exist
            if (!Directory.Exists(texturesDir))
            {
                Directory.CreateDirectory(texturesDir);
            }
            
            // Encode the texture as PNG
            byte[] bytes = texture.EncodeToPNG();
            
            // Save the texture to file
            string path = $"{texturesDir}/{name}.png";
            File.WriteAllBytes(path, bytes);
            
            // Import the texture asset
            AssetDatabase.ImportAsset(path);
            
            // Configure texture import settings
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                
                // Apply import settings
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }
            
            Debug.Log($"Generated texture: {name}.png");
        }
        
        /// <summary>
        /// Draws a line on a texture
        /// </summary>
        private static void DrawLine(Texture2D texture, Color[] pixels, int x0, int y0, int x1, int y1, Color color)
        {
            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;
            
            while (true)
            {
                // Set pixel if it's within bounds
                if (x0 >= 0 && x0 < texture.width && y0 >= 0 && y0 < texture.height)
                {
                    pixels[y0 * texture.width + x0] = color;
                }
                
                // Break if we've reached the endpoint
                if (x0 == x1 && y0 == y1) break;
                
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }
        
        /// <summary>
        /// Creates materials from the generated textures
        /// </summary>
        public static void GenerateMaterials()
        {
            // Create the materials directory if it doesn't exist
            if (!Directory.Exists(materialsDir))
            {
                Directory.CreateDirectory(materialsDir);
            }
            
            // Create UI materials with appropriate shaders
            CreateMaterial("CardFrame", "UI/Default");
            CreateMaterial("CardBackground", "UI/Default");
            CreateMaterial("AttackIcon", "UI/Default");
            CreateMaterial("DefenseIcon", "UI/Default");
            CreateMaterial("RecoveryIcon", "UI/Default");
            CreateMaterial("SpecialIcon", "UI/Default");
            CreateMaterial("MeterFrame", "UI/Default");
            CreateMaterial("MeterFill", "UI/Default");
            CreateMaterial("CooldownOverlay", "UI/Default");
            CreateMaterial("DebugBackground", "UI/Default");
            
            AssetDatabase.Refresh();
        }
        
        /// <summary>
        /// Creates a material from a texture
        /// </summary>
        private static void CreateMaterial(string textureName, string shaderName)
        {
            // Check if the texture exists
            string texturePath = $"{texturesDir}/{textureName}.png";
            if (!File.Exists(texturePath))
            {
                Debug.LogWarning($"Texture not found: {texturePath}");
                return;
            }
            
            // Create the material
            Material material = new Material(Shader.Find(shaderName));
            material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            
            // Save the material to file
            string materialPath = $"{materialsDir}/{textureName}Material.mat";
            AssetDatabase.CreateAsset(material, materialPath);
            
            Debug.Log($"Generated material: {textureName}Material.mat");
        }
        #endregion
        
        #region Menu Items
        [MenuItem("PDX Underground/Generate UI Resources/Generate All")]
        public static void GenerateAll()
        {
            GenerateAllTextures();
            GenerateMaterials();
            
            Debug.Log("All UI resources generated successfully!");
        }
        
        [MenuItem("PDX Underground/Generate UI Resources/Generate Textures Only")]
        public static void GenerateTexturesMenuItem()
        {
            GenerateAllTextures();
            Debug.Log("UI textures generated successfully!");
        }
        
        [MenuItem("PDX Underground/Generate UI Resources/Generate Materials Only")]
        public static void GenerateMaterialsMenuItem()
        {
            GenerateMaterials();
            Debug.Log("UI materials generated successfully!");
        }
        
        [MenuItem("PDX Underground/Generate UI Resources/Create Debug UI Prefab")]
        public static void CreateDebugUIPrefab()
        {
            // Create the prefabs directory if it doesn't exist
            if (!Directory.Exists(prefabsDir))
            {
                Directory.CreateDirectory(prefabsDir);
            }
            
            // Create a debug panel game object
            GameObject debugPanelObj = new GameObject("DebugPanel");
            RectTransform rectTransform = debugPanelObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(400, 600);
            
            // Add background image
            Image bgImage = debugPanelObj.AddComponent<Image>();
            bgImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{texturesDir}/DebugBackground.png");
            bgImage.color = new Color(1f, 1f, 1f, 0.8f);
            
            // Add debug panel component
            debugPanelObj.AddComponent<PDXUnderground.Test.DebugPanel>();
            
            // Save as prefab
            string prefabPath = $"{prefabsDir}/DebugPanel.prefab";
            
#if UNITY_2018_3_OR_NEWER
            PrefabUtility.SaveAsPrefabAsset(debugPanelObj, prefabPath);
#else
            PrefabUtility.CreatePrefab(prefabPath, debugPanelObj);
#endif
            
            // Clean up
            Object.DestroyImmediate(debugPanelObj);
            
            Debug.Log($"Debug UI prefab created at: {prefabPath}");
        }
        #endregion
    }
}
