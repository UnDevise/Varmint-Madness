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

    private int currentPlayer = 0;
    private int dangerButtonIndex;
    private bool gameOver = false;

    void Start()
    {
        if (buttons == null || buttons.Length == 0)
        {
            Debug.LogError("BowserBlastMinigameManager: No buttons assigned.");
            return;
        }

        if (buttonPositions == null || buttonPositions.Length == 0)
        {
            Debug.LogError("BowserBlastMinigameManager: No buttonPositions assigned.");
            return;
        }

        if (players == null || players.Length == 0)
        {
            Debug.LogError("BowserBlastMinigameManager: No players assigned.");
            return;
        }

        if (playerSpawnPositions == null || playerSpawnPositions.Length < players.Length)
        {
            Debug.LogError("BowserBlastMinigameManager: Not enough playerSpawnPositions for all players.");
            return;
        }

        dangerButtonIndex = Random.Range(0, buttons.Length);

        int count = Mathf.Min(players.Length, playerSpawnPositions.Length);
        for (int i = 0; i < count; i++)
        {
            players[i].Initialize(this, playerSpawnPositions[i], buttonPositions);
        }

        StartPlayerTurn();
    }

    void StartPlayerTurn()
    {
        if (gameOver) return;

        // If only one player left → winner
        if (players.Length == 1)
        {
            EndGameWithWinner(players[0]);
            return;
        }

        players[currentPlayer].EnableInput(true);
    }

    public void OnPlayerSelectedButton(int index)
    {
        StartCoroutine(ResolveButtonPress(index));
    }

    IEnumerator ResolveButtonPress(int buttonIndex)
    {
        buttons[buttonIndex].PlayPressAnimation();
        yield return new WaitForSeconds(1f);

        if (buttonIndex == dangerButtonIndex)
        {
            // Player explodes and is removed
            explosionSound.Play();
            buttons[buttonIndex].PlayExplosionFX();

            yield return new WaitForSeconds(1.5f);

            RemovePlayer(currentPlayer);
            RemoveButton(buttonIndex);

            // Re-roll danger button among remaining buttons
            dangerButtonIndex = Random.Range(0, buttons.Length);

            // Adjust currentPlayer index
            if (currentPlayer >= players.Length)
                currentPlayer = 0;

            StartPlayerTurn();
        }
        else
        {
            // Safe button removed
            RemoveButton(buttonIndex);

            // Next player
            currentPlayer++;
            if (currentPlayer >= players.Length)
                currentPlayer = 0;

            StartPlayerTurn();
        }
    }

    void RemovePlayer(int index)
    {
        List<PlayerMovementBlast> list = new List<PlayerMovementBlast>(players);
        list.RemoveAt(index);
        players = list.ToArray();
    }

    void RemoveButton(int index)
    {
        List<ButtonSpriteController> list = new List<ButtonSpriteController>(buttons);
        list[index].DisableButton();
        list.RemoveAt(index);
        buttons = list.ToArray();
    }

    void EndGameWithWinner(PlayerMovementBlast winner)
    {
        gameOver = true;

        // Winner gets reward
        MarbleRewardData.WinnerPlayerIndices.Clear();
        MarbleRewardData.WinnerPlayerIndices.Add(winner.playerIndex);
        MarbleRewardData.BonusTrash = 20;

        SceneManager.LoadScene(BoardStateSaver.lastBoardSceneName);
    }
}