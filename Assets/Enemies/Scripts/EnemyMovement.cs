using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private GameObject A;
    [SerializeField] private GameObject B;
    [SerializeField] private GameObject attackHitbox;
    [SerializeField] private bool detachPatrolPointsOnStart = true;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject player;
    [SerializeField] private float normalSpeed = 2f; // Increased slightly for physics
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float insideTrigerRange = 5f;
    [SerializeField] private float outsideTrigerRange = 7f;
    [SerializeField] private float speedAnimationMultiplier = 2.0f;

    private Rigidbody2D rb;
    private float attackCooldown = 1f;
    private float currentCooldown = 0f;
    private bool canWalk;
    private bool isTrigered = false;
    private bool isLastVisitedA = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (detachPatrolPointsOnStart)
        {
            DetachPatrolPoint(A);
            DetachPatrolPoint(B);
        }

        canWalk = true;
        animator = GetComponent<Animator>();
        animator.SetBool("canWalk", true);
    }

    private void DetachPatrolPoint(GameObject patrolPoint)
    {
        if (patrolPoint == null) return;
        patrolPoint.transform.SetParent(null, true);
    }

    private void FixedUpdate()
    {
        manageAttackCooldown();
        Move();
        checkTrigger();
        CheckForJump(); // New jump logic
    }

    private void Move()
    {
        Vector3 direction;
        if (!canWalk)
        {
            // Stop horizontal movement when attacking
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            direction = moveTowardsPlayer();
            flipCharacter(direction);
            return;
        }

        float currentSpeed = isTrigered ? chaseSpeed : normalSpeed;

        if (!isTrigered)
        {
            direction = walkBetweenPoints();
        }
        else
        {
            direction = moveTowardsPlayer();
        }

        flipCharacter(direction);

        // Physics-based movement (avoids the "curling" and "teleporting" issues)
        float horizontalMove = direction.x > 0 ? 1 : -1;
        rb.linearVelocity = new Vector2(horizontalMove * currentSpeed, rb.linearVelocity.y);
    }

    [Header("Detection Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float wallCheckDistance = 1.0f;


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

        float checkDist = 1.1f; // Scale is 2, so feet are further down

        Debug.DrawRay(leftFoot, Vector2.down * checkDist, Color.green);
        Debug.DrawRay(rightFoot, Vector2.down * checkDist, Color.green);

        bool left = Physics2D.Raycast(leftFoot, Vector2.down, checkDist, groundLayer);
        bool right = Physics2D.Raycast(rightFoot, Vector2.down, checkDist, groundLayer);

        return left || right;
    }

    private void checkTrigger()
    {
        float dist = Vector3.Distance(transform.position, player.transform.position);
        if (dist < insideTrigerRange)
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

    private Vector3 walkBetweenPoints()
    {
        Vector3 targetPos = isLastVisitedA ? B.transform.position : A.transform.position;
        Vector3 direction = targetPos - transform.position;

        if (Vector3.Distance(transform.position, targetPos) < 0.5f)
        {
            isLastVisitedA = !isLastVisitedA;
        }
        return direction;
    }

    private Vector3 moveTowardsPlayer()
    {
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

    // Visualize the jump detection in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 dir = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        Gizmos.DrawRay(transform.position, dir * wallCheckDistance);
        Gizmos.DrawWireSphere(transform.position + new Vector3(0, -0.8f, 0), 0.2f);
    }
}