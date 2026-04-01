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

    void Start()
    {
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

        Debug.Log($"Player half-width: {playerHalfWidth}");
        Debug.Log($"Left bound: {leftBound}, Right bound: {rightBound}");
    }

    void Update()
    {
        HandleMovement();
        ClampMovement();
        FlipCharacterX();
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
#else
        // Legacy Input System
        float input = Input.GetAxis("Horizontal");
        movement.x = input * speed * Time.deltaTime;
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

        // Flip the character based on movement direction
        if (transform.position.x > xPositionLastFrame)
        {
            spriteRenderer.flipX = false; // Facing right
        }
        else if (transform.position.x < xPositionLastFrame)
        {
            spriteRenderer.flipX = true; // Facing left
        }

        // Update the last frame position
        xPositionLastFrame = transform.position.x;
    }
}