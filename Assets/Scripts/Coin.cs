using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private int value = 1;
    private bool hasTriggered;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasTriggered)
        {
            return;
        }

        if (collision == null || collision.transform == null)
        {
            return;
        }

        if (collision.CompareTag("Player"))
        {
            hasTriggered = true;
            CoinManager.Instance?.ChangeCoins(value);
            Destroy(gameObject);
        }
    }
}       