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

    // Public variables for adjustable starting position and text size.
    public float startXPosition = 0f;
    public float startYPosition = -50f;
    public float textSize = 24f; // New variable to control the text size

    private SpriteRenderer spriteRenderer;
    private int currentPlayerIndex = 0;
    private bool isRolling = false;

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

            // Set the text size using the new public variable
            newText.fontSize = textSize;

            // Use the adjustable starting position and spacing.
            newText.rectTransform.anchoredPosition = new Vector2(startXPosition, startYPosition - i * uiElementSpacing);
            player.playerName = player.gameObject.name;

            // Immediately set the initial text to avoid showing "TEST".
            newText.text = $"{player.playerName}: 0 Garbage";

            player.garbageText = newText;
            player.SetDiceController(this);
        }
    }

    private void OnMouseDown()
    {
        if (playersToMove.Count == 0 || isRolling)
        {
            return;
        }

        PlayerMovement currentPlayer = playersToMove[currentPlayerIndex];

        if (currentPlayer.IsStunned)
        {
            Debug.Log($"{currentPlayer.playerName} is stunned and skips their turn.");
            currentPlayer.IsStunned = false;
            OnPlayerTurnFinished();
            return;
        }

        if (!currentPlayer.IsMoving)
        {
            RollAndMoveCharacter();
        }
    }

    public void RollAndMoveCharacter()
    {
        isRolling = true;

        int rollResult = Random.Range(1, diceSides + 1);

        if (spriteRenderer != null && rollResult > 0 && rollResult <= diceSprites.Length)
        {
            spriteRenderer.sprite = diceSprites[rollResult - 1];
        }

        PlayerMovement currentPlayer = playersToMove[currentPlayerIndex];

        if (currentPlayer != null)
        {
            currentPlayer.MoveCharacter(rollResult);
        }
    }

    public void OnPlayerTurnFinished()
    {
        isRolling = false;

        currentPlayerIndex++;
        if (currentPlayerIndex >= playersToMove.Count)
        {
            currentPlayerIndex = 0;
        }
    }
}