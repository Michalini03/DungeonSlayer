using System.Collections;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private int health = 100;
    [SerializeField] private GameObject attackHitbox;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject player;
    [SerializeField] private float normalSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float insideTrigerRange = 3f;
    [SerializeField] private float outsideTrigerRange = 5f;
    [SerializeField] private float speedAnimationMultiplier = 2.0f;
    private bool canSeePlayer = false;

    [Header("Collision Settings")]
    [SerializeField] private string enemyLayerName = "Enemy";
    [SerializeField] private string playerLayerName = "Player";

    [Header("Attack Settings")]
    private Rigidbody2D rb;
    private float attackCooldown = 1f;
    private float currentCooldown = 0f;
    private bool canWalk;
    private bool isTrigered = false;

    [Header("Patrol AI")]
    [SerializeField] private Vector3 walkDirection = Vector3.right;
    [SerializeField] private float patrolProbeHeight = 0.5f;
    [SerializeField] private float patrolFootProbeForward = 0.45f;
    [SerializeField] private float patrolGroundCheckDistance = 1.2f;
    [SerializeField] private float patrolSlopeNormalThreshold = 0.98f;
    [SerializeField] private int patrolBlockedFramesToTurn = 3;
    [SerializeField] private float patrolTurnCooldown = 0.25f;
    
    [Header("Detection Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float wallCheckDistance = 1.0f;

    [Header("Attack Hitbox")]
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private Collider2D attackHitboxCollider;

    [Header("Deadh Settings")]
    [SerializeField] private float deathAnimationLength = 1.2f;
    private bool isDead = false;
    
    private int patrolBlockedFrames = 0;
    private float patrolNextTurnTime = 0f;


    //marek: tady je zdroj a range utoku kalibrovane na animaci, klidne se to muze nejak upravit, zatim prototyp
    public Transform attackPoint;
    public float trueAttackRange = 1f;

    private void Awake()
    {
        setRigidBody();
        setCanWalk(true);
        InitializeAnimator();
        ConfigureCollisionRules();
        DisableHitbox();
    }


    private void ConfigureCollisionRules()
    {
        int enemyLayer = LayerMask.NameToLayer(enemyLayerName);
        int playerLayer = LayerMask.NameToLayer(playerLayerName);

        if (enemyLayer == -1)
        {
            Debug.LogWarning($"Layer '{enemyLayerName}' was not found. Collision setup skipped.", this);
            return;
        }

        Physics2D.IgnoreLayerCollision(enemyLayer, enemyLayer, true);

        if (playerLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(enemyLayer, playerLayer, true);
        }
        else
        {
            Debug.LogWarning($"Layer '{playerLayerName}' was not found. Enemy-player collision ignore was not applied.", this);
        }
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        manageAttackCooldown();
        Move();
        checkTrigger();
        checkEnemyMapPosition();
        checkPlayerVisibility();
        
        if(isTrigered)
        {
            //CheckForJump();
        }
    }

    private void checkPlayerVisibility()
    {
        if (player == null) return;

        Vector2 directionToPlayer = player.transform.position - transform.position;
        // Raycast detekuje VŠECHNY collidery - pokud je zeď mezi skeletem a hráčem, vrátí zeď
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer.normalized, directionToPlayer.magnitude);

        canSeePlayer = hit.collider != null && hit.collider.gameObject == player;

        // Debug visualization - zelená = vidí, červená = nevidí
        Debug.DrawRay(transform.position, directionToPlayer.normalized * directionToPlayer.magnitude, canSeePlayer ? Color.green : Color.red);
    }

    private void Move()
    {
        Vector3 direction;
        if (!canWalk)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            direction = moveTowardsPlayer();
            flipCharacter(direction);
            return;
        }

        float currentSpeed = isTrigered && canSeePlayer ? chaseSpeed : normalSpeed;

        if(isTrigered && !canSeePlayer)
        {
            direction = getPatrolPoint();
        }
        else if(isTrigered && canSeePlayer)
        {
            direction = moveTowardsPlayer();
        }
        else
        {
            direction = getPatrolPoint();
        }

        if (Mathf.Abs(direction.x) < 0.01f)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        flipCharacter(direction);
        float horizontalMove = direction.x > 0 ? 1f : -1f;
        rb.linearVelocity = new Vector2(horizontalMove * currentSpeed, rb.linearVelocity.y);
    }

    private void CheckForJump()
    {
        float faceDir = transform.localScale.x > 0 ? 1 : -1;

        Vector2 chestOrigin = (Vector2)transform.position + new Vector2(0f, 0.5f);
        Vector2 wallEnd = chestOrigin + new Vector2(wallCheckDistance * faceDir, 0);
        RaycastHit2D hitWall = Physics2D.Linecast(chestOrigin, wallEnd, groundLayer);

        Vector2 footOrigin = (Vector2)transform.position + new Vector2(0.5f * faceDir, 0f);
        Vector2 holeEnd = footOrigin + new Vector2(0, -2f);
        RaycastHit2D hitFloor = Physics2D.Linecast(footOrigin, holeEnd, groundLayer);

        Debug.DrawLine(chestOrigin, wallEnd, Color.red);
        Debug.DrawLine(footOrigin, holeEnd, Color.blue);

        if (IsGrounded())
        {
            if (hitWall.collider != null || hitFloor.collider == null)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
        }
    }

    private bool IsGrounded()
    {
        Vector2 leftFoot = (Vector2)transform.position + new Vector2(-0.2f, 0f);
        Vector2 rightFoot = (Vector2)transform.position + new Vector2(0.2f, 0f);

        float checkDist = 1.1f;

        Debug.DrawRay(leftFoot, Vector2.down * checkDist, Color.green);
        Debug.DrawRay(rightFoot, Vector2.down * checkDist, Color.green);

        bool left = Physics2D.Raycast(leftFoot, Vector2.down, checkDist, groundLayer);
        bool right = Physics2D.Raycast(rightFoot, Vector2.down, checkDist, groundLayer);

        return left || right;
    }

    private void checkTrigger()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.transform.position);
        if (dist < insideTrigerRange && canSeePlayer) 
        {
            animator.SetFloat("walkSpeedMultiplier", speedAnimationMultiplier);
            isTrigered = true;
        }
        else if (dist > outsideTrigerRange)
        {
            animator.SetFloat("walkSpeedMultiplier", 1.0f);
            isTrigered = false;
        }
    }

    private Vector3 getPatrolPoint()
{
    // Základní inicializace směru
    if (Mathf.Abs(walkDirection.x) < 0.01f)
    {
        walkDirection = Vector3.right;
    }

    // Směr bereme z aktuálního otočení (Scale), aby Raycasty mířily vždy vpřed
    float faceDir = transform.localScale.x >= 0f ? 1f : -1f;

    // --- DETEKCE ZDI ---
    // Posuneme počátek paprsku trochu výš, aby skeleton nezakopával o drobné nerovnosti
    Vector2 wallOrigin = (Vector2)transform.position + new Vector2(0f, patrolProbeHeight);
    RaycastHit2D wallHit = Physics2D.Raycast(wallOrigin, Vector2.right * faceDir, wallCheckDistance, groundLayer);

    // --- DETEKCE PROPASTI A KOPCŮ ---
    // frontProbe vysíláme před skeletona. 
    // Důležité: patrolGroundCheckDistance musí být dostatečně velký (např. 1.5f - 2f), 
    // aby detekoval zem i při chůzi dolů ze svahu.
    Vector2 groundProbeOrigin = (Vector2)transform.position + new Vector2(faceDir * patrolFootProbeForward, 0.2f);
    RaycastHit2D groundHit = Physics2D.Raycast(groundProbeOrigin, Vector2.down, patrolGroundCheckDistance, groundLayer);

    // --- DETEKCE SKLONU (SLOPE) ---
    bool isSlopeTooSteep = false;
    if (groundHit.collider != null)
    {
        // Kontrolujeme úhel povrchu přes normálu (Y složka)
        // Čím menší je Y, tím strmější je kopec.
        isSlopeTooSteep = groundHit.normal.y < patrolSlopeNormalThreshold;
    }

    // Blokován, pokud: trefí zeď NEBO před ním není země (propast) NEBO je kopec moc strmý
    bool blocked = wallHit.collider != null || groundHit.collider == null || isSlopeTooSteep;

    if (blocked)
    {
        patrolBlockedFrames++;
    }
    else
    {
        patrolBlockedFrames = 0;
    }

    // Logika otočení
    bool canTurnNow = Time.time >= patrolNextTurnTime;
    if (blocked && patrolBlockedFrames >= patrolBlockedFramesToTurn && canTurnNow)
    {
        faceDir *= -1f;
        walkDirection = new Vector3(faceDir, 0f, 0f);
        
        // OKAMŽITĚ otočíme skeletona, aby příští frame Raycasty mířily správně
        flipCharacter(walkDirection); 
        
        patrolNextTurnTime = Time.time + patrolTurnCooldown;
        patrolBlockedFrames = 0;
    }
    else
    {
        // Udržujeme stávající směr
        walkDirection = new Vector3(faceDir, 0f, 0f);
    }

    return walkDirection;
}

    private Vector3 moveTowardsPlayer()
    {
        if (player == null)
        {
            return Vector3.zero;
        }

        return player.transform.position - transform.position;
    }

    private void flipCharacter(Vector3 direction)
    {
        // Simplified flipping logic to maintain your localScale of 2
        float x = direction.x < 0 ? -2f : 2f;
        transform.localScale = new Vector3(x, 2f, 2f);
    }

    private void manageAttackCooldown()
    {
        if (player == null)
        {
            animator.SetBool("Attack", false);
            canWalk = true;
            return;
        }

        float dist = Vector3.Distance(transform.position, player.transform.position);

        if (dist < attackRange && currentCooldown <= 0)
        {
            animator.SetBool("Attack", true);
            currentCooldown = attackCooldown;
            canWalk = false;
        }
        else if (currentCooldown > 0)
        {
            currentCooldown -= Time.fixedDeltaTime;
        }
        else
        {
            animator.SetBool("Attack", false);
            canWalk = true;
        }
    }

    public void EnableHitbox() => attackHitbox.SetActive(true);
    public void DisableHitbox() => attackHitbox.SetActive(false);

    public void SetPlayer(GameObject playerReference)
    {
        player = playerReference;
    }

    // Visualize the jump detection in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 dir = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        Gizmos.DrawRay(transform.position, dir * wallCheckDistance);
        Gizmos.DrawWireSphere(transform.position + new Vector3(0, -0.8f, 0), 0.2f);

        // Detection ranges - viditelné jen když je vybraný
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, insideTrigerRange);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, outsideTrigerRange);

        // Attack range
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, trueAttackRange);
        }
    }

    private void InitializeAnimator()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("Animator component not found on the enemy.", this);
            return;
        }
        animator.SetBool("canWalk", true);
    }

    private void setCanWalk(bool value)
    {
        canWalk = value;
    }

    private void setRigidBody()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Jen nastavuje hitbox collider, pac je to debilne dany jako child objekt
    public void SetAttackHitboxCollider(Collider2D collider)
    {
        attackHitboxCollider = collider;

        if (attackHitboxCollider != null)
        {
            attackHitboxCollider.isTrigger = true;
            int defaultLayer = LayerMask.NameToLayer("Default");
            if (defaultLayer != -1)
            {
                attackHitboxCollider.gameObject.layer = defaultLayer;
            }
        }
    }

    // marek: dopsal jsem si tady aby enemak mohl mlatit i mecem
    private void AttackHitboxLogic()
    {
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, trueAttackRange, LayerMask.GetMask(playerLayerName));
        if (hitPlayers.Length > 0)
        {
            hitPlayers[0].gameObject.GetComponent<PlayerCombat>().takeDamage(attackDamage);
        }
    }

