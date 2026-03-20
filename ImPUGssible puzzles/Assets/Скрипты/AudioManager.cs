using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Clips")]
    public AudioClip menuMusic;
    public AudioClip settingsMusic;
    public AudioClip gameMusic;

    private AudioSource musicSource;

    private const string VolumeKey = "MusicVolume";
    private const string MuteKey = "MusicMuted";

    public bool IsMuted { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Гарантируем наличие AudioSource
        musicSource = GetComponent<AudioSource>();
        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        musicSource.playOnAwake = false;

        // Загружаем громкость и mute
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 0.2f);
        IsMuted = PlayerPrefs.GetInt(MuteKey, 0) == 1;

        musicSource.volume = IsMuted ? 0f : savedVolume;
        musicSource.mute = IsMuted;

        // Воспроизводим музыку меню
        if (menuMusic != null && !musicSource.isPlaying)
            PlayMusic(menuMusic);
    }

    public void SetVolume(float value)
    {
        if (musicSource == null) return;

        PlayerPrefs.SetFloat(VolumeKey, value);

        if (!IsMuted)
            musicSource.volume = value;

        PlayerPrefs.Save();
    }

    public float GetVolume()
    {
        return PlayerPrefs.GetFloat(VolumeKey, 0.2f);
    }

    public void SetMute(bool mute)
    {
        IsMuted = mute;
        musicSource.mute = mute;

        if (mute)
            musicSource.volume = 0f;
        else
            musicSource.volume = PlayerPrefs.GetFloat(VolumeKey, 0.2f);

        PlayerPrefs.SetInt(MuteKey, mute ? 1 : 0);
        PlayerPrefs.Save();
    }

    public bool GetMute()
    {
        return IsMuted;
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;

        musicSource.clip = clip;
        musicSource.Play();

        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 0.2f);
        musicSource.volume = IsMuted ? 0f : savedVolume;
    }

    public void PlayMenuMusic() => PlayMusic(menuMusic);
    public void PlaySettingsMusic() => PlayMusic(settingsMusic);
    public void PlayGameMusic() => PlayMusic(gameMusic);
}
