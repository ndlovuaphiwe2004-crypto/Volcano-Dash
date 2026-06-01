using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void PlayGame()
    {
        // Make sure the game isn’t paused
        Time.timeScale = 1f;

        // Load Scene 1 normally (replace with your scene name)
        SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
    }

    public void ResetProgress()
    {
        PlayerPrefs.SetInt("UnlockedLevel", 1); // lock back to Level 1 only
        PlayerPrefs.SetInt("ReachedIndex", 0);  // optional, if you use it
        PlayerPrefs.Save();
        Debug.Log("Progress reset. Only Level 1 unlocked.");
    }

}
