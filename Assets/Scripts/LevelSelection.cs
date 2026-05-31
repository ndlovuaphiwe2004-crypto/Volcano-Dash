using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelection : MonoBehaviour
{
    public void back()
    {
        SceneManager.LoadScene("Testing mainM");
    }

    public void firstLevel()
    {
        SceneManager.LoadScene("Level 1");
    }

    public void secondLevel()
    {
         SceneManager.LoadScene("Level 2");
    }
}


