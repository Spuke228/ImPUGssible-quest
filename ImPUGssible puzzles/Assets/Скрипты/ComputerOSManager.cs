using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AppData
{
    public string appName;
    public Sprite iconSprite;
    public Func<GameObject> createWindowFunc;
}

public class ComputerOSManager : MonoBehaviour
{
    [Header("Player")]
    public Transform player;
    public Управлениемопсом pugController;
    public MonoBehaviour cameraController;

    [Header("Monitor")]
    public Transform monitor;
    public float interactDistance = 3f;

    [Header("UI Settings")]
    public Color desktopColor = new Color(0.1f, 0.15f, 0.25f);
    public Color taskbarColor = new Color(0.05f, 0.05f, 0.05f, 0.7f);
    public AppData[] apps;

    private Canvas canvas;
    private RectTransform desktop;
    private RectTransform iconsContainer;
    private RectTransform taskbar;
    private RectTransform runningApps;

    private Dictionary<string, GameObject> openedApps = new Dictionary<string, GameObject>();
    private GameObject currentPopup;
    private bool computerOpened = false;

    void Start()
    {
        computerOpened = false; // Canvas создаётся позже
    }

    void Update()
    {
        if (!computerOpened && monitor != null)
        {
            if (Vector3.Distance(player.position, monitor.position) <= interactDistance && Input.GetKeyDown(KeyCode.E))
            {
                OpenComputer();
            }
        }
    }

