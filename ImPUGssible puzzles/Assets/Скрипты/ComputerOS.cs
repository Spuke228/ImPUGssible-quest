using UnityEngine;
using UnityEngine.UI;
using TMPro;
using
UnityEngine.EventSystems;
using System.Collections.Generic;

// // COMPUTER OS MAIN SYSTEM // Attach to Canvas //

public class ComputerOS : MonoBehaviour
{
    [Header(“Player”)]
    public
MonoBehaviour playerController; public MonoBehaviour cameraController;
    public Transform player;

    [Header("Computer")]
    public Transform monitor;
    public float interactDistance = 3f;

    [Header("Icons")]
    public Sprite minesIcon;
    public Sprite browserIcon;
    public Sprite galleryIcon;

    Canvas canvas;
    RectTransform desktop;
    RectTransform taskbar;

    Dictionary<string, GameObject> windows = new Dictionary<string, GameObject>();

    bool opened;

    void Start()
    {
        canvas = GetComponent<Canvas>();
        BuildDesktop();
    }

    void Update()
    {
        if (!opened)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (Vector3.Distance(player.position, monitor.position) < interactDistance)
                {
                    OpenComputer();
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseComputer();
            }
        }
    }

    void OpenComputer()
    {
        opened = true;

        if (playerController) playerController.enabled = false;
        if (cameraController) cameraController.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        desktop.gameObject.SetActive(true);
        taskbar.gameObject.SetActive(true);
    }

    void CloseComputer()
    {
        opened = false;

        if (playerController) playerController.enabled = true;
        if (cameraController) cameraController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        desktop.gameObject.SetActive(false);
        taskbar.gameObject.SetActive(false);
    }

    void BuildDesktop()
    {
        desktop = CreatePanel("Desktop", transform, new Color(.1f, .45f, .8f));
        desktop.anchorMin = Vector2.zero;
        desktop.anchorMax = Vector2.one;
        desktop.gameObject.SetActive(false);

        taskbar = CreatePanel("Taskbar", transform, new Color(.1f, .1f, .1f));
        taskbar.anchorMin = new Vector2(0, 0);
        taskbar.anchorMax = new Vector2(1, .07f);
        taskbar.gameObject.SetActive(false);

        CreateIcon("Minesweeper", minesIcon, new Vector2(90, -90));
        CreateIcon("Browser", browserIcon, new Vector2(90, -200));
        CreateIcon("Gallery", galleryIcon, new Vector2(90, -310));
    }

    RectTransform CreatePanel(string name, Transform parent, Color color)
    {
        GameObject g = new GameObject(name);
        g.transform.SetParent(parent, false);

        Image img = g.AddComponent<Image>();
        img.color = color;

        RectTransform r = g.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;

        return r;
    }

    void CreateIcon(string name, Sprite icon, Vector2 pos)
    {
        GameObject g = new GameObject(name);
        g.transform.SetParent(desktop, false);

        Image img = g.AddComponent<Image>();
        img.sprite = icon;

        RectTransform r = g.GetComponent<RectTransform>();
        r.sizeDelta = new Vector2(64, 64);
        r.anchorMin = new Vector2(0, 1);
        r.anchorMax = new Vector2(0, 1);
        r.pivot = new Vector2(0, 1);
        r.anchoredPosition = pos;

        Button b = g.AddComponent<Button>();
        b.onClick.AddListener(() => OpenApp(name));

        g.AddComponent<UIDrag>();

        CreateLabel(g, name);
    }

    void CreateLabel(GameObject icon, string text)
    {
        GameObject t = new GameObject("Label");
        t.transform.SetParent(icon.transform, false);

        TMP_Text label = t.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = 20;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;

        RectTransform r = label.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0, -0.8f);
        r.anchorMax = new Vector2(1, -0.2f);
    }

    void OpenApp(string name)
    {
        if (windows.ContainsKey(name))
        {
            windows[name].SetActive(true);
            return;
        }

        GameObject w = CreateWindow(name);

        if (name == "Minesweeper") BuildMines(w);
        if (name == "Browser") BuildBrowser(w);

        windows.Add(name, w);
    }

    GameObject CreateWindow(string title)
    {
        GameObject w = new GameObject(title);
        w.transform.SetParent(desktop, false);

        Image img = w.AddComponent<Image>();
        img.color = new Color(.2f, .2f, .2f);

        RectTransform r = w.GetComponent<RectTransform>();
        r.sizeDelta = new Vector2(900, 650);

        w.AddComponent<UIDrag>();

        CreateTitleBar(w, title);

        return w;
    }

    void CreateTitleBar(GameObject window, string title)
    {
        GameObject bar = new GameObject("TitleBar");
        bar.transform.SetParent(window.transform, false);

        Image img = bar.AddComponent<Image>();
        img.color = new Color(.1f, .1f, .1f);

        RectTransform r = bar.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0, .93f);
        r.anchorMax = new Vector2(1, 1);

        TMP_Text t = new GameObject("Title").AddComponent<TextMeshProUGUI>();
        t.transform.SetParent(bar.transform, false);
        t.text = title;
        t.fontSize = 26;
        t.color = Color.white;

        RectTransform tr = t.GetComponent<RectTransform>();
        tr.anchorMin = new Vector2(.02f, 0);
        tr.anchorMax = new Vector2(.7f, 1);

        CreateButton("X", bar.transform, new Vector2(40, 40), () => Destroy(window));
        CreateButton("_", bar.transform, new Vector2(40, 40), () => window.SetActive(false));
    }

    GameObject CreateButton(string text, Transform parent, Vector2 size, UnityEngine.Events.UnityAction action)
    {
        GameObject g = new GameObject(text);
        g.transform.SetParent(parent, false);

        Image img = g.AddComponent<Image>();
        img.color = new Color(.35f, .35f, .35f);

        Button b = g.AddComponent<Button>();
        b.onClick.AddListener(action);

        RectTransform r = g.GetComponent<RectTransform>();
        r.sizeDelta = size;

        TMP_Text t = new GameObject("Text").AddComponent<TextMeshProUGUI>();
        t.transform.SetParent(g.transform, false);
        t.text = text;
        t.fontSize = 24;
        t.alignment = TextAlignmentOptions.Center;

        RectTransform tr = t.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;

        return g;
    }

    void BuildBrowser(GameObject parent)
    {
        CreateButton("YouTube", parent.transform, new Vector2(200, 80),
            () => Debug.Log("Fake YouTube"));

        CreateButton("Steam", parent.transform, new Vector2(200, 80),
            () => Debug.Log("Fake Steam"));
    }

    void BuildMines(GameObject parent)
    {
        GameObject grid = new GameObject("Mines");
        grid.transform.SetParent(parent.transform, false);

        GridLayoutGroup g = grid.AddComponent<GridLayoutGroup>();
        g.cellSize = new Vector2(60, 60);

        Minesweeper m = grid.AddComponent<Minesweeper>();
        m.StartGame();
    }

}

