using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    private Vector2 movement;
    private float leftBound;
    private float rightBound;
    private float playerHalfWidth;

    void Start()
    {
        // Get the camera's boundaries in world space
        Camera mainCamera = Camera.main;

        // Get the world position of the left and right edges of the screen
        Vector2 leftEdge = mainCamera.ScreenToWorldPoint(new Vector2(0, 0));
        Vector2 rightEdge = mainCamera.ScreenToWorldPoint(new Vector2(Screen.width, 0));

        // Get the player's half-width from the sprite renderer
        playerHalfWidth = GetComponent<SpriteRenderer>().bounds.extents.x;

        // Set bounds accounting for player width
        leftBound = leftEdge.x + playerHalfWidth;
        rightBound = rightEdge.x - playerHalfWidth;

        print($"Player half-width: {playerHalfWidth}");
        print($"Left bound: {leftBound}, Right bound: {rightBound}");
    }

    void Update()
    {
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
#else
        float input = Input.GetAxis("Horizontal");
        movement.x = input * speed * Time.deltaTime;
#endif

        transform.Translate(movement);

        // Clamp the player's position to screen bounds (accounting for player width)
        float clampedX = Mathf.Clamp(transform.position.x, leftBound, rightBound);
        Vector2 pos = transform.position;
        pos.x = clampedX;
        transform.position = pos;
    }
}