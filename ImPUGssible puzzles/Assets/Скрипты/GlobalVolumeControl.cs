using UnityEngine;
using UnityEngine.UI;

public class GlobalVolumeControl : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;

    private void Start()
    {
        DontDestroyOnLoad(gameObject); // Делаем сам слайдер неуничтожаемым
        InitializeSlider();
    }

    private void InitializeSlider()
    {
        if (volumeSlider == null) return;

        if (AudioManager.Instance == null)
        {
            Invoke(nameof(InitializeSlider), 0.1f);
            return;
        }

        float currentVolume = AudioManager.Instance.GetVolume();
        volumeSlider.SetValueWithoutNotify(currentVolume);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    private void OnVolumeChanged(float value)
    {
        AudioManager.Instance?.SetVolume(value);
    }
}