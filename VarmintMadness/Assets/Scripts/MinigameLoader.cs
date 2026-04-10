using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameLoader : MonoBehaviour
{
    // Optional: MinigameInfo assets (if your minigames use intro screens)
    public MinigameInfo blastInfo;
    public MinigameInfo secretSequenceInfo;

    // Read by minigame intro screens
    public static MinigameInfo nextMinigameInfo;

    // Load any minigame by name
    public void LoadMinigame(string minigameSceneName)
    {
        if (string.IsNullOrEmpty(minigameSceneName))
        {
            Debug.LogError("MinigameLoader: Minigame scene name is EMPTY!");
            return;
        }

        // Save which board scene we came from
        BoardStateSaver.lastBoardSceneName = SceneManager.GetActiveScene().name;

        // Save full board state (positions, garbage, tile index, character, cage, stun)
        DiceController dice = FindFirstObjectByType<DiceController>();
        if (dice != null)
        {
            dice.SaveBoardStateBeforeMinigame();
        }

        // Mark that we are returning from a minigame
        BoardStateSaver.returningFromMinigame = true;

        Debug.Log("MinigameLoader: Loading minigame → " + minigameSceneName);

        // Load the minigame directly
        SceneManager.LoadScene(minigameSceneName);
    }

    // Convenience functions for specific minigames
    public void LoadBlast()
    {
        nextMinigameInfo = blastInfo;
        LoadMinigame("Blast");
    }

    public void LoadSecretSequence()
    {
        nextMinigameInfo = secretSequenceInfo;
        LoadMinigame("SecretSequence");
    }

    // Called by minigames when finished
    public void ReturnToBoard()
    {
        if (!string.IsNullOrEmpty(BoardStateSaver.lastBoardSceneName))
        {
            SceneManager.LoadScene(BoardStateSaver.lastBoardSceneName);
        }
        else
        {
            Debug.LogWarning("MinigameLoader: No board scene saved!");
        }
    }
}