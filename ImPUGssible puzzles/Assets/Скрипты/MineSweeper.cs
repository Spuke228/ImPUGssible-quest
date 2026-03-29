using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class Minesweeper : MonoBehaviour
{
    public int width = 8;
    public int height = 8;
    public int mines = 10;

    public Transform grid;
    public GameObject cellPrefab;

    public TextMeshProUGUI statusText;
    public TextMeshProUGUI mineCounter;
    public TextMeshProUGUI timerText;

    MinesweeperCell[,] cells;

    bool gameOver;
    bool firstClick = true;

    int flags;
    float timer;
    bool timerRunning;

    void Update()
    {
        if (!timerRunning) return;

        timer += Time.deltaTime;
        timerText.text = Mathf.FloorToInt(timer).ToString("000");
    }

    public void Generate()
    {
        gameOver = false;
        firstClick = true;

        timer = 0;
        timerRunning = true;

        flags = 0;
        mineCounter.text = mines.ToString("000");
        statusText.text = "";

        foreach (Transform c in grid)
            Destroy(c.gameObject);

        cells = new MinesweeperCell[width, height];

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                GameObject obj = Instantiate(cellPrefab, grid);
                MinesweeperCell cell = obj.GetComponent<MinesweeperCell>();

                cell.Init(this, x, y);

                cells[x, y] = cell;
            }

        PlaceMines();
        RecalculateNumbers();
    }

    void PlaceMines()
    {
        int placed = 0;

        while (placed < mines)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            if (!cells[x, y].mine)
            {
                cells[x, y].mine = true;
                placed++;
            }
        }
    }

    public void OpenCell(int x, int y)
    {
        if (gameOver) return;

        MinesweeperCell cell = cells[x, y];

        if (cell.opened || cell.flag) return;

        if (firstClick)
        {
            firstClick = false;

            if (cell.mine)
            {
                cell.mine = false;
                PlaceRandomMine();
                RecalculateNumbers();
            }
        }

        cell.Open();

        if (cell.mine)
        {
            Lose();
            return;
        }

        if (cell.number == 0)
            FloodFill(x, y);

        CheckWin();
    }

    void FloodFill(int startX, int startY)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startX, startY));

        while (queue.Count > 0)
        {
            Vector2Int pos = queue.Dequeue();

            for (int y = -1; y <= 1; y++)
                for (int x = -1; x <= 1; x++)
                {
                    int nx = pos.x + x;
                    int ny = pos.y + y;

                    if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                        continue;

                    MinesweeperCell n = cells[nx, ny];

                    if (n.opened || n.flag)
                        continue;

                    n.Open();

                    if (n.number == 0 && !n.mine)
                        queue.Enqueue(new Vector2Int(nx, ny));
                }
        }
    }

    public void OpenAround(int x, int y)
    {
        MinesweeperCell cell = cells[x, y];

        if (!cell.opened) return;

        int flagCount = 0;

        for (int yy = -1; yy <= 1; yy++)
            for (int xx = -1; xx <= 1; xx++)
            {
                int nx = x + xx;
                int ny = y + yy;

                if (nx >= 0 && ny >= 0 && nx < width && ny < height)
                    if (cells[nx, ny].flag)
                        flagCount++;
            }

        if (flagCount == cell.number)
        {
            for (int yy = -1; yy <= 1; yy++)
                for (int xx = -1; xx <= 1; xx++)
                {
                    int nx = x + xx;
                    int ny = y + yy;

                    if (nx >= 0 && ny >= 0 && nx < width && ny < height)
                        OpenCell(nx, ny);
                }
        }
    }

    void Lose()
    {
        gameOver = true;
        timerRunning = false;

        statusText.text = "BOOM";

        foreach (var c in cells)
            if (c.mine)
                c.ShowMine();
    }

    void CheckWin()
    {
        int opened = 0;

        foreach (var c in cells)
            if (c.opened)
                opened++;

        if (opened == width * height - mines)
        {
            gameOver = true;
            timerRunning = false;

            statusText.text = "YOU WIN";
        }
    }

    void PlaceRandomMine()
    {
        while (true)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            if (!cells[x, y].mine)
            {
                cells[x, y].mine = true;
                break;
            }
        }
    }

    void RecalculateNumbers()
    {
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                if (cells[x, y].mine) continue;

                cells[x, y].number = CountMines(x, y);
            }
    }

    int CountMines(int px, int py)
    {
        int c = 0;

        for (int y = -1; y <= 1; y++)
            for (int x = -1; x <= 1; x++)
            {
                int nx = px + x;
                int ny = py + y;

                if (nx >= 0 && ny >= 0 && nx < width && ny < height)
                    if (cells[nx, ny].mine)
                        c++;
            }

        return c;
    }

    public void AddFlag()
    {
        flags++;
        mineCounter.text = (mines - flags).ToString("000");
    }

    public void RemoveFlag()
    {
        flags--;
        mineCounter.text = (mines - flags).ToString("000");
    }
}