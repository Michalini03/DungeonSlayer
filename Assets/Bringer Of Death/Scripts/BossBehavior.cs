using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System;
using Unity.VisualScripting;
using Pathfinding;

public enum EnumBossState
{
    FoesState,
    CastSpellState,
    CombatState
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
    
    [SerializeField] private int foesStateHealthThreshold = 400;
    [SerializeField] private int castSpellStateHealthThreshold = 800;
    [SerializeField] private int maxHealth = 1200;
    [SerializeField] private int health;
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
    private Vector3 playerSpawnPosition;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackDuration = 0.15f;
    [SerializeField] private float knockbackResistance = 1f;

    private bool isKnockedBack = false;
    private Coroutine knockbackCoroutine;

    [Header("Boss UI")]
    [SerializeField] private BossHealthBarUI bossHealthBarUI;
    [SerializeField] private string bossDisplayName = "Vratimor, the Shadow Lich";

    [Header("Attack Settings")]
    private float attackTimer = 0f;
    [SerializeField] private float attackInterval = 10f;
    [SerializeField] private GameObject AttackPoint;
    [SerializeField] private int attackDiameter = 50;
    [SerializeField] private int bossCombatDamage = 20;
    [Header("Boss Shield")]
    [SerializeField] private GameObject bossShield;

    [Header("Map settings")]
    [SerializeField] private GameObject forestBackground;
    [SerializeField] private GameObject caveBackground;
    [SerializeField] private GameObject castleBackground;

    [SerializeField] private GameObject forestTilemap;
    [SerializeField] private GameObject caveTilemap;
    [SerializeField] private GameObject castleTilemap;

    [SerializeField] private GameObject fogRight;
    [SerializeField] private GameObject fogLeft;

    [Header("SpawnPoints")]
    [SerializeField] private GameObject spawnPointCave;
    [SerializeField] private GameObject spawnPointForest;

    [Header("Transition Settings")]
    [SerializeField] private CanvasGroup fadeScreen;
    [SerializeField] private float fadeDuration = 1f;
    private bool isTransitioning = false;
    [Header("Boss Manager Reference")]
    [SerializeField] private BossManager bossManager;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = player.GetComponent<PlayerMovement>();

        playerSpawnPosition = player.transform.position;

        health = maxHealth;

        spellTimer = spellInterval;
        ConfigureCollisionRules();
        getSpawnPointsLists();
        
        this.health = maxHealth;

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

    public void checkAndDamagePlayer()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(AttackPoint.transform.position, player.transform.position);
        if (distanceToPlayer <= attackDiameter / 2)
        {
            if (player != null)
            {
                player.GetComponent<PlayerCombat>().takeDamage(bossCombatDamage);
            }
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

        if (isTransitioning) return;

        if (bossState == EnumBossState.FoesState)
        {
            FoesStateUpdate();
        }
        else if(bossState == EnumBossState.CastSpellState)
        {
            CastSpellStateUpdate();
        }
        else if(bossState == EnumBossState.CombatState)
        {
            CombatStateUpdate();
        }
    }

    // UPDATE pro první fázi, kdy boss spawnuje nepřátele
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

    // UPDATE pro druhou fázi, kdy boss začíná útočit blesky
    private void CastSpellStateUpdate()
    {
        if (isDead)
        {
            bossShield.SetActive(false);
            canSpawn = true;
            return;
        }

        UpdateWaveSet();
        checkEnemySpawnConditions();

        if (canSpawn)
        {
            manageSpawn();
        }

        adjustSpellTimerAndCast();
    }

    // UPDATE pro třetí fázi, kdy boss začíná mlátit
    private void CombatStateUpdate()
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

        manageAttack();
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

