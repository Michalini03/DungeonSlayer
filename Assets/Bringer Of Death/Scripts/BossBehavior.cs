using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Collections;

public enum EnumBossState
{
    FoesState,
    CastSpellState,
}

public class BossBehavior : MonoBehaviour
{
    [Header("Boss Settings")]
    Rigidbody2D rb;
    [SerializeField] private GameObject spellPrefab;
    [SerializeField] private GameObject player;
    private PlayerMovement playerMovement;
    [SerializeField] private Vector2 castSpellOffset;
    [SerializeField] private Vector2 castSpellOnSelfOffset;
    [SerializeField] private EnumBossState bossState;
    [SerializeField] private bool ignorePlayerCollision = true;
    [SerializeField] private string enemyLayerName = "Enemy";
    [SerializeField] private string playerLayerName = "Player";
    [SerializeField] private BossBehaviorGX bossBehaviorGX;
    
    [SerializeField] private int foesStateHealthThreshold = 500;
    [SerializeField] private int maxHealth = 1500;
    [SerializeField] private int health = 1500;
    [SerializeField] private bool isDead = false;

    [Header("Spell Settings")]
    private float spellTimer;
    private float spellInterval = 5f;

    [Header("Spawn Enemies Logic")]
    [SerializeField] private GameObject skeletonPrefab;
    [SerializeField] private GameObject flyingEyePrefab;
    [SerializeField] private GameObject goblinPrefab;
    [SerializeField] private GameObject ratPrefab;
    [SerializeField] private TileMapSpawnPointFinder spawnPointFinder;
    private int positionIndex = 0;
    private int tileMapSpawnPointIndex = 0;
    private List<List<Vector3>> spawnPointsLists = new List<List<Vector3>>();
    private List<GameObject> waveSet = new List<GameObject>();
    private float spawnTimer = 0f;
    [SerializeField] private float spawnInterval = 2f;
    private bool canSpawn = true;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackDuration = 0.15f;
    [SerializeField] private float knockbackResistance = 1f;

    private bool isKnockedBack = false;
    private Coroutine knockbackCoroutine;

    [Header("Boss UI")]
    [SerializeField] private BossHealthBarUI bossHealthBarUI;
    [SerializeField] private string bossDisplayName = "Vratimor, the Shadow Lich";

    [Header("Boss Shield")]
    [SerializeField] private GameObject bossShield;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = player.GetComponent<PlayerMovement>();

        health = maxHealth;

        spellTimer = spellInterval;
        ConfigureCollisionRules();
        getSpawnPointsLists();

