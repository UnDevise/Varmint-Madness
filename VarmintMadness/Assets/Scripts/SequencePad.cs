using UnityEngine;

public class SequencePad : MonoBehaviour
{
    public int padID;
    public SpriteRenderer sprite;

    public AudioSource audioSource;   // NEW: sound player
    public AudioClip glowSound;       // NEW: sound clip

    private Color baseColor;
    private Color glowColor;

    private SecretSequenceManager manager;

    void Start()
    {
        manager = Object.FindFirstObjectByType<SecretSequenceManager>();

        baseColor = sprite.color;
        glowColor = baseColor * 2.5f;
    }

    public void LightUp()
    {
        sprite.color = glowColor;

        // NEW: play sound when glowing
        if (audioSource != null && glowSound != null)
            audioSource.PlayOneShot(glowSound);
    }

    public void Dim()
    {
        sprite.color = baseColor;
    }

    void OnMouseDown()
    {
        manager.OnPadClicked(padID);
        LightUp();
        Invoke(nameof(Dim), 0.2f);
    }
}