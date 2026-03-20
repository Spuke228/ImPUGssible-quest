using UnityEngine;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle muteToggle;

    private void Start()
    {
        Invoke(nameof(InitializeUI), 0.1f);
    }

    private void InitializeUI()
    {
        if (AudioManager.Instance == null) return;

        // Настройка слайдера громкости
        if (volumeSlider != null)
        {
            float currentVolume = AudioManager.Instance.GetVolume();
            volumeSlider.value = currentVolume;
            volumeSlider.onValueChanged.RemoveAllListeners();
            volumeSlider.onValueChanged.AddListener(OnSliderChanged);
        }

        // Настройка mute-тумблера
        if (muteToggle != null)
        {
            bool isMuted = AudioManager.Instance.GetMute();
            muteToggle.isOn = isMuted;
            muteToggle.onValueChanged.RemoveAllListeners();
            muteToggle.onValueChanged.AddListener(OnMuteChanged);
        }

        UpdateSliderState();
    }

    private void OnSliderChanged(float value)
    {
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.SetVolume(value);
    }

    private void OnMuteChanged(bool mute)
    {
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.SetMute(mute);
        UpdateSliderState();
    }

    private void UpdateSliderState()
    {
        if (volumeSlider != null)
            volumeSlider.interactable = !AudioManager.Instance.GetMute();
    }
}
