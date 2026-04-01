using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnConfig
    {
        public EnemyType Type;
        public GameObject Prefab;
        public int Count;
    }

    [Header("Spawning Setup")]
    [SerializeField] private TileMapSpawnPointFinder spawnPointFinder;
    [SerializeField] private EnemySpawnConfig[] enemyGroups;

    [Header("Scene Management & UI")]
    [SerializeField] private string nextSceneName = "NextScene";
    [SerializeField] private UIFader sceneFader;
    [SerializeField] private float delayBeforeLoad = 1.5f;

    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool allEnemiesSpawned = false;

    private void Start()
    {
        if (sceneFader != null)
        {
            sceneFader.FadeIn();
        }

        SpawnEnemies();
        allEnemiesSpawned = true;
    }

    private void Update()
    {
        if (!allEnemiesSpawned) return;

        activeEnemies.RemoveAll(enemy => enemy == null);

        if (activeEnemies.Count == 0)
        {
            allEnemiesSpawned = false;
            StartCoroutine(TransitionToNextLevel());
        }
    }

    private IEnumerator TransitionToNextLevel()
    {
        Debug.Log("Level Complete! Fading out...");

        if (sceneFader != null)
        {
            sceneFader.FadeOut();
        }

        yield return new WaitForSeconds(delayBeforeLoad);

        SceneManager.LoadScene(nextSceneName);
    }

    private void SpawnEnemies()
    {
        GameObject playerReference = spawnPointFinder.player;

        foreach (var group in enemyGroups)
        {
            if (group.Prefab == null) continue;

            for (int i = 0; i < group.Count; i++)
            {
                SpawnSingleEnemy(group.Prefab, group.Type, playerReference);
            }

            group.Prefab.SetActive(false);
        }
    }

    private void SpawnSingleEnemy(GameObject prefab, EnemyType enemyType, GameObject player)
    {
        Vector3 spawnPoint = spawnPointFinder.GetRandomSpawnPoint(enemyType);
        
        spawnPoint.z = player.transform.position.z;

        GameObject instance = Instantiate(prefab, spawnPoint, Quaternion.identity);
        
        activeEnemies.Add(instance);
    }
}