using UnityEngine;
using TMPro;

public class RunnerTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public float currentTime = 0f;
    public bool isRunning = false;

    void Update()
    {
        if (!isRunning) return;

        currentTime += Time.deltaTime;
        timerText.text = currentTime.ToString("F2");
    }

    public void StartTimer()
    {
        currentTime = 0f;
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }
}