using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameLoader : MonoBehaviour
{
    // Optional: assign MinigameInfo assets in the Inspector
    public MinigameInfo blastInfo;
    public MinigameInfo secretSequenceInfo;

    // This is read by the minigame intro screen
    public static MinigameInfo nextMinigameInfo;

    // Load a minigame by name
    public void LoadMinigame(string minigameSceneName)
    {
        if (string.IsNullOrEmpty(minigameSceneName))
        {
            Debug.LogError("MinigameLoader: Minigame scene name is EMPTY!");
            return;
        }

        // Save which board scene we came from
        BoardStateSaver.lastBoardSceneName = SceneManager.GetActiveScene().name;

        // Save full board state (positions, garbage, cage, stun, tile, character)
        DiceController dice = FindFirstObjectByType<DiceController>();
        if (dice != null)
            dice.SaveBoardStateBeforeMinigame();

        // Mark that we are returning after the minigame
        BoardStateSaver.returningFromMinigame = true;

        Debug.Log("MinigameLoader: Loading minigame → " + minigameSceneName);

        // If you use a loading screen, load that instead
        // Otherwise load the minigame directly
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