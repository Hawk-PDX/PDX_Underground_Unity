using UnityEngine;
using UnityEditor;

namespace PDXUnderground.UI
{
    /// <summary>
    /// Utility to create placeholder icons for testing the TimeWeatherDisplay
    /// This creates simple programmatic sprites representing different weather conditions and time of day
    /// </summary>
    public class PlaceholderIconCreator : EditorWindow
    {
        [MenuItem("PDX Underground/Utilities/Create Placeholder Icons")]
        public static void ShowWindow()
        {
            GetWindow<PlaceholderIconCreator>("Create Icons");
        }

        private void OnGUI()
        {
            GUILayout.Label("Create Placeholder Icons", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("This utility will create simple placeholder icons for time and weather", MessageType.Info);

            GUILayout.Space(10);

            if (GUILayout.Button("Create Icons", GUILayout.Height(30)))
            {
                CreateIcons();
            }
        }

        private void CreateIcons()
        {
            // Create directory if it doesn't exist
            string directory = "Assets/Textures/UI";
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            // Create day icon (sun)
            Texture2D dayTexture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            CreateSunIcon(dayTexture);
            SaveTextureAsSprite(dayTexture, directory + "/DayIcon.png");

            // Create night icon (moon)
            Texture2D nightTexture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            CreateMoonIcon(nightTexture);
            SaveTextureAsSprite(nightTexture, directory + "/NightIcon.png");

            // Create clear icon (sun with blue sky)
            Texture2D clearTexture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            CreateClearIcon(clearTexture);
            SaveTextureAsSprite(clearTexture, directory + "/ClearIcon.png");

            // Create windy icon (clouds with wind lines)
            Texture2D windyTexture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            CreateWindyIcon(windyTexture);
            SaveTextureAsSprite(windyTexture, directory + "/WindyIcon.png");

            // Create stormy icon (clouds with lightning)
            Texture2D stormyTexture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            CreateStormyIcon(stormyTexture);
            SaveTextureAsSprite(stormyTexture, directory + "/StormyIcon.png");

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Icons Created", "Placeholder icons have been created in " + directory, "OK");
        }

        private void CreateSunIcon(Texture2D texture)
        {
            // Fill with transparent
            FillTexture(texture, new Color(0, 0, 0, 0));

            // Draw sun circle
            Color sunColor = new Color(1f, 0.9f, 0.2f, 1f);
            DrawCircle(texture, 64, 64, 40, sunColor);

            // Draw sun rays
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI / 4f;
                int x1 = 64 + Mathf.RoundToInt(Mathf.Cos(angle) * 45);
                int y1 = 64 + Mathf.RoundToInt(Mathf.Sin(angle) * 45);
                int x2 = 64 + Mathf.RoundToInt(Mathf.Cos(angle) * 60);
                int y2 = 64 + Mathf.RoundToInt(Mathf.Sin(angle) * 60);
                DrawLine(texture, x1, y1, x2, y2, 3, sunColor);
            }

            // Apply changes
            texture.Apply();
        }

        private void CreateMoonIcon(Texture2D texture)
        {
            // Fill with transparent
            FillTexture(texture, new Color(0, 0, 0, 0));

            // Draw moon
            Color moonColor = new Color(0.9f, 0.9f, 1f, 1f);
            DrawCircle(texture, 64, 64, 40, moonColor);

            // Draw shadow to create crescent
            Color shadowColor = new Color(0, 0, 0, 0);
            DrawCircle(texture, 74, 64, 35, shadowColor);

            // Draw stars
            Color starColor = new Color(1f, 1f, 1f, 0.8f);
            int[] starX = { 20, 100, 30, 110, 50, 90 };
            int[] starY = { 30, 40, 90, 80, 20, 100 };
            
            for (int i = 0; i < starX.Length; i++)
            {
                DrawStar(texture, starX[i], starY[i], 2, starColor);
            }

            // Apply changes
            texture.Apply();
        }

