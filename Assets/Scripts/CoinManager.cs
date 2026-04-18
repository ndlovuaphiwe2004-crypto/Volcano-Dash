using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;
    private int coins;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    [SerializeField] private TMPro.TMP_Text coinsDisplay;

    private void OnGUI() 
    { if (coinsDisplay != null) 
        {
            coinsDisplay.text = coins.ToString();
        }
    }
    public void ChangeCoins(int amount)
    {
        coins += amount;
    }
}
