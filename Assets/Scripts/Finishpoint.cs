using UnityEngine;

public class FinishPoint : MonoBehaviour
{
    [SerializeField] bool goNextLevel;
    [SerializeField] string levelName;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (goNextLevel)
            {
                SceneSwitch.instance.NextLevel();
            }
            else
            {
                SceneSwitch.instance.LoadScene(levelName);
            }
        }
    }
}