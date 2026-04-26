using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;

public class UISoundManager : MonoBehaviour
{
    [Header("UI звуки")]
    public AudioClip clickSound;

    [Header("Музыка")]
    public AudioClip menuMusic;
    public AudioClip gameMusic;

    public float musicFadeDuration = 1f;

    [Header("Scene Names")]
    public string menuSceneName = "MainMenu";
    public string gameSceneName = "Game"; // твоя игровая сцена

    private AudioSource uiAudioSource;
    private AudioSource musicAudioSource;
    private bool isMuted = false;
    private float currentVolume = 0.2f;
    private float targetVolume = 0.2f;

    public static UISoundManager Instance;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        uiAudioSource = gameObject.AddComponent<AudioSource>();
        uiAudioSource.loop = false;
        uiAudioSource.playOnAwake = false;

        musicAudioSource = gameObject.AddComponent<AudioSource>();
        musicAudioSource.loop = true;
        musicAudioSource.playOnAwake = false;
    }

    public void SetMuted(bool muted)
    {
        isMuted = muted;

        if (musicAudioSource != null)
            musicAudioSource.mute = muted;

        if (uiAudioSource != null)
            uiAudioSource.mute = muted;
    }

    public bool IsMuted() => isMuted;

    public float GetCurrentVolume()
    {
        return currentVolume;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        StartCoroutine(InitMusic());
    }

    IEnumerator InitMusic()
    {
        while (SettingsManager.Instance == null)
            yield return null;

        currentVolume = SettingsManager.Instance.GetMusicVolume();

        HandleSceneMusic(SceneManager.GetActiveScene().name);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HandleSceneMusic(scene.name);
    }

    void HandleSceneMusic(string sceneName)
    {
        // МЕНЮ И НАСТРОЙКИ (та же музыка)
        if (sceneName == menuSceneName || sceneName == "Settings")
        {
            StartCoroutine(SwitchMusic(menuMusic));
        }
        // ИГРА
        else if (sceneName == gameSceneName)
        {
            StartCoroutine(SwitchMusic(gameMusic));
        }
    }

    // ---------------- UI ----------------

    public void PlayClickSound()
    {
        if (clickSound != null)
            uiAudioSource.PlayOneShot(clickSound);
    }

    public void RegisterSlider(Slider slider)
    {
        if (slider == null) return;

        EventTrigger trigger = slider.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = slider.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerDown
        };

        entry.callback.AddListener((data) => { PlayClickSound(); });
        trigger.triggers.Add(entry);
    }

    // ---------------- Музыка ----------------

    IEnumerator SwitchMusic(AudioClip newMusic)
    {
        if (newMusic == null)
            yield break;

        if (musicAudioSource.clip == newMusic && musicAudioSource.isPlaying)
            yield break;

        float startVolume = musicAudioSource.volume;

        // FADE OUT
        if (musicAudioSource.isPlaying)
        {
            float t = 0f;

            while (t < musicFadeDuration)
            {
                t += Time.unscaledDeltaTime;

                float v = Mathf.Lerp(startVolume, 0f, t / musicFadeDuration);
                musicAudioSource.volume = isMuted ? 0f : v;

                yield return null;
            }
        }

        musicAudioSource.clip = newMusic;
        musicAudioSource.Play();

        // FADE IN (ВАЖНО: используем targetVolume, не currentVolume напрямую)
        float t2 = 0f;

        while (t2 < musicFadeDuration)
        {
            t2 += Time.unscaledDeltaTime;

            float v = Mathf.Lerp(0f, targetVolume, t2 / musicFadeDuration);
            musicAudioSource.volume = isMuted ? 0f : v;

            yield return null;
        }

        musicAudioSource.volume = isMuted ? 0f : targetVolume;
    }

    public void SetMusicVolume(float volume)
    {
        targetVolume = volume;
        currentVolume = volume;

        if (isMuted || musicAudioSource == null)
            return;

        musicAudioSource.volume = volume;
    }

    public void SetMusicVolumeSlider(float volume)
    {
        SetMusicVolume(volume);
    }
    public void SetVolume(float volume)
    {
        SetMusicVolume(volume);
    }
}