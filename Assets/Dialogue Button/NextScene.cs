using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // Call this method to load the next scene
    public void LoadNextScene()
    {
        // Get the current scene index
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Load the next scene in the Build Settings
        SceneManager.LoadScene(currentSceneIndex + 1);
    }
}
