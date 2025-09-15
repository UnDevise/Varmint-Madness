using Mono.Cecil.Cil;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEngine.Rendering.DebugUI.Table;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterSelect : MonoBehaviour
{
    public GameObject[] characters; // Array of character prefabs
    public Transform[] playerPositions; // Positions for selected characters
    private int[] selectedIndices; // Tracks selected character for each player
    private bool[] isReady; // Tracks if players are ready

    private int playerCount = 4; // Number of players (adjust as needed)

    void Start()
    {
        selectedIndices = new int[playerCount];
        isReady = new bool[playerCount];
    }

    void Update()
    {
        for (int i = 0; i < playerCount; i++)
        {
            if (!isReady[i])
            {
                HandleInput(i);
            }
        }

        if (AllPlayersReady())
        {
            StartGame();
        }
    }

    void HandleInput(int playerIndex)
    {
        string horizontalAxis = $"Player{playerIndex + 1}_Horizontal";
        string submitButton = $"Player{playerIndex + 1}_Submit";

        if (Input.GetAxisRaw(horizontalAxis) > 0)
        {
            selectedIndices[playerIndex] = (selectedIndices[playerIndex] + 1) % characters.Length;
        }
        else if (Input.GetAxisRaw(horizontalAxis) < 0)
        {
            selectedIndices[playerIndex] = (selectedIndices[playerIndex] - 1 + characters.Length) % characters.Length;
        }

        if (Input.GetButtonDown(submitButton))
        {
            isReady[playerIndex] = true;
            Instantiate(characters[selectedIndices[playerIndex]], playerPositions[playerIndex].position, Quaternion.identity);
        }
    }

    bool AllPlayersReady()
    {
        foreach (bool ready in isReady)
        {
            if (!ready) return false;
        }
        return true;
    }

    void StartGame()
    {
        Debug.Log("All players are ready! Starting the game...");
        // Load the next scene or start the game logic
    }
}

