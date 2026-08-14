using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Аудио")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)]
    public float volume = 0.5f;

    private AudioSource audioSource;
    private bool isMuted = false;

    private const string MUTE_KEY = "MusicMuted";

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
            return;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = backgroundMusic;
        audioSource.loop = true;
        audioSource.volume = volume;
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        // Загружаем сохранённое состояние
        int savedMute = PlayerPrefs.GetInt(MUTE_KEY, 0);
        SetMute(savedMute == 1);
        PlayMusic();
    }

    public void PlayMusic()
    {
        if (audioSource != null && backgroundMusic != null && !audioSource.isPlaying && !isMuted)
        {
            audioSource.Play();
        }
    }

    public void StopMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    public void SetMute(bool mute)
    {
        isMuted = mute;
        if (audioSource != null)
        {
            audioSource.mute = mute;
            if (mute && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            else if (!mute && !audioSource.isPlaying && backgroundMusic != null)
            {
                audioSource.Play();
            }
        }
        PlayerPrefs.SetInt(MUTE_KEY, mute ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleMute()
    {
        SetMute(!isMuted);
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
            audioSource.volume = volume;
    }

    public bool IsPlaying => audioSource != null && audioSource.isPlaying;
}