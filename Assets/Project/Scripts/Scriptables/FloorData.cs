using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon/Floor Data")]
public class FloorData : ScriptableObject
{
    public Texture2D layoutTexture;
    public List<DecorLayer> decorLayers = new List<DecorLayer>();
    public int repeatCount = 1;
}
[System.Serializable]
public class DecorLayer
{
    public Texture2D decorTexture;
    public DecorType decorType;
    public List<GameObject> listOfDecor = new List<GameObject>();
}

[System.Serializable]
public class SpawnLayer
{
    public Texture2D spawnTexture;
    public SpawnType spawnType;
    public GameObject prefab;
    [Range(0f, 1f)] public float spawnChance = 1f;
}

[System.Serializable]
public enum DecorType
{
    Floor,
    Wall,
    Ceiling
}
public enum SpawnType
{
    Enemy,
    Healing,
    Gold,
    Hazard,
    Shop,
    Breakable
}
