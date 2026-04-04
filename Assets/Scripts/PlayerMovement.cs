using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private Vector2 movement;
    private float leftBound;
    private float rightBound;
    private float playerHalfWidth;
    private float xPositionLastFrame;
    private float currentInputX;

    // For player to jump
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private float jumpForce = 8f; // Increased for better height
    [SerializeField] private float doubleJumpForce = 7f; // Slightly less than ground jump

    // Double-jump config
    [SerializeField] private int maxJumps = 2;
    private int jumpsRemaining;

    // Ground check
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.08f;
    [SerializeField] private LayerMask groundLayer = ~0;
    private bool isGrounded;

    // For jump input buffering
    private bool jumpRequested = false;

    void Start()
    {
        if (rigidBody == null)
        {
            rigidBody = GetComponent<Rigidbody2D>();
            if (rigidBody == null)
            {
                Debug.LogError("No Rigidbody2D found!");
                return;
            }
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError("No SpriteRenderer found!");
                return;
            }
        }

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
        HandleJumpInput();
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

    private void RequestJump()
    {
        // Check if we can jump
        if (isGrounded || jumpsRemaining > 0)
        {
            jumpRequested = true;
        }
        else
        {
            Debug.Log($"Can't jump! Grounded: {isGrounded}, Jumps left: {jumpsRemaining}");
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

        transform.Translate(new Vector3(movement.x, 0f, 0f));
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

        if (currentInputX > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (currentInputX < 0)
        {
            spriteRenderer.flipX = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}