using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    // The original and alternative waypoint paths.
    public Transform waypointsParent;
    public Transform alternativeWaypointsParent;

    // Adjustable teleport points for layer switching.
    // Drag a GameObject from the scene into these fields in the Inspector.
    public Transform layerInTeleportPoint;
    public Transform layerOutTeleportPoint;

    public float moveSpeed = 5.0f;

    private List<Vector2> targetPositions = new List<Vector2>();
    private int currentPositionIndex = 0;
    private Coroutine movementCoroutine;
    private Transform originalWaypointsParent; // Store the original path

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
        // Store the initial waypoint parent.
        originalWaypointsParent = waypointsParent;

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
            SwitchWaypoints(alternativeWaypointsParent, layerInTeleportPoint);
            bonusMoveTriggered = true;
        }
        else if (currentWaypoint.CompareTag("LayerOutSquare"))
        {
            SwitchWaypoints(originalWaypointsParent, layerOutTeleportPoint);
            bonusMoveTriggered = true;
        }
        else if (currentWaypoint.CompareTag("AddGarbageSquare"))
        {
            IncrementGarbageCount();
        }
        else if (currentWaypoint.CompareTag("RemoveGarbageSquare"))
        {
            DecrementGarbageCount();
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

    public void SwitchWaypoints(Transform newWaypointsParent, Transform teleportTarget)
    {
        if (waypointsParent == newWaypointsParent)
        {
            return;
        }

        waypointsParent = newWaypointsParent;
        StoreChildPositions();

        // Find the index of the closest waypoint to the teleport target on the new path.
        int closestIndex = FindClosestWaypointIndex(teleportTarget);
        currentPositionIndex = closestIndex;

        // Start a separate coroutine to move to the teleport target.
        StartCoroutine(MoveToTeleportPoint(teleportTarget));

        Debug.Log("Switched to new waypoint path.");
    }

    private IEnumerator MoveToTeleportPoint(Transform target)
    {
        IsMoving = true;
        Vector3 nextPosition = new Vector3(target.position.x, target.position.y, spriteZPosition);

        while (Vector2.Distance(transform.position, nextPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, nextPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = nextPosition;
        IsMoving = false;

        // Once the smooth move is finished, end the turn.
        if (diceController != null)
        {
            diceController.OnPlayerTurnFinished();
        }
    }

    private int FindClosestWaypointIndex(Transform target)
    {
        if (target == null || targetPositions.Count == 0)
        {
            return 0;
        }

        int closestIndex = 0;
        float minDistance = float.MaxValue;

        for (int i = 0; i < targetPositions.Count; i++)
        {
            float distance = Vector2.Distance(target.position, targetPositions[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestIndex = i;
            }
        }
        return closestIndex;
    }

    private void IncrementGarbageCount()
    {
        garbageCount++;
        UpdateGarbageText();
        Debug.Log($"Garbage count increased for {playerName}. New count: {garbageCount}");
    }

    private void DecrementGarbageCount()
    {
        if (garbageCount > 0)
        {
            garbageCount--;
            UpdateGarbageText();
            Debug.Log($"Garbage count decreased for {playerName}. New count: {garbageCount}");
        }
        else
        {
            Debug.Log($"Garbage count is already at 0 for {playerName}.");
        }
    }

    private void UpdateGarbageText()
    {
        if (garbageText != null)
        {
            garbageText.text = $"{playerName}: {garbageCount} Garbage";
        }
    }
}