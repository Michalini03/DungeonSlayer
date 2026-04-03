using Unity.Cinemachine;
using Unity.Netcode;
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

    private bool IsNetworkOwner()
    {
        var netObj = GetComponent<NetworkObject>();
        // If no NetworkObject component, we're in single-player — always own
        if (netObj == null) return true;
        if (!netObj.IsSpawned) return true;
        return netObj.IsOwner;
    }

    private bool IsGameplayBlocked()
    {
        return RunController.Instance != null && RunController.Instance.IsGameplayInputBlocked;
    }

    void Start()
    {
        Application.targetFrameRate = 120;
        PlayerRegistry.Register(gameObject);
    }

    void Awake()
    {
        inputAction = new InputSystem_Actions();
    }

    void OnEnable()
    {
        inputAction.Player.Enable();
        inputAction.Player.Jump.performed += OnJump;
        inputAction.Player.Interact.performed += OnInteract;
        inputAction.Player.Attack.performed += OnAttack;
    }

    void OnDisable()
    {
        inputAction.Player.Disable();
        inputAction.Player.Jump.performed -= OnJump;
        inputAction.Player.Interact.performed -= OnInteract;
        inputAction.Player.Attack.performed -= OnAttack;
        PlayerRegistry.Unregister(gameObject);
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!IsNetworkOwner()) return;
        animator.SetTrigger("BuffRitual");
    }

    private void OnAttack(InputAction.CallbackContext ctx)
    {
        if (!IsNetworkOwner()) return;
        GetComponent<PlayerCombat>().Attack();
    }

    void Update()
    {
        if (!IsNetworkOwner()) return;

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
        if (IsGameplayBlocked())
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