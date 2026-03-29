using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class WindowAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    Vector3 startScale;

    void Start()
    {
        startScale = transform.localScale;
    }

    void OnEnable()
    {
        StartCoroutine(OpenAnim());
    }

    IEnumerator OpenAnim()
    {
        transform.localScale = Vector3.zero;

        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * 6f;
            transform.localScale = Vector3.Lerp(Vector3.zero, startScale, t);
            yield return null;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = startScale * 1.15f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = startScale;
    }
}