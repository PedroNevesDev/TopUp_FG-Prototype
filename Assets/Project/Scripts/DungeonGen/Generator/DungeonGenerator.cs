using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    [SerializeField] int maxTileCount = 100;
    [SerializeField] int tileDistance = 1;
    [SerializeField] Vector3 scale = Vector3.one;
    [SerializeField] Vector3 spawnOffset;
    [SerializeField] bool generateOnStart = true;

    [Header("Tile Prefab")]
    [SerializeField] SuperTile superTilePrefab;

    [Header("Texture Floor Data")]
    [SerializeField] List<FloorData> floors;

    [Header("Special Object Prefabs")]
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] GameObject healingPrefab;
    [SerializeField] GameObject goldPrefab;
    [SerializeField] GameObject hazardPrefab;
    [SerializeField] GameObject shopPrefab;
    [SerializeField] float specialSpawnYOffset = 0.5f;
    [SerializeField] Transform player;

    [Header("Decoration")]
    [SerializeField] List<GameObject> decorationPrefabs;
    [SerializeField] float decorYOffset = 0.1f;
    [SerializeField] [Range(0f, 1f)] float decorationDensity = 0.2f;

    [Header("Darkning")]
    [SerializeField] SuperTile theDarkningTilePrefab;
    [SerializeField] int darkningRange = 5;
    [SerializeField] bool generateDarkningTiles = true;

    // Optimization fields
    private Dictionary<Vector3Int, SuperTile> grid = new Dictionary<Vector3Int, SuperTile>();
    private List<SuperTile> spawnedTiles = new List<SuperTile>();
    private List<GameObject> activeDecorations = new List<GameObject>();
    private List<SuperTile> darkningTiles = new List<SuperTile>();
    private bool isGenerating = false;

    void Start()
    {
        if (generateOnStart)
        {
            GenerateDungeonImmediate();
        }
    }

    public void GenerateDungeonImmediate()
    {
        if (isGenerating) return;
        isGenerating = true;

        ClearDungeon();
        grid.Clear();
        spawnedTiles.Clear();

        int baseY = 0;

        // Pre-calculate all positions first for better cache performance
        List<Vector3Int> positionsToGenerate = new List<Vector3Int>();
        Dictionary<Vector3Int, Color> specialPositions = new Dictionary<Vector3Int, Color>();
        Dictionary<Vector3Int, bool> decorationPositions = new Dictionary<Vector3Int, bool>();

        foreach (var floor in floors)
        {
            if (floor.layoutTexture == null) continue;

            int texHeight = floor.layoutTexture.height;
            int texWidth = floor.layoutTexture.width;

            for (int repeat = 0; repeat < floor.repeatCount; repeat++)
            {
                for (int z = 0; z < texHeight; z++)
                {
                    for (int x = 0; x < texWidth; x++)
                    {
                        Color pixel = floor.layoutTexture.GetPixel(x, z);
                        if (pixel.grayscale > 0.5f)
                        {
                            Vector3Int gridPos = new Vector3Int(x * tileDistance, baseY, z * tileDistance);
                            positionsToGenerate.Add(gridPos);

                            // Check for special colors
                            if (Approximately(pixel, Color.red) || Approximately(pixel, Color.green) || 
                                Approximately(pixel, Color.yellow) || Approximately(pixel, new Color(0.5f, 0f, 0.5f)) || 
                                Approximately(pixel, Color.blue) || Approximately(pixel, Color.cyan))
                            {
                                specialPositions[gridPos] = pixel;
                            }

                            // Check for decorations
                            if (floor.decorTexture != null && decorationPrefabs.Count > 0)
                            {
                                Color decorPixel = floor.decorTexture.GetPixel(x, z);
                                if (decorPixel.grayscale > 0.5f && Random.value < decorationDensity)
                                {
                                    decorationPositions[gridPos] = true;
                                }
                            }
                        }
                    }
                }
                baseY += tileDistance;
            }
        }

        // Instantiate all tiles in one pass
        foreach (var pos in positionsToGenerate)
        {
            SuperTile tile = SpawnTile(pos);
            grid[pos] = tile;
            spawnedTiles.Add(tile);

            // Handle special positions
            if (specialPositions.TryGetValue(pos, out Color pixel))
            {
                if (Approximately(pixel, Color.red)) SpawnSpecial(enemyPrefab, pos);
                else if (Approximately(pixel, Color.green)) SpawnSpecial(healingPrefab, pos);
                else if (Approximately(pixel, Color.yellow)) SpawnSpecial(goldPrefab, pos);
                else if (Approximately(pixel, new Color(0.5f, 0f, 0.5f))) SpawnSpecial(hazardPrefab, pos);
                else if (Approximately(pixel, Color.blue)) SpawnSpecial(shopPrefab, pos);
                else if (Approximately(pixel, Color.cyan) && player != null)
                {
                    player.position = tile.transform.position + new Vector3(0, 0.5f, 0);
                    print("Found blue pixel");                    
                }

            }

            // Handle decorations
            if (decorationPositions.ContainsKey(pos))
            {
                PlaceDecoration(tile);
            }
        }

        // Connect tiles
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

        // Generate darkning tiles if enabled
        if (generateDarkningTiles && theDarkningTilePrefab != null && darkningRange > 0)
        {
            FillWithDarkningTiles();
        }

        isGenerating = false;
    }

    void FillWithDarkningTiles()
    {
        // Get bounds of the existing dungeon
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

        // Create a hashset of all core positions for faster lookup
        HashSet<Vector3Int> corePositions = new HashSet<Vector3Int>(grid.Keys);

        for (int z = minZ; z <= maxZ; z += tileDistance)
        {
            for (int x = minX; x <= maxX; x += tileDistance)
            {
                Vector3Int pos = new Vector3Int(x, y, z);
                if (!grid.ContainsKey(pos) && IsWithinDarkningRange(pos, corePositions))
                {
                    SuperTile tile = Instantiate(theDarkningTilePrefab, 
                        new Vector3(x, y, z) + spawnOffset, 
                        Quaternion.identity, 
                        transform);
                    tile.transform.localScale = scale;
                    tile.GridPosition = pos;
                    darkningTiles.Add(tile);
                }
            }
        }
    }

    bool IsWithinDarkningRange(Vector3Int pos, HashSet<Vector3Int> corePositions)
    {
        // Check in a spiral pattern outwards for better early exit
        for (int r = 1; r <= darkningRange; r++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                for (int dz = -r; dz <= r; dz++)
                {
                    if (Mathf.Abs(dx) != r && Mathf.Abs(dz) != r) continue;

                    Vector3Int checkPos = new Vector3Int(
                        pos.x + dx * tileDistance,
                        pos.y,
                        pos.z + dz * tileDistance
                    );

                    if (corePositions.Contains(checkPos))
                        return true;
                }
            }
        }
        return false;
    }

