using UnityEngine;

public class DiceController : MonoBehaviour
{
    public PlayerMovement characterToMove;
    public int diceSides = 6;
    public Sprite[] diceSprites;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (diceSprites.Length < diceSides)
        {
            // The dice sprite will not change, but no error is logged.
        }
    }

    private void OnMouseDown()
    {
        // Only roll the dice if the character is not currently moving.
        if (characterToMove != null && !characterToMove.IsMoving)
        {
            RollAndMoveCharacter();
        }
    }

    public void RollAndMoveCharacter()
    {
        int rollResult = Random.Range(1, diceSides + 1);

        if (spriteRenderer != null && rollResult > 0 && rollResult <= diceSprites.Length)
        {
            spriteRenderer.sprite = diceSprites[rollResult - 1];
        }

        if (characterToMove != null)
        {
            characterToMove.MoveCharacter(rollResult);
        }
    }
}