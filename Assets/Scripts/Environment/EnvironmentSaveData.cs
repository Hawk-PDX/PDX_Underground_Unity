using UnityEngine;
using System;

/// <summary>
/// EnvironmentSaveData contains serializable data to save and load Portland environment layouts.
/// Used by PortlandEnvironmentSetup for saving/loading historical layouts.
/// </summary>
[Serializable]
public class EnvironmentSaveData
{
    // Basic configuration
    public Vector2Int gridSize;
    public float cellSize;
    
    // Object positions
    public Vector3[] buildingPositions;
    public Vector3[] gaslightPositions;
    public Vector3[] dockPositions;
    public Vector3[] spawnPoints;
    
    // Historical data
    public string layoutName;
    public string layoutDescription;
    public int historicalYear = 1880;
    
    // Optional terrain data
    public float[] terrainHeights;
    public int terrainResolution;
}

