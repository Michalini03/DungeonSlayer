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
    private int direction = 1;
    private bool isMoving = true;

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

    void Start()
    {
        adjustSpeed();
        adjustLookAroundTimer();
    }

    void FixedUpdate()
    {
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
        checkPlayerDistance();
        managerLookAround();
        move();
    }

    void Flip()
    {
        direction *= -1;

        // Otočení grafiky (funguje správně i pro Scale 3)
        Vector3 newScale = transform.localScale;
        newScale.x = Mathf.Abs(newScale.x) * direction;
        transform.localScale = newScale;

        // Malý postrč, aby senzor hned znovu nenarazil do té samé věci
        transform.Translate(Vector2.right * direction * 0.1f);
    }

    private void flipVisuals()
    {
        Debug.Log("Flipping visuals");
        Vector3 newScale = transform.localScale;
        newScale.x = newScale.x * -1;
        transform.localScale = newScale;
    }

    private void move()
    {
        if(!isMoving)
        {
            return;
        }

        transform.Translate(Vector2.right * direction * speed * Time.fixedDeltaTime);
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
}