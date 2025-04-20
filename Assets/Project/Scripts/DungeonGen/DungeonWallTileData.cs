using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Dungeon/DungeonWallTile")]
public class DungeonWallTileData : ScriptableObject
{
    public string tileName; // Unique identifier for the wall tile
    public GameObject prefab; // General wall prefab (used if specific ones aren't set)
    
    // Wall-specific prefabs for different orientations
    public GameObject leftWallPrefab;  // Wall facing left
    public GameObject rightWallPrefab; // Wall facing right
    
    // Can this wall connect to ground tiles?
    public bool canConnectToGround = true;
    
    // Wall-to-wall connection rules
    public List<string> validLeftWalls = new List<string>();
    public List<string> validRightWalls = new List<string>();
    public List<string> validForwardWalls = new List<string>();
    public List<string> validBackwardWalls = new List<string>();
    
    // Optional custom rotations for specific connections
    [Header("Connection Rotation Overrides")]
    public bool useCustomRotations = false;
    
    [System.Serializable]
    public class WallConnectionRotation
    {
        public string neighborWallName;
        public string direction; // Left, Right, Forward, Backward
        public float rotationAngle;
    }
    
    public List<WallConnectionRotation> customRotations = new List<WallConnectionRotation>();
    
    // Get custom rotation for a specific wall neighbor and direction
    public float GetCustomRotation(string neighborWallName, string direction)
    {
        if (!useCustomRotations) return 0f;
        
        foreach (var rotation in customRotations)
        {
            if (rotation.neighborWallName == neighborWallName && rotation.direction == direction)
                return rotation.rotationAngle;
        }
        
        return 0f; // Default rotation if no custom one is found
    }
}

