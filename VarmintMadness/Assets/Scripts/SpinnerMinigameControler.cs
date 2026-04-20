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
    private bool hasSpun = false;   // <-- prevents instant finish
    private bool canSpin = true;    // <-- prevents double spins

    // Player → assigned color
    private Dictionary<string, string> playerColorChoice = new Dictionary<string, string>();

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
            string randomColor = pointer.availableColors[Random.Range(0, pointer.availableColors.Count)];
            playerColorChoice[p.playerId] = randomColor;

            Debug.Log($"{p.playerName} assigned color: {randomColor}");
        }
    }

    void CheckWinner()
    {
        string winningColor = pointer.currentColor;
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
}