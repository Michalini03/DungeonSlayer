using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapTeleporter : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Assign the player's Transform here.")]
    public Transform playerTransform;

    [Tooltip("Array of Tilemaps. Each Tilemap should contain EXACTLY 2 tiles.")]
    public Tilemap[] teleportTilemaps;

    [Header("Settings")]
    [Tooltip("Cooldown in seconds to prevent infinite teleportation loops.")]
    public float teleportCooldown = 1.0f;

    private Dictionary<Vector3Int, Vector3Int> teleportPairs = new Dictionary<Vector3Int, Vector3Int>();
    private float lastTeleportTime = 0f;
    private Grid parentGrid;

    void Start()
    {
        InitializeTeleporters();
    }

    void InitializeTeleporters()
    {
        if (teleportTilemaps.Length == 0)
        {
            Debug.LogWarning("No teleport Tilemaps assigned!");
            return;
        }

        parentGrid = teleportTilemaps[0].layoutGrid;

        foreach (Tilemap tm in teleportTilemaps)
        {
            if (tm == null) continue;

            List<Vector3Int> points = new List<Vector3Int>();

            BoundsInt bounds = tm.cellBounds;
            foreach (Vector3Int pos in bounds.allPositionsWithin)
            {
                if (tm.HasTile(pos))
                {
                    points.Add(pos);
                }
            }

            if (points.Count == 2)
            {
                teleportPairs[points[0]] = points[1];
                teleportPairs[points[1]] = points[0];

            }
            else
            {
                Debug.LogWarning($"Tilemap '{tm.name}' has {points.Count} tiles. It needs exactly 2 to work as a teleporter.");
            }
        }
    }

    void Update()
    {
        if (Time.time - lastTeleportTime < teleportCooldown) return;

        CheckForTeleport();
    }

    void CheckForTeleport()
    {
        if (parentGrid == null || playerTransform == null) return;

        Vector3Int playerCellPosition = parentGrid.WorldToCell(playerTransform.position);

        if (teleportPairs.TryGetValue(playerCellPosition, out Vector3Int destinationCell))
        {
            ExecuteTeleport(destinationCell);
        }
    }

    void ExecuteTeleport(Vector3Int destCell)
    {
        Vector3 destinationWorldPos = teleportTilemaps[0].GetCellCenterWorld(destCell);

        playerTransform.position = new Vector3(
            destinationWorldPos.x,
            destinationWorldPos.y,
            playerTransform.position.z
        );

        // Start the cooldown
        lastTeleportTime = Time.time;

        Debug.Log("Player Teleported!");
    }
}