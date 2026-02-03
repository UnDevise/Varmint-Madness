using UnityEngine;

public class SpriteSwapper : MonoBehaviour
{
    [System.Serializable]
    public struct SpriteSwapPair
    {
        public SpriteRenderer targetRenderer;
        public Sprite alternateSprite;
    }

    [Header("Sprite Swap Settings")]
    public SpriteSwapPair[] swapList;

    [Header("Color Change Settings (Turns Black)")]
    public SpriteRenderer[] blackTintList;

    public void OnToggleChanged(bool isOn)
    {
        // 1. Handle Sprite Swapping
        foreach (var pair in swapList)
        {
            if (pair.targetRenderer == null) continue;
            Animator anim = pair.targetRenderer.GetComponent<Animator>();

            if (isOn)
            {
                if (anim != null) anim.enabled = false;
                pair.targetRenderer.sprite = pair.alternateSprite;
            }
            else
            {
                if (anim != null) { anim.enabled = true; anim.Rebind(); }
            }
        }

        // 2. Handle Color Changing to Black
        foreach (var sr in blackTintList)
        {
            if (sr == null) continue;
            // Set color to pure black (0,0,0) or back to white (1,1,1)
            sr.color = isOn ? Color.black : Color.white;
        }
    }
}
