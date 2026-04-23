using UnityEngine;

public class SpriteSpinner : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Degrees per second")]
    public float rotationSpeed = 100f;

    [Tooltip("Toggle clockwise vs counter-clockwise")]
    public bool clockwise = true;

    void Update()
    {
        // Clockwise in Unity is a negative Z rotation
        float direction = clockwise ? -1f : 1f;

        // Rotate around the Z-axis (standard for 2D)
        transform.Rotate(0, 0, rotationSpeed * direction * Time.deltaTime);
    }
}
