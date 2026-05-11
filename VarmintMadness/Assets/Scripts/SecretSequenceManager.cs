using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class SecretSequenceManager : MonoBehaviour
{
    public PlayerSequenceInput[] players;
    public Transform turnPosition;

    public AudioSource backgroundMusic;
    public AudioSource victorySound;

    public SequencePad[] pads; // Assign in Inspector

    private List<int> sequence = new List<int>();
    private int currentPlayer = 0;

    private bool inputEnabled = false;
    private bool gameOver = false;
    private bool waitingForMovement = false;

    // ⭐ NEW: Expose the actual player ID for SequencePad
    public int currentPlayerID
    {
        get
        {
            if (players == null || players.Length == 0) return -1;
            if (currentPlayer < 0 || currentPlayer >= players.Length) return -1;
            return players[currentPlayer].playerIndex; // PlayerSequenceInput has playerIndex
        }
    }

    void Start()
    {
        // Build active players list from whoever MinigameCharacterApplier activated
        List<PlayerSequenceInput> activePlayers = new List<PlayerSequenceInput>();
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null && players[i].gameObject.activeSelf)
                activePlayers.Add(players[i]);
        }
        players = activePlayers.ToArray();

        StartNewRound();
    }

    void StartNewRound()
    {
        inputEnabled = false;

        // Pads cannot be pressed while showing pattern
        SetPadsPressable(false);
        SetPadsShowingPattern(true);

        int newStep = Random.Range(0, pads.Length);
        sequence.Add(newStep);

        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        inputEnabled = false;

        yield return new WaitForSeconds(0.5f);

        // ⭐ Show the pattern
        foreach (int step in sequence)
        {
            pads[step].LightUp();
            yield return new WaitForSeconds(0.4f);
            pads[step].Dim();
            yield return new WaitForSeconds(0.2f);
        }

        // Pattern finished
        SetPadsShowingPattern(false);

        // Move current player to turn position
        waitingForMovement = true;
        players[currentPlayer].MoveToTurnPosition(turnPosition.position);
    }

    public void OnPlayerFinishedMoving()
    {
        if (!waitingForMovement) return;

        waitingForMovement = false;

        players[currentPlayer].BeginInput(sequence.Count);

        // ⭐ Now pads can be pressed
        SetPadsPressable(true);
        inputEnabled = true;

        // ⭐ Only this player can press pads
        foreach (var pad in pads)
            pad.allowedPlayerID = currentPlayerID;
    }

    public void OnPadClicked(int padID)
    {
        if (!inputEnabled || gameOver) return;

        bool correct = players[currentPlayer].RegisterInput(padID, sequence);

        if (!correct)
        {
            EliminatePlayer(currentPlayer);
            return;
        }

        if (players[currentPlayer].HasCompletedSequence())
        {
            AdvanceTurn();
        }
    }

    void AdvanceTurn()
    {
        inputEnabled = false;
        SetPadsPressable(false);

        players[currentPlayer].ReturnToStartPosition();

        currentPlayer++;
        if (currentPlayer >= players.Length)
            currentPlayer = 0;

        StartCoroutine(DelayNextRound());
    }

    IEnumerator DelayNextRound()
    {
        yield return new WaitForSeconds(1.2f);
        StartNewRound();
    }

    void EliminatePlayer(int index)
    {
        players[index].gameObject.SetActive(false);

        List<PlayerSequenceInput> list = new List<PlayerSequenceInput>(players);
        list.RemoveAt(index);
        players = list.ToArray();

        if (players.Length == 1)
        {
            EndGameWithWinner(players[0]);
            return;
        }

        if (index < currentPlayer)
            currentPlayer--;

        if (currentPlayer >= players.Length)
            currentPlayer = 0;

        sequence.Clear();
        StartNewRound();
    }

    void EndGameWithWinner(PlayerSequenceInput winner)
    {
        gameOver = true;

        if (backgroundMusic != null)
            backgroundMusic.Stop();

        if (victorySound != null)
            victorySound.Play();

        MarbleRewardData.WinnerPlayerIndices.Clear();
        MarbleRewardData.WinnerPlayerIndices.Add(winner.playerIndex);
        MarbleRewardData.BonusTrash = 10;

        StartCoroutine(ReturnToBoard());
    }

    IEnumerator ReturnToBoard()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("LoadingScene");
    }

    // ⭐ Pad control helpers
    public void SetPadsPressable(bool value)
    {
        foreach (var pad in pads)
            pad.canBePressed = value;
    }

    public void SetPadsShowingPattern(bool value)
    {
        foreach (var pad in pads)
            pad.isShowingPattern = value;
    }
}