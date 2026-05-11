using UnityEngine;

// Attach this to a GameObject in each minigame scene.
// Maps selected characters to their correct player GameObjects by character index,
// in the order players selected them.

public class MinigameCharacterApplier : MonoBehaviour
{
    [Header("Assign all 4 player GameObjects matching their CHARACTER index order")]
    [Tooltip("Slot 0 = character index 0, Slot 1 = character index 1, etc. Match the character select screen order.")]
    public GameObject[] playerObjectsByCharacterIndex;

    [Header("Assign all character sprites in character select screen order")]
    public Sprite[] characterSprites;

    [Header("Optional: Animator controllers in character select screen order")]
    public RuntimeAnimatorController[] characterAnimators;

    // Ordered list of active player GameObjects in selection order (Player 1 first)
    [HideInInspector]
    public GameObject[] orderedActivePlayers;

    void Awake()
    {
        int totalPlayers = PlayerPrefs.GetInt("TotalPlayers", 4);

        // First disable ALL player objects
        foreach (var obj in playerObjectsByCharacterIndex)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        // Use BoardStateSaver selection order if available, otherwise fall back to PlayerPrefs
        int[] selectionOrder = BoardStateSaver.playerSelectionOrder;

        orderedActivePlayers = new GameObject[totalPlayers];

        // Enable and apply sprites in player selection order
        for (int playerSlot = 0; playerSlot < totalPlayers; playerSlot++)
        {
            int charIndex;

            if (selectionOrder != null && playerSlot < selectionOrder.Length)
                charIndex = selectionOrder[playerSlot];
            else
                charIndex = PlayerPrefs.GetInt($"P{playerSlot + 1}_Character", -1);

            Debug.Log($"MinigameCharacterApplier: Player slot {playerSlot + 1} → charIndex = {charIndex}");

            if (charIndex < 0 || charIndex >= playerObjectsByCharacterIndex.Length)
            {
                Debug.LogWarning($"MinigameCharacterApplier: Invalid charIndex {charIndex} for player {playerSlot + 1}");
                continue;
            }

            GameObject playerObj = playerObjectsByCharacterIndex[charIndex];
            if (playerObj == null)
            {
                Debug.LogWarning($"MinigameCharacterApplier: No GameObject at index {charIndex}");
                continue;
            }

            playerObj.SetActive(true);
            orderedActivePlayers[playerSlot] = playerObj;

            // Apply sprite
            SpriteRenderer sr = playerObj.GetComponent<SpriteRenderer>();
            if (sr != null && charIndex < characterSprites.Length)
                sr.sprite = characterSprites[charIndex];

            // Apply animator if provided
            if (characterAnimators != null && characterAnimators.Length > charIndex && characterAnimators[charIndex] != null)
            {
                Animator anim = playerObj.GetComponent<Animator>();
                if (anim != null)
                    anim.runtimeAnimatorController = characterAnimators[charIndex];
            }

            Debug.Log($"MinigameCharacterApplier: Activated {playerObj.name} for player slot {playerSlot + 1}");
        }
    }
}
