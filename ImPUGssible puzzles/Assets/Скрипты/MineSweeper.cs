using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

public class Minesweeper : MonoBehaviour
{
    int width = 9;
    int height = 9;
    int mines = 10;

    Cell[,] grid;

    bool firstClick = true;

    GridLayoutGroup layout;
    ComputerOSManager os;

    float timer;
    bool running;

    TMP_Text timerText;
    TMP_Text minesText;

    int flags;

    static readonly Color[] colors =
    {
        Color.clear,
        new Color(0.1f,0.3f,1f),
        new Color(0f,0.5f,0f),
        Color.red,
        new Color(0f,0f,0.5f),
        new Color(0.5f,0f,0f),
        new Color(0f,0.5f,0.5f),
        Color.black,
        Color.gray
    };

    public void Init(GridLayoutGroup g, ComputerOSManager computer)
    {
        layout = g;
        os = computer;

        CreateTopBar();
        Generate();
    }

    void Update()
    {
        if (running)
        {
            timer += Time.unscaledDeltaTime;
            if (timerText != null) timerText.text = Mathf.FloorToInt(timer).ToString("000");
        }
    }

    void CreateTopBar()
    {
        GameObject bar = new GameObject("TopBar");
        bar.transform.SetParent(transform.parent, false);

        Image bg = bar.AddComponent<Image>();
        bg.color = new Color(.15f, .15f, .15f);

        RectTransform r = bar.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(.1f, .9f);
        r.anchorMax = new Vector2(.9f, .98f);
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;

        timerText = CreateText(bar.transform, "000");
        timerText.alignment = TextAlignmentOptions.Right;

        minesText = CreateText(bar.transform, mines.ToString("000"));
        minesText.alignment = TextAlignmentOptions.Left;
    }

    TMP_Text CreateText(Transform parent, string text)
    {
        GameObject g = new GameObject("Text");
        g.transform.SetParent(parent, false);

        TMP_Text t = g.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = 36;
        t.color = Color.white;
        RectTransform r = t.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;

        return t;
    }

    void Generate()
    {
        grid = new Cell[width, height];

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                GameObject c = new GameObject("Cell");
                c.transform.SetParent(layout.transform, false);

                Image img = c.AddComponent<Image>();
                img.color = new Color(.7f, .7f, .7f);

                Button b = c.AddComponent<Button>();

                TMP_Text t = new GameObject("Text").AddComponent<TextMeshProUGUI>();
                t.transform.SetParent(c.transform, false);
                t.alignment = TextAlignmentOptions.Center;
                t.fontSize = 28;
                RectTransform tr = t.GetComponent<RectTransform>();
                tr.anchorMin = Vector2.zero;
                tr.anchorMax = Vector2.one;
                tr.offsetMin = Vector2.zero;
                tr.offsetMax = Vector2.zero;

                Cell cell = new Cell
                {
                    x = x,
                    y = y,
                    img = img,
                    text = t,
                    button = b
                };

                grid[x, y] = cell;

                int cx = x, cy = y;
                b.onClick.AddListener(() => LeftClick(cx, cy));

                EventTrigger trigger = c.AddComponent<EventTrigger>();
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerClick;
                entry.callback.AddListener((data) =>
                {
                    PointerEventData p = (PointerEventData)data;
                    if (p.button == PointerEventData.InputButton.Right)
                        ToggleFlag(cx, cy);
                });
                trigger.triggers.Add(entry);
            }
    }

    void PlaceMines(int safeX, int safeY)
    {
        int placed = 0;
        while (placed < mines)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);
            if (grid[x, y].mine) continue;
            if (x == safeX && y == safeY) continue;

            grid[x, y].mine = true;
            placed++;
        }

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int count = 0;
                for (int yy = -1; yy <= 1; yy++)
                    for (int xx = -1; xx <= 1; xx++)
                    {
                        int nx = x + xx;
                        int ny = y + yy;
                        if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                        if (grid[nx, ny].mine) count++;
                    }
                grid[x, y].number = count;
            }
    }

    void LeftClick(int x, int y)
    {
        Cell c = grid[x, y];
        if (c.flag) return;

        if (firstClick)
        {
            firstClick = false;
            running = true;
            PlaceMines(x, y);
        }

        if (c.mine)
        {
            Lose();
            return;
        }

        if (c.open)
        {
            DoubleOpen(x, y);
            return;
        }

        Flood(x, y);
        CheckWin();
    }

    void Flood(int x, int y)
    {
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(new Vector2Int(x, y));

        while (q.Count > 0)
        {
            var p = q.Dequeue();
            Cell c = grid[p.x, p.y];
            if (c.open) continue;
            c.open = true;
            c.img.color = Color.white;

            if (c.number > 0)
            {
                c.text.text = c.number.ToString();
                c.text.color = colors[c.number];
                continue;
            }

            for (int yy = -1; yy <= 1; yy++)
                for (int xx = -1; xx <= 1; xx++)
                {
                    int nx = p.x + xx;
                    int ny = p.y + yy;
                    if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                    if (!grid[nx, ny].open)
                        q.Enqueue(new Vector2Int(nx, ny));
                }
        }
    }

    void DoubleOpen(int x, int y)
    {
        Cell c = grid[x, y];
        if (c.number == 0) return;
        int aroundFlags = 0;

        for (int yy = -1; yy <= 1; yy++)
            for (int xx = -1; xx <= 1; xx++)
            {
                int nx = x + xx;
                int ny = y + yy;
                if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                if (grid[nx, ny].flag) aroundFlags++;
            }

        if (aroundFlags != c.number) return;

        for (int yy = -1; yy <= 1; yy++)
            for (int xx = -1; xx <= 1; xx++)
            {
                int nx = x + xx;
                int ny = y + yy;
                if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                if (!grid[nx, ny].flag)
                    LeftClick(nx, ny);
            }
    }

    void ToggleFlag(int x, int y)
    {
        Cell c = grid[x, y];
        if (c.open) return;
        c.flag = !c.flag;

        if (c.flag)
        {
            flags++;
            c.text.text = "🚩";
        }
        else
        {
            flags--;
            c.text.text = "";
        }

        if (minesText != null)
            minesText.text = (mines - flags).ToString("000");
    }

    void Lose()
    {
        running = false;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                if (grid[x, y].mine)
                    grid[x, y].text.text = "*";
    }

    void CheckWin()
    {
        int opened = 0;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                if (grid[x, y].open) opened++;

        if (opened == width * height - mines)
        {
            running = false;
            if (os != null)
                os.ShowAbilityUnlock("Mine Detector");
        }
    }

    class Cell
    {
        public int x;
        public int y;
        public bool mine;
        public bool open;
        public bool flag;
        public int number;
        public Image img;
        public TMP_Text text;
        public Button button;
    }
}