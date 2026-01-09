using UnityEngine;

[DisallowMultipleComponent]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Clips")]
    public AudioClip musicBackground;
    public AudioClip sfxShoot;
    public AudioClip sfxEnemyDeath;

    [Header("Volumes")]
    [Range(0f, 1f)] public float musicVolume = 0.6f;
    [Range(0f, 1f)] public float sfxVolume = 1.0f;

    [Tooltip("Окремий множник гучності ТІЛЬКИ для пострілу (0..1).")]
    [Range(0f, 1f)] public float shootVolume = 0.35f;

    [Tooltip("Окремий множник гучності для звуку вбивства ворога (0..1).")]
    [Range(0f, 1f)] public float enemyDeathVolume = 0.85f;

    [Header("Behaviour")]
    public bool playMusicOnStart = true;
    public bool dontDestroyOnLoad = true;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    private bool isMuted = false;
    private bool musicWasPlayingBeforeMute = false;

    private const string PREF_MUTED = "audio_muted";

    public bool IsMuted => isMuted;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);

        EnsureSources();

        // Відновлення стану mute
        isMuted = PlayerPrefs.GetInt(PREF_MUTED, 0) == 1;

        ApplyVolumes();

        if (musicBackground != null)
        {
            musicSource.clip = musicBackground;
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        ApplyMuteState(initial: true);
    }

    void Start()
    {
        if (playMusicOnStart && !isMuted)
            PlayMusic();
    }

    void OnValidate()
    {
        if (!Application.isPlaying) return;
        ApplyVolumes();
        ApplyMuteState(initial: false);
    }

    private void EnsureSources()
    {
        var sources = GetComponents<AudioSource>();

        if (sources.Length == 0)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
        else if (sources.Length == 1)
        {
            musicSource = sources[0];
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            musicSource = sources[0];
            sfxSource = sources[1];
        }

        // Базові налаштування (2D)
        musicSource.spatialBlend = 0f;
        sfxSource.spatialBlend = 0f;

        musicSource.loop = true;
        musicSource.playOnAwake = false;

        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
    }

    private void ApplyVolumes()
    {
        if (musicSource != null) musicSource.volume = musicVolume;
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }

    private void ApplyMuteState(bool initial)
    {
        if (musicSource != null) musicSource.mute = isMuted;
        if (sfxSource != null) sfxSource.mute = isMuted;

        if (musicSource == null) return;

        if (isMuted)
        {
            // Пауза музики, щоб при розм’юті продовжилася з того ж місця
            if (musicSource.isPlaying)
            {
                musicWasPlayingBeforeMute = true;
                musicSource.Pause();
            }
        }
        else
        {
            // Якщо це не перша ініціалізація і музика грала до mute — відновити
            if (!initial && musicWasPlayingBeforeMute)
            {
                musicWasPlayingBeforeMute = false;
                musicSource.UnPause();
            }
        }
    }

    public void SetMuted(bool muted)
    {
        if (isMuted == muted) return;

        isMuted = muted;
        PlayerPrefs.SetInt(PREF_MUTED, isMuted ? 1 : 0);
        PlayerPrefs.Save();

        ApplyMuteState(initial: false);

        // Якщо зняли mute і треба музика — запустити (коли її ще не було)
        if (!isMuted && playMusicOnStart)
        {
            if (musicSource != null && !musicSource.isPlaying)
                PlayMusic();
        }
    }

    public void ToggleMuted()
    {
        SetMuted(!isMuted);
    }

    public void PlayMusic()
    {
        if (musicSource == null) return;
        if (musicBackground == null) return;

        if (musicSource.clip != musicBackground)
            musicSource.clip = musicBackground;

        ApplyVolumes();

        if (isMuted) return;

        if (!musicSource.isPlaying)
            musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource == null) return;
        musicSource.Stop();
    }

    public void PlayShoot()
    {
        if (sfxSource == null) return;
        if (sfxShoot == null) return;
        if (isMuted) return;

        sfxSource.PlayOneShot(sfxShoot, shootVolume);
    }

    public void PlayEnemyDeath()
    {
        if (sfxSource == null) return;
        if (sfxEnemyDeath == null) return;
        if (isMuted) return;

        sfxSource.PlayOneShot(sfxEnemyDeath, enemyDeathVolume);
    }
}
