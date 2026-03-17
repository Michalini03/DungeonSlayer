using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private TileMapSpawnPointFinder spawnPointFinder;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject PlayerPrefab;
    [SerializeField] private int SkeletonCount = 5;
    [SerializeField] private string playerTag = "Player";

    private void Start()
    {
        SpawnEnemies();
    }

    private void SpawnEnemies()
    {
        if (spawnPointFinder == null)
        {
            Debug.LogWarning("Spawn Point Finder is not assigned.", this);
            return;
        }
        SpawnSkeletons();
        // Now just simple spawn of one skeleton at certain spawn points, can be expanded to spawn more types of enemies or different counts

    }

    private void SpawnSkeletons()
    {
        GameObject playerReference = PlayerPrefab != null ? PlayerPrefab : GameObject.FindGameObjectWithTag(playerTag);

        for (int i = 0; i < SkeletonCount; i++)
        {
            Vector3 spawnPoint = spawnPointFinder.GetRandomSpawnPoint();
            spawnPoint.z = -7f;

            GameObject skeletonInstance = Instantiate(enemyPrefab, spawnPoint, Quaternion.identity);
            EnemyMovement skeletonBehavior = skeletonInstance.GetComponent<EnemyMovement>();
            Collider2D attackHitboxCollider = skeletonInstance.transform.Find("HitBox").GetComponent<Collider2D>();
            skeletonBehavior.SetAttackHitboxCollider(attackHitboxCollider);

            if (skeletonBehavior != null && playerReference != null)
            {
                skeletonBehavior.SetPlayer(playerReference);
            }
        }
    }
}