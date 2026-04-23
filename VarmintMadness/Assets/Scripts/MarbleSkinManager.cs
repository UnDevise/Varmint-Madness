using UnityEngine;
using System.Collections.Generic;

// Attach this to a persistent manager GameObject in the marble minigame scene.
// It randomly assigns a unique weighted skin to each marble at the start of the game.

public class MarbleSkinManager : MonoBehaviour
{
    [System.Serializable]
    public class MarbleSkin
    {
        public string skinName;
        public Sprite sprite;
        [Range(0.1f, 100f)]
        public float weight = 1f; // Higher = more likely to be chosen
        public AnimatorOverrideController animatorOverride; // Optional: assign if this skin has an animation
    }

    [Header("Marble Skins")]
    public List<MarbleSkin> availableSkins = new List<MarbleSkin>();

    [Header("Marble References")]
    public MarbleSelector[] marbleSelectors; // Drag all marble prefab instances here

    [Header("Scale Settings")]
    public bool overrideScale = true;
    public float marbleScale = 1f; // Adjust this to match your original marble size

    void Awake()
    {
        AssignSkinsToMarbles();
    }

    private void AssignSkinsToMarbles()
    {
        if (availableSkins.Count == 0)
        {
            Debug.LogWarning("MarbleSkinManager: No skins assigned!");
            return;
        }

        // Build a pool of available skins (copy so we can remove as we pick)
        List<MarbleSkin> skinPool = new List<MarbleSkin>(availableSkins);

        foreach (var selector in marbleSelectors)
        {
            if (skinPool.Count == 0)
            {
                Debug.LogWarning("MarbleSkinManager: Ran out of unique skins for marbles!");
                break;
            }

            // Pick a weighted random skin from the remaining pool
            MarbleSkin chosen = PickWeightedRandom(skinPool);

            // Apply the sprite to the marble's SpriteRenderer
            SpriteRenderer sr = selector.GetComponent<SpriteRenderer>();
            if (sr != null && chosen.sprite != null)
            {
                sr.sprite = chosen.sprite;

                // Apply scale override if enabled
                if (overrideScale)
                {
                    selector.transform.localScale = new Vector3(marbleScale, marbleScale, selector.transform.localScale.z);

                    // Resize CircleCollider2D to match the new sprite size
                    CircleCollider2D circle = selector.GetComponent<CircleCollider2D>();
                    if (circle != null)
                    {
                        // Use the sprite's bounds to calculate the correct radius
                        float spriteRadius = chosen.sprite.bounds.extents.x;
                        circle.radius = spriteRadius;
                        circle.offset = Vector2.zero;
                    }

                    // If using a BoxCollider2D instead
                    BoxCollider2D box = selector.GetComponent<BoxCollider2D>();
                    if (box != null)
                    {
                        box.size = chosen.sprite.bounds.size;
                        box.offset = Vector2.zero;
                    }
                }
            }
            else
            {
                Debug.LogWarning("MarbleSkinManager: Missing SpriteRenderer or sprite on marble " + selector.name);
            }

                // Handle animation - add/enable Animator if this skin has one
                Animator animator = selector.GetComponent<Animator>();

                if (chosen.animatorOverride != null)
                {
                    // Add an Animator component at runtime if one doesn't exist
                    if (animator == null)
                        animator = selector.gameObject.AddComponent<Animator>();

                    animator.runtimeAnimatorController = chosen.animatorOverride;
                    animator.enabled = true;
                }
                else
                {
                    // No animation for this skin - disable Animator if one exists
                    if (animator != null)
                        animator.enabled = false;
                }

            // Remove from pool so it can't be picked again
            skinPool.Remove(chosen);
        }
    }

    private MarbleSkin PickWeightedRandom(List<MarbleSkin> pool)
    {
        // Calculate total weight of remaining skins
        float totalWeight = 0f;
        foreach (var skin in pool)
            totalWeight += skin.weight;

        // Roll a random value
        float roll = Random.Range(0f, totalWeight);

        // Walk through the pool and find which skin the roll lands on
        float cumulative = 0f;
        foreach (var skin in pool)
        {
            cumulative += skin.weight;
            if (roll <= cumulative)
                return skin;
        }

        // Fallback (shouldn't happen)
        return pool[pool.Count - 1];
    }
}
