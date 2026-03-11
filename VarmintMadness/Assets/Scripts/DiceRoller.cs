using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI; // Necessary for UI Image

public class DiceController : MonoBehaviour
{
    [Header("Player Settings")]
    public List<PlayerMovement> playersToMove;
    public int currentPlayerIndex = 0;

    [Header("Dice Settings")]
    public int diceSides = 6;
    public Sprite[] diceSprites;
    public DiceRoller diceRoller;
    public Collider diceCollider;
    public Transform physicsDiceTransform;

    [Header("UI Settings")]
    public TextMeshProUGUI turnNotificationText;
    public TextMeshProUGUI playerGarbageTextPrefab;
    public Transform uiParentPanel;
    public float uiElementSpacing = 50f;
    public float startXPosition = 0f;
    public float startYPosition = -50f;
    public float textSize = 24f;

    [Header("Fade Settings")]
    public Image fadeImage; // Drag your UI Image here
    public float fadeSpeed = 1f;

    [Header("Systems")]
    public ShopManager shopManager;
    public List<string> roundMinigames = new List<string>();

    private Vector3 originalDicePosition;
    private Quaternion originalDiceRotation;
    private SpriteRenderer spriteRenderer;
    private int turnsCompleted = 0;
    private bool isWaitingForSpecialSquare = false; // Prevents turn skipping too early

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (uiParentPanel == null)
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas != null)
                uiParentPanel = canvas.transform;
        }

        for (int i = 0; i < playersToMove.Count; i++)
        {
            PlayerMovement player = playersToMove[i];
            TextMeshProUGUI newText = Instantiate(playerGarbageTextPrefab, uiParentPanel);
            newText.fontSize = textSize;
            newText.rectTransform.anchoredPosition = new Vector2(startXPosition, startYPosition - i * uiElementSpacing);
            player.playerName = player.gameObject.name;
            newText.text = $"{player.playerName}: 0 Garbage";
            player.garbageText = newText;
            player.SetDiceController(this);
        }

        if (physicsDiceTransform != null)
        {
            originalDicePosition = physicsDiceTransform.position;
            originalDiceRotation = physicsDiceTransform.rotation;
        }

        // Ensure fade image starts invisible
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.raycastTarget = false;
        }
    }

    private void Start()
    {
        RestoreBoardState();
        ApplyMarbleReward();
        SetFirstAvailablePlayer();
        StartCoroutine(BeginAfterRestore());
    }

    private IEnumerator BeginAfterRestore()
    {
        yield return null;
        StartPlayerTurn();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (shopManager != null && shopManager.shopOpen)
            {
                shopManager.CloseShop();
                return;
            }

            PlayerMovement current = playersToMove[currentPlayerIndex];
            if (diceRoller != null && diceRoller.IsRolling) return;
            if (current != null && current.IsMoving) return;

            if (CameraController.Instance != null)
                CameraController.Instance.FollowPlayer(current.transform);

            if (shopManager != null) shopManager.OpenShop();
        }
    }

    // --- UPDATED TEXT SYSTEM ---

    public void UpdateTurnText(string message)
    {
        if (turnNotificationText != null)
            turnNotificationText.text = message;
    }

    public void UpdateTurnTextWithDelay(string message, float delaySeconds = 2.0f)
    {
        StartCoroutine(ShowSpecialMessage(message, delaySeconds));
    }

    private IEnumerator ShowSpecialMessage(string message, float delay)
    {
        isWaitingForSpecialSquare = true;
        UpdateTurnText(message);
        yield return new WaitForSeconds(delay);
        isWaitingForSpecialSquare = false;
        OnPlayerTurnFinished();
    }

    public void StartPlayerTurn()
    {
        CheckForWinner();
        DisableDice();

        PlayerMovement current = playersToMove[currentPlayerIndex];
        UpdateTurnText($"{current.playerName} is rolling...");

        if (CameraController.Instance != null)
        {
            CameraController.Instance.StopFollowing();
            CameraController.Instance.StartFollowing(current.transform);
        }

        if (current.ShouldSkipTurn())
        {
            current.IsStunned = false;
            UpdateTurnTextWithDelay($"{current.playerName} is stunned and skips a turn!");
            return;
        }

        if (current.IsInCage)
        {
            UpdateTurnText($"{current.playerName} is in the cage!");
            OnPlayerTurnFinished();
            return;
        }

        EnableDice();
    }

    public void MoveCurrentPlayer(int rollResult)
    {
        PlayerMovement currentPlayer = playersToMove[currentPlayerIndex];
        UpdateTurnText($"{currentPlayer.playerName} rolled {rollResult}!");

        if (!currentPlayer.IsStunned)
        {
            if (spriteRenderer != null && rollResult > 0 && rollResult <= diceSprites.Length)
                spriteRenderer.sprite = diceSprites[rollResult - 1];
        }

        currentPlayer.MoveCharacter(rollResult);
    }

    public void RollAgain()
    {
        int rollResult = Random.Range(1, diceSides + 1);
        MoveCurrentPlayer(rollResult);
    }

    public void OnPlayerTurnFinished()
    {
        if (isWaitingForSpecialSquare) return;
        StartCoroutine(DelayedTurnTransition());
    }

    private IEnumerator DelayedTurnTransition()
    {
        yield return new WaitForSeconds(1.5f);

        EnableDice();
        turnsCompleted++;

        if (turnsCompleted >= playersToMove.Count)
        {
            turnsCompleted = 0;
            StartMinigameRound();
            yield break;
        }

        currentPlayerIndex = (currentPlayerIndex + 1) % playersToMove.Count;
        ResetDicePhysics();
        StartPlayerTurn();
    }

    // --- FADE LOGIC ---

    private IEnumerator FadeAndLoad(string sceneName)
    {
        if (fadeImage == null)
        {
            SceneManager.LoadScene(sceneName);
            yield break;
        }

        fadeImage.raycastTarget = true;
        float alpha = 0;
        while (alpha < 1)
        {
            alpha += Time.deltaTime * fadeSpeed;
            Color c = fadeImage.color;
            c.a = alpha;
            fadeImage.color = c;
            yield return null;
        }

        SceneManager.LoadScene(sceneName);
    }

    // --- NECESSARY METHODS FOR OTHER SCRIPTS ---

    public bool IsPlayerMoving()
    {
        if (playersToMove.Count > 0 && playersToMove[currentPlayerIndex] != null)
        {
            Rigidbody playerRb = playersToMove[currentPlayerIndex].GetComponent<Rigidbody>();
            if (playerRb != null)
                return playerRb.linearVelocity.sqrMagnitude > 0.01f;
        }
        return false;
    }

    public void DisableDice() { if (diceCollider != null) diceCollider.enabled = false; }
    public void EnableDice() { if (diceCollider != null) diceCollider.enabled = true; }

    private void ResetDicePhysics()
    {
        if (physicsDiceTransform != null)
        {
            Rigidbody rb = physicsDiceTransform.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            physicsDiceTransform.position = originalDicePosition;
            physicsDiceTransform.rotation = originalDiceRotation;
        }
    }

    private void StartMinigameRound()
    {
        BoardStateSaver.SaveBoardState(playersToMove.Count, this);
        SetFirstAvailablePlayer();
        int index = Random.Range(0, roundMinigames.Count);
        StartCoroutine(FadeAndLoad(roundMinigames[index]));
    }

    public void CheckForWinner()
    {
        int activePlayers = 0;
        PlayerMovement lastStanding = null;
        foreach (PlayerMovement p in playersToMove)
        {
            if (!p.IsInCage) { activePlayers++; lastStanding = p; }
        }
        if (activePlayers == 1 && lastStanding != null)
        {
            WinnerData.WinnerName = lastStanding.playerName;
            SpriteRenderer sr = lastStanding.GetComponent<SpriteRenderer>();
            WinnerData.WinnerSprite = sr != null ? sr.sprite : null;
            StartCoroutine(FadeAndLoad("Winner"));
        }
    }

    private void RestoreBoardState()
    {
        if (BoardStateSaver.savedPositions != null)
        {
            for (int i = 0; i < playersToMove.Count; i++)
            {
                playersToMove[i].transform.position = BoardStateSaver.savedPositions[i];
                playersToMove[i].IsInCage = BoardStateSaver.playerIsInCage[i];
                playersToMove[i].garbageCount = BoardStateSaver.savedGarbageCounts[i];
                playersToMove[i].UpdateGarbageText();
            }
        }
    }

    private void SetFirstAvailablePlayer()
    {
        for (int i = 0; i < playersToMove.Count; i++)
        {
            if (!playersToMove[i].IsInCage) { currentPlayerIndex = i; return; }
        }
        currentPlayerIndex = 0;
    }

    private void ApplyMarbleReward()
    {
        // No winners? Nothing to apply.
        if (MarbleRewardData.WinnerPlayerIndices == null ||
            MarbleRewardData.WinnerPlayerIndices.Count == 0)
            return;

        // Apply reward to ALL winners (supports ties)
        foreach (int index in MarbleRewardData.WinnerPlayerIndices)
        {
            if (index >= 0 && index < playersToMove.Count)
            {
                PlayerMovement winner = playersToMove[index];
                winner.garbageCount += MarbleRewardData.BonusTrash;
                winner.UpdateGarbageText();
            }
        }

        // Reset reward data
        MarbleRewardData.WinnerPlayerIndices.Clear();
        MarbleRewardData.BonusTrash = 0;
    }
    public void SaveBoardStateBeforeMinigame()
    {
        // Save scene name
        BoardStateSaver.lastBoardSceneName = SceneManager.GetActiveScene().name;

        // Save board layer, tile index, cage, stun, etc.
        int count = playersToMove.Count;

        BoardStateSaver.playerBoardLayer = new int[count];
        BoardStateSaver.playerTileIndex = new int[count];
        BoardStateSaver.playerIsStunned = new bool[count];
        BoardStateSaver.playerIsInCage = new bool[count];

        for (int i = 0; i < count; i++)
        {
            PlayerMovement p = playersToMove[i];

            BoardStateSaver.playerBoardLayer[i] =
                (p.waypointsParent == p.alternativeWaypointsParent) ? 1 : 0;

            BoardStateSaver.playerTileIndex[i] = p.GetCurrentTileIndex();
            BoardStateSaver.playerIsStunned[i] = p.IsStunned;
            BoardStateSaver.playerIsInCage[i] = p.IsInCage;
        }
    }

}
