using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class SecretSequenceManager : MonoBehaviour
{
    public SequencePad[] pads; // 0–3 in a 2×2 grid
    public PlayerSequenceInput[] players;

    public AudioSource backgroundMusic;
    public AudioSource victorySound;

    private List<int> sequence = new List<int>();
    private int currentPlayer = 0;
    private bool inputEnabled = false;
    private bool gameOver = false;

    void Start()
    {
        StartNewRound();
    }

    void StartNewRound()
    {
        inputEnabled = false;

        int newStep = Random.Range(0, pads.Length);
        sequence.Add(newStep);

        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        inputEnabled = false;

        yield return new WaitForSeconds(0.5f);

        foreach (int step in sequence)
        {
            pads[step].LightUp();
            yield return new WaitForSeconds(0.4f);
            pads[step].Dim();
            yield return new WaitForSeconds(0.2f);
        }

        players[currentPlayer].BeginInput(sequence.Count);
        inputEnabled = true;
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

        currentPlayer++;
        if (currentPlayer >= players.Length)
            currentPlayer = 0;

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
        SceneManager.LoadScene(BoardStateSaver.lastBoardSceneName);
    }
}
