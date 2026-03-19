using UnityEngine;

public class SequencePad : MonoBehaviour
{
    public int padID;
    public SpriteRenderer sprite;

    private Color baseColor;
    private Color glowColor;

    private SecretSequenceManager manager;

    void Start()
    {
        manager = FindObjectOfType<SecretSequenceManager>();

        baseColor = sprite.color;
        glowColor = baseColor * 2.5f; // medium glow
    }

    public void LightUp()
    {
        sprite.color = glowColor;
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