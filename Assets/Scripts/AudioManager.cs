using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public AudioSource sfxSource;
    public AudioSource musicSource;
    public float musicVolume = 0.5f;

    [Header("Layers")]
    public AudioSource asteroidLoopSource;
    public AudioSource ufoLoopSource;
    public float loopVolume = 0.4f;
    private float currentMusicVolume;

    public AudioSource thrustSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (sfxSource == null || clip == null)
            return;

        sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayMusic(AudioClip clip, float volume = 1f)
    {
        if (musicSource == null || clip == null)
            return;

        currentMusicVolume = musicVolume * volume;
        musicSource.volume = currentMusicVolume;

        // Don't play music if the same music is already playing
        if (musicSource.clip != clip)
        {
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayLayer(AudioSource source, AudioClip clip, float volume = 1f)
    {
        if (source == null || clip == null)
            return;

        source.volume = loopVolume * volume;

        if (source.isPlaying && source.clip == clip)
            return;

        source.clip = clip;
        source.loop = true;
        source.Play();
    }

    public void RestartMusic()
    {
        if (musicSource == null || musicSource.clip == null)
            return;

        musicSource.Stop();
        musicSource.time = 0f;
        musicSource.volume = currentMusicVolume;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopLayers()
    {
        StopLayer(asteroidLoopSource);
        StopLayer(ufoLoopSource);
        StopLayer(thrustSource);
    }

    public void StopLayer(AudioSource source)
    {
        if (source == null)
            return;

        source.Stop();
        source.clip = null;
    }

    public void StopMusic()
    {
        if (musicSource == null)
            return;

        musicSource.Stop();
    }

}