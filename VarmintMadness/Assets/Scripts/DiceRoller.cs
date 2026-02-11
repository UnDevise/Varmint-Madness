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

    public Transform physicsDiceTransform;
    private Vector3 originalDicePosition;
    private Quaternion originalDiceRotation;

    private SpriteRenderer spriteRenderer;
    private int currentPlayerIndex = 0;

    private const float MovementThreshold = 0.01f;

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

    // Uses a variable (Rigidbody velocity) to determine whether the player is moving
    public bool IsPlayerMoving()
    {
        if (playersToMove.Count > 0 && playersToMove[currentPlayerIndex] != null)
        {
            // NOTE: The Player GameObject must have a Rigidbody attached for this to work.
            Rigidbody playerRb = playersToMove[currentPlayerIndex].GetComponent<Rigidbody>();

            if (playerRb != null)
            {
                // If velocity magnitude > threshold, player is moving, so return true
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
            currentPlayer.MoveCharacter(rollResult);
        }
    }

    public void OnPlayerTurnFinished()
    {
        currentPlayerIndex++;
        if (currentPlayerIndex >= playersToMove.Count)
        {
            currentPlayerIndex = 0;
        }

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
}