    void OpenComputer()
    {
        if (canvas == null)
        {
            CreateCanvas();
            CreateDesktop();
            CreateIconsContainer();
            CreateTaskbar();
            CreateIcons();
        }

        canvas.gameObject.SetActive(true);
        computerOpened = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseComputer()
    {
        if (canvas != null)
            canvas.gameObject.SetActive(false);

        computerOpened = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    #region CREATE UI

    void CreateCanvas()
    {
        GameObject canvasGO = new GameObject("ComputerCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.layer = LayerMask.NameToLayer("UI");
        canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler cs = canvasGO.GetComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        cs.matchWidthOrHeight = 0.5f;
    }

    void CreateDesktop()
    {
        GameObject desktopGO = new GameObject("Desktop", typeof(RectTransform), typeof(Image));
        desktopGO.transform.SetParent(canvas.transform, false);
        desktop = desktopGO.GetComponent<RectTransform>();
        desktop.anchorMin = Vector2.zero;
        desktop.anchorMax = Vector2.one;
        desktop.offsetMin = Vector2.zero;
        desktop.offsetMax = Vector2.zero;

        Image bg = desktopGO.GetComponent<Image>();
        bg.color = desktopColor;
    }

    void CreateIconsContainer()
    {
        GameObject containerGO = new GameObject("Icons", typeof(RectTransform), typeof(GridLayoutGroup));
        containerGO.transform.SetParent(desktop, false);
        iconsContainer = containerGO.GetComponent<RectTransform>();
        iconsContainer.anchorMin = new Vector2(0, 0);
        iconsContainer.anchorMax = new Vector2(0, 1);
        iconsContainer.pivot = new Vector2(0, 1);
        iconsContainer.anchoredPosition = new Vector2(20, -20);
        iconsContainer.sizeDelta = new Vector2(400, 800);

        GridLayoutGroup grid = containerGO.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(80, 80);
        grid.spacing = new Vector2(10, 10);
        grid.startAxis = GridLayoutGroup.Axis.Vertical;
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
    }

    void CreateTaskbar()
    {
        GameObject taskbarGO = new GameObject("Taskbar", typeof(RectTransform), typeof(Image));
        taskbarGO.transform.SetParent(canvas.transform, false);
        taskbar = taskbarGO.GetComponent<RectTransform>();
        taskbar.anchorMin = new Vector2(0, 0);
        taskbar.anchorMax = new Vector2(1, 0);
        taskbar.pivot = new Vector2(0.5f, 0);
        taskbar.sizeDelta = new Vector2(0, 60);

        Image img = taskbarGO.GetComponent<Image>();
        img.color = taskbarColor;

        GameObject runningGO = new GameObject("RunningApps", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        runningGO.transform.SetParent(taskbarGO.transform, false);
        runningApps = runningGO.GetComponent<RectTransform>();
        runningApps.anchorMin = new Vector2(0, 0);
        runningApps.anchorMax = new Vector2(1, 1);
        runningApps.offsetMin = new Vector2(10, 10);
        runningApps.offsetMax = new Vector2(-10, -10);
    }

    void CreateIcons()
    {
        foreach (var app in apps)
        {
            CreateIcon(app);
        }
    }

    void CreateIcon(AppData app)
    {
        GameObject iconGO = new GameObject(app.appName + "Icon", typeof(RectTransform), typeof(Button), typeof(Image));
        iconGO.transform.SetParent(iconsContainer, false);
        Image img = iconGO.GetComponent<Image>();
        img.sprite = app.iconSprite;
        img.color = Color.white;

        RectTransform rt = iconGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(80, 80);

        Button btn = iconGO.GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            if (openedApps.ContainsKey(app.appName))
            {
                openedApps[app.appName].SetActive(true);
                return;
            }
            GameObject w = app.createWindowFunc.Invoke();
            openedApps[app.appName] = w;
            AddToTaskbar(app.appName, w);
        });

        // TMP текст под иконкой
        GameObject textGO = new GameObject("Text", typeof(RectTransform));
        textGO.transform.SetParent(iconGO.transform, false);
        TMP_Text txt = textGO.AddComponent<TMP_Text>();
        txt.text = app.appName;
        txt.fontSize = 20;
        txt.alignment = TextAlignmentOptions.Bottom;
        txt.color = Color.white;
        RectTransform txtRT = txt.rectTransform;
        txtRT.anchorMin = new Vector2(0, 0);
        txtRT.anchorMax = new Vector2(1, 0.3f);
        txtRT.offsetMin = Vector2.zero;
        txtRT.offsetMax = Vector2.zero;
    }

    void AddToTaskbar(string name, GameObject window)
    {
        GameObject btnGO = new GameObject(name + "TaskbarButton", typeof(RectTransform), typeof(Button), typeof(Image));
        btnGO.transform.SetParent(runningApps, false);
        Image img = btnGO.GetComponent<Image>();
        img.color = new Color(0.8f, 0.8f, 0.8f);

        Button btn = btnGO.GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            bool active = window.activeSelf;
            window.SetActive(!active);
        });

        GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(TMP_Text));
        textGO.transform.SetParent(btnGO.transform, false);
        TMP_Text txt = textGO.GetComponent<TMP_Text>();
        txt.text = name;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.black;
        txt.rectTransform.sizeDelta = new Vector2(100, 40);
    }

    #endregion

    #region WINDOWS + CLOSE BUTTON

    public GameObject CreateWindow(string title, Vector2 size, Color bgColor)
    {
        GameObject panel = new GameObject(title + "Window", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvas.transform, false);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        panel.AddComponent<WindowDraggable>();
        panel.GetComponent<Image>().color = bgColor;

        // Заголовок
        GameObject header = new GameObject("Header", typeof(RectTransform), typeof(Image));
        header.transform.SetParent(panel.transform, false);
        RectTransform hrt = header.GetComponent<RectTransform>();
        hrt.anchorMin = new Vector2(0, 1);
        hrt.anchorMax = new Vector2(1, 1);
        hrt.pivot = new Vector2(0.5f, 1);
        hrt.sizeDelta = new Vector2(0, 30);
        Image hdrImg = header.GetComponent<Image>();
        hdrImg.color = Color.gray;

        GameObject textGO = new GameObject("Title", typeof(RectTransform));
        textGO.transform.SetParent(header.transform, false);
        Text txt = textGO.AddComponent<Text>();
        txt.text = title;
        txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.alignment = TextAnchor.MiddleLeft;
        txt.color = Color.white;
        RectTransform txtRT = txt.rectTransform;
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = new Vector2(10, 0);
        txtRT.offsetMax = new Vector2(-30, 0);

        GameObject closeBtnGO = new GameObject("CloseButton", typeof(RectTransform), typeof(Button), typeof(Image));
        closeBtnGO.transform.SetParent(header.transform, false);
        RectTransform cRT = closeBtnGO.GetComponent<RectTransform>();
        cRT.anchorMin = new Vector2(1, 0);
        cRT.anchorMax = new Vector2(1, 1);
        cRT.pivot = new Vector2(1, 0.5f);
        cRT.sizeDelta = new Vector2(30, 30);
        Image cImg = closeBtnGO.GetComponent<Image>();
        cImg.color = Color.red;

        Button closeBtn = closeBtnGO.GetComponent<Button>();
        closeBtn.onClick.AddListener(() =>
        {
            panel.SetActive(false);
        });

        return panel;
    }

    #endregion

    #region POPUP

    public void ShowAbilityUnlock(string abilityName)
    {
        if (canvas == null) return;

        if (currentPopup != null) Destroy(currentPopup);

        GameObject panel = new GameObject("AbilityPopup", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvas.transform, false);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 80);
        rt.anchoredPosition = new Vector2(960, 900);

        Image img = panel.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0.7f);

        GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(TMP_Text));
        textGO.transform.SetParent(panel.transform, false);
        TMP_Text tmp = textGO.GetComponent<TMP_Text>();
        tmp.text = $"Ability Unlocked: {abilityName}";
        tmp.fontSize = 36;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.rectTransform.sizeDelta = rt.sizeDelta;
        tmp.rectTransform.anchoredPosition = Vector2.zero;

        currentPopup = panel;

        StartCoroutine(HidePopupAfterDelay(panel, 3f));
    }

    private IEnumerator HidePopupAfterDelay(GameObject popup, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (popup != null) Destroy(popup);
    }

    #endregion
}