using Pathfinding;
using UnityEngine;
using System.Collections;

public class FlyingEyeBehavior : MonoBehaviour
{
    [Header("Appearance")]
    [SerializeField] private int size = 3;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Movement Settings")]
    [SerializeField] private TileMapSpawnPointFinder spawnPointFinder;
    [SerializeField] private float moveSpeedChase = 300;
    [SerializeField] private float moveSpeedPatrol = 170f;
    [SerializeField] private float maxSpeedChaseAdjustment = 100f;
    [SerializeField] private float maxSpeedPatrolAdjustment = 50f;
    [SerializeField] private float insideTrigerRange = 5f;
    [SerializeField] private float outsideTrigerRange = 8f;
    private Vector3 targetPosition;
    private bool isPatrolling = true;

    [Header("Stats")]
    [SerializeField] private float health = 120f;
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private string playerLayerName = "Player";
    private float attackCooldown = 1f;
    private float currentCooldown = 0f;
    [SerializeField] private bool isDead = false;

    public GameObject Player;
    public float nextWaypointDistance = 1f;

    private Path path;
    int currentWaypoint = 0;
    bool reachedEndOfPath = false;

    Seeker seeker;
    Rigidbody2D rb;

    [Header("Death Settings")]
    [SerializeField] private float deathAnimationLength = 1.0f;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackDuration = 0.15f;
    [SerializeField] private float knockbackResistance = 0.5f;

    private bool isKnockedBack = false;
    private Coroutine knockbackCoroutine;

    void Start()
    {
        speedAdjustment();
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        InvokeRepeating("UpdatePath", 0f, 0.5f);
        setNewPatrolTarget();
    }

    void UpdatePath()
    {
        if (isDead)
        {
            return;
        }

        Vector3 targetPos = isPatrolling ? targetPosition : Player.transform.position;
        if (seeker.IsDone())
        {
            seeker.StartPath(transform.position, targetPos, OnPathComplete);
        }
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    void flipCharecter()
    {
        if (rb.linearVelocity.x > 0.01f)
        {
            transform.localScale = new Vector3(size, size, size);
        }
        else if (rb.linearVelocity.x < -0.01f)
        {
            transform.localScale = new Vector3(-size, size, size);
        }
    }

    void FixedUpdate()
    {
        if (isDead)
        {
            return;
        }

        if (isKnockedBack)
        {
            return;
        }

        managePatrolTarget();
        manageAttackCooldown();
        flipCharecter();
        manageTrigerRange();
    }

    void Update()
    {
        if (isDead || isKnockedBack)
        {
            return;
        }

        if (path == null)
            return;

        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else
        {
            reachedEndOfPath = false;
        }

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = direction * (isPatrolling ? moveSpeedPatrol : moveSpeedChase) * Time.deltaTime;
        rb.AddForce(force);

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }
    }


    private void manageAttackCooldown()
    {
        if (Player == null)
        {
            animator.SetBool("Attack", false);
            return;
        }

        float dist = Vector3.Distance(transform.position, Player.transform.position);

        if (dist < attackRange && currentCooldown <= 0)
        {
            animator.SetBool("Attack", true);
            currentCooldown = attackCooldown;
        }
        else if (currentCooldown > 0)
        {
            currentCooldown -= Time.fixedDeltaTime;
        }
        else
        {
            animator.SetBool("Attack", false);
        }
    }

    private void speedAdjustment()
    {
        moveSpeedPatrol = moveSpeedPatrol + Random.Range(-maxSpeedPatrolAdjustment, maxSpeedPatrolAdjustment);
        moveSpeedChase = moveSpeedChase + Random.Range(-maxSpeedChaseAdjustment, maxSpeedChaseAdjustment);
    }

    private void managePatrolTarget()
    {
        Vector2 enemyPos2D = new Vector2(transform.position.x, transform.position.y);
        Vector2 targetPos2D = new Vector2(targetPosition.x, targetPosition.y);

        float dist = Vector2.Distance(enemyPos2D, targetPos2D);

        if (isPatrolling && (dist < nextWaypointDistance || reachedEndOfPath))
        {
            setNewPatrolTarget();
        }
    }

    private void setNewPatrolTarget()
    {
        if (isPatrolling && !isDead)
        {
            if (!spawnPointFinder)
            {
                spawnPointFinder = FindAnyObjectByType<TileMapSpawnPointFinder>();
            }

            if (spawnPointFinder != null)
            {
                targetPosition = spawnPointFinder.GetRandomSpawnPoint(EnemyType.FlyingEye);
            }
        }
    }

    private void manageTrigerRange()
    {
        if (Player == null) return;

        float dist = Vector3.Distance(transform.position, Player.transform.position);

        if (dist < insideTrigerRange)
        {
            isPatrolling = false;
        }
        else if (dist > outsideTrigerRange)
        {
            isPatrolling = true;
        }
    }

    // Stejne jako u skeletona
    public void manageEnemyHit(int playerDamage, Vector2 sourcePosition, float knockbackForce)
    {
        // Prevent taking damage if already dead
        if (health <= 0 || isDead)
        {
            return;
        }

        health -= playerDamage;
        ApplyKnockback(sourcePosition, knockbackForce);

        if (health <= 0)
        {
            isDead = true;
            animator.SetTrigger("tookHit");
            animator.SetBool("isDead", true);

            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 2f;
            gameObject.layer = LayerMask.NameToLayer("Default");
            ignorePlayerCollision();

            StartCoroutine(DeathRoutine());
        }
        else
        {
            animator.SetTrigger("tookHit");
        }
    }

    private IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(deathAnimationLength);
        destroyEnemy();
    }


    // vola se v animaci utoku, v jeden frame
    private void AttackHitboxLogic()
    {
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, LayerMask.GetMask(playerLayerName));
        if (hitPlayers.Length > 0)
        {
            hitPlayers[0].gameObject.GetComponent<PlayerCombat>().takeDamage(attackDamage);
        }
    }

    // Toto se muze volat na konci animace pro smrt. Vsechny potrebne animaci by tam mely byt.
    public void destroyEnemy()
    {
        Destroy(gameObject);
    }

    private void ignorePlayerCollision()
    {
        if (Player == null)
        {
            return;
        }

        Collider2D[] enemyColliders = GetComponentsInChildren<Collider2D>();
        Collider2D[] playerColliders = Player.GetComponentsInChildren<Collider2D>();

        foreach (Collider2D enemyCollider in enemyColliders)
        {
            foreach (Collider2D playerCollider in playerColliders)
            {
                Physics2D.IgnoreCollision(enemyCollider, playerCollider, true);
            }
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, insideTrigerRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, outsideTrigerRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
