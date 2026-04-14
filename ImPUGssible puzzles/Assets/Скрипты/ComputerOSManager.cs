using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ComputerOSManager : MonoBehaviour
{
    public Transform player;
    public MonoBehaviour pugController;
    public MonoBehaviour cameraController;

    [Header("Desktop")]
    public Sprite desktopBackground;
    public Sprite defaultIconSprite;

    [Header("App Sprites")]
    public Sprite dotaSprite;
    public Sprite robloxSprite;
    public Sprite minesweeperSprite;
    public Sprite browserSprite;
    public Sprite gallerySprite;

    [Header("Gallery Images")]
    public Sprite[] galleryPhotos;

    [Header("Rutube")]
    public Sprite[] rutubePreviews;
    public string[] rutubeTitles;

    [Header("App Images")]
    public Sprite dotaImage;
    public Sprite robloxImage;

    [Header("Minesweeper Sprites")]
    public Sprite mineIcon;
    public Sprite flagIcon;

    bool computerOpen;

    Canvas canvas;
    GameObject desktop;
    GameObject windowLayer;
    GameObject taskbar;

    List<GameObject> windows = new List<GameObject>();


    void Start()
    {
        CreateEventSystem();
        CreateCanvas();
        CreateDesktop();
        CreateTaskbar();

        canvas.gameObject.SetActive(false);
    }

    void Update()
    {
        if (computerOpen && Input.GetKeyDown(KeyCode.Escape))
            CloseComputer();
    }

    public void OpenComputer()
    {
        computerOpen = true;

        canvas.gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (pugController) pugController.enabled = false;
        if (cameraController) cameraController.enabled = false;
    }

    void CloseComputer()
    {
        computerOpen = false;

        canvas.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (pugController) pugController.enabled = true;
        if (cameraController) cameraController.enabled = true;
    }

    void CreateEventSystem()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(StandaloneInputModule)
            );
        }
    }

    void CreateCanvas()
    {
        GameObject c = new GameObject("ComputerCanvas", typeof(RectTransform));

        canvas = c.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler cs = c.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);

        c.AddComponent<GraphicRaycaster>();
    }

    void CreateDesktop()
    {
        desktop = new GameObject("Desktop", typeof(RectTransform));
        desktop.transform.SetParent(canvas.transform, false);

        RectTransform r = desktop.GetComponent<RectTransform>();

        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;

        Image bg = desktop.AddComponent<Image>();

        if (desktopBackground)
            bg.sprite = desktopBackground;
        else
            bg.color = new Color(.08f, .08f, .08f);

        windowLayer = new GameObject("Windows", typeof(RectTransform));
        windowLayer.transform.SetParent(desktop.transform, false);

        RectTransform wr = windowLayer.GetComponent<RectTransform>();

        wr.anchorMin = Vector2.zero;
        wr.anchorMax = Vector2.one;
        wr.offsetMin = Vector2.zero;
        wr.offsetMax = Vector2.zero;

        CreateIcons();
    }

    void CreateTaskbar()
    {
        taskbar = new GameObject("Taskbar", typeof(RectTransform));
        taskbar.transform.SetParent(canvas.transform, false);

        RectTransform r = taskbar.GetComponent<RectTransform>();

        r.anchorMin = new Vector2(0, 0);
        r.anchorMax = new Vector2(1, 0);

        r.pivot = new Vector2(0.5f, 0);

        r.sizeDelta = new Vector2(0, 60);

        Image img = taskbar.AddComponent<Image>();
        img.color = new Color(.1f, .1f, .1f);
    }

    void CreateIcons()
    {
        float x = 80;
        float y = -80;
        float step = 110;

        CreateIcon("Dota2", dotaSprite, new Vector2(x, y), OpenDota);
        CreateIcon("Roblox", robloxSprite, new Vector2(x, y - step), OpenRoblox);
        CreateIcon("Сапёр", minesweeperSprite, new Vector2(x, y - step * 2), OpenMinesweeper);
        CreateIcon("Browser", browserSprite, new Vector2(x, y - step * 3), OpenBrowser);
        CreateIcon("Gallery", gallerySprite, new Vector2(x, y - step * 4), OpenGallery);
    }

    void CreateIcon(string name, Sprite sprite, Vector2 pos, UnityEngine.Events.UnityAction action)
    {
        GameObject icon = new GameObject(name, typeof(RectTransform));
        icon.transform.SetParent(desktop.transform, false);

        RectTransform r = icon.GetComponent<RectTransform>();

        r.anchorMin = new Vector2(0, 1);
        r.anchorMax = new Vector2(0, 1);
        r.pivot = new Vector2(0, 1);

        r.sizeDelta = new Vector2(90, 100);
        r.anchoredPosition = pos;

        GameObject imgGO = new GameObject("Icon", typeof(RectTransform));
        imgGO.transform.SetParent(icon.transform, false);

        RectTransform ir = imgGO.GetComponent<RectTransform>();

        ir.anchorMin = new Vector2(0.5f, 1);
        ir.anchorMax = new Vector2(0.5f, 1);

        ir.pivot = new Vector2(0.5f, 1);

        ir.sizeDelta = new Vector2(64, 64);
        ir.anchoredPosition = Vector2.zero;

        Image img = imgGO.AddComponent<Image>();
        img.sprite = sprite ? sprite : defaultIconSprite;
        img.preserveAspect = true;

        Button b = imgGO.AddComponent<Button>();
        b.onClick.AddListener(action);

        GameObject textGO = new GameObject("Text", typeof(RectTransform));
        textGO.transform.SetParent(icon.transform, false);

        RectTransform tr = textGO.GetComponent<RectTransform>();

        tr.anchorMin = new Vector2(0.5f, 1);
        tr.anchorMax = new Vector2(0.5f, 1);
        tr.pivot = new Vector2(0.5f, 1);

        tr.sizeDelta = new Vector2(120, 30);
        tr.anchoredPosition = new Vector2(0, -70);

        TMP_Text text = textGO.AddComponent<TextMeshProUGUI>();
        text.text = name;
        text.fontSize = 18;
        text.alignment = TextAlignmentOptions.Center;
    }

    GameObject CreateWindow(string title, Vector2 size)
    {
        GameObject w = new GameObject(title, typeof(RectTransform));
        w.transform.SetParent(windowLayer.transform, false);
        w.AddComponent<WindowOpenAnim>();

        RectTransform r = w.GetComponent<RectTransform>();

        r.sizeDelta = size;
        r.anchoredPosition = Vector2.zero;

        Image bg = w.AddComponent<Image>();
        bg.color = new Color(.15f, .15f, .18f);

        w.AddComponent<WindowDraggable>();

        GameObject bar = new GameObject("TitleBar", typeof(RectTransform));
        bar.transform.SetParent(w.transform, false);

        RectTransform br = bar.GetComponent<RectTransform>();

        br.anchorMin = new Vector2(0, .9f);
        br.anchorMax = new Vector2(1, 1);
        br.offsetMin = Vector2.zero;
        br.offsetMax = Vector2.zero;

        Image bi = bar.AddComponent<Image>();
        bi.color = new Color(.1f, .1f, .1f);

        TMP_Text t = new GameObject("Text").AddComponent<TextMeshProUGUI>();
        t.transform.SetParent(bar.transform, false);

        t.text = title;
        t.fontSize = 24;
        t.alignment = TextAlignmentOptions.Center;

        RectTransform tr = t.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero;
        tr.offsetMax = Vector2.zero;

        GameObject close = new GameObject("Close", typeof(RectTransform));
        close.transform.SetParent(w.transform, false);

        RectTransform cr = close.GetComponent<RectTransform>();

        cr.anchorMin = new Vector2(1, 1);
        cr.anchorMax = new Vector2(1, 1);

        cr.pivot = new Vector2(1, 1);

        cr.sizeDelta = new Vector2(30, 30);
        cr.anchoredPosition = new Vector2(-5, -5);

        Image ci = close.AddComponent<Image>();
        ci.color = Color.red;

        Button cb = close.AddComponent<Button>();
        cb.onClick.AddListener(() => Destroy(w));

        TMP_Text xt = new GameObject("X").AddComponent<TextMeshProUGUI>();
        xt.transform.SetParent(close.transform, false);

        xt.text = "X";
        xt.alignment = TextAlignmentOptions.Center;
        xt.fontSize = 20;

        RectTransform xr = xt.GetComponent<RectTransform>();
        xr.anchorMin = Vector2.zero;
        xr.anchorMax = Vector2.one;
        xr.offsetMin = Vector2.zero;
        xr.offsetMax = Vector2.zero;

        windows.Add(w);

        return w;
    }

    void OpenDota()
    {
        GameObject w = CreateWindow("Dota2", new Vector2(700, 500));

        Image img = new GameObject("Image").AddComponent<Image>();
        img.transform.SetParent(w.transform, false);
        img.sprite = dotaImage;
        img.preserveAspect = true;

        RectTransform r = img.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(.05f, .05f);
        r.anchorMax = new Vector2(.95f, .85f);
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    void OpenRoblox()
    {
        GameObject w = CreateWindow("Roblox", new Vector2(700, 500));

        Image img = new GameObject("Image").AddComponent<Image>();
        img.transform.SetParent(w.transform, false);
        img.sprite = robloxImage;
        img.preserveAspect = true;

        RectTransform r = img.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(.05f, .05f);
        r.anchorMax = new Vector2(.95f, .85f);
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    void OpenBrowser()
    {
        GameObject w = CreateWindow("Browser", new Vector2(800, 550));

        HorizontalLayoutGroup tabs = new GameObject("Tabs")
            .AddComponent<HorizontalLayoutGroup>();

        tabs.transform.SetParent(w.transform, false);

        RectTransform tr = tabs.GetComponent<RectTransform>();
        tr.anchorMin = new Vector2(.1f, .85f);
        tr.anchorMax = new Vector2(.9f, .9f);
        tr.offsetMin = tr.offsetMax = Vector2.zero;

        tabs.spacing = 10;
        tabs.childAlignment = TextAnchor.MiddleCenter;

        GameObject content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(w.transform, false);

        RectTransform cr = content.GetComponent<RectTransform>();
        cr.anchorMin = new Vector2(.02f, .05f);
        cr.anchorMax = new Vector2(.98f, .83f);
        cr.offsetMin = cr.offsetMax = Vector2.zero;

        void Clear()
        {
            foreach (Transform c in content.transform)
                Destroy(c.gameObject);
        }

        void CreateTab(string name, System.Action open)
        {
            GameObject tab = new GameObject(name);
            tab.transform.SetParent(tabs.transform, false);

            Image img = tab.AddComponent<Image>();
            img.color = new Color(.2f, .2f, .2f);

            Button b = tab.AddComponent<Button>();
            b.onClick.AddListener(() => { Clear(); open(); });

            TMP_Text t = new GameObject("Text").AddComponent<TextMeshProUGUI>();
            t.transform.SetParent(tab.transform, false);

            t.text = name;
            t.alignment = TextAlignmentOptions.Center;

            RectTransform r = t.GetComponent<RectTransform>();
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = r.offsetMax = Vector2.zero;

            RectTransform rt = tab.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(150, 30);
        }

        CreateTab("ChatGPT", () =>
        {
            TMP_Text t = new GameObject("Text").AddComponent<TextMeshProUGUI>();
            t.transform.SetParent(content.transform, false);

            t.text = "ERROR 404";
            t.fontSize = 60;
            t.alignment = TextAlignmentOptions.Center;

            RectTransform r = t.GetComponent<RectTransform>();
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = r.offsetMax = Vector2.zero;
        });
        Clear();
        CreateTab("Rutube", () =>
        {
            if (rutubePreviews == null || rutubeTitles == null) return;

            GameObject scrollGO = new GameObject("Scroll", typeof(RectTransform));
            scrollGO.transform.SetParent(content.transform, false);

            RectTransform sr = scrollGO.GetComponent<RectTransform>();
            sr.anchorMin = Vector2.zero;
            sr.anchorMax = Vector2.one;
            sr.offsetMin = Vector2.zero;
            sr.offsetMax = Vector2.zero;

            Image bg = scrollGO.AddComponent<Image>();
            bg.color = new Color(.12f, .12f, .12f);

            ScrollRect scroll = scrollGO.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.scrollSensitivity = 10f;
            scroll.decelerationRate = 0.05f;
            scroll.inertia = true;

            GameObject viewport = new GameObject("Viewport", typeof(RectTransform));
            viewport.transform.SetParent(scrollGO.transform, false);

            RectTransform vr = viewport.GetComponent<RectTransform>();
            vr.anchorMin = Vector2.zero;
            vr.anchorMax = Vector2.one;
            vr.offsetMin = Vector2.zero;
            vr.offsetMax = Vector2.zero;

            Image vimg = viewport.AddComponent<Image>();
            vimg.color = new Color(0, 0, 0, 0.01f);

            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            GameObject list = new GameObject("Content", typeof(RectTransform));
            list.transform.SetParent(viewport.transform, false);

            RectTransform lr = list.GetComponent<RectTransform>();
            lr.anchorMin = new Vector2(0, 1);
            lr.anchorMax = new Vector2(1, 1);
            lr.pivot = new Vector2(0.5f, 1);
            lr.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup layout = list.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 25;
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = list.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = vr;
            scroll.content = lr;

            int count = Mathf.Min(rutubePreviews.Length, rutubeTitles.Length);

            for (int i = 0; i < count; i++)
            {
                GameObject video = new GameObject("Video" + i, typeof(RectTransform));
                video.transform.SetParent(list.transform, false);

                LayoutElement le = video.AddComponent<LayoutElement>();
                le.preferredHeight = 320;

                VerticalLayoutGroup v = video.AddComponent<VerticalLayoutGroup>();
                v.spacing = 10;

                Image preview = new GameObject("Preview").AddComponent<Image>();
                preview.transform.SetParent(video.transform, false);
                preview.sprite = rutubePreviews[i];
                preview.preserveAspect = true;

                LayoutElement ple = preview.gameObject.AddComponent<LayoutElement>();
                ple.preferredHeight = 240;

                TMP_Text title = new GameObject("Title").AddComponent<TextMeshProUGUI>();
                title.transform.SetParent(video.transform, false);
                title.text = rutubeTitles[i];
                title.fontSize = 32;
                title.alignment = TextAlignmentOptions.Center;
            }
        });

        CreateTab("Instagram", () =>
        {
            TMP_Text t = new GameObject("Text").AddComponent<TextMeshProUGUI>();
            t.transform.SetParent(content.transform, false);

            t.text = "Недоступно в вашей стране";
            t.fontSize = 40;
            t.alignment = TextAlignmentOptions.Center;

            RectTransform r = t.GetComponent<RectTransform>();
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = r.offsetMax = Vector2.zero;
        });
        Clear();
    }

    void OpenGallery()
    {
        GameObject w = CreateWindow("Gallery", new Vector2(700, 500));

        GridLayoutGroup grid = new GameObject("Grid").AddComponent<GridLayoutGroup>();
        grid.transform.SetParent(w.transform, false);

        RectTransform r = grid.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(.05f, .05f);
        r.anchorMax = new Vector2(.95f, .85f);
        r.offsetMin = r.offsetMax = Vector2.zero;

        grid.cellSize = new Vector2(150, 120);
        grid.spacing = new Vector2(10, 10);

        foreach (Sprite photo in galleryPhotos)
        {
            GameObject img = new GameObject(photo.name);
            img.transform.SetParent(grid.transform, false);

            Image i = img.AddComponent<Image>();
            i.sprite = photo;
            i.preserveAspect = true;

            Button b = img.AddComponent<Button>();
            b.onClick.AddListener(() => OpenPhoto(photo));
        }
    }

    void OpenPhoto(Sprite photo)
    {
        GameObject w = CreateWindow(photo.name, new Vector2(600, 500));

        GameObject imgGO = new GameObject("Photo", typeof(RectTransform));
        imgGO.transform.SetParent(w.transform, false);

        RectTransform r = imgGO.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(.05f, .05f);
        r.anchorMax = new Vector2(.95f, .9f);
        r.offsetMin = r.offsetMax = Vector2.zero;

        Image img = imgGO.AddComponent<Image>();
        img.sprite = photo;
        img.preserveAspect = true;

        Button zoom = imgGO.AddComponent<Button>();
        zoom.onClick.AddListener(() =>
        {
            if (r.localScale == Vector3.one)
                r.localScale = Vector3.one * 1.8f;
            else
                r.localScale = Vector3.one;
        });
    }

    void OpenMinesweeper()
    {
        GameObject w = CreateWindow("Сапёр", new Vector2(600, 500));

        GridLayoutGroup grid = new GameObject("Grid").AddComponent<GridLayoutGroup>();
        grid.transform.SetParent(w.transform, false);

        RectTransform r = grid.GetComponent<RectTransform>();

        r.anchorMin = new Vector2(.5f, .5f);
        r.anchorMax = new Vector2(.5f, .5f);
        r.pivot = new Vector2(.5f, .5f);

        r.sizeDelta = new Vector2(310, 310);
        r.anchoredPosition = new Vector2(0, -40);

        grid.cellSize = new Vector2(32, 32);
        grid.spacing = new Vector2(2, 2);

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 9;

        Minesweeper m = grid.gameObject.AddComponent<Minesweeper>();
        m.Init(grid, this);
        m.mineSprite = mineIcon;
        m.flagSprite = flagIcon;
    }

    public void ShowAbilityUnlock(string ability)
    {
        GameObject w = CreateWindow("Способность", new Vector2(400, 200));

        TMP_Text t = new GameObject("Text").AddComponent<TextMeshProUGUI>();
        t.transform.SetParent(w.transform, false);

        t.text = "Разблокировано: Ускорение";

        t.fontSize = 28;
        t.color = Color.yellow;

        t.alignment = TextAlignmentOptions.Center;

        RectTransform r = t.GetComponent<RectTransform>();

        r.anchorMin = new Vector2(.05f, .2f);
        r.anchorMax = new Vector2(.95f, .8f);

        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
    }

    public void ShowMinesweeperLose()
    {
        GameObject w = CreateWindow("Поражение", new Vector2(400, 200));

        TMP_Text t = new GameObject("Text").AddComponent<TextMeshProUGUI>();
        t.transform.SetParent(w.transform, false);

        t.text = "Вы подорвались на мине";

        t.fontSize = 36;
        t.alignment = TextAlignmentOptions.Center;

        RectTransform r = t.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(.1f, .2f);
        r.anchorMax = new Vector2(.9f, .8f);
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    public void UnlockAbilityByTag(string tag)
    {
        var pug = player.GetComponent<Управлениемопсом>();

        if (tag == "SprintBlock")
        {
            pug.UnlockSprint();
            ShowAbilityUnlock("Sprint");
        }
    }
}