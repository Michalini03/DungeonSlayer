using UnityEngine;
using System.Collections.Generic;
using Pathfinding;

public class FlyingEyeBehavior : MonoBehaviour
{
    [Header("Appearance")]
    [SerializeField] private int size = 3;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Movement Settings")]
    [SerializeField] private TileMapSpawnPointFinder spawnPointFinder;
    [SerializeField] private float moveSpeed = 400f;
    [SerializeField] private float maxSpeedAdjustment = 150f;
    [SerializeField] private float insideTrigerRange = 5f;
    [SerializeField] private float outsideTrigerRange = 8f;
    private Vector3 targetPosition;
    private bool isPatrolling = true;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 1.5f;
    private float attackCooldown = 1f;
    private float currentCooldown = 0f;

    public Transform Player;
    public float nextWaypointDistance = 1f;

    private Path path;
    int currentWaypoint = 0;
    bool reachedEndOfPath = false;

    Seeker seeker;
    Rigidbody2D rb;

    void Start()
    {
        speedAdjustment();
        setPosition();
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        InvokeRepeating("UpdatePath", 0f, 0.5f);
        setNewPatrolTarget();
    }

    void UpdatePath() {
        
        Vector3 targetPos = isPatrolling ? targetPosition : Player.position;
        if (seeker.IsDone()) {
            seeker.StartPath(transform.position, targetPos, OnPathComplete);
        }
    }

    void OnPathComplete(Path p) {
        if (!p.error) {
            path = p;
            currentWaypoint = 0;
        }
    }
    
    void flipCharecter()
    {
        if(rb.linearVelocity.x > 0.01f)
        {
            transform.localScale = new Vector3(size, size, size);
        }
        else if(rb.linearVelocity.x < -0.01f)
        {
            transform.localScale = new Vector3(-size, size, size);
        }
    }

    void FixedUpdate()
    {
        managePatrolTarget();
        manageAttackCooldown();
        flipCharecter();
        manageTrigerRange();
    }

    void Update()
    {
        if (path == null)
            return;

        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        } 
        
        else {
            reachedEndOfPath = false;
        }

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = direction * moveSpeed * Time.deltaTime;
        rb.AddForce(force);
        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        if (distance < nextWaypointDistance) {
            currentWaypoint++;
        }
    }

    void setPosition()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, -7);
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
            Debug.Log("Attacking player!");
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
        moveSpeed = 400f + Random.Range(-maxSpeedAdjustment, maxSpeedAdjustment);
    }

    private void managePatrolTarget()
    {
        Vector2 enemyPos2D = new Vector2(transform.position.x, transform.position.y);
        Vector2 targetPos2D = new Vector2(targetPosition.x, targetPosition.y);

        float dist = Vector2.Distance(enemyPos2D, targetPos2D);
        Debug.Log("Enemy Position: " + enemyPos2D);
        Debug.Log("Target Position: " + targetPos2D);
        Debug.Log("2D Distance to patrol target: " + dist);

        if (isPatrolling && (dist < nextWaypointDistance || reachedEndOfPath))
        {
            setNewPatrolTarget();
        }
    }

    private void setNewPatrolTarget()
    {
        if(isPatrolling)
        {
            targetPosition = spawnPointFinder.GetRandomSpawnPoint(EnemyType.FlyingEye);
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, insideTrigerRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, outsideTrigerRange);
    }
}
