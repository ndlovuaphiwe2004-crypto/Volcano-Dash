using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
    private static BackgroundMusic backgroundMusic;
    private AudioSource audioSource;

    void Awake()
    {
        if (backgroundMusic == null)
        {
            backgroundMusic = this;
            DontDestroyOnLoad(gameObject);

            audioSource = GetComponent<AudioSource>();
            audioSource.loop = true;
            ApplyMuteState();
            audioSource.Play();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ApplyMuteState()
    {
        bool muted = PlayerPrefs.GetInt("muted", 0) == 1;
        audioSource.mute = muted;   // only mute music
    }
}
