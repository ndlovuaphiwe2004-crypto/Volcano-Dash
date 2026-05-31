using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private Image soundOnIcon;
    [SerializeField] private Image soundOffIcon;
    private bool muted;

    void Start()
    {
        // Load saved mute state
        muted = PlayerPrefs.GetInt("muted", 0) == 1;
        ApplyMute();
        UpdateButtonIcon();
    }

    public void ToggleSound()
    {
        // Flip mute state
        muted = !muted;
        PlayerPrefs.SetInt("muted", muted ? 1 : 0);
        PlayerPrefs.Save();

        ApplyMute();
        UpdateButtonIcon();
    }

    private void ApplyMute()
    {
        // Only mute background music, not all sounds
        BackgroundMusic bg = Object.FindFirstObjectByType<BackgroundMusic>();
        if (bg != null) bg.ApplyMuteState();
    }

    private void UpdateButtonIcon()
    {
        soundOnIcon.enabled = !muted;
        soundOffIcon.enabled = muted;
    }
}
