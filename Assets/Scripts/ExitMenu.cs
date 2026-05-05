using UnityEngine;

public class ExitMenu : MonoBehaviour
{
    [SerializeField] GameObject exitMenu;

    public void ShowExitMenu()
    {
        exitMenu.SetActive(true);
    }

    public void HideExitMenu()
    {
        exitMenu.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}