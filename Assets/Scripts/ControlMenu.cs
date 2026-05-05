using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlMenu : MonoBehaviour
{
    [SerializeField] GameObject controlMenu;

    public void Back()
    {
        SceneManager.LoadScene("Main_Menu");
    }
}