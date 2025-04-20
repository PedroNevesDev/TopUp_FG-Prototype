using UnityEngine;

[System.Serializable]
public class PlaceholderData
{
    public Vector3 localPosition;      // Local position within the tile where an object can be placed
    public Quaternion localRotation = Quaternion.identity;  // Rotation for the object
    public string tag;                 // Tag to categorize the object
    public Vector2Int size = Vector2Int.one; // Size of the placeholder grid (how many grid units it takes up)
}
