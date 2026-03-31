using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class ComputerOS : MonoBehaviour
{
    [Header("Player")]
    public Управлениемопсом pugController;
    public Transform player;

    [Header("Computer")]
    public Transform monitor;
    public float interactDistance = 3f;

    [Header("Icons")]
    public Sprite galleryIcon;
    public Sprite browserIcon;
    public Sprite robloxIcon;
    public Sprite dotaIcon;
    public Sprite minesweeperIcon;

    private Canvas computerCanvas;
    private RectTransform desktop;
    private RectTransform taskbar;
    private bool open;

    private Dictionary<string, Window> windows = new();
    private float doubleClickTime = 0.25f;
    private Dictionary<GameObject, float> lastClick = new();

    private GameObject notification;
    private TMP_Text notificationText;

    private List<MonoBehaviour> disabledScripts = new List<MonoBehaviour>();
    private List<GameObject> disabledUIs = new List<GameObject>();

    void Start()
    {
        CreateCanvas();      // отдельный Canvas
        BuildDesktop();
        BuildNotification();
        BuildIcons();
        BuildWindows();
        AddDesktopCloseButton();

        desktop.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!monitor || !player) return;

        if (Vector3.Distance(player.position, monitor.position) < interactDistance && Input.GetKeyDown(KeyCode.E))
        {
            if (!open) OpenComputer();
        }
    }

    void OpenComputer()
    {
        open = true;
        desktop.gameObject.SetActive(true);

        // Отключаем скрипты игрока и паузу
        disabledScripts.Clear();
        foreach (MonoBehaviour mb in FindObjectsOfType<MonoBehaviour>())
        {
            if (mb != this && mb.enabled &&
                (mb.GetType() == typeof(Управлениемопсом) || mb.GetType().Name == "PauseMenu" || mb.GetType().Name == "BaldiLikeQuiz"))
            {
                mb.enabled = false;
                disabledScripts.Add(mb);
            }
        }

        // Скрываем все UI мопса
        disabledUIs.Clear();
        foreach (Image img in FindObjectsOfType<Image>())
        {
            if (!img.transform.IsChildOf(computerCanvas.transform) && img.enabled)
            {
                img.enabled = false;
                disabledUIs.Add(img.gameObject);
            }
        }
        foreach (TMP_Text t in FindObjectsOfType<TMP_Text>())
        {
            if (!t.transform.IsChildOf(computerCanvas.transform) && t.enabled)
            {
                t.enabled = false;
                disabledUIs.Add(t.gameObject);
            }
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    void CloseComputer()
    {
        open = false;
        desktop.gameObject.SetActive(false);

        foreach (MonoBehaviour mb in disabledScripts) if (mb != null) mb.enabled = true;
        disabledScripts.Clear();

        foreach (GameObject go in disabledUIs) if (go != null) go.SetActive(true);
        disabledUIs.Clear();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
    }

    void CreateCanvas()
    {
        GameObject canvasGO = new GameObject("ComputerCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        computerCanvas = canvasGO.GetComponent<Canvas>();
        computerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        computerCanvas.overrideSorting = true;
        computerCanvas.sortingOrder = 1000;

        CanvasScaler cs = canvasGO.GetComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        cs.matchWidthOrHeight = 0.5f;
    }

    void BuildDesktop()
    {
        desktop = CreatePanel("Desktop", computerCanvas.transform, new Color(.07f, .08f, .12f));
        desktop.anchorMin = Vector2.zero;
        desktop.anchorMax = Vector2.one;
        desktop.offsetMin = Vector2.zero;
        desktop.offsetMax = Vector2.zero;

        taskbar = CreatePanel("Taskbar", desktop, new Color(.1f, .1f, .1f));
        taskbar.anchorMin = new Vector2(0, 0);
        taskbar.anchorMax = new Vector2(1, 0);
        taskbar.pivot = new Vector2(0.5f, 0);
        taskbar.sizeDelta = new Vector2(0, 80);
    }

    void BuildIcons()
    {
        string[] names = { "Gallery", "Browser", "Roblox", "Dota", "Minesweeper" };
        Sprite[] icons = { galleryIcon, browserIcon, robloxIcon, dotaIcon, minesweeperIcon };

        for (int i = 0; i < icons.Length; i++)
        {
            CreateIcon(names[i], icons[i]);
        }

        ArrangeIcons(); // авто-выравнивание
    }

    void CreateIcon(string name, Sprite icon)
    {
        GameObject g = new GameObject(name);
        g.transform.SetParent(desktop, false);

        Image img = g.AddComponent<Image>();
        img.sprite = icon;
        img.color = Color.white;

        Button b = g.AddComponent<Button>();
        RectTransform r = g.GetComponent<RectTransform>();
        r.sizeDelta = new Vector2(100, 100);

        TMP_Text label = CreateText(g.transform, name);
        label.alignment = TextAlignmentOptions.Center;
        RectTransform tr = label.rectTransform;
        tr.pivot = new Vector2(0.5f, 1);
        tr.anchorMin = new Vector2(0.5f, 0);
        tr.anchorMax = new Vector2(0.5f, 0);
        tr.anchoredPosition = new Vector2(0, -10);

        DragWindow dw = g.AddComponent<DragWindow>();
        dw.Init(true, this);

        b.onClick.AddListener(() => HandleClick(g, name));
    }

    void ArrangeIcons()
    {
        int columns = 4;
        float xSpacing = 140f;
        float ySpacing = 160f;
        Vector2 startPos = new Vector2(20, -20);

        int idx = 0;
        foreach (Transform child in desktop)
        {
            if (child == taskbar || child == notification.transform) continue;
            int row = idx / columns;
            int col = idx % columns;
            RectTransform r = child.GetComponent<RectTransform>();
            r.anchoredPosition = new Vector2(startPos.x + col * xSpacing, startPos.y - row * ySpacing);
            idx++;
        }
    }

    void BuildWindows()
    {
        CreateWindow("Gallery", BuildGallery);
        CreateWindow("Browser", BuildBrowser);
        CreateWindow("Roblox", BuildRoblox);
        CreateWindow("Dota", BuildDota);
        CreateWindow("Minesweeper", BuildMinesweeper);
    }

    void AddDesktopCloseButton()
    {
        GameObject close = CreateButton("CloseDesktop", desktop, new Vector2(40, 40), CloseComputer);
        RectTransform dcr = close.GetComponent<RectTransform>();
        dcr.anchorMin = new Vector2(1, 1);
        dcr.anchorMax = new Vector2(1, 1);
        dcr.pivot = new Vector2(1, 1);
        dcr.anchoredPosition = new Vector2(-10, -10);
        close.GetComponent<Image>().color = Color.red;
    }

    RectTransform CreatePanel(string name, Transform parent, Color color)
    {
        GameObject g = new GameObject(name);
        g.transform.SetParent(parent, false);
        Image img = g.AddComponent<Image>();
        img.color = color;
        RectTransform r = g.GetComponent<RectTransform>();
        r.localScale = Vector3.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
        return r;
    }

    TMP_Text CreateText(Transform parent, string text)
    {
        GameObject g = new GameObject("Text");
        g.transform.SetParent(parent, false);
        TMP_Text t = g.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = 24;
        t.color = Color.white;
        RectTransform r = t.rectTransform;
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
        return t;
    }

    GameObject CreateButton(string name, Transform parent, Vector2 size, System.Action onClick)
    {
        GameObject g = new GameObject(name);
        g.transform.SetParent(parent, false);

        Image img = g.AddComponent<Image>();
        img.color = Color.white;

        Button btn = g.AddComponent<Button>();
        if (onClick != null) btn.onClick.AddListener(() => onClick());

        RectTransform r = g.GetComponent<RectTransform>();
        r.sizeDelta = size;

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(g.transform, false);
        TMP_Text t = textGO.AddComponent<TextMeshProUGUI>();
        t.text = name;
        t.fontSize = 18;
        t.alignment = TextAlignmentOptions.Center;
        t.color = Color.black;
        RectTransform tr = t.rectTransform;
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero;
        tr.offsetMax = Vector2.zero;

        return g;
    }

    void CreateWindow(string name, System.Action<GameObject> builder)
    {
        RectTransform w = CreatePanel(name, desktop, new Color(.2f, .2f, .25f));
        w.anchorMin = new Vector2(0.25f, 0.25f);
        w.anchorMax = new Vector2(0.75f, 0.75f);

        GameObject top = CreatePanel("Top", w, new Color(.15f, .15f, .2f)).gameObject;
        RectTransform tr = top.GetComponent<RectTransform>();
        tr.anchorMin = new Vector2(0, 1);
        tr.anchorMax = new Vector2(1, 1);
        tr.pivot = new Vector2(0.5f, 1);
        tr.sizeDelta = new Vector2(0, 40);
        top.AddComponent<DragWindow>().Init(false, this);

        GameObject close = CreateButton("X", top.transform, new Vector2(30, 30), () =>
        {
            Minimize(name);
            if (windows[name].taskButton != null) windows[name].taskButton.SetActive(false);
        });
        close.GetComponent<Image>().color = Color.red;
        RectTransform cr = close.GetComponent<RectTransform>();
        cr.anchorMin = new Vector2(1, 1);
        cr.anchorMax = new Vector2(1, 1);
        cr.pivot = new Vector2(1, 1);
        cr.anchoredPosition = new Vector2(-5, -5);

        builder(w.gameObject);

        Window win = new Window();
        win.root = w.gameObject;
        GameObject taskBtn = CreateButton(name, taskbar, new Vector2(120, 40), () =>
        {
            OpenWindow(name);
            taskBtn.SetActive(true);
        });
        win.taskButton = taskBtn;

        windows[name] = win;
        w.gameObject.SetActive(false);
        taskBtn.SetActive(false);
    }

    void OpenWindow(string name)
    {
        GameObject w = windows[name].root;
        w.SetActive(true);
        w.transform.localScale = Vector3.zero;
        windows[name].taskButton.SetActive(true);
        StartCoroutine(OpenAnim(w.transform));
    }

    void Minimize(string name)
    {
        windows[name].root.SetActive(false);
        if (windows[name].taskButton != null) windows[name].taskButton.SetActive(false);
    }

    IEnumerator OpenAnim(Transform t)
    {
        float v = 0;
        while (v < 1)
        {
            v += Time.unscaledDeltaTime * 6;
            t.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, v);
            yield return null;
        }
    }

    public void ShowAbilityUnlock(string abilityName)
    {
        StartCoroutine(NotificationCoroutine(abilityName));
    }

    private IEnumerator NotificationCoroutine(string abilityName)
    {
        if (notification == null || notificationText == null) yield break;
        notification.SetActive(true);
        notificationText.text = "Вы разблокировали способность:\n" + abilityName;
        yield return new WaitForSecondsRealtime(4f);
        notification.SetActive(false);
    }

    void BuildGallery(GameObject parent) { TMP_Text t = CreateText(parent.transform, "Галерея пуста"); t.alignment = TextAlignmentOptions.Center; }
    void BuildBrowser(GameObject parent) { TMP_Text t = CreateText(parent.transform, "Браузер открыт"); t.alignment = TextAlignmentOptions.Center; }
    void BuildRoblox(GameObject parent) { TMP_Text t = CreateText(parent.transform, "Roblox запускается..."); t.alignment = TextAlignmentOptions.Center; }
    void BuildDota(GameObject parent) { TMP_Text t = CreateText(parent.transform, "Dota открыта"); t.alignment = TextAlignmentOptions.Center; }
    void BuildMinesweeper(GameObject parent) { TMP_Text t = CreateText(parent.transform, "Сапер готов"); t.alignment = TextAlignmentOptions.Center; }

    private class Window { public GameObject root; public GameObject taskButton; }

    public class DragWindow : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private RectTransform rect;
        private Vector2 offset;
        private bool icon;
        private ComputerOS os;

        public void Init(bool iconMode, ComputerOS o) { icon = iconMode; os = o; }
        void Awake() { rect = GetComponent<RectTransform>(); }

        public void OnBeginDrag(PointerEventData e)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect.parent as RectTransform, e.position, e.pressEventCamera, out offset);
        }

        public void OnDrag(PointerEventData e)
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect.parent as RectTransform, e.position, e.pressEventCamera, out pos);
            rect.localPosition = pos - offset;
        }

        public void OnEndDrag(PointerEventData e)
        {
            if (icon)
            {
                float grid = 140f;
                Vector2 p = rect.anchoredPosition;
                p.x = Mathf.Round(p.x / grid) * grid;
                p.y = Mathf.Round(p.y / grid) * grid;
                rect.anchoredPosition = p;
            }
        }
    }
}