    public void moveToNextPosition(GameObject spawnObject)
    {
        // Check if the object we passed in has a Tilemap component
        Tilemap tilemap = spawnObject.GetComponent<Tilemap>();
        Vector2 targetPosition;

        if (tilemap != null)
        {
            // Compress bounds forces the tilemap to calculate exactly where tiles are painted
            tilemap.CompressBounds(); 
        
            // Get the coordinate of the bottom-left-most painted tile
            Vector3Int cellPosition = tilemap.cellBounds.min; 
        
            // Convert that grid coordinate into real-world Unity units
            targetPosition = tilemap.GetCellCenterWorld(cellPosition);

            targetPosition.y -= 0.25f;
        }
        else
        {
            // Fallback: If it's just a normal GameObject, use its transform
            targetPosition = spawnObject.transform.position;
        }

        Debug.Log($"Přesouvám se na pozici: {targetPosition}");

        // Teleport using the Rigidbody
        if (rb != null)
        {
            rb.position = targetPosition;
            rb.linearVelocity = Vector2.zero; 
        }
        else
        {
            this.transform.position = new Vector3(targetPosition.x, targetPosition.y, this.transform.position.z);
        }
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

    private void manageAttack()
    {
        attackTimer += Time.deltaTime;
        float attackPointAndPlayerDistance = Vector2.Distance(AttackPoint.transform.position, player.transform.position);
        if (attackTimer >= attackInterval && attackPointAndPlayerDistance <= attackDiameter / 2)
        {
            attackTimer = 0f;
            bossBehaviorGX.animator.SetTrigger("attack");
        }
    }

    public void manageEnemyHit(int playerDamage, Vector2 sourcePosition, float knockbackForce)
    {
        if (isDead || isTransitioning) return; // Ignore hits while dying or transitioning maps

        if (!canSpawn)
        {
            return; // Boss is invulnerable
        }

        health -= playerDamage;
        health = Mathf.Max(health, 0);

        if (bossHealthBarUI != null)
        {
            bossHealthBarUI.SetHealth(health, maxHealth);
        }

        ApplyKnockback(sourcePosition, knockbackForce);

        // Check for Phase Transitions
        if (health <= foesStateHealthThreshold && bossState == EnumBossState.FoesState)
        {
            Debug.Log("Boss přechází do CastSpellState!");
            StartCoroutine(MapTransitionRoutine(EnumBossState.CastSpellState));
        }
        else if (health <= castSpellStateHealthThreshold && bossState == EnumBossState.CastSpellState)
        {
            Debug.Log("Boss přechází do CombatState!");
            StartCoroutine(MapTransitionRoutine(EnumBossState.CombatState));
        }
        else if (health <= 0)
        {
            isDead = true;

            if (bossHealthBarUI != null)
            {
                bossHealthBarUI.Hide();
            }

            bossBehaviorGX.animator.SetTrigger("death");

            DestroyBossAndAllEnemies();
        }
        else
        {
            bossBehaviorGX.animator.SetTrigger("tookHit");
        }
    }

    private IEnumerator MapTransitionRoutine(EnumBossState nextState)
    {
        isTransitioning = true;

        fadeScreen.blocksRaycasts = true;

        bossBehaviorGX.animator.SetTrigger("teleport");

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeScreen.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }

        fadeScreen.alpha = 1f;

        if (nextState == EnumBossState.CastSpellState)
        {
            bossState = EnumBossState.CastSpellState;
            bossShield.SetActive(false);
            moveToNextPosition(spawnPointCave);
            flipBossDirection();

            castleBackground.SetActive(false);
            caveBackground.SetActive(true);
            castleTilemap.SetActive(false);
            caveTilemap.SetActive(true);

            player.transform.position = playerSpawnPosition;

            if (AstarPath.active != null)
            {
                AstarPath.active.Scan();
            }
            else
            {
                Debug.LogWarning("A* Pathfinding není aktivní! Ujistěte se, že máte AstarPath komponentu ve scéně.");
            }
        }
        else if (nextState == EnumBossState.CombatState)
        {
            bossState = EnumBossState.CombatState;
            moveToNextPosition(spawnPointForest);
            flipBossDirection();

            caveBackground.SetActive(false);
            forestBackground.SetActive(true);
            caveTilemap.SetActive(false);
            forestTilemap.SetActive(true);

            fogRight.SetActive(true);
            fogLeft.SetActive(true);

            player.transform.position = playerSpawnPosition;

            if (AstarPath.active != null)
            {
                AstarPath.active.Scan();
            }
            else
            {
                Debug.LogWarning("A* Pathfinding není aktivní! Ujistěte se, že máte AstarPath komponentu ve scéně.");
            }
        }

        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeScreen.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }

        fadeScreen.alpha = 0f;
        fadeScreen.blocksRaycasts = false;

        isTransitioning = false;
    }

    private void destroyAllEnemies()
    {
        foreach (GameObject enemy in waveSet)
        {
            if (enemy != null)
            {
                enemy.GetComponent<EnemyHitInfo>().manageEnemyHit(9999, transform.position, 0f);
            }
        }
        waveSet.Clear();
    }

    public void DestroyBossAndAllEnemies()
    {
        StartCoroutine(DestroyRoutine(1.5f));
    }

    private IEnumerator DestroyRoutine(float delay)
    {
        destroyAllEnemies();

        yield return new WaitForSeconds(delay);

        if (bossManager != null)
        {
            bossManager.FadeToLevel("main_menu");
        }
        else
        {
            Debug.LogWarning("BossManager wasn't assigned in the Inspector! Trying to find it...");
            FindFirstObjectByType<BossManager>()?.FadeToLevel("main_menu");
        }
        Destroy(gameObject);
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

    private void OnDrawGizmosSelected()
    {
        if (AttackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(AttackPoint.transform.position, attackDiameter);
        }
    }
}
