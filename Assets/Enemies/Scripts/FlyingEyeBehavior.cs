using UnityEngine;
using System.Collections.Generic;
using Pathfinding;

public class FlyingEyeBehavior : MonoBehaviour
{
    [Header("Appearance")]
    [SerializeField] private int size = 3;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 1.5f;
    private float attackCooldown = 1f;
    private float currentCooldown = 0f;

    public Transform Player;
    public float speed = 200f;
    public float nextWaypointDistance = 1f;

    private Path path;
    int currentWaypoint = 0;
    bool reachedEndOfPath = false;

    Seeker seeker;
    Rigidbody2D rb;


    void Start()
    {
        setPosition();
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        InvokeRepeating("UpdatePath", 0f, 0.5f);
    }

    void UpdatePath() {
        if (seeker.IsDone()) {
            seeker.StartPath(transform.position, Player.position, OnPathComplete);
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
        manageAttackCooldown();
        flipCharecter();
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
        Vector2 force = direction * speed * Time.deltaTime;
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
    
}
