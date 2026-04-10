using UnityEngine;
using TMPro;

public class DebugOverlay : MonoBehaviour
{
    public DiceController diceController;
    public TextMeshProUGUI debugText;

    [Header("Toggle Key")]
    public KeyCode toggleKey = KeyCode.F3;

    private bool isVisible = false;

    void Start()
    {
        if (debugText != null)
            debugText.gameObject.SetActive(isVisible);
    }

    void Update()
    {
        // Toggle visibility
        if (Input.GetKeyDown(toggleKey))
        {
            isVisible = !isVisible;
            debugText.gameObject.SetActive(isVisible);
        }

        if (!isVisible) return;
        if (diceController == null || debugText == null) return;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.AppendLine("<b>=== DEBUG OVERLAY ===</b>\n");

        for (int i = 0; i < diceController.playersToMove.Count; i++)
        {
            PlayerMovement p = diceController.playersToMove[i];

            sb.AppendLine($"<b>Player {i + 1}</b>");
            sb.AppendLine($"Name: {p.playerName}");
            sb.AppendLine($"Character ID: {p.characterId}");
            sb.AppendLine($"Garbage: {p.garbageCount}");
            sb.AppendLine($"Tile Index: {p.CurrentPositionIndex}");
            sb.AppendLine($"Board Layer: {(p.waypointsParent == p.alternativeWaypointsParent ? "Sewer" : "Top")}");
            sb.AppendLine($"In Cage: {p.IsInCage}");
            sb.AppendLine($"Stunned: {p.IsStunned}");
            sb.AppendLine("");
        }

        debugText.text = sb.ToString();
    }
}