public class Minesweeper : MonoBehaviour
{
    int width = 9; int height =
9; int mines = 10;

    int[,] board;
    bool[,] revealed;
    bool firstClick = true;

    void StartGame()
    {
        board = new int[width, height];
        revealed = new bool[width, height];

        BuildGrid();
    }

    void BuildGrid()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                GameObject cell = new GameObject("Cell");
                cell.transform.SetParent(transform, false);

                Image img = cell.AddComponent<Image>();
                img.color = Color.gray;

                Button b = cell.AddComponent<Button>();

                int cx = x;
                int cy = y;

                b.onClick.AddListener(() => Click(cx, cy, img));
            }
    }

    void PlaceMines(int sx, int sy)
    {
        int placed = 0;

        while (placed < mines)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            if (board[x, y] == -1) continue;
            if (x == sx && y == sy) continue;

            board[x, y] = -1;
            placed++;
        }
    }

    void Click(int x, int y, Image img)
    {
        if (firstClick)
        {
            PlaceMines(x, y);
            firstClick = false;
        }

        if (board[x, y] == -1)
        {
            img.color = Color.red;
            Debug.Log("BOOM");
            return;
        }

        img.color = Color.white;
        revealed[x, y] = true;
    }

}

public class UIDrag : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    Vector2 offset;

    public void OnBeginDrag(PointerEventData e)
    {
        RectTransform r = transform as RectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            r, e.position, e.pressEventCamera, out offset);
    }

    public void OnDrag(PointerEventData e)
    {
        RectTransform r = transform as RectTransform;

        Vector2 pos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            r.parent as RectTransform,
            e.position, e.pressEventCamera, out pos);

        r.localPosition = pos - offset;
    }

}
