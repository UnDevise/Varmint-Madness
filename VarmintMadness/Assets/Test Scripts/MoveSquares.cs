using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Add this line to access TextMeshPro functions

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

    // --- New variables for the Garbage counter ---
    public TextMeshProUGUI garbageText; // Reference to the TextMeshPro UI element
    private int garbageCount = 0;

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
        UpdateGarbageText(); // Initialize the text display.
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

        GameObject currentWaypoint = waypointsParent.GetChild(currentPositionIndex).gameObject;

        if (currentWaypoint.CompareTag("MoveBackSquare"))
        {
            MoveCharacter(-3);
        }
        else if (currentWaypoint.CompareTag("LayerInSquare"))
        {
            SwitchWaypoints(alternativeWaypointsParent);
            MoveCharacter(1);
        }
        // --- New check for the AddGarbageSquare tag ---
        else if (currentWaypoint.CompareTag("AddGarbageSquare"))
        {
            IncrementGarbageCount();
        }
    }

    public void SwitchWaypoints(Transform newWaypointsParent)
    {
        if (waypointsParent == newWaypointsParent)
        {
            return;
        }

        waypointsParent = newWaypointsParent;
        StoreChildPositions();
        currentPositionIndex = 0;
        Debug.Log("Switched to new waypoint path.");
    }

    // --- New method to increment garbage count and update the UI ---
    private void IncrementGarbageCount()
    {
        garbageCount++;
        UpdateGarbageText();
        Debug.Log("Garbage count increased. New count: " + garbageCount);
    }

    // --- New method to update the UI Text field ---
    private void UpdateGarbageText()
    {
        if (garbageText != null)
        {
            garbageText.text = "Garbage: " + garbageCount;
        }
    }
}