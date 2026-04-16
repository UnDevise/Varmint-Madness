using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpinnerMinigameController : MonoBehaviour
{
    public Rigidbody2D spinnerRb;
    public float minSpinForce = 500f;
    public float maxSpinForce = 1200f;
    private bool hasSpun = false;
    public List<string> availableColors = new List<string>();
    public GameObject colorBoxPrefab;
    public Transform canvasParent; // assign your Canvas here


    void Start()
    {
        AssignRandomColorsToPlayers();
    }



    // Player picks a color → store it here
    private Dictionary<string, string> playerColorChoice = new Dictionary<string, string>();

    public void PlayerPickColor(string playerId, string color)
    {
        playerColorChoice[playerId] = color;
        Debug.Log(playerId + " picked " + color);
    }

    public void SpinWheel()
    {
        float randomForce = Random.Range(minSpinForce, maxSpinForce);
        spinnerRb.AddTorque(randomForce);

        hasSpun = true; // <-- IMPORTANT
    }

    public PointerDetector pointer;
    public float stopThreshold = 5f; // how slow the wheel must be to count as stopped
    private bool resultChecked = false;

    void Update()
    {
        if (!hasSpun) return; // don't check until spun

        if (!resultChecked && Mathf.Abs(spinnerRb.angularVelocity) < stopThreshold)
        {
            resultChecked = true;
            CheckWinner();
        }
    }

    void CheckWinner()
    {
        string winningColor = pointer.currentColor;
        Debug.Log("Spinner landed on: " + winningColor);

        foreach (var entry in playerColorChoice)
        {
            string playerId = entry.Key;
            string chosenColor = entry.Value;

            if (chosenColor == winningColor)
            {
                AwardGarbage(playerId);
            }
        }

        ReturnToBoard();
    }
    void AwardGarbage(string playerId)
    {
        PlayerMovement[] players = FindObjectsOfType<PlayerMovement>();

        foreach (var p in players)
        {
            if (p.playerId == playerId)
            {
                p.garbageCount += 10;
                p.UpdateGarbageText();
                Debug.Log(playerId + " won 10 garbage!");
            }
        }
    }
    void ReturnToBoard()
    {
        SceneManager.LoadScene("LoadingScene");
    }
    public void AssignRandomColorsToPlayers()
    {
        playerColorChoice.Clear();

        PlayerMovement[] players = FindObjectsOfType<PlayerMovement>();

        foreach (var p in players)
        {
            string randomColorName = availableColors[Random.Range(0, availableColors.Count)];
            playerColorChoice[p.playerId] = randomColorName;

            // Convert string → Unity Color
            Color colorValue;
            if (!ColorUtility.TryParseHtmlString(randomColorName, out colorValue))
                colorValue = Color.white;

            // Spawn UI box
            GameObject box = Instantiate(colorBoxPrefab, canvasParent);
            PlayerColorBoxUI ui = box.GetComponent<PlayerColorBoxUI>();

            ui.target = p.transform; // follow the player
            ui.SetColor(colorValue);

            Debug.Log($"{p.playerName} assigned color: {randomColorName}");
        }
    }
}