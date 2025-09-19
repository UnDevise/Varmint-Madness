using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static UnityEngine.UIElements.UxmlAttributeDescription;

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
    private SpriteRenderer spriteRenderer;

    // Will be assigned by DiceController
    [HideInInspector] public TextMeshProUGUI garbageText;
    [HideInInspector] public string playerName;
    private int garbageCount = 0;

    private DiceController diceController;

    private void Awake()
    {
        // Store the initial waypoint parent.
        originalWaypointsParent = waypointsParent;
        spriteRenderer = GetComponent<SpriteRenderer>();

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

        int closestIndex = FindClosestWaypointIndex(teleportTarget);
        currentPositionIndex = closestIndex;

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
        // Fade out
        yield return StartCoroutine(Fade(0, 0.5f)); // Fade out over 0.5 seconds

        // Normal teleport logic (teleport player while invisible)
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

        // Fade in
        yield return StartCoroutine(Fade(1, 0.5f)); // Fade in over 0.5 seconds

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