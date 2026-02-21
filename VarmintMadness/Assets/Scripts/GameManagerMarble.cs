using UnityEngine;
using TMPro;

public class GameManagerMarble : MonoBehaviour
{
    public MarbleMovement[] marbles;          // Assign all marbles in Inspector
    public MarbleSelector[] marbleSelectors;  // Assign all selectors in Inspector
    public TextMeshProUGUI playerTurnText;

    public int totalPlayers = 4;
    private int currentPlayer = 1;
    private int playersChosen = 0;

    private int[] playerMarbleChoices;
    private bool winnerChosen = false;

    void Start()
    {
        playerMarbleChoices = new int[totalPlayers];

        // Hide all marbles at the start
        foreach (var selector in marbleSelectors)
            selector.HideUnpicked();

        UpdateTurnText();
    }

    public void PlayerPickedMarble(MarbleSelector marbleButton)
    {
        int marbleIndex = marbleButton.marbleIndex;

        // Save the choice (this is the marbleIndex, not array index)
        playerMarbleChoices[currentPlayer - 1] = marbleIndex;

        // Disable this marble so it can't be picked again
        marbleButton.DisableMarble();

        playersChosen++;
        currentPlayer++;

        // Hide all marbles
        foreach (var selector in marbleSelectors)
            selector.HideUnpicked();

        // Show only chosen marbles
        for (int i = 0; i < playersChosen; i++)
        {
            int chosenMarbleIndex = playerMarbleChoices[i];

            // Find the selector with this marbleIndex
            foreach (var selector in marbleSelectors)
            {
                if (selector.marbleIndex == chosenMarbleIndex)
                {
                    selector.EnableMarble();
                    break;
                }
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

        // Start race ONLY for chosen marbles
        for (int i = 0; i < playersChosen; i++)
        {
            int chosenMarbleIndex = playerMarbleChoices[i];

            // Find the MarbleMovement with matching index
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

        int winningPlayer = -1;

        for (int i = 0; i < totalPlayers; i++)
        {
            if (playerMarbleChoices[i] == marbleIndex)
            {
                winningPlayer = i + 1;
                break;
            }
        }

        playerTurnText.gameObject.SetActive(true);

        if (winningPlayer != -1)
            playerTurnText.text = "Player " + winningPlayer + " wins!";
        else
            playerTurnText.text = "No one wins!";
    }
}




