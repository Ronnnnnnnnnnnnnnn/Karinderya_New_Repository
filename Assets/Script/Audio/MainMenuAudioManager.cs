using UnityEngine;

public class MainMenuAudioManager : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource musicSource;

    [Header("Main Menu Music")]
    public AudioClip mainMenuMusic;

    [Range(0f, 1f)]
    public float musicVolume = 0.35f;

    [Tooltip("Automatically play the main menu music when the scene starts.")]
    public bool playMusicOnStart = true;


    private void Start()
    {
        if (playMusicOnStart &&
            mainMenuMusic != null &&
            musicSource != null)
        {
            musicSource.clip = mainMenuMusic;
            musicSource.loop = true;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }
    }
}