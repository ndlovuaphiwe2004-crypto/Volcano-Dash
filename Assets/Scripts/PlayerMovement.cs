using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private Vector2 movement;
    private float leftBound;
    private float rightBound;
    private float playerHalfWidth;
    private float xPositionLastFrame;
    private float currentInputX;

    [Header("Jump Settings")]
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float doubleJumpForce = 7f;
    [SerializeField] private int maxJumps = 2;
    private int jumpsRemaining;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.08f;
    [SerializeField] private LayerMask groundLayer = ~0;
    private bool isGrounded;

    [Header("Dash Settings")]
    [SerializeField] private bool enableDash = true;
    [SerializeField] private float dashingPower = 24f;
    [SerializeField] private float dashingCooldown = 0.5f; // Reduced cooldown for better feel
    [SerializeField] private TrailRenderer trailRenderer;

    private bool canDash = true;
    private bool isDashing;
    private float dashDirection = 1f;
    private float originalGravity;

    // For jump input buffering
    private bool jumpRequested = false;

    void Start()
    {
        // Rigidbody setup
        if (rigidBody == null)
        {
            rigidBody = GetComponent<Rigidbody2D>();
            if (rigidBody == null)
            {
                Debug.LogError("No Rigidbody2D found!");
                return;
            }
        }

        // Store original gravity
        originalGravity = rigidBody.gravityScale;

        // Configure Rigidbody2D for better dash performance
        rigidBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // SpriteRenderer setup
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError("No SpriteRenderer found!");
                return;
            }
        }

        // TrailRenderer setup (optional)
        if (trailRenderer == null)
        {
            trailRenderer = GetComponent<TrailRenderer>();
        }

        // Camera bounds setup
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No main camera found!");
            return;
        }

        Vector2 leftEdge = mainCamera.ScreenToWorldPoint(new Vector2(0, 0));
        Vector2 rightEdge = mainCamera.ScreenToWorldPoint(new Vector2(Screen.width, 0));

        playerHalfWidth = spriteRenderer.bounds.extents.x;
        leftBound = leftEdge.x + playerHalfWidth;
        rightBound = rightEdge.x - playerHalfWidth;

        xPositionLastFrame = transform.position.x;
        currentInputX = 0f;

        // Ground check setup
        if (groundCheck == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.parent = transform;
            float feetOffset = -(spriteRenderer.bounds.extents.y + 0.05f);
            gc.transform.localPosition = new Vector3(0f, feetOffset, 0f);
            groundCheck = gc.transform;
        }

        jumpsRemaining = maxJumps;
    }

    void Update()
    {
        CheckGrounded();

        // Handle dash input FIRST - this determines if we're dashing
        HandleDashInput();

        // Only handle other inputs if not dashing
        if (!isDashing)
        {
            HandleJumpInput();
            HandleMovement();
            FlipCharacterX();
        }

        ClampMovement();
    }

    void FixedUpdate()
    {
        // Handle physics-based jumping
        if (jumpRequested)
        {
            PerformJump();
            jumpRequested = false;
        }

        // Apply dash movement while dashing
        if (isDashing)
        {
            // Constantly apply dash velocity every frame while dashing
            rigidBody.linearVelocity = new Vector2(dashDirection * dashingPower, 0f);
        }
    }

    private void CheckGrounded()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        Collider2D hit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        bool wasGrounded = isGrounded;
        isGrounded = (hit != null);

        if (isGrounded && !wasGrounded)
        {
            jumpsRemaining = maxJumps;
            Debug.Log($"Landed! Jumps reset to: {jumpsRemaining}");
        }
    }

    private void HandleJumpInput()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        bool jumpPressed = false;
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) jumpPressed = true;
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame) jumpPressed = true;

        if (jumpPressed)
        {
            RequestJump();
        }
#else
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RequestJump();
        }
