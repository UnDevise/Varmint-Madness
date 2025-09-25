using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class PlayerMovement : MonoBehaviour
{
    // ... (All other variables are the same)
    public Transform waypointsParent;
    public Transform alternativeWaypointsParent;
    public Transform layerInTeleportPoint;
    public Transform layerOutTeleportPoint;
    public float moveSpeed = 5.0f;
    private List<Vector2> targetPositions = new List<Vector2>();
    private int currentPositionIndex = 0;
    private Coroutine movementCoroutine;
    private Transform originalWaypointsParent;
    public bool IsMoving { get; private set; } = false;
    public bool IsStunned { get; set; } = false;
    public float spriteZPosition = -5.0f;
    private SpriteRenderer spriteRenderer;
    [HideInInspector] public TextMeshProUGUI garbageText;
    [HideInInspector] public string playerName;
    private int garbageCount = 0;
    private DiceController diceController;
    private CameraController cameraController;

    private void Awake()
    {
        originalWaypointsParent = waypointsParent;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (waypointsParent != null)
        {
            StoreChildPositions();
        }

        if (CameraController.Instance != null)
        {
            cameraController = CameraController.Instance;
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

    // Public method to start the player's turn, including camera zoom.
    public void MoveCharacter(int stepsToMove)
    {
        StartCoroutine(HandlePlayerTurn(stepsToMove));
    }

    private IEnumerator HandlePlayerTurn(int stepsToMove)
    {
        // Tell the camera to start following this player with a zoom transition.
        if (cameraController != null)
        {
            yield return StartCoroutine(cameraController.StartFollowingCoroutine(transform));
        }

        // After the zoom is complete, start the player's movement.
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

        // ... (Same logic for special waypoints)
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
        else if (currentWaypoint.CompareTag("Tunnel"))
        {
            TeleportToTunnel(currentWaypoint);
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

        if (!bonusMoveTriggered)
        {
            if (cameraController != null)
            {
                cameraController.StopFollowing();
            }

            if (diceController != null)
            {
                diceController.OnPlayerTurnFinished();
            }
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

        int closestIndex = FindClosestWaypointIndex(teleportTarget);
        currentPositionIndex = closestIndex;

        // No need to follow the camera during this short transition, the camera returns to full view afterward.
        StartCoroutine(MoveToTeleportPoint(teleportTarget));

        Debug.Log("Switched to new waypoint path.");
    }

    private IEnumerator MoveToTeleportPoint(Transform target)
    {
        IsMoving = true;
        Vector3 nextPosition = new Vector3(target.position.x, target.position.y, spriteZPosition);

        // Tell the camera to start following the player during the teleport.
        if (cameraController != null)
        {
            yield return StartCoroutine(cameraController.StartFollowingCoroutine(transform));
        }

        while (Vector2.Distance(transform.position, nextPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, nextPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = nextPosition;
        IsMoving = false;

        // Stop camera following after teleport is complete.
        if (cameraController != null)
        {
            cameraController.StopFollowing();
        }

        if (diceController != null)
        {
            diceController.OnPlayerTurnFinished();
        }
    }

    // ... (Other helper methods are the same)
    private int FindClosestWaypointIndex(Transform target) { /* ... */ return 0; }
    private void TeleportToTunnel(GameObject currentTunnel) { /* ... */ }
    private IEnumerator HandleTunnelTeleport(GameObject currentTunnel) { /* ... */ yield break; }
    private IEnumerator Fade(float targetAlpha, float duration) { /* ... */ yield break; }
    private void UpdateGarbageText() { /* ... */ }
    private void IncrementGarbageCount() { /* ... */ }
    private void DecrementGarbageCount() { /* ... */ }
}
