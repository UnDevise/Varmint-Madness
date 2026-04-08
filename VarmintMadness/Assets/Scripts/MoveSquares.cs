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
    public bool isMoving = false;
    public int characterId;

    [Header("Character Appearance")]
    public Sprite[] characterSprites; // Assign in Inspector
    public SpriteRenderer characterRenderer; // Assign in Inspector

    [Header("Character Models")]
    public GameObject[] characterModels; // assign all character prefabs or child objects


    public bool IsInCage = false;
    public Transform cageTeleportPoint;

    public List<string> marbleMinigameScenes = new List<string>();

    [Header("Special Square Sounds")]
    public AudioClip moveBackSound;
    public AudioClip MinigameSound;
    public AudioClip switchLayerSound;
    public AudioClip tunnelSound;
    public AudioClip garbageAddSound;
    public AudioClip garbageRemoveSound;
    public AudioClip cageSound;
    public AudioClip stunSound;
    public AudioClip trainSound;

    [Header("Bomb Space")]
    public AudioClip bombExplodeSound;
    public ParticleSystem bombExplosionFX;
    public float bombExplodeChance = 0.5f;

    private List<WaypointData> targetWaypoints = new List<WaypointData>();
    public int currentPositionIndex = 0;
    private Transform originalWaypointsParent;
    public bool IsMoving { get; private set; } = false;
    public bool IsStunned { get; set; } = false;
    public float spriteZPosition = -5.0f;
    [HideInInspector] public TextMeshProUGUI garbageText;
    [HideInInspector] public string playerName;
    public int garbageCount = 0;
    private DiceController diceController;
    private CameraController cameraController;
    public TrainController trainController;
    private Animator playerAnimator;
    private AudioSource audioSource;
    public string playerId;
    public int CurrentBoardLayer { get; set; } = 0;

    // Expose the tile index safely
    public int CurrentPositionIndex
    {
        get => currentPositionIndex;
        set => currentPositionIndex = value;
    }


    private void Awake()
    {
        originalWaypointsParent = waypointsParent;
        playerAnimator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (waypointsParent != null) StoreWaypointData();
        if (CameraController.Instance != null) cameraController = CameraController.Instance;
    }

    private void Start()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (scene.Contains("Marble") || scene.Contains("Minigame"))
            return;

        if (targetWaypoints.Count == 0) return;

        if (BoardStateSaver.playerBoardLayer != null)
        {
            int index = diceController.playersToMove.IndexOf(this);

            if (index >= 0)
            {
                bool shouldBeOnSewer = BoardStateSaver.playerBoardLayer[index] == 1;

                if (shouldBeOnSewer)
                    MoveToSewerBoard();
                else
                    MoveToTopBoard();
            }
        }

        if (BoardStateSaver.playerTileIndex != null)
        {
            int index = diceController.playersToMove.IndexOf(this);

            if (index >= 0)
            {
                currentPositionIndex = BoardStateSaver.playerTileIndex[index];

                if (currentPositionIndex >= 0 && currentPositionIndex < targetWaypoints.Count)
                {
                    Vector3 pos = targetWaypoints[currentPositionIndex].Position;
                    pos.z = spriteZPosition;
                    transform.position = pos;
                }
            }
        }

        if (BoardStateSaver.playerIsStunned != null)
        {
            int index = diceController.playersToMove.IndexOf(this);

            if (index >= 0)
                IsStunned = BoardStateSaver.playerIsStunned[index];
        }

        if (BoardStateSaver.playerIsInCage != null)
        {
            int index = diceController.playersToMove.IndexOf(this);

            if (index >= 0)
                IsInCage = BoardStateSaver.playerIsInCage[index];
        }

        if (IsInCage && cageTeleportPoint != null)
        {
            transform.position = cageTeleportPoint.position;
            currentPositionIndex = -1;
        }

        diceController.CheckForWinner();
        UpdateGarbageText();
    }

    private void PlaySquareSound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    public void SetDiceController(DiceController controller) => diceController = controller;

    private void StoreWaypointData()
    {
        targetWaypoints.Clear();
        if (waypointsParent != null)
        {
            foreach (Transform child in waypointsParent)
                targetWaypoints.Add(new WaypointData(child.position, child.tag, child.name));
        }
    }

    private void FaceTowards(Vector3 targetPos)
    {
        float direction = targetPos.x - transform.position.x;

        if (direction > 0.01f)
            characterRenderer.flipX = false;
        else if (direction < -0.01f)
            characterRenderer.flipX = true;
    }

    public bool ShouldSkipTurn()
    {
        if (IsInCage) return true;
        if (IsStunned) return true;
        return false;
    }

    public void MoveCharacter(int stepsToMove)
    {
        if (IsInCage)
        {
            Debug.Log($"{playerName} is in the cage and cannot move.");
            diceController?.OnPlayerTurnFinished();
            return;
        }

        if (IsStunned)
        {
            Debug.Log($"{playerName} skips a turn (stunned).");
            IsStunned = false;
            diceController?.OnPlayerTurnFinished();
            return;
        }

        StartCoroutine(HandlePlayerTurn(stepsToMove));
    }

    private IEnumerator HandlePlayerTurn(int stepsToMove)
    {
        if (cameraController != null)
            yield return StartCoroutine(cameraController.StartFollowingCoroutine(transform));

        IsMoving = true;
        SetRunningAnimation(true);
        yield return StartCoroutine(MoveSequence(stepsToMove));
        IsMoving = false;
        SetRunningAnimation(false);

        bool bonusMoveTriggered = CheckForSpecialWaypoint();

        if (!bonusMoveTriggered)
        {
            cameraController?.StopFollowing();
            diceController?.OnPlayerTurnFinished();
        }
    }

    private IEnumerator MoveSequence(int steps)
    {
        if (currentPositionIndex < 0 || targetWaypoints.Count == 0)
            yield break;

        for (int i = 0; i < Mathf.Abs(steps); i++)
        {
            int direction = steps > 0 ? 1 : -1;
            currentPositionIndex += direction;

            if (currentPositionIndex >= targetWaypoints.Count) currentPositionIndex = 0;
            else if (currentPositionIndex < 0) currentPositionIndex = targetWaypoints.Count - 1;

            Vector3 nextPosition = new Vector3(
                targetWaypoints[currentPositionIndex].Position.x,
                targetWaypoints[currentPositionIndex].Position.y,
                spriteZPosition
            );

            FaceTowards(nextPosition);

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

        if (currentPositionIndex < 0 || currentPositionIndex >= targetWaypoints.Count)
            return false;

        string currentWaypointTag = targetWaypoints[currentPositionIndex].Tag;
        string currentWaypointName = targetWaypoints[currentPositionIndex].Name;

        if (currentWaypointTag == "Cage Space")
        {
            PlaySquareSound(cageSound);
            SendPlayerToCage();
            return true;
        }
        else if (currentWaypointTag == "Roll again space")
        {
            Debug.Log($"{playerName} landed on a Roll Again space!");
            PlaySquareSound(MinigameSound);

            if (diceController != null)
                diceController.RollAgain();

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
        else if (currentWaypointTag == "Player Mover Space")
        {
            Debug.Log($"{playerName} landed on a Player Mover Space!");
            PlaySquareSound(MinigameSound);

            TeleportToRandomTile();
            return true;
        }
        else if (currentWaypointTag == "Bomb changer space")
        {
            Debug.Log($"{playerName} landed on a Bomb Space!");
            HandleBombSpace();
            return true;
        }
        else if (currentWaypointTag == "Train Mover")
        {
            PlaySquareSound(trainSound);

            if (trainController != null)
                trainController.StartTrainEvent(this);

            return true;
        }
        else if (currentWaypointTag == "Minigame Space")
        {
            PlaySquareSound(MinigameSound);

            // NEW: Load minigame through MinigameLoader
            MinigameLoader loader = Object.FindFirstObjectByType<MinigameLoader>();
            if (loader != null)
            {
                // Waypoint Name MUST match the minigame scene name
                loader.LoadMinigame(currentWaypointName);
            }
            else
            {
                Debug.LogError("MinigameLoader not found in scene! Add it to the board scene.");
            }

            return true;
        }

        return false;
    }

    private void HandleBombSpace()
    {
        float roll = Random.value;

        if (roll <= bombExplodeChance)
        {
            Debug.Log($"{playerName} triggered the bomb and got sent to the cage!");

            if (bombExplosionFX != null)
                bombExplosionFX.Play();

            if (bombExplodeSound != null)
                audioSource.PlayOneShot(bombExplodeSound);

            SendPlayerToCage();
        }
        else
        {
            Debug.Log($"{playerName} survived the bomb!");
        }
    }

    private IEnumerator FocusCameraOnCage()
    {
        if (cameraController == null || cageTeleportPoint == null)
            yield break;

        cameraController.StopAllCoroutines();

        yield return StartCoroutine(cameraController.StartFollowingCoroutine(cageTeleportPoint));

        yield return new WaitForSeconds(1.0f);

        cameraController.StopFollowing();

        if (diceController != null)
            diceController.OnPlayerTurnFinished();
    }

    private void SendPlayerToCage()
    {
        IsInCage = true;

        FaceTowards(cageTeleportPoint.position);
        transform.position = cageTeleportPoint.position;
        currentPositionIndex = -1;

        Debug.Log("Player has been sent to the cage and is out of the game.");

        StartCoroutine(FocusCameraOnCage());

        diceController.CheckForWinner();
    }

    private void StartMarbleMinigame()
    {
        if (diceController != null)
            diceController.SaveBoardStateBeforeMinigame();

        int index = Random.Range(0, marbleMinigameScenes.Count);
        string selectedScene = marbleMinigameScenes[index];

        SceneManager.LoadScene(selectedScene);
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

        FaceTowards(nextPosition);

        if (cameraController != null)
            yield return StartCoroutine(cameraController.StartFollowingCoroutine(transform));

        while (Vector2.Distance(transform.position, nextPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, nextPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = nextPosition;
        IsMoving = false;
        SetRunningAnimation(false);

        cameraController?.StopFollowing();
        diceController?.OnPlayerTurnFinished();
    }

    private int FindClosestWaypointIndex(Transform target)
    {
        if (target == null || targetWaypoints.Count == 0) return 0;

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
        => StartCoroutine(HandleTunnelTeleport(currentTunnelName));

    private IEnumerator HandleTunnelTeleport(string currentTunnelName)
    {
        if (cameraController != null)
            yield return StartCoroutine(cameraController.StartFollowingCoroutine(transform));

        yield return StartCoroutine(Fade(0, 0.5f));

        string destinationName = (currentTunnelName == "Tunnel1 (15)") ? "Tunnel2 (32)" : "Tunnel1 (15)";
        int newIndex = targetWaypoints.FindIndex(w => w.Name == destinationName);

        if (newIndex != -1)
        {
            currentPositionIndex = newIndex;

            Vector3 pos = targetWaypoints[newIndex].Position;
            pos.z = spriteZPosition;

            FaceTowards(pos);
            transform.position = pos;
        }

        yield return StartCoroutine(Fade(1, 0.5f));

        cameraController?.StopFollowing();
        diceController?.OnPlayerTurnFinished();
    }

    private IEnumerator Fade(float targetAlpha, float duration)
    {
        if (characterRenderer == null) yield break;

        Color startColor = characterRenderer.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);

        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            characterRenderer.color = Color.Lerp(startColor, endColor, elapsedTime / duration);
            yield return null;
        }

        characterRenderer.color = endColor;
    }

    public void UpdateGarbageText()
    {
        if (garbageText != null)
            garbageText.text = $"{playerName}: {garbageCount} garbage";
    }

    public void IncrementGarbageCount()
    {
        garbageCount++;
        UpdateGarbageText();
    }

    private void DecrementGarbageCount()
    {
        if (garbageCount > 0)
        {
            garbageCount--;
            UpdateGarbageText();
        }
    }

    private void SetRunningAnimation(bool isRunning)
    {
        if (playerAnimator != null)
            playerAnimator.SetBool("Running", isRunning);
    }

    public void MoveToTopBoard()
    {
        waypointsParent = originalWaypointsParent;
        StoreWaypointData();
    }

    public void MoveToSewerBoard()
    {
        waypointsParent = alternativeWaypointsParent;
        StoreWaypointData();
    }

    public int GetCurrentTileIndex()
    {
        if (currentPositionIndex < 0 || currentPositionIndex >= targetWaypoints.Count)
            return 0;

        return currentPositionIndex;
    }

    private void TeleportToRandomTile()
    {
        if (targetWaypoints == null || targetWaypoints.Count == 0)
            return;

        int randomIndex = Random.Range(0, targetWaypoints.Count);
        currentPositionIndex = randomIndex;

        Vector3 pos = targetWaypoints[randomIndex].Position;
        pos.z = spriteZPosition;

        FaceTowards(pos);
        transform.position = pos;

        Debug.Log($"{playerName} teleported to tile {randomIndex}!");
    }

    public int GetGarbage()
    {
        return garbageCount;
    }

    public void SpendGarbage(int amount)
    {
        garbageCount -= amount;
        if (garbageCount < 0) garbageCount = 0;
        UpdateGarbageText();
    }
    public void ForceMoveToTile(int tileIndex)
    {
        if (tileIndex < 0 || tileIndex >= targetWaypoints.Count)
            return;

        currentPositionIndex = tileIndex;

        Vector3 pos = targetWaypoints[tileIndex].Position;
        pos.z = spriteZPosition;

        FaceTowards(pos);
        transform.position = pos;
    }

    public void ApplyCharacter(int characterIndex)
    {
        characterId = characterIndex;
        Debug.Log($"{name} ApplyCharacter({characterIndex})");

        if (characterRenderer != null &&
            characterSprites != null &&
            characterIndex >= 0 &&
            characterIndex < characterSprites.Length)
        {
            characterRenderer.sprite = characterSprites[characterIndex];
            Debug.Log($"{name}: renderer sprite AFTER = {characterRenderer.sprite}");
        }
        else
        {
            Debug.LogWarning($"{name}: invalid characterIndex {characterIndex} or missing sprites/renderer");
        }
    }
}