        private void CreateClearIcon(Texture2D texture)
        {
            // Fill with light blue sky
            FillTexture(texture, new Color(0.5f, 0.8f, 1f, 1f));

            // Draw sun
            Color sunColor = new Color(1f, 0.9f, 0.2f, 1f);
            DrawCircle(texture, 90, 30, 25, sunColor);

            // Apply changes
            texture.Apply();
        }

        private void CreateWindyIcon(Texture2D texture)
        {
            // Fill with light blue sky
            FillTexture(texture, new Color(0.5f, 0.8f, 1f, 1f));

            // Draw clouds
            Color cloudColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            DrawCircle(texture, 40, 40, 20, cloudColor);
            DrawCircle(texture, 60, 30, 25, cloudColor);
            DrawCircle(texture, 80, 40, 20, cloudColor);

            // Draw wind lines
            Color windColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            DrawLine(texture, 20, 70, 50, 70, 5, windColor);
            DrawLine(texture, 40, 90, 80, 90, 5, windColor);
            DrawLine(texture, 60, 110, 100, 110, 5, windColor);

            // Apply changes
            texture.Apply();
        }

        private void CreateStormyIcon(Texture2D texture)
        {
            // Fill with dark blue sky
            FillTexture(texture, new Color(0.2f, 0.2f, 0.4f, 1f));

            // Draw clouds
            Color cloudColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            DrawCircle(texture, 40, 40, 30, cloudColor);
            DrawCircle(texture, 80, 30, 35, cloudColor);
            DrawCircle(texture, 60, 40, 25, cloudColor);

            // Draw lightning bolt
            Color lightningColor = new Color(1f, 1f, 0.4f, 1f);
            int[] xPoints = { 70, 60, 75, 55, 65 };
            int[] yPoints = { 50, 70, 80, 100, 120 };
            
            for (int i = 0; i < xPoints.Length - 1; i++)
            {
                DrawLine(texture, xPoints[i], yPoints[i], xPoints[i+1], yPoints[i+1], 3, lightningColor);
            }

            // Apply changes
            texture.Apply();
        }

        // Helper methods for drawing
        private void FillTexture(Texture2D texture, Color color)
        {
            Color[] pixels = new Color[texture.width * texture.height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            texture.SetPixels(pixels);
        }

        private void DrawCircle(Texture2D texture, int centerX, int centerY, int radius, Color color)
        {
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    float distance = Mathf.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));
                    if (distance <= radius)
                    {
                        texture.SetPixel(x, y, color);
                    }
                }
            }
        }

        private void DrawLine(Texture2D texture, int x1, int y1, int x2, int y2, int thickness, Color color)
        {
            int dx = x2 - x1;
            int dy = y2 - y1;
            float length = Mathf.Sqrt(dx * dx + dy * dy);
            float angle = Mathf.Atan2(dy, dx);
            
            for (int i = 0; i < length; i++)
            {
                int x = x1 + Mathf.RoundToInt(i * Mathf.Cos(angle));
                int y = y1 + Mathf.RoundToInt(i * Mathf.Sin(angle));
                
                for (int t = -thickness/2; t <= thickness/2; t++)
                {
                    for (int s = -thickness/2; s <= thickness/2; s++)
                    {
                        int drawX = x + t;
                        int drawY = y + s;
                        
                        if (drawX >= 0 && drawX < texture.width && drawY >= 0 && drawY < texture.height)
                        {
                            texture.SetPixel(drawX, drawY, color);
                        }
                    }
                }
            }
        }

        private void DrawStar(Texture2D texture, int centerX, int centerY, int size, Color color)
        {
            for (int y = centerY - size; y <= centerY + size; y++)
            {
                for (int x = centerX - size; x <= centerX + size; x++)
                {
                    if (x >= 0 && x < texture.width && y >= 0 && y < texture.height)
                    {
                        texture.SetPixel(x, y, color);
                    }
                }
            }
        }

        private void SaveTextureAsSprite(Texture2D texture, string path)
        {
            byte[] bytes = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, bytes);
            
            // Import as sprite
            AssetDatabase.ImportAsset(path);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.filterMode = FilterMode.Bilinear;
                importer.spriteImportMode = SpriteImportMode.Single;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }
    }
}

