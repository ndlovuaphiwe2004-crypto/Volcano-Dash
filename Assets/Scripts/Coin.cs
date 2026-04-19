using UnityEngine;

public class Coin : MonoBehaviour
{
    
    public void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(gameObject);  
    }   
}
