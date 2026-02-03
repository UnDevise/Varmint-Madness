using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LocalCharacterSelect : MonoBehaviour
{
    public Button[] characterButtons;   // Assign all character buttons in Inspector
    public Image playerIndicator;       // UI element that shows which player is picking
    public Sprite[] playerIcons;        // 4 sprites: P1, P2, P3, P4

    private int totalPlayers = 4;
    private int currentPlayer = 0;      // 0 = Player 1, 1 = Player 2, etc.
    private int[] playerChoices;        // Stores each player's choice

    void Start()
    {
        playerChoices = new int[totalPlayers];

        UpdatePlayerIndicator();
        AssignButtonListeners();
    }

    void AssignButtonListeners()
    {
        for (int i = 0; i < characterButtons.Length; i++)
        {
            int index = i; // local copy for closure
            characterButtons[i].onClick.AddListener(() => SelectCharacter(index));
        }
    }

    void SelectCharacter(int index)
    {
        // Save the choice for the current player
        playerChoices[currentPlayer] = index;

        // Disable the chosen character so others can't pick it
        characterButtons[index].interactable = false;

        currentPlayer++;

        // If all players have chosen, finish
        if (currentPlayer >= totalPlayers)
        {
            FinishSelection();
            return;
        }

        UpdatePlayerIndicator();
    }

    void UpdatePlayerIndicator()
    {
        if (currentPlayer < playerIcons.Length)
            playerIndicator.sprite = playerIcons[currentPlayer];
    }

    void FinishSelection()
    {
        // Save all choices for the next scene
        for (int i = 0; i < totalPlayers; i++)
        {
            PlayerPrefs.SetInt("P" + (i + 1) + "_Character", playerChoices[i]);
        }

        // Load your 4‑player local multiplayer scene
        SceneManager.LoadScene("Board Select");
    }
}
