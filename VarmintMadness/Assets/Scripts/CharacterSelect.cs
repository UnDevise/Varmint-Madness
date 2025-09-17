using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;



public class CharacterSelectUI : MonoBehaviour
{
    [SerializeField] private GameObject RacoonSelect;
    [SerializeField] private GameObject FoxSelect;
    [SerializeField] private GameObject SquirrelSelect;
    [SerializeField] private GameObject PossumSelect;

    [System.Serializable]

    public class PlayerPanel
    {
        public string playerName = "Player 1";
        public Transform spawnPoint;

        // One button per character option, in the same order as characterPrefabs.
        public Button[] characterButtons;

        public Button readyButton;
        public Image selectionHighlight; // optional (moves to the selected button)

        [HideInInspector] public int selectedIndex = -1;
        [HideInInspector] public bool isReady = false;
    }

    [Header("Character Options")]
    public GameObject[] characterPrefabs;

    [Header("Players")]
    public PlayerPanel[] players;

    [Header("Rules")]
    public bool allowDuplicatePicks = false;

    private HashSet<int> takenChoices = new HashSet<int>();

    void Awake()
    {
        // Basic validation
        foreach (var p in players)
        {
            if (p.characterButtons == null || p.characterButtons.Length != characterPrefabs.Length)
            {
                Debug.LogError($"[{nameof(CharacterSelectUI)}] {p.playerName} must have exactly {characterPrefabs.Length} character buttons to match Character Prefabs.");
            }
            if (p.readyButton) p.readyButton.interactable = false; // enable after a pick
        }

        // Wire up button listeners
        for (int pIndex = 0; pIndex < players.Length; pIndex++)
        {
            int capturedPIndex = pIndex;
            var panel = players[pIndex];

            for (int choice = 0; choice < panel.characterButtons.Length; choice++)
            {
                int capturedChoice = choice;
                panel.characterButtons[choice].onClick.AddListener(() => OnPick(capturedPIndex, capturedChoice));
            }

            if (panel.readyButton != null)
            {
                panel.readyButton.onClick.AddListener(() => OnReady(capturedPIndex));
            }
        }
    }

    private void OnPick(int pIndex, int choice)
    {
        var panel = players[pIndex];
        if (panel.isReady) return;

        // Enforce no-duplicate rule if desired
        if (!allowDuplicatePicks)
        {
            // If another player has already locked in this choice, block it
            if (takenChoices.Contains(choice) && panel.selectedIndex != choice)
            {
                Debug.Log($"{panel.playerName} tried to pick an already taken character.");
                return;
            }

            // Free previous pick (if any)
            if (panel.selectedIndex != -1 && panel.selectedIndex != choice)
            {
                takenChoices.Remove(panel.selectedIndex);
            }

            takenChoices.Add(choice);
        }

        panel.selectedIndex = choice;

        // Move highlight under/onto the selected button
        if (panel.selectionHighlight != null)
        {
            panel.selectionHighlight.transform.SetParent(panel.characterButtons[choice].transform, false);
            panel.selectionHighlight.rectTransform.anchoredPosition = Vector2.zero;
            panel.selectionHighlight.gameObject.SetActive(true);
        }

        // Enable Ready once a selection is made
        if (panel.readyButton != null) panel.readyButton.interactable = true;
    }

    private void OnReady(int pIndex)
    {
        var panel = players[pIndex];
        if (panel.selectedIndex < 0 || panel.isReady) return;

        panel.isReady = true;

        // Spawn selected character at the player's spawn point
        var prefab = characterPrefabs[panel.selectedIndex];
        if (prefab != null && panel.spawnPoint != null)
        {
            Instantiate(prefab, panel.spawnPoint.position, Quaternion.identity);
        }

        // Lock UI for this player
        foreach (var b in panel.characterButtons) b.interactable = false;
        if (panel.readyButton != null) panel.readyButton.interactable = false;

        // Optionally dim highlight to show "locked"
        if (panel.selectionHighlight != null)
        {
            var c = panel.selectionHighlight.color;
            panel.selectionHighlight.color = new Color(c.r, c.g, c.b, 0.6f);
        }

        if (AllReady())
        {
            StartGame();
        }
    }

    private bool AllReady()
    {
        foreach (var p in players)
        {
            if (!p.isReady) return false;
        }
        return true;
    }

    private void StartGame()
    {
        Debug.Log("All players ready — start the game / load next scene here.");
        // e.g., SceneManager.LoadScene("GameScene");
    }
}

