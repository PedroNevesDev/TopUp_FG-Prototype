using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DungeonGenerator))]
public class OrganicDecorationSystem : MonoBehaviour
{
    [SerializeField] private List<EnhancedDecorLayer> decorLayers = new List<EnhancedDecorLayer>();
    [SerializeField] private bool visualizeSpawnPoints = false;

    private DungeonGenerator generator;
    private List<Vector3> occupiedPositions = new List<Vector3>();
    private List<GameObject> activeDecorations = new List<GameObject>();

    public void Initialize(DungeonGenerator dungeonGenerator)
    {
        this.generator = dungeonGenerator;
    }
    public void PopulateDungeon()
    {
        if (generator == null) return;

        var allTiles = generator.GetAllTiles();
        if (allTiles.Count == 0) return;

        ClearDecorations();

        foreach (var layer in decorLayers)
        {
            if (layer == null || Random.value > layer.layerDensity) continue;

            foreach (var decorData in layer.decorations)
            {
                if (decorData.decorPrefab == null) continue;

                int targetCount = Random.Range(decorData.minCountPerDungeon, decorData.maxCountPerDungeon + 1);
                PlaceDecorations(decorData, allTiles, targetCount);
            }
        }
    }

    private void PlaceDecorations(EnhancedDecorData decorData, List<SuperTile> tiles, int targetCount)
    {
        int placed = 0;
        foreach (var tile in tiles)
        {
            if (placed >= targetCount) break;
            if (Random.value > decorData.spawnProbability) continue;

            if (TryGetPlacementPosition(tile, decorData, out Vector3 position, out Vector3 normal))
            {
                if (IsPositionValid(position, decorData))
                {
                    Quaternion rotation = Quaternion.LookRotation(normal);
                    if (!decorData.alignToWallNormal)
                        rotation = Quaternion.Euler(0, Random.Range(0, decorData.maxRotation), 0);

                    GameObject instance = Instantiate(decorData.decorPrefab, position, rotation, transform);
                    float scaleMultiplier = Random.Range(decorData.scaleRange.x, decorData.scaleRange.y);
                    instance.transform.localScale *= scaleMultiplier;

                    occupiedPositions.Add(position);
                    activeDecorations.Add(instance);
                    placed++;
                }
            }
        }
    }

    private bool TryGetPlacementPosition(SuperTile tile, EnhancedDecorData decorData, out Vector3 position, out Vector3 normal)
    {
        position = Vector3.zero;
        normal = Vector3.up;

        switch (decorData.placementType)
        {
            case DecorPlacementType.FloorCenter:
                position = tile.floor.transform.position + Vector3.up * decorData.heightOffset;
                return true;

            case DecorPlacementType.WallSurface:
                return GetWallPosition(tile, decorData, out position, out normal);

            case DecorPlacementType.Ceiling:
                if (tile.ceiling != null)
                {
                    position = tile.ceiling.transform.position + Vector3.down * decorData.heightOffset;
                    normal = Vector3.down;
                    return true;
                }
                break;

            case DecorPlacementType.FloorEdge:
                if (tile.floor != null)
                {
                    Bounds bounds = tile.floor.GetComponent<Renderer>().bounds;
                    Vector3 edgeOffset = new Vector3(
                        Random.Range(-bounds.extents.x, bounds.extents.x),
                        0,
                        Random.Range(-bounds.extents.z, bounds.extents.z)
                    ).normalized * bounds.extents.magnitude * 0.5f;

                    position = bounds.center + edgeOffset + Vector3.up * decorData.heightOffset;
                    return true;
                }
                break;
        }

        return false;
    }

    private bool GetWallPosition(SuperTile tile, EnhancedDecorData decorData, out Vector3 position, out Vector3 normal)
    {
        position = Vector3.zero;
        normal = Vector3.forward;

        List<(GameObject wall, Vector3 dir)> walls = new()
        {
            (tile.rightWall, Vector3.right),
            (tile.leftWall, Vector3.left),
            (tile.forwardWall, Vector3.forward),
            (tile.backWall, Vector3.back)
        };

        foreach (var (wall, dir) in walls)
        {
            if (wall != null && wall.activeSelf)
            {
                var renderer = wall.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Bounds bounds = renderer.bounds;
                    Vector3 localPos = new Vector3(
                        Random.Range(-bounds.extents.x, bounds.extents.x),
                        Random.Range(-bounds.extents.y, bounds.extents.y),
                        0f
                    );

                    position = bounds.center + localPos + dir * decorData.wallOffset;
                    normal = dir;
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsPositionValid(Vector3 position, EnhancedDecorData decorData)
    {
        foreach (var pos in occupiedPositions)
        {
            if (Vector3.Distance(pos, position) < decorData.minDistanceFromAnyDecor)
                return false;
        }
        return true;
    }

    private void ClearDecorations()
    {
        foreach (var go in activeDecorations)
        {
            if (go != null) Destroy(go);
        }
        activeDecorations.Clear();
        occupiedPositions.Clear();
    }

    private void OnDrawGizmos()
    {
        if (!visualizeSpawnPoints) return;

        Gizmos.color = Color.green;
        foreach (var pos in occupiedPositions)
        {
            Gizmos.DrawSphere(pos, 0.2f);
        }
    }
}