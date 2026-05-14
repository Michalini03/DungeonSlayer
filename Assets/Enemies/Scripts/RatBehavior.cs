using System.Collections;
using UnityEngine;

public class RatBehavior : MonoBehaviour
{
    [Header("Nastavení pohybu")]
    [SerializeField] private float speed = 1.0f;
    [SerializeField] private float speedConstant;
    [SerializeField] private float maxSpeedAdjustment = 0.7f;
    [SerializeField] private GameObject player;
    [SerializeField] private float playerTriggerDistance = 3.0f;
    [SerializeField] private int lookMinTime = 7;
    [SerializeField] private int lookMaxTime = 15;
    private Rigidbody2D rb;
    private int direction = 1;
    private bool isMoving = true;
    [SerializeField] private int health = 100;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int healthRegenAmount = 30;
    [SerializeField] private string playerLayerName = "Player";
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private float attackCooldownConstant = 1.5f;
    private float attackCooldown = 2f;

    [Header("Detekce země (Červená čára)")]
    [SerializeField] private float groundDistance = 0.5f;
    [SerializeField] private Vector2 groundOffset = new Vector2(0.5f, -0.5f);

    [Header("Detekce stěny (Zelená čára)")]
    [SerializeField] private float wallDistance = 0.2f;
    [SerializeField] private Vector2 wallOffset = new Vector2(0.5f, 0f);

    [Header("Fyzika")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Animace")]
    [SerializeField] private Animator animator;
    [SerializeField] private float lookAroundTimer;

    [Header("Deadh Settings")]
    [SerializeField] private float deathAnimationLength = 2.0f;
    private bool isDead = false;    

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackDuration = 0.15f;
    [SerializeField] private float knockbackResistance = 0.1f;

    private bool isKnockedBack = false;
    private Coroutine knockbackCoroutine;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        adjustSpeed();
        adjustLookAroundTimer();
    }

    void FixedUpdate()
    {
        if (isKnockedBack)
        {
            return;
        }

        Vector2 currentGroundOffset = new Vector2(groundOffset.x * direction, groundOffset.y);
        Vector2 groundOrigin = (Vector2)transform.position + currentGroundOffset;

        Vector2 currentWallOffset = new Vector2(wallOffset.x * direction, wallOffset.y);
        Vector2 wallOrigin = (Vector2)transform.position + currentWallOffset;

        RaycastHit2D groundHit = Physics2D.Raycast(groundOrigin, Vector2.down, groundDistance, groundLayer);
        RaycastHit2D wallHit = Physics2D.Raycast(wallOrigin, Vector2.right * direction, wallDistance, groundLayer);

        if (groundHit.collider == null || wallHit.collider != null)
        {
            Flip();
        }
        AttackHitboxLogic(Time.fixedDeltaTime);
        checkPlayerDistance();
        managerLookAround();
        Move();
    }

    private void AttackHitboxLogic(float deltaTime)
    {
        Collider2D ratCollider = this.GetComponent<Collider2D>();
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        Collider2D[] hitPlayers = Physics2D.OverlapBoxAll(ratCollider.bounds.center, ratCollider.bounds.size, 0f, LayerMask.GetMask(playerLayerName));

        if (hitPlayers.Length > 0 && attackCooldown == 0f && health > 0)
        {
            hitPlayers[0].gameObject.GetComponent<PlayerCombat>().takeDamage(attackDamage);
            SoundManager.PlaySound(SoundType.RAT_ATTACK, 0.3f);
            attackCooldown = attackCooldownConstant;
        }

        if(attackCooldown > 0f)
        {
            attackCooldown -= deltaTime;
        }

        if(attackCooldown < 0f)
        {
            attackCooldown = 0f;
        }
    }

    void Flip()
    {
        direction *= -1;

        Vector3 newScale = transform.localScale;
        newScale.x = Mathf.Abs(newScale.x) * direction;
        transform.localScale = newScale;
    }

    private void flipVisuals()
    {
        Debug.Log("Flipping visuals");
        Vector3 newScale = transform.localScale;
        newScale.x = newScale.x * -1;
        transform.localScale = newScale;
    }

    private void Move()
    {
        if (!isMoving || isDead || isKnockedBack)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
    }

    // Vykreslení senzorů v Editoru (v okně Scene)
    private void OnDrawGizmos()
    {
        // Země (Červená)
        Gizmos.color = Color.red;
        Vector2 groundOrigin = (Vector2)transform.position + new Vector2(groundOffset.x * direction, groundOffset.y);
        Gizmos.DrawLine(groundOrigin, groundOrigin + Vector2.down * groundDistance);

        // Stěna (Zelená)
        Gizmos.color = Color.green;
        Vector2 wallOrigin = (Vector2)transform.position + new Vector2(wallOffset.x * direction, wallOffset.y);
        Gizmos.DrawLine(wallOrigin, wallOrigin + (Vector2.right * direction * wallDistance));
    }

    private void managerLookAround()
    {
        lookAroundTimer -= Time.fixedDeltaTime;
        if(lookAroundTimer <= 0f)
        {
            lookAround();
            adjustLookAroundTimer();
        }
    }

    private void checkPlayerDistance()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        if (distanceToPlayer < playerTriggerDistance)
        {
            speed = 2 * speedConstant;
        }
        else
        {
            speed = speedConstant;
        }
    }

    private void adjustSpeed()
    {
        float speedAdjustment = Random.Range(-maxSpeedAdjustment, maxSpeedAdjustment);
        speed += speedAdjustment;
        speedConstant = speed;
    }

    private void adjustLookAroundTimer()
    {
        lookAroundTimer = Random.Range(lookMinTime, lookMaxTime);
    }

    private void lookAround()
    {
        animator.SetTrigger("lookAround");
    }

    public void startMoving()
    {
        isMoving = true;
    }

    public void stopMoving()
    {
        isMoving = false;
    }

    public void manageEnemyHit(int playerDamage, Vector2 sourcePosition, float knockbackForce)
    {
        // Prevent the enemy from taking more hits or triggering death twice
        if (isDead) return;

        health -= playerDamage;
        ApplyKnockback(sourcePosition, knockbackForce);

        if (health <= 0)
        {
            isDead = true;

            // 1. Play death animations
            animator.SetTrigger("tookHit");
            animator.SetBool("isDead", true);

            // 2. Disable colliders immediately
            disableColiders();

            // Stop the Rigidbody so the enemy doesn't fall through the floor 
            // now that its colliders are turned off
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }
        else
        {
            stopMoving();
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

    private void healthRegen()
    {
        if (health + healthRegenAmount > maxHealth)
        {
            health = maxHealth;
        }
        else
        {
            health += healthRegenAmount;
        }
    }

    public void disableColiders()
    {
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }
    }

    public void ApplyKnockback(Vector2 sourcePosition, float force)
    {
        if (isDead || rb == null)
        {
            return;
        }

        float horizontalDirection = transform.position.x >= sourcePosition.x ? 1f : -1f;
        Vector2 direction = new Vector2(horizontalDirection, 0f);

        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }

        knockbackCoroutine = StartCoroutine(KnockbackRoutine(direction, force));
    }

    private IEnumerator KnockbackRoutine(Vector2 direction, float force)
    {
        isKnockedBack = true;
        isMoving = false;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction * (force / Mathf.Max(knockbackResistance, 0.01f)), ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        isKnockedBack = false;
        isMoving = true;
        knockbackCoroutine = null;
    }
}