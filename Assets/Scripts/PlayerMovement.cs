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
    [SerializeField] private float jumpForce = 6f;

    // Ground check
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.08f;
    [SerializeField] private LayerMask groundLayer = ~0; // default to everything
    private bool isGrounded;

    void Start()
    {
        // Auto-assign rigidBody if not set in Inspector
        if (rigidBody == null)
        {
            rigidBody = GetComponent<Rigidbody2D>();

            if (rigidBody == null)
            {
                Debug.LogError("No Rigidbody2D found on this GameObject! Please assign one in the Inspector or add a Rigidbody2D component.");
                return;
            }
        }

        // Auto-assign spriteRenderer if not set in Inspector
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer == null)
            {
                Debug.LogError("No SpriteRenderer found on this GameObject! Please assign one in the Inspector or add a SpriteRenderer component.");
                return;
            }
        }

        // Get the camera's boundaries in world space
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("No main camera found in the scene!");
            return;
        }

        // Get the world position of the left and right edges of the screen
        Vector2 leftEdge = mainCamera.ScreenToWorldPoint(new Vector2(0, 0));
        Vector2 rightEdge = mainCamera.ScreenToWorldPoint(new Vector2(Screen.width, 0));

        // Get the player's half-width from the sprite renderer
        playerHalfWidth = spriteRenderer.bounds.extents.x;

        // Set bounds accounting for player width
        leftBound = leftEdge.x + playerHalfWidth;
        rightBound = rightEdge.x - playerHalfWidth;

        // Initialize last frame position
        xPositionLastFrame = transform.position.x;
        currentInputX = 0f;

        // If no groundCheck transform was provided, create one at the feet
        if (groundCheck == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.parent = transform;
            // place just below the sprite
            float feetOffset = -(spriteRenderer.bounds.extents.y + 0.05f);
            gc.transform.localPosition = new Vector3(0f, feetOffset, 0f);
            groundCheck = gc.transform;
        }

        Debug.Log($"Player half-width: {playerHalfWidth}");
        Debug.Log($"Left bound: {leftBound}, Right bound: {rightBound}");
    }

    void Update()
    {
        CheckGrounded();
        HandleMovement();
        ClampMovement();
        FlipCharacterX();
        HandleJump();
    }

    private void CheckGrounded()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        // Check overlap circle for any collider on the ground layer
        Collider2D hit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        isGrounded = (hit != null);
    }

    private void HandleJump()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        // New Input System - Check for jump input
        if (isGrounded && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Jump();
        }
        else if (isGrounded && Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            Jump(); // A button on Xbox, X on PlayStation, B on Switch
        }
#else
        // Legacy Input System
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
#endif
    }

    private void Jump()
    {
        if (rigidBody != null)
        {
            rigidBody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            // Immediately mark as not grounded to avoid double-jump within the same frame
            isGrounded = false;
            Debug.Log("Jump executed!");
        }
        else
        {
            Debug.LogError("Rigidbody2D is missing! Cannot jump.");
        }
    }

    private void HandleMovement()
    {
        // Reset movement each frame to avoid accumulation
        movement = Vector2.zero;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        // New Input System
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
        // Legacy Input System
        float input = Input.GetAxis("Horizontal");
        movement.x = input * speed * Time.deltaTime;
        currentInputX = input;
#endif

        transform.Translate(new Vector3(movement.x, 0f, 0f));
    }

    private void ClampMovement()
    {
        // Clamp the player's position to screen bounds (accounting for player width)
        float clampedX = Mathf.Clamp(transform.position.x, leftBound, rightBound);
        Vector3 pos = transform.position;
        pos.x = clampedX;
        transform.position = pos;
    }

    private void FlipCharacterX()
    {
        // Skip flipping if spriteRenderer is missing
        if (spriteRenderer == null) return;

        // Flip the character based on movement direction using stored input value
        if (currentInputX > 0)
        {
            spriteRenderer.flipX = false; // Facing right
        }
        else if (currentInputX < 0)
        {
            spriteRenderer.flipX = true; // Facing left
        }

        // Update the last frame position
        xPositionLastFrame = transform.position.x;
    }

    // Visualize ground check in the editor
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}