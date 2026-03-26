using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class TileMapSpawnPointFinder : MonoBehaviour
{
    [SerializeField] private Tilemap skeletonSpawnTilemap;
    [SerializeField] private Tilemap flyingEyeSpawnTilemap;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.5f, 0f);
    [SerializeField] public GameObject player;

    private readonly List<Vector3> skeletonSpawnPoints = new List<Vector3>();
    private readonly List<Vector3> flyingEyeSpawnPoints = new List<Vector3>();
    
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

        if (skeletonSpawnTilemap == null)
        {
            Debug.LogWarning("Skeleton Spawn Tilemap is not assigned.", this);
            return;
        }

        else if (flyingEyeSpawnTilemap == null)
        {
            Debug.LogWarning("Flying Eye Spawn Tilemap is not assigned.", this);
            return;
        }

        BoundsInt SkeletonBounds = skeletonSpawnTilemap.cellBounds;
        foreach (Vector3Int cellPosition in SkeletonBounds.allPositionsWithin)
        {
            if (!skeletonSpawnTilemap.HasTile(cellPosition))
            {
                continue;
            }

            Vector3 worldPoint = skeletonSpawnTilemap.GetCellCenterWorld(cellPosition) + spawnOffset;
            worldPoint.z = player.transform.position.z;
            skeletonSpawnPoints.Add(worldPoint);
        }

        BoundsInt FlyingEyeBounds = flyingEyeSpawnTilemap.cellBounds;
        foreach (Vector3Int cellPosition in FlyingEyeBounds.allPositionsWithin)
        {
            if (!flyingEyeSpawnTilemap.HasTile(cellPosition))
            {
                continue;
            }

            Vector3 worldPoint = flyingEyeSpawnTilemap.GetCellCenterWorld(cellPosition) + spawnOffset;
            worldPoint.z = player.transform.position.z;
            flyingEyeSpawnPoints.Add(worldPoint);
        }
        hasFoundSpawnPoints = true;
    }

    public Vector3 GetRandomSpawnPoint(EnemyType enemyType)
    {
        if (skeletonSpawnPoints.Count == 0 && flyingEyeSpawnPoints.Count == 0)
        {
            Debug.LogWarning("No spawn points found. Ensure the tilemap has tiles and is assigned.", this);
            return Vector3.zero;
        }

        int randomIndex = 0;
        if(enemyType == EnemyType.Skeleton && skeletonSpawnPoints.Count > 0)
        {
            randomIndex = Random.Range(0, skeletonSpawnPoints.Count);
        }
        else if(enemyType == EnemyType.FlyingEye && flyingEyeSpawnPoints.Count > 0)
        {
            randomIndex = Random.Range(0, flyingEyeSpawnPoints.Count);
        }
        
        switch (enemyType)
        {
            case EnemyType.Skeleton:
                return skeletonSpawnPoints[randomIndex];
            
            case EnemyType.FlyingEye:
                return flyingEyeSpawnPoints[randomIndex];
            
            default:
                Debug.LogError("Unhandled enemy type.");
                return Vector3.zero;
        }        
    }


}