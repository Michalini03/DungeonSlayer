using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController2D cController;
    public AttributesController aController;
    public Rigidbody2D rb;
    public Animator animator;

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

    private bool canDash = true;
    private bool isDashing;

    private bool IsGameplayBlocked()
    {
        return RunController.Instance != null && RunController.Instance.IsGameplayInputBlocked;
    }

    void Start()
    {
        Application.targetFrameRate = 120;
        
    }

    void Awake()
    {
        inputAction = new InputSystem_Actions();
    }

    void OnEnable()
    {
        inputAction.Player.Enable();
        inputAction.Player.Jump.performed += OnJump;
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
    }

    void Update()
    {
        if (IsGameplayBlocked())
        {
            horizontalMove = 0f;
            smoothedInputX = 0f;
            jump = false;
            animator.SetFloat("Speed", 0f);
            return;
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

        // Count down the timer every frame
        if (jumpCooldown > 0)
        {
            jumpCooldown -= Time.deltaTime;
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

        cController.Move(horizontalMove * Time.fixedDeltaTime, false, jump); // movement, crouch, jump
        jump = false;
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (IsGameplayBlocked())
            return;

        jump = true;
        animator.SetBool("IsJumping", true);

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

        Debug.Log("Landed");
        animator.SetBool("IsJumping", false);
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