using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static UnityEngine.Timeline.DirectorControlPlayable;

[RequireComponent(typeof(SpriteRenderer))]
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

    [Header("Sprite")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite spriteFacingRight;
    [SerializeField] private Sprite spriteFacingLeft;

    // [Header("Animator")]
    // [SerializeField] private Animator player_Animator;
    // public string currentState;
    // private const string IDLE_RIGHT = "Player_IdleRight", IDLE_LEFT = "Player_IdleLeft";
    // private const string RUN_RIGHT = "Player_RunRight", RUN_LEFT = "Player_RunLeft";

    [Header("Basic properties")]
    float currentSpeed;
    [SerializeField] float normalSpeed = 5f;
    [SerializeField] float runSpeed = 8f;
    Vector2 dir;

    [Header("Jump properties")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded;

    private int facing = 1;
    private const float X_DEADZONE = 0.01f;

    private void Awake()
    {
        instance = this;
        if (!playerInput) playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();

        interaction = GetComponentInChildren<Player_Interaction>();

        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();

        #region InputSystem_ActionsBinding
        move = playerInput.actions["Move"];
        run = playerInput.actions["Run"];
        jump = playerInput.actions["Jump"];
        interact = playerInput.actions["Interact"];
        pause = playerInput.actions["Pause"];
        #endregion

        currentSpeed = normalSpeed;
        UpdateFacingSprite();
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

        pause.performed += PauseAction;
        pause.canceled += PauseAction;
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

        pause.performed -= PauseAction;
        pause.canceled -= PauseAction;
        #endregion
    }

    #region Basic Actions
    private void MovementAction(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            float x = context.ReadValue<Vector2>().x;
            dir = new Vector2(x, 0f);

            if (Mathf.Abs(x) < X_DEADZONE)
            {
                isMoving = false;
                // ChangeAnimationState(facing > 0 ? IDLE_RIGHT : IDLE_LEFT);
            }
            else
            {
                if (x > 0f)
                {
                    // ChangeAnimationState(RUN_RIGHT);
                    facing = 1;
                }
                else
                {
                    // ChangeAnimationState(RUN_LEFT);
                    facing = -1;
                }
                isMoving = true;
            }

            UpdateFacingSprite();
            currentSpeed = isRunning ? runSpeed : normalSpeed;
        }
        else if (context.canceled)
        {
            // ChangeAnimationState(facing > 0 ? IDLE_RIGHT : IDLE_LEFT);
            dir = Vector2.zero;
            isMoving = false;
            UpdateFacingSprite();
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

    private void PauseAction(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            var PM = Pause_Menu.instance;
            PM.gameObject.SetActive(true);
            Time.timeScale = 0f;
        }
    }
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
        // if (currentState == newState) return;
        // player_Animator.CrossFade(newState, 0.1f);
        // currentState = newState;
    }
    #endregion

    void UpdateFacingSprite()
    {
        if (!spriteRenderer) return;

        if (facing >= 0)
        {
            if (spriteFacingRight) spriteRenderer.sprite = spriteFacingRight;
            else spriteRenderer.flipX = false;
        }
        else
        {
            if (spriteFacingLeft) spriteRenderer.sprite = spriteFacingLeft;
            else spriteRenderer.flipX = true;
        }
    }

    #region Events
    public void TriggerItemEvent(int index)
    {
        switch (index)
        {
            case 1:
                OnUseItem1();
                break;
            case 2:
                OnUseItem2();
                break;
            default:
                Debug.LogWarning("Invalid item index: " + index);
                break;
        }
    }

    private void OnUseItem1()
    {
        Debug.Log("Item 1 used: Speed Boost");
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
        switch (index)
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
