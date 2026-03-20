using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public class PauseMenu : MonoBehaviour
{
    [Header("Menus")]
    public CanvasGroup pauseMenu;
    public CanvasGroup optionsMenu;

    [Header("Blur")]
    public Volume blurVolume;

    [Header("Settings")]
    public Slider volumeSlider;
    public Slider brightnessSlider;
    public Slider sensitivitySlider;

    public Управлениемопсом playerController;

    [Header("Animation")]
    public float fadeSpeed = 5f;

    private bool isPaused = false;
    public GameObject gameplayUI;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume(); // Полный выход из паузы
            }
            else
            {
                Pause();
            }
        }
    }

    void OnEnable()
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveAllListeners();
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        if (brightnessSlider != null)
        {
            brightnessSlider.onValueChanged.RemoveAllListeners();
            brightnessSlider.onValueChanged.AddListener(SetBrightness);
        }

        if (sensitivitySlider != null && playerController != null)
        {
            sensitivitySlider.onValueChanged.RemoveAllListeners();
            sensitivitySlider.onValueChanged.AddListener(SetSensitivity);

            // Синхронизируем ползунок с текущей сенситивити
            sensitivitySlider.value = playerController.mouseSensitivity;
        }
    }

    public void Pause()
    {
        if (gameplayUI != null)
            gameplayUI.SetActive(false);

        if (playerController != null)
            playerController.SetUIActive(false);

        Time.timeScale = 0f;
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(FadeIn(pauseMenu));
        StartCoroutine(BlurIn());
    }

    public void Resume()
    {
        if (gameplayUI != null)
            gameplayUI.SetActive(true);

        if (playerController != null)
            playerController.SetUIActive(true);

        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        StartCoroutine(FadeOut(pauseMenu));
        StartCoroutine(FadeOut(optionsMenu));
        StartCoroutine(BlurOut());
    }

    public void OpenOptions()
    {
        StartCoroutine(SwitchMenu(pauseMenu, optionsMenu));
    }

    public void CloseOptions()
    {
        StartCoroutine(SwitchMenu(optionsMenu, pauseMenu));
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // SETTINGS
    void SetVolume(float v) => AudioListener.volume = v;

    void SetBrightness(float v) => SettingsManager.Instance.SetBrightness(v);

    void SetSensitivity(float v)
    {
        if (playerController != null)
            playerController.mouseSensitivity = v;
    }

    // ANIMATION
    System.Collections.IEnumerator FadeIn(CanvasGroup cg)
    {
        cg.gameObject.SetActive(true);

        while (cg.alpha < 1)
        {
            cg.alpha += Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }

        cg.alpha = 1;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    System.Collections.IEnumerator FadeOut(CanvasGroup cg)
    {
        cg.interactable = false;
        cg.blocksRaycasts = false;

        while (cg.alpha > 0)
        {
            cg.alpha -= Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }

        cg.alpha = 0;
        cg.gameObject.SetActive(false);
    }

    System.Collections.IEnumerator SwitchMenu(CanvasGroup from, CanvasGroup to)
    {
        yield return FadeOut(from);
        yield return FadeIn(to);
    }

    System.Collections.IEnumerator BlurIn()
    {
        if (blurVolume == null) yield break;

        while (blurVolume.weight < 1)
        {
            blurVolume.weight += Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }

        blurVolume.weight = 1;
    }

    System.Collections.IEnumerator BlurOut()
    {
        if (blurVolume == null) yield break;

        while (blurVolume.weight > 0)
        {
            blurVolume.weight -= Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }

        blurVolume.weight = 0;
    }
}