void PlaceDecoration(SuperTile tile)
{
    if (decorationPrefabs.Count == 0) return;

    GameObject prefab = decorationPrefabs[Random.Range(0, decorationPrefabs.Count)];
    GameObject decorInstance = Instantiate(prefab);
    
    // Get visual bounds height
    Renderer renderer = decorInstance.GetComponentInChildren<Renderer>();
    float heightOffset = 0f;
    if (renderer != null)
    {
        heightOffset = renderer.bounds.extents.y; // Half the height of the object
    }

    Vector3 basePos = tile.GetSurfacePosition(); // Uses collider height from tile
    decorInstance.transform.position = basePos + Vector3.up * heightOffset + Vector3.up * decorYOffset;
    decorInstance.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
    decorInstance.transform.SetParent(transform);

    activeDecorations.Add(decorInstance);
}

    void ClearDungeon()
    {
        // Clear tiles
        foreach (var tile in spawnedTiles)
        {
            if (tile != null) Destroy(tile.gameObject);
        }
        
        // Clear darkning tiles
        foreach (var tile in darkningTiles)
        {
            if (tile != null) Destroy(tile.gameObject);
        }
        
        // Clear decorations
        foreach (var decor in activeDecorations)
        {
            if (decor != null) Destroy(decor);
        }
        
        spawnedTiles.Clear();
        darkningTiles.Clear();
        activeDecorations.Clear();
    }
SuperTile SpawnTile(Vector3Int gridPos)
{
    Vector3 worldPos = new Vector3(gridPos.x, gridPos.y, gridPos.z) + spawnOffset;
    SuperTile tile = Instantiate(superTilePrefab, worldPos, Quaternion.identity, transform);
    tile.transform.localScale = scale;
    tile.GridPosition = gridPos;
    
    // Optional visual variation
    if (tile.TryGetComponent<Renderer>(out var renderer))
    {
        renderer.material.color = new Color(
            Random.Range(0.8f, 1f),
            Random.Range(0.8f, 1f),
            Random.Range(0.8f, 1f)
        );
    }
    
    return tile;
}

