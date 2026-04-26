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

    [Header("Player")]
    public Управлениемопсом player;
    public MonoBehaviour playerController;

    [Header("Sounds")]
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip startSound;

    [Header("Settings")]
    public float minTime = 120f;
    public float maxTime = 240f;
    public float sleepinessPenalty = 20f;
    public float answerTime = 10f;
    public float sleepinessHardLevel = 50f;
    public float fakeAnswerChance = 0.15f;

    [Header("UI Blocking")]
    public GameObject[] blockingCanvases;

    private int questionIndex;
    private int correctAnswer;
    private float currentTimer;
    private bool timerRunning;

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
    }

    bool IsAnyUIOpen()
    {
        foreach (var obj in blockingCanvases)
        {
            if (obj != null && obj.activeInHierarchy)
                return true;
        }
        return false;
    }

    IEnumerator QuizLoop()
    {
        while (true)
        {
            float wait = Random.Range(minTime, maxTime);
            float t = 0f;

            while (t < wait)
            {
                if (!IsAnyUIOpen())
                    t += Time.unscaledDeltaTime;

                yield return null;
            }

            if (!IsAnyUIOpen())
                StartQuiz();
        }
    }

    void StartQuiz()
    {
        if (IsAnyUIOpen()) return;

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerController != null)
            playerController.enabled = false;

        panel.SetActive(true);
        questionIndex = 0;

        GenerateQuestion();

        answerInput.ActivateInputField();

        if (startSound != null)
            audioSource.PlayOneShot(startSound);
    }

    void GenerateQuestion()
    {
        questionIndex++;

        answerInput.text = "";
        feedbackText.text = "";

        GenerateNormalQuestion();

        timerRunning = true;
        currentTimer = answerTime;
    }

    void GenerateNormalQuestion()
    {
        int typeMax = 6;
        if (player != null && player.sleepiness > sleepinessHardLevel)
            typeMax = 6;

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

        if (Random.value < fakeAnswerChance)
            correctAnswer += Random.Range(1, 4);
    }

    public void SubmitAnswer()
    {
        timerRunning = false;

        if (!int.TryParse(answerInput.text, out int parsedValue))
        {
            feedbackText.text = "ВВЕДИ ЧИСЛО";
            return;
        }

        if (parsedValue == correctAnswer)
        {
            feedbackText.text = "ПРАВИЛЬНО!";

            if (player != null)
                player.sleepiness -= 5f;

            if (correctSound != null)
                audioSource.PlayOneShot(correctSound);

            StartCoroutine(CorrectRoutine());
        }
        else
        {
            StartCoroutine(WrongRoutine());
        }
    }

    IEnumerator CorrectRoutine()
    {
        yield return new WaitForSecondsRealtime(1f);

        if (questionIndex >= 3)
            EndQuiz();
        else
            GenerateQuestion();
    }

    IEnumerator WrongRoutine()
    {
        feedbackText.text = "НЕПРАВИЛЬНО";

        if (player != null)
            player.sleepiness += sleepinessPenalty;

        if (wrongSound != null)
            audioSource.PlayOneShot(wrongSound);

        yield return new WaitForSecondsRealtime(1.5f);

        EndQuiz();
    }

    void EndQuiz()
    {
        panel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerController != null)
            playerController.enabled = true;

        timerRunning = false;
        Time.timeScale = 1f;
    }
}