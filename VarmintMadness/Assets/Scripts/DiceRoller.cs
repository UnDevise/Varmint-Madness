using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DiceController : MonoBehaviour
{
    public List<PlayerMovement> playersToMove;
    public int diceSides = 6;
    public Sprite[] diceSprites;

    public TextMeshProUGUI playerGarbageTextPrefab;
    public Transform uiParentPanel;
    public float uiElementSpacing = 50f;

    public float startXPosition = 0f;
    public float startYPosition = -50f;
    public float textSize = 24f;

    // --- New variables for the 3D physics dice management ---
    public Transform physicsDiceTransform; // Assign the 3D dice GameObject here
    private Vector3 originalDicePosition;
    private Quaternion originalDiceRotation;
    // -----------------------------------------------------

    private SpriteRenderer spriteRenderer;
    private int currentPlayerIndex = 0;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (uiParentPanel == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
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

        // --- Store the original position of the physics dice ---
        if (physicsDiceTransform != null)
        {
            originalDicePosition = physicsDiceTransform.position;
            originalDiceRotation = physicsDiceTransform.rotation;
        }
    }

    public void MoveCurrentPlayer(int rollResult)
    {
        PlayerMovement currentPlayer = playersToMove[currentPlayerIndex];

        if (currentPlayer.IsStunned)
        {
            Debug.Log($"{currentPlayer.playerName} is stunned and skips their turn.");
            // OnPlayerTurnFinished() will be called after the player moves or skips
        }
        else
        {
            // Update the 2D sprite representation for visual consistency
            if (spriteRenderer != null && rollResult > 0 && rollResult <= diceSprites.Length)
            {
                spriteRenderer.sprite = diceSprites[rollResult - 1];
            }
        }

        if (currentPlayer != null)
        {
            // Move the character using the actual physics roll result
            // The PlayerMovement script should call OnPlayerTurnFinished()
            // once the movement is complete (e.g., in a movement coroutine callback).
            currentPlayer.MoveCharacter(rollResult);
        }
    }

    public void OnPlayerTurnFinished()
    {
        // Increment player index for the next turn
        currentPlayerIndex++;
        if (currentPlayerIndex >= playersToMove.Count)
        {
            currentPlayerIndex = 0;
        }

        // --- Reset the physical die's position and kinematic state ---
        if (physicsDiceTransform != null)
        {
            Rigidbody rb = physicsDiceTransform.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true; // Stop physics simulation
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // Instantly move it back to the start
            physicsDiceTransform.position = originalDicePosition;
            physicsDiceTransform.rotation = originalDiceRotation;

            // The DiceRoller script manages the 'canRoll' flag internally now, 
            // which is re-enabled by its CheckIfAtRest coroutine.
        }
    }
}
