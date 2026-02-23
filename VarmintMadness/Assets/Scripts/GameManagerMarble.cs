using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManagerMarble : MonoBehaviour
{
    public MarbleMovement[] marbles;
    public MarbleSelector[] marbleSelectors;
    public TextMeshProUGUI playerTurnText;

    public int totalPlayers = 4;
    private int currentPlayer = 1;
    private int playersChosen = 0;

    private int[] playerMarbleChoices;
    private bool winnerChosen = false;

    private int lastWinningMarbleIndex = -1;
    private int lastWinningPlayerIndex = -1;

    void Start()
    {
        playerMarbleChoices = new int[totalPlayers];

        // Show all marbles at the start
        foreach (var selector in marbleSelectors)
            selector.EnableMarble();

        UpdateTurnText();
    }

    public void PlayerPickedMarble(MarbleSelector marbleButton)
    {
        int marbleIndex = marbleButton.marbleIndex;

        // Save the choice
        playerMarbleChoices[currentPlayer - 1] = marbleIndex;

        // Disable this marble so it cannot be picked again
        marbleButton.DisableMarble();

        playersChosen++;
        currentPlayer++;

        // Hide ALL marbles
        foreach (var selector in marbleSelectors)
            selector.gameObject.SetActive(false);

        // Show chosen marbles
        for (int i = 0; i < playersChosen; i++)
        {
            int chosenIndex = playerMarbleChoices[i];

            foreach (var selector in marbleSelectors)
            {
                if (selector.marbleIndex == chosenIndex)
                {
                    selector.gameObject.SetActive(true);
                    selector.DisableMarble();
                }
            }
        }

        // Show remaining marbles for next player
        if (playersChosen < totalPlayers)
        {
            foreach (var selector in marbleSelectors)
            {
                if (selector.enabled)
                    selector.gameObject.SetActive(true);
            }
        }

        // If all players have chosen, start the race
        if (playersChosen >= totalPlayers)
        {
            StartRace();
            return;
        }

        UpdateTurnText();
    }

    private void UpdateTurnText()
    {
        playerTurnText.text = "Player " + currentPlayer + ", pick your marble";
    }

    private void StartRace()
    {
        playerTurnText.gameObject.SetActive(false);

        for (int i = 0; i < playersChosen; i++)
        {
            int chosenMarbleIndex = playerMarbleChoices[i];

            foreach (var marble in marbles)
            {
                if (marble.marbleIndex == chosenMarbleIndex)
                {
                    marble.StartRace();
                    break;
                }
            }
        }
    }

    public void MarbleReachedFinish(int marbleIndex)
    {
        if (winnerChosen)
            return;

        winnerChosen = true;

        lastWinningMarbleIndex = marbleIndex;
        lastWinningPlayerIndex = -1;

        int winningPlayer = -1;

        for (int i = 0; i < totalPlayers; i++)
        {
            if (playerMarbleChoices[i] == marbleIndex)
            {
                winningPlayer = i;
                break;
            }
        }

        if (winningPlayer != -1)
            lastWinningPlayerIndex = winningPlayer;

        playerTurnText.gameObject.SetActive(true);

        if (winningPlayer != -1)
        {
            playerTurnText.text = "Player " + (winningPlayer + 1) + " wins!";
            AwardTrashToPlayer(winningPlayer);
        }
        else
        {
            playerTurnText.text = "No one wins!";
        }

        Invoke(nameof(ReturnToBoard), 2f);
    }

    private void AwardTrashToPlayer(int playerIndex)
    {
        DiceController dice = FindAnyObjectByType<DiceController>();
        if (dice == null)
            return;

        PlayerMovement player = dice.playersToMove[playerIndex];

        if (player != null)
        {
            for (int i = 0; i < 10; i++)
                player.SendMessage("IncrementGarbageCount", SendMessageOptions.DontRequireReceiver);
        }
    }

    private void ReturnToBoard()
    {
        SceneManager.LoadScene("Board 1"); // Your board scene
    }
}
