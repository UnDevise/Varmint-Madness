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

    void Start()
    {
        playerMarbleChoices = new int[totalPlayers];

        foreach (var selector in marbleSelectors)
            selector.HideUnpicked();

        UpdateTurnText();
    }

    public void PlayerPickedMarble(MarbleSelector marbleButton)
    {
        int marbleIndex = marbleButton.marbleIndex;

        playerMarbleChoices[currentPlayer - 1] = marbleIndex;

        marbleButton.DisableMarble();

        playersChosen++;
        currentPlayer++;

        foreach (var selector in marbleSelectors)
            selector.HideUnpicked();

        for (int i = 0; i < playersChosen; i++)
        {
            int chosenMarbleIndex = playerMarbleChoices[i];

            foreach (var selector in marbleSelectors)
            {
                if (selector.marbleIndex == chosenMarbleIndex)
                {
                    selector.EnableMarble();
                    break;
                }
            }
        }

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

        int winningPlayer = -1;

        for (int i = 0; i < totalPlayers; i++)
        {
            if (playerMarbleChoices[i] == marbleIndex)
            {
                winningPlayer = i;
                break;
            }
        }

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
        SceneManager.LoadScene("BoardSceneName"); // Replace with your board scene name
    }
}