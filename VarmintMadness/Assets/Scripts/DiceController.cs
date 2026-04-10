using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class DiceController : MonoBehaviour
{
    [Header("Player Settings")]
    public List<PlayerMovement> playersToMove;
    public int currentPlayerIndex = 0;
    public int startingGarbage = 1;

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
    public Image fadeImage;
    public float fadeSpeed = 1f;

    [Header("Systems")]
    public List<string> roundMinigames = new List<string>();

    private Vector3 originalDicePosition;
    private Quaternion originalDiceRotation;
    private int turnsCompleted = 0;
    private bool isWaitingForSpecialSquare = false;

    private void Awake()
    {
        if (startingGarbage <= 0) startingGarbage = 5;

        if (uiParentPanel == null)
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas != null) uiParentPanel = canvas.transform;
        }

        // NEW GAME ONLY
        if (!BoardStateSaver.returningFromMinigame)
        {
            BoardStateSaver.playerCharacterIndices = new int[playersToMove.Count];

            for (int i = 0; i < playersToMove.Count; i++)
            {
                PlayerMovement p = playersToMove[i];
                int id = p.playerID;

                int charIndex = PlayerPrefs.GetInt($"P{id + 1}_Character", 0);

                p.ApplyCharacter(charIndex);
                BoardStateSaver.playerCharacterIndices[id] = charIndex;

                p.garbageCount = startingGarbage;
            }

            for (int i = 0; i < playersToMove.Count; i++)
            {
                Debug.Log($"PLAYER OBJECT CHECK: index={i}, name={playersToMove[i].name}, playerID={playersToMove[i].playerID}");
            }

        }

        // UI setup
        for (int i = 0; i < playersToMove.Count; i++)
        {
            PlayerMovement player = playersToMove[i];

            TextMeshProUGUI newText = Instantiate(playerGarbageTextPrefab, uiParentPanel);
            newText.fontSize = textSize;
            newText.rectTransform.anchoredPosition =
                new Vector2(startXPosition, startYPosition - i * uiElementSpacing);

            if (string.IsNullOrEmpty(player.playerName))
                player.playerName = player.gameObject.name;

            player.garbageText = newText;
            player.UpdateGarbageText();
            player.SetDiceController(this);
        }
    }

    private void Start()
    {
        // If this is a NEW GAME, wipe old save data
        if (!BoardStateSaver.returningFromMinigame)
        {
            Debug.Log("NEW GAME — Clearing BoardStateSaver");
            BoardStateSaver.Clear();
        }

        // Only restore if we actually came from a minigame
        if (BoardStateSaver.returningFromMinigame)
        {
            Debug.Log("RETURNING FROM MINIGAME — Restoring state");
            RestoreBoardState();
        }

        ApplyMarbleReward();
        SetFirstAvailablePlayer();
        StartCoroutine(BeginAfterRestore());
    }


    private IEnumerator BeginAfterRestore()
    {
        yield return null;
        StartPlayerTurn();
    }

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

        if (current.garbageCount <= 0 || current.IsInCage)
        {
            string reason = current.garbageCount <= 0 ? "is out of garbage" : "is in the cage";
            UpdateTurnText($"{current.playerName} {reason} and is skipped!");
            OnPlayerTurnFinished();
            return;
        }

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

        EnableDice();
    }

    public void MoveCurrentPlayer(int rollResult)
    {
        PlayerMovement currentPlayer = playersToMove[currentPlayerIndex];
        UpdateTurnText($"{currentPlayer.playerName} rolled {rollResult}!");
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
        SaveBoardStateBeforeMinigame();
        BoardStateSaver.returningFromMinigame = true;

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
            if (!p.IsInCage && p.garbageCount > 0)
            {
                activePlayers++;
                lastStanding = p;
            }
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
        if (BoardStateSaver.playerPositions == null ||
            BoardStateSaver.playerGarbageCounts == null ||
            BoardStateSaver.playerTileIndex == null ||
            BoardStateSaver.playerBoardLayer == null)
        {
            Debug.Log("Restore skipped — incomplete data.");
            return;
        }

        Debug.Log("RESTORING BOARD STATE...");

        for (int i = 0; i < playersToMove.Count; i++)
        {
            PlayerMovement p = playersToMove[i];
            int id = p.playerID;

            p.transform.position = BoardStateSaver.playerPositions[id];
            p.IsInCage = BoardStateSaver.playerIsInCage[id];
            p.IsStunned = BoardStateSaver.playerIsStunned[id];
            p.garbageCount = BoardStateSaver.playerGarbageCounts[id];
            p.CurrentPositionIndex = BoardStateSaver.playerTileIndex[id];

            if (BoardStateSaver.playerBoardLayer[id] == 1)
                p.MoveToSewerBoard();
            else
                p.MoveToTopBoard();

            p.ApplyCharacter(BoardStateSaver.playerCharacterIndices[id]);

            p.UpdateGarbageText();
        }
    }

    private void SetFirstAvailablePlayer()
    {
        for (int i = 0; i < playersToMove.Count; i++)
        {
            if (!playersToMove[i].IsInCage && playersToMove[i].garbageCount > 0)
            {
                currentPlayerIndex = i;
                return;
            }
        }
        currentPlayerIndex = 0;
    }

    private void ApplyMarbleReward()
    {
        if (MarbleRewardData.WinnerPlayerIndices == null ||
            MarbleRewardData.WinnerPlayerIndices.Count == 0)
            return;

        foreach (int index in MarbleRewardData.WinnerPlayerIndices)
        {
            if (index >= 0 && index < playersToMove.Count)
            {
                PlayerMovement winner = playersToMove[index];
                winner.garbageCount += MarbleRewardData.BonusTrash;
                winner.UpdateGarbageText();
            }
        }

        MarbleRewardData.WinnerPlayerIndices.Clear();
        MarbleRewardData.BonusTrash = 0;
    }

    public void SaveBoardStateBeforeMinigame()
    {
        BoardStateSaver.lastBoardSceneName = SceneManager.GetActiveScene().name;

        int count = playersToMove.Count;

        BoardStateSaver.playerBoardLayer = new int[count];
        BoardStateSaver.playerTileIndex = new int[count];
        BoardStateSaver.playerIsStunned = new bool[count];
        BoardStateSaver.playerIsInCage = new bool[count];
        BoardStateSaver.playerGarbageCounts = new int[count];
        BoardStateSaver.playerPositions = new Vector3[count];
        BoardStateSaver.playerCharacterIndices = new int[count];

        for (int i = 0; i < count; i++)
        {
            PlayerMovement p = playersToMove[i];
            int id = p.playerID;

            BoardStateSaver.playerPositions[id] = p.transform.position;
            BoardStateSaver.playerBoardLayer[id] =
                (p.waypointsParent == p.alternativeWaypointsParent) ? 1 : 0;
            BoardStateSaver.playerTileIndex[id] = p.CurrentPositionIndex;
            BoardStateSaver.playerIsStunned[id] = p.IsStunned;
            BoardStateSaver.playerIsInCage[id] = p.IsInCage;
            BoardStateSaver.playerGarbageCounts[id] = p.garbageCount;
            BoardStateSaver.playerCharacterIndices[id] = p.characterId;
        }
    }
}