using UnityEngine;

public class Damage : MonoBehaviour
{
    public int damage = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Health playerHealth = other.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }

        // Trigger a damage flash on the player if a flash component exists.
        // This uses SendMessage so it won't require a compile-time dependency on a specific method name.
        other.SendMessage("TriggerFlash", SendMessageOptions.DontRequireReceiver);
    }
}