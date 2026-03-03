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

    // ⭐ Stuck detection
    private float stuckCheckInterval = 1.0f;
    private float stuckCheckTimer = 0f;
    private bool raceStarted = false;

    void Start()
    {
        playerMarbleChoices = new int[totalPlayers];
        pickedMarblesInOrder = new int[totalPlayers];

        foreach (var selector in marbleSelectors)
            selector.EnableMarble();

        CalculateEligiblePlayers();
        SkipCagedPlayersAtStart();
        UpdateTurnText();
    }

    private void Update()
    {
        if (!raceStarted)
            return;

        stuckCheckTimer += Time.deltaTime;

        if (stuckCheckTimer >= stuckCheckInterval)
        {
            stuckCheckTimer = 0f;
            CheckForStuckMarbles();
        }
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
            eligiblePlayers = totalPlayers;
        }
    }

    private void SkipCagedPlayersAtStart()
    {
        if (BoardStateSaver.playerIsInCage == null)
            return;

        int safety = 0;
        while (BoardStateSaver.playerIsInCage[currentPlayer - 1])
        {
            currentPlayer++;
            if (currentPlayer > totalPlayers)
                currentPlayer = 1;

            safety++;
            if (safety > totalPlayers) break;
        }
    }

    private void AdvanceToNextEligiblePlayer()
    {
        if (BoardStateSaver.playerIsInCage == null)
        {
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
            if (safety > totalPlayers) break;

        } while (BoardStateSaver.playerIsInCage[currentPlayer - 1]);
    }

    public void PlayerPickedMarble(MarbleSelector marbleButton)
    {
        int marbleIndex = marbleButton.marbleIndex;
        int playerIndex = currentPlayer - 1;

        playerMarbleChoices[playerIndex] = marbleIndex;
        pickedMarblesInOrder[playersChosen] = marbleIndex;

        marbleButton.DisableMarble();

        playersChosen++;

        if (playersChosen < eligiblePlayers)
            AdvanceToNextEligiblePlayer();

        foreach (var selector in marbleSelectors)
            selector.gameObject.SetActive(false);

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

        if (playersChosen < eligiblePlayers)
        {
            foreach (var selector in marbleSelectors)
            {
                if (selector.enabled)
                    selector.gameObject.SetActive(true);
            }
        }

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
        raceStarted = true;

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

    // ⭐ Detect if all marbles are stuck
    private void CheckForStuckMarbles()
    {
        bool allStuck = true;

        for (int i = 0; i < playersChosen; i++)
        {
            int chosenIndex = pickedMarblesInOrder[i];

            foreach (var marble in marbles)
            {
                if (marble.marbleIndex == chosenIndex)
                {
                    marble.CheckIfStuck();

                    if (!marble.IsStuck)
                        allStuck = false;
                }
            }
        }

        if (allStuck)
            PushAllMarblesFree();
    }

    // ⭐ Push all marbles free
    private void PushAllMarblesFree()
    {
        for (int i = 0; i < playersChosen; i++)
        {
            int chosenIndex = pickedMarblesInOrder[i];

            foreach (var marble in marbles)
            {
                if (marble.marbleIndex == chosenIndex)
                {
                    marble.PushFree();
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
        SceneManager.LoadScene("Board 1");
    }
}
