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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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


    }

    void OnDisable()
    {
        inputAction.Player.Disable();

        inputAction.Player.Jump.performed -= OnJump;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 moveInput = inputAction.Player.Move.ReadValue<Vector2>();

        bool isKeyboard = inputAction.Player.Move.activeControl!=null && inputAction.Player.Move.activeControl.device is Keyboard;

        if(isKeyboard )
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
        //jump = inputAction.Player.Jump.triggered;



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
    }

    public void OnLanding()
    {
        if (rb.linearVelocityY > 0.1f)
        {
            Debug.Log("Fake Landing");
            return;
        }
        Debug.Log("Landed");
        animator.SetBool("IsJumping", false);
    }


}
