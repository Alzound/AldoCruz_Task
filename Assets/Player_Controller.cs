using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Timeline.DirectorControlPlayable;

public class Player_Controller : MonoBehaviour
{
    [Header("Player Instance")]
    public static Player_Controller instance;

    [Header("Input System")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private bool isMoving, isJumping, isRunning;
    private InputAction move, run, jump, pause;

    [Header("Rigidbody 2D")]
    private Rigidbody2D rb;

    [Header("Basic properties")]
    float currentSpeed;
    float normalSpeed = 5f;
    float runSpeed = 8f;

    [Header("Jump properties")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private Transform groundCheck;    
    [SerializeField] private float groundCheckRadius = 0.2f; 
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded;

    Vector2 dir;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();

        #region InputSystem_ActionsBinding
        move = playerInput.actions["Move"];
        run = playerInput.actions["Run"];
        jump = playerInput.actions["Jump"];
        pause = playerInput.actions["Pause"];
        #endregion
    }

    private void OnEnable()
    {
        #region InputSystem
        move.performed += MovementAction;
        move.canceled += MovementAction;

        run.performed += RunAction;
        run.canceled += RunAction;

        jump.performed += JumpAction;  
        #endregion
    }

    private void OnDisable()
    {
        #region InputSystem
        move.performed -= MovementAction;
        move.canceled -= MovementAction;

        run.performed -= RunAction;
        run.canceled -= RunAction;

        jump.performed -= JumpAction;
        #endregion
    }

    #region Basic Actions
    private void MovementAction(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            dir = new Vector2(context.ReadValue<Vector2>().x, 0f);
            currentSpeed = isRunning ? runSpeed : normalSpeed;
            isMoving = dir != Vector2.zero;
        }
        else if (context.canceled)
        {
            dir = Vector2.zero;
            isMoving = false;
        }
    }

    private void MovementUpdate()
    {
        float targetX = dir.x * currentSpeed;
        rb.linearVelocity = new Vector2(targetX, rb.linearVelocity.y);
    }

    private void RunAction(InputAction.CallbackContext context)
    {
        if (context.performed && isMoving)
        {
            isRunning = true;
            currentSpeed = runSpeed;
        }
        else if (context.canceled)
        {
            isRunning = false;
            currentSpeed = normalSpeed;
        }
    }

    private void JumpAction(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isJumping = true;
        }
    }

    private void PauseAction(InputAction.CallbackContext context) { }
    #endregion

    #region GRAVITY
    private void GroundCheck()
    {
        if (!groundCheck) return;

        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckRadius, groundLayer);
        isGrounded = hit.collider != null;

        if (isGrounded) isJumping = false; 
    }

    private void OnDrawGizmosSelected()
    {
        if (!groundCheck) return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckRadius);
    }
    #endregion

    private void FixedUpdate()
    {
        GroundCheck();      
        MovementUpdate();   
    }

    void Update() { }
}
