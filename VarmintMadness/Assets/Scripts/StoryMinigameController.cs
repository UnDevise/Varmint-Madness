using UnityEngine;
using System.Collections.Generic;

public class StoryMinigameController : MonoBehaviour
{
    public ScenarioGenerator generator;
    public TextToSpeech tts;

    public List<PlayerMovement> players;
    private int currentPlayerIndex = 0;

    private Dictionary<string, string> playerStories = new Dictionary<string, string>();

    public void StartMinigame()
    {
        currentPlayerIndex = 0;
        NextTurn();
    }

    public void NextTurn()
    {
        if (currentPlayerIndex >= players.Count)
        {
            StartVotingPhase();
            return;
        }

        PlayerMovement p = players[currentPlayerIndex];

        string story = generator.GenerateScenario();
        playerStories[p.playerId] = story;

        Debug.Log($"Player {p.playerName} story: {story}");

        tts.Speak(story);

        currentPlayerIndex++;
    }

    void StartVotingPhase()
    {
        Debug.Log("Voting phase started!");
        // Show UI for players to vote
    }

    public void PlayerVoted(string votedPlayerId)
    {
        // Count votes, determine winner
        Debug.Log("Players voted for: " + votedPlayerId);

        AwardWinner(votedPlayerId);
    }

    void AwardWinner(string playerId)
    {
        foreach (var p in players)
        {
            if (p.playerId == playerId)
            {
                p.garbageCount += 10;
                p.UpdateGarbageText();
                Debug.Log(p.playerName + " won the story minigame!");
            }
        }

        // Return to board
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoadingScene");
    }
}