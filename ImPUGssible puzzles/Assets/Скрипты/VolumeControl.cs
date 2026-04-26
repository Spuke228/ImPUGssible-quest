using UnityEngine;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;

    private void Start()
    {
        if (SettingsManager.Instance == null) return;

        volumeSlider.onValueChanged.RemoveAllListeners();
        volumeSlider.value = SettingsManager.Instance.GetMusicVolume();
        volumeSlider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnSliderChanged(float value)
    {
        if (SettingsManager.Instance == null) return;

        SettingsManager.Instance.SetMusicVolume(value);
    }
}