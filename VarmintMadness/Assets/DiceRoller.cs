using UnityEngine;

public class DiceController : MonoBehaviour
{
    // A reference to the PlayerMovement script on the character.
    public PlayerMovement characterToMove;

    // The number of sides on the dice.
    public int diceSides = 6;

    // An array to hold the 6 dice face sprites.
    public Sprite[] diceSprites;

    // A reference to the SpriteRenderer component of the dice.
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        // Get the SpriteRenderer component when the game starts.
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (diceSprites.Length < diceSides)
        {
            Debug.LogError("The Dice Sprites array does not contain enough sprites for the dice sides.", this);
        }
    }

    // Called when the dice sprite is clicked.
    private void OnMouseDown()
    {
        RollAndMoveCharacter();
    }

    /// <summary>
    /// Rolls the dice, changes its sprite, and tells the character to move.
    /// </summary>
    public void RollAndMoveCharacter()
    {
        // Generate a random number.
        int rollResult = Random.Range(1, diceSides + 1);

        Debug.Log("Dice rolled: " + rollResult);

        // Change the dice sprite based on the roll result.
        // We subtract 1 because array indices start at 0.
        if (spriteRenderer != null && rollResult > 0 && rollResult <= diceSprites.Length)
        {
            spriteRenderer.sprite = diceSprites[rollResult - 1];
        }

        // Tell the character to move, if the reference is not null.
        if (characterToMove != null)
        {
            characterToMove.MoveCharacter(rollResult);
        }
        else
        {
            Debug.LogError("CharacterToMove is not assigned in the Inspector!", this);
        }
    }
}