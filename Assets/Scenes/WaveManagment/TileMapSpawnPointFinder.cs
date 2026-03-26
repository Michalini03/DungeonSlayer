using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class TileMapSpawnPointFinder : MonoBehaviour
{
    [SerializeField] private Tilemap spawnTilemap;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.5f, 0f);

    private readonly List<Vector3> spawnPoints = new List<Vector3>();
    private bool hasFoundSpawnPoints = false;

    private void Awake()
    {
        if (!hasFoundSpawnPoints)
        {
            FindSpawnPoints();
        }
    }
    
    /*
    private void Update()
    {
        Vector3 randomSpawnPoint = GetRandomSpawnPoint();
        Debug.Log("Random Spawn Point: " + randomSpawnPoint);
    }
    */

    private void FindSpawnPoints()
    {
        spawnPoints.Clear();

        if (spawnTilemap == null)
        {
            Debug.LogWarning("Spawn Tilemap is not assigned.", this);
            return;
        }

        BoundsInt bounds = spawnTilemap.cellBounds;
        foreach (Vector3Int cellPosition in bounds.allPositionsWithin)
        {
            if (!spawnTilemap.HasTile(cellPosition))
            {
                continue;
            }

            Vector3 worldPoint = spawnTilemap.GetCellCenterWorld(cellPosition) + spawnOffset;
            spawnPoints.Add(worldPoint);
        }
    }

    public Vector3 GetRandomSpawnPoint()
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning("No spawn points found. Ensure the tilemap has tiles and is assigned.", this);
            return Vector3.zero;
        }

        int randomIndex = Random.Range(0, spawnPoints.Count);
        return spawnPoints[randomIndex];
    }
}