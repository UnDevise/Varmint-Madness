using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MarathonMinigameController : MonoBehaviour
{
    [Header("Runner Setup")]
    public Transform startPoint;
    public RunnerController runner;   // Assign the runner in the scene
    public RunnerTimer timer;

    [Header("UI")]
    public TextMeshProUGUI turnAnnouncementText;

    private int[] players = { 0, 1, 2, 3 };
    private Dictionary<int, float> finishTimes = new Dictionary<int, float>();

    private int currentPlayerIndex = 0;

    void Start()
    {
        // Move runner to start
        runner.ResetRunner(startPoint.position);

        // Camera follow
        Camera.main.GetComponent<CameraFollow2D>().target = runner.transform;

        StartCoroutine(StartNextPlayerRoutine());
    }

    IEnumerator StartNextPlayerRoutine()
    {
        if (currentPlayerIndex >= players.Length)
        {
            EndMinigame();
            yield break;
        }

        int playerID = players[currentPlayerIndex];

        // Show announcement
        turnAnnouncementText.text = $"Player {playerID + 1}'s Turn!";
        turnAnnouncementText.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        turnAnnouncementText.gameObject.SetActive(false);

        // Reset runner for next player
        runner.ResetRunner(startPoint.position);

        // Delay before allowing finish
        StartCoroutine(EnableFinishAfterDelay());

        // Start timer
        timer.StartTimer();
    }

    IEnumerator EnableFinishAfterDelay()
    {
        runner.canFinish = false;
        yield return new WaitForSeconds(0.5f);
        runner.canFinish = true;
    }

    public void PlayerFinished()
    {
        timer.StopTimer();

        int playerID = players[currentPlayerIndex];
        finishTimes[playerID] = timer.currentTime;

        currentPlayerIndex++;

        StartCoroutine(StartNextPlayerRoutine());
    }

    void EndMinigame()
    {
        float bestTime = float.MaxValue;
        int winner = 0;

        foreach (var entry in finishTimes)
        {
            if (entry.Value < bestTime)
            {
                bestTime = entry.Value;
                winner = entry.Key;
            }
        }

        MarbleRewardData.WinnerPlayerIndices.Clear();
        MarbleRewardData.WinnerPlayerIndices.Add(winner);
        MarbleRewardData.BonusTrash = 10;

        SceneManager.LoadScene("LoadingScene");
    }
}