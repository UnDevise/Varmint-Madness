using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    public Transform waypointsParent;
    public Transform alternativeWaypointsParent;
    public float moveSpeed = 5.0f;

    private List<Vector2> targetPositions = new List<Vector2>();
    private int currentPositionIndex = 0;
    private Coroutine movementCoroutine;

    public bool IsMoving { get; private set; } = false;
    public bool IsStunned { get; set; } = false;

    public float spriteZPosition = -5.0f;

    // Will be assigned by DiceController
    [HideInInspector] public TextMeshProUGUI garbageText;
    [HideInInspector] public string playerName;
    private int garbageCount = 0;

    private DiceController diceController;

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
        UpdateGarbageText();
    }

    public void SetDiceController(DiceController controller)
    {
        diceController = controller;
    }

    private void StoreChildPositions()
    {
        targetPositions.Clear();
        if (waypointsParent != null)
        {
            foreach (Transform child in waypointsParent)
            {
                targetPositions.Add(child.position);
            }
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
        bool bonusMoveTriggered = false;

        if (currentWaypoint.CompareTag("MoveBackSquare"))
        {
            MoveCharacter(-3);
            bonusMoveTriggered = true;
        }
        else if (currentWaypoint.CompareTag("LayerInSquare"))
        {
            SwitchWaypoints(alternativeWaypointsParent);
            MoveCharacter(1);
            bonusMoveTriggered = true;
        }
        // Check for the correct "AddGarbageSquare" tag.
        else if (currentWaypoint.CompareTag("AddGarbageSquare"))
        {
            IncrementGarbageCount();
        }
        else if (currentWaypoint.CompareTag("StunPlayerSquare"))
        {
            IsStunned = true;
            Debug.Log($"{playerName} landed on a stun square and will skip their next turn.");
        }

        if (!bonusMoveTriggered && diceController != null)
        {
            diceController.OnPlayerTurnFinished();
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

    private void IncrementGarbageCount()
    {
        garbageCount++;
        UpdateGarbageText();
        Debug.Log($"Garbage count increased for {playerName}. New count: {garbageCount}");
    }

    private void UpdateGarbageText()
    {
        if (garbageText != null)
        {
            // Update the text to display "Garbage" instead of "Items".
            garbageText.text = $"{playerName}: {garbageCount} Garbage";
        }
    }
}
