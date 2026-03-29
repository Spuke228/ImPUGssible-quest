using UnityEngine;
using TMPro;

public class MinesweeperTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;

    float time;
    bool running;

    void Update()
    {
        if (!running) return;

        time += Time.deltaTime;
        timerText.text = Mathf.FloorToInt(time).ToString();
    }

    public void StartTimer()
    {
        time = 0;
        running = true;
    }

    public void StopTimer()
    {
        running = false;
    }
}