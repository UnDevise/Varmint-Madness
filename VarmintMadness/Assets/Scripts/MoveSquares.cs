using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Transform waypointsParent;
    public float moveSpeed = 5.0f;

    private List<Vector2> targetPositions = new List<Vector2>();
    private int currentPositionIndex = 0;
    private Coroutine movementCoroutine;

    // A small offset for the Z-position to ensure the sprite is visible.
    public float spriteZPosition = -5.0f;

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
            Debug.LogWarning("No child objects found on the 'Waypoints Parent'.", this);
            return;
        }

        // Set the initial position, including the Z-position.
        Vector3 initialPosition = targetPositions[currentPositionIndex];
        initialPosition.z = spriteZPosition;
        transform.position = initialPosition;
    }

    private void StoreChildPositions()
    {
        targetPositions.Clear();
        foreach (Transform child in waypointsParent)
        {
            targetPositions.Add(child.position);
        }
    }

    public void MoveCharacter(int stepsToMove)
    {
        Debug.Log("MoveCharacter called with " + stepsToMove + " steps.");
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }
        movementCoroutine = StartCoroutine(MoveSequence(stepsToMove));
    }

    private IEnumerator MoveSequence(int steps)
    {
        Debug.Log("Starting move sequence for " + steps + " steps.");
        for (int i = 0; i < steps; i++)
        {
            currentPositionIndex++;

            if (currentPositionIndex >= targetPositions.Count)
            {
                currentPositionIndex = 0;
            }

            // Create a Vector3 for the target position, preserving the Z-index.
            Vector3 nextPosition = new Vector3(targetPositions[currentPositionIndex].x, targetPositions[currentPositionIndex].y, spriteZPosition);

            while (Vector2.Distance(transform.position, nextPosition) > 0.01f)
            {
                // Use Vector3.MoveTowards to correctly move the character.
                transform.position = Vector3.MoveTowards(transform.position, nextPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = nextPosition;
            Debug.Log("Moved to next waypoint: " + transform.position);
        }
        movementCoroutine = null;
        Debug.Log("Move sequence finished.");
    }
}