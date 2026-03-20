using UnityEngine;
using UnityEngine.UI;

public class UISettingsControl : MonoBehaviour
{
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private Toggle dogToggle;

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
    }

    private void RefreshUI()
    {
        if (SettingsManager.Instance == null) return;

        if (fullscreenToggle != null)
            fullscreenToggle.isOn = SettingsManager.Instance.IsFullscreen();

        if (brightnessSlider != null)
            brightnessSlider.value = SettingsManager.Instance.GetBrightness();

        if (dogToggle != null)
            dogToggle.isOn = SettingsManager.Instance.IsDogShown();
    }
}
