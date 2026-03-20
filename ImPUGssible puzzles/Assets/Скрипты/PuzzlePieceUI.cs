using UnityEngine;
using UnityEngine.EventSystems;

public class PuzzlePieceUI : MonoBehaviour,
    IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    public Vector2 targetPosition;
    public float snapThreshold = 20f;

    private RectTransform rect;
    private Canvas canvas;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            rect.Rotate(0, 0, 90f);
            rect.localEulerAngles = new Vector3(0, 0, Mathf.Round(rect.eulerAngles.z / 90) * 90);
        }
    }

    public void OnBeginDrag(PointerEventData eventData) { }

    public void OnDrag(PointerEventData eventData)
    {
        rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (Vector2.Distance(rect.anchoredPosition, targetPosition) <= snapThreshold)
        {
            rect.anchoredPosition = targetPosition;
        }
    }

    public bool IsCorrect()
    {
        return Vector2.Distance(rect.anchoredPosition, targetPosition) <= snapThreshold;
    }
}