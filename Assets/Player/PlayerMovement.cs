using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController2D controller;
    public Rigidbody2D rb;
    public Animator animator;

    public float runSpeed = 40f;
    public float acceleration = 10f;

    private InputSystem_Actions inputAction;

    float horizontalMove = 0f;
    float smoothedInputX = 0f;
    bool jump = false;

    private float jumpCooldown = 0f;

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
        inputAction.Player.Attack.performed += ctx => GetComponent<PlayerCombat>().Attack();
    }

    void OnDisable()
    {
        inputAction.Player.Disable();
        inputAction.Player.Jump.performed -= OnJump;
        
    }

    void Update()
    {
        Vector2 moveInput = inputAction.Player.Move.ReadValue<Vector2>();

        bool isKeyboard = inputAction.Player.Move.activeControl != null && inputAction.Player.Move.activeControl.device is Keyboard;

        if (isKeyboard)
        {
            smoothedInputX = Mathf.MoveTowards(smoothedInputX, moveInput.x, acceleration * Time.deltaTime);
            horizontalMove = smoothedInputX * runSpeed;
        }
        else
        {
            smoothedInputX = moveInput.x;
            horizontalMove = moveInput.x * runSpeed;
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
        controller.Move(horizontalMove * Time.fixedDeltaTime, false, jump); // movement, crouch, jump
        jump = false;
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        jump = true;
        animator.SetBool("IsJumping", true);

        // Tell the script to ignore the ground for the next 0.2 seconds
        jumpCooldown = 0.05f;
    }

    public void OnLanding()
    {
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