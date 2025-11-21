using UnityEngine;

public class minigameCamera : MonoBehaviour
{
    public Transform[] marbles;   // Assign all marble objects in the Inspector
    public float smoothSpeed = 2f;
    public Transform endPoint;    // Empty GameObject at bottom of level

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (marbles.Length == 0) return;

        // Find the lowest marble (smallest Y position)
        float lowestY = marbles[0].position.y;
        for (int i = 1; i < marbles.Length; i++)
        {
            if (marbles[i].position.y < lowestY)
                lowestY = marbles[i].position.y;
        }

        // Desired camera position: follow lowest marble
        Vector3 targetPos = new Vector3(transform.position.x, lowestY, transform.position.z);

        // Clamp so camera never goes past the end point
        if (endPoint != null)
        {
            targetPos.y = Mathf.Max(targetPos.y, endPoint.position.y);
        }

        // Smoothly move camera downwards
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
    }
}