void SpawnSpecial(GameObject prefab, Vector3Int gridPos)
{
    if (prefab == null || !grid.TryGetValue(gridPos, out SuperTile tile)) return;

    Vector3 spawnPos = tile.transform.position + Vector3.up * specialSpawnYOffset;
    Instantiate(prefab, spawnPos, Quaternion.identity, transform);
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

void Shuffle<T>(List<T> list)
{
    int n = list.Count;
    while (n > 1)
    {
        n--;
        int k = Random.Range(0, n + 1);
        (list[k], list[n]) = (list[n], list[k]);
    }
}

// Optional: Coroutine version for async generation if needed
public IEnumerator GenerateDungeonAsync(float maxFrameTime = 0.016f)
{
    if (isGenerating) yield break;
    isGenerating = true;
    
    ClearDungeon();
    grid.Clear();
    spawnedTiles.Clear();
    
    float startTime = Time.realtimeSinceStartup;
    int baseY = 0;
    
    // Phase 1: Generate main tiles
    foreach (var floor in floors)
    {
        if (floor.layoutTexture == null) continue;
        
        int texHeight = floor.layoutTexture.height;
        int texWidth = floor.layoutTexture.width;
        
        for (int repeat = 0; repeat < floor.repeatCount; repeat++)
        {
            for (int z = 0; z < texHeight; z++)
            {
                for (int x = 0; x < texWidth; x++)
                {
                    if (Time.realtimeSinceStartup - startTime > maxFrameTime)
                    {
                        yield return null;
                        startTime = Time.realtimeSinceStartup;
                    }
                    
                    Color pixel = floor.layoutTexture.GetPixel(x, z);
                    if (pixel.grayscale > 0.5f)
                    {
                        Vector3Int gridPos = new Vector3Int(x * tileDistance, baseY, z * tileDistance);
                        if (!grid.ContainsKey(gridPos))
                        {
                            SuperTile tile = SpawnTile(gridPos);
                            grid[gridPos] = tile;
                            spawnedTiles.Add(tile);
                            
                            // Handle special objects
                            if (Approximately(pixel, Color.red)) SpawnSpecial(enemyPrefab, gridPos);
                            else if (Approximately(pixel, Color.green)) SpawnSpecial(healingPrefab, gridPos);
                            else if (Approximately(pixel, Color.yellow)) SpawnSpecial(goldPrefab, gridPos);
                            else if (Approximately(pixel, new Color(0.5f, 0f, 0.5f))) SpawnSpecial(hazardPrefab, gridPos);
                            else if (Approximately(pixel, Color.blue)) SpawnSpecial(shopPrefab, gridPos);
                            else if (Approximately(pixel, Color.cyan) && player != null)
                                player.position = tile.transform.position + new Vector3(0, 0.5f, 0);
                        }
                    }
                }
            }
            baseY += tileDistance;
        }
    }
    
    // Phase 2: Connect tiles
    int connectionsProcessed = 0;
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
        
        connectionsProcessed++;
        if (connectionsProcessed % 50 == 0 && Time.realtimeSinceStartup - startTime > maxFrameTime)
        {
            yield return null;
            startTime = Time.realtimeSinceStartup;
        }
    }
    
    // Phase 3: Darkning tiles (optional)
    if (generateDarkningTiles && theDarkningTilePrefab != null && darkningRange > 0)
    {
        yield return StartCoroutine(GenerateDarkningTilesAsync(maxFrameTime));
    }
    
    isGenerating = false;
}

IEnumerator GenerateDarkningTilesAsync(float maxFrameTime)
{
    float startTime = Time.realtimeSinceStartup;
    
    // Get bounds
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
    
    HashSet<Vector3Int> corePositions = new HashSet<Vector3Int>(grid.Keys);
    int tilesProcessed = 0;
    
    for (int z = minZ; z <= maxZ; z += tileDistance)
    {
        for (int x = minX; x <= maxX; x += tileDistance)
        {
            Vector3Int pos = new Vector3Int(x, y, z);
            if (!grid.ContainsKey(pos) && IsWithinDarkningRange(pos, corePositions))
            {
                SuperTile tile = Instantiate(theDarkningTilePrefab, 
                    new Vector3(x, y, z) + spawnOffset, 
                    Quaternion.identity, 
                    transform);
                tile.transform.localScale = scale;
                tile.GridPosition = pos;
                darkningTiles.Add(tile);
                
                tilesProcessed++;
                if (tilesProcessed % 20 == 0 && Time.realtimeSinceStartup - startTime > maxFrameTime)
                {
                    yield return null;
                    startTime = Time.realtimeSinceStartup;
                }
            }
        }
    }
}

// Editor helper method
#if UNITY_EDITOR
[ContextMenu("Regenerate Dungeon")]
void RegenerateInEditor()
{
    if (Application.isPlaying)
    {
        GenerateDungeonImmediate();
    }
    else
    {
        // Handle editor-time generation if needed
        Debug.Log("Dungeon generation in edit mode not implemented");
    }
}
#endif
    // ... (keep all other methods the same as in the first version)
}
[System.Serializable]
public class FloorData
{
    public Texture2D layoutTexture;
    public Texture2D decorTexture;
    public int repeatCount = 1;
}