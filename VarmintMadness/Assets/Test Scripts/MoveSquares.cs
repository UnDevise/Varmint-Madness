using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Transform waypointsParent;
    public Transform alternativeWaypointsParent;
    public float moveSpeed = 5.0f;

    private List<Vector2> targetPositions = new List<Vector2>();
    private int currentPositionIndex = 0;
    private Coroutine movementCoroutine;

    // Now a public property so other scripts can read its state.
    public bool IsMoving { get; private set; } = false;

    public float spriteZPosition = -5.0f;

    private void Awake()
    {
        if (waypointsParent != null)
        {
            StoreChildPositions();
        }
    }

    private void Start()
    {
        if (targetPositions.Count == 0)
        {
            return;
        }

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
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }

        IsMoving = true;
        movementCoroutine = StartCoroutine(MoveSequence(stepsToMove));
    }

    private IEnumerator MoveSequence(int steps)
    {
        // Loop through each step of the movement.
        for (int i = 0; i < Mathf.Abs(steps); i++)
        {
            int direction = steps > 0 ? 1 : -1;
            currentPositionIndex += direction;

            if (currentPositionIndex >= targetPositions.Count)
            {
                currentPositionIndex = 0;
            }
            else if (currentPositionIndex < 0)
            {
                currentPositionIndex = targetPositions.Count - 1;
            }

            Vector3 nextPosition = new Vector3(targetPositions[currentPositionIndex].x, targetPositions[currentPositionIndex].y, spriteZPosition);

            // Move smoothly to the next position.
            while (Vector2.Distance(transform.position, nextPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, nextPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = nextPosition;
        }

        IsMoving = false;
        CheckForSpecialWaypoint();
    }

    private void CheckForSpecialWaypoint()
    {
        if (IsMoving) return;

        // Get the current waypoint object from the currently active waypointsParent.
        GameObject currentWaypoint = waypointsParent.GetChild(currentPositionIndex).gameObject;

        if (currentWaypoint.CompareTag("MoveBackSquare"))
        {
            MoveCharacter(-3);
        }
        else if (currentWaypoint.CompareTag("LayerInSquare"))
        {
            SwitchWaypoints(alternativeWaypointsParent);
            // After switching, start a new move sequence to the first waypoint of the new path.
            MoveCharacter(1);
        }
    }

    // New method to switch the active waypoint parent without snapping.
    public void SwitchWaypoints(Transform newWaypointsParent)
    {
        if (waypointsParent == newWaypointsParent)
        {
            return;
        }

        waypointsParent = newWaypointsParent;
        StoreChildPositions();
        currentPositionIndex = 0; // Reset to the start of the new path.
        Debug.Log("Switched to new waypoint path.");
    }
}