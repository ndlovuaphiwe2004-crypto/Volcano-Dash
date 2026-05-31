using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishPoint : MonoBehaviour
{
     // [SerializeField] bool goNextLevel;
     // [SerializeField] string levelName;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            UnlockNewLevel();
            SceneSwitch.instance.NextLevel();
        }
    }

    void UnlockNewLevel()
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        // Only increase if we’re at the highest unlocked so far
        if (SceneManager.GetActiveScene().name == "Level 1" && unlockedLevel < 2)
        {
            PlayerPrefs.SetInt("UnlockedLevel", 2);
            PlayerPrefs.Save();
            Debug.Log("Unlocked Level 2!");
        }
        else if (SceneManager.GetActiveScene().name == "Level 2" && unlockedLevel < 3)
        {
            PlayerPrefs.SetInt("UnlockedLevel", 3);
            PlayerPrefs.Save();
            Debug.Log("Unlocked Level 3!");
        }
    }
}