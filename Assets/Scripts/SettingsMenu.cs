using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] GameObject settingsMenu;
    public void Settings()
    {
        settingsMenu.SetActive(true);
    }
    public void Back()
    {
        settingsMenu.SetActive(false);
    }

    public void Controls()
    {
        SceneManager.LoadScene("Control_Menu");
    }
}
