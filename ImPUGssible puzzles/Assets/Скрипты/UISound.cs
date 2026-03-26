using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;

public class UISoundManager : MonoBehaviour
{
    [Header("UI звуки")]
    public AudioClip clickSound;

    [Header("Музыка сцены")]
    public AudioClip sceneMusic;
    public float musicFadeDuration = 1f;

    private AudioSource uiAudioSource;
    private AudioSource musicAudioSource;

    private void Awake()
    {
        uiAudioSource = gameObject.AddComponent<AudioSource>();
        uiAudioSource.playOnAwake = false;

        musicAudioSource = gameObject.AddComponent<AudioSource>();
        musicAudioSource.loop = true;
        musicAudioSource.playOnAwake = false;
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(SwitchMusic(sceneMusic));
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(SwitchMusic(sceneMusic));
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

        EventTrigger trigger = slider.gameObject.GetComponent<EventTrigger>();
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

        if (musicAudioSource.isPlaying)
        {
            float startVolume = musicAudioSource.volume;
            float t = 0f;

            while (t < musicFadeDuration)
            {
                t += Time.unscaledDeltaTime;
                musicAudioSource.volume = Mathf.Lerp(startVolume, 0f, t / musicFadeDuration);
                yield return null;
            }

            musicAudioSource.Stop();
        }

        musicAudioSource.clip = newMusic;
        musicAudioSource.volume = 0f;
        musicAudioSource.Play();

        float t2 = 0f;

        while (t2 < musicFadeDuration)
        {
            t2 += Time.unscaledDeltaTime;
            musicAudioSource.volume = Mathf.Lerp(0f, 0.2f, t2 / musicFadeDuration);
            yield return null;
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (musicAudioSource != null)
            musicAudioSource.volume = volume;
    }

    public void SetMusicVolumeSlider(float volume)
    {
        if (musicAudioSource != null)
            musicAudioSource.volume = volume;
    }
}