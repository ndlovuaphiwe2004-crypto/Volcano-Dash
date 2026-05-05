using UnityEngine;
using UnityEngine.SceneManagement;

public class Main_menu : MonoBehaviour
{
    [SerializeField] GameObject mainMenu; 
    public void PlayGame()
    {
        SceneManager.LoadScene("Aphiwe1 Thabo1");
    }
}