#endif
    }

    private void HandleDashInput()
    {
        if (!enableDash) return;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        // Check if dash key is currently being held
        bool dashHeld = false;
        if (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed) dashHeld = true;
        if (Gamepad.current != null && Gamepad.current.buttonWest.isPressed) dashHeld = true;

        // START DASH: Key is held AND we can dash AND not already dashing
        if (dashHeld && canDash && !isDashing)
        {
            StartDash();
        }

        // STOP DASH: Key is released AND we are currently dashing
        if (!dashHeld && isDashing)
        {
            StopDash();
        }
#else
        // Legacy Input System
        bool shiftHeld = Input.GetKey(KeyCode.LeftShift);
        
        // START DASH: Shift held AND can dash AND not dashing
        if (shiftHeld && canDash && !isDashing)
        {
            StartDash();
        }
        
        // STOP DASH: Shift released AND currently dashing
        if (!shiftHeld && isDashing)
        {
            StopDash();
        }
#endif
    }

    private void StartDash()
    {
        isDashing = true;

        // Set gravity to 0 during dash
        rigidBody.gravityScale = 0f;

        // Store dash direction based on player facing
        dashDirection = spriteRenderer.flipX ? -1f : 1f;

        // Apply dash force
        rigidBody.linearVelocity = new Vector2(dashDirection * dashingPower, 0f);

        // Enable trail renderer if assigned
        if (trailRenderer != null)
        {
            trailRenderer.emitting = true;
        }

        Debug.Log("DASH ACTIVE - Holding Shift");
    }

    private void StopDash()
    {
        isDashing = false;

        // Disable trail renderer
        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }

        // Restore gravity
        rigidBody.gravityScale = originalGravity;

        // Stop all momentum (optional - set to 0 for instant stop, or keep some velocity)
        // Comment out the next line if you want to keep some momentum after dash
        rigidBody.linearVelocity = new Vector2(0f, rigidBody.linearVelocity.y);

        Debug.Log("DASH STOPPED - Shift released");

        // Start cooldown
        StartCoroutine(DashCooldown());
    }

    private IEnumerator DashCooldown()
    {
        canDash = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
        Debug.Log("Dash ready again!");
    }

    private void RequestJump()
    {
        if (isGrounded || jumpsRemaining > 0)
        {
            jumpRequested = true;

            // If dashing, stop dash on jump
            if (isDashing)
            {
                StopDash();
            }
        }
    }

    private void PerformJump()
    {
        if (rigidBody == null) return;

        float currentJumpForce = isGrounded ? jumpForce : doubleJumpForce;

        Vector2 v = rigidBody.linearVelocity;
        if (v.y < 0)
        {
            v.y = 0f;
        }
        rigidBody.linearVelocity = v;

        rigidBody.AddForce(Vector2.up * currentJumpForce, ForceMode2D.Impulse);

        if (!isGrounded)
        {
            jumpsRemaining--;
            Debug.Log($"Double jump! Jumps remaining: {jumpsRemaining}");
        }
        else
        {
            jumpsRemaining = maxJumps - 1;
            Debug.Log($"Ground jump! Jumps remaining: {jumpsRemaining}");
        }

        Invoke(nameof(ResetJumpRequest), 0.05f);
    }

    private void ResetJumpRequest()
    {
        jumpRequested = false;
    }

    private void HandleMovement()
    {
        movement = Vector2.zero;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        Vector2 inputVector = Vector2.zero;

        if (Gamepad.current != null)
            inputVector = Gamepad.current.leftStick.ReadValue();
        else if (Keyboard.current != null)
        {
            float left = (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) ? -1f : 0f;
            float right = (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) ? 1f : 0f;
            inputVector.x = left + right;
        }

        movement.x = inputVector.x * speed * Time.deltaTime;
        currentInputX = inputVector.x;
#else
        float input = Input.GetAxis("Horizontal");
        movement.x = input * speed * Time.deltaTime;
        currentInputX = input;
#endif

        // Only apply normal movement if not dashing
        if (!isDashing)
        {
            transform.Translate(new Vector3(movement.x, 0f, 0f));
        }
    }

    private void ClampMovement()
    {
        float clampedX = Mathf.Clamp(transform.position.x, leftBound, rightBound);
        Vector3 pos = transform.position;
        pos.x = clampedX;
        transform.position = pos;
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

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}