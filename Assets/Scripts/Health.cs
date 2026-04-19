using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public int maxLives = 3;
    private int currentLives;
    public Slider healthBar;

    void Awake()
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

        if (currentLives <= 0)
        {
            Debug.Log("PLAYER DIED");
        }
    }
}