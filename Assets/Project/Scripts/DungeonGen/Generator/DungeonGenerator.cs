using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

public class DungeonGenerator : Singleton<DungeonGenerator>
{
    [Header("Generation Settings")]
    [SerializeField] int maxTileCount = 100;
    [SerializeField] int tileDistance = 1;
    [SerializeField] Vector3 scale = Vector3.one;
    [SerializeField] Vector3 spawnOffset;
    [SerializeField] bool generateOnStart = true;

    [Header("Tile Prefab")]
    [SerializeField] SuperTile superTilePrefab;

    [Header("Special Object Prefabs")]
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] GameObject healingPrefab;
    [SerializeField] GameObject goldPrefab;
    [SerializeField] GameObject hazardPrefab;
    [SerializeField] GameObject shopPrefab;
    [SerializeField] float specialSpawnYOffset = 0.5f;
    [SerializeField] Transform player;

    [Header("Decoration")]
    [SerializeField] float decorYOffset = 0.1f;
    [SerializeField] [Range(0f, 1f)] float decorationDensity = 0.2f;

    [Header("Darkning")]
    [SerializeField] SuperTile theDarkningTilePrefab;
    [SerializeField] int darkningRange = 5;
    [SerializeField] bool generateDarkningTiles = true;

    // Internal state
    private Dictionary<Vector3Int, SuperTile> grid = new Dictionary<Vector3Int, SuperTile>();
    private List<SuperTile> spawnedTiles = new List<SuperTile>();
    private List<GameObject> activeDecorations = new List<GameObject>();
    private List<SuperTile> darkningTiles = new List<SuperTile>();
    private bool isGenerating = false;

    public Vector3 Scale { get => scale; set => scale = value; }

    public List<Enemy> listOfEnemiesWithin = new List<Enemy>();

    public List<MapData> mapDatas = new List<MapData>();
    private MapData lastMap;

    void Start()
    {
        if (generateOnStart)
            GenerateDungeonImmediate();
    }

    public void Regenerate()
    {
        LevelManager.Instance.TogglePause(false);
        GenerateDungeonImmediate(UIManager.Instance.ResetTransition);
    }

    public void AddEnemy(Enemy enemy)
    {
        listOfEnemiesWithin.Add(enemy);
    }

    public void RemoveEnemy(Enemy enemy)
    {
        listOfEnemiesWithin.Remove(enemy);
        if (listOfEnemiesWithin.Count <= 0)
        {
            StartCoroutine(WaitAwhile());
        }
    }

    IEnumerator WaitAwhile()
    {
        yield return new WaitForSeconds(1.5f);
        UIManager.Instance.ShowDungeonMessage();
    }
    MapData GetRandomMap()
    {
        return mapDatas[UnityEngine.Random.Range(0,mapDatas.Count)];
    }
    #region Dungeon Generation

   public void GenerateDungeonImmediate(Action action = null)
{
    if (isGenerating) return;
    isGenerating = true;

    MapData newMap = GetRandomMap();
    while (newMap == lastMap)
        newMap = GetRandomMap();

    ClearDungeon();
    grid.Clear();
    spawnedTiles.Clear();

    foreach (var floor in newMap.floors)
    {
        int baseY = 0;
        List<Vector3Int> tilePositions = new();
        Dictionary<Vector3Int, Color> specialPixels = new();
        Dictionary<Vector3Int, FloorData> decorSources = new();

        for (int r = 0; r < floor.repeatCount; r++)
        {
            if (floor.layoutTexture == null) continue;

            for (int z = 0; z < floor.layoutTexture.height; z++)
            {
                for (int x = 0; x < floor.layoutTexture.width; x++)
                {
                    Color pixel = floor.layoutTexture.GetPixel(x, z);
                    if (pixel.grayscale <= 0.5f) continue;

                    Vector3Int pos = new(x * tileDistance, baseY, z * tileDistance);
                    tilePositions.Add(pos);

                    if (IsSpecialColor(pixel))
                        specialPixels[pos] = pixel;

                    if (HasDecorAt(floor, x, z))
                        decorSources[pos] = floor;
                }
            }
            baseY += tileDistance;
        }

        foreach (var pos in tilePositions)
        {
            SuperTile tile = SpawnTile(pos);
            grid[pos] = tile;
            spawnedTiles.Add(tile);

            if (specialPixels.TryGetValue(pos, out var color))
                HandleSpecial(color, pos, tile);

            if (decorSources.TryGetValue(pos, out var floorData))
                PlaceDecorations(tile, floorData, pos);
        }
    }

    foreach (var kvp in grid)
    {
        foreach (var dir in GetAllPossibleDirections())
        {
            var neighborPos = kvp.Key + dir * tileDistance;
            if (grid.TryGetValue(neighborPos, out var neighbor))
                kvp.Value.Connect(neighbor, dir);
        }
    }

    if (generateDarkningTiles && theDarkningTilePrefab != null)
        FillWithDarkningTiles();

    spawnedTiles.ForEach(tile => tile.RandomlyReplaceWalls());
    isGenerating = false;
    action?.Invoke();
}

   void FillWithDarkningTiles()
    {
        int minX = int.MaxValue, maxX = int.MinValue;
        int minZ = int.MaxValue, maxZ = int.MinValue;
        int y = 0;

        foreach (var pos in grid.Keys)
        {
            minX = Mathf.Min(minX, pos.x);
            maxX = Mathf.Max(maxX, pos.x);
            minZ = Mathf.Min(minZ, pos.z);
            maxZ = Mathf.Max(maxZ, pos.z);
            y = pos.y;
        }

        minX -= darkningRange * tileDistance;
        maxX += darkningRange * tileDistance;
        minZ -= darkningRange * tileDistance;
        maxZ += darkningRange * tileDistance;

        HashSet<Vector3Int> corePositions = new(grid.Keys);

        for (int z = minZ; z <= maxZ; z += tileDistance)
        {
            for (int x = minX; x <= maxX; x += tileDistance)
            {
                var pos = new Vector3Int(x, y, z);
                if (!grid.ContainsKey(pos) && IsWithinDarkningRange(pos, corePositions))
                {
                    SuperTile darkTile = Instantiate(theDarkningTilePrefab, new Vector3(x, y, z) + spawnOffset, Quaternion.identity, transform);
                    darkTile.transform.localScale = scale;
                    darkTile.GridPosition = pos;
                    darkningTiles.Add(darkTile);
                }
            }
        }
    }
        bool IsWithinDarkningRange(Vector3Int pos, HashSet<Vector3Int> corePositions)
    {
        for (int r = 1; r <= darkningRange; r++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                for (int dz = -r; dz <= r; dz++)
                {
                    if (Mathf.Abs(dx) != r && Mathf.Abs(dz) != r) continue;
                    if (corePositions.Contains(pos + new Vector3Int(dx, 0, dz) * tileDistance))
                        return true;
                }
            }
        }
        return false;
    }
    #endregion

    #region Helpers

