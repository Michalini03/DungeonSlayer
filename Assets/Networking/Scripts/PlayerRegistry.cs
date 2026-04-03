using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static registry of all active player GameObjects.
/// Used by enemies to find the nearest player.
/// </summary>
public static class PlayerRegistry
{
    private static readonly List<GameObject> players = new List<GameObject>();

    public static IReadOnlyList<GameObject> Players => players;

    public static void Register(GameObject player)
    {
        if (!players.Contains(player))
        {
            players.Add(player);
            Debug.Log($"PlayerRegistry: Registered player. Total: {players.Count}");
        }
    }

    public static void Unregister(GameObject player)
    {
        players.Remove(player);
        Debug.Log($"PlayerRegistry: Unregistered player. Total: {players.Count}");
    }

    public static void Clear()
    {
        players.Clear();
    }

    public static GameObject GetClosestPlayer(Vector3 position)
    {
        GameObject closest = null;
        float closestDist = float.MaxValue;

        for (int i = players.Count - 1; i >= 0; i--)
        {
            if (players[i] == null)
            {
                players.RemoveAt(i);
                continue;
            }

            float dist = Vector3.Distance(position, players[i].transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = players[i];
            }
        }

        return closest;
    }

    public static GameObject GetFirstPlayer()
    {
        for (int i = players.Count - 1; i >= 0; i--)
        {
            if (players[i] == null)
            {
                players.RemoveAt(i);
                continue;
            }
            return players[i];
        }
        return null;
    }
}
