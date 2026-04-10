using UnityEngine;
using UnityEngine.EventSystems;

public class WindowDraggable : MonoBehaviour, IDragHandler
{
    RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void OnDrag(PointerEventData e)
    {
        rect.anchoredPosition += e.delta;
    }
}