using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pausepanel;
    private bool isPaused = false;

    void Start()
    {
        if (pausepanel == null)
        {
            Debug.LogError("Pause Menu Panel is not assigned! Please drag your pause menu UI into the script.");
            return;
        }

        pausepanel.SetActive(false);
        Time.timeScale = 1f;
        Debug.Log("Pause menu initialized. Press Escape to pause.");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Escape key pressed!");

            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        pausepanel.SetActive(true);
        Time.timeScale = 0f;
        Debug.Log("Game Paused");
    }

    public void ResumeGame()
    {
        isPaused = false;
        pausepanel.SetActive(false);
        Time.timeScale = 1f;
        Debug.Log("Game Resumed");
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("Restarting level");
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main_Menu");
        Debug.Log("Quitting to main menu");
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Debug.Log("Quitting game");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}