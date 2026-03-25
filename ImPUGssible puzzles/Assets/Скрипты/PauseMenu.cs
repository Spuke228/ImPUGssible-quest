using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class TutorialControl
{
    public KeyCode key;
    public string description;
}

public class TutorialSystem : MonoBehaviour
{
    public CanvasGroup panel;
    public TextMeshProUGUI textUI;

    public Transform controlsContainer;
    public GameObject controlIconPrefab;

    public Button continueButton;

    public MonoBehaviour playerController;

    public float letterSpeed = 0.03f;
    public float fadeSpeed = 2f;

    void Start()
    {
        panel.alpha = 0;
        panel.interactable = false;
        panel.blocksRaycasts = false;

        continueButton.gameObject.SetActive(false);

        LockPlayer();

        TutorialControl[] controls =
        {
            new TutorialControl{ key = KeyCode.W, description = "Идти вперёд"},
            new TutorialControl{ key = KeyCode.A, description = "Идти влево"},
            new TutorialControl{ key = KeyCode.S, description = "Идти назад"},
            new TutorialControl{ key = KeyCode.D, description = "Идти вправо"},
            new TutorialControl{ key = KeyCode.E, description = "Поднять предмет"}
        };

        ShowTutorial(
            "Осмотритесь вокруг и попробуйте двигаться.",
            controls
        );
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

    public void ShowTutorial(string text, TutorialControl[] controls)
    {
        StartCoroutine(TutorialRoutine(text, controls));
    }

    IEnumerator TutorialRoutine(string text, TutorialControl[] controls)
    {
        panel.interactable = true;
        panel.blocksRaycasts = true;

        yield return Fade(0, 1);

        SpawnControls(controls);

        yield return TypeText(text);

        continueButton.gameObject.SetActive(true);
    }

    void SpawnControls(TutorialControl[] controls)
    {
        foreach (Transform c in controlsContainer)
            Destroy(c.gameObject);

        foreach (TutorialControl control in controls)
        {
            GameObject icon = Instantiate(controlIconPrefab, controlsContainer);

            TextMeshProUGUI[] texts =
                icon.GetComponentsInChildren<TextMeshProUGUI>();

            texts[0].text = control.key.ToString();
            texts[1].text = control.description;
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

    IEnumerator Fade(float start, float end)
    {
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * fadeSpeed;

            panel.alpha = Mathf.Lerp(start, end, t);

            yield return null;
        }
    }

    public void Continue()
    {
        StartCoroutine(Hide());
    }

    IEnumerator Hide()
    {
        continueButton.gameObject.SetActive(false);

        yield return Fade(1, 0);

        panel.interactable = false;
        panel.blocksRaycasts = false;

        UnlockPlayer();
    }
}