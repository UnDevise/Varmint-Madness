using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform waypointsParent;
    public Transform alternativeWaypointsParent;
    public Transform layerInTeleportPoint;
    public Transform layerOutTeleportPoint;
    public float moveSpeed = 5.0f;

    public List<string> marbleMinigameScenes = new List<string>();

    [Header("Special Square Sounds")]
    public AudioClip moveBackSound;
    public AudioClip MinigameSound;
    public AudioClip switchLayerSound;
    public AudioClip tunnelSound;
    public AudioClip garbageAddSound;
    public AudioClip garbageRemoveSound;
    public AudioClip stunSound;

    private List<WaypointData> targetWaypoints = new List<WaypointData>();
    private int currentPositionIndex = 0;
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
    private Animator playerAnimator;
    private AudioSource audioSource;

    private void Awake()
    {
        originalWaypointsParent = waypointsParent;
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerAnimator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (waypointsParent != null) StoreWaypointData();
        if (CameraController.Instance != null) cameraController = CameraController.Instance;
    }

    private void Start()
    {
        if (targetWaypoints.Count == 0) return;
        Vector3 initialPosition = targetWaypoints[currentPositionIndex].Position;
        initialPosition.z = spriteZPosition;
        transform.position = initialPosition;
        UpdateGarbageText();
    }

    // --- Helper to play sound once ---
    private void PlaySquareSound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void SetDiceController(DiceController controller) => diceController = controller;

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

    public void MoveCharacter(int stepsToMove) => StartCoroutine(HandlePlayerTurn(stepsToMove));

    private IEnumerator HandlePlayerTurn(int stepsToMove)
    {
        if (cameraController != null) yield return StartCoroutine(cameraController.StartFollowingCoroutine(transform));

        IsMoving = true;
        SetRunningAnimation(true);
        yield return StartCoroutine(MoveSequence(stepsToMove));
        IsMoving = false;
        SetRunningAnimation(false);

        bool bonusMoveTriggered = CheckForSpecialWaypoint();

        if (!bonusMoveTriggered)
        {
            if (cameraController != null) cameraController.StopFollowing();
            if (diceController != null) diceController.OnPlayerTurnFinished();
        }
    }

    private IEnumerator MoveSequence(int steps)
    {
        for (int i = 0; i < Mathf.Abs(steps); i++)
        {
            int direction = steps > 0 ? 1 : -1;
            currentPositionIndex += direction;

            if (currentPositionIndex >= targetWaypoints.Count) currentPositionIndex = 0;
            else if (currentPositionIndex < 0) currentPositionIndex = targetWaypoints.Count - 1;

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
            PlaySquareSound(moveBackSound);
            MoveCharacter(-3);
            return true;
        }
        else if (currentWaypointTag == "Gambling Space")
        {
            PlaySquareSound(MinigameSound);
            StartMarbleMinigame();
            return true;
        }
        else if (currentWaypointTag == "LayerOutSquare")
        {
            PlaySquareSound(switchLayerSound);
            SwitchWaypoints(originalWaypointsParent, layerOutTeleportPoint);
            return true;
        }
        else if (currentWaypointTag == "Sewer Space")
        {
            PlaySquareSound(tunnelSound);
            TeleportToTunnel(currentWaypointName);
            return true;
        }
        else if (currentWaypointTag == "Trash Space")
        {
            PlaySquareSound(garbageAddSound);
            IncrementGarbageCount();
        }
        else if (currentWaypointTag == "Lose Trash Space")
        {
            PlaySquareSound(garbageRemoveSound);
            DecrementGarbageCount();
        }
        else if (currentWaypointTag == "Skip Turn Space")
        {
            PlaySquareSound(stunSound);
            IsStunned = true;
            Debug.Log($"{playerName} stunned.");
        }

        return false;
    }

    public void SwitchWaypoints(Transform newWaypointsParent, Transform teleportTarget)
    {
        if (waypointsParent == newWaypointsParent) return;
        waypointsParent = newWaypointsParent;
        StoreWaypointData();
        currentPositionIndex = FindClosestWaypointIndex(teleportTarget);
        StartCoroutine(MoveToTeleportPoint(teleportTarget));
    }

    private IEnumerator MoveToTeleportPoint(Transform target)
    {
        IsMoving = true;
        SetRunningAnimation(true);
        Vector3 nextPosition = new Vector3(target.position.x, target.position.y, spriteZPosition);
        if (cameraController != null) yield return StartCoroutine(cameraController.StartFollowingCoroutine(transform));

        while (Vector2.Distance(transform.position, nextPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, nextPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = nextPosition;
        IsMoving = false;
        SetRunningAnimation(false);
        if (cameraController != null) cameraController.StopFollowing();
        if (diceController != null) diceController.OnPlayerTurnFinished();
    }

    private int FindClosestWaypointIndex(Transform target)
    {
        if (target == null || targetWaypoints.Count == 0) return 0;
        int closestIndex = 0;
        float minDistance = float.MaxValue;
        for (int i = 0; i < targetWaypoints.Count; i++)
        {
            float distance = Vector2.Distance(target.position, targetWaypoints[i].Position);
            if (distance < minDistance) { minDistance = distance; closestIndex = i; }
        }
        return closestIndex;
    }

    private void TeleportToTunnel(string currentTunnelName) => StartCoroutine(HandleTunnelTeleport(currentTunnelName));

    private IEnumerator HandleTunnelTeleport(string currentTunnelName)
    {
        if (cameraController != null) yield return StartCoroutine(cameraController.StartFollowingCoroutine(transform));
        yield return StartCoroutine(Fade(0, 0.5f));

        string destinationName = (currentTunnelName == "Tunnel1 (15)") ? "Tunnel2 (32)" : "Tunnel1 (15)";
        int newIndex = targetWaypoints.FindIndex(w => w.Name == destinationName);

        if (newIndex != -1)
        {
            currentPositionIndex = newIndex;
            transform.position = new Vector3(targetWaypoints[newIndex].Position.x, targetWaypoints[newIndex].Position.y, spriteZPosition);
        }

        yield return StartCoroutine(Fade(1, 0.5f));
        if (cameraController != null) cameraController.StopFollowing();
        if (diceController != null) diceController.OnPlayerTurnFinished();
    }

    private IEnumerator Fade(float targetAlpha, float duration)
    {
        if (spriteRenderer == null) yield break;
        Color startColor = spriteRenderer.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            spriteRenderer.color = Color.Lerp(startColor, endColor, elapsedTime / duration);
            yield return null;
        }
        spriteRenderer.color = endColor;
    }
    private void StartMarbleMinigame()
    {
        if (marbleMinigameScenes == null || marbleMinigameScenes.Count == 0)
        {
            Debug.LogError("no minigame scenes assigned to marbleMinigameScenes list!");
            return;
        }

        int index = Random.Range(0, marbleMinigameScenes.Count);
        string selectedScene = marbleMinigameScenes[index];

        Debug.Log("Loading minigame: " +  selectedScene);

        SceneManager.LoadScene(selectedScene);
    }

    private void UpdateGarbageText() { if (garbageText != null) garbageText.text = $"{playerName}: {garbageCount} garbage"; }
    private void IncrementGarbageCount() { garbageCount++; UpdateGarbageText(); }
    private void DecrementGarbageCount() { if (garbageCount > 0) { garbageCount--; UpdateGarbageText(); } }
    private void SetRunningAnimation(bool isRunning) { if (playerAnimator != null) playerAnimator.SetBool("Running", isRunning); }
}
