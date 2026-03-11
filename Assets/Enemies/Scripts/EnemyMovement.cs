using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyMovement : MonoBehaviour
{   
    [Header("Movement Settings")]
    [SerializeField] private GameObject A;
    [SerializeField] private GameObject B;
    [SerializeField] private GameObject attackHitbox;
    [SerializeField] private bool detachPatrolPointsOnStart = true;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject player;
    [SerializeField] private float normalSpeed = 0.7f;
    [SerializeField] private float chaseSpeed = 2.5f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float insideTrigerRange = 5f;
    [SerializeField] private float outsideTrigerRange = 7f;
    [SerializeField] private float speedAnimationMultiplier = 2.0f;

    // Attack cooldown management
    private float attackCooldown = 1f;
    private float currentCooldown = 0f;
    private bool canWalk;
    private bool isTrigered = false;
    private bool isLastVisitedA = false;
    
    // Vzbudí se a nastaví animaci na chůzi
    private void Awake()
    {
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
        if (patrolPoint == null)
        {
            return;
        }

        // Keep world position/rotation while unparenting so patrol targets stay fixed in scene.
        patrolPoint.transform.SetParent(null, true);
    }

    // Každý fram zkontroluje cooldown útoku, pohyb a trigger pro přepínání mezi chůzí a honěním hráče
    private void FixedUpdate()
    {
        manageAttackCooldown();
        Move();
        checkTrigger();
    }

    // Metoda pro řízení pohybu nepřítele, který se buď pohybuje mezi dvěma body, nebo honí hráče
    private void Move()
    {
        // Pokud je nepřítel v dosahu útoku, nemůže chodit, ale pouze útočit
        if (!canWalk){
            Vector3 direction = moveTowardsPlayer();
            flipCharacter(direction);
            return;
        }

        // Pokud není v dosahu útoku, ale je v dosahu triggeru, honí hráče, jinak chodí mezi body A a B
        if(!isTrigered)
        {
            Vector3 direction = walkBetweenPoints();
            flipCharacter(direction);
            transform.Translate(direction.normalized * normalSpeed * Time.fixedDeltaTime);
        }

        // Pokud je v dosahu triggeru, ale není v dosahu útoku, honí hráče
        else if(isTrigered)
        {
            Vector3 direction = moveTowardsPlayer();
            flipCharacter(direction);
            transform.Translate(direction.normalized * chaseSpeed * Time.fixedDeltaTime);
        }
    }

    // Metoda pro kontrolu vzdálenosti mezi nepřítelem a hráčem, která určuje, zda se má přepnout mezi chůzí a honěním hráče
    private void checkTrigger()
    {
        if(Vector3.Distance(transform.position, player.transform.position) < insideTrigerRange)
        {
            // Pokud jsme v dosahu triggeru, ale ještě nejsme v dosahu útoku, zrychlíme animaci chůze a přepneme na honění hráče
            animator.SetFloat("walkSpeedMultiplier", speedAnimationMultiplier);
            isTrigered = true;
        }
        else if(Vector3.Distance(transform.position, player.transform.position) > outsideTrigerRange)
        {
            // Pokud jsme mimo dosah triggeru, ale ještě nejsme v dosahu útoku, zpomalíme animaci chůze a přepneme na chůzi mezi body A a B
            animator.SetFloat("walkSpeedMultiplier", 1.0f);
            isTrigered = false;
        }
    }

    // Metoda pro pohyb mezi dvěma body A a B, která vrací směr pohybu a aktualizuje, který bod byl naposledy navštíven
    private Vector3 walkBetweenPoints()
    {
        // Směr, kterým se nepřítel bude pohybovta... ten vracíme
        Vector3 direction;
        
        // Pokud jsme naposledy navštívili bod A, jdeme k bodu B, jinak jdeme k bodu A
        if(isLastVisitedA)
        {
            direction = B.transform.position - transform.position;
            if(Vector3.Distance(transform.position, B.transform.position) < 0.1f)
            {
                isLastVisitedA = false;
            }
        }

        // Pokud jsme naposledy navštívili bod B, jdeme k bodu A, jinak jdeme k bodu B
        else
        {
            direction = A.transform.position - transform.position;
            if(Vector3.Distance(transform.position, A.transform.position) < 0.1f)
            {
                isLastVisitedA = true;
            }
        }
        return direction;
    }

    // Metoda pro pohyb směrem k hráči, která vrací směr pohybu
    private Vector3 moveTowardsPlayer()
    {
        Vector3 direction = player.transform.position - transform.position;
        return direction;
    }
    
    // Metoda pro otočení postavy směrem k pohybu, která mění měřítko postavy podle směru pohybu
    private void flipCharacter(Vector3 direction)
    {
        if (direction.x < 0)
        {
            transform.localScale = new Vector3(-2, 2, 2);
        }
        else
        {
            transform.localScale = new Vector3(2, 2, 2);
        }
    }

    // Metoda pro řízení cooldownu útoku, která kontroluje, zda je nepřítel v dosahu útoku a zda může útočit, a aktualizuje stav animace a pohybu podle toho
    private void manageAttackCooldown()
    {
        //Jsme v dosahu útoku (poprve)
        if(Vector3.Distance(transform.position, player.transform.position) < attackRange && currentCooldown <= 0)
        {
            animator.SetBool("Attack", true);
            currentCooldown = attackCooldown;
            canWalk = false;
            return;
        }

        //Jsme mimo dosah útoku, ale cooldown ještě nevypršel
        else if (currentCooldown > 0)
        {
            currentCooldown -= Time.fixedDeltaTime;
            return;
        }

        //Jsme mimo dosah útoku a cooldown vypršel
        else
        {
            animator.SetBool("Attack", false);
            currentCooldown = 0f;
            canWalk = true;
        }
    }

    public void EnableHitbox() {
        attackHitbox.SetActive(true);
    }

    public void DisableHitbox() {
        attackHitbox.SetActive(false);
    }
}