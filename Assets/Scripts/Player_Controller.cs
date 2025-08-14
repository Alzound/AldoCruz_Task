using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static UnityEngine.Timeline.DirectorControlPlayable;

public class Player_Controller : MonoBehaviour
{
    [Header("Player Instance")]
    public static Player_Controller instance;
    [Header("Player Interaction")]
    [SerializeField] private Player_Interaction interaction;

    [Header("Input System")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private bool isMoving, isJumping, isRunning;
    private InputAction move, run, jump, pause, interact;

    [Header("Rigidbody 2D")]
    private Rigidbody2D rb;

    [Header("Animator")]
    [SerializeField] private Animator player_Animator;
    public string currentState;
    private const string IDLE_RIGHT = "Player_IdleRight", IDLE_LEFT = "Player_IdleLeft";
    private const string RUN_RIGHT = "Player_RunRight", RUN_LEFT = "Player_RunLeft";
    private const string JUMP = "Player_Jump";
    private const string ATTACK_RIGHT = "Player_AttackRight", ATTACK_LEFT = "Player_AttackLeft";

    [Header("Basic properties")]
    float currentSpeed;
    [SerializeField] float normalSpeed = 5f;
    [SerializeField] float runSpeed = 8f;

    [Header("Jump properties")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private Transform groundCheck;    
    [SerializeField] private float groundCheckRadius = 0.2f; 
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded;

    Vector2 dir;

    private void Awake()
    {
        instance = this;
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();

        interaction = GetComponentInChildren<Player_Interaction>();
        player_Animator = GetComponent<Animator>();

        #region InputSystem_ActionsBinding
        move = playerInput.actions["Move"];
        run = playerInput.actions["Run"];
        jump = playerInput.actions["Jump"];
        interact = playerInput.actions["Interact"];
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

        interact.performed += InteractAction;
        interact.canceled += InteractAction;
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

        interact.performed -= InteractAction;
        interact.canceled -= InteractAction;
        #endregion
    }

    #region Basic Actions
    private void MovementAction(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            dir = new Vector2(context.ReadValue<Vector2>().x, 0f);
            if(dir.x > 0)
            {
                ChangeAnimationState(RUN_RIGHT);
            }
            else
            {
                ChangeAnimationState(RUN_LEFT);
            }

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

    private void InteractAction(InputAction.CallbackContext context)
    {
        if (context.performed && interaction.isInteractable)
        {
            interaction.Interact();
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

    #region Animation
    private void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return;

        player_Animator.CrossFade(newState, 0.1f);
        currentState = newState;
    }
    #endregion

    #region Events
    public void TriggerItemEvent(int index)
    {
        switch (index)
        {
            case 0:
                OnUseItem1(); 
                break;
            case 1:
                OnUseItem2(); 
                break;
            default:
                Debug.LogWarning("Invalid item index: " + index);
                break;
        }
    }

    private void OnUseItem1()
    {
        normalSpeed = 12f;
        runSpeed = 15f;
        StartCoroutine(ReturnValuesToNormal(15f, 1));
    }

    private void OnUseItem2()
    {
        jumpForce = 15f;    
        StartCoroutine(ReturnValuesToNormal(10f, 2));
    }

    IEnumerator ReturnValuesToNormal(float duration, int index)
    {
        yield return new WaitForSeconds(duration);
        switch(index)
        {
            case 1:
                normalSpeed = 5f;
                runSpeed = 8f;
                break;
            case 2:
                jumpForce = 7f;
                break;
            default:
                Debug.LogWarning("Invalid index for resetting values: " + index);
                break;
        }
    }
    #endregion
}
