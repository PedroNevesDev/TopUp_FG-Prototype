using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon/Map Data")]
public class MapData : ScriptableObject
{
    public List<FloorData> floors = new List<FloorData>();
    public int maxTileCount;
    public int tileDistance;
    public Vector3 scale = Vector3.one;
    public Vector3 spawnOffset;
    public bool generateDarkningTiles = true;
    public int darkningRange = 5;
}