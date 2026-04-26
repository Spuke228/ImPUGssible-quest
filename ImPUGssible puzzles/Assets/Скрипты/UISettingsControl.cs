using UnityEngine;
using UnityEngine.UI;

public class UISettingsControl : MonoBehaviour
{
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private Toggle dogToggle;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle muteToggle;

    private void OnEnable()
    {
        // Подписка на событие менеджера
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnSettingsUpdated -= RefreshUI; // на всякий случай
            SettingsManager.Instance.OnSettingsUpdated += RefreshUI;
            RefreshUI(); // подтягиваем актуальные значения сразу
        }

        AttachUIEvents();
    }

    private void OnDisable()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.OnSettingsUpdated -= RefreshUI;
    }

    private void AttachUIEvents()
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveAllListeners();
            volumeSlider.value = SettingsManager.Instance.GetMusicVolume();
            volumeSlider.onValueChanged.AddListener(v => SettingsManager.Instance.SetMusicVolume(v));
        }
        if (SettingsManager.Instance == null) return;

        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.RemoveAllListeners();
            fullscreenToggle.isOn = SettingsManager.Instance.IsFullscreen();
            fullscreenToggle.onValueChanged.AddListener(v => SettingsManager.Instance.SetFullscreen(v));
        }

        if (brightnessSlider != null)
        {
            brightnessSlider.onValueChanged.RemoveAllListeners();
            brightnessSlider.value = SettingsManager.Instance.GetBrightness();
            brightnessSlider.onValueChanged.AddListener(v => SettingsManager.Instance.SetBrightness(v));
        }

        if (dogToggle != null)
        {
            dogToggle.onValueChanged.RemoveAllListeners();
            dogToggle.isOn = SettingsManager.Instance.IsDogShown();
            dogToggle.onValueChanged.AddListener(v => SettingsManager.Instance.SetShowDog(v));
        }
        if (muteToggle != null)
        {
            muteToggle.onValueChanged.RemoveAllListeners();
            muteToggle.isOn = SettingsManager.Instance.IsMuted();
            muteToggle.onValueChanged.AddListener(v => SettingsManager.Instance.SetMuted(v));
        }
    }

    private void RefreshUI()
    {
        if (muteToggle != null)
            muteToggle.isOn = SettingsManager.Instance.IsMuted();
        if (volumeSlider != null)
            volumeSlider.value = SettingsManager.Instance.GetMusicVolume();
        if (SettingsManager.Instance == null) return;

        if (fullscreenToggle != null)
            fullscreenToggle.isOn = SettingsManager.Instance.IsFullscreen();

        if (brightnessSlider != null)
            brightnessSlider.value = SettingsManager.Instance.GetBrightness();

        if (dogToggle != null)
            dogToggle.isOn = SettingsManager.Instance.IsDogShown();
    }
}
