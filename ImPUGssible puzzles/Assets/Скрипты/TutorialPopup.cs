using UnityEngine;
using System.Collections;
using TMPro;

public class TutorialPopup : MonoBehaviour
{
    public GameObject panel;
    public CanvasGroup panelGroup;

    public RectTransform textTransform;
    public CanvasGroup textGroup;
    public TextMeshProUGUI textUI;

    [TextArea]
    public string fullText;

    public float delay = 1f;
    public float fadeDuration = 0.5f;
    public float typeSpeed = 0.03f;
    public float showTime = 3f;

    public float moveDistance = 30f;
    public float scaleStart = 0.5f;

    private Vector2 startPos;

    void Start()
    {
        panel.SetActive(true);

        panelGroup.alpha = 0f;
        textGroup.alpha = 0f;

        startPos = textTransform.anchoredPosition;
        textTransform.anchoredPosition = startPos;
        textTransform.localScale = Vector3.one;

        textUI.text = "";

        StartCoroutine(ShowTutorial());
    }

    IEnumerator ShowTutorial()
    {
        yield return new WaitForSeconds(delay);

        // Появление панели
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / fadeDuration);
            panelGroup.alpha = p;
            yield return null;
        }

        // Появление текста по буквам
        yield return StartCoroutine(TypeTextAnimated());

        // Задержка перед скрытием
        yield return new WaitForSeconds(showTime);

        // Исчезновение панели
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / fadeDuration);
            panelGroup.alpha = 1f - p;
            textGroup.alpha = 1f - p;
            yield return null;
        }

        panel.SetActive(false);
    }

    IEnumerator TypeTextAnimated()
    {
        textUI.text = "";

        for (int i = 0; i < fullText.Length; i++)
        {
            char c = fullText[i];
            textUI.text += c;

            // Анимация последней буквы
            RectTransform charTransform = textTransform; // текст целиком анимируется
            Vector2 startPosChar = startPos - new Vector2(0, moveDistance);
            Vector3 startScale = Vector3.one * scaleStart;
            Vector3 endScale = Vector3.one;

            float t = 0f;
            while (t < typeSpeed)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / typeSpeed);

                textTransform.anchoredPosition = Vector2.Lerp(startPosChar, startPos, p);
                textTransform.localScale = Vector3.Lerp(startScale, endScale, p);

                yield return null;
            }
        }
    }

    IEnumerator TypeTextAnimated()
    {
        textUI.text = "";

        textGroup.alpha = 1f; // сразу делаем видимым текст

        for (int i = 0; i < fullText.Length; i++)
        {
            char c = fullText[i];
            textUI.text += c;

            // Анимация последней буквы (весь текст анимируется как блок)
            Vector2 startPosChar = startPos - new Vector2(0, moveDistance);
            Vector3 startScale = Vector3.one * scaleStart;
            Vector3 endScale = Vector3.one;

            float t = 0f;
            while (t < typeSpeed)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / typeSpeed);

                textTransform.anchoredPosition = Vector2.Lerp(startPosChar, startPos, p);
                textTransform.localScale = Vector3.Lerp(startScale, endScale, p);

                yield return null;
            }
        }
    }
}