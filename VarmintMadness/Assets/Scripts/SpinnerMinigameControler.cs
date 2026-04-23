using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpinnerMinigameController : MonoBehaviour
{
    public Rigidbody2D spinnerRb;
    public float minSpinForce = 500f;
    public float maxSpinForce = 1200f;

    public PointerDetector pointer;
    public float stopThreshold = 5f;

    private bool resultChecked = false;
    private bool hasSpun = false;
    private bool canSpin = true;

    // Player → assigned color
    Dictionary<int, Color> playerColorChoice = new Dictionary<int, Color>();

    void Start()
    {
        AssignRandomColorsToPlayers();
    }

    public void SpinWheel()
    {
        if (!canSpin) return;

        canSpin = false;
        hasSpun = true;

        float randomForce = Random.Range(minSpinForce, maxSpinForce);
        spinnerRb.AddTorque(randomForce, ForceMode2D.Impulse);

        Debug.Log("SPIN BUTTON CLICKED — torque applied: " + randomForce);
    }

    void Update()
    {
        if (!hasSpun) return;

        if (!resultChecked && Mathf.Abs(spinnerRb.angularVelocity) < stopThreshold)
        {
            resultChecked = true;
            CheckWinner();
        }
    }

    void AssignRandomColorsToPlayers()
    {
        playerColorChoice.Clear();

        PlayerMovement[] players = FindObjectsOfType<PlayerMovement>();

        foreach (var p in players)
        {
            Color randomColor = pointer.availableColors[Random.Range(0, pointer.availableColors.Length)];

            playerColorChoice[p.playerID] = randomColor;

            Debug.Log($"{p.playerName} assigned color: {randomColor}");
        }
    }

    void CheckWinner()
    {
        Color winningColor = pointer.CurrentColor;
        Debug.Log("Spinner landed on: " + winningColor);

        foreach (var entry in playerColorChoice)
        {
            if (entry.Value == winningColor)
            {
                AwardGarbage(entry.Key);
            }
        }

        ReturnToBoard();
    }

    void AwardGarbage(int playerID)
    {
        PlayerMovement[] players = FindObjectsOfType<PlayerMovement>();

        foreach (var p in players)
        {
            if (p.playerID == playerID)
            {
                p.garbageCount += 10;
                p.UpdateGarbageText();
                Debug.Log($"Player {playerID} won 10 garbage!");
            }
        }
    }

    void ReturnToBoard()
    {
        SceneManager.LoadScene("LoadingScene");
    }
}
