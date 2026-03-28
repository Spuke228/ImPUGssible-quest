using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class BaldiLikeQuiz : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;
    public TextMeshProUGUI questionText;
    public TMP_InputField answerInput;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI timerText;
    public Image redFlash;
    public RawImage VHSNoise;
    public RawImage CRTScanlines;

    [Header("Player")]
    public Управлениемопсом player;
    public MonoBehaviour playerController;

    [Header("Camera")]
    public Transform cameraTransform;
    public Camera cam;

    [Header("Sounds")]
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip startSound;
    public AudioClip baldiAngry;

    [Header("Settings")]
    public float minTime = 120f;
    public float maxTime = 240f;
    public float sleepinessPenalty = 20f;
    public float answerTime = 10f;
    public float sleepinessHardLevel = 50f;
    public float glitchSpeed = 0.05f;
    public float fakeAnswerChance = 0.15f;
    public float impossibleChance = 0.05f;
    public float insanityEffect = 0.02f;

    private int questionIndex;
    private int correctAnswer;
    private bool impossible;
    private float currentTimer;
    private bool timerRunning;
    private float insanity;
    private bool glitchActive;
    private bool textCrawlActive;

    string[] angryTexts = { "НЕПРАВИЛЬНО", "ОЧЕНЬ ПЛОХО", "ДУМАЙ БЫСТРЕЕ", "ТЫ ОШИБСЯ" };
    string[] glitchChars = { "@", "#", "$", "%", "&", "?", "!", "|", "~", "^", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

    void Start()
    {
        panel.SetActive(false);
        StartCoroutine(QuizLoop());
    }

    void Update()
    {
        if (timerRunning)
        {
            currentTimer -= Time.unscaledDeltaTime;
            timerText.text = Mathf.Ceil(currentTimer).ToString();

            if (currentTimer <= 0)
            {
                timerRunning = false;
                StartCoroutine(WrongRoutine());
            }
        }

        // Безумие
        if (insanity > 30) questionText.color = Color.red;
        if (insanity > 60) glitchSpeed = 0.02f;
        if (insanity > 100) fakeAnswerChance = 0.4f;

        // VHS шум
        if (VHSNoise != null)
        {
            Rect uv = VHSNoise.uvRect;
            uv.y += Time.unscaledDeltaTime * 30f;
            VHSNoise.uvRect = uv;
            VHSNoise.color = new Color(1, 1, 1, Random.Range(0.05f, 0.15f));
        }

        // CRT Scanlines
        if (CRTScanlines != null)
            CRTScanlines.color = new Color(1, 1, 1, Random.Range(0.02f, 0.1f));
    }

    IEnumerator QuizLoop()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(Random.Range(minTime, maxTime));
            StartQuiz();
        }
    }

    void StartQuiz()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (playerController != null) playerController.enabled = false;

        panel.SetActive(true);
        questionIndex = 0;
        GenerateQuestion();

        answerInput.ActivateInputField();
        if (startSound != null) audioSource.PlayOneShot(startSound);
    }

    void GenerateQuestion()
    {
        questionIndex++;
        answerInput.text = "";
        feedbackText.text = "";

        impossible = false;

        // Третий вопрос
        if (questionIndex == 3)
        {
            if (Random.value < 0.5f)
            {
                // невозможный глитч
                impossible = true;
                correctAnswer = Random.Range(1, 9999);
                timerRunning = true;
                currentTimer = answerTime - insanity * insanityEffect;
                StartCoroutine(RealBaldiGlitch());
                StartCoroutine(CrawlGlitchText());
                return;
            }
            else
            {
                // суперсложный пример
                GenerateSuperHardQuestion();
                timerRunning = true;
                currentTimer = answerTime - insanity * insanityEffect;
                return;
            }
        }

        // обычные вопросы
        GenerateNormalQuestion();
        timerRunning = true;
        currentTimer = answerTime - insanity * insanityEffect;
    }

    void GenerateNormalQuestion()
    {
        int typeMax = 6;
        if (player != null && player.sleepiness > sleepinessHardLevel) typeMax = 10;
        int type = Random.Range(0, typeMax);

        int a = Random.Range(2, 12);
        int b = Random.Range(2, 12);

        switch (type)
        {
            case 0: correctAnswer = a + b; questionText.text = $"{a} + {b} = ?"; break;
            case 1: correctAnswer = a * b; questionText.text = $"{a} × {b} = ?"; break;
            case 2: correctAnswer = a; questionText.text = $"{a * b} ÷ {b} = ?"; break;
            case 3: correctAnswer = a * a; questionText.text = $"{a}² = ?"; break;
            case 4: int sq = a * a; correctAnswer = a; questionText.text = $"√{sq} = ?"; break;
            case 5: correctAnswer = a; questionText.text = $"x + {b} = {a + b}. x = ?"; break;
        }

        if (Random.value < fakeAnswerChance) correctAnswer += Random.Range(1, 4);

        currentTimer = answerTime - insanity * insanityEffect;
        timerRunning = true;
    }

    void GenerateSuperHardQuestion()
    {
        int a = Random.Range(2, 12);
        int b = Random.Range(2, 10);
        int c = Random.Range(1, 5);

        int type = Random.Range(0, 6);
        switch (type)
        {
            case 0: correctAnswer = a * a + b; questionText.text = $"{a}² + {b} = ?"; break;
            case 1: correctAnswer = a * a * a; questionText.text = $"{a}³ = ?"; break;
            case 2: int sq = a * a; correctAnswer = a; questionText.text = $"√{sq} = ?"; break;
            case 3: correctAnswer = (a * b) / c; questionText.text = $"{a * b} ÷ {c} = ?"; break;
            case 4: correctAnswer = a + b - c; questionText.text = $"{a} + {b} - {c} = ?"; break;
            case 5: correctAnswer = (a * a) + (b * b); questionText.text = $"{a}² + {b}² = ?"; break;
        }

        currentTimer = answerTime - insanity * insanityEffect;
        timerRunning = true;
    }

    public void SubmitAnswer()
    {
        timerRunning = false;

        if (!int.TryParse(answerInput.text, out int parsedValue))
        {
            feedbackText.text = "ВВЕДИ ЧИСЛО";
            return;
        }

        if (impossible)
        {
            if (parsedValue == correctAnswer)
            {
                player.sleepiness = 0f;
                feedbackText.text = "ПРАВИЛЬНО!";
                StartCoroutine(CorrectRoutine());
                if (correctSound != null) audioSource.PlayOneShot(correctSound);
            }
            else
            {
                StartCoroutine(WrongRoutine());
            }
        }
        else
        {
            if (parsedValue == correctAnswer)
            {
                feedbackText.text = "ПРАВИЛЬНО!";
                player.sleepiness -= 5f;
                StartCoroutine(CorrectRoutine());
                if (correctSound != null) audioSource.PlayOneShot(correctSound);
            }
            else
            {
                StartCoroutine(WrongRoutine());
            }
        }
    }

    IEnumerator CorrectRoutine()
    {
        yield return new WaitForSecondsRealtime(1.5f);

        if (questionIndex >= 3)
            EndQuiz();
        else
            GenerateQuestion();
    }

    IEnumerator WrongRoutine()
    {
        feedbackText.text = angryTexts[Random.Range(0, angryTexts.Length)];

        if (player != null) player.sleepiness += sleepinessPenalty;
        insanity += 10f;

        if (wrongSound != null) audioSource.PlayOneShot(wrongSound);
        if (baldiAngry != null) audioSource.PlayOneShot(baldiAngry);

        StartCoroutine(CameraShake());
        StartCoroutine(BaldiScream());
        StartCoroutine(Flash());
        StartCoroutine(RealBaldiGlitch());
        StartCoroutine(CrawlGlitchText());

        yield return new WaitForSecondsRealtime(2f);
        EndQuiz();
    }

    IEnumerator CameraShake()
    {
        if (cameraTransform == null) yield break;

        Vector3 start = cameraTransform.localPosition;
        float t = 0;
        while (t < 0.5f)
        {
            t += Time.unscaledDeltaTime;
            cameraTransform.localPosition = start + Random.insideUnitSphere * 0.2f;
            yield return null;
        }
        cameraTransform.localPosition = start;
    }

    IEnumerator BaldiScream()
    {
        if (cam == null) yield break;

        float t = 0;
        float normalZoom = cam.fieldOfView;
        float screamZoom = 40f;

        while (t < 0.5f)
        {
            t += Time.unscaledDeltaTime;
            cam.fieldOfView = Mathf.Lerp(normalZoom, screamZoom, t * 5);
            cameraTransform.localPosition += Random.insideUnitSphere * 0.2f;
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.5f);
        cam.fieldOfView = normalZoom;
    }

    IEnumerator Flash()
    {
        if (redFlash == null) yield break;

        redFlash.color = new Color(1, 0, 0, 0.6f);
        yield return new WaitForSecondsRealtime(0.2f);
        redFlash.color = new Color(1, 0, 0, 0);
    }

    IEnumerator RealBaldiGlitch()
    {
        glitchActive = true;

        while (glitchActive)
        {
            string line = "";
            int length = Random.Range(20, 35);

            for (int i = 0; i < length; i++)
            {
                float rnd = Random.value;
                if (rnd < 0.4f) line += glitchChars[Random.Range(0, glitchChars.Length)];
                else if (rnd < 0.7f) line += Random.Range(0, 9).ToString();
                else line += " ";
            }

            // Мерцание цвета: красный с яркостью и зелёным/синим шумом
            float intensity = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 25f + Random.value * 10f);
            questionText.color = new Color(1f, intensity * 0.3f, intensity * 0.2f);

            // Дрожащие буквы + случайные смещения
            string jittered = "";
            foreach (char c in line)
            {
                float xShift = Random.Range(-3f, 3f);
                float yShift = Random.Range(-3f, 3f);
                jittered += $"<voffset={yShift}em><pos={xShift}em>{c}</pos></voffset>";
            }

            questionText.text = jittered;

            // Усиление VHS шума
            if (VHSNoise != null)
            {
                Rect uv = VHSNoise.uvRect;
                uv.y += Time.unscaledDeltaTime * 50f; // ускоряем
                VHSNoise.uvRect = uv;
                VHSNoise.color = new Color(1, 1, 1, Random.Range(0.1f, 0.25f));
            }

            // Усиление CRT мерцания
            if (CRTScanlines != null)
            {
                CRTScanlines.color = new Color(1, 1, 1, Random.Range(0.05f, 0.15f));
            }

            // Лёгкая дрожь камеры
            if (cameraTransform != null)
            {
                cameraTransform.localPosition += Random.insideUnitSphere * 0.3f;
            }

            yield return new WaitForSecondsRealtime(glitchSpeed);
        }
    }

    IEnumerator CrawlGlitchText()
    {
        textCrawlActive = true;

        while (textCrawlActive)
        {
            string current = questionText.text;

            // сдвигаем текст на один символ влево
            if (current.Length > 1)
                current = current.Substring(1);

            // добавляем случайный символ в конец
            char newChar = glitchChars[Random.Range(0, glitchChars.Length)][0];
            questionText.text = current + newChar;

            yield return new WaitForSecondsRealtime(glitchSpeed * 1.5f);
        }
    }

    char RandomChar() => glitchChars[Random.Range(0, glitchChars.Length)][0];

    void EndQuiz()
    {
        panel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerController != null) playerController.enabled = true;

        // останавливаем глитч
        glitchActive = false;
        textCrawlActive = false;

        // сброс таймера и нормальный Time.timeScale
        timerRunning = false;
        Time.timeScale = 1f;
    }
}