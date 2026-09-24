using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource;

    [Header("Sound Effects")]
    public AudioClip pickupSound;
    public AudioClip ingredientDropSound;
    public AudioClip cookingDoneSound;
    public AudioClip dishTakeSound;
    public AudioClip serveSound;
    public AudioClip buttonClickSound;
    public AudioClip errorSound;

    [Header("Customer Sounds")]
    public AudioClip customerHappySound;
    public AudioClip customerAngrySound;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}