using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MarathonMinigameController : MonoBehaviour
{
    [Header("Runner Setup")]
    public Transform startPoint;
    public RunnerController runner;
    public RunnerTimer timer;

    [Header("UI")]
    public TextMeshProUGUI turnAnnouncementText;

    private int[] players = { 0, 1, 2, 3 };
    private Dictionary<int, float> finishTimes = new Dictionary<int, float>();

    private int currentPlayerIndex = 0;

    void Start()
    {
        runner.ResetRunner(startPoint.position);
        Camera.main.GetComponent<CameraFollow2D>().target = runner.transform;
        StartCoroutine(StartNextPlayerRoutine());
    }

    IEnumerator StartNextPlayerRoutine()
    {
        Debug.Log("StartNextPlayerRoutine() started for player index: " + currentPlayerIndex);
        if (currentPlayerIndex >= players.Length)
        {
            EndMinigame();
            yield break;
        }

        int playerID = players[currentPlayerIndex];

        turnAnnouncementText.text = $"Player {playerID + 1}'s Turn!";
        turnAnnouncementText.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        turnAnnouncementText.gameObject.SetActive(false);

        runner.ResetRunner(startPoint.position);

        StartCoroutine(EnableFinishAfterDelay());

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
        Debug.LogError("PlayerFinished() CALLED by: " + new System.Diagnostics.StackTrace());

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