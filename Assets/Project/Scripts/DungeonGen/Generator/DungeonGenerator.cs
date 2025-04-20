using UnityEngine;
using System.Collections.Generic;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Dungeon Size")]
    public int width = 10;
    public int height = 1;
    public int depth = 10;

    [Header("Tiles & Props")]
    public List<DungeonGroundTileData> groundTiles;  // Ground tiles
    public List<DungeonWallTileData> wallTiles;  // Wall tiles
    public List<DungeonObjectTileData> dungeonObjects;  // Dungeon objects to place

    [Header("Settings")]
    public bool hasWalls = true;
    public float tileSize = 10f;

    private Dictionary<Vector3Int, GameObject> placedTiles = new();
    private Dictionary<Vector3Int, TileReference> tileReferences = new();
    private HashSet<Vector3Int> occupiedPlaceholders = new();

    void Start() 
    {
        GenerateDungeon();    
    }

    public void GenerateDungeon()
    {
        ClearDungeon();

        // Ground pass
        for (int y = 0; y < height; y++)
        {
            for (int z = 0; z < depth; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector3Int pos = new(x, y, z);
                    var tileData = PickValidGroundTile(pos);
                    if (tileData != null)
                    {
                        Vector3 worldPos = new(pos.x * tileSize, pos.y * tileSize, pos.z * tileSize);
                        GameObject tile = Instantiate(tileData.prefab, worldPos, Quaternion.Euler(0, GetTileRotation(pos, tileData), 0), transform);
                        tile.name = $"Tile_{x}_{y}_{z}";
                        
                        // Add reference component to track tile data
                        var tileRef = tile.AddComponent<TileReference>();
                        tileRef.groundTileData = tileData;
                        
                        placedTiles[pos] = tile;
                        tileReferences[pos] = tileRef;

                        PlaceObjectsOnTile(tile, tileData, pos);
                    }
                }
            }
        }

        // Wall pass
        if (hasWalls)
        {
            List<Vector3Int> keys = new(placedTiles.Keys);
            foreach (var pos in keys)
            {
                if (!IsBorderTile(pos)) continue;

                var tileData = PickValidWallTile(pos);
                if (tileData != null)
                    PlaceWalls(pos, tileData);
            }
        }
    }

    public void ClearDungeon()
    {
        foreach (Transform child in transform)
            DestroyImmediate(child.gameObject);
        placedTiles.Clear();
        tileReferences.Clear();
        occupiedPlaceholders.Clear();
    }

    // Handle rotation based on neighbors
    float GetTileRotation(Vector3Int pos, DungeonGroundTileData tileData)
    {
        // Check neighboring tiles to determine appropriate rotation
        // This is a simple example - expand based on your specific rotation needs
        bool hasLeft = placedTiles.ContainsKey(pos + Vector3Int.left);
        bool hasRight = placedTiles.ContainsKey(pos + Vector3Int.right);
        bool hasForward = placedTiles.ContainsKey(pos + new Vector3Int(0, 0, 1));
        bool hasBackward = placedTiles.ContainsKey(pos + new Vector3Int(0, 0, -1));
        
        // Simple rotation logic (expand this based on your needs)
        if (hasLeft && !hasRight && !hasForward && !hasBackward) return 90;
        if (!hasLeft && hasRight && !hasForward && !hasBackward) return 270;
        if (!hasLeft && !hasRight && hasForward && !hasBackward) return 180;
        if (!hasLeft && !hasRight && !hasForward && hasBackward) return 0;
        
        // Corner cases
        if (hasLeft && hasForward && !hasRight && !hasBackward) return 135;
        if (hasRight && hasForward && !hasLeft && !hasBackward) return 225;
        if (hasRight && hasBackward && !hasLeft && !hasForward) return 315;
        if (hasLeft && hasBackward && !hasRight && !hasForward) return 45;
        
        // If we have a custom rotation based on neighbor tile types, apply it
        return ApplyCustomRotation(pos, tileData);
    }
    
    float ApplyCustomRotation(Vector3Int pos, DungeonGroundTileData tileData)
    {
        // Check each neighbor and apply custom rotation rules
        Vector3Int[] neighborPositions = {
            pos + Vector3Int.left,        // Left
            pos + Vector3Int.right,       // Right
            pos + new Vector3Int(0, 0, 1), // Forward
            pos + new Vector3Int(0, 0, -1) // Backward
        };
        
        string[] directions = { "Left", "Right", "Forward", "Backward" };
        
        for (int i = 0; i < neighborPositions.Length; i++)
        {
            Vector3Int neighborPos = neighborPositions[i];
            if (tileReferences.TryGetValue(neighborPos, out TileReference neighborRef))
            {
                // Check if there's a custom rotation rule for this neighbor
                if (neighborRef.groundTileData != null)
                {
                    string neighborTileName = neighborRef.groundTileData.tileName;
                    
                    // This is where you'd implement the custom rotation logic
                    // based on specific tile combinations
                    
                    // Example for a simple case - can be expanded:
                    if (tileData.tileName == "CornerTile" && neighborTileName == "CorridorTile")
                    {
                        // Rotate the corner to match the corridor
                        if (directions[i] == "Left") return 90;
                        if (directions[i] == "Right") return 270;
                        if (directions[i] == "Forward") return 180;
                        if (directions[i] == "Backward") return 0;
                    }
                }
            }
        }
        
        // Default rotation if no custom rule matches
        return 0;
    }

    DungeonGroundTileData PickValidGroundTile(Vector3Int pos)
    {
        List<DungeonGroundTileData> validTiles = new List<DungeonGroundTileData>(groundTiles);
        
        // Check neighbors in all four directions
        CheckNeighborTiles(pos, ref validTiles);
        
        // If no valid tiles found, return null
        if (validTiles.Count == 0)
            return null;
        
        // Return a random valid tile
        return validTiles[Random.Range(0, validTiles.Count)];
    }
    
    void CheckNeighborTiles(Vector3Int pos, ref List<DungeonGroundTileData> validTiles)
    {
        // Check tile to the left
        CheckNeighbor(pos + Vector3Int.left, "Left", ref validTiles);
        
        // Check tile to the right
        CheckNeighbor(pos + Vector3Int.right, "Right", ref validTiles);
        
        // Check tile forward (z+1)
        CheckNeighbor(pos + new Vector3Int(0, 0, 1), "Forward", ref validTiles);
        
        // Check tile backward (z-1)
        CheckNeighbor(pos + new Vector3Int(0, 0, -1), "Backward", ref validTiles);
        
        // Check tile above (y+1)
        CheckNeighbor(pos + Vector3Int.up, "Above", ref validTiles);
        
        // Check tile below (y-1)
        CheckNeighbor(pos + Vector3Int.down, "Below", ref validTiles);
    }
    
    void CheckNeighbor(Vector3Int neighborPos, string direction, ref List<DungeonGroundTileData> validTiles)
    {
        if (tileReferences.TryGetValue(neighborPos, out TileReference neighborRef))
        {
            if (neighborRef.groundTileData != null)
            {
                string neighborTileName = neighborRef.groundTileData.tileName;
                
                // Filter tiles based on directional validation
                switch (direction)
                {
                    case "Left":
                        validTiles = validTiles.FindAll(t => t.validLeft.Contains(neighborTileName));
                        break;
                    case "Right":
                        validTiles = validTiles.FindAll(t => t.validRight.Contains(neighborTileName));
                        break;
                    case "Forward":
                        validTiles = validTiles.FindAll(t => t.validForward.Contains(neighborTileName));
                        break;
                    case "Backward":
                        validTiles = validTiles.FindAll(t => t.validBackward.Contains(neighborTileName));
                        break;
                    case "Above":
                        validTiles = validTiles.FindAll(t => t.validAbove.Contains(neighborTileName));
                        break;
                    case "Below":
                        validTiles = validTiles.FindAll(t => t.validBelow.Contains(neighborTileName));
                        break;
                }
            }
        }
    }

    DungeonWallTileData PickValidWallTile(Vector3Int pos)
    {
        List<DungeonWallTileData> candidates = new List<DungeonWallTileData>();
        
        foreach (var wallTile in wallTiles)
        {
            // Check if this wall tile can connect to adjacent tiles
            bool isValid = true;
            
            // Check if wall can connect to the left
            Vector3Int leftPos = pos + Vector3Int.left;
            if (tileReferences.TryGetValue(leftPos, out TileReference leftRef))
            {
                // If left tile is a ground tile, make sure our wall connects to it
                if (leftRef.groundTileData != null && 
                    !wallTile.validLeftWalls.Contains(leftRef.groundTileData.tileName))
                {
                    isValid = false;
                }
            }
            
            // Similar checks for right, forward, backward
            // Add these as needed based on your wall placement rules
            
            if (isValid)
                candidates.Add(wallTile);
        }
        
        return candidates.Count > 0 ? candidates[Random.Range(0, candidates.Count)] : null;
    }

    void PlaceWalls(Vector3Int pos, DungeonWallTileData wallTile)
    {
        // Check the four cardinal directions for wall placement
        PlaceWallInDirection(pos, Vector3Int.left, wallTile, "Left");
        PlaceWallInDirection(pos, Vector3Int.right, wallTile, "Right");
        PlaceWallInDirection(pos, new Vector3Int(0, 0, 1), wallTile, "Forward");
        PlaceWallInDirection(pos, new Vector3Int(0, 0, -1), wallTile, "Backward");
    }
    
    void PlaceWallInDirection(Vector3Int tilePos, Vector3Int direction, DungeonWallTileData wallTile, string directionName)
    {
        Vector3Int wallPos = tilePos + direction;
        
        // Check if position is valid for a wall (outside grid or no tile exists there)
        if (IsValidWallPosition(wallPos))
        {
            GameObject wallPrefab = null;
            Quaternion rotation = Quaternion.identity;
            
            // Determine which wall prefab to use based on direction
            switch (directionName)
            {
                case "Left":
                    wallPrefab = wallTile.leftWallPrefab;
                    rotation = Quaternion.Euler(0, 270, 0);
                    break;
                case "Right": 
                    wallPrefab = wallTile.rightWallPrefab;
                    rotation = Quaternion.Euler(0, 90, 0);
                    break;
                case "Forward":
                    wallPrefab = wallTile.leftWallPrefab; // Reuse left wall for forward
                    rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case "Backward":
                    wallPrefab = wallTile.rightWallPrefab; // Reuse right wall for backward
                    rotation = Quaternion.Euler(0, 180, 0);
                    break;
            }
            
            if (wallPrefab != null)
            {
                Vector3 worldPos = new(
                    wallPos.x * tileSize, 
                    tilePos.y * tileSize, 
                    wallPos.z * tileSize);
                
                GameObject wall = Instantiate(wallPrefab, worldPos, rotation, transform);
                wall.name = $"Wall_{directionName}_{wallPos.x}_{tilePos.y}_{wallPos.z}";
                
                // Add reference component
                var wallRef = wall.AddComponent<TileReference>();
                wallRef.wallTileData = wallTile;
                
                placedTiles[wallPos] = wall;
                tileReferences[wallPos] = wallRef;
            }
        }
    }

    bool IsValidWallPosition(Vector3Int pos)
    {
        // A wall position is valid if:
        // 1. It's outside the grid bounds, OR
        // 2. It's inside the grid bounds but there's no tile there yet
        return (pos.x < 0 || pos.x >= width || pos.z < 0 || pos.z >= depth) || 
               (!placedTiles.ContainsKey(pos));
    }

    void PlaceObjectsOnTile(GameObject tile, DungeonGroundTileData tileData, Vector3Int tilePos)
    {
        // Place objects based on placeholders in the tile data
        foreach (var placeholder in tileData.placeholders)
        {
            // Find suitable objects for this placeholder
            List<DungeonObjectTileData> validObjects = dungeonObjects.FindAll(obj => 
                obj.validBelow == tileData.tileName || obj.validBelow == "Ground");
            
            if (validObjects.Count == 0)
                continue;
            
            // Try to place an object at this placeholder
            Vector3 worldPlaceholderPos = tile.transform.TransformPoint(placeholder.localPosition);
            Vector3Int gridPos = tilePos + Vector3Int.RoundToInt(placeholder.localPosition / tileSize);
            
            if (IsAreaFree(gridPos, placeholder.size))
            {
                // Choose a random valid object
                DungeonObjectTileData objectData = validObjects[Random.Range(0, validObjects.Count)];
                GameObject objInstance = Instantiate(
                    objectData.prefab, 
                    worldPlaceholderPos, 
                    Quaternion.Euler(objectData.localRotation), 
                    tile.transform);
                
                objInstance.name = $"Object_{objectData.objectName}";
                
                // Add reference component
                var objRef = objInstance.AddComponent<TileReference>();
                objRef.objectTileData = objectData;
                
                // Mark area as occupied
                MarkOccupied(gridPos, placeholder.size);
            }
        }
    }

    bool IsBorderTile(Vector3Int pos)
    {
        return pos.x == 0 || pos.x == width - 1 || pos.z == 0 || pos.z == depth - 1;
    }

    bool IsAreaFree(Vector3Int origin, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
            for (int z = 0; z < size.y; z++)
                if (occupiedPlaceholders.Contains(origin + new Vector3Int(x, 0, z)))
                    return false;
        return true;
    }

    void MarkOccupied(Vector3Int origin, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
            for (int z = 0; z < size.y; z++)
                occupiedPlaceholders.Add(origin + new Vector3Int(x, 0, z));
    }
}

// Add this new class to store references to tile data
public class TileReference : MonoBehaviour
{
    public DungeonGroundTileData groundTileData;
    public DungeonWallTileData wallTileData;
    public DungeonObjectTileData objectTileData;
}