using UnityEngine;

public class SequencePad : MonoBehaviour
{
    public int padID;
    public SpriteRenderer sprite;

    public AudioSource audioSource;
    public AudioClip glowSound;

    private Color baseColor;
    private Color glowColor;

    private SecretSequenceManager manager;

    // NEW FLAGS
    [HideInInspector] public bool canBePressed = false;      // Only true during player's input phase
    [HideInInspector] public bool isShowingPattern = false;  // True while pads are glowing automatically

    // OPTIONAL: restrict to a specific player
    [HideInInspector] public int allowedPlayerID = -1;       // -1 = any player

    void Start()
    {
        manager = Object.FindFirstObjectByType<SecretSequenceManager>();

        baseColor = sprite.color;
        glowColor = baseColor * 2.5f;
    }

    public void LightUp()
    {
        sprite.color = glowColor;

        if (audioSource != null && glowSound != null)
            audioSource.PlayOneShot(glowSound);
    }

    public void Dim()
    {
        sprite.color = baseColor;
    }

    void OnMouseDown()
    {
        // BLOCK INPUT IF:
        // 1. Pads are showing the pattern
        // 2. Pads are not allowed to be pressed
        // 3. This pad is not for the current player (optional)
        if (isShowingPattern)
            return;

        if (!canBePressed)
            return;

        if (allowedPlayerID != -1 && manager.currentPlayerID != allowedPlayerID)
            return;

        // VALID CLICK
        manager.OnPadClicked(padID);
        LightUp();
        Invoke(nameof(Dim), 0.2f);
    }
}