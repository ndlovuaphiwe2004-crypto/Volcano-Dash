using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelMenu : MonoBehaviour
{
    public Button[] buttons;
    public GameObject levelButtons;

    private void Awake()
    {
        ButtonsToArray();

        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = false;
        }

        int levelsToUnlock = Mathf.Min(unlockedLevel, buttons.Length);
        for (int i = 0; i < levelsToUnlock; i++)
        {
            buttons[i].interactable = true;
        }
    }

    public void OpenLevel(int levelId)
    {
        int currentSavedHighest = PlayerPrefs.GetInt("UnlockedLevel", 1);

        int nextLevel = levelId + 1;

        if (nextLevel > currentSavedHighest)
        {
            PlayerPrefs.SetInt("UnlockedLevel", nextLevel);
            PlayerPrefs.Save();
        }

        string levelName = "Level " + levelId;
        SceneManager.LoadScene(levelName);
    }

    void ButtonsToArray()
    {
        int childCount = levelButtons.transform.childCount;
        buttons = new Button[childCount];
        for (int i = 0; i < childCount; i++)
        {
            buttons[i] = levelButtons.transform.GetChild(i).gameObject.GetComponent<Button>();
        }
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("UnlockedLevel");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}