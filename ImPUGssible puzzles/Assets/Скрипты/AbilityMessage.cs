using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class TriggerText : MonoBehaviour
{
    public TextMeshProUGUI text;
    public CanvasGroup canvasGroup;
    public string message = "Текст сообщения";
    public float showTime = 3f;

    private bool triggered;

    private void Awake()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        text.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        StartCoroutine(ShowText());
    }

    private IEnumerator ShowText()
    {
        text.text = message;
        text.enabled = true;

        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(showTime);

        canvasGroup.alpha = 0f;
        text.enabled = false;
    }
}