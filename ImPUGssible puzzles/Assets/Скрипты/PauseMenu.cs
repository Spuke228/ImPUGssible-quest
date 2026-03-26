using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

public class PauseMenu : MonoBehaviour
{
    [Header("Панели")]
    public GameObject pausePanel;
    public GameObject settingsPanel;

    [Header("Игровой UI")]
    public Canvas gameplayUI;

    [Header("Фон")]
    public Image darkBackground;

    [Header("Динамический свет UI")]
    public RectTransform uiLight;
    public float lightSmooth = 8f;

    [Header("Лого")]
    public Image gameLogo;
    public Vector2 logoPosition = new Vector2(30, -30);

    [Header("Слайдеры")]
    public Slider sensivitySlider;
    public Slider brightnessSlider;
    public Slider volumeSlider;

    [Header("Кнопки")]
    public Button continueButton;
    public Button settingsButton;
    public Button backButton;
    public Button quitButton;

    [Header("Анимация")]
    public float fadeDuration = 0.25f;
    public float buttonScale = 1.1f;

    [Header("Цветовая схема")]
    public Color buttonNormal = new Color(0.18f, 0.23f, 0.35f);
    public Color buttonHover = new Color(0.95f, 0.78f, 0.25f);
    public Color buttonPressed = new Color(0.75f, 0.75f, 0.75f);

    private bool isPaused = false;

    private CanvasGroup pauseCG;
    private CanvasGroup settingsCG;

    private Управлениемопсом pug;

    void Awake()
    {
        pauseCG = GetCanvasGroup(pausePanel);
        settingsCG = GetCanvasGroup(settingsPanel);
    }

    void Start()
    {
        if (uiLight != null)
        {
            Image img = uiLight.GetComponent<Image>();
            img.raycastTarget = false;
        }

        pug = FindObjectOfType<Управлениемопсом>();

        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);

        if (darkBackground != null)
        {
            Color c = darkBackground.color;
            c.a = 0;
            darkBackground.color = c;
        }

        continueButton.onClick.AddListener(ResumeGame);
        settingsButton.onClick.AddListener(OpenSettings);
        backButton.onClick.AddListener(CloseSettings);
        quitButton.onClick.AddListener(QuitToMenu);

        sensivitySlider.onValueChanged.AddListener(SetSensitivity);
        brightnessSlider.onValueChanged.AddListener(SetBrightness);
        volumeSlider.onValueChanged.AddListener(SetVolume);

        if (pug != null)
            sensivitySlider.value = pug.mouseSensitivity;

        brightnessSlider.value = RenderSettings.ambientIntensity;
        volumeSlider.value = AudioListener.volume;

        SetupButton(continueButton);
        SetupButton(settingsButton);
        SetupButton(backButton);
        SetupButton(quitButton);

        SetupLogo();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused) PauseGame();
            else ResumeGame();
        }

        UpdateUILight();
    }

    void SetupLogo()
    {
        if (gameLogo == null) return;

        RectTransform rt = gameLogo.rectTransform;

        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = logoPosition;
    }

    void UpdateUILight()
    {
        if (uiLight == null) return;
        if (!isPaused) return;

        Vector2 pos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            uiLight.parent as RectTransform,
            Input.mousePosition,
            null,
            out pos
        );

        uiLight.anchoredPosition = Vector2.Lerp(
            uiLight.anchoredPosition,
            pos,
            Time.unscaledDeltaTime * lightSmooth
        );
    }

    CanvasGroup GetCanvasGroup(GameObject obj)
    {
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();
        return cg;
    }

    void PauseGame()
    {
        isPaused = true;

        Time.timeScale = 0f;

        if (gameplayUI != null)
            gameplayUI.enabled = false;

        if (pug != null)
            pug.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        pausePanel.SetActive(true);
        settingsPanel.SetActive(false);

        StartCoroutine(FadeBackground(0, 0.6f));
        StartCoroutine(Fade(pauseCG, 0, 1));
    }

    void ResumeGame()
    {
        StartCoroutine(ResumeRoutine());
    }

    IEnumerator ResumeRoutine()
    {
        yield return Fade(pauseCG, 1, 0);
        yield return FadeBackground(0.6f, 0);

        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);

        if (gameplayUI != null)
            gameplayUI.enabled = true;

        if (pug != null)
            pug.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;

        isPaused = false;
    }

    void OpenSettings()
    {
        StartCoroutine(OpenSettingsRoutine());
    }

    IEnumerator OpenSettingsRoutine()
    {
        yield return Fade(pauseCG, 1, 0);

        pausePanel.SetActive(false);

        settingsPanel.SetActive(true);
        yield return Fade(settingsCG, 0, 1);
    }

    void CloseSettings()
    {
        StartCoroutine(CloseSettingsRoutine());
    }

    IEnumerator CloseSettingsRoutine()
    {
        yield return Fade(settingsCG, 1, 0);

        settingsPanel.SetActive(false);

        pausePanel.SetActive(true);
        yield return Fade(pauseCG, 0, 1);
    }

    IEnumerator Fade(CanvasGroup cg, float start, float end)
    {
        float t = 0;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(start, end, t / fadeDuration);
            yield return null;
        }

        cg.alpha = end;
    }

    IEnumerator FadeBackground(float start, float end)
    {
        if (darkBackground == null) yield break;

        float t = 0;
        Color c = darkBackground.color;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(start, end, t / fadeDuration);
            darkBackground.color = c;
            yield return null;
        }
    }

    void QuitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    void SetSensitivity(float v)
    {
        if (pug != null)
            pug.mouseSensitivity = v;
    }

    void SetBrightness(float v)
    {
        RenderSettings.ambientIntensity = v;
    }

    void SetVolume(float v)
    {
        AudioListener.volume = v;
    }

    void SetupButton(Button btn)
    {
        ColorBlock c = btn.colors;

        c.normalColor = buttonNormal;
        c.highlightedColor = buttonHover;
        c.pressedColor = buttonPressed;
        c.selectedColor = buttonNormal;

        btn.colors = c;

        Vector3 originalScale = btn.transform.localScale;

        EventTrigger trigger = btn.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry enter = new EventTrigger.Entry();
        enter.eventID = EventTriggerType.PointerEnter;

        enter.callback.AddListener((e) =>
        {
            StopCoroutine("ScaleRoutine");
            StartCoroutine(ScaleRoutine(btn.transform, originalScale, originalScale * buttonScale));
        });

        trigger.triggers.Add(enter);

        EventTrigger.Entry exit = new EventTrigger.Entry();
        exit.eventID = EventTriggerType.PointerExit;

        exit.callback.AddListener((e) =>
        {
            StopCoroutine("ScaleRoutine");
            StartCoroutine(ScaleRoutine(btn.transform, btn.transform.localScale, originalScale));
        });

        trigger.triggers.Add(exit);
    }

    IEnumerator ScaleRoutine(Transform t, Vector3 start, Vector3 end)
    {
        float time = 0;

        while (time < 0.15f)
        {
            time += Time.unscaledDeltaTime;
            t.localScale = Vector3.Lerp(start, end, time / 0.15f);
            yield return null;
        }

        t.localScale = end;
    }
}