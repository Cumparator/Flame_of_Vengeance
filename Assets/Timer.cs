using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour // ќбратите внимание на ": MonoBehaviour"
{
    [SerializeField] private TMPro.TextMeshProUGUI timerText; // —сылка на UI Text'

    private float startTime;
    private bool isRunning;

    void Start()
    {
        StartTimer();
    }

    public void StartTimer()
    {
        startTime = Time.time;
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    void Update()
    {
        if (isRunning)
        {
            float currentTime = Time.time - startTime;

            string minutes = ((int)currentTime / 60).ToString("00");
            string seconds = (currentTime % 60).ToString("00");
            string milliseconds = ((currentTime * 1000) % 1000).ToString("000");

            timerText.text = $"{minutes}:{seconds}:{milliseconds}";
        }
    }

    public float GetCurrentTime()
    {
        return isRunning ? Time.time - startTime : 0f;
    }
}