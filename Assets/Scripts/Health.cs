using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    public int maxLives = 3;
    private int currentLives;
    public Slider healthBar;

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
        Debug.Log("Player died! Restarting level...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}