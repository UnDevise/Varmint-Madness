using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class BowserBlastMinigameManager : MonoBehaviour
{
    public ButtonSpriteController[] buttons;
    public PlayerMovementBlast[] players;

    public Transform[] buttonPositions;
    public Transform[] playerSpawnPositions;

    public AudioSource explosionSound;

    private ButtonSpriteController[] originalButtons;
    private Transform[] originalButtonPositions;

    private int currentPlayer = 0;
    private int dangerButtonIndex;
    private bool gameOver = false;

    void Start()
    {
        // Store originals for full reset
        originalButtons = (ButtonSpriteController[])buttons.Clone();
        originalButtonPositions = (Transform[])buttonPositions.Clone();

        dangerButtonIndex = Random.Range(0, buttons.Length);

        // Initialize players at spawn
        for (int i = 0; i < players.Length; i++)
        {
            players[i].Initialize(this, playerSpawnPositions[i], buttonPositions);
        }

        StartPlayerTurn();
    }

    public void StartPlayerTurn()
    {
        if (gameOver) return;

        if (players.Length == 1)
        {
            EndGameWithWinner(players[0]);
            return;
        }

        // Disable all players
        for (int i = 0; i < players.Length; i++)
            players[i].EndTurn();

        // Begin current player's turn
        players[currentPlayer].BeginTurn();
    }

    public void OnPlayerSelectedButton(int index)
    {
        StartCoroutine(ResolveButtonPress(index));
    }

    IEnumerator ResolveButtonPress(int buttonIndex)
    {
        buttons[buttonIndex].PlayPressAnimation();
        yield return new WaitForSeconds(1f);

        bool isBomb = (buttonIndex == dangerButtonIndex);

        if (isBomb)
        {
            explosionSound.Play();
            buttons[buttonIndex].PlayExplosionFX();
            yield return new WaitForSeconds(1.5f);

            int eliminatedPlayer = currentPlayer;

            RemovePlayer(eliminatedPlayer);

            ResetButtonsAndPositions();

            if (players.Length == 1)
            {
                EndGameWithWinner(players[0]);
                yield break;
            }

            // Next round starts with the player AFTER the eliminated one
            currentPlayer = eliminatedPlayer;
            if (currentPlayer >= players.Length)
                currentPlayer = 0;

            StartPlayerTurn();
            yield break;
        }
        else
        {
            RemoveButton(buttonIndex);

            if (buttons.Length == 1)
                dangerButtonIndex = 0;
        }

        // Walk back then next player
        players[currentPlayer].StartWalkingBack();

        currentPlayer++;
        if (currentPlayer >= players.Length)
            currentPlayer = 0;
    }

    void RemovePlayer(int index)
    {
        players[index].ExplodeAndRemove();

        List<PlayerMovementBlast> list = new List<PlayerMovementBlast>(players);
        list.RemoveAt(index);
        players = list.ToArray();
    }

    void RemoveButton(int index)
    {
        buttons[index].DisableButton();

        List<ButtonSpriteController> list = new List<ButtonSpriteController>(buttons);
        list.RemoveAt(index);
        buttons = list.ToArray();

        List<Transform> posList = new List<Transform>(buttonPositions);
        posList.RemoveAt(index);
        buttonPositions = posList.ToArray();

        foreach (var p in players)
            p.UpdateButtonPositions(buttonPositions);
    }

    void ResetButtonsAndPositions()
    {
        // Reactivate all buttons
        for (int i = 0; i < originalButtons.Length; i++)
            originalButtons[i].gameObject.SetActive(true);

        // Restore arrays
        buttons = (ButtonSpriteController[])originalButtons.Clone();
        buttonPositions = (Transform[])originalButtonPositions.Clone();

        // Update players
        foreach (var p in players)
        {
            p.UpdateButtonPositions(buttonPositions);
            p.ReturnToSpawnInstant();
        }

        // New bomb each round
        dangerButtonIndex = Random.Range(0, buttons.Length);
    }

    void EndGameWithWinner(PlayerMovementBlast winner)
    {
        gameOver = true;

        MarbleRewardData.WinnerPlayerIndices.Clear();
        MarbleRewardData.WinnerPlayerIndices.Add(winner.playerIndex);
        MarbleRewardData.BonusTrash = 20;

        SceneManager.LoadScene(BoardStateSaver.lastBoardSceneName);
    }
}