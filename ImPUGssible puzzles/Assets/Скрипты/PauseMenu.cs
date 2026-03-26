using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class PauseMenu : MonoBehaviour
{
    public Image brightnessOverlay; // белый Image поверх всего экрана, Canvas = Overlay
    public GameObject pausePanel;
    public GameObject settingsPanel;

    public Canvas gameplayUI;

    public RawImage videoBackground;
    public VideoClip backgroundVideo;

    public Slider sensivitySlider;
    public Slider brightnessSlider;
    public Slider volumeSlider;

    public Button continueButton;
    public Button settingsButton;
    public Button backButton;
    public Button quitButton;

    public float buttonScale = 1.1f;

    public UISoundManager soundManager; // основной звук сцены

    bool isPaused;

    VideoPlayer videoPlayer;
    RenderTexture videoTexture;

    Dictionary<Transform, Vector3> originalScales = new();
    Dictionary<Transform, Coroutine> scaleCoroutines = new();

    void Start()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);

        if (videoBackground != null)
            videoBackground.gameObject.SetActive(false);

        SetupButton(continueButton);
        SetupButton(settingsButton);
        SetupButton(backButton);
        SetupButton(quitButton);

        continueButton.onClick.AddListener(ResumeGame);
        settingsButton.onClick.AddListener(OpenSettings);
        backButton.onClick.AddListener(CloseSettings);
        quitButton.onClick.AddListener(QuitToMenu);

        // --- слайдеры ---
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveAllListeners();
            volumeSlider.onValueChanged.AddListener(OnVolumeSliderChanged);
        }

        if (sensivitySlider != null)
        {
            sensivitySlider.onValueChanged.RemoveAllListeners();
            sensivitySlider.onValueChanged.AddListener(OnSensivitySliderChanged);
        }

        if (brightnessSlider != null)
        {
            brightnessSlider.onValueChanged.RemoveAllListeners();
            brightnessSlider.onValueChanged.AddListener(OnBrightnessSliderChanged);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !TutorialSyste.TutorialActive)
        {
            if (!isPaused) PauseGame();
            else ResumeGame();
        }
    }

    void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0;

        // музыка на паузе = 0
        if (soundManager != null)
            soundManager.SetMusicVolumeSlider(0f);

        // создаём VideoPlayer только при паузе
        if (videoPlayer == null && videoBackground != null && backgroundVideo != null)
        {
            videoTexture = new RenderTexture(1920, 1080, 0);
            videoBackground.texture = videoTexture;

            videoPlayer = gameObject.AddComponent<VideoPlayer>();
            videoPlayer.clip = backgroundVideo;
            videoPlayer.isLooping = true;
            videoPlayer.playOnAwake = false;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = videoTexture;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            videoPlayer.SetDirectAudioVolume(0, 0.1f);

            videoBackground.gameObject.SetActive(true);
            videoPlayer.Play();
        }
        else if (videoPlayer != null)
        {
            videoBackground.gameObject.SetActive(true);
            videoPlayer.Play();
        }

        if (gameplayUI)
            gameplayUI.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        pausePanel.SetActive(true);
    }

    void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1;

        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);

        // возвращаем громкость основной музыки
        if (soundManager != null && volumeSlider != null)
            soundManager.SetMusicVolumeSlider(volumeSlider.value);

        if (videoPlayer != null)
        {
            videoPlayer.Pause();
            videoBackground.gameObject.SetActive(false);
        }

        if (gameplayUI)
            gameplayUI.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OpenSettings()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    void CloseSettings()
    {
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    void QuitToMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }

    void SetupButton(Button btn)
    {
        Transform t = btn.transform;
        originalScales[t] = t.localScale;

        EventTrigger trigger = btn.gameObject.AddComponent<EventTrigger>();

        // Наведение
        AddEvent(trigger, EventTriggerType.PointerEnter, () => Scale(t, originalScales[t] * buttonScale));
        AddEvent(trigger, EventTriggerType.PointerExit, () => Scale(t, originalScales[t]));

        // Сброс после нажатия
        btn.onClick.AddListener(() => Scale(t, originalScales[t]));
    }

    void AddEvent(EventTrigger trigger, EventTriggerType type, System.Action action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = type;
        entry.callback.AddListener((e) => action());
        trigger.triggers.Add(entry);
    }

    void Scale(Transform t, Vector3 target)
    {
        if (scaleCoroutines.ContainsKey(t) && scaleCoroutines[t] != null)
            StopCoroutine(scaleCoroutines[t]);

        scaleCoroutines[t] = StartCoroutine(ScaleRoutine(t, target));
    }

    IEnumerator ScaleRoutine(Transform t, Vector3 target)
    {
        Vector3 start = t.localScale;
        float time = 0;

        while (time < 0.12f)
        {
            time += Time.unscaledDeltaTime;
            t.localScale = Vector3.Lerp(start, target, time / 0.12f);
            yield return null;
        }

        t.localScale = target;
    }

    // --- слайдеры ---
    void OnVolumeSliderChanged(float value)
    {
        if (soundManager != null)
            soundManager.SetMusicVolumeSlider(value);
    }

    void OnSensivitySliderChanged(float value)
    {
        Управлениемопсом playerController = FindObjectOfType<Управлениемопсом>();
        if (playerController != null)
            playerController.mouseSensitivity = value;
    }

    void OnBrightnessSliderChanged(float value)
    {
        // value 0..1 → 0 = полностью темно, 1 = полностью ярко
        value = Mathf.Clamp01(value);

        if (brightnessOverlay != null)
        {
            // переворачиваем: макс. яркость = прозрачный, мин. яркость = полностью черный
            float alpha = 1f - value;
            brightnessOverlay.color = new Color(0f, 0f, 0f, alpha);
            brightnessOverlay.gameObject.SetActive(alpha > 0f);
        }
    }
}