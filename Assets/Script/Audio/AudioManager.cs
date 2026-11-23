using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource musicSource;
    public AudioSource ambientSource;
    public AudioSource randomAmbientSource;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float sfxVolume = 1f;

    [Range(0f, 1f)]
    public float musicVolume = 0.35f;

    [Range(0f, 1f)]
    public float ambientVolume = 0.5f;

    [Range(0f, 1f)]
    public float randomAmbientVolume = 0.7f;

    [Header("Gameplay Sound Effects")]
    public AudioClip pickupSound;
    public AudioClip ingredientDropSound;
    public AudioClip cookingDoneSound;
    public AudioClip dishTakeSound;
    public AudioClip serveSound;
    public AudioClip buttonClickSound;
    public AudioClip errorSound;

    [Header("Customer Sounds")]
    public AudioClip customerArrivalSound;
    public AudioClip customerHappySound;
    public AudioClip customerAngrySound;

    [Header("Farming Sounds")]
    public AudioClip hoeSound;
    public AudioClip plantSound;
    public AudioClip waterSound;
    public AudioClip witherSound;

    [Header("Radio Music")]
    public AudioClip sundayRadioMusic;

    [Tooltip("Radio music loops when enabled.")]
    public bool radioLoop = true;

    [Header("General Ambient")]
    public AudioClip ambientLoop;

    [Tooltip("Automatically start the ambient loop when the Playing Scene starts.")]
    public bool playAmbientOnStart = true;

    [Header("Random Provincial Ambience")]

    [Tooltip("Different rooster recordings. One will be randomly selected.")]
    public AudioClip[] roosterSounds;

    [Tooltip("Different walis tingting recordings. One will be randomly selected.")]
    public AudioClip[] walisTingtingSounds;

    [Header("Rooster Timing")]
    public bool playRooster = true;

    [Tooltip("Minimum seconds before another rooster sound.")]
    public float roosterMinDelay = 25f;

    [Tooltip("Maximum seconds before another rooster sound.")]
    public float roosterMaxDelay = 60f;

    [Header("Walis Tingting Timing")]
    public bool playWalisTingting = true;

    [Tooltip("Minimum seconds before another sweeping sound.")]
    public float walisMinDelay = 45f;

    [Tooltip("Maximum seconds before another sweeping sound.")]
    public float walisMaxDelay = 120f;

    private Coroutine roosterCoroutine;
    private Coroutine walisCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            EnsureDistinctSources();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void EnsureDistinctSources()
    {
        HashSet<AudioSource> used = new HashSet<AudioSource>();

        sfxSource = EnsureUniqueSource(sfxSource, used);
        musicSource = EnsureUniqueSource(musicSource, used);
        ambientSource = EnsureUniqueSource(ambientSource, used);
        randomAmbientSource =
            EnsureUniqueSource(randomAmbientSource, used);
    }

    AudioSource EnsureUniqueSource(
        AudioSource source,
        HashSet<AudioSource> used)
    {
        if (source != null && used.Add(source))
            return source;

        AudioSource created = gameObject.AddComponent<AudioSource>();
        created.playOnAwake = false;
        created.loop = false;
        created.spatialBlend = 0f;

        used.Add(created);
        return created;
    }

    private void Start()
    {
        StartBackgroundAudio();
    }

    private void StartBackgroundAudio()
    {

        if (playAmbientOnStart &&
            ambientLoop != null &&
            ambientSource != null)
        {
            ambientSource.clip = ambientLoop;
            ambientSource.loop = true;
            ambientSource.volume = ambientVolume;
            ambientSource.Play();
        }

        if (playRooster &&
            roosterSounds != null &&
            roosterSounds.Length > 0 &&
            randomAmbientSource != null)
        {
            roosterCoroutine = StartCoroutine(RoosterRoutine());
        }

        if (playWalisTingting &&
            walisTingtingSounds != null &&
            walisTingtingSounds.Length > 0 &&
            randomAmbientSource != null)
        {
            walisCoroutine = StartCoroutine(WalisTingtingRoutine());
        }
    }

    public void PlayRadio()
    {
        if (sundayRadioMusic == null ||
            musicSource == null)
        {
            return;
        }

        musicSource.Stop();

        musicSource.clip = sundayRadioMusic;
        musicSource.loop = radioLoop;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void StopRadio()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
            musicSource.clip = null;
        }
    }

    private IEnumerator RoosterRoutine()
    {
        while (true)
        {
            float delay = Random.Range(
                roosterMinDelay,
                roosterMaxDelay
            );

            yield return new WaitForSeconds(delay);

            if (roosterSounds == null ||
                roosterSounds.Length == 0)
            {
                continue;
            }

            AudioClip roosterClip =
                roosterSounds[
                    Random.Range(
                        0,
                        roosterSounds.Length
                    )
                ];

            if (roosterClip != null &&
                randomAmbientSource != null)
            {
                randomAmbientSource.volume =
                    randomAmbientVolume;

                randomAmbientSource.PlayOneShot(
                    roosterClip
                );
            }
        }
    }

    private IEnumerator WalisTingtingRoutine()
    {
        while (true)
        {
            float delay = Random.Range(
                walisMinDelay,
                walisMaxDelay
            );

            yield return new WaitForSeconds(delay);

            if (walisTingtingSounds == null ||
                walisTingtingSounds.Length == 0)
            {
                continue;
            }

            AudioClip walisClip =
                walisTingtingSounds[
                    Random.Range(
                        0,
                        walisTingtingSounds.Length
                    )
                ];

            if (walisClip != null &&
                randomAmbientSource != null)
            {
                randomAmbientSource.volume =
                    randomAmbientVolume;

                randomAmbientSource.PlayOneShot(
                    walisClip
                );
            }
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null &&
            sfxSource != null)
        {
            sfxSource.PlayOneShot(
                clip,
                sfxVolume
            );
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null ||
            musicSource == null)
        {
            return;
        }

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void PlayAmbient(AudioClip clip)
    {
        if (clip == null ||
            ambientSource == null)
        {
            return;
        }

        ambientSource.clip = clip;
        ambientSource.loop = true;
        ambientSource.volume = ambientVolume;
        ambientSource.Play();
    }

    public void StopAmbient()
    {
        if (ambientSource != null)
        {
            ambientSource.Stop();
        }
    }

    public void PlayRandomAmbient(AudioClip[] clips)
    {
        if (clips == null ||
            clips.Length == 0 ||
            randomAmbientSource == null)
        {
            return;
        }

        AudioClip randomClip =
            clips[
                Random.Range(
                    0,
                    clips.Length
                )
            ];

        if (randomClip != null)
        {
            randomAmbientSource.volume =
                randomAmbientVolume;

            randomAmbientSource.PlayOneShot(
                randomClip
            );
        }
    }

    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSound);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        if (roosterCoroutine != null)
        {
            StopCoroutine(roosterCoroutine);
        }

        if (walisCoroutine != null)
        {
            StopCoroutine(walisCoroutine);
        }
    }
}
