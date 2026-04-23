using UnityEngine;

public class PointerDetector : MonoBehaviour
{
    public Color[] availableColors;
    public SpriteRenderer spinner;

    private int currentColorIndex = 0;

    private void Start()
    {
        if (spinner == null)
        {
            Debug.LogError("PointerDetector: Spinner SpriteRenderer is not assigned.");
            return;
        }

        if (availableColors == null || availableColors.Length == 0)
        {
            Debug.LogError("PointerDetector: No colors assigned in availableColors.");
            return;
        }

        ApplyColor();
    }

    public void NextColor()
    {
        if (availableColors == null || availableColors.Length == 0) return;

        currentColorIndex++;
        if (currentColorIndex >= availableColors.Length)
            currentColorIndex = 0;

        ApplyColor();
    }

    public void PreviousColor()
    {
        if (availableColors == null || availableColors.Length == 0) return;

        currentColorIndex--;
        if (currentColorIndex < 0)
            currentColorIndex = availableColors.Length - 1;

        ApplyColor();
    }

    private void ApplyColor()
    {
        if (spinner == null) return;
        if (availableColors == null || availableColors.Length == 0) return;

        spinner.color = availableColors[currentColorIndex];
    }

    // ⭐ THIS is what SpinnerMinigameController is trying to use
    public Color CurrentColor => availableColors[currentColorIndex];
}