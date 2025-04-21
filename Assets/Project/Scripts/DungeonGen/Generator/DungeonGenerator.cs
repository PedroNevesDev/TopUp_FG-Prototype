using UnityEngine;
using System.Collections.Generic;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    [SerializeField] int maxTileCount = 100;
    [SerializeField] int tileDistance = 1;
    [SerializeField] Vector3 scale = Vector3.one;
    [SerializeField] Vector3 spawnOffset;

    [Header("Tile Prefab")]
    [SerializeField] SuperTile superTilePrefab;

    [Header("Texture Floor Data")]
    [SerializeField] List<FloorData> floors;

    [Header("Procedural Options")]
    [SerializeField] bool stopWhenStuck = true;

    [Header("Special Object Prefabs")]
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] GameObject healingPrefab;
    [SerializeField] GameObject goldPrefab;
    [SerializeField] GameObject hazardPrefab;
    [SerializeField] GameObject shopPrefab;
    [SerializeField] float specialSpawnYOffset = 0.5f;
    [SerializeField] Transform player;

    private Dictionary<Vector3Int, SuperTile> grid = new Dictionary<Vector3Int, SuperTile>();
    private List<SuperTile> spawnedTiles = new List<SuperTile>();

    void Start()
    {
        GenerateDungeon();
    }

    void OnValidate()
    {
        foreach (SuperTile tile in spawnedTiles)
        {
            if (tile != null)
                tile.transform.localScale = scale;
        }
    }

    public void GenerateDungeon()
    {
        ClearDungeon();
        grid.Clear();
        spawnedTiles.Clear();

        int baseY = 0;

        foreach (var floor in floors)
        {
            if (floor.layoutTexture == null) continue;

            int texHeight = floor.layoutTexture.height;

            for (int repeat = 0; repeat < floor.repeatCount; repeat++)
            {
                GenerateFloor(floor.layoutTexture, baseY);
                baseY += tileDistance; // shift height for next repetition
            }
        }

        foreach (var kvp in grid)
        {
            Vector3Int pos = kvp.Key;
            SuperTile tile = kvp.Value;

            foreach (Vector3Int dir in GetAllPossibleDirections())
            {
                Vector3Int neighborPos = pos + (dir * tileDistance);
                if (grid.TryGetValue(neighborPos, out SuperTile neighbor))
                {
                    tile.Connect(neighbor, dir);
                }
            }
        }

        if (spawnedTiles.Count > 0)
        {
            player.position = spawnedTiles[0].transform.position + new Vector3(0, 0.5f, 0);
        }
    }

    void GenerateFloor(Texture2D layoutTexture, int yOffset)
    {
        int height = layoutTexture.height;
        int width = layoutTexture.width;

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixel = layoutTexture.GetPixel(x, z);

                if (pixel.grayscale > 0.5f)
                {
                    Vector3Int gridPos = new Vector3Int(x * tileDistance, yOffset, z * tileDistance);

                    if (!grid.ContainsKey(gridPos))
                    {
                        SuperTile tile = SpawnTile(gridPos);
                        grid[gridPos] = tile;
                        spawnedTiles.Add(tile);

                        if (Approximately(pixel, Color.red) && enemyPrefab != null)
                            SpawnSpecial(enemyPrefab, gridPos);
                        else if (Approximately(pixel, Color.green) && healingPrefab != null)
                            SpawnSpecial(healingPrefab, gridPos);
                        else if (Approximately(pixel, Color.yellow) && goldPrefab != null)
                            SpawnSpecial(goldPrefab, gridPos);
                        else if (Approximately(pixel, new Color(0.5f, 0f, 0.5f)) && hazardPrefab != null)
                            SpawnSpecial(hazardPrefab, gridPos);
                        else if (Approximately(pixel, Color.blue) && shopPrefab != null)
                            SpawnSpecial(shopPrefab, gridPos);
                    }
                }
            }
        }
    }

    SuperTile SpawnTile(Vector3Int gridPos)
    {
        Vector3 worldPos = new Vector3(gridPos.x, gridPos.y, gridPos.z) + spawnOffset;
        SuperTile tile = Instantiate(superTilePrefab, worldPos, Quaternion.identity, transform);
        tile.transform.localScale = scale;
        tile.GridPosition = gridPos;

        // 🎨 Optional: Give walls a random color
        Color[] possibleColors = { Color.gray, Color.white, new Color(0.5f, 0.5f, 0.6f) };
        Renderer rend = tile.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = possibleColors[Random.Range(0, possibleColors.Length)];
        }

        return tile;
    }

    void SpawnSpecial(GameObject prefab, Vector3Int gridPos)
    {
        Vector3 worldPos = new Vector3(gridPos.x, gridPos.y, gridPos.z) + spawnOffset + Vector3.up * specialSpawnYOffset;
        Instantiate(prefab, worldPos, Quaternion.identity, transform);
    }

    bool Approximately(Color a, Color b, float tolerance = 0.1f)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }

    List<Vector3Int> GetAllPossibleDirections()
    {
        return new List<Vector3Int>
        {
            Vector3Int.right,
            Vector3Int.left,
            Vector3Int.forward,
            Vector3Int.back,
            Vector3Int.up,
            Vector3Int.down
        };
    }

    public void ClearDungeon()
    {
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
[System.Serializable]
public class FloorData
{
    public Texture2D layoutTexture;
    public int repeatCount = 1;
}