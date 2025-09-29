using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    public Transform waypointsParent;
    public Transform alternativeWaypointsParent;
    public Transform layerInTeleportPoint;
    public Transform layerOutTeleportPoint;
    public float moveSpeed = 5.0f;

    private List<WaypointData> targetWaypoints = new List<WaypointData>();

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
            StoreWaypointData();
        }

        if (CameraController.Instance != null)
        {
            cameraController = CameraController.Instance;
        }
    }

    private void Start()
    {
        if (targetWaypoints.Count == 0)
        {
            return;
        }

        Vector3 initialPosition = targetWaypoints[currentPositionIndex].Position;
        initialPosition.z = spriteZPosition;
        transform.position = initialPosition;
        UpdateGarbageText();
    }

    public void SetDiceController(DiceController controller)
    {
        diceController = controller;
    }

    private void StoreWaypointData()
    {
        targetWaypoints.Clear();
        if (waypointsParent != null)
        {
            foreach (Transform child in waypointsParent)
            {
                targetWaypoints.Add(new WaypointData(child.position, child.tag, child.name));
            }
        }
    }

    public void MoveCharacter(int stepsToMove)
    {
        StartCoroutine(HandlePlayerTurn(stepsToMove));
    }

    private IEnumerator HandlePlayerTurn(int stepsToMove)
    {
        if (cameraController != null)
        {
            yield return StartCoroutine(cameraController.StartFollowingCoroutine(transform));
        }

        IsMoving = true;
        yield return StartCoroutine(MoveSequence(stepsToMove));
        IsMoving = false;

        bool bonusMoveTriggered = CheckForSpecialWaypoint();

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

            if (currentPositionIndex >= targetWaypoints.Count)
            {
                currentPositionIndex = 0;
            }
            else if (currentPositionIndex < 0)
            {
                currentPositionIndex = targetWaypoints.Count - 1;
            }

            Vector3 nextPosition = new Vector3(targetWaypoints[currentPositionIndex].Position.x, targetWaypoints[currentPositionIndex].Position.y, spriteZPosition);

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

        string currentWaypointTag = targetWaypoints[currentPositionIndex].Tag;
        string currentWaypointName = targetWaypoints[currentPositionIndex].Name;

        if (currentWaypointTag == "MoveBackSquare")
        {
            MoveCharacter(-3);
            return true;
        }
        else if (currentWaypointTag == "LayerInSquare")
        {
            SwitchWaypoints(alternativeWaypointsParent, layerInTeleportPoint);
            return true;
        }
        else if (currentWaypointTag == "LayerOutSquare")
        {
            SwitchWaypoints(originalWaypointsParent, layerOutTeleportPoint);
            return true;
        }
        else if (currentWaypointTag == "Tunnel")
        {
            TeleportToTunnel(currentWaypointName);
            return true;
        }
        else if (currentWaypointTag == "AddGarbageSquare")
        {
            IncrementGarbageCount();
        }
        else if (currentWaypointTag == "RemoveGarbageSquare")
        {
            DecrementGarbageCount();
        }
        else if (currentWaypointTag == "StunPlayerSquare")
        {
            IsStunned = true;
            Debug.Log($"{playerName} landed on a stun square and will skip their next turn.");
        }

        return false;
    }

    public void SwitchWaypoints(Transform newWaypointsParent, Transform teleportTarget)
    {
        if (waypointsParent == newWaypointsParent)
        {
            return;
        }

        waypointsParent = newWaypointsParent;
        StoreWaypointData();

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
        if (target == null || targetWaypoints.Count == 0)
        {
            return 0;
        }

        int closestIndex = 0;
        float minDistance = float.MaxValue;

        for (int i = 0; i < targetWaypoints.Count; i++)
        {
            float distance = Vector2.Distance(target.position, targetWaypoints[i].Position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestIndex = i;
            }
        }
        return closestIndex;
    }

    private void TeleportToTunnel(string currentTunnelName)
    {
        StartCoroutine(HandleTunnelTeleport(currentTunnelName));
    }

    private IEnumerator HandleTunnelTeleport(string currentTunnelName)
    {
        if (cameraController != null)
        {
            yield return StartCoroutine(cameraController.StartFollowingCoroutine(transform));
        }

        yield return StartCoroutine(Fade(0, 0.5f));

        string destinationName;
        if (currentTunnelName == "Tunnel1 (15)")
        {
            destinationName = "Tunnel2 (32)";
        }
        else if (currentTunnelName == "Tunnel2 (32)")
        {
            destinationName = "Tunnel1 (15)";
        }
        else
        {
            Debug.LogError($"Tunnel name not recognized: {currentTunnelName}");
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

        int newIndex = -1;
        for (int i = 0; i < targetWaypoints.Count; i++)
        {
            if (targetWaypoints[i].Name == destinationName)
            {
                newIndex = i;
                break;
            }
        }

        if (newIndex != -1)
        {
            currentPositionIndex = newIndex;
            Vector3 destinationPosition = targetWaypoints[newIndex].Position;
            transform.position = new Vector3(destinationPosition.x, destinationPosition.y, spriteZPosition);
            Debug.Log($"{playerName} teleported from {currentTunnelName} to {destinationName}.");
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
        Color endColor = new Color(startColor.r, startColor.g, startColor.g, targetAlpha);
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
