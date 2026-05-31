using UnityEngine;
using UnityEngine.SceneManagement;


public class GameOver : MonoBehaviour
{
    public GameObject gameOverUI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GameOverScreen()
    {
        gameOverUI.SetActive(true);
        Time.timeScale = 0f; // Freeze everything
    }

    public void Restart()
    {
        Time.timeScale = 1f; // Resume before reload
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void MainMenu()
    {
        Time.timeScale = 1f; // Resume before reload
        SceneManager.LoadScene("Testing mainM");
    }
}