using UnityEngine;

public class Gem : MonoBehaviour
{

    public void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(gameObject);
    }
}
