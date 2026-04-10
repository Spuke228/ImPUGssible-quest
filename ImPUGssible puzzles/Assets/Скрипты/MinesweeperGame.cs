using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

public class Minesweeper : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite mineSprite;
    public Sprite flagSprite;
    int width = 9;
    int height = 9;

    [SerializeField] int mines = 10; // можно оставить private, но через свойство
    Cell[,] grid;
    bool firstClick = true;
    GridLayoutGroup layout;
    ComputerOSManager os;
    GameObject TopBar;

    float timer;
    bool running;

    TMP_Text timerText;
    TMP_Text minesText;

    int flags;
    bool gameOver;

    // Добавляем публичные свойства для доступа из ComputerOSManager
    public int MinesCount => mines;
    public TMP_Text TimerText { get; set; }
    public TMP_Text MinesText { get; set; }

    static readonly Color[] colors = {
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
                img.preserveAspect = true;
                img.color = new Color(.55f, .55f, .55f);

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
        if (gameOver) return;

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

    void ToggleFlag(int x, int y)
    {
        if (gameOver) return;
        Cell c = grid[x, y];
        if (c.open) return;
        c.flag = !c.flag;

        if (c.flag)
        {
            flags++;
            c.img.sprite = flagSprite;
            c.img.color = Color.white;
            c.text.text = "";
        }
        else
        {
            flags--;
            c.img.sprite = null;
            c.img.color = new Color(.7f, .7f, .7f);
        }

        if (MinesText != null)
            MinesText.text = (mines - flags).ToString("000");
    }

    void Lose()
    {
        running = false;
        gameOver = true;
        if (os != null)
            os.ShowMinesweeperLose();

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y].mine)
                {
                    grid[x, y].img.sprite = mineSprite; // исправлено
                    grid[x, y].img.color = Color.white;
                }

                grid[x, y].button.interactable = false;
            }
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
            gameOver = true;

            if (os != null)
                os.ShowAbilityUnlock("Mine Detector");
        }
    }

    public void RestartGame()
    {
        foreach (Transform c in layout.transform)
            Destroy(c.gameObject);

        flags = 0;
        timer = 0;
        running = false;
        gameOver = false;
        firstClick = true;

        Generate();
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

                if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                    continue;

                if (grid[nx, ny].flag)
                    aroundFlags++;
            }

        if (aroundFlags != c.number)
            return;

        for (int yy = -1; yy <= 1; yy++)
            for (int xx = -1; xx <= 1; xx++)
            {
                int nx = x + xx;
                int ny = y + yy;

                if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                    continue;

                if (!grid[nx, ny].flag)
                    LeftClick(nx, ny);
            }
    }

    void CreateTopBar()
    {
        TopBar = new GameObject("TopBar", typeof(RectTransform));
        TopBar.transform.SetParent(layout.transform.parent, false);

        RectTransform tr = TopBar.GetComponent<RectTransform>();
        tr.anchorMin = new Vector2(0, .88f);
        tr.anchorMax = new Vector2(1, .95f);
        tr.offsetMin = Vector2.zero;
        tr.offsetMax = Vector2.zero;

        Image img = TopBar.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.1f);

        // таймер
        GameObject timerGO = new GameObject("Timer", typeof(RectTransform));
        timerGO.transform.SetParent(TopBar.transform, false);

        RectTransform trTimer = timerGO.GetComponent<RectTransform>();
        trTimer.anchorMin = new Vector2(0, 0);
        trTimer.anchorMax = new Vector2(0, 1);
        trTimer.sizeDelta = new Vector2(120, 0);
        trTimer.anchoredPosition = new Vector2(60, 0);

        timerText = CreateText(timerGO.transform, "000");
        TimerText = timerText;

        // мины
        GameObject minesGO = new GameObject("Mines", typeof(RectTransform));
        minesGO.transform.SetParent(TopBar.transform, false);

        RectTransform trMines = minesGO.GetComponent<RectTransform>();
        trMines.anchorMin = new Vector2(1, 0);
        trMines.anchorMax = new Vector2(1, 1);
        trMines.sizeDelta = new Vector2(120, 0);
        trMines.anchoredPosition = new Vector2(-60, 0);

        minesText = CreateText(minesGO.transform, mines.ToString("000"));
        MinesText = minesText;

        // кнопка рестарта
        GameObject restart = new GameObject("Restart", typeof(RectTransform));
        restart.transform.SetParent(TopBar.transform, false);

        RectTransform rr = restart.GetComponent<RectTransform>();
        rr.anchorMin = new Vector2(0.5f, 0);
        rr.anchorMax = new Vector2(0.5f, 1);
        rr.sizeDelta = new Vector2(120, 0);

        Image ri = restart.AddComponent<Image>();
        ri.color = new Color(.25f, .25f, .25f);

        Button rb = restart.AddComponent<Button>();
        rb.onClick.AddListener(RestartGame);

        ColorBlock cb = rb.colors;
        cb.normalColor = new Color(.25f, .25f, .25f);
        cb.highlightedColor = new Color(.35f, .35f, .35f);
        cb.pressedColor = new Color(.15f, .15f, .15f);
        rb.colors = cb;

        TMP_Text rt = new GameObject("Text").AddComponent<TextMeshProUGUI>();
        rt.transform.SetParent(restart.transform, false);
        rt.text = "Restart";
        rt.fontSize = 28;
        rt.alignment = TextAlignmentOptions.Center;

        RectTransform rtr = rt.GetComponent<RectTransform>();
        rtr.anchorMin = Vector2.zero;
        rtr.anchorMax = Vector2.one;
        rtr.offsetMin = Vector2.zero;
        rtr.offsetMax = Vector2.zero;
    }
}