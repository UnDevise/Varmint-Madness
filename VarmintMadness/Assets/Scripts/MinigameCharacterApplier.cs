using UnityEngine;

// Attach this to a GameObject in each minigame scene.
// Maps selected characters to their correct player GameObjects by character index.

public class MinigameCharacterApplier : MonoBehaviour
{
    [Header("Assign all 4 player GameObjects matching their CHARACTER index order")]
    [Tooltip("Slot 0 = character index 0, Slot 1 = character index 1, etc. Match the character select screen order.")]
    public GameObject[] playerObjectsByCharacterIndex;

    [Header("Assign all character sprites in character select screen order")]
    public Sprite[] characterSprites;

    [Header("Optional: Animator controllers in character select screen order")]
    public RuntimeAnimatorController[] characterAnimators;

    void Awake()
    {
        int totalPlayers = PlayerPrefs.GetInt("TotalPlayers", 4);

        // First disable ALL player objects
        foreach (var obj in playerObjectsByCharacterIndex)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        // Now enable and apply sprites only for selected characters
        for (int playerSlot = 0; playerSlot < totalPlayers; playerSlot++)
        {
            int charIndex = PlayerPrefs.GetInt($"P{playerSlot + 1}_Character", -1);
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
            Debug.Log($"MinigameCharacterApplier: Activated {playerObj.name} for player slot {playerSlot + 1}");

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
        }
    }
}
