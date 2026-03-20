using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UISoundManager : MonoBehaviour
{
    public AudioClip clickSound;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    // Привязать к OnClick() всех кнопок
    public void PlayClickSound()
    {
        if (clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }

    // Для слайдера: подключаем динамически
    public void RegisterSlider(Slider slider)
    {
        if (slider == null) return;

        // Воспроизводим звук при начале перетаскивания
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
}
