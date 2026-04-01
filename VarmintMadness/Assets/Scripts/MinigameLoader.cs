using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameLoader : MonoBehaviour
{
    // Optional: assign MinigameInfo assets in the Inspector
    public MinigameInfo blastInfo;
    public MinigameInfo secretSequenceInfo;

    // This is read by the minigame intro screen
    public static MinigameInfo nextMinigameInfo;

    // Saves the current board scene name
    private void SaveBoardScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        BoardStateSaver.lastBoardSceneName = sceneName;

        Debug.Log("MinigameLoader: Saved board scene → " + sceneName);
    }

    // Saves board state before leaving
    private void SaveBoardState()
    {
        BoardStateSaver.SavePlayerPositions();
        BoardStateSaver.SaveBoardState();
    }

    // Generic loader for any minigame
    public void LoadMinigame(string minigameSceneName)
    {
        if (string.IsNullOrEmpty(minigameSceneName))
        {
            Debug.LogError("MinigameLoader: Minigame scene name is EMPTY!");
            return;
        }

        SaveBoardScene();
        SaveBoardState();

        Debug.Log("MinigameLoader: Loading minigame → " + minigameSceneName);

        // Instead of loading the minigame directly,
        // we load the LoadingScene which will load the minigame.
        BoardStateSaver.nextMinigameScene = minigameSceneName;

        SceneManager.LoadScene("LoadingScene");
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
}