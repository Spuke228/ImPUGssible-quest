using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.Video;

[RequireComponent(typeof(CharacterController))]
public class Управлениемопсом : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public float rotationSpeed = 10f;

    [Header("Jumping")]
    public float gravity = 9.81f;
    private float normalGravity;
    public float jumpHeight = 2f;
    public float jumpCooldown = 1f;
    private float lastJumpTime = -999f;
    private bool canJump = false;

    [Header("Slow Fall Ability")]
    public float slowFallDuration = 3f;
    public float slowFallGravity = 2f;
    public float slowFallCooldown = 5f;
    private float lastSlowFallTime = -999f;
    private float slowFallTimer = 0f;
    private bool isSlowFalling = false;
    private bool canSlowFall = false;

    [Header("Sprint Ability")]
    public float sprintDuration = 3f;
    public float sprintMultiplier = 1.8f;
    public float sprintCooldown = 5f;
    private float sprintTimer = 0f;
    private float lastSprintTime = -999f;
    private bool isSprinting = false;
    private bool canSprint = false;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float stamina = 100f;
    public float staminaRegenRate = 15f;
    public float staminaDrainRate = 10f;

    [Header("Sleepiness")]
    public float maxSleepiness = 100f;
    public float sleepiness = 0f;
    public Color sleepinessColor = new Color(0.2f, 0, 0.5f, 1f);
    public Vector2 sleepinessBarSize = new Vector2(500, 70); // width, height
    public Vector2 sleepinessBarPosition = new Vector2(10, 10); // offset from bottom-left
    public float sleepinessPassiveRate = 2f;

    [Header("Rest Ability")]
    public float restAmount = 30f;
    public float restCooldown = 10f;
    private float lastRestTime = -999f;
    private Image restButtonImage;
    public Sprite restSprite;
    public Vector2 restButtonSize = new Vector2(320, 320);

    [Header("Camera")]
    public Transform cameraTransform;
    public float mouseSensitivity = 2f;
    public float minY = -60f;
    public float maxY = 80f;
    public float minDistance = 1.5f;
    public float maxDistance = 5f;
    public float zoomSpeed = 2f;
    public Vector3 cameraOffset = new Vector3(0, 1.5f, 0);

    [Header("UI Colors")]
    public Color readyColor = Color.green;
    public Color notReadyColor = Color.red;
    public Color staminaDefaultColor = Color.yellow;

    [Header("UI Buttons (sizes are adjustable)")]
    public Vector2 jumpButtonSize = new Vector2(300, 300);
    public Vector2 slowFallButtonSize = new Vector2(300, 300);
    public Vector2 sprintButtonSize = new Vector2(320, 320);
    public Sprite jumpSprite;
    public Sprite slowFallSprite;
    public Sprite sprintSprite;

    [Header("Stamina UI Settings")]
    public Vector2 staminaBarSize = new Vector2(500, 70);
    public Vector2 staminaBarPosition = new Vector2(10, 90); // offset from bottom-left
    public bool fillStaminaFromLeft = true;

    [Header("Sleepiness Blackout Settings")]
    public float blackoutDuration = 20f;
    public RawImage videoRawImage; // привязать в инспекторе, на нем должен быть VideoPlayer
    public float fadeDuration = 1f; // длительность плавного затемнения
    public Image blackoutImage; // черное затемнение поверх всего экрана
    public float blackoutAnimDuration = 1f; // длина анимации обморока
    private Animator animator;

    [Header("Icons (left of bars)")]
    public float staminaIconOffsetX = 0f;
    public float staminaIconOffsetY = 0f;
    public float sleepinessIconOffsetX = 0f;
    public float sleepinessIconOffsetY = 0f;
    public Sprite staminaIconSprite;
    public Vector2 staminaIconSize = new Vector2(70, 70);
    public Sprite sleepinessIconSprite;
    public Vector2 sleepinessIconSize = new Vector2(70, 70);
    public float iconPadding = 8f; // gap between icon and bar

    // internal
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private Image jumpButtonImage;
    private Image slowFallButtonImage;
    private Image sprintButtonImage;
    private Image staminaBarImage;         // the fill image for the bar
    private Image sleepinessBarImage;      // the fill image for the bar
    private Image staminaIconImage;        // icon image
    private Image sleepinessIconImage;     // icon image
    private RectTransform staminaContainerRect;   // container that holds icon + bar
    private RectTransform sleepinessContainerRect;
    private Transform cameraPivot;
    private float yaw = 0f;
    private float pitch = 0f;
    private float cameraDistance;
    private bool hasReachedApex = false;
    private bool isBlackout = false;
    private float fallStartY;
    private bool isFalling = false;
    public float minFallHeight = 3f;
    public float fallSleepinessMultiplier = 10f;
    private bool canRest = false;
    public bool canMove = true;

    // top button container
    private RectTransform topButtonsContainer;

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        normalGravity = gravity;
        cameraDistance = maxDistance;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Camera pivot creation
        GameObject pivotGO = new GameObject("CameraPivot");
        cameraPivot = pivotGO.transform;
        cameraPivot.position = transform.position + cameraOffset;

        // Canvas: find or create and configure scaler for flexible sizing
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas", typeof(RectTransform));
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler cs = canvasGO.AddComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1920, 1080);
            cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            cs.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            RectTransform rect = canvasGO.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        else
        {
            CanvasScaler cs = canvas.GetComponent<CanvasScaler>();
            if (cs == null)
            {
                cs = canvas.gameObject.AddComponent<CanvasScaler>();
                cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                cs.referenceResolution = new Vector2(1920, 1080);
                cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                cs.matchWidthOrHeight = 0.5f;
            }
        }

        // Create top-left container for buttons with HorizontalLayoutGroup so sizes auto-update
        topButtonsContainer = CreateTopButtonsContainer(canvas.transform);

        // Create buttons as children of the container
        jumpButtonImage = CreateButton(topButtonsContainer, "Jump", jumpSprite, jumpButtonSize, OnJumpButtonClicked);
        slowFallButtonImage = CreateButton(topButtonsContainer, "SlowFall", slowFallSprite, slowFallButtonSize, OnSlowFallButtonClicked);
        sprintButtonImage = CreateButton(topButtonsContainer, "Sprint", sprintSprite, sprintButtonSize, OnSprintButtonClicked);
        restButtonImage = CreateButton(topButtonsContainer, "Rest", restSprite, restButtonSize, OnRestButtonClicked);

        // Create bars with icons anchored bottom-left
        CreateStaminaBarWithIcon(canvas.transform);
        CreateSleepinessBarWithIcon(canvas.transform);

        if (blackoutImage != null)
        {
            blackoutImage.transform.SetAsLastSibling();
            RectTransform rt = blackoutImage.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            blackoutImage.color = new Color(0f, 0f, 0f, 0f);
            blackoutImage.gameObject.SetActive(false);
        }
    }

    // Creates a top-left anchored container using HorizontalLayoutGroup
    RectTransform CreateTopButtonsContainer(Transform canvasTransform)
    {
        GameObject go = new GameObject("TopButtonsContainer", typeof(RectTransform));
        go.transform.SetParent(canvasTransform, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(10f, -10f); // top-left with padding

        HorizontalLayoutGroup h = go.AddComponent<HorizontalLayoutGroup>();
        h.childForceExpandHeight = false;
        h.childForceExpandWidth = false;
        h.childControlHeight = false;
        h.childControlWidth = false;
        h.spacing = 10f;
        h.padding = new RectOffset(0, 0, 0, 0);

        ContentSizeFitter csf = go.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        return rect;
    }

    // Create a top-left anchored button as a child of provided parent (layout group uses LayoutElement)
    Image CreateButton(Transform parent, string name, Sprite sprite, Vector2 size, UnityAction onClick)
    {
        GameObject buttonGO = new GameObject(name + "Button");
        buttonGO.transform.SetParent(parent, false);

        Button btn = buttonGO.AddComponent<Button>();
        Image img = buttonGO.AddComponent<Image>();
        if (sprite != null) img.sprite = sprite;
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Radial360;
        img.fillOrigin = (int)Image.Origin360.Top;
        img.fillClockwise = true;
        img.fillAmount = 1f;
        img.color = readyColor;

        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        RectTransform rect = buttonGO.GetComponent<RectTransform>();
        LayoutElement le = buttonGO.AddComponent<LayoutElement>();
        le.preferredWidth = size.x;
        le.preferredHeight = size.y;
        le.minWidth = size.x;
        le.minHeight = size.y;
        le.flexibleWidth = 0;
        le.flexibleHeight = 0;
        rect.sizeDelta = size;
        img.type = Image.Type.Simple;
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        img.preserveAspect = false;

        // ensure layout rebuilt to place new button correctly
        LayoutRebuilder.ForceRebuildLayoutImmediate(parent as RectTransform);

        return img;
    }

    // Create a container at bottom-left that holds icon (left) + bar (right)
    void CreateStaminaBarWithIcon(Transform canvasTransform)
    {
        GameObject container = new GameObject("StaminaBarContainer", typeof(RectTransform));
        container.transform.SetParent(canvasTransform, false);
        staminaContainerRect = container.GetComponent<RectTransform>();
        staminaContainerRect.anchorMin = new Vector2(0, 0);
        staminaContainerRect.anchorMax = new Vector2(0, 0);
        staminaContainerRect.pivot = new Vector2(0, 0);
        staminaContainerRect.anchoredPosition = staminaBarPosition;

        // icon
        GameObject iconGO = new GameObject("StaminaIcon", typeof(RectTransform));
        iconGO.transform.SetParent(container.transform, false);
        staminaIconImage = iconGO.AddComponent<Image>();
        if (staminaIconSprite != null) staminaIconImage.sprite = staminaIconSprite;
        RectTransform iconRect = iconGO.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0, 0.5f);
        iconRect.anchorMax = new Vector2(0, 0.5f);
        iconRect.pivot = new Vector2(0, 0.5f);
        iconRect.anchoredPosition = new Vector2(0, 0);
        iconRect.sizeDelta = staminaIconSize;
        iconRect.anchoredPosition += new Vector2(staminaIconOffsetX, staminaIconOffsetY);

        // bar
        GameObject barGO = new GameObject("StaminaBar", typeof(RectTransform));
        barGO.transform.SetParent(container.transform, false);
        staminaBarImage = barGO.AddComponent<Image>();
        staminaBarImage.color = staminaDefaultColor;
        staminaBarImage.type = Image.Type.Filled;
        staminaBarImage.fillMethod = Image.FillMethod.Horizontal;
        staminaBarImage.fillOrigin = fillStaminaFromLeft ? (int)Image.OriginHorizontal.Left : (int)Image.OriginHorizontal.Right;
        RectTransform barRect = barGO.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0, 0.5f);
        barRect.anchorMax = new Vector2(0, 0.5f);
        barRect.pivot = new Vector2(0, 0.5f);
        barRect.anchoredPosition = new Vector2(staminaIconSize.x + iconPadding, 0);
        barRect.sizeDelta = staminaBarSize;

        // set container size to encompass icon + padding + bar
        float containerWidth = staminaIconSize.x + iconPadding + staminaBarSize.x;
        float containerHeight = Mathf.Max(staminaIconSize.y, staminaBarSize.y);
        staminaContainerRect.sizeDelta = new Vector2(containerWidth, containerHeight);
    }

    void CreateSleepinessBarWithIcon(Transform canvasTransform)
    {
        GameObject container = new GameObject("SleepinessBarContainer", typeof(RectTransform));
        container.transform.SetParent(canvasTransform, false);
        sleepinessContainerRect = container.GetComponent<RectTransform>();
        sleepinessContainerRect.anchorMin = new Vector2(0, 0);
        sleepinessContainerRect.anchorMax = new Vector2(0, 0);
        sleepinessContainerRect.pivot = new Vector2(0, 0);
        sleepinessContainerRect.anchoredPosition = sleepinessBarPosition;

        // icon
        GameObject iconGO = new GameObject("SleepinessIcon", typeof(RectTransform));
        iconGO.transform.SetParent(container.transform, false);
        sleepinessIconImage = iconGO.AddComponent<Image>();
        if (sleepinessIconSprite != null) sleepinessIconImage.sprite = sleepinessIconSprite;
        RectTransform iconRect = iconGO.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0, 0.5f);
        iconRect.anchorMax = new Vector2(0, 0.5f);
        iconRect.pivot = new Vector2(0, 0.5f);
        iconRect.anchoredPosition = new Vector2(0, 0);
        iconRect.sizeDelta = sleepinessIconSize;
        iconRect.anchoredPosition += new Vector2(sleepinessIconOffsetX, sleepinessIconOffsetY);

        // bar
        GameObject barGO = new GameObject("SleepinessBar", typeof(RectTransform));
        barGO.transform.SetParent(container.transform, false);
        sleepinessBarImage = barGO.AddComponent<Image>();
        sleepinessBarImage.color = sleepinessColor;
        sleepinessBarImage.type = Image.Type.Filled;
        sleepinessBarImage.fillMethod = Image.FillMethod.Horizontal;
        sleepinessBarImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        RectTransform barRect = barGO.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0, 0.5f);
        barRect.anchorMax = new Vector2(0, 0.5f);
        barRect.pivot = new Vector2(0, 0.5f);
        barRect.anchoredPosition = new Vector2(sleepinessIconSize.x + iconPadding, 0);
        barRect.sizeDelta = sleepinessBarSize;

        float containerWidth = sleepinessIconSize.x + iconPadding + sleepinessBarSize.x;
        float containerHeight = Mathf.Max(sleepinessIconSize.y, sleepinessBarSize.y);
        sleepinessContainerRect.sizeDelta = new Vector2(containerWidth, containerHeight);
    }

    void Update()
    {
        if (Time.timeScale == 0f)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        HandleCamera();
        HandleMovement();
        UpdateCooldownUI();
        UpdateStaminaUI();
        UpdateSleepinessUI();


        if (Input.GetKeyDown(KeyCode.Space)) TryJump();
        if (Input.GetKeyDown(KeyCode.F)) TrySlowFall();
        if (Input.GetKeyDown(KeyCode.LeftShift)) TrySprint();
        if (Input.GetKeyDown(KeyCode.R)) TryRest();

        if (!isBlackout)
        {
            sleepiness += sleepinessPassiveRate * Time.deltaTime;
            sleepiness = Mathf.Clamp(sleepiness, 0f, maxSleepiness);

            if (sleepiness >= maxSleepiness && !isBlackout)
                StartCoroutine(BlackoutCoroutine());
        }
        if (sleepiness >= maxSleepiness && !isBlackout)
            StartCoroutine(BlackoutCoroutine());
    }

    void HandleMovement()
    {
        if (!canMove) return;

        if (!isGrounded && velocity.y < 0f && !isFalling)
        {
            isFalling = true;
            fallStartY = transform.position.y;
        }

        if (isGrounded && isFalling)
        {
            float fallHeight = fallStartY - transform.position.y;
            if (fallHeight > minFallHeight && !isSlowFalling)
            {
                float addedSleepiness = (fallHeight - minFallHeight) * fallSleepinessMultiplier;
                sleepiness += addedSleepiness;
                sleepiness = Mathf.Clamp(sleepiness, 0f, maxSleepiness);
                if (sleepiness >= maxSleepiness && !isBlackout)
                    StartCoroutine(BlackoutCoroutine());
            }
            isFalling = false;
        }

        if (isBlackout) return;

        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0f) { velocity.y = -0.5f; hasReachedApex = false; }
        if (velocity.y > 0f) hasReachedApex = false;
        else if (!isGrounded && velocity.y <= 0f) hasReachedApex = true;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;
        if (cameraPivot != null)
        {
            forward = Vector3.ProjectOnPlane(cameraPivot.forward, Vector3.up).normalized;
            right = Vector3.ProjectOnPlane(cameraPivot.right, Vector3.up).normalized;
        }

        Vector3 move = (forward * z + right * x).normalized;

        if (move.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        bool isMoving = move.magnitude > 0.1f;
        float currentSpeed = speed;

        if (isSprinting && sprintTimer > 0f && stamina > 0f)
        {
            currentSpeed = speed * sprintMultiplier;
            sprintTimer -= Time.deltaTime;
            stamina -= staminaDrainRate * 2f * Time.deltaTime;
        }
        else isSprinting = false;

        if (stamina <= 0f) { currentSpeed = speed * 0.5f; isSprinting = false; }

        if (!isSprinting)
        {
            if (isMoving) stamina -= staminaDrainRate * Time.deltaTime;
            else stamina += staminaRegenRate * Time.deltaTime;
        }
        stamina = Mathf.Clamp(stamina, 0f, maxStamina);

        Vector3 horizontalMove = move * currentSpeed;

        if (isSlowFalling && hasReachedApex)
        {
            gravity = slowFallGravity;
            slowFallTimer -= Time.deltaTime;
            if (slowFallTimer <= 0f) { isSlowFalling = false; gravity = normalGravity; }
        }
        else gravity = normalGravity;

        velocity.y -= gravity * Time.deltaTime;
        controller.Move((horizontalMove + velocity) * Time.deltaTime);

        if (isGrounded && velocity.y <= -5f)
        {
            if (!isSlowFalling)
            {
                float addedSleep = Mathf.Abs(velocity.y) * 2f;
                sleepiness += addedSleep;
                sleepiness = Mathf.Clamp(sleepiness, 0f, maxSleepiness);
                if (sleepiness >= maxSleepiness && !isBlackout) StartCoroutine(BlackoutCoroutine());
            }
        }
    }

    IEnumerator BlackoutCoroutine()
    {
        isBlackout = true;
        canMove = false;

        // 1. Запуск анимации сна
        animator?.SetTrigger("Blackout");

        // Ждать окончания анимации
        yield return new WaitForSeconds(blackoutAnimDuration);

        // 2. Fade-in (затемнение)
        if (blackoutImage != null)
        {
            blackoutImage.gameObject.SetActive(true);
            Color c = blackoutImage.color;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                c.a = Mathf.Clamp01(t / fadeDuration);
                blackoutImage.color = c;
                yield return null;
            }
            c.a = 1f;
            blackoutImage.color = c;
        }

        // 3. Видео фиксированной длины
        if (videoRawImage != null)
        {
            videoRawImage.enabled = true;
            videoRawImage.gameObject.SetActive(true);

            VideoPlayer vp = videoRawImage.GetComponent<VideoPlayer>();
            if (vp != null) vp.Play();

            yield return new WaitForSeconds(blackoutDuration); // длительность видео

            if (vp != null) vp.Stop();
            videoRawImage.gameObject.SetActive(false);
            videoRawImage.enabled = false;
        }

        // 4. Fade-out (прояснение)
        if (blackoutImage != null)
        {
            Color c = blackoutImage.color;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                c.a = 1f - Mathf.Clamp01(t / fadeDuration);
                blackoutImage.color = c;
                yield return null;
            }
            c.a = 0f;
            blackoutImage.color = c;
            blackoutImage.gameObject.SetActive(false);
        }

        // 5. Сброс сонливости
        sleepiness = 30f;

        canMove = true;
        isBlackout = false;
    }

    IEnumerator RestCoroutine()
    {
        canMove = false;
        isBlackout = true;

        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Blackout");
            yield return new WaitForSeconds(blackoutAnimDuration);
        }
        else
        {
            Debug.LogWarning("Animator не найден, пропускаю анимацию.");
        }

        float fadeTime = 1f;
        float restDuration = 2f;
        float elapsed = 0f;

        if (blackoutImage != null)
        {
            blackoutImage.gameObject.SetActive(true);
            Color c = blackoutImage.color;
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                c.a = Mathf.Clamp01(elapsed / fadeTime);
                blackoutImage.color = c;
                yield return null;
            }
            blackoutImage.color = new Color(0f, 0f, 0f, 1f);
        }

        yield return new WaitForSeconds(restDuration);

        if (blackoutImage != null)
        {
            elapsed = 0f;
            Color c = blackoutImage.color;
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                c.a = 1f - Mathf.Clamp01(elapsed / fadeTime);
                blackoutImage.color = c;
                yield return null;
            }
            blackoutImage.color = new Color(0f, 0f, 0f, 0f);
            blackoutImage.gameObject.SetActive(false);
        }

        if (animator != null) animator.SetBool("isSleeping", false);

        canMove = true;
        isBlackout = false;
    }

    void UpdateStaminaUI()
    {
        if (staminaBarImage == null || staminaContainerRect == null) return;

        float fill = Mathf.Clamp01(stamina / maxStamina);
        Color targetColor = fill > 0.5f ? Color.green : (fill > 0.2f ? Color.yellow : Color.red);

        Vector2 shakeOffset = Vector2.zero;
        if (fill <= 0.2f)
        {
            float pulse = Mathf.Abs(Mathf.Sin(Time.time * 10f)) * 0.5f + 0.5f;
            targetColor = Color.Lerp(Color.red * 0.5f, Color.red, pulse);
            float shakeAmount = 5f;
            shakeOffset = new Vector2(Mathf.Sin(Time.time * 20f) * shakeAmount,
                                      Mathf.Cos(Time.time * 25f) * shakeAmount);
        }

        staminaBarImage.color = Color.Lerp(staminaBarImage.color, targetColor, Time.deltaTime * 10f);

        // update bar size inside container; icon remains at left
        RectTransform barRect = staminaBarImage.GetComponent<RectTransform>();
        float barWidth = staminaBarSize.x * fill;
        barRect.sizeDelta = new Vector2(barWidth, staminaBarSize.y);
        barRect.anchoredPosition = new Vector2(staminaIconSize.x + iconPadding, (staminaContainerRect.sizeDelta.y - staminaBarSize.y) / 2f);

        // update container size to match current bar width + icon
        float containerWidth = staminaIconSize.x + iconPadding + barWidth;
        float containerHeight = Mathf.Max(staminaIconSize.y, staminaBarSize.y);
        staminaContainerRect.sizeDelta = new Vector2(containerWidth, containerHeight);

        // apply shake offset to whole container anchored position
        staminaContainerRect.anchoredPosition = staminaBarPosition + shakeOffset;
        staminaBarImage.fillAmount = fill;
    }

    void UpdateSleepinessUI()
    {
        if (sleepinessBarImage == null || sleepinessContainerRect == null) return;

        float fill = Mathf.Clamp01(sleepiness / maxSleepiness);

        RectTransform barRect = sleepinessBarImage.GetComponent<RectTransform>();
        float barWidth = sleepinessBarSize.x * fill;
        barRect.sizeDelta = new Vector2(barWidth, sleepinessBarSize.y);
        barRect.anchoredPosition = new Vector2(sleepinessIconSize.x + iconPadding, (sleepinessContainerRect.sizeDelta.y - sleepinessBarSize.y) / 2f);

        float containerWidth = sleepinessIconSize.x + iconPadding + barWidth;
        float containerHeight = Mathf.Max(sleepinessIconSize.y, sleepinessBarSize.y);
        sleepinessContainerRect.sizeDelta = new Vector2(containerWidth, containerHeight);

        sleepinessContainerRect.anchoredPosition = sleepinessBarPosition;
        sleepinessBarImage.fillAmount = fill;
    }

    void UpdateCooldownUI()
    {
        void UpdateButton(Image img, float lastTime, float cooldown, bool unlocked)
        {
            if (img == null) return;
            float fill = Mathf.Clamp01((Time.time - lastTime) / Mathf.Max(0.0001f, cooldown));
            img.fillAmount = Mathf.Lerp(img.fillAmount, fill, Time.deltaTime * 5f);
            Color targetColor = unlocked ? (fill >= 1f ? readyColor : notReadyColor) : Color.red;
            img.color = Color.Lerp(img.color, targetColor, Time.deltaTime * 5f);
        }

        UpdateButton(jumpButtonImage, lastJumpTime, jumpCooldown, canJump);
        UpdateButton(slowFallButtonImage, lastSlowFallTime, slowFallCooldown, canSlowFall);
        UpdateButton(sprintButtonImage, lastSprintTime, sprintCooldown, canSprint);
        UpdateButton(restButtonImage, lastRestTime, restCooldown, canRest);
    }

    void HandleCamera()
    {
        if (cameraTransform == null || cameraPivot == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minY, maxY);

        cameraPivot.position = transform.position + cameraOffset;
        cameraPivot.rotation = Quaternion.Euler(pitch, yaw, 0f);

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        cameraDistance = Mathf.Clamp(cameraDistance - scroll * zoomSpeed, minDistance, maxDistance);

        Vector3 desiredPosition = cameraPivot.position - cameraPivot.forward * cameraDistance;
        if (Physics.Linecast(cameraPivot.position, desiredPosition, out RaycastHit hit))
            cameraTransform.position = hit.point + hit.normal * 0.2f;
        else cameraTransform.position = desiredPosition;

        cameraTransform.LookAt(cameraPivot);
    }

    void OnJumpButtonClicked() => TryJump();
    void OnSlowFallButtonClicked() => TrySlowFall();
    void OnSprintButtonClicked() => TrySprint();
    void OnRestButtonClicked() => TryRest();

    void TryJump()
    {
        if (canJump && isGrounded && Time.time >= lastJumpTime + jumpCooldown)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * 2f * normalGravity);
            lastJumpTime = Time.time;
        }
    }

    void TrySlowFall()
    {
        if (!canSlowFall) return;
        if (Time.time >= lastSlowFallTime + slowFallCooldown)
        {
            isSlowFalling = true;
            slowFallTimer = slowFallDuration;
            lastSlowFallTime = Time.time;
        }
    }

    void TrySprint()
    {
        if (!canSprint) return;
        if (Time.time >= lastSprintTime + sprintCooldown && stamina > 0f)
        {
            sprintTimer = sprintDuration;
            isSprinting = true;
            lastSprintTime = Time.time;
        }
    }

    void TryRest()
    {
        if (!canRest) return;
        if (Time.time >= lastRestTime + restCooldown)
        {
            sleepiness -= restAmount;
            sleepiness = Mathf.Clamp(sleepiness, 0f, maxSleepiness);
            lastRestTime = Time.time;
            StartCoroutine(RestCoroutine());
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("JumpBlock")) canJump = true;
        if (hit.collider.CompareTag("SlowFallBlock")) canSlowFall = true;
        if (hit.collider.CompareTag("SprintBlock")) canSprint = true;
        if (hit.collider.CompareTag("RestBlock")) canRest = true;
    }

    public void SetUIActive(bool active)
    {
        if (topButtonsContainer != null)
            topButtonsContainer.gameObject.SetActive(active);
        if (staminaContainerRect != null)
            staminaContainerRect.gameObject.SetActive(active);
        if (sleepinessContainerRect != null)
            sleepinessContainerRect.gameObject.SetActive(active);
    }
}
