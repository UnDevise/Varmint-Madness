using System.Collections.Generic;
using UnityEngine;

public class ClickToCyclePosition : MonoBehaviour
{
    public Transform waypointsParent;
    private List<Vector2> targetPositions = new List<Vector2>();
    private int currentPositionIndex = 0;

    // A small offset for the Z-position to ensure the sprite is always visible.
    // Ensure this value is greater than the Z-position of your waypoints.
    private const float spriteZOffset = -0.1f;

    private void Awake()
    {
        if (waypointsParent == null)
        {
            Debug.LogError("The 'Waypoints Parent' object is not assigned. Please assign it in the Inspector.", this);
            return;
        }
        StoreChildPositions();
    }

    private void Start()
    {
        if (targetPositions.Count == 0)
        {
            Debug.LogWarning("No child objects found on the 'Waypoints Parent' object to use as target positions.", this);
            return;
        }

        // Set the initial position, including the Z-offset, so the sprite starts visible.
        Vector3 initialPosition = targetPositions[0];
        initialPosition.z = spriteZOffset;
        transform.position = initialPosition;
    }

    private void StoreChildPositions()
    {
        targetPositions.Clear();
        foreach (Transform child in waypointsParent)
        {
            // Only store the x and y coordinates. The z-coordinate is handled separately.
            targetPositions.Add(child.position);
        }
    }

    private void OnMouseDown()
    {
        if (targetPositions.Count == 0) return;

        currentPositionIndex++;
        if (currentPositionIndex >= targetPositions.Count)
        {
            currentPositionIndex = 0;
        }

        // Get the new 2D position.
        Vector2 nextPosition2D = targetPositions[currentPositionIndex];

        // Create a new Vector3 with the original waypoint's x and y, and a modified z.
        // We use the sprite's current Z-position, which was set to spriteZOffset.
        Vector3 newPosition = new Vector3(nextPosition2D.x, nextPosition2D.y, transform.position.z);

        // Move the sprite to the new position.
        transform.position = newPosition;

        Debug.Log("Sprite clicked! Moving to new child position at index " + currentPositionIndex);
    }
}