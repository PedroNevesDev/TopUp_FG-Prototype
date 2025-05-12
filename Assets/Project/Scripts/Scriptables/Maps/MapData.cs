using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "MapData", menuName = "MapData", order = 0)]
public class MapData : ScriptableObject 

{
    public List<FloorData> floors;
}

[System.Serializable]
public class FloorData
{
    public Texture2D layoutTexture;  // The texture representing the floor's tile layout
    public int repeatCount = 1;      // How many times to repeat this floor pattern
    public List<DecorLayer> decorLayers = new List<DecorLayer>();  // Decoration layers for the floor
}

[System.Serializable]
public class DecorLayer
{
    public string layerName;
    public Texture2D decorTexture;
    public List<GameObject> listOfDecor = new List<GameObject>();
    public DecorType decorType;

    [Tooltip("Maximum number of decorations from this layer that can be placed.")]
    public int maxCount = 100;  // or any default you prefer

    [NonSerialized]
    public List<Vector3Int> cachedTiles = new(); // <- Transient, cleared per floor
}
    public enum DecorType
    {
        Wall,
        Floor,
        Ceiling
    }