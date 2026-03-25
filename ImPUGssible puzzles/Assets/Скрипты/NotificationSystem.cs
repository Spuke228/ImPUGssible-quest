using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class NotificationSystem : MonoBehaviour
{
    public CanvasGroup group;
    public Image icon;
    public TextMeshProUGUI text;

    public float showTime = 3f;

    public void Show(Sprite iconSprite, string message)
    {
        icon.sprite = iconSprite;
        text.text = message;

        StartCoroutine(Routine());
    }

    IEnumerator Routine()
    {
        group.alpha = 1;

        yield return new WaitForSeconds(showTime);

        group.alpha = 0;
    }
}