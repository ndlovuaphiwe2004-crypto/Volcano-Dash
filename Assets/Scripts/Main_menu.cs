using UnityEngine;
using UnityEngine.SceneManagement;

public class Main_menu : MonoBehaviour
{
    [SerializeField] GameObject mainMenu; 
    public void PlayGame()
    {
        SceneManager.LoadScene("Thabo level 1");
    }
}
