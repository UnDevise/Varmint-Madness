using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class DiceController : MonoBehaviour
{
    public List<PlayerMovement> playersToMove;
    public int diceSides = 6;
    public Sprite[] diceSprites;

    public TextMeshProUGUI playerGarbageTextPrefab;
    public Transform uiParentPanel;
    public float uiElementSpacing = 50f;
    public int currentPlayerIndex = 0;

    public float startXPosition = 0f;
    public float startYPosition = -50f;
    public float textSize = 24f;

    public Transform physicsDiceTransform;
    private Vector3 originalDicePosition;
    private Quaternion originalDiceRotation;

    private SpriteRenderer spriteRenderer;

    private const float MovementThreshold = 0.01f;

    private int turnsCompleted = 0;

    [Header("Minigame Settings")]
    public List<string> roundMinigames = new List<string>();

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (uiParentPanel == null)
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                uiParentPanel = canvas.transform;
            }
        }

        for (int i = 0; i < playersToMove.Count; i++)
        {
            PlayerMovement player = playersToMove[i];
            TextMeshProUGUI newText = Instantiate(playerGarbageTextPrefab, uiParentPanel);
            newText.fontSize = textSize;
            newText.rectTransform.anchoredPosition = new Vector2(startXPosition, startYPosition - i * uiElementSpacing);
            player.playerName = player.gameObject.name;
            newText.text = $"{player.playerName}: 0 Garbage";
            player.garbageText = newText;
            player.SetDiceController(this);
        }

        if (physicsDiceTransform != null)
        {
            originalDicePosition = physicsDiceTransform.position;
            originalDiceRotation = physicsDiceTransform.rotation;
        }
    }

    private void Start()
    {
        // Restore camera follow after returning from a minigame
        if (CameraController.Instance != null && playersToMove.Count > 0)
        {
            CameraController.Instance.FocusOnPlayer(playersToMove[currentPlayerIndex].transform);
        }

        // Start the first player's turn
        StartPlayerTurn();
    }

    // ---------------------------------------------------------
    // TURN ENTRY POINT
    // ---------------------------------------------------------
    public void StartPlayerTurn()
    {
        CheckForWinner();

        PlayerMovement current = playersToMove[currentPlayerIndex];

        if (current.ShouldSkipTurn())
        {
            Debug.Log($"{current.playerName} skips their turn immediately.");
            current.IsStunned = false;
            OnPlayerTurnFinished();
            return;
        }

        if (CameraController.Instance != null)
            CameraController.Instance.FocusOnDice(physicsDiceTransform);
    }

    public bool IsPlayerMoving()
    {
        if (playersToMove.Count > 0 && playersToMove[currentPlayerIndex] != null)
        {
            Rigidbody playerRb = playersToMove[currentPlayerIndex].GetComponent<Rigidbody>();

            if (playerRb != null)
            {
                return playerRb.linearVelocity.sqrMagnitude > MovementThreshold;
            }
            return false;
        }
        return false;
    }

    public void MoveCurrentPlayer(int rollResult)
    {
        PlayerMovement currentPlayer = playersToMove[currentPlayerIndex];

        if (currentPlayer.IsStunned)
        {
            Debug.Log($"{currentPlayer.playerName} is stunned and skips their turn.");
        }
        else
        {
            if (spriteRenderer != null && rollResult > 0 && rollResult <= diceSprites.Length)
            {
                spriteRenderer.sprite = diceSprites[rollResult - 1];
            }
        }

        if (currentPlayer != null)
        {
            // Camera switches to player when they start moving
            if (CameraController.Instance != null)
                CameraController.Instance.FocusOnPlayer(currentPlayer.transform);

            currentPlayer.MoveCharacter(rollResult);
        }
    }

    public void RollAgain()
    {
        int rollResult = Random.Range(1, diceSides + 1);
        Debug.Log($"Roll Again triggered! Rolled a {rollResult}");
        MoveCurrentPlayer(rollResult);
    }

    public void OnPlayerTurnFinished()
    {
        turnsCompleted++;

        if (turnsCompleted >= playersToMove.Count)
        {
            turnsCompleted = 0;
            StartMinigameRound();
            return;
        }

        currentPlayerIndex++;
        if (currentPlayerIndex >= playersToMove.Count)
        {
            currentPlayerIndex = 0;
        }

        ResetDicePhysics();

        // ⭐ Start next player's turn
        StartPlayerTurn();
    }

    private void ResetDicePhysics()
    {
        if (physicsDiceTransform != null)
        {
            Rigidbody rb = physicsDiceTransform.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            physicsDiceTransform.position = originalDicePosition;
            physicsDiceTransform.rotation = originalDiceRotation;
        }
    }

    private void StartMinigameRound()
    {
        Debug.Log("All players have moved. Starting a random minigame!");

        // Save which board each player is on
        BoardStateSaver.SaveBoardState(playersToMove.Count, this);

        currentPlayerIndex = 0;

        if (roundMinigames == null || roundMinigames.Count == 0)
        {
            Debug.LogError("No minigames assigned!");
            return;
        }

        int index = Random.Range(0, roundMinigames.Count);
        string selectedMinigame = roundMinigames[index];

        SceneManager.LoadScene(selectedMinigame);
    }
    public void CheckForWinner()
    {
        int activePlayers = 0;
        PlayerMovement lastStanding = null;

        foreach (PlayerMovement p in playersToMove)
        {
            if (!p.IsInCage)
            {
                activePlayers++;
                lastStanding = p;
            }
        }

        if (activePlayers == 1 && lastStanding != null)
        {
            Debug.Log($"{lastStanding.playerName} WINS THE GAME!");

            // ⭐ Save winner name and prefab path
            WinnerData.WinnerName = lastStanding.playerName;
            WinnerData.WinnerPrefab = lastStanding.gameObject;

            SceneManager.LoadScene("Winner");
        }
    }
}
