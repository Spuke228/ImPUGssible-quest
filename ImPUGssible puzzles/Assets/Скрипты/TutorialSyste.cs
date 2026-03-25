using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class TutorialSyste : MonoBehaviour
{
    public CanvasGroup panel;
    public Image panelImage;

    public TextMeshProUGUI textUI;
    public TextMeshProUGUI pressAnyKeyText;

    public Transform controlsContainer;
    public GameObject controlIconPrefab;

    public MonoBehaviour playerController;

    public float letterSpeed = 0.03f;

    [Header("Key Icons")]
    public Sprite keyW;
    public Sprite keyA;
    public Sprite keyS;
    public Sprite keyD;
    public Sprite keyE;

    [Header("Mouse Icons")]
    public Sprite mouseMoveHorizontal;
    public Sprite mouseMoveVertical;
    public Sprite mouseWheel;

    bool waitingForKey = false;

    void Start()
    {
        panel.alpha = 0;
        panel.interactable = false;
        panel.blocksRaycasts = false;

        if (panelImage != null)
            panelImage.color = new Color32(20, 25, 40, 220);

        textUI.color = new Color32(144, 165, 255, 255);
        pressAnyKeyText.color = new Color32(255, 210, 120, 255);

        pressAnyKeyText.gameObject.SetActive(false);

        LockPlayer();

        ShowTutorial(
        "Бедный мопс остался один дома и проголодался.\nПомогите ему добраться до корма.\nЧтобы это сделать, нужно пройти головоломки\nи решить математические задачи.\n\nУправление:",
        new KeyCode[]
        {
            KeyCode.W,
            KeyCode.A,
            KeyCode.S,
            KeyCode.D,
            KeyCode.E,
            KeyCode.Mouse0,
            KeyCode.Mouse1,
            KeyCode.Mouse2
        });
    }

    void Update()
    {
        if (waitingForKey && Input.anyKeyDown)
        {
            waitingForKey = false;
            StopAllCoroutines();
            StartCoroutine(Hide());
        }
    }

    void LockPlayer()
    {
        if (playerController != null)
            playerController.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void UnlockPlayer()
    {
        if (playerController != null)
            playerController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ShowTutorial(string text, KeyCode[] keys)
    {
        StartCoroutine(TutorialRoutine(text, keys));
    }

    IEnumerator TutorialRoutine(string text, KeyCode[] keys)
    {
        panel.interactable = true;
        panel.blocksRaycasts = true;

        yield return Fade(0, 1, 0.5f);

        yield return TypeText(text);

        SpawnIcons(keys);

        pressAnyKeyText.gameObject.SetActive(true);
        StartCoroutine(PulseText());

        waitingForKey = true;
    }

    void SpawnIcons(KeyCode[] keys)
    {
        foreach (Transform c in controlsContainer)
            Destroy(c.gameObject);

        GameObject mouseText = null;

        for (int i = 0; i < keys.Length; i++)
        {
            KeyCode key = keys[i];

            GameObject icon = Instantiate(controlIconPrefab, controlsContainer);

            RectTransform rt = icon.GetComponent<RectTransform>();
            Image img = icon.GetComponent<Image>();
            TextMeshProUGUI txt = icon.GetComponentInChildren<TextMeshProUGUI>();

            Vector2 pos = Vector2.zero;
            string description = "";

            switch (key)
            {
                case KeyCode.W:
                    img.sprite = keyW;
                    pos = new Vector2(-300, 0);
                    description = "Идти вперёд";
                    break;

                case KeyCode.S:
                    img.sprite = keyS;
                    pos = new Vector2(-300, -200);
                    description = "Идти назад";
                    break;

                case KeyCode.A:
                    img.sprite = keyA;
                    pos = new Vector2(-600, -200);
                    description = "Идти влево";
                    break;

                case KeyCode.D:
                    img.sprite = keyD;
                    pos = new Vector2(0, -200);
                    description = "Идти вправо";
                    break;

                case KeyCode.E:
                    img.sprite = keyE;
                    pos = new Vector2(0, 0);
                    description = "Взаимодействовать";
                    break;

                case KeyCode.Mouse0:
                    img.sprite = mouseMoveHorizontal;
                    pos = new Vector2(500, 0); // поменял на уникальную позицию
                    if (txt != null)
                        txt.gameObject.SetActive(false);
                    StartCoroutine(MouseLeftRight(icon, pos));
                    break;

                case KeyCode.Mouse1:
                    img.sprite = mouseMoveVertical;
                    pos = new Vector2(300, 0); // другая уникальная позиция
                    if (txt != null)
                        txt.gameObject.SetActive(false);
                    StartCoroutine(MouseUpDown(icon, pos));

                    if (mouseText == null)
                    {
                        GameObject textObj = Instantiate(controlIconPrefab, controlsContainer);
                        RectTransform tr = textObj.GetComponent<RectTransform>();
                        Image im = textObj.GetComponent<Image>();
                        TextMeshProUGUI t = textObj.GetComponentInChildren<TextMeshProUGUI>();

                        Destroy(im);

                        tr.anchoredPosition = new Vector2(400, -50);
                        t.text = "Двигайте мышью\nчтобы вращать камеру";

                        mouseText = textObj;
                    }
                    break;

                case KeyCode.Mouse2:
                    img.sprite = mouseWheel;
                    pos = new Vector2(700, -200);
                    description = "Колёсико мыши\nприближает и отдаляет камеру";

                    StartCoroutine(MouseWheelAnimation(icon));
                    break;
            }

            rt.anchoredPosition = pos;

            if (txt != null && description != "")
                txt.text = description;

            StartCoroutine(AnimateIcon(icon, i * 0.15f));
        }
    }

    IEnumerator TypeText(string text)
    {
        textUI.text = "";

        foreach (char c in text)
        {
            textUI.text += c;
            yield return new WaitForSeconds(letterSpeed);
        }
    }

    IEnumerator Fade(float start, float end, float duration)
    {
        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;
            panel.alpha = Mathf.Lerp(start, end, t / duration);
            yield return null;
        }

        panel.alpha = end;
    }

    IEnumerator Hide()
    {
        pressAnyKeyText.gameObject.SetActive(false);

        yield return Fade(1, 0, 0.4f);

        panel.interactable = false;
        panel.blocksRaycasts = false;

        UnlockPlayer();
    }

    IEnumerator AnimateIcon(GameObject icon, float delay)
    {
        icon.transform.localScale = Vector3.zero;
        yield return new WaitForSeconds(delay);

        float t = 0f;
        float duration = 0.25f;

        while (t < duration)
        {
            t += Time.deltaTime;

            float scale = Mathf.SmoothStep(0, 1, t / duration);

            icon.transform.localScale = new Vector3(scale, scale, scale);

            yield return null;
        }

        icon.transform.localScale = Vector3.one;
    }

    IEnumerator PulseText()
    {
        while (waitingForKey)
        {
            float scale = 1f + Mathf.Sin(Time.time * 3f) * 0.05f;
            pressAnyKeyText.transform.localScale = new Vector3(scale, scale, 1);
            yield return null;
        }
    }

    IEnumerator MouseLeftRight(GameObject icon, Vector2 basePos)
    {
        RectTransform rt = icon.GetComponent<RectTransform>();
        while (true)
        {
            float offset = Mathf.Sin(Time.time * 2f) * 10f;
            rt.anchoredPosition = basePos + new Vector2(offset, 0);
            yield return null;
        }
    }

    IEnumerator MouseUpDown(GameObject icon, Vector2 basePos)
    {
        RectTransform rt = icon.GetComponent<RectTransform>();
        while (true)
        {
            float offset = Mathf.Sin(Time.time * 2f) * 10f;
            rt.anchoredPosition = basePos + new Vector2(0, offset);
            yield return null;
        }
    }

    IEnumerator MouseWheelAnimation(GameObject icon)
    {
        while (true)
        {
            float scale = 1f + Mathf.Sin(Time.time * 4f) * 0.05f;
            icon.transform.localScale = new Vector3(scale, scale, 1);
            yield return null;
        }
    }
}