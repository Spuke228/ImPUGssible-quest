using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("References (scene-independent)")]
    [SerializeField] private GameObject dogPrefab; // prefab (can be UI element or world object)

    // Scene-specific overlay (found on scene load). Not serialized for persistent linking.
    private Image brightnessOverlay;

    private GameObject dogInstance;

    private bool isFullscreen = false;
    private float brightness = 1f; // [0..1]
    private bool showDog = false;

    // Event UI can subscribe to in order to refresh widgets when settings change or scene loads
    public event Action OnSettingsUpdated;

    float musicVolume = 0.2f;

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);

        UISoundManager sm = FindObjectOfType<UISoundManager>();
        if (sm != null)
            sm.SetMusicVolume(musicVolume);
    }

    public float GetMusicVolume() => musicVolume;

    bool isMuted = false;

    public void SetMuted(bool muted)
    {
        isMuted = muted;

        UISoundManager sm = FindObjectOfType<UISoundManager>();
        if (sm != null)
            sm.SetMuted(isMuted);

        OnSettingsUpdated?.Invoke();
    }

    public bool IsMuted() => isMuted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Subscribe to scene loaded to re-find scene-specific objects (overlay, canvas, etc.)
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // Apply initial values to the current scene (if overlay exists)
        FindBrightnessOverlayInScene();
        ApplySettings();
    }

    private void OnDestroy()
    {
        // Clean up static reference and event subscriptions
        if (Instance == this)
            Instance = null;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Try to find brightness overlay in the newly loaded scene
        FindBrightnessOverlayInScene();

        // If dog is shown, ensure the dog instance exists in the new scene (DontDestroyOnLoad keeps it)
        // but if dog's UI depends on scene canvas, we may want to re-parent it. We'll re-parent if needed.
        if (dogInstance != null)
            TryReparentDogToCanvas();

        ApplySettings();

        // Notify any UI scripts in the scene to refresh their widgets
        OnSettingsUpdated?.Invoke();
    }

    #region Public API (used by UI)
    // These are the proper entry points for UI widgets
    public void SetFullscreen(bool fullscreen)
    {
        if (isFullscreen == fullscreen) return;
        isFullscreen = fullscreen;
        ApplySettings();
        OnSettingsUpdated?.Invoke();
    }

    public void SetBrightness(float value)
    {
        value = Mathf.Clamp01(value);
        if (Mathf.Approximately(brightness, value)) return;
        brightness = value;
        ApplySettings();
        OnSettingsUpdated?.Invoke();
    }

    public void SetShowDog(bool show)
    {
        if (showDog == show) return;
        showDog = show;
        ApplySettings();
        OnSettingsUpdated?.Invoke();
    }

    public bool IsFullscreen() => isFullscreen;
    public float GetBrightness() => brightness;
    public bool IsDogShown() => showDog;
    #endregion

    public void ResetToDefaults()
    {
        isFullscreen = false;
        brightness = 1f;
        showDog = false;

        ApplySettings();
        OnSettingsUpdated?.Invoke();

        Debug.Log("⚙️ Настройки сброшены (только для текущей сессии)");
    }

    private void ApplySettings()
    {
        // Fullscreen
        Screen.fullScreen = isFullscreen;

        // Brightness: set overlay alpha so alpha = 1 - brightness
        if (brightnessOverlay != null)
        {
            // Keep original RGB (usually black), only change alpha
            Color c = brightnessOverlay.color;
            c.a = 1f - Mathf.Clamp01(brightness);
            brightnessOverlay.color = c;
        }

        // Dog: create/destroy or show/hide instance
        HandleDog(showDog);
    }

    #region Dog handling
    private void HandleDog(bool show)
    {
        if (dogPrefab == null) return;

        if (show)
        {
            if (dogInstance == null)
            {
                dogInstance = Instantiate(dogPrefab);
                DontDestroyOnLoad(dogInstance);
                TryReparentDogToCanvas();

                RectTransform rect = dogInstance.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(1f, 1f);
                    rect.anchorMax = new Vector2(1f, 1f);
                    rect.pivot = new Vector2(1f, 1f);
                    rect.anchoredPosition = Vector2.zero; // ← без отступов
                    rect.localScale = Vector3.one;        // ← не уменьшать

                    // Зафиксируем размер (если Canvas имеет auto-scaling)
                    rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.rect.width);
                    rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rect.rect.height);
                }
            }
            else
            {
                dogInstance.SetActive(true);
                TryReparentDogToCanvas();
            }
        }
        else
        {
            if (dogInstance != null)
            {
                Destroy(dogInstance);
                dogInstance = null;
            }
        }
    }

    // If dogInstance is a UI element, attempt to parent it to a Canvas in the current scene
    private void TryReparentDogToCanvas()
    {
        if (dogInstance == null) return;

        RectTransform dogRect = dogInstance.GetComponent<RectTransform>();
        if (dogRect == null) return; // not UI element

        // Find a Canvas in the active scenes (prefer root canvases)
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            dogRect.SetParent(canvas.transform, worldPositionStays: false);
            dogRect.localScale = Vector3.one;
        }
    }
    #endregion

    #region Brightness overlay finder
    private void FindBrightnessOverlayInScene()
    {
        brightnessOverlay = null;

        // 1) try to find by name
        GameObject named = GameObject.Find("BrightnessOverlay");
        if (named != null)
        {
            brightnessOverlay = named.GetComponent<Image>();
            if (brightnessOverlay != null) return;
        }

        // 2) try to find by tag (user can set tag "BrightnessOverlay")
        try
        {
            GameObject tagged = GameObject.FindWithTag("BrightnessOverlay");
            if (tagged != null)
            {
                brightnessOverlay = tagged.GetComponent<Image>();
                if (brightnessOverlay != null) return;
            }
        }
        catch
        {
            // in case tag doesn't exist - ignore
        }

        // 3) fallback: find first Image in scene (including inactive if API supports it)
#if UNITY_2020_1_OR_NEWER
        // FindObjectsOfType has includeInactive parameter in newer Unity versions
        Image[] images = FindObjectsOfType<Image>(includeInactive: true);
#else
        Image[] images = FindObjectsOfType<Image>();
#endif
        if (images != null && images.Length > 0)
        {
            // Prefer overlays (full-screen images) — crude heuristic: rect size covers screen?
            foreach (var img in images)
            {
                RectTransform rt = img.GetComponent<RectTransform>();
                if (rt != null && Mathf.Approximately(rt.anchorMin.x, 0f) && Mathf.Approximately(rt.anchorMax.x, 1f))
                {
                    brightnessOverlay = img;
                    break;
                }
            }

            if (brightnessOverlay == null)
                brightnessOverlay = images[0];
        }
    }
    #endregion
}
