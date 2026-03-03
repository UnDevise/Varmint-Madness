using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class DiceController : MonoBehaviour
{
    public List<PlayerMovement> playersToMove;

    public int diceSides = 6; // ⭐ Needed for RollAgain()
    public Sprite[] diceSprites;

    public Collider diceCollider;

    public ShopManager shopManager;

    public TextMeshProUGUI playerGarbageTextPrefab;
    public Transform uiParentPanel;
    public float uiElementSpacing = 50f;
    public int currentPlayerIndex = 0; // ⭐ Needed for turn logic

    public float startXPosition = 0f;
    public float startYPosition = -50f;
    public float textSize = 24f;

    public Transform physicsDiceTransform;
    private Vector3 originalDicePosition;
    private Quaternion originalDiceRotation;

    private SpriteRenderer spriteRenderer;

    private const float MovementThreshold = 0.01f;

    private int turnsCompleted = 0;

    [Header("Minigame Settings")]
    public List<string> roundMinigames = new List<string>();

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (uiParentPanel == null)
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas != null)
                uiParentPanel = canvas.transform;
        }

        // Create UI garbage counters
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
    }

    private void Start()
    {
        RestoreBoardState();
        StartCoroutine(BeginAfterRestore());
    }

    private IEnumerator BeginAfterRestore()
    {
        yield return null;
        StartPlayerTurn();
    }

    private void Update()
    {
        // ⭐ Shop open key
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (diceCollider != null && diceCollider.enabled)
            {
                PlayerMovement current = playersToMove[currentPlayerIndex];

                if (!current.IsMoving && !current.IsStunned && !current.IsInCage)
                {
                    shopManager.ToggleShop();
                }
            }
        }
    }

    // ⭐ Restore saved board state
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

            currentPlayerIndex = BoardStateSaver.savedCurrentPlayerIndex;
        }
    }

    // ⭐ Turn start
    public void StartPlayerTurn()
    {
        CheckForWinner();

        DisableDice();

        PlayerMovement current = playersToMove[currentPlayerIndex];

        if (current.ShouldSkipTurn())
        {
            current.IsStunned = false;
            OnPlayerTurnFinished();
            return;
        }

        EnableDice();
    }

    // ⭐ Move player after dice roll
    public void MoveCurrentPlayer(int rollResult)
    {
        PlayerMovement currentPlayer = playersToMove[currentPlayerIndex];

        if (!currentPlayer.IsStunned)
        {
            if (spriteRenderer != null && rollResult > 0 && rollResult <= diceSprites.Length)
                spriteRenderer.sprite = diceSprites[rollResult - 1];
        }

        currentPlayer.MoveCharacter(rollResult);
    }

    // ⭐ Fix for missing RollAgain()
    public void RollAgain()
    {
        int rollResult = Random.Range(1, diceSides + 1);
        Debug.Log($"Roll Again triggered! Rolled a {rollResult}");
        MoveCurrentPlayer(rollResult);
    }

    // ⭐ End turn
    public void OnPlayerTurnFinished()
    {
        EnableDice();

        turnsCompleted++;

        if (turnsCompleted >= playersToMove.Count)
        {
            turnsCompleted = 0;
            StartMinigameRound();
            return;
        }

        currentPlayerIndex++;
        if (currentPlayerIndex >= playersToMove.Count)
            currentPlayerIndex = 0;

        ResetDicePhysics();
        StartPlayerTurn();
    }

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

    // ⭐ Start minigame
    private void StartMinigameRound()
    {
        BoardStateSaver.SaveBoardState(playersToMove.Count, this);

        currentPlayerIndex = 0;

        int index = Random.Range(0, roundMinigames.Count);
        SceneManager.LoadScene(roundMinigames[index]);
    }

    // ⭐ Winner check
    public void CheckForWinner()
    {
        int activePlayers = 0;
        PlayerMovement lastStanding = null;

        foreach (PlayerMovement p in playersToMove)
        {
            if (!p.IsInCage)
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

            SceneManager.LoadScene("Winner");
        }
    }

    // ⭐ Shop ability: Move random player
    public void MoveRandomPlayer()
    {
        if (playersToMove.Count <= 1)
            return;

        int randomIndex = currentPlayerIndex;

        while (randomIndex == currentPlayerIndex)
            randomIndex = Random.Range(0, playersToMove.Count);

        PlayerMovement target = playersToMove[randomIndex];

        int roll = Random.Range(1, 7);
        target.MoveCharacter(roll);
    }

    // ⭐ Shop ability: Force random player into cage
    public void ForceRandomPlayerIntoCage()
    {
        if (playersToMove.Count <= 1)
            return;

        int randomIndex = currentPlayerIndex;

        while (randomIndex == currentPlayerIndex)
            randomIndex = Random.Range(0, playersToMove.Count);

        PlayerMovement target = playersToMove[randomIndex];

        target.IsInCage = true;

        if (target.cageTeleportPoint != null)
            target.transform.position = target.cageTeleportPoint.position;

        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void DisableDice()
    {
        if (diceCollider != null)
            diceCollider.enabled = false;
    }

    public void EnableDice()
    {
        if (diceCollider != null)
            diceCollider.enabled = true;
    }
    public bool IsPlayerMoving()
    {
        if (playersToMove.Count > 0 && playersToMove[currentPlayerIndex] != null)
        {
            Rigidbody playerRb = playersToMove[currentPlayerIndex].GetComponent<Rigidbody>();

            if (playerRb != null)
            {
                // You can tweak this threshold if needed
                return playerRb.linearVelocity.sqrMagnitude > 0.01f;
            }
        }

        return false;
    }

}