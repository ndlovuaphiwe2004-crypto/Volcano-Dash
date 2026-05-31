using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    public int maxLives = 3;
    private int currentLives;
    public Slider healthBar;

    // Add this for the red flash
    [Header("Damage Flash")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private string flashTriggerName = "FlashRed";
    // Just added
    private bool isDead;
    public GameOver gameOver;


    void Start()
    {
        currentLives = maxLives;

        if (healthBar != null)
        {
            healthBar.maxValue = maxLives;
            healthBar.minValue = 0;
            healthBar.wholeNumbers = false;
            healthBar.value = currentLives;
        }

        // Auto-find animator if not assigned
        if (playerAnimator == null)
        {
            playerAnimator = GetComponent<Animator>();
            if (playerAnimator == null)
                playerAnimator = GetComponentInChildren<Animator>();
            if (playerAnimator == null)
                playerAnimator = GetComponentInParent<Animator>();

            if (playerAnimator == null)
            {
                Debug.LogWarning($"Health: Animator not found on '{name}'. Assign it in the inspector.");
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle"))
        {
            TakeDamage(1);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            TakeDamage(1);
        }
    }

    public void TakeDamage(int damage)
    {
        currentLives -= damage;
        currentLives = Mathf.Max(0, currentLives);

        // TRIGGER THE RED FLASH
        TriggerRedFlash();

        if (healthBar != null)
        {
            healthBar.value = currentLives;
        }

        Debug.Log("Lives: " + currentLives + "/" + maxLives);

        // even here
        if (currentLives <= 0 && !isDead)
        {
            isDead = true;
            gameOver.GameOverScreen();
        }
    }

    void TriggerRedFlash()
    {
        if (playerAnimator == null)
            return;

        // Stop any existing flash routine
        StopAllCoroutines();

        // CRITICAL FIX: Reset ALL triggers and force the animator back to idle
        playerAnimator.Rebind(); // This resets the entire animator state

        // Re-enable the animator if Rebind disabled it
        playerAnimator.enabled = true;

        // Now trigger the flash
        playerAnimator.SetTrigger(flashTriggerName);

        // Start coroutine to force stop the flash
        StartCoroutine(ForceStopFlash());
    }

    IEnumerator ForceStopFlash()
    {
        yield return new WaitForSeconds(0.15f);

        if (playerAnimator != null)
        {
            // Force the animator back to a clean state
            playerAnimator.Rebind();
            playerAnimator.enabled = true;

            // Clear the trigger
            playerAnimator.ResetTrigger(flashTriggerName);
        }
    }

    void Die()
    {
        Debug.Log("Player died! Restarting level...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
