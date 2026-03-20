using UnityEngine;

public class PuzzleManagerUI : MonoBehaviour
{
    public MonoBehaviour playerController;
    public PuzzlePieceUI[] pieces;

    public GameObject puzzleUI;
    public GameObject door;
    public GameObject passage;

    [Header("Grid settings")]
    public Vector2 gridCellSize = new Vector2(100f, 100f); // размер ячейки
    public Vector2 gridStartPosition = new Vector2(0, 0); // левый верхний угол сетки

    void Start()
    {
        // выставляем позиции по сетке
        for (int i = 0; i < pieces.Length; i++)
        {
            int row = i / Mathf.CeilToInt(Mathf.Sqrt(pieces.Length));
            int col = i % Mathf.CeilToInt(Mathf.Sqrt(pieces.Length));
            pieces[i].targetPosition = gridStartPosition + new Vector2(col * gridCellSize.x, -row * gridCellSize.y);
        }
    }

    void Update()
    {
        if (!puzzleUI.activeSelf)
            return;

        bool solved = true;

        foreach (PuzzlePieceUI p in pieces)
        {
            if (!p.IsCorrect())
            {
                solved = false;
                break;
            }
        }

        if (solved)
        {
            puzzleUI.SetActive(false);
            door.SetActive(false);
            if (passage != null) passage.SetActive(true);

            playerController.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}