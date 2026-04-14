using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class TileMapSpawnPointFinder : MonoBehaviour
{
    [SerializeField] private Tilemap skeletonSpawnTilemap;
    [SerializeField] private Tilemap flyingEyeSpawnTilemap;
    [SerializeField] private Tilemap ratSpawnTilemap;
    [SerializeField] private Vector3 spawnSkeletonOffset = new Vector3(0f, 0.5f, 0f);
    [SerializeField] private Vector3 spawnFlyingEyeOffset = new Vector3(0f, 0.5f, 0f);
    [SerializeField] private Vector3 spawnRatOffset = new Vector3(0f, 0.5f, 0f);
    [SerializeField] public GameObject player;

    public readonly List<Vector3> skeletonSpawnPoints = new List<Vector3>();
    public readonly List<Vector3> flyingEyeSpawnPoints = new List<Vector3>();
    public readonly List<Vector3> ratSpawnPoints = new List<Vector3>();

    private bool hasFoundSpawnPoints = false;

    private void Awake()
    {
        if (!hasFoundSpawnPoints)
        {
            FindSpawnPoints();
        }
    }

    private void FindSpawnPoints()
    {
        skeletonSpawnPoints.Clear();
        flyingEyeSpawnPoints.Clear();
        ratSpawnPoints.Clear();

        // SKELETON SPAWN POINTS
        if (skeletonSpawnTilemap == null)
        {
            Debug.LogWarning("Skeleton Spawn Tilemap is not assigned.", this);
        }
        else
        {
            BoundsInt skeletonBounds = skeletonSpawnTilemap.cellBounds;
            foreach (Vector3Int cellPosition in skeletonBounds.allPositionsWithin)
            {
                if (skeletonSpawnTilemap.HasTile(cellPosition))
                {
                    Vector3 worldPoint = skeletonSpawnTilemap.GetCellCenterWorld(cellPosition) + spawnSkeletonOffset;
                    worldPoint.z = player.transform.position.z;
                    skeletonSpawnPoints.Add(worldPoint);
                }
            }
        }

        // FLYING EYE SPAWN POINTS
        if (flyingEyeSpawnTilemap == null)
        {
            Debug.LogWarning("Flying Eye Spawn Tilemap is not assigned.", this);
        }
        else
        {
            BoundsInt flyingEyeBounds = flyingEyeSpawnTilemap.cellBounds;
            foreach (Vector3Int cellPosition in flyingEyeBounds.allPositionsWithin)
            {
                if (flyingEyeSpawnTilemap.HasTile(cellPosition))
                {
                    Vector3 worldPoint = flyingEyeSpawnTilemap.GetCellCenterWorld(cellPosition) + spawnFlyingEyeOffset;
                    worldPoint.z = player.transform.position.z;
                    flyingEyeSpawnPoints.Add(worldPoint);
                }
            }
        }

        // RAT SPAWN POINTS
        if (ratSpawnTilemap == null)
        {
            Debug.LogWarning("Rat Spawn Tilemap is not assigned.", this);
        }
        else
        {
            BoundsInt ratBounds = ratSpawnTilemap.cellBounds;
            foreach (Vector3Int cellPosition in ratBounds.allPositionsWithin)
            {
                if (ratSpawnTilemap.HasTile(cellPosition))
                {
                    Vector3 worldPoint = ratSpawnTilemap.GetCellCenterWorld(cellPosition) + spawnRatOffset;
                    worldPoint.z = player.transform.position.z;
                    ratSpawnPoints.Add(worldPoint);
                }
            }
        }

        hasFoundSpawnPoints = true;

        Debug.Log($"Found {skeletonSpawnPoints.Count} Skeleton points, {flyingEyeSpawnPoints.Count} Flying Eye points, and {ratSpawnPoints.Count} Rat points.");
    }

    public Vector3 GetRandomSpawnPoint(EnemyType enemyType)
    {
        switch (enemyType)
        {
            case EnemyType.Skeleton:
                if (skeletonSpawnPoints.Count > 0)
                {
                    int randomIndex = Random.Range(0, skeletonSpawnPoints.Count);
                    return skeletonSpawnPoints[randomIndex];
                }
                Debug.LogWarning("Requested Skeleton spawn, but no Skeleton spawn points exist. Defaulting to Vector3.zero.");
                return Vector3.zero;

            case EnemyType.FlyingEye:
                if (flyingEyeSpawnPoints.Count > 0)
                {
                    int randomIndex = Random.Range(0, flyingEyeSpawnPoints.Count);
                    return flyingEyeSpawnPoints[randomIndex];
                }
                Debug.LogWarning("Requested FlyingEye spawn, but no FlyingEye spawn points exist. Defaulting to Vector3.zero.");
                return Vector3.zero;

            case EnemyType.Rat:
                if (ratSpawnPoints.Count > 0)
                {
                    int randomIndex = Random.Range(0, ratSpawnPoints.Count);
                    return ratSpawnPoints[randomIndex];
                }
                Debug.LogWarning("Requested Rat spawn, but no Rat spawn points exist. Defaulting to Vector3.zero.");
                return Vector3.zero;

            default:
                Debug.LogError("Unhandled enemy type.");
                return Vector3.zero;
        }
    }
}