//-----------------------------------------ENEMY GETTING HIT BY PLAYER LOGIC-----------------------------------------

private void OnTriggerEnter2D(Collider2D other)
    {
        if (attackHitbox == null || !attackHitbox.activeInHierarchy)
        {
            return;
        }

        GameObject hitObject = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;
        if (player != null && (hitObject == player || hitObject.transform.root.gameObject == player))
        {
            Debug.Log("Player hit by attack!");
            // DisableHitbox() je tady dulezitej aby se nedal 2x hit. Pls pls nemazat  
            DisableHitbox();
        }
    }

    // Tady si pak pridej klidne vice paramentru jak budes potrebovat (knockbackForce, hitEffect, atd.) 
    public void manageEnemyHit(int playerDamage)
    {
        // Prevent the enemy from taking more hits or triggering death twice
        if (isDead) return;

        health -= playerDamage;

        if (health <= 0)
        {
            isDead = true;

            // 1. Play death animations
            animator.SetTrigger("tookHit");
            animator.SetBool("isDead", true);

            // 2. Disable hitboxes and colliders immediately
            DisableHitbox();
            disableColiders();

            // Stop the Rigidbody so the enemy doesn't fall through the floor 
            // now that its colliders are turned off
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;

            // 3. Start the timer to destroy the object
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
        DestroyEnemy();
    }

    public void DestroyEnemy()
    {
        Debug.Log("Enemy destroyed!");
        Destroy(gameObject);
    }

    public void disableColiders()
    {
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }
    }

    private void checkEnemyMapPosition()
    {
        if (transform.position.y < -10f)
        {
            DestroyEnemy();
        }
    }
}