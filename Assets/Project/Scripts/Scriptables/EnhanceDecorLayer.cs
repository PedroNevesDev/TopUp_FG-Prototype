using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon/Enhanced Decor Layer")]
public class EnhancedDecorLayer : ScriptableObject
{
    public string layerName = "New Decor Layer";
    public List<EnhancedDecorData> decorations = new List<EnhancedDecorData>();
    [Range(0f, 1f)] public float layerDensity = 0.5f;
    public bool clearExistingOnGenerate = true;
}

[System.Serializable]
public class EnhancedDecorData
{
    [Header("Basic Settings")]
    public GameObject decorPrefab;
    public DecorPlacementType placementType = DecorPlacementType.FloorCenter;
    [Range(0f, 1f)] public float spawnProbability = 0.5f;
    
    [Header("Placement Settings")]
    public float heightOffset = 0.1f;
    public float minDistanceFromSameType = 2f;
    public float minDistanceFromOtherTypes = 0.5f;
    
    [Header("Variation")]
    [Range(0f, 360f)] public float maxRotation = 90f;
    public Vector2 scaleRange = new Vector2(0.8f, 1.2f);
    
    [Header("Wall Settings")]
    [Range(0f, 1f)] public float wallOffset = 0.1f;
    public bool alignToWallNormal = true;
    internal int maxCountPerDungeon;
    internal int minCountPerDungeon;
    internal float minDistanceFromAnyDecor;
}
public enum DecorPlacementType 
{
    FloorCenter,
    FloorEdge,
    WallSurface,
    Ceiling
}