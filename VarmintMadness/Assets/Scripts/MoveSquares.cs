using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class PlayerMovement : MonoBehaviour
{
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
        // 1. Tell the camera to start following this player with a zoom transition.
        if (cameraController != null)
        {
            yield return StartCoroutine(cameraController.StartFollowingCoroutine(transform));
        }

        // 2. Perform the player's movement.
        IsMoving = true;
        yield return StartCoroutine(MoveSequence(stepsToMove));
        IsMoving = false;

        // 3. Check for the special waypoint.
        bool bonusMoveTriggered = CheckForSpecialWaypoint();

        // 4. End the turn if no bonus move was triggered.
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
    }

    private bool CheckForSpecialWaypoint()
    {
        if (IsMoving) return false;

        GameObject currentWaypoint = waypointsParent.GetChild(currentPositionIndex).gameObject;

        if (currentWaypoint.CompareTag("MoveBackSquare"))
        {
            MoveCharacter(-3);
            return true;
        }
        else if (currentWaypoint.CompareTag("LayerInSquare"))
        {
            SwitchWaypoints(alternativeWaypointsParent, layerInTeleportPoint);
            return true;
        }
        else if (currentWaypoint.CompareTag("LayerOutSquare"))
        {
            SwitchWaypoints(originalWaypointsParent, layerOutTeleportPoint);
            return true;
        }
        else if (currentWaypoint.CompareTag("Tunnel"))
        {
            TeleportToTunnel(currentWaypoint);
            return true;
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

        return false; // Return false if no bonus move was triggered.
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

        StartCoroutine(MoveToTeleportPoint(teleportTarget));

        Debug.Log("Switched to new waypoint path.");
    }

    private IEnumerator MoveToTeleportPoint(Transform target)
    {
        IsMoving = true;
        Vector3 nextPosition = new Vector3(target.position.x, target.position.y, spriteZPosition);

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

        if (cameraController != null)
        {
            cameraController.StopFollowing();
        }
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

    private void TeleportToTunnel(GameObject currentTunnel)
    {
        StartCoroutine(HandleTunnelTeleport(currentTunnel));
    }

    private IEnumerator HandleTunnelTeleport(GameObject currentTunnel)
    {
        if (cameraController != null)
        {
            yield return StartCoroutine(cameraController.StartFollowingCoroutine(transform));
        }

        yield return StartCoroutine(Fade(0, 0.5f));

        string destinationName;
        if (currentTunnel.name == "Tunnel1 (15)")
        {
            destinationName = "Tunnel2 (32)";
        }
        else if (currentTunnel.name == "Tunnel2 (32)")
        {
            destinationName = "Tunnel1 (15)";
        }
        else
        {
            Debug.LogError($"Tunnel name not recognized: {currentTunnel.name}");
            if (diceController != null)
            {
                diceController.OnPlayerTurnFinished();
            }
            if (cameraController != null)
            {
                cameraController.StopFollowing();
            }
            yield break;
        }

        GameObject destinationTunnel = GameObject.Find(destinationName);

        if (destinationTunnel != null)
        {
            int newIndex = -1;
            for (int i = 0; i < waypointsParent.childCount; i++)
            {
                if (waypointsParent.GetChild(i).gameObject == destinationTunnel)
                {
                    newIndex = i;
                    break;
                }
            }

            if (newIndex != -1)
            {
                currentPositionIndex = newIndex;
            }
            transform.position = new Vector3(destinationTunnel.transform.position.x, destinationTunnel.transform.position.y, spriteZPosition);
            Debug.Log($"{playerName} teleported from {currentTunnel.name} to {destinationName}.");
        }
        else
        {
            Debug.LogError($"Could not find destination tunnel: {destinationName}");
        }

        yield return StartCoroutine(Fade(1, 0.5f));

        if (cameraController != null)
        {
            cameraController.StopFollowing();
        }
        if (diceController != null)
        {
            diceController.OnPlayerTurnFinished();
        }
    }

    private IEnumerator Fade(float targetAlpha, float duration)
    {
        if (spriteRenderer == null)
        {
            yield break;
        }

        Color startColor = spriteRenderer.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            spriteRenderer.color = Color.Lerp(startColor, endColor, progress);
            yield return null;
        }

        spriteRenderer.color = endColor;
    }

    private void UpdateGarbageText()
    {
        if (garbageText != null)
        {
            garbageText.text = $"{playerName}: {garbageCount} garbage";
        }
    }

    private void IncrementGarbageCount()
    {
        garbageCount++;
        UpdateGarbageText();
        Debug.Log($"{playerName} collected garbage. Total: {garbageCount}");
    }

    private void DecrementGarbageCount()
    {
        if (garbageCount > 0)
        {
            garbageCount--;
            UpdateGarbageText();
            Debug.Log($"{playerName} removed garbage. Total: {garbageCount}");
        }
    }
}