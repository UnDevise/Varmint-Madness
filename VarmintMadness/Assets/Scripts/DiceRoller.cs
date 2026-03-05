using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class DiceController : MonoBehaviour
{
    public List<PlayerMovement> playersToMove;

    public int diceSides = 6;
    public Sprite[] diceSprites;
    public DiceRoller diceRoller;

    public Collider diceCollider;

    public ShopManager shopManager;

    public TextMeshProUGUI playerGarbageTextPrefab;
    public Transform uiParentPanel;
    public float uiElementSpacing = 50f;
    public int currentPlayerIndex = 0;

    public float startXPosition = 0f;
    public float startYPosition = -50f;
    public float textSize = 24f;

    public Transform physicsDiceTransform;
    private Vector3 originalDicePosition;
    private Quaternion originalDiceRotation;

    private SpriteRenderer spriteRenderer;

    private const float MovementThreshold = 0.01f;

    private int turnsCompleted = 0;

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
        ApplyMarbleReward();
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

            if (diceRoller != null && diceRoller.IsRolling)
                return;

            if (current != null && current.IsMoving)
                return;

            if (CameraController.Instance != null)
            {
                CameraController.Instance.FollowPlayer(current.transform);
            }

            if (shopManager != null)
            {
                shopManager.OpenShop();
            }
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

            currentPlayerIndex = BoardStateSaver.savedCurrentPlayerIndex;
        }
    }

    public void StartPlayerTurn()
    {
        CheckForWinner();

        DisableDice();

        PlayerMovement current = playersToMove[currentPlayerIndex];
        CameraController.Instance.FollowPlayer(current.transform);

        if (CameraController.Instance != null)
        {
            CameraController.Instance.StopFollowing();
            CameraController.Instance.StartFollowing(current.transform);
        }

        if (current.ShouldSkipTurn())
        {
            current.IsStunned = false;

            // FIX: Properly advance turn when skipping
            currentPlayerIndex = (currentPlayerIndex + 1) % playersToMove.Count;
            StartPlayerTurn();
            return;
        }

        EnableDice();
    }

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

    public void RollAgain()
    {
        int rollResult = Random.Range(1, diceSides + 1);
        Debug.Log($"Roll Again triggered! Rolled a {rollResult}");
        MoveCurrentPlayer(rollResult);
    }

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

    private void StartMinigameRound()
    {
        BoardStateSaver.SaveBoardState(playersToMove.Count, this);

        // FIX: Removed the line that broke turn order
        // currentPlayerIndex = 0;

        int index = Random.Range(0, roundMinigames.Count);
        SceneManager.LoadScene(roundMinigames[index]);
    }

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
                return playerRb.linearVelocity.sqrMagnitude > 0.01f;
            }
        }

        return false;
    }

    private void RestoreGarbageCounts()
    {
        if (BoardStateSaver.savedGarbageCounts == null)
            return;

        for (int i = 0; i < playersToMove.Count; i++)
        {
            playersToMove[i].garbageCount = BoardStateSaver.savedGarbageCounts[i];
            playersToMove[i].UpdateGarbageText();
        }
    }

    private void ApplyMarbleReward()
    {
        if (!MarbleRewardData.WinnerPlayerIndex.HasValue)
            return;

        int index = MarbleRewardData.WinnerPlayerIndex.Value;

        if (index >= 0 && index < playersToMove.Count)
        {
            PlayerMovement winner = playersToMove[index];
            winner.garbageCount += MarbleRewardData.BonusTrash;
            winner.UpdateGarbageText();
        }

        MarbleRewardData.WinnerPlayerIndex = null;
        MarbleRewardData.BonusTrash = 0;
    }
}