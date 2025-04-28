using UnityEngine;
using UnityEditor;
using PDXUnderground.UI;

namespace PDXUnderground.UI.Editor
{
    /// <summary>
    /// Editor tools for creating and setting up UI prefabs for the Gambler character.
    /// </summary>
    public class GamblerUIEditorTools : EditorWindow
    {
        private GamblerUISetup uiSetupReference;
        
        [MenuItem("PDX Underground/UI Tools/UI Prefabs Creator")]
        public static void ShowWindow()
        {
            GetWindow<GamblerUIEditorTools>("Gambler UI Tools");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Gambler UI Prefab Creation Tools", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            uiSetupReference = EditorGUILayout.ObjectField(
                "UI Setup Reference", 
                uiSetupReference, 
                typeof(GamblerUISetup), 
                false) as GamblerUISetup;
                
            EditorGUILayout.HelpBox(
                "This tool helps create UI prefabs for the PDX Underground game. " +
                "You can either select an existing GamblerUISetup component or create " +
                "new prefabs using default settings.", 
                MessageType.Info);
                
            EditorGUILayout.Space();
            
            GUILayout.Label("Create Prefabs", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical("box");
            
            if (GUILayout.Button("Create Card Prefab"))
            {
                CreateCardPrefab();
            }
            
            if (GUILayout.Button("Create UI Canvas"))
            {
                CreateGamblerUICanvas();
            }
            
            if (GUILayout.Button("Create All Prefabs"))
            {
                CreateAllPrefabs();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void CreateCardPrefab()
        {
            if (uiSetupReference != null)
            {
                uiSetupReference.CreateCardPrefab();
            }
            else
            {
                // Create temporary setup
                GameObject tempObj = new GameObject("TempGamblerUISetup");
                GamblerUISetup setup = tempObj.AddComponent<GamblerUISetup>();
                setup.CreateCardPrefab();
                DestroyImmediate(tempObj);
            }
        }
        
        private void CreateGamblerUICanvas()
        {
            if (uiSetupReference != null)
            {
                uiSetupReference.CreateGamblerUICanvas();
            }
            else
            {
                // Create temporary setup
                GameObject tempObj = new GameObject("TempGamblerUISetup");
                GamblerUISetup setup = tempObj.AddComponent<GamblerUISetup>();
                setup.CreateGamblerUICanvas();
                DestroyImmediate(tempObj);
            }
        }
        
        private void CreateAllPrefabs()
        {
            if (uiSetupReference != null)
            {
                uiSetupReference.CreateAllPrefabs();
            }
            else
            {
                // Create temporary setup
                GameObject tempObj = new GameObject("TempGamblerUISetup");
                GamblerUISetup setup = tempObj.AddComponent<GamblerUISetup>();
                setup.CreateAllPrefabs();
                DestroyImmediate(tempObj);
            }
        }
    }
}

