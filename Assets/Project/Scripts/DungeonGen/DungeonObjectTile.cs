using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon/DungeonObjectTile")]
public class DungeonObjectTileData : ScriptableObject
{
    public string objectName; // Name for the object
    public GameObject prefab; // The object prefab to be placed in the dungeon
    public Vector3 localPosition; // The position relative to the tile
    public Vector3 localRotation; // The rotation relative to the tile
    public Vector2Int size = Vector2Int.one; // The size of the object (in terms of grid spaces)

    // Placement rules
    public string validBelow = "Ground"; // What tile types this object can be placed on
    
    // Optional random rotation settings
    [Header("Random Rotation")]
    public bool useRandomRotation = false;
    public float minRotationY = 0f;
    public float maxRotationY = 360f;
    
    // Get a rotation for this object (random if enabled)
    public Quaternion GetRotation()
    {
        if (useRandomRotation)
        {
            float randomY = Random.Range(minRotationY, maxRotationY);
            return Quaternion.Euler(localRotation.x, randomY, localRotation.z);
        }
        
        return Quaternion.Euler(localRotation);
    }
}
