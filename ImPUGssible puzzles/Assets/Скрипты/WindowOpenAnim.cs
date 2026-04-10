using UnityEngine;
using System.Collections;

public class WindowOpenAnim : MonoBehaviour
{
    void OnEnable()
    {
        StartCoroutine(Anim());
    }

    IEnumerator Anim()
    {
        transform.localScale = Vector3.zero;

        float t = 0;
        while (t < 1)
        {
            t += Time.unscaledDeltaTime * 6f;
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            yield return null;
        }

        transform.localScale = Vector3.one;
    }
}