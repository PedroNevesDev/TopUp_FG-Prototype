using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Dungeon/GroundTile")]
public class DungeonGroundTileData : ScriptableObject
{
    public string tileName;
    public GameObject prefab;  // Prefab for the ground tile
    
    // Connection rules
    public List<string> validAbove = new List<string>();
    public List<string> validBelow = new List<string>();
    public List<string> validRight = new List<string>();
    public List<string> validLeft = new List<string>();
    public List<string> validForward = new List<string>();
    public List<string> validBackward = new List<string>();

    // Optional connection rotation overrides
    [Header("Connection Rotation Overrides")]
    public bool useCustomRotations = false;
    
    [System.Serializable]
    public class ConnectionRotation
    {
        public string neighborTileName;
        public string direction; // Left, Right, Forward, Backward
        public float rotationAngle;
    }
    
    public List<ConnectionRotation> customRotations = new List<ConnectionRotation>();
    
    // List of placeholders for object placement
    [Header("Object Placeholders")]
    public List<PlaceholderData> placeholders = new List<PlaceholderData>();
    
    // Get custom rotation for a specific neighbor and direction
    public float GetCustomRotation(string neighborTileName, string direction)
    {
        if (!useCustomRotations) return 0f;
        
        foreach (var rotation in customRotations)
        {
            if (rotation.neighborTileName == neighborTileName && rotation.direction == direction)
                return rotation.rotationAngle;
        }
        
        return 0f; // Default rotation if no custom one is found
    }
}

