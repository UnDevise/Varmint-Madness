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
    private bool gameOver = false;

    private int pumpsThisTurn = 0;
    private const int maxPumps = 3;

    void Start()
    {
        // Place players at their starting spots
        for (int i = 0; i < players.Length; i++)
            players[i].transform.position = playerStartPoints[i].position;

        StartPlayerTurn();
    }

    void Update()
    {
        if (gameOver || !waitingForInput) return;

        // Pump
        if (Input.GetKeyDown(KeyCode.Space))
            PumpBalloon();

        // Skip (only allowed after at least 1 pump)
        if (Input.GetKeyDown(KeyCode.Q) && pumpsThisTurn > 0)
            EndTurn();
    }

    void StartPlayerTurn()
    {
        waitingForInput = false;
        pumpsThisTurn = 0;

        players[currentPlayer].MoveTo(
            pumpPoint.position,
            () => waitingForInput = true
        );
    }

    void PumpBalloon()
    {
        pumpsThisTurn++;

        bool popped = balloon.Blow();

        if (popped)
        {
            gameOver = true;
            Debug.Log("Player " + (currentPlayer + 1) + " popped the balloon!");

            int winner = GetWinnerIndex();
            MarbleRewardData.WinnerPlayerIndex = winner;
            MarbleRewardData.BonusTrash = 3;

            SceneManager.LoadScene("BoardScene");
            return;
        }

        // If they used all pumps, end turn automatically
        if (pumpsThisTurn >= maxPumps)
        {
            EndTurn();
        }
    }

    void EndTurn()
    {
        waitingForInput = false;

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