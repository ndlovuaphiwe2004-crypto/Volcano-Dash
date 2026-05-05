using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    [SerializeField] Image soundOnIcon;
    [SerializeField] Image soundOffIcon;
    private bool muted = false;

    void Start()
    {
        if (!PlayerPrefs.HasKey("muted"))
        {
            PlayerPrefs.SetInt("muted", 0);
            load();
        }
        else
        {
            load();
        }

        UpdateButtonIcon();
        AudioListener.pause = muted;
    }

    public void OnSoundToggle()
    {
        if (muted == false)
        {
            muted = true;
            AudioListener.pause = true;
        }

        else
        {
            muted = false;
            AudioListener.pause = false;
        }
        save();
        UpdateButtonIcon();
    }
    public void OffSoundToggle()
    {
        if (muted == true)
        {
            muted = false ;
            AudioListener.pause = false;
        }
        save();
        UpdateButtonIcon();
    }


    private void UpdateButtonIcon()
    {
        if (muted == false)
        {
            soundOnIcon.enabled = true;
            soundOffIcon.enabled = false;
        }
        else
        {
            soundOnIcon.enabled = false;
            soundOffIcon.enabled = true;
        }
    }

    private void load()
    {
            muted = PlayerPrefs.GetInt("muted") == 1;
    }

    private void save()
    {
        PlayerPrefs.SetInt("muted", muted ? 1 : 0);
        PlayerPrefs.Save();
    }   
}
