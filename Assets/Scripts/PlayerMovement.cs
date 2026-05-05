using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private float currentInputX;
    private float playerHalfWidth;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Jump Settings")]
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private int maxJumps = 2;
    private int jumpCount = 0;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer = ~0;
    private bool isGrounded;

    [Header("Dash Settings")]
    [SerializeField] private bool enableDash = true;
    [SerializeField] private float dashingPower = 24f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashingCooldown = 0.5f;
    [SerializeField] private TrailRenderer trailRenderer;

    private bool canDash = true;
    private bool isDashing;
    private float dashDirection = 1f;
    private float originalGravity;
    private bool jumpRequested = false;
    private bool dashKeyHeld = false;

    private string currentState = "Idle";

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (rigidBody == null) rigidBody = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        originalGravity = rigidBody.gravityScale;

        if (groundCheck == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.parent = transform;
            gc.transform.localPosition = new Vector3(0f, -0.3f, 0f);
            groundCheck = gc.transform;
        }

        playerHalfWidth = spriteRenderer.bounds.extents.x;
        ChangeAnimationState("Idle");
        jumpCount = 0;
        isGrounded = true;
    }

    void Update()
    {
        if (animator == null || rigidBody == null) return;

        CheckGroundedSimple();
        HandleDashInput();

        if (!isDashing)
        {
            HandleJumpInput();
            HandleMovement();
            FlipCharacterX();
        }

        ClampMovement();
        DecideWhichAnimationToPlay();
    }

    private void CheckGroundedSimple()
    {
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
        bool wasGrounded = isGrounded;
        isGrounded = hit.collider != null;

        if (isGrounded && !wasGrounded)
        {
            jumpCount = 0;
            canDash = true;
            Debug.Log("Landed on ground");
        }
    }

    private void DecideWhichAnimationToPlay()
    {
        if (isDashing)
        {
            ChangeAnimationState("Dash");
            return;
        }

        if (!isGrounded)
        {
            ChangeAnimationState("Jump");
            return;
        }

        if (isGrounded && Mathf.Abs(currentInputX) > 0.05f)
        {
            ChangeAnimationState("Run");
            return;
        }

        ChangeAnimationState("Idle");
    }

    private void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return;
        animator.Play(newState);
        currentState = newState;
    }

    void FixedUpdate()
    {
        if (jumpRequested)
        {
            PerformJump();
            jumpRequested = false;
        }

        // Apply dash velocity only while dashing
        if (isDashing)
        {
            rigidBody.linearVelocity = new Vector2(dashDirection * dashingPower, 0f);
        }
    }

    private void HandleJumpInput()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        bool jumpPressed = false;
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            jumpPressed = true;
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
            jumpPressed = true;

        if (jumpPressed)
        {
            RequestJump();
        }
#else
        if (Input.GetButtonDown("Jump"))
        {
            RequestJump();
        }
#endif
    }

    private void HandleDashInput()
    {
        if (!enableDash) return;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        bool dashPressed = false;
        bool dashReleased = false;

        if (Keyboard.current != null)
        {
            dashPressed = Keyboard.current.leftShiftKey.wasPressedThisFrame;
            dashReleased = Keyboard.current.leftShiftKey.wasReleasedThisFrame;
        }
        if (Gamepad.current != null)
        {
            dashPressed = Gamepad.current.buttonWest.wasPressedThisFrame;
            dashReleased = Gamepad.current.buttonWest.wasReleasedThisFrame;
        }

        // Start dash when pressing key on ground
        if (dashPressed && canDash && !isDashing && isGrounded)
        {
            StartDash();
        }

        // Stop dash when releasing key
        if (dashReleased && isDashing)
        {
            StopDash();
        }
#else
        bool dashPressed = Input.GetKeyDown(KeyCode.LeftShift);
        bool dashReleased = Input.GetKeyUp(KeyCode.LeftShift);
        
        // Start dash when pressing key on ground
        if (dashPressed && canDash && !isDashing && isGrounded)
        {
            StartDash();
        }
        
        // Stop dash when releasing key
        if (dashReleased && isDashing)
        {
            StopDash();
        }
#endif
    }

    private void StartDash()
    {
        isDashing = true;
        rigidBody.gravityScale = 0f;

        if (Mathf.Abs(currentInputX) > 0.1f)
        {
            dashDirection = Mathf.Sign(currentInputX);
        }
        else
        {
            dashDirection = spriteRenderer != null && spriteRenderer.flipX ? -1f : 1f;
        }

        rigidBody.linearVelocity = new Vector2(dashDirection * dashingPower, 0f);

        if (trailRenderer != null)
        {
            trailRenderer.emitting = true;
        }

        ChangeAnimationState("Dash");
        Debug.Log("Dash started");
    }

    private void StopDash()
    {
        isDashing = false;

        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }

        rigidBody.gravityScale = originalGravity;
        rigidBody.linearVelocity = new Vector2(0f, rigidBody.linearVelocity.y);

        StartCoroutine(DashCooldown());
        Debug.Log("Dash stopped");
    }

    private IEnumerator DashCooldown()
    {
        canDash = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
        Debug.Log("Dash ready again");
    }

    private void RequestJump()
    {
        if (jumpCount < maxJumps)
        {
            jumpRequested = true;
        }
    }

    private void PerformJump()
    {
        if (rigidBody == null) return;

        jumpCount++;

        Vector2 velocity = rigidBody.linearVelocity;
        velocity.y = 0f;
        rigidBody.linearVelocity = velocity;

        rigidBody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        ChangeAnimationState("Jump");
    }

    private void HandleMovement()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        Vector2 inputVector = Vector2.zero;

        if (Keyboard.current != null)
        {
            float left = (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) ? -1f : 0f;
            float right = (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) ? 1f : 0f;
            inputVector.x = left + right;
        }

        if (Gamepad.current != null)
        {
            inputVector = Gamepad.current.leftStick.ReadValue();
        }

        currentInputX = inputVector.x;
#else
        currentInputX = Input.GetAxis("Horizontal");
#endif

        if (!isDashing)
        {
            transform.Translate(new Vector3(currentInputX * speed * Time.deltaTime, 0f, 0f));
        }
    }

    private void ClampMovement()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null || spriteRenderer == null) return;

        Vector2 leftEdge = mainCamera.ScreenToWorldPoint(new Vector2(0, 0));
        Vector2 rightEdge = mainCamera.ScreenToWorldPoint(new Vector2(Screen.width, 0));

        playerHalfWidth = spriteRenderer.bounds.extents.x;

        float minX = leftEdge.x + playerHalfWidth;
        float maxX = rightEdge.x - playerHalfWidth;

        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        transform.position = new Vector2(clampedX, transform.position.y);
    }

    private void FlipCharacterX()
    {
        if (spriteRenderer == null) return;
        if (isDashing) return;

        if (currentInputX > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (currentInputX < 0)
        {
            spriteRenderer.flipX = true;
        }
    }

    public bool IsDashing()
    {
        return isDashing;
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public void ResetPlayerState()
    {
        isDashing = false;
        canDash = true;
        jumpCount = 0;

        if (rigidBody != null)
        {
            rigidBody.linearVelocity = Vector2.zero;
            rigidBody.gravityScale = originalGravity;
        }

        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }

        ChangeAnimationState("Idle");
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckDistance);
    }
}