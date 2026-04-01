using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingSceneController : MonoBehaviour
{
    [Header("Delay Before Loading")]
    public float preLoadDelay = 2f;   // You can change this in the Inspector

    void Start()
    {
        StartCoroutine(LoadBoardAsync());
    }

    IEnumerator LoadBoardAsync()
    {
        // Optional delay before loading begins
        yield return new WaitForSeconds(preLoadDelay);

        string boardScene = BoardStateSaver.lastBoardSceneName;

        if (string.IsNullOrEmpty(boardScene))
        {
            Debug.LogWarning("BoardStateSaver.lastBoardSceneName was empty! Using fallback.");
            boardScene = "BoardScene"; // Replace with your default board
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(boardScene);
        asyncLoad.allowSceneActivation = false;

        // Wait until Unity finishes loading (90% = ready)
        while (asyncLoad.progress < 0.9f)
            yield return null;

        // Optional small buffer to ensure board initializes
        yield return new WaitForSeconds(0.5f);

        asyncLoad.allowSceneActivation = true;
    }
}