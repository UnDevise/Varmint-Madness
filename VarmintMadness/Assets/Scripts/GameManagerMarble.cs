using UnityEngine;
using TMPro;

public class GameManagerMarble : MonoBehaviour
{
    public MarbleMovement[] marbles;          // Assign all marbles in Inspector
    public TextMeshProUGUI playerTurnText;    // Drag your UI text here

    public int totalPlayers = 4;              // Set number of players (2–4)
    private int currentPlayer = 1;
    private int playersChosen = 0;

    private int[] playerMarbleChoices;

    void Start()
    {
        playerMarbleChoices = new int[totalPlayers];
        UpdateTurnText();
    }

    // Called when a player clicks a marble
    public void PlayerPickedMarble(MarbleSelector marbleButton)
    {
        int marbleIndex = marbleButton.marbleIndex;

        // Save the choice
        playerMarbleChoices[currentPlayer - 1] = marbleIndex;

        // Disable the marble so others can't pick it
        marbleButton.DisableMarble();

        playersChosen++;
        currentPlayer++;

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
        // Hide the picking text
        playerTurnText.gameObject.SetActive(false);

        foreach (MarbleMovement marble in marbles)
        {
            marble.StartRace();
        }
    }

    // Called by the finish block
    public void MarbleReachedFinish(int marbleIndex)
    {
        int winningPlayer = -1;

        // Find which player picked this marble
        for (int i = 0; i < totalPlayers; i++)
        {
            if (playerMarbleChoices[i] == marbleIndex)
            {
                winningPlayer = i + 1;
                break;
            }
        }

        // Show the text again
        playerTurnText.gameObject.SetActive(true);

        if (winningPlayer != -1)
            playerTurnText.text = "Player " + winningPlayer + " wins!";
        else
            playerTurnText.text = "No one wins!";
    }
}