        if (bossHealthBarUI != null)
        {
            bossHealthBarUI.Initialize(bossDisplayName, health, maxHealth);
        }
    }

    private void adjustSpellTimerAndCast()
    {
        spellTimer -= Time.deltaTime;
        if (spellTimer <= 0f)
        {
            spellTimer = spellInterval;
            bossBehaviorGX.animator.SetTrigger("castSpell");
        }
    }

    private void ConfigureCollisionRules()
    {
        int enemyLayer = LayerMask.NameToLayer(enemyLayerName);
        int playerLayer = LayerMask.NameToLayer(playerLayerName);

        if (enemyLayer == -1)
        {
            Debug.LogWarning($"Layer '{enemyLayerName}' was not found. Boss collision setup skipped.", this);
            return;
        }

        gameObject.layer = enemyLayer;

        Physics2D.IgnoreLayerCollision(enemyLayer, enemyLayer, true);

        if (ignorePlayerCollision && playerLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(enemyLayer, playerLayer, true);
        }
    }

    private void getSpawnPointsLists()
    {
        spawnPointsLists.Clear();
        spawnPointsLists.Add(spawnPointFinder.skeletonSpawnPoints);
        spawnPointsLists.Add(spawnPointFinder.flyingEyeSpawnPoints);
        spawnPointsLists.Add(spawnPointFinder.goblinSpawnPoints);
        spawnPointsLists.Add(spawnPointFinder.ratSpawnPoints);
    }

    void Update()
    {
        if(player == null)
        {
            Debug.LogWarning("BossBehavior: Hráč není přiřazen! Boss nebude moci kouzlit.");
            return;
        }

        if (isKnockedBack)
        {
            return;
        }

        if (bossState == EnumBossState.FoesState)
        {
            FoesStateUpdate();
        }
        else if(bossState == EnumBossState.CastSpellState)
        {
            CastSpellStateUpdate();
        }
    }

    private void FoesStateUpdate()
    {
        if (isDead)
        {
            return;
        }

        UpdateWaveSet();
        checkEnemySpawnConditions();

        if (canSpawn)
        {
            manageSpawn();
        }
    }

    private void checkEnemySpawnConditions()
    {
        int aliveEnemies = GetAliveEnemiesCount();
        if (aliveEnemies == 0 && canSpawn == false)
        {
            bossShield.SetActive(false);
            canSpawn = true;
        }
    }

    private void CastSpellStateUpdate()
    {
        if (isDead)
        {
            return;
        }
        adjustSpellTimerAndCast();

        UpdateWaveSet();
        checkEnemySpawnConditions();

        if (canSpawn)
        {
            manageSpawn();
        }
    }

    public void CastSpell()
    {
        // 1. Kontrola, zda máme přiřazený prefab
        if (spellPrefab == null)
        {
            Debug.LogError("Chybí spellPrefab v BossBehavior!");
            return;
        }

        // 2. Kontrola, zda hráč stále žije/existuje
        if (player != null)
        {   
            if (playerMovement.IsInAir())
            {   
                return;
            }
            Instantiate(spellPrefab, player.transform.position + (Vector3)castSpellOffset, Quaternion.identity);
        }

        else
        {
            Debug.LogWarning("Boss se pokusil kouzlit, ale hráč už neexistuje (asi je po smrti).");
        }
    }

    private void castSpellOnSelf()
    {
        if (spellPrefab == null)
        {
            Debug.LogError("Chybí spellPrefab v BossBehavior!");
            return;
        }

        GameObject spell = Instantiate(spellPrefab, transform.position + (Vector3)castSpellOnSelfOffset, Quaternion.identity);
        spell.transform.localScale = new Vector3(2f, 2f, 2f);
    }

    private void SpawnSingleEnemy()
    {
        // 1. Kontrola, zda vůbec existují kategorie (seznamy)
        if (spawnPointsLists == null || spawnPointsLists.Count == 0) return;

        // 2. Získání aktuálního seznamu bodů
        List<Vector3> currentList = spawnPointsLists[tileMapSpawnPointIndex];

        // 3. KRITICKÁ KONTROLA: Je v tomto konkrétním seznamu alespoň jeden bod?
        if (currentList == null || currentList.Count == 0)
        {
            Debug.LogWarning($"Seznam spawn pointů pro index {tileMapSpawnPointIndex} je prázdný!");
            MoveToNextList(); // Přeskočíme na další seznam, abychom se nezasekli
            return;
        }

        // 4. Bezpečné získání prefabu
        GameObject enemyPrefab = GetEnemyPrefabByIndex(tileMapSpawnPointIndex);

        // 5. Bezpečné získání pozice (nyní víme, že currentList.Count > 0)
        Vector3 spawnPoint = currentList[positionIndex];
        GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPoint, Quaternion.identity);
        bossBehaviorGX.animator.SetTrigger("spawnEnemy");
        waveSet.Add(spawnedEnemy);

        // 6. Logika posunu indexů
        AdvanceIndexes(currentList.Count);
    }

    private void flipBossDirection()
    {
        if (player == null) return;

        float scale_x = transform.localScale.x;
        float scale_y = transform.localScale.y;
        this.transform.localScale = new Vector3(-scale_x, scale_y, 1);
    }

    // Pomocná metoda pro posun indexů
    private void AdvanceIndexes(int currentListSize)
    {
        if (positionIndex < currentListSize - 1)
        {
            positionIndex++;
        }
        else
        {
            positionIndex = 0;
            MoveToNextList();
        }
    }

    public void moveToNextPosition()
    {
        float posX = this.transform.position.x;
        float posY = this.transform.position.y;
        this.transform.position = new Vector3(posX - 7f, posY, 0f);
    }

    private void MoveToNextList()
    {
        if (tileMapSpawnPointIndex < spawnPointsLists.Count - 1)
        {
            tileMapSpawnPointIndex++;
        }
        else
        {
            tileMapSpawnPointIndex = 0;
            bossShield.SetActive(true);
            canSpawn = false;  
        }
    }

    private void UpdateWaveSet()
    {
        // Odstraní ze seznamu všechny položky, které jsou null (tedy zničené objekty)
        waveSet.RemoveAll(item => item == null);
    }

    // Příklad, jak zjistit aktuální počet živých nepřátel:
    private int GetAliveEnemiesCount()
    {
        UpdateWaveSet();
        return waveSet.Count;
    }

    private GameObject GetEnemyPrefabByIndex(int index)
    {
        switch (index)
        {
            case 0: return skeletonPrefab;
            case 1: return flyingEyePrefab;
            case 2: return goblinPrefab;
            default: return ratPrefab;
        }
    }

    private void manageSpawn()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnSingleEnemy();
        }
    }

    public void manageEnemyHit(int playerDamage, Vector2 sourcePosition, float knockbackForce)
    {
        if (isDead) return;

        if (!canSpawn)
        {
            // Boss je ve fazi kdy nesspawnuje takze je nezranitelny
            return;
        }

        health -= playerDamage;
        health = Mathf.Max(health, 0);

        if (bossHealthBarUI != null)
        {
            bossHealthBarUI.SetHealth(health, maxHealth);
        }

        ApplyKnockback(sourcePosition, knockbackForce);

        if (health <= foesStateHealthThreshold && bossState == EnumBossState.FoesState)
        {
            bossState = EnumBossState.CastSpellState;
            bossShield.SetActive(false);
            bossBehaviorGX.animator.SetTrigger("teleport");
            moveToNextPosition();
            flipBossDirection();
            Debug.Log("Boss přechází do CastSpellState!");
        }
        else if (health <= 0)
        {
            isDead = true;
            Debug.Log("Boss je mrtvý!");

            if (bossHealthBarUI != null)
            {
                bossHealthBarUI.Hide();
            }
        }
        else
        {
            bossBehaviorGX.animator.SetTrigger("tookHit");
        }
    }

    public void ApplyKnockback(Vector2 sourcePosition, float force)
    {
        if (isDead || rb == null)
        {
            return;
        }

        Vector2 direction = ((Vector2)transform.position - sourcePosition).normalized;

        if (direction.sqrMagnitude <= 0.001f)
        {
            direction = transform.localScale.x >= 0 ? Vector2.right : Vector2.left;
        }

        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }

        knockbackCoroutine = StartCoroutine(KnockbackRoutine(direction, force));
    }

    private IEnumerator KnockbackRoutine(Vector2 direction, float force)
    {
        isKnockedBack = true;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction * (force / Mathf.Max(knockbackResistance, 0.01f)), ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        isKnockedBack = false;
        knockbackCoroutine = null;
    }
}
