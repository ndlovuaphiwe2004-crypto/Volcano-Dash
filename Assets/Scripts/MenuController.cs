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
}
