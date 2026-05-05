using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MarathonMinigameController : MonoBehaviour
{
    [Header("Runner Setup")]
    public Transform startPoint;
    public RunnerController runnerPrefab;
    public RunnerTimer timer;

    private List<PlayerMovement> boardPlayers = new List<PlayerMovement>();
    private Dictionary<PlayerMovement, float> finishTimes = new Dictionary<PlayerMovement, float>();

    private RunnerController currentRunner;
    private int currentPlayerIndex = 0;

    void Start()
    {
        // Get the 4 players from the board
        boardPlayers.AddRange(FindObjectsOfType<PlayerMovement>());

        StartNextPlayer();
    }

    void StartNextPlayer()
    {
        if (currentPlayerIndex >= boardPlayers.Count)
        {
            EndMinigame();
            return;
        }

        PlayerMovement p = boardPlayers[currentPlayerIndex];

        // Spawn runner
        currentRunner = Instantiate(runnerPrefab, startPoint.position, Quaternion.identity);

        // Assign camera follow target
        Camera.main.GetComponent<CameraFollow2D>().target = currentRunner.transform;


        // Copy sprite from board player
        SpriteRenderer runnerSR = currentRunner.GetComponent<SpriteRenderer>();
        SpriteRenderer playerSR = p.GetComponent<SpriteRenderer>();
        runnerSR.sprite = playerSR.sprite;

        // Reset timer
        timer.StartTimer();
    }

    public void PlayerFinished()
    {
        timer.StopTimer();

        PlayerMovement p = boardPlayers[currentPlayerIndex];
        finishTimes[p] = timer.currentTime;

        Destroy(currentRunner.gameObject);

        currentPlayerIndex++;
        StartNextPlayer();
    }

    void EndMinigame()
    {
        float bestTime = float.MaxValue;
        PlayerMovement winner = null;

        foreach (var entry in finishTimes)
        {
            if (entry.Value < bestTime)
            {
                bestTime = entry.Value;
                winner = entry.Key;
            }
        }

        // Reward winner
        MarbleRewardData.WinnerPlayerIndices.Clear();
        MarbleRewardData.WinnerPlayerIndices.Add(winner.playerID);
        MarbleRewardData.BonusTrash = 10;

        SceneManager.LoadScene("LoadingScene");
    }
}