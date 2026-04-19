using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Health : MonoBehaviour
{
    public int maxLives = 3;
    private int currentLives;
    public Slider healthBar;
    public Transform respawnPoint;

    void Start()
    {
        currentLives = maxLives;

        if (healthBar != null)
        {
            healthBar.maxValue = maxLives;
            healthBar.minValue = 0;
            healthBar.wholeNumbers = false;  // THIS MUST BE FALSE for thirds to work
            healthBar.value = currentLives;
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

        if (healthBar != null)
        {
            healthBar.value = currentLives;
        }

        Debug.Log("Lives: " + currentLives + "/" + maxLives);

        if (currentLives <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("PLAYER DIED! Respawning...");

        // Reset lives to full
        currentLives = maxLives;

        if (healthBar != null)
        {
            healthBar.value = currentLives;
        }

        // Teleport player to respawn point
        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;

            // Reset velocity
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            // Reset player movement state
            PlayerMovement playerMovement = GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.ResetPlayerState();
            }
        }
        else
        {
            Debug.LogWarning("Respawn point not assigned! Please drag a Transform to the Respawn Point field.");
        }
    }
}