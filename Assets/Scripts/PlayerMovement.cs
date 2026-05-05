using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private float currentInputX;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Jump Settings")]
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private float jumpForce = 8f;
    private bool isGrounded;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckDistance = 0.3f;

    [Header("Dash Settings")]
    [SerializeField] private bool enableDash = true;
    [SerializeField] private float dashingPower = 30f;
    [SerializeField] private float dashDuration = 0.35f;
    [SerializeField] private float dashingCooldown = 0.5f;
    [SerializeField] private TrailRenderer trailRenderer;

    private bool canDash = true;
    private bool isDashing;
    private float dashDirection = 1f;
    private float originalGravity;
    private float dashTimeLeft;

    private string currentState = "Idle";

    [Header("Sound Effects")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private AudioClip[] runSounds;
    [SerializeField] private float footstepInterval = 0.3f;
    [SerializeField] private AudioSource runAudioSource;

    private AudioSource audioSource;
    private float footstepTimer = 0f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (audioSource != null)
        {
            audioSource.spatialBlend = 0f;
            audioSource.volume = 0.7f;
            audioSource.playOnAwake = false;
        }

        if (runAudioSource == null)
        {
            runAudioSource = gameObject.AddComponent<AudioSource>();
            runAudioSource.spatialBlend = 0f;
            runAudioSource.volume = 0.5f;
            runAudioSource.playOnAwake = false;
        }

        if (animator == null) animator = GetComponent<Animator>();
        if (rigidBody == null) rigidBody = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        originalGravity = rigidBody.gravityScale;

        if (groundCheck == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.parent = transform;
            gc.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            groundCheck = gc.transform;
        }

        ChangeAnimationState("Idle");
    }

    void Update()
    {
        CheckGrounded();

        if (isDashing)
        {
            dashTimeLeft -= Time.deltaTime;
            if (dashTimeLeft <= 0f)
            {
                StopDash();
            }
            return;
        }

        HandleJump();
        HandleMovement();
        HandleDash();
        FlipCharacterX();
        UpdateAnimations();
        HandleRunSounds();
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            rigidBody.linearVelocity = new Vector2(dashDirection * dashingPower, 0f);
            rigidBody.gravityScale = 0f;
        }
        else
        {
            if (rigidBody.gravityScale != originalGravity)
            {
                rigidBody.gravityScale = originalGravity;
            }
        }
    }

    private void CheckGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance);
        bool wasGrounded = isGrounded;
        isGrounded = hit.collider != null;

        if (isGrounded && !wasGrounded)
        {
            canDash = true;
            Debug.Log("Landed on ground");
        }
    }

    private void HandleJump()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        bool jumpPressed = false;
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            jumpPressed = true;
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
            jumpPressed = true;

        if (jumpPressed && isGrounded)
        {
            PerformJump();
        }
#else
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            PerformJump();
        }
#endif
    }

    private void PerformJump()
    {
        rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, 0f);
        rigidBody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        if (jumpSound != null)
        {
            PlaySound(jumpSound, 0.6f);
        }

        ChangeAnimationState("Jump");
    }

    private void HandleMovement()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        if (Keyboard.current != null)
        {
            float left = (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) ? -1f : 0f;
            float right = (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) ? 1f : 0f;
            currentInputX = left + right;
        }
        if (Gamepad.current != null)
        {
            currentInputX = Gamepad.current.leftStick.ReadValue().x;
        }
#else
        currentInputX = Input.GetAxis("Horizontal");
#endif

        transform.Translate(new Vector3(currentInputX * speed * Time.deltaTime, 0f, 0f));
    }

    private void HandleDash()
    {
        if (!enableDash) return;

        bool dashPressed = false;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        if (Keyboard.current != null && Keyboard.current.leftShiftKey.wasPressedThisFrame)
        {
            dashPressed = true;
        }
        if (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame)
        {
            dashPressed = true;
        }
#else
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            dashPressed = true;
        }
#endif

        if (dashPressed && canDash && !isDashing && isGrounded)
        {
            StartDash();
        }
    }

    private void StartDash()
    {
        isDashing = true;
        canDash = false;
        dashTimeLeft = dashDuration;

        if (Mathf.Abs(currentInputX) > 0.1f)
        {
            dashDirection = Mathf.Sign(currentInputX);
        }
        else if (!spriteRenderer.flipX)
        {
            dashDirection = 1f;
        }
        else
        {
            dashDirection = -1f;
        }

        rigidBody.linearVelocity = new Vector2(dashDirection * dashingPower, 0f);
        rigidBody.gravityScale = 0f;

        if (trailRenderer != null)
        {
            trailRenderer.emitting = true;
        }

        if (dashSound != null)
        {
            PlaySound(dashSound, 0.8f);
        }

        ChangeAnimationState("Dash");
        Debug.Log("Dash started");

        StartCoroutine(DashCooldownReset());
    }

    private void StopDash()
    {
        isDashing = false;

        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }

        rigidBody.linearVelocity = new Vector2(0f, rigidBody.linearVelocity.y);
        rigidBody.gravityScale = originalGravity;

        ChangeAnimationState("Idle");
        Debug.Log("Dash stopped");
    }

    private IEnumerator DashCooldownReset()
    {
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    private void HandleRunSounds()
    {
        bool isMoving = Mathf.Abs(currentInputX) > 0.05f;

        if (isGrounded && !isDashing && isMoving)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f && runSounds != null && runSounds.Length > 0)
            {
                int randomIndex = Random.Range(0, runSounds.Length);

                if (runAudioSource != null)
                {
                    if (runAudioSource.isPlaying)
                    {
                        runAudioSource.Stop();
                    }
                    runAudioSource.PlayOneShot(runSounds[randomIndex], 0.5f);
                }
                footstepTimer = footstepInterval;
            }
        }
        else
        {
            footstepTimer = 0f;
            if (runAudioSource != null && runAudioSource.isPlaying)
            {
                runAudioSource.Stop();
            }
        }
    }

    private void UpdateAnimations()
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
        if (animator != null)
        {
            animator.Play(newState);
        }
        currentState = newState;
    }

    private void FlipCharacterX()
    {
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

    private void PlaySound(AudioClip clip, float volumeScale = 1f)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volumeScale);
        }
    }

    public void ResetPlayerState()
    {
        StopAllCoroutines();
        isDashing = false;
        canDash = true;
        isGrounded = true;

        if (rigidBody != null)
        {
            rigidBody.linearVelocity = Vector2.zero;
            rigidBody.gravityScale = originalGravity;
        }

        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }

        footstepTimer = 0f;

        if (runAudioSource != null && runAudioSource.isPlaying)
        {
            runAudioSource.Stop();
        }

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        ChangeAnimationState("Idle");
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckDistance);
        Gizmos.DrawWireSphere(groundCheck.position + Vector3.down * groundCheckDistance, 0.05f);
    }
}