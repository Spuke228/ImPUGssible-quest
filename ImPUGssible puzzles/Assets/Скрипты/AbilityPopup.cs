using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class AbilityPopup : MonoBehaviour
{
    public CanvasGroup group;
    public Image icon;
    public TextMeshProUGUI text;

    public float showTime = 3f;

    public void Show(Sprite abilityIcon, string abilityName)
    {
        icon.sprite = abilityIcon;
        text.text = "Получена способность: " + abilityName;

        StartCoroutine(Routine());
    }

    IEnumerator Routine()
    {
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * 3;
            group.alpha = t;
            yield return null;
        }

        yield return new WaitForSeconds(showTime);

        while (t > 0)
        {
            t -= Time.deltaTime * 3;
            group.alpha = t;
            yield return null;
        }
    }
}