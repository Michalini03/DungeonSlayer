using Unity.Netcode;
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
    [SerializeField] private AugmentSelectionUI augmentSelectionUI;

    public bool selectedAugment = false;

    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool allEnemiesSpawned = false;
    private bool isTransitioning = false;

    private bool IsMultiplayer()
    {
        return GameNetworkManager.Instance != null && GameNetworkManager.Instance.IsMultiplayer;
    }

    private void Start()
    {
        if (sceneFader != null)
        {
            sceneFader.FadeIn();
        }

        // In multiplayer, only the host spawns enemies
        if (IsMultiplayer() && !NetworkManager.Singleton.IsServer)
        {
            allEnemiesSpawned = true;
            return;
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

        if (IsMultiplayer() && NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
        }
        else if (!IsMultiplayer())
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private void SpawnEnemies()
    {
        GameObject playerReference = spawnPointFinder.player;
        if (playerReference == null)
        {
            // In multiplayer the scene player ref might not be set; use registry
            playerReference = PlayerRegistry.GetFirstPlayer();
        }

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

        if (player != null)
            spawnPoint.z = player.transform.position.z;

        GameObject instance = Instantiate(prefab, spawnPoint, Quaternion.identity);

        // In multiplayer, spawn as network object so clients can see it
        if (IsMultiplayer() && NetworkManager.Singleton.IsServer)
        {
            var netObj = instance.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn();
            }
        }

        activeEnemies.Add(instance);
    }
}