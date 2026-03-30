using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameLoader : MonoBehaviour
{
    public MinigameInfo blastInfo;
    public MinigameInfo secretSequenceInfo;

    public static MinigameInfo nextMinigameInfo;

    // Saves the board scene name before loading a minigame
    private void SaveBoardScene()
    {
        BoardStateSaver.lastBoardSceneName = SceneManager.GetActiveScene().name;
        Debug.Log("Saved board scene: " + BoardStateSaver.lastBoardSceneName);
    }

    // Generic loader for any minigame
    public void LoadMinigame(string minigameSceneName)
    {
        SaveBoardScene();

        if (string.IsNullOrEmpty(minigameSceneName))
        {
            Debug.LogError("MinigameLoader: Minigame scene name is empty!");
            return;
        }

        SceneManager.LoadScene(minigameSceneName);
    }

    // Optional: specific minigame shortcuts
    public void LoadBlast()
    {
        LoadMinigame("MinigameBlast");
    }
    public void LoadBalloon()
    {
        LoadMinigame("MinigameBalloon");
    }

    public void LoadSecretSequence()
    {
        LoadMinigame("MinigameSecretSequence");
    }
    public void TriggerMinigame(string minigameName)
    {
        Object.FindFirstObjectByType<MinigameLoader>().LoadMinigame(minigameName);
    }

}