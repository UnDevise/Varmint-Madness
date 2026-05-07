using UnityEngine;
using TMPro;

/// <summary>
/// ScoreboardUI — Optional sidebar scoreboard that lists all 4 players and
/// their live scores.  Refreshes automatically via the PlayerController events.
///
/// Attach to a Canvas panel that contains 4 rows (one per player).
/// </summary>
public class ScoreboardUI : MonoBehaviour
{
    [System.Serializable]
    public class ScoreRow
    {
        [Tooltip("TMP label for the player name in this row.")]
        public TextMeshProUGUI nameText;

        [Tooltip("TMP label for the player score in this row.")]
        public TextMeshProUGUI scoreText;

        [Tooltip("(Optional) Image/panel that highlights the active player's row.")]
        public GameObject activeHighlight;
    }

    [Header("── References ──")]
    public GameShowManager gameShowManager;
    public ScoreRow[] rows; // Exactly 4 rows

    [Header("── Style ──")]
    public Color activeRowColor   = new Color(1f, 0.85f, 0f, 0.3f);
    public Color inactiveRowColor = new Color(1f, 1f, 1f, 0f);

    private void Start()
    {
        if (gameShowManager == null)
            gameShowManager = FindObjectOfType<GameShowManager>();

        RefreshAll();
        InvokeRepeating(nameof(RefreshAll), 0.5f, 0.5f); // poll every half-second
    }

    public void RefreshAll()
    {
        if (gameShowManager == null) return;

        for (int i = 0; i < rows.Length && i < gameShowManager.players.Length; i++)
        {
            PlayerController p = gameShowManager.players[i];
            if (p == null) continue;

            rows[i].nameText.text  = p.playerName;
            rows[i].scoreText.text = p.Points.ToString("N0");

            if (rows[i].activeHighlight != null)
                rows[i].activeHighlight.SetActive(false); // managed by GameShowManager separately
        }
    }

    /// <summary>Called by GameShowManager to highlight the current player's row.</summary>
    public void SetActivePlayer(int index)
    {
        for (int i = 0; i < rows.Length; i++)
            if (rows[i].activeHighlight != null)
                rows[i].activeHighlight.SetActive(i == index);
    }
}
