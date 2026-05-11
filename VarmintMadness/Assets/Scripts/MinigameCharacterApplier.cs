using UnityEngine;

// Attach this to a GameObject in each minigame scene.
// Assign all 4 player GameObjects in order and all character sprites in order.
// It reads which characters were selected from PlayerPrefs and applies the correct sprites.

public class MinigameCharacterApplier : MonoBehaviour
{
    [Header("Assign all 4 player GameObjects in order (Player 1 first)")]
    public GameObject[] playerObjects;

    [Header("Assign all character sprites in the same order as the character select screen")]
    public Sprite[] characterSprites;

    [Header("Optional: Animator controllers per character (same order as sprites)")]
    public RuntimeAnimatorController[] characterAnimators;

    void Awake()
    {
        int totalPlayers = PlayerPrefs.GetInt("TotalPlayers", 4);

        for (int i = 0; i < playerObjects.Length; i++)
        {
            if (playerObjects[i] == null) continue;

            if (i >= totalPlayers)
            {
                // Disable unused players
                playerObjects[i].SetActive(false);
                continue;
            }

            // Get the character index this player selected
            int charIndex = PlayerPrefs.GetInt($"P{i + 1}_Character", 0);

            // Apply sprite
            SpriteRenderer sr = playerObjects[i].GetComponent<SpriteRenderer>();
            if (sr != null && charIndex >= 0 && charIndex < characterSprites.Length)
                sr.sprite = characterSprites[charIndex];

            // Apply animator if provided
            if (characterAnimators != null && characterAnimators.Length > 0)
            {
                Animator anim = playerObjects[i].GetComponent<Animator>();
                if (anim != null && charIndex >= 0 && charIndex < characterAnimators.Length && characterAnimators[charIndex] != null)
                    anim.runtimeAnimatorController = characterAnimators[charIndex];
            }
        }
    }
}
