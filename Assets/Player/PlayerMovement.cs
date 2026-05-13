using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController2D cController;
    public AttributesController aController;
    public PlayerCombat pCombat;
    public Rigidbody2D rb;
    public Animator animator;
    public Transform fallCheck;

    public float acceleration = 10f;
    private float runSpeedModifier = 4f;

    private InputSystem_Actions inputAction;

    float horizontalMove = 0f;
    float smoothedInputX = 0f;
    bool jump = false;

    private float jumpCooldown = 0f;

    [Header("Dash Settings")]
    [SerializeField] private float dashVelocity = 20f;
    [SerializeField] private float dashTime = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Double Jump")]
    [SerializeField] private int maxJumps = 1;
    private int jumpsRemaining;

    [Header("Coyote Time")]
    [SerializeField] private float coyoteTimeDuration = 0.15f;
    private float coyoteTimeCounter;
    private bool wasGrounded;

    public bool canDash = true;
    private bool isDashing;

    // Variables for fall reset
    private float startPosX;
    private float startPosY;
    private bool isReturning = false;

    private bool IsGameplayBlocked()
    {
        return RunController.Instance != null && RunController.Instance.IsGameplayInputBlocked;
    }

    void Start()
    {
        Application.targetFrameRate = 120;
        startPosX = transform.position.x;
        startPosY = transform.position.y;
        jumpsRemaining = maxJumps - 1;

    }

    void Awake()
    {
        inputAction = new InputSystem_Actions();
    }

    void OnEnable()
    {
        inputAction.Player.Enable();
        inputAction.Player.Jump.performed += OnJump;
        inputAction.Player.Jump.canceled += ctx => cController.CutJump();
        //dash input
        inputAction.Player.Dash.performed += ctx =>
        {
            if (canDash && aController.ConsumeStamina(aController.dashStaminaCost))
            {
                StartCoroutine(Dash());
            }
        };
        inputAction.Player.Interact.performed += ctx => animator.SetTrigger("BuffRitual");
        inputAction.Player.Attack.performed += ctx => GetComponent<PlayerCombat>().Attack();
    }

    void OnDisable()
    {
        inputAction.Player.Disable();

        inputAction.Player.Jump.performed -= OnJump;
        inputAction.Player.Jump.canceled -= ctx => cController.CutJump();
    }

    // Pavel - Přidal jsem si sem jen dva gettery pro cast spellu u bosse
    public bool getJump()
    {
        return jump;
    }

    public bool IsInAir()
    {
        return animator.GetBool("IsJumping");
    }

    void Update()
    {
        if (aController.currentHealth <= 0)
        {
            return;
        }

        if (IsGameplayBlocked())
        {
            horizontalMove = 0f;
            smoothedInputX = 0f;
            jump = false;
            animator.SetFloat("Speed", 0f);
            return;
        }

        bool isGroundedNow = cController.IsGrounded(); 

        if (isGroundedNow)
        {
            coyoteTimeCounter = coyoteTimeDuration;
            wasGrounded = true;
            jumpsRemaining = maxJumps;
        }
        else if (wasGrounded)
        {
            coyoteTimeCounter -= Time.deltaTime;
            if (jumpCooldown <= 0)
                jumpsRemaining = maxJumps - 1;
            if (coyoteTimeCounter <= 0f)
                wasGrounded = false;
        }

        Vector2 moveInput = inputAction.Player.Move.ReadValue<Vector2>();

        bool isKeyboard = inputAction.Player.Move.activeControl != null && inputAction.Player.Move.activeControl.device is Keyboard;

        if (isKeyboard)
        {
            smoothedInputX = Mathf.MoveTowards(smoothedInputX, moveInput.x, acceleration * Time.deltaTime);
            horizontalMove = smoothedInputX * aController.movementSpeed * runSpeedModifier;
        }
        else
        {
            smoothedInputX = moveInput.x;
            horizontalMove = moveInput.x * aController.movementSpeed * runSpeedModifier;
        }

        animator.SetFloat("Speed", Mathf.Abs(horizontalMove));
        animator.SetFloat("VerticalSpeed", rb.linearVelocityY);

        // Count down the timer every frame
        if (jumpCooldown > 0)
        {
            jumpCooldown -= Time.deltaTime;
        }

        // Check if the player has fallen below the death threshold
        if (rb.position.y < -10f)
        {
            if (!isReturning)
            {
                isReturning = true;
                float zDist = transform.position.z;

                int damageAmount = aController.maxHealth / 2; // Adjust this value as needed
                if (aController.currentHealth > 1) { 
                    if(aController.currentHealth - damageAmount <= 0)
                    {
                        aController.currentHealth = 1;
                    }
                    else
                    {
                        aController.currentHealth -= damageAmount;
                    }
                    transform.position = new Vector3(startPosX, startPosY, zDist);
                    animator.SetTrigger("Landed");
                    SoundManager.PlaySound(SoundType.PLAYER_HIT);
                }
                else
                {
                    pCombat.takeDamage(9999);
                }
            }

        }
        else
        {
            isReturning = false;
        }
    }

    private void FixedUpdate()
    {
        if (IsGameplayBlocked() || isDashing)
        {
            cController.Move(0f, false, false);
            jump = false;
            return;
        }

        if(Physics2D.OverlapBoxAll(new Vector2(this.transform.position.x, this.transform.position.y - 0.6f), new Vector2(0.4f, 0.3f), 0f, LayerMask.GetMask("Ground")).Length == 0)
        {
            animator.SetTrigger("Falling");
        }

        cController.Move(horizontalMove * Time.fixedDeltaTime, false, jump); // movement, crouch, jump
        jump = false;
    }

    private void resetFallingTrigger()
    {
        animator.ResetTrigger("Falling");
        animator.ResetTrigger("Landed");
        animator.SetInteger("ComboStep", 0);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed|| IsGameplayBlocked() || aController.currentHealth <= 0)
            return;

        bool canCoyoteJump = wasGrounded && coyoteTimeCounter > 0f && jumpsRemaining == 0;

        if (jumpsRemaining > 0 || canCoyoteJump)
        {
            if (canCoyoteJump)
            {
                coyoteTimeCounter = 0f;
                wasGrounded = false;
            }
            jump = true;
            jumpsRemaining = Mathf.Max(0, jumpsRemaining - 1);
            animator.SetBool("IsJumping", true);
            jumpCooldown = 0.05f;
        }

        // Tell the script to ignore the ground for the next 0.2 seconds
        jumpCooldown = 0.05f;
    }

    public void OnLanding()
    {
        if (IsGameplayBlocked())
            return;

        // If we just jumped, ignore this fake landing entirely!
        if (jumpCooldown > 0f)
        {
            return;
        }

        jumpsRemaining = maxJumps;

        //Debug.Log("Landed");
        animator.SetBool("IsJumping", false);
        animator.SetTrigger("Landed");
        animator.ResetTrigger("Falling");
    }

    

    [Header("Teleport Boundaries")]
    [SerializeField] private bool enableTeleport = false;
    [SerializeField] private float leftX = -15f;
    [SerializeField] private float rightX = 15f;
    [SerializeField] private float leftY = 10f;
    [SerializeField] private float rightY = -10f;
    

    private void LateUpdate()
    {
        if (IsGameplayBlocked())
            return;

        if (enableTeleport)
            HandleTeleport();
    }

    private IEnumerator Dash()
    {
        if (IsGameplayBlocked() || aController.currentHealth <= 0) yield break;

        canDash = false;
        isDashing = true;
        animator.SetTrigger("Dash");

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        // Use a higher dashVelocity (try 30-40) because your normal
        // run speed is already speed * modifier
        float dashDirection = transform.localScale.x;
        float startTime = Time.time;

        while (Time.time < startTime + dashTime)
        {
            // Constantly re-apply velocity every frame to prevent friction decay
            // and ensure we move the full distance.
            rb.linearVelocity = new Vector2(dashDirection * dashVelocity, 0f);
            yield return null;
        }

        // FIX FOR INCLINES: Reset velocity to zero (or your move speed) 
        // at the end to stop the upward 'launch' force.
        rb.linearVelocity = new Vector2(0f, 0f);

        rb.gravityScale = originalGravity;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void HandleTeleport()
    {
        Vector3 currentPos = transform.position;


        // Horizontal Wrap (X)
        if (currentPos.x > rightX)
        {
            currentPos.x = leftX;
            currentPos.y = leftY;
        }
        else if (currentPos.x < leftX)
        {
            currentPos.x = rightX;
            currentPos.y = rightY;
        }

        transform.position = currentPos;
    }


}