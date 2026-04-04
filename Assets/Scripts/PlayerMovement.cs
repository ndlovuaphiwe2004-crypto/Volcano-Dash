using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
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
    [SerializeField] private float dashingTime = 0.2f;
    [SerializeField] private float dashingCooldown = 1f;
    [SerializeField] private TrailRenderer trailRenderer;

    private bool canDash = true;
    private bool isDashing;

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
        // Don't allow input while dashing (except movement)
        if (isDashing)
        {
            // Still allow screen clamping while dashing
            ClampMovement();
            return;
        }

        CheckGrounded();
        HandleJumpInput();
        HandleDashInput();
        HandleMovement();
        ClampMovement();
        FlipCharacterX();
    }

    void FixedUpdate()
    {
        // Handle physics-based jumping in FixedUpdate for consistency
        if (jumpRequested)
        {
            PerformJump();
            jumpRequested = false;
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

        // Reset jumps when landing
        if (isGrounded && !wasGrounded)
        {
            jumpsRemaining = maxJumps;
            canDash = true; // Reset dash on landing
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
        bool dashPressed = false;
        if (Keyboard.current != null && Keyboard.current.leftShiftKey.wasPressedThisFrame) dashPressed = true;
        if (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame) dashPressed = true; // X button on Xbox, Square on PS

        if (dashPressed && canDash && !isDashing)
        {
            StartCoroutine(Dash());
        }
#else
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && !isDashing)
        {
            StartCoroutine(Dash());
        }
#endif
    }

    private void RequestJump()
    {
        // Check if we can jump (don't allow jumping while dashing)
        if (!isDashing && (isGrounded || jumpsRemaining > 0))
        {
            jumpRequested = true;
        }
        else
        {
            Debug.Log($"Can't jump! Grounded: {isGrounded}, Jumps left: {jumpsRemaining}, Dashing: {isDashing}");
        }
    }

    private void PerformJump()
    {
        if (rigidBody == null) return;

        // Different jump force for ground vs air jumps
        float currentJumpForce = isGrounded ? jumpForce : doubleJumpForce;

        // Don't reset vertical velocity completely - just cap it if going down
        Vector2 v = rigidBody.linearVelocity;
        if (v.y < 0)
        {
            v.y = 0f; // Only reset if falling
        }
        rigidBody.linearVelocity = v;

        // Apply jump force
        rigidBody.AddForce(Vector2.up * currentJumpForce, ForceMode2D.Impulse);

        // Decrement jumps if in air
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

        // Small cooldown to prevent frame-perfect double inputs
        Invoke(nameof(ResetJumpRequest), 0.05f);
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        // Store original gravity scale
        float originalGravity = rigidBody.gravityScale;
        rigidBody.gravityScale = 0f;

        // Apply dash force in the direction the player is facing
        float dashDirection = spriteRenderer.flipX ? -1f : 1f;
        rigidBody.linearVelocity = new Vector2(dashDirection * dashingPower, 0f);

        // Enable trail renderer if assigned
        if (trailRenderer != null)
        {
            trailRenderer.emitting = true;
        }

        Debug.Log("Dash started!");

        // Wait for dash duration
        yield return new WaitForSeconds(dashingTime);

        // Disable trail renderer
        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }

        // Restore gravity
        rigidBody.gravityScale = originalGravity;
        isDashing = false;

        Debug.Log("Dash ended!");

        // Wait for cooldown before allowing another dash
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
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
        {
            inputVector = Gamepad.current.leftStick.ReadValue();
        }
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

        // Only apply movement if not dashing
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

        // Don't flip while dashing to maintain direction
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

    // Public method to check if player is dashing (for other scripts)
    public bool IsDashing()
    {
        return isDashing;
    }

    // Public method to check if player is grounded (for other scripts)
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