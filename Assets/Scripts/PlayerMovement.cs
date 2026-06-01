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
    [SerializeField] private int maxJumps = 2;
    private int jumpsRemaining;
    private bool isGrounded;

    // Coyote time implementation
    private float groundedTimer = 0f;
    [SerializeField] private float coyoteTime = 0.15f;

    // Add jump buffer for better responsiveness
    private float jumpBufferTimer = 0f;
    [SerializeField] private float jumpBufferTime = 0.1f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.25f;

    [SerializeField] private Transform groundCheckFront;
    [SerializeField] private Transform groundCheckBack;

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
    [SerializeField] private AudioClip doubleJumpSound;
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private AudioClip[] runSounds;
    [SerializeField] private float footstepInterval = 0.3f;
    [SerializeField] private AudioSource runAudioSource;

    private AudioSource audioSource;
    private float footstepTimer = 0f;

    public CoinManager cm;
    private Transform currentPlatform;
    private Vector3 lastPlatformPosition;

    private bool jumpRequested = false;

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

        if (groundCheckFront == null)
        {
            GameObject gc = new GameObject("GroundCheckFront");
            gc.transform.parent = transform;
            gc.transform.localPosition = new Vector3(0.3f, -0.5f, 0f);
            groundCheckFront = gc.transform;
        }

        if (groundCheckBack == null)
        {
            GameObject gc = new GameObject("GroundCheckBack");
            gc.transform.parent = transform;
            gc.transform.localPosition = new Vector3(-0.3f, -0.5f, 0f);
            groundCheckBack = gc.transform;
        }

        if (groundLayer == 0)
        {
            groundLayer = LayerMask.GetMask("Default");
            Debug.LogWarning("No ground layer assigned, defaulting to 'Default' layer", this);
        }

        jumpsRemaining = maxJumps;
        ChangeAnimationState("Idle");
    }

    void Update()
    {
        UpdateTimers();
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

        HandleJumpInput();
        HandleMovement();
        HandleDash();
        FlipCharacterX();
        UpdateAnimations();
        HandleRunSounds();
        HandleMovingPlatform();

        ProcessJump();
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
            Vector2 targetVelocity = new Vector2(currentInputX * speed, rigidBody.linearVelocity.y);
            rigidBody.linearVelocity = targetVelocity;

            if (rigidBody.gravityScale != originalGravity)
            {
                rigidBody.gravityScale = originalGravity;
            }
        }
    }

    private void UpdateTimers()
    {
        if (groundedTimer > 0)
        {
            groundedTimer -= Time.deltaTime;
        }

        if (jumpBufferTimer > 0)
        {
            jumpBufferTimer -= Time.deltaTime;
        }
    }

    private void CheckGrounded()
    {
        bool wasGrounded = isGrounded;

        // Physics checks to see if we are PHYSICALLY on the ground right now
        bool mainGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) != null;
        bool frontGrounded = groundCheckFront != null && Physics2D.OverlapCircle(groundCheckFront.position, groundCheckRadius, groundLayer) != null;
        bool backGrounded = groundCheckBack != null && Physics2D.OverlapCircle(groundCheckBack.position, groundCheckRadius, groundLayer) != null;
        RaycastHit2D rayHit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
        bool rayGrounded = rayHit.collider != null;

        // This is actual hardware-level grounding
        isGrounded = mainGrounded || frontGrounded || backGrounded || rayGrounded;

        if (isGrounded)
        {
            // Reset coyote timer continuously while genuinely touching floor
            groundedTimer = coyoteTime;

            // Moving platform detection
            Collider2D hitCollider = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            if (hitCollider != null && (hitCollider.CompareTag("MovingPlatform") || hitCollider.transform.parent?.CompareTag("MovingPlatform") == true))
            {
                currentPlatform = hitCollider.transform;
                while (currentPlatform.parent != null && currentPlatform.parent.CompareTag("MovingPlatform"))
                {
                    currentPlatform = currentPlatform.parent;
                }
            }

            // FIXED: Reliably reset jumps on true physical landing
            if (!wasGrounded)
            {
                jumpsRemaining = maxJumps;
                canDash = true;
                Debug.Log($"Landed! Jumps reset to {jumpsRemaining}");
            }
        }
    }

    private void HandleJumpInput()
    {
        bool jumpPressed = false;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            jumpPressed = true;
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
            jumpPressed = true;
#else
        if (Input.GetButtonDown("Jump"))
            jumpPressed = true;
#endif

        if (jumpPressed && !isDashing)
        {
            jumpBufferTimer = jumpBufferTime;
            jumpRequested = true;
        }
    }

    private void ProcessJump()
    {
        // FIXED: Use groundedTimer > 0 for first jump (Coyote Time check)
        if (jumpRequested && jumpBufferTimer > 0)
        {
            bool hasCoyoteTime = groundedTimer > 0f;
            bool isFirstJump = (jumpsRemaining == maxJumps);

            // Condition: Either we have Coyote Time active, or we are doing a mid-air multi-jump
            if (hasCoyoteTime || (!isFirstJump && jumpsRemaining > 0))
            {
                // If we fell off a ledge without jumping and use coyote time, consume the first jump
                if (hasCoyoteTime && isFirstJump)
                {
                    // Perfectly valid first jump
                }
                else if (!hasCoyoteTime && isFirstJump)
                {
                    // Fell off ledge completely past coyote time: penalize one jump
                    jumpsRemaining--;
                }

                if (currentPlatform != null)
                {
                    currentPlatform = null;
                    lastPlatformPosition = Vector3.zero;
                }

                PerformJump();
            }

            // Clean up request flags regardless of success to prevent queued misfires
            jumpRequested = false;
            jumpBufferTimer = 0;
        }
    }

    private void PerformJump()
    {
        // Track jump state before updating counts
        bool isFirstJump = (jumpsRemaining == maxJumps);

        rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, 0f);
        rigidBody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        jumpsRemaining--;

        if (isFirstJump)
        {
            if (jumpSound != null) PlaySound(jumpSound, 0.6f);
            Debug.Log($"First Jump! Jumps remaining: {jumpsRemaining}");
        }
        else
        {
            if (doubleJumpSound != null) PlaySound(doubleJumpSound, 0.7f);
            else if (jumpSound != null) PlaySound(jumpSound, 0.5f);
            Debug.Log($"DOUBLE JUMP! Jumps remaining: {jumpsRemaining}");
        }

        ChangeAnimationState("Jump");

        // Clear timers instantly so we don't double jump instantly
        groundedTimer = 0f;
        isGrounded = false;
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
    }

    private void HandleDash()
    {
        if (!enableDash) return;

        bool dashPressed = false;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        if (Keyboard.current != null && Keyboard.current.leftShiftKey.wasPressedThisFrame)
            dashPressed = true;
        if (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame)
            dashPressed = true;
#else
        if (Input.GetKeyDown(KeyCode.LeftShift))
            dashPressed = true;
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
        else if (spriteRenderer != null && !spriteRenderer.flipX)
        {
            dashDirection = 1f;
        }
        else
        {
            dashDirection = -1f;
        }

        rigidBody.linearVelocity = new Vector2(dashDirection * dashingPower, 0f);
        rigidBody.gravityScale = 0f;

        if (trailRenderer != null) trailRenderer.emitting = true;
        if (dashSound != null) PlaySound(dashSound, 0.8f);

        ChangeAnimationState("Dash");
        StartCoroutine(DashCooldownReset());
    }

    private void StopDash()
    {
        isDashing = false;
        if (trailRenderer != null) trailRenderer.emitting = false;

        rigidBody.linearVelocity = new Vector2(0f, rigidBody.linearVelocity.y);
        rigidBody.gravityScale = originalGravity;

        ChangeAnimationState("Idle");
    }

    private IEnumerator DashCooldownReset()
    {
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    private void HandleMovingPlatform()
    {
        if (currentPlatform != null && isGrounded && !isDashing)
        {
            // Vector3 platformDelta = currentPlatform.position - lastPlatformPosition;
            // Unused placeholder assignment to remain consistent with your setup
            lastPlatformPosition = currentPlatform.position;
        }
    }

    private void HandleRunSounds()
    {
        bool isMoving = Mathf.Abs(currentInputX) > 0.05f;

        if (isGrounded && !isDashing && isMoving && runSounds != null && runSounds.Length > 0)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f)
            {
                int randomIndex = Random.Range(0, runSounds.Length);
                if (runAudioSource != null)
                {
                    if (runAudioSource.isPlaying) runAudioSource.Stop();
                    runAudioSource.PlayOneShot(runSounds[randomIndex], 0.5f);
                }
                footstepTimer = footstepInterval;
            }
        }
        else
        {
            footstepTimer = 0f;
            if (runAudioSource != null && runAudioSource.isPlaying) runAudioSource.Stop();
        }
    }

    private void UpdateAnimations()
    {
        if (isDashing)
        {
            ChangeAnimationState("Dash");
            return;
        }

        if (!isGrounded && rigidBody.linearVelocity.y > 0.1f)
        {
            ChangeAnimationState("Jump");
            return;
        }

        if (!isGrounded && rigidBody.linearVelocity.y < -0.1f)
        {
            ChangeAnimationState("Fall");
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
            if (animator.HasState(0, Animator.StringToHash(newState)))
            {
                animator.Play(newState);
            }
        }
        currentState = newState;
    }

    private void FlipCharacterX()
    {
        if (isDashing) return;

        if (currentInputX > 0) spriteRenderer.flipX = false;
        else if (currentInputX < 0) spriteRenderer.flipX = true;
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
        jumpsRemaining = maxJumps;
        groundedTimer = coyoteTime;
        jumpBufferTimer = 0;
        jumpRequested = false;

        if (rigidBody != null)
        {
            rigidBody.linearVelocity = Vector2.zero;
            rigidBody.gravityScale = originalGravity;
        }

        if (trailRenderer != null) trailRenderer.emitting = false;
        footstepTimer = 0f;

        if (runAudioSource != null && runAudioSource.isPlaying) runAudioSource.Stop();
        if (audioSource != null && audioSource.isPlaying) audioSource.Stop();

        ChangeAnimationState("Idle");
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckDistance);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("coin"))
        {
            Destroy(other.gameObject);
            if (cm != null) cm.coinCount++;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            if (currentPlatform == collision.transform)
            {
                currentPlatform = null;
                lastPlatformPosition = Vector3.zero;
            }
        }
    }
}