void HandleSpecial(Color pixel, Vector3Int pos, SuperTile tile)
{
    Debug.Log($"[Special Color Detected] at {pos}: r={pixel.r:F2}, g={pixel.g:F2}, b={pixel.b:F2}");

    if (Approximately(pixel, Color.red))
        SpawnSpecial(enemyPrefab, pos);
    else if (Approximately(pixel, Color.cyan))
    {
        if (player != null)
        {
            player.position = tile.transform.position + new Vector3(0, 0.5f, 0);
            Debug.Log($"[Player Placed] at {tile.transform.position}");
        }
        else
        {
            Debug.LogWarning("[Player Not Assigned] Cannot place player!");
        }
    }
}


    void SpawnSpecial(GameObject prefab, Vector3Int gridPos)
    {
        if (prefab == null || !grid.TryGetValue(gridPos, out SuperTile tile)) return;
        Vector3 spawnPos = tile.transform.position + Vector3.up * specialSpawnYOffset;
        Instantiate(prefab, spawnPos, Quaternion.identity, transform);
    }

    void PlaceDecorations(SuperTile tile, FloorData floorData, Vector3Int gridPos)
    {
        foreach (var layer in floorData.decorLayers)
        {
            if (layer.decorTexture == null || layer.listOfDecor.Count == 0) continue;

            int x = (gridPos.x / tileDistance) % layer.decorTexture.width;
            int z = (gridPos.z / tileDistance) % layer.decorTexture.height;
            Color pixel = layer.decorTexture.GetPixel(x, z);

            if (pixel.grayscale > 0.5f && UnityEngine.Random.value < decorationDensity)
            {
                var prefab = layer.listOfDecor[UnityEngine.Random.Range(0, layer.listOfDecor.Count)];
                tile.PlaceDecor(prefab, layer.decorType);
            }
        }
    }

    SuperTile SpawnTile(Vector3Int gridPos)
    {
        Vector3 worldPos = new Vector3(gridPos.x, gridPos.y, gridPos.z) + spawnOffset;
        SuperTile tile = Instantiate(superTilePrefab, worldPos, Quaternion.identity, transform);
        tile.transform.localScale = scale;
        tile.GridPosition = gridPos;

        if (tile.TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material.color = new Color(UnityEngine.Random.Range(0.8f, 1f), UnityEngine.Random.Range(0.8f, 1f), UnityEngine.Random.Range(0.8f, 1f));
        }

        return tile;
    }

    void ClearDungeon()
    {
        foreach (var tile in spawnedTiles) if (tile != null) Destroy(tile.gameObject);
        foreach (var tile in darkningTiles) if (tile != null) Destroy(tile.gameObject);
        foreach (var decor in activeDecorations) if (decor != null) Destroy(decor);

        spawnedTiles.Clear();
        darkningTiles.Clear();
        activeDecorations.Clear();
    }

    bool IsSpecialColor(Color color)
    {
        return Approximately(color, Color.red) || Approximately(color, Color.green) ||
               Approximately(color, Color.yellow) || Approximately(color, new Color(0.5f, 0f, 0.5f)) ||
               Approximately(color, Color.blue) || Approximately(color, Color.cyan);
    }

    bool HasDecorAt(FloorData floor, int x, int z)
    {
        foreach (var layer in floor.decorLayers)
        {
            if (layer.decorTexture == null) continue;
            Color decorPixel = layer.decorTexture.GetPixel(x, z);
            if (decorPixel.grayscale > 0.5f && UnityEngine.Random.value < decorationDensity)
                return true;
        }
        return false;
    }

bool Approximately(Color a, Color b, float tolerance = 0.2f)
{
    return Mathf.Abs(a.r - b.r) < tolerance &&
           Mathf.Abs(a.g - b.g) < tolerance &&
           Mathf.Abs(a.b - b.b) < tolerance;
}


    List<Vector3Int> GetAllPossibleDirections()
    {
        return new List<Vector3Int> {
            Vector3Int.right, Vector3Int.left,
            Vector3Int.forward, Vector3Int.back,
            Vector3Int.up, Vector3Int.down
        };
    }

    #endregion

#if UNITY_EDITOR
    [ContextMenu("Regenerate Dungeon")]
    void RegenerateInEditor()
    {
        if (Application.isPlaying) GenerateDungeonImmediate();
        else Debug.Log("Dungeon generation in edit mode not implemented");
    }
#endif
}
