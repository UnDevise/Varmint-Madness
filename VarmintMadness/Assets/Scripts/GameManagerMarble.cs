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
    private int[] pickedMarblesInOrder;

    private bool winnerChosen = false;

    private int eligiblePlayers = 0;

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

    private void CalculateEligiblePlayers()
    {
        eligiblePlayers = 0;

        for (int i = 0; i < totalPlayers; i++)
        {
            if (!BoardStateSaver.playerIsInCage[i])
                eligiblePlayers++;
        }
    }

    private void SkipCagedPlayersAtStart()
    {
        int safety = 0;
        while (BoardStateSaver.playerIsInCage[currentPlayer - 1])
        {
            currentPlayer++;
            if (currentPlayer > totalPlayers)
                currentPlayer = 1;

            safety++;
            if (safety > totalPlayers)
                break;
        }
    }

    private void AdvanceToNextEligiblePlayer()
    {
        int safety = 0;

        do
        {
            currentPlayer++;
            if (currentPlayer > totalPlayers)
                currentPlayer = 1;

            safety++;
            if (safety > totalPlayers)
                break;

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

    private void PushAllMarblesFree()
    {
        for (int i = 0; i < playersChosen; i++)
        {
            int chosenIndex = pickedMarblesInOrder[i];

            foreach (var marble in marbles)
            {
                if (marble.marbleIndex == chosenIndex)
                    marble.PushFree();
            }
        }
    }

    public void MarbleReachedFinish(int marbleIndex)
    {
        if (winnerChosen)
            return;

        winnerChosen = true;

        int winningPlayerIndex = -1;

        for (int i = 0; i < totalPlayers; i++)
        {
            if (playerMarbleChoices[i] == marbleIndex)
            {
                winningPlayerIndex = i;
                break;
            }
        }

        playerTurnText.gameObject.SetActive(true);

        if (winningPlayerIndex != -1)
        {
            playerTurnText.text = "Player " + (winningPlayerIndex + 1) + " wins!";

            MarbleRewardData.WinnerPlayerIndices.Clear();
            MarbleRewardData.WinnerPlayerIndices.Add(winningPlayerIndex);
        }
        else
        {
            playerTurnText.text = "No one wins!";
            MarbleRewardData.WinnerPlayerIndices = null;
            MarbleRewardData.BonusTrash = 0;
        }

        Invoke(nameof(ReturnToBoard), 2f);
    }

    private void ReturnToBoard()
    {
        // Safety check: ensure the board scene name is valid
        if (string.IsNullOrEmpty(BoardStateSaver.lastBoardSceneName))
        {
            Debug.LogWarning("BoardStateSaver.lastBoardSceneName was NULL — using fallback board scene!");
            BoardStateSaver.lastBoardSceneName = "Board 1"; // your real board scene
        }

        // Always go through the loading scene
        SceneManager.LoadScene("LoadingScene");
    }
}