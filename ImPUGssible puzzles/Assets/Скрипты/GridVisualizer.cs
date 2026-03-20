using UnityEngine;
using UnityEngine.UI;

public class GridVisualizer : MonoBehaviour
{
    public PuzzlePieceUI[] pieces;

    public Vector2 cellSize = new Vector2(100f, 100f);
    public Vector2 startPosition = new Vector2(0, 0);
    public Color cellColor = new Color(1f, 1f, 1f, 0.2f);

    void Start()
    {
        RectTransform board = GetComponent<RectTransform>();

        if (pieces == null || pieces.Length != 9)
        {
            Debug.LogError("Нужно ровно 9 пазлов");
            return;
        }

        int columns = 3;
        int rows = 3;

        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i] == null) continue;

            int r = i / columns;
            int c = i % columns;

            GameObject cell = new GameObject("Cell_" + i, typeof(Image));
            cell.transform.SetParent(transform, false);

            Image img = cell.GetComponent<Image>();
            img.color = cellColor;
            img.raycastTarget = false;

            RectTransform rt = cell.GetComponent<RectTransform>();

            // КРИТИЧНО
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);

            rt.sizeDelta = cellSize;

            Vector2 pos = startPosition + new Vector2(c * cellSize.x, -r * cellSize.y);
            rt.anchoredPosition = pos;

            // задаём цель пазлу
            pieces[i].targetPosition = pos;
        }
    }
}