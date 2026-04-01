using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }
    public float speed = 5f; // Speed of the player movement

    private Vector2 movement;

    // Update is called once per frame
    void Update()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        // New Input System: prefer gamepad left stick, fallback to keyboard A/D or arrows
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
        // Legacy Input Manager
        float input = Input.GetAxis("Horizontal");
        movement.x = input * speed * Time.deltaTime;
#endif

        transform.Translate(movement);
    }
}