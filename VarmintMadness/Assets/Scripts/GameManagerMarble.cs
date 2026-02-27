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

    // By player index: which marble each player owns
    private int[] playerMarbleChoices;

    // By pick order: which marbles were picked in sequence
    private int[] pickedMarblesInOrder;

    private bool winnerChosen = false;

    private int lastWinningMarbleIndex = -1;
    private int lastWinningPlayerIndex = -1;

    // How many players are actually allowed to pick (not in cage)
    private int eligiblePlayers = 0;

    void Start()
    {
        playerMarbleChoices = new int[totalPlayers];
        pickedMarblesInOrder = new int[totalPlayers];

        // Show all marbles at the start
        foreach (var selector in marbleSelectors)
            selector.EnableMarble();

        // Determine how many players are eligible (not in cage)
        CalculateEligiblePlayers();

        // Ensure we start on an eligible player
        SkipCagedPlayersAtStart();

        UpdateTurnText();
    }

    // Count how many players are NOT in a cage
    private void CalculateEligiblePlayers()
    {
        eligiblePlayers = 0;

        if (BoardStateSaver.playerIsInCage != null &&
            BoardStateSaver.playerIsInCage.Length >= totalPlayers)
        {
            for (int i = 0; i < totalPlayers; i++)
            {
                if (!BoardStateSaver.playerIsInCage[i])
                    eligiblePlayers++;
            }
        }
        else
        {
            // Fallback: if we don't have cage data, assume all are eligible
            eligiblePlayers = totalPlayers;
        }
    }

    // Skip caged players before the first turn
    private void SkipCagedPlayersAtStart()
    {
        if (BoardStateSaver.playerIsInCage == null ||
            BoardStateSaver.playerIsInCage.Length < totalPlayers)
            return;

        int safety = 0;
        while (BoardStateSaver.playerIsInCage[currentPlayer - 1])
        {
            currentPlayer++;
            if (currentPlayer > totalPlayers)
                currentPlayer = 1;

            safety++;
            if (safety > totalPlayers) break; // avoid infinite loop if all caged
        }
    }

    // Skip caged players when advancing turns
    private void AdvanceToNextEligiblePlayer()
    {
        if (BoardStateSaver.playerIsInCage == null ||
            BoardStateSaver.playerIsInCage.Length < totalPlayers)
        {
            // No cage data: just advance normally
            currentPlayer++;
            if (currentPlayer > totalPlayers)
                currentPlayer = 1;
            return;
        }

        int safety = 0;
        do
        {
            currentPlayer++;

            if (currentPlayer > totalPlayers)
                currentPlayer = 1;

            safety++;
            if (safety > totalPlayers) break; // all caged? then stop

        } while (BoardStateSaver.playerIsInCage[currentPlayer - 1]);
    }

    public void PlayerPickedMarble(MarbleSelector marbleButton)
    {
        int marbleIndex = marbleButton.marbleIndex;
        int playerIndex = currentPlayer - 1;

        // Save the choice for this player index
        playerMarbleChoices[playerIndex] = marbleIndex;

        // Save the choice in pick order
        pickedMarblesInOrder[playersChosen] = marbleIndex;

        // Disable this marble so it cannot be picked again
        marbleButton.DisableMarble();

        playersChosen++;

        // Move to the next eligible (non-caged) player
        if (playersChosen < eligiblePlayers)
            AdvanceToNextEligiblePlayer();

        // Hide ALL marbles
        foreach (var selector in marbleSelectors)
            selector.gameObject.SetActive(false);

        // Show chosen marbles in the order they were picked
        for (int i = 0; i < playersChosen; i++)
        {
            int chosenIndex = pickedMarblesInOrder[i];

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
        if (playersChosen < eligiblePlayers)
        {
            foreach (var selector in marbleSelectors)
            {
                if (selector.enabled)
                    selector.gameObject.SetActive(true);
            }
        }

        // If all eligible players have chosen, start the race
        if (playersChosen >= eligiblePlayers)
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

        // Start only the marbles that were actually picked
        for (int i = 0; i < playersChosen; i++)
        {
            int chosenMarbleIndex = pickedMarblesInOrder[i];

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

        // Find which player owned the winning marble
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
        SceneManager.LoadScene("Board 1");
    }
}