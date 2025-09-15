using System.Collections.Generic;
using UnityEngine;

public class DiceController : MonoBehaviour
{
    public List<PlayerMovement> playersToMove;
    public int diceSides = 6;
    public Sprite[] diceSprites;

    private SpriteRenderer spriteRenderer;
    private int currentPlayerIndex = 0;
    private bool isRolling = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        foreach (PlayerMovement player in playersToMove)
        {
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

        // If the current player is stunned, skip their turn.
        if (currentPlayer.IsStunned)
        {
            Debug.Log(currentPlayer.gameObject.name + " is stunned and skips their turn.");
            currentPlayer.IsStunned = false; // Reset the stun status
            OnPlayerTurnFinished(); // Immediately advance to the next player
            return;
        }

        // Only roll the dice if the character is not currently moving.
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

        // Advance to the next player's turn.
        currentPlayerIndex++;
        if (currentPlayerIndex >= playersToMove.Count)
        {
            currentPlayerIndex = 0;
        }
    }
}