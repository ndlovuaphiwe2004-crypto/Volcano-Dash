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

        Debug.Log($"Player half-width: {playerHalfWidth}");
        Debug.Log($"Left bound: {leftBound}, Right bound: {rightBound}");
    }

    void Update()
    {
        HandleMovement();
        ClampMovement();
        FlipCharacterX();
        HandleJump();
    }

    private void HandleJump()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        // New Input System - Check for jump input
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Jump();
        }
        else if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            Jump(); // A button on Xbox, X on PlayStation, B on Switch
        }
#else
        // Legacy Input System
        if (Input.GetKeyDown(KeyCode.Space))
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
            Debug.Log("Jump executed!");
        }
        else
        {
            Debug.LogError("Rigidbody2D is missing! Cannot jump.");
        }
    }

    private void HandleMovement()
    {
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

        transform.Translate(movement);
    }

    private void ClampMovement()
    {
        // Clamp the player's position to screen bounds (accounting for player width)
        float clampedX = Mathf.Clamp(transform.position.x, leftBound, rightBound);
        Vector2 pos = transform.position;
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
}