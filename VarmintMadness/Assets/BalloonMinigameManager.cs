using UnityEngine;

public class BalloonMinigameManager : MonoBehaviour
{
    public BalloonController balloon;
    public int playerCount = 4;
    public int currentPlayer = 0;

    public bool gameOver = false;

    void Start()
    {
        currentPlayer = 0;
        UpdateUI();
    }

    void Update()
    {
        if (gameOver) return;

        if (Input.GetKeyDown(KeyCode.Space))
            PlayerBlow();
    }

    void PlayerBlow()
    {
        bool popped = balloon.Blow();

        if (popped)
        {
            Debug.Log("Player " + (currentPlayer + 1) + " popped the balloon!");
            gameOver = true;

            // Send result back to board game
            MarbleRewardData.WinnerPlayerIndex = GetWinner();
            MarbleRewardData.BonusTrash = 3;

            UnityEngine.SceneManagement.SceneManager.LoadScene("BoardScene");
            return;
        }

        NextPlayer();
    }

    void NextPlayer()
    {
        currentPlayer++;
        if (currentPlayer >= playerCount)
            currentPlayer = 0;

        UpdateUI();
    }

    int GetWinner()
    {
        // Winner is the player BEFORE the one who popped it
        int winner = currentPlayer - 1;
        if (winner < 0) winner = playerCount - 1;
        return winner;
    }

    void UpdateUI()
    {
        Debug.Log("Player " + (currentPlayer + 1) + "'s turn!");
    }
}

