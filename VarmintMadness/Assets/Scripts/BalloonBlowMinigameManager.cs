using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class BalloonBlowMinigameManager : MonoBehaviour
{
    public BalloonController balloon;

    public Transform pumpPoint;
    public Transform[] playerStartPoints;

    public PlayerMinigameMovement[] players;

    [Header("UI")]
    public TextMeshProUGUI[] playerPointTexts;

    [Header("Audio")]
    public AudioSource minigameMusic;
    public AudioSource winAudioSource;

    private int currentPlayer = 0;
    private bool waitingForInput = false;
    private bool playerAtPump = false;
    private bool gameOver = false;

    void Start()
    {
        for (int i = 0; i < players.Length; i++)
            players[i].transform.position = playerStartPoints[i].position;

        balloon.InitializePlayers(players.Length);
        UpdatePointUI();
        StartPlayerTurn();
    }

    void Update()
    {
        if (gameOver || !waitingForInput || !playerAtPump)
            return;

        if (Input.GetKeyDown(KeyCode.E))
            HandlePump();

        if (Input.GetKeyDown(KeyCode.Q) && balloon.CanSkip())
            EndTurn();
    }

    void HandlePump()
    {
        if (!balloon.HasPumpsLeft())
        {
            EndTurn();
            return;
        }

        bool popped = balloon.Pump(currentPlayer);
        UpdatePointUI();

        if (popped)
        {
            gameOver = true;

            List<int> winners = GetAllHighestScoringPlayers();

            MarbleRewardData.WinnerPlayerIndices = winners;
            MarbleRewardData.BonusTrash = 10;

            StartCoroutine(PlayWinAndReturn());
            return;
        }

        if (!balloon.HasPumpsLeft())
            EndTurn();
    }

    IEnumerator PlayWinAndReturn()
    {
        if (minigameMusic != null)
            minigameMusic.Stop();

        if (winAudioSource != null && winAudioSource.clip != null)
        {
            winAudioSource.Play();
            yield return new WaitForSeconds(winAudioSource.clip.length);
        }

        SceneManager.LoadScene("LoadingScene");
    }

    void StartPlayerTurn()
    {
        waitingForInput = false;
        playerAtPump = false;
        balloon.ResetTurn();

        players[currentPlayer].MoveTo(
            pumpPoint.position,
            () =>
            {
                playerAtPump = true;
                waitingForInput = true;
            }
        );
    }

    void EndTurn()
    {
        waitingForInput = false;
        playerAtPump = false;

        players[currentPlayer].MoveTo(
            playerStartPoints[currentPlayer].position,
            NextPlayer
        );
    }

    void NextPlayer()
    {
        currentPlayer++;
        if (currentPlayer >= players.Length)
            currentPlayer = 0;

        StartPlayerTurn();
    }

    // ⭐ NEW — returns ALL tied winners
    List<int> GetAllHighestScoringPlayers()
    {
        List<int> winners = new List<int>();

        int bestScore = balloon.playerPoints[0];

        // Find highest score
        for (int i = 1; i < balloon.playerPoints.Length; i++)
        {
            if (balloon.playerPoints[i] > bestScore)
                bestScore = balloon.playerPoints[i];
        }

        // Add all players who match highest score
        for (int i = 0; i < balloon.playerPoints.Length; i++)
        {
            if (balloon.playerPoints[i] == bestScore)
                winners.Add(i);
        }

        return winners;
    }

    void UpdatePointUI()
    {
        for (int i = 0; i < playerPointTexts.Length; i++)
        {
            playerPointTexts[i].text =
                $"Player {i + 1} Points: {balloon.playerPoints[i]}";
        }
    }
}