using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ObjectivePanel : MonoBehaviour
{
    public GameObject objectivePanel;
    public TextMeshProUGUI levelText;        // "Level 1", "Level 2"
    public TextMeshProUGUI descriptionText;  // The long sentence

    // Coin requirement
    public Image coinIcon;
    public TextMeshProUGUI coinText;

    // Gem requirement
    public Image gemIcon;
    public TextMeshProUGUI gemText;

    // Scroll requirement
    public Image scrollIcon;
    public TextMeshProUGUI scrollText;

    public Button nextButton;

    void Start()
    {
        Time.timeScale = 0f; // pause gameplay

        int levelIndex = SceneManager.GetActiveScene().buildIndex;

        if (levelIndex == 4) // Level 1
        {
            levelText.text = "Level 1";
            descriptionText.text = "Level requirements: collect the following items to clear the level";

            coinIcon.gameObject.SetActive(true);
            coinText.text = "×5";

            gemIcon.gameObject.SetActive(true);
            gemText.text = "×3";

            scrollIcon.gameObject.SetActive(true);
            scrollText.text = "×1";
        }
        else if (levelIndex == 5) // Level 2
        {
            levelText.text = "Level 2";
            descriptionText.text = "Level requirements: collect the following items to clear the level";

            coinIcon.gameObject.SetActive(true);
            coinText.text = "×10";

            gemIcon.gameObject.SetActive(true);
            gemText.text = "×5";

            scrollIcon.gameObject.SetActive(true);
            scrollText.text = "×2";
        }
        else
        {
            objectivePanel.SetActive(false);
            Time.timeScale = 1f;
            return;
        }

        objectivePanel.SetActive(true);

        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(ClosePanelAndStartGame);
    }

    void ClosePanelAndStartGame()
    {
        objectivePanel.SetActive(false);
        Time.timeScale = 1f; // resume gameplay
    }
}
