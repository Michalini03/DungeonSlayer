using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject skeletonPrefab;
    [SerializeField] private GameObject flyingEyePrefab;
    [SerializeField] private GameObject ratPrefab;

    [Header("Spawning Setup")]
    [SerializeField] private TileMapSpawnPointFinder spawnPointFinder;

    [Header("Scene Management & UI")]
    [SerializeField] private string nextSceneName = "NextScene";
    [SerializeField] private UIFader sceneFader;
    [SerializeField] private float delayBeforeLoad = 1.5f;
    [SerializeField] private AugmentSelectionUI augmentSelectionUI;

    public bool selectedAugment = false;

    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool allEnemiesSpawned = false;
    private bool isTransitioning = false;

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

        if (activeEnemies.Count == 0 && !augmentSelectionUI.isSelecting && !selectedAugment)
        {
            if (augmentSelectionUI.gameObject.activeSelf)
                augmentSelectionUI.Hide();
            else
                augmentSelectionUI.ShowSelection();
        }

        if (activeEnemies.Count == 0 && selectedAugment && !isTransitioning)
        {
            isTransitioning = true;
            StartCoroutine(TransitionToNextLevel());
        }
    }

    private IEnumerator TransitionToNextLevel()
    {
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
        SpawnSingleEnemy(spawnPointFinder.skeletonSpawnPoints, skeletonPrefab);
        SpawnSingleEnemy(spawnPointFinder.flyingEyeSpawnPoints, flyingEyePrefab);
        SpawnSingleEnemy(spawnPointFinder.ratSpawnPoints, ratPrefab);
    }

    private void SpawnSingleEnemy(List<Vector3> spawnPoints, GameObject prefab)
    {
        foreach (Vector3 spawnPoint in spawnPoints)
        {
            GameObject enemy = Instantiate(prefab, spawnPoint, Quaternion.identity);
            activeEnemies.Add(enemy);
        }
    }
}