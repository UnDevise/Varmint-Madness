using UnityEngine;
using UnityEngine.SceneManagement;

public class BalloonBlowMinigameManager : MonoBehaviour
{
    public BalloonController balloon;

    public Transform pumpPoint;
    public Transform[] playerStartPoints;

    public PlayerMinigameMovement[] players;

    private int currentPlayer = 0;
    private bool waitingForInput = false;
    private bool playerAtPump = false;
    private bool gameOver = false;

    void Start()
    {
        for (int i = 0; i < players.Length; i++)
            players[i].transform.position = playerStartPoints[i].position;

        StartPlayerTurn();
    }

    void Update()
    {
        if (gameOver || !waitingForInput || !playerAtPump)
            return;

        // Pump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandlePump();
        }

        // Skip (only if pumped at least once)
        if (Input.GetKeyDown(KeyCode.Q) && balloon.CanSkip())
        {
            EndTurn();
        }
    }

    void HandlePump()
    {
        // If no pumps left, end turn
        if (!balloon.HasPumpsLeft())
        {
            EndTurn();
            return;
        }

        bool popped = balloon.Pump();

        if (popped)
        {
            gameOver = true;

            int winner = GetWinnerIndex();
            MarbleRewardData.WinnerPlayerIndex = winner;
            MarbleRewardData.BonusTrash = 3;

            SceneManager.LoadScene("BoardScene");
            return;
        }

        // After pumping, check again
        if (!balloon.HasPumpsLeft())
        {
            EndTurn();
        }
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

    int GetWinnerIndex()
    {
        int winner = currentPlayer - 1;
        if (winner < 0) winner = players.Length - 1;
        return winner;
    }
}