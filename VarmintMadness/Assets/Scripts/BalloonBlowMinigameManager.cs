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

    [Header("Pump Animation Settings")]
    public GameObject pumpHandle; // Drag the pump handle sprite here
    public float pumpDistance = 0.5f; // How far down it moves
    public float pumpSpeed = 5f; // How fast it moves
    public float pauseDuration = 0.1f; // Pause at the bottom

    [Header("UI")]
    public TextMeshProUGUI[] playerPointTexts;

    [Header("Audio")]
    public AudioSource minigameMusic;
    public AudioSource winAudioSource;

    private int currentPlayer = 0;
    private bool waitingForInput = false;
    private bool playerAtPump = false;
    private bool gameOver = false;
    private bool isPumping = false; // Prevents spamming during animation

    void Start()
    {
        int totalPlayers = PlayerPrefs.GetInt("TotalPlayers", 4);

        // Build active players list from whichever GameObjects are active
        // (MinigameCharacterApplier already handled enabling the correct ones)
        List<PlayerMinigameMovement> activePlayers = new List<PlayerMinigameMovement>();
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null && players[i].gameObject.activeSelf)
                activePlayers.Add(players[i]);
        }

        // Trim to active players only
        players = activePlayers.ToArray();

        // Disable unused UI
        for (int i = 0; i < playerPointTexts.Length; i++)
        {
            if (playerPointTexts[i] != null)
                playerPointTexts[i].gameObject.SetActive(i < players.Length);
        }

        // Move active players to their start points
        for (int i = 0; i < players.Length; i++)
            players[i].transform.position = playerStartPoints[i].position;

        balloon.InitializePlayers(players.Length);
        UpdatePointUI();
        StartPlayerTurn();
    }

    void Update()
    {
        // Added !isPumping check to block input during animation
        if (gameOver || !waitingForInput || !playerAtPump || isPumping)
            return;

        if (Input.GetKeyDown(KeyCode.E))
            StartCoroutine(AnimatePump());

        if (Input.GetKeyDown(KeyCode.Q) && balloon.CanSkip())
            EndTurn();
    }

    IEnumerator AnimatePump()
    {
        isPumping = true;

        if (pumpHandle != null)
        {
            Vector3 startPos = pumpHandle.transform.localPosition;
            Vector3 targetPos = startPos + Vector3.down * pumpDistance;

            // Move Down
            while (Vector3.Distance(pumpHandle.transform.localPosition, targetPos) > 0.01f)
            {
                pumpHandle.transform.localPosition = Vector3.MoveTowards(pumpHandle.transform.localPosition, targetPos, pumpSpeed * Time.deltaTime);
                yield return null;
            }

            // Pause at bottom
            yield return new WaitForSeconds(pauseDuration);

            // Logic trigger (the actual balloon pump happens here)
            HandlePump();

            // Move Up
            while (Vector3.Distance(pumpHandle.transform.localPosition, startPos) > 0.01f)
            {
                pumpHandle.transform.localPosition = Vector3.MoveTowards(pumpHandle.transform.localPosition, startPos, pumpSpeed * Time.deltaTime);
                yield return null;
            }

            pumpHandle.transform.localPosition = startPos; // Ensure precise reset
        }
        else
        {
            // Fallback if no handle is assigned
            HandlePump();
        }

        isPumping = false;
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

    // ... [Rest of your existing methods: PlayWinAndReturn, StartPlayerTurn, EndTurn, NextPlayer, etc. remain the same]

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

    List<int> GetAllHighestScoringPlayers()
    {
        List<int> winners = new List<int>();
        int bestScore = balloon.playerPoints[0];

        for (int i = 1; i < balloon.playerPoints.Length; i++)
        {
            if (balloon.playerPoints[i] > bestScore)
                bestScore = balloon.playerPoints[i];
        }

        for (int i = 0; i < balloon.playerPoints.Length; i++)
        {
            if (balloon.playerPoints[i] == bestScore)
                winners.Add(i);
        }

        return winners;
    }

    void UpdatePointUI()
    {
        int totalPlayers = PlayerPrefs.GetInt("TotalPlayers", 4);
        for (int i = 0; i < playerPointTexts.Length && i < totalPlayers; i++)
        {
            if (playerPointTexts[i] != null && playerPointTexts[i].gameObject.activeSelf)
                playerPointTexts[i].text = $"Player {i + 1} Points: {balloon.playerPoints[i]}";
        }
    }
}
