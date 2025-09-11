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

    // Now a public property so other scripts can read its state.
    public bool IsMoving { get; private set; } = false;

    public float spriteZPosition = -5.0f;

    private void Awake()
    {
        if (waypointsParent == null)
        {
            return;
        }
        StoreChildPositions();
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
        // This check is redundant now but kept for clarity. The IsMoving flag is set to false just above.
        if (IsMoving) return;

        GameObject currentWaypoint = waypointsParent.GetChild(currentPositionIndex).gameObject;

        if (currentWaypoint.CompareTag("MoveBackSquare"))
        {
            MoveCharacter(-3);
        }